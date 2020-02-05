using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TestServer
{
    public class TestServer
    {
        public static ManualResetEvent clientConnected = new ManualResetEvent(false);

        static void Run(string[] args)
        {
            try
            {
                var listener = new TcpListener(IPAddress.Any, 2188);
                listener.Start();
                while (true)
                {
                    clientConnected.Reset();
                    listener.BeginAcceptTcpClient(DoAcceptTcpClientCallback, listener);
                    clientConnected.WaitOne();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }
        
        public static void DoAcceptTcpClientCallback(IAsyncResult ar)
        {
            try
            {
                var listener = (TcpListener) ar.AsyncState;

                var client = listener.EndAcceptTcpClient(ar);

                var reader = new StreamReader(client.GetStream());
                var input = reader.ReadLine();
                var output = string.Empty;
                var data = input.Split();
               
                switch (data[0])
                {
                    case "sendKey":
                        File.WriteAllText("./" + data[1] + ".key", data[2]);
                        output = "Key saved";
                        break;
                    case "getKey":
                        if (File.Exists("./" + data[1] + ".key"))
                        {
                            output = File.ReadAllText("./" + data[1] + ".key");
                        }
                        else
                        {
                            output = "Key not found";
                        }

                        break;
                    case "sendMsg":
                        File.WriteAllText("./" + data[1] + ".msg", data[2]);
                        output = "Message written";
                        break;
                    case "getMsg":
                        output = output = File.ReadAllText("./" + data[1] + ".msg");
                          
                        break;
                    default:
                        output = "Invalid option, check your message, you sent: " + input;
                        break;
                }

                var writer = new StreamWriter(client.GetStream());
                writer.WriteLine(output);
                writer.Flush();
                client.Close();

                clientConnected.Set();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, ex.StackTrace);
            }
        }
    }
}