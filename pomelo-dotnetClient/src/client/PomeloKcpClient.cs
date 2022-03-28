using SimpleJson;
using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Pomelo.DotNetClient
{

    public class KcpClientParam {
        public string host;
        public int port;

        public uint conv; // conv为一个KCP会话编号 为整数

        // 普通模式： ikcp_nodelay(kcp, 0, 40, 0, 0);
        // 极速模式： ikcp_nodelay(kcp, 1, 10, 2, 1);
        public int nodelay = 0; // 是否启用 nodelay模式，0不启用；1启用。
        public int interval = 40; // 协议内部工作的 interval，单位毫秒，比如 10ms或者 20ms
        public int resend = 0; // 快速重传模式，默认0关闭，可以设置2（2次ACK跨越将会直接重传）
        public int nc = 0; // 是否关闭流控，默认是0代表不关闭，1代表关闭。

        public int sndwnd = 32; // 最大发送窗口
        public int rcvwnd = 32; // 最大接收窗口大小

        public int mtu = 1400; // 最大传输单元，该值将会影响数据包归并及分片时候的最大传输单元。

        public KcpClientParam(
            string host, int port, uint conv, int nodelay = 0, int interval = 40,
            int resend = 0, int nc = 0, int sndwnd = 32, int rcvwnd = 32, int mtu = 1400
        ) {
            this.host = host;
            this.port = port;
            this.conv = conv;
            this.nodelay = nodelay;
            this.interval = interval;
            this.resend = resend;
            this.nc = nc;
            this.sndwnd = sndwnd;
            this.rcvwnd = rcvwnd;
            this.mtu = mtu;
        }
    }

    public class PomeloKcpClient : PomeloClient
    {
        
        public IPEndPoint sendIpEndPoint;

        public PomeloKcpClient()
        {
        }

        /// <summary>
        /// initialize pomelo client
        /// </summary>
        /// <param name="host">server name or server ip (www.xxx.com/127.0.0.1/::1/localhost etc.)</param>
        /// <param name="port">server port</param>
        /// <param name="callback">socket successfully connected callback(in network thread)</param>
        public void initClient(KcpClientParam param, Action callback = null)
        {
            SetHostAndPort(param.host, param.port);

            timeoutEvent.Reset();
            eventManager = new EventManager();
            NetWorkChanged(NetWorkState.CONNECTING);

            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this.socket.Bind(new IPEndPoint(IPAddress.Any, 0));
            // this.socket.ReceiveBufferSize = 65536; // 64KB UDP不会对发送的包进行自动分组，一次发多大，就接收多大，所以要设置的尽量大，否则会报错，无法接收
            this.protocol = new Protocol(this, param, this.socket);
            if (callback != null)
            {
                callback();
            }
        }

        public void SetHostAndPort(string host, int port)
        {
            if (sendIpEndPoint == null) {
                sendIpEndPoint = new IPEndPoint(IPAddress.Parse(host), port);
            } else
            {
                sendIpEndPoint.Address = IPAddress.Parse(host);
                sendIpEndPoint.Port = port;
            }
        }
    }
}