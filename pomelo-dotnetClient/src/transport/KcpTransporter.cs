using System;
using System.Net.Sockets;
using System.Net;
using System.Net.Sockets.Kcp;
using System.Buffers;
using System.Threading.Tasks;

namespace Pomelo.DotNetClient
{

    public class KcpTransporter : Transporter, IKcpCallback
    {
        IPEndPoint ipEndPoint = null;
        PoolSegManager.Kcp kcp;

        public KcpTransporter(Socket socket, KcpClientParam param, IPEndPoint ipEndPoint, Action<byte[]> processer): base(socket, processer)
        {
            stateObject = new StateObject(65535); // 64KB UDP不会对发送的包进行自动分组，一次发多大，就接收多大，所以要设置的尽量大，否则会报错，无法接收
            this.ipEndPoint = ipEndPoint;

            if (param != null) { 
                kcp = new PoolSegManager.Kcp(param.conv, this);
                kcp.NoDelay(param.nodelay, param.interval, param.resend, param.nc);
                kcp.WndSize(param.sndwnd, param.rcvwnd);
                kcp.SetMtu(param.mtu);
                kcpUpdate();
            }
        }

        void kcpUpdate() {
            Task.Run(async () =>
            {
                try
                {
                    while (true)
                    {
                        kcp.Update(DateTime.UtcNow);

                        int len;
                        while ((len = kcp.PeekSize()) > 0)
                        {
                            var buffer = new byte[len];
                            if (kcp.Recv(buffer) >= 0)
                            {
                                processBytes(buffer, 0, buffer.Length);
                            }
                        }
                        await Task.Delay(5);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            });
        }

        public override void send(byte[] buffer)
        {
            if (this.transportState != TransportState.closed)
            {
                //string str = "";
                //foreach (byte code in buffer)
                //{
                //    str += code.ToString();
                //}
                //Console.WriteLine("send:" + buffer.Length + " " + str.Length + "  " + str);
                kcp.Send(buffer.AsSpan());
            }
        }
        protected override void endReceive(IAsyncResult asyncReceive)
        {
            if (this.transportState == TransportState.closed)
                return;
            StateObject state = (StateObject)asyncReceive.AsyncState;
            Socket socket = this.socket;

            try
            {
                int length = socket.EndReceive(asyncReceive);

                this.onReceiving = false;

                if (length > 0)
                {

                    // var buff = buffer.Memory.Slice(0, avalidLength);
                    kcp.Input(state.buffer.AsSpan());
                    // processBytes(state.buffer, 0, length);
                    //Receive next message
                    if (this.transportState != TransportState.closed) receive();
                }
                else
                {
                    if (this.onDisconnect != null) this.onDisconnect();
                }

            }
            catch (System.Net.Sockets.SocketException e)
            {
                if (this.onDisconnect != null)
                    this.onDisconnect();
            }
        }

        public void Output(IMemoryOwner<byte> buffer, int avalidLength)
        {
            using (buffer)
            {
                var buff = buffer.Memory.Slice(0, avalidLength).ToArray();
                this.asyncSend = socket.BeginSendTo(buff, 0, buff.Length, SocketFlags.None, ipEndPoint, new AsyncCallback(sendCallback), socket);
                this.onSending = true;
            }
        }
    }
}