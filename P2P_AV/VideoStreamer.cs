using ScreenCapturerNS;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace P2P_AV
{
    class VideoStreamer
    {
        static int bufSize = 65536;

        public static int width = 1366;
        public static int height = 768;
        public static long encodeQuality = 50L;
        public static string codecName = "JPEG";
        public static int H264Bitrate = 500;
        static int codec;
        static Socket client;

        static OpenH264Lib.Encoder encoder;
        static OpenH264Lib.Decoder decoder;

        static string OpenH264DllName = "openh264.dll";

        public async static Task MainAsync(int role, string addr, int port)
        {
            if (codecName == "JPEG")
            {
                codec = 0;
            }
            if (codecName == "H264")
            {
                codec = 1;
            }

            if (role == 0)
            {
                TcpListener server = new TcpListener(IPAddress.Any, port);
                server.Start(1);
                client = server.AcceptSocket();
                client.ReceiveBufferSize = bufSize;
                client.SendBufferSize = bufSize;
                if (codec == 1)
                {
                    decoder = new OpenH264Lib.Decoder(OpenH264DllName);
                }
                Thread rcv = new Thread(new ThreadStart(receiverThreadProcessor));
                rcv.IsBackground = false;
                rcv.Priority = ThreadPriority.Highest;
                rcv.Start();
            }
            else
            {
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect(addr, port);
                client.ReceiveBufferSize = bufSize;
                client.SendBufferSize = bufSize;
                if (codec == 1)
                {
                    encoder = new OpenH264Lib.Encoder(OpenH264DllName);
                    encoder.Setup(width, height, H264Bitrate * 1000, 15, 1f, H264EncoderCallback);
                }
                ScreenCapturer.OnScreenUpdated += CapturedEvent;
                ScreenCapturer.StartCapture();
            }
            await Task.Delay(-1);
        }

        static void CapturedEvent(object sender, OnScreenUpdatedEventArgs args)
        {
            using (Bitmap decoded = new Bitmap(args.Bitmap, new Size(width, height)))
            {
                MainWindow.current.Dispatcher.Invoke(() =>
                {
                    var rect = new System.Drawing.Rectangle(0, 0, width, height);

                    var bitmapData = decoded.LockBits(
                    rect,
                    ImageLockMode.ReadWrite,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    var size = (rect.Width * rect.Height) * 4;
                    MainWindow.current.VideoObject.Source = BitmapSource.Create(
                    decoded.Width,
                    decoded.Height,
                    decoded.HorizontalResolution,
                    decoded.VerticalResolution,
                    PixelFormats.Bgra32,
                    null,
                    bitmapData.Scan0,
                    size,
                    bitmapData.Stride);
                    decoded.UnlockBits(bitmapData);
                });

                encodeAndSend(decoded);
            }
        }

        static void receiverThreadProcessor()
        {
            while (true)
            {
                try
                {
                    byte[] buffer = new byte[bufSize];
                    byte[] buf = new byte[4];
                    while (client != null && client.Connected)
                    {
                        client.Send(new byte[1]);
                        while (client.Available == 0) continue;

                        using (MemoryStream recv = new MemoryStream())
                        {
                            int totalLen = 0;
                            using (MemoryStream lenStream = new MemoryStream())
                            {
                                int recvv = 0;
                                while (recvv < 4)
                                {
                                    int r = client.Receive(buf, 4, SocketFlags.None);
                                    recvv += r;
                                    lenStream.Write(buf, 0, r);
                                }

                                totalLen = BitConverter.ToInt32(lenStream.ToArray(), 0);
                            }

                            int recvLen = 0;
                            while (recvLen < totalLen)
                            {
                                int rl = client.Receive(buffer, bufSize, SocketFlags.None);
                                recv.Write(buffer, 0, rl);
                                recvLen += rl;
                            }

                            decodeAndSet(recv.ToArray(), 0);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        static private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        static byte[] nullBuf = new byte[1];
        static void encodeAndSend(Bitmap image)
        {
            if (client != null && client.Connected)
            {
                if (codec == 0)
                {
                    if (client.Available == 0) return;
                    client.Receive(nullBuf);

                    using (MemoryStream encoder = new MemoryStream())
                    {
                        ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                        EncoderParameters myEncoderParameters = new EncoderParameters(1);
                        myEncoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, encodeQuality);
                        image.Save(encoder, jpgEncoder, myEncoderParameters);

                        client.Send(BitConverter.GetBytes((int)encoder.Length));
                        client.Send(encoder.ToArray());
                    }
                }
                else if (codec == 1)
                {
                    encoder.Encode(image);
                }
            }
        }

        static void H264EncoderCallback(byte[] data, int len, OpenH264Lib.Encoder.FrameType type)
        {
            if (type == OpenH264Lib.Encoder.FrameType.Skip) return;
            if (client != null && client.Connected)
            {
                client.Send(BitConverter.GetBytes((int)data.Length));
                client.Send(data);
            }
        }

        static void decodeAndSet(byte[] buffer, int h264len)
        {
            if (buffer == null || buffer.Length == 0) return;
            if (codec == 0)
            {
                using (MemoryStream str = new MemoryStream(buffer))
                {
                    using (Bitmap decoded = new Bitmap(str))
                    {
                        if (decoded == null) return;
                        MainWindow.current.Dispatcher.Invoke(() =>
                        {
                            var rect = new System.Drawing.Rectangle(0, 0, decoded.Width, decoded.Height);

                            var bitmapData = decoded.LockBits(
                                rect,
                                ImageLockMode.ReadWrite,
                                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                            var size = (rect.Width * rect.Height) * 4;
                            MainWindow.current.VideoObject.Source = BitmapSource.Create(
                                decoded.Width,
                                decoded.Height,
                                decoded.HorizontalResolution,
                                decoded.VerticalResolution,
                                PixelFormats.Bgra32,
                                null,
                                bitmapData.Scan0,
                                size,
                                bitmapData.Stride);
                            decoded.UnlockBits(bitmapData);
                        });
                    }
                }
            }
            else if (codec == 1)
            {
                using (Bitmap decoded = decoder.Decode(buffer, h264len))
                {
                    if (decoded == null) return;
                    MainWindow.current.Dispatcher.Invoke(() =>
                    {
                        var rect = new System.Drawing.Rectangle(0, 0, decoded.Width, decoded.Height);

                        var bitmapData = decoded.LockBits(
                            rect,
                            ImageLockMode.ReadWrite,
                            System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                        var size = (rect.Width * rect.Height) * 4;
                        MainWindow.current.VideoObject.Source = BitmapSource.Create(
                            decoded.Width,
                            decoded.Height,
                            decoded.HorizontalResolution,
                            decoded.VerticalResolution,
                            PixelFormats.Bgra32,
                            null,
                            bitmapData.Scan0,
                            size,
                            bitmapData.Stride);
                        decoded.UnlockBits(bitmapData);
                    });
                }
            }
        }
    }
}
