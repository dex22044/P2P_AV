using NAudio.Wave;
using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace P2P_AV
{
    class AudioStreamer
    {
        static TcpListener server;
        static TcpClient client;
        static NetworkStream stream;
        static IPEndPoint endPoint;
        static int maxBufferSize = 19200 * 4;

        public static WaveOut WaveOut;

        public static async Task MainAsync(int role, int bufSize, string addr, int port)
        {
            WaveOut sp = new WaveOut();
            WasapiLoopbackCapture input = new WasapiLoopbackCapture();
            input.DataAvailable += Input_DataAvailable;
            BufferedWaveProvider streamOut = new BufferedWaveProvider(input.WaveFormat);
            streamOut.BufferLength = maxBufferSize;
            streamOut.DiscardOnBufferOverflow = true;
            sp.Init(streamOut);
            sp.Play();

            WaveOut = sp;

            if (role == 0)
            {
                server = new TcpListener(IPAddress.Any, port);
                server.Start(1);
                client = server.AcceptTcpClient();
                client.SendBufferSize = bufSize;
                client.ReceiveBufferSize = bufSize;
                stream = client.GetStream();

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
                endPoint = new IPEndPoint(IPAddress.Parse(addr.Split(':')[0]), Convert.ToInt32(addr.Split(':')[1]));
                client = new TcpClient();
                client.Connect(endPoint);
                client.SendBufferSize = bufSize;
                client.ReceiveBufferSize = bufSize;
                stream = client.GetStream();
                input.StartRecording();
                await Task.Delay(-1);
            }
        }

        private static void Input_DataAvailable(object sender, WaveInEventArgs e)
        {
            stream.Write(e.Buffer, 0, e.BytesRecorded);
        }
    }
}
