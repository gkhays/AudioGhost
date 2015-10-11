using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;

namespace AudioGhost
{
    class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        static void Main(string[] args)
        {
            string maskPrefix = "10.21";

            // http://www.csharp-examples.net/socket-send-receive/
            // http://msdn.microsoft.com/en-us/library/system.net.sockets.tcplistener(v=vs.110).aspx
            TcpListener tcpListener = null;

            try
            {
                Int32 port = 8085;
                //IPAddress localAddr = IPAddress.Parse("127.0.0.1");
                //IPAddress localAddr = Dns.Resolve("localhost").AddressList[0]; // Deprecated
                //IPHostEntry hostEntry = Dns.GetHostEntry("localhost");
                string hostName = Dns.GetHostName();
                IPHostEntry hostEntry = Dns.GetHostEntry(hostName);
                IPAddress[] addys = hostEntry.AddressList;
                IPAddress localAddr = addys[addys.Length - 1];
                foreach (IPAddress a in addys)
                {
                    if (a.ToString().StartsWith(maskPrefix) || a.ToString().StartsWith("192.168"))
                    {
                        localAddr = a;
                    }
                }
                Console.WriteLine("Binding to {0} on {1}", localAddr, hostName);
                tcpListener = new TcpListener(localAddr, port);
                tcpListener.Start();

                // Hide ourself.
                // http://stackoverflow.com/questions/3571627/show-hide-the-console-window-of-a-c-sharp-console-application
                // http://stackoverflow.com/questions/3563744/how-can-i-hide-a-console-window
                var handle = GetConsoleWindow();
                ShowWindow(handle, SW_HIDE);

                byte[] buffer = new byte[512];
                string data = null;
                //GhostServer.Receive(socket, buffer, 0, buffer.Length, 10000);
                //string str = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                while (true)
                {
                    data = null;
                    Console.WriteLine("Waiting for a connection...");
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    int bytesRead;
                    NetworkStream stream = tcpClient.GetStream();
                    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Console.WriteLine("Received: {0}", data);

                        // http://code.msdn.microsoft.com/windowsdesktop/Text-to-Speech-Converter-0ed77dd5
                        SpeechSynthesizer reader = new SpeechSynthesizer();
                        reader.Speak(data);
                    }

                    tcpClient.Close();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("Ghost Server Error: {0}", e);
            }
            finally
            {
                tcpListener.Stop();
            }
        }
    }
}
