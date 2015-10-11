using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using NAudio.Wave;
using NDesk.Options;
using System.Speech.Synthesis;

namespace AudioGhost
{
    class Program
    {
        static int verbosity;

        static void Debug(string format, params object[] args)
        {
            if (verbosity > 0)
            {
                Console.Write("# ");
                Console.WriteLine(format, args);
            }
        }

        static void ParseCommandLine(string[] args)
        {
            bool showHelp = false;
            string maskPrefix = null;
            var p = new OptionSet() {
                { "h|?|help", "show this message and exit", v => showHelp = v != null },
                { "m|mask=", "the first two octets of the subnet mask", v => maskPrefix = v },
            };
            List<string> extra = p.Parse(args);

            if (showHelp)
            {
                ShowHelp(p);
                return;
            }
            
            string message = null;
            if (extra.Count > 0)
            {
                message = string.Join(" ", extra.ToArray());
                Debug("Using new message: {0}", message);
            }
            else
            {
                message = "You will never guess from where this is coming.";
                Debug("Using default message: {0}", message);
            }
        }

        static void PlayUsingNAudio(String soundFile)
        {
            // The hard way.
            // http://naudio.codeplex.com/documentation
            // http://naudio.codeplex.com/wikipage?title=WAV
            using (var wfr = new WaveFileReader(soundFile))
            using (WaveChannel32 wc = new WaveChannel32(wfr) { PadWithZeroes = false })
            using (var audioOutput = new DirectSoundOut())
            {
                audioOutput.Init(wc);
                audioOutput.Play();

                while (audioOutput.PlaybackState != PlaybackState.Stopped)
                {
                    Thread.Sleep(20);
                }

                audioOutput.Stop();
            }
        }

        static void PlayUsingSoundPlayer(String soundFile)
        {
            // The easy way.
            // http://stackoverflow.com/questions/3502311/how-to-play-a-sound-in-c-net
            // http://stackoverflow.com/questions/5756855/c-sharp-play-sound-with-one-line-of-c-sharp-code
            System.Media.SoundPlayer player = new System.Media.SoundPlayer(soundFile);
            player.Play();
        }

        static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: ag [OPTIONS]+ message");
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }

        static void Send(string hostName, int port, string message)
        {
            // http://www.csharp-examples.net/socket-send-receive/
            //TcpClient tcpClient = new TcpClient();
            //Socket socket = tcpClient.Client;
            //string hello = "Hello world!";

            //try
            //{
            //    GhostClient.Send(socket, Encoding.UTF8.GetBytes(hello), 0, hello.Length, 10000);
            //}
            //catch (Exception)
            //{
            //    // Do nothing.
            //}
            
            TcpClient tcpClient = new TcpClient(hostName, port);
            Byte[] data = Encoding.UTF8.GetBytes(message);
            NetworkStream stream = tcpClient.GetStream();
            stream.Write(data, 0, data.Length);
            Debug("Sent: {0}", message);
            stream.Close();
            tcpClient.Close();
        }

        static void Main(string[] args)
        {
            int port = 8085;
            bool showHelp = false;
            bool useLocal = false;
            string server = "localhost";
            string maskPrefix = "10.21";
            var p = new OptionSet() {
                { "h|?|help", "show this message and exit", v => showHelp = v != null },
                { "l|local", "send to localhost", v => useLocal = v != null },
                { "m|mask=", "the first two octets of the subnet mask", v => maskPrefix = v },
                { "p|port=", "the TCP port to which to send", (int v) => port = v },
                { "s|server=", "the host to which to send", v => server = v },
                { "v|verbose", "", v => { if (v != null) ++verbosity; } },
            };
            List<string> extra = p.Parse(args);

            if (showHelp)
            {
                ShowHelp(p);
                return;
            }

            string message = null;
            if (extra.Count > 0)
            {
                message = string.Join(" ", extra.ToArray());
                Debug("Using new message: {0}", message);
            }
            else
            {
                message = "You will never guess from where this is coming.";
                Debug("Using default message: {0}", message);
            }

            if (verbosity > 0)
            {
                int index = 0;
                string hostName = Dns.GetHostName();
                IPHostEntry hostEntry = Dns.GetHostEntry(hostName);
                IPAddress[] addys = hostEntry.AddressList;
                IPAddress bindTo = null;
                Console.WriteLine("\n  NICS");
                foreach (IPAddress a in addys)
                {
                    if (a.ToString().StartsWith(maskPrefix))
                    {
                        bindTo = a;
                    }
                    Console.WriteLine("    [{0}] {1}", index, a);
                    index++;
                }
                Console.WriteLine("\nWe should bind to {0}", bindTo);
            }

            if (useLocal)
            {
                // http://code.msdn.microsoft.com/windowsdesktop/Text-to-Speech-Converter-0ed77dd5
                SpeechSynthesizer reader = new SpeechSynthesizer();
                reader.Speak(message);
            }
            else
            {
                Send(server, port, message);
            }

            var soundFile = "tweet.wav";
            PlayUsingNAudio(soundFile);

            // Pause...
            Console.ReadLine();
        }
    }
}
