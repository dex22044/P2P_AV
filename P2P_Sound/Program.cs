using NAudio.Wave;
using P2P_Sound;
using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace P2P_sound
{
    class Program
    {
        static TcpListener server;
        static TcpClient client;
        static NetworkStream stream;
        static IPEndPoint endPoint;
        static int bufSize;
        static int maxBufferSize = 19200 * 64;

        static ulong packetSendCount;
        static ulong bytesSendCount;
        static ulong bytesSendCountSpeed;
        static DateTime startTime;

        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            WaveOut sp = new WaveOut();
            WasapiLoopbackCapture input = new WasapiLoopbackCapture();
            input.DataAvailable += Input_DataAvailable;
            BufferedWaveProvider streamOut = new BufferedWaveProvider(input.WaveFormat);
            streamOut.BufferLength = maxBufferSize;
            streamOut.DiscardOnBufferOverflow = true;
            sp.Init(streamOut);
            sp.Play();

            Console.WriteLine("Какова роль приложения? (0=приёмник, 1=передатчик):");
            int role = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Размер окна:");
            bufSize = Convert.ToInt32(Console.ReadLine());
            if (role == 0)
            {
                Console.WriteLine("Порт:");
                int port = Convert.ToInt32(Console.ReadLine());
                server = new TcpListener(IPAddress.Any, port);
                server.Start(1);
                client = server.AcceptTcpClient();
                client.SendBufferSize = bufSize;
                client.ReceiveBufferSize = bufSize;
                stream = client.GetStream();

                ulong packetCount = 0;
                DateTime lastPacketTime = DateTime.Now;

                while (true)
                {
                    try
                    {
                        while (stream.DataAvailable)
                        {
                            byte[] buf = new byte[8192];
                            int len = stream.Read(buf, 0, buf.Length);
                            streamOut.AddSamples(buf, 0, len);
                        }
                    }
                    catch (Exception) { }
                }
            }
            else
            {
                Console.WriteLine("Адрес клиента (IP:Port):");
                string addr = Console.ReadLine();
                endPoint = new IPEndPoint(IPAddress.Parse(addr.Split(':')[0]), Convert.ToInt32(addr.Split(':')[1]));
                client = new TcpClient();
                client.Connect(endPoint);
                client.SendBufferSize = bufSize;
                client.ReceiveBufferSize = bufSize;
                stream = client.GetStream();
                input.StartRecording();
                startTime = DateTime.Now;
                Thread.Sleep(-1);
            }
        }

        private static void Input_DataAvailable(object sender, WaveInEventArgs e)
        {
            stream.Write(e.Buffer, 0, e.BytesRecorded);
        }
    }
}
