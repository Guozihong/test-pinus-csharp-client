using System;
using System.Net.Sockets;

namespace Pomelo.DotNetClient
{
    public class TcpTransporter : Transporter
    {
        public TcpTransporter(Socket socket, Action<byte[]> processer): base(socket, processer)
        {
            stateObject  = new StateObject(1024);
        }
    }
}