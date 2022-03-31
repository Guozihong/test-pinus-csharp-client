using System;
using System.Threading.Tasks;
using SimpleJson;

namespace Pomelo.DotNetClient.Test
{
    public class ClientTest
    {
        public static PomeloClient pc = null;
        private static int n = 0;

        public static void kcpTest(string host, int port)
        {
            var kcp = new PomeloKcpClient();
            pc = kcp;

            pc.NetWorkStateChangedEvent += (state) =>
            {
                Console.WriteLine(state);
            };

            var param = new KcpClientParam(
                host, port, conv: 123, nodelay: 1, interval: 20,
                resend: 2, nc: 1, sndwnd: 64, rcvwnd: 64, mtu: 1400
            );
            kcp.on("onChat", (data) =>
            {
                Console.WriteLine("onChat：" + data.ToString());
            });
            kcp.initClient(param, data =>
            {
                // Console.WriteLine("on data back" + data.ToString());
                Task.Run(async delegate
                {
                    while (true) { 
                        n ++;
                        //if (n == 3)
                        //{
                        //    param.host = "127.0.0.1";
                        //    param.port = 3010;
                        //    kcp.disconnect();
                        //    kcp.initClient(param, data2 => {
                        //        Console.WriteLine("连接切换成功！");
                        //    });
                        //}
                        Entry();
                        await Task.Delay(3000);
                    }
                });
            });
        }

        public static void tcpTest(string host, int port)
        {
            var tcp = new PomeloTcpClient();
            pc = tcp;

            pc.NetWorkStateChangedEvent += (state) =>
            {
                Console.WriteLine(state);
            };

            tcp.initClient(host, port, data =>
            {
                // Console.WriteLine("on data back" + data.ToString());
                Task.Run(async delegate
                {
                    while (true)
                    {
                        Entry();
                        await Task.Delay(3000);
                    }
                });
            });
        }

        private static void Entry()
        {
            JsonObject msg = new JsonObject();
            msg["uid"] = 111;
            pc.request("connector.entryHandler.entry", msg, OnEnter);
        }

        //public static void OnQuery(JsonObject result)
        //{
        //    if (Convert.ToInt32(result["code"]) == 200)
        //    {
        //        pc.disconnect();

        //        string host = (string)result["host"];
        //        int port = Convert.ToInt32(result["port"]);
        //        pc = new PomeloKcpClient();

        //        pc.NetWorkStateChangedEvent += (state) =>
        //        {
        //            Console.WriteLine(state);
        //        };

        //        pc.initClient(host, port, () =>
        //        {
        //            pc.connect(null, (data) =>
        //            {
        //                JsonObject userMessage = new JsonObject();
        //                Console.WriteLine("on connect to connector!");

        //                //Login
        //                JsonObject msg = new JsonObject();
        //                msg["username"] = "test";
        //                msg["rid"] = "pomelo";

        //                pc.request("connector.entryHandler.enter", msg, OnEnter);
        //            });
        //        });
        //    }
        //}

        public static void OnEnter(JsonObject result)
        {
            Console.WriteLine("on login " + result.ToString());
        }

        public static void onDisconnect(JsonObject result)
        {
            Console.WriteLine("on sockect disconnected!");
        }

        public static void Run()
        {
            string host = "118.25.97.56";
            int port = 3010;

            kcpTest(host, port);
            // tcpTest(host, port);
        }
    }
}