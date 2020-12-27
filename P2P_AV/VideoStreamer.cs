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
        static NetworkStream stream;
        static int bufSize = 32768;

        public static int width = 1366;
        public static int height = 768;
        public static long encodeQuality = 50L;
        public static string codecName = "H264";
        public static int H264Bitrate = 500;
        static int codec;

        static OpenH264Lib.Encoder encoder;
        static OpenH264Lib.Decoder decoder;

        static string OpenH264DllName = "openh264.dll";

        public async static Task MainAsync(int role, string addr, int port)
        {
            TcpClient client;

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
                client = server.AcceptTcpClient();
                client.ReceiveBufferSize = bufSize;
                client.SendBufferSize = bufSize;
                stream = client.GetStream();
                if (codec == 1)
                {
                    decoder = new OpenH264Lib.Decoder(OpenH264DllName);
                }
                new Thread(new ThreadStart(receiverThreadProcessor)).Start();
            }
            else
            {
                ScreenCapturer.OnScreenUpdated += CapturedEvent;
                ScreenCapturer.StartCapture();
                client = new TcpClient();
                client.Connect(addr, port);
                client.ReceiveBufferSize = bufSize;
                client.SendBufferSize = bufSize;
                if (codec == 1)
                {
                    encoder = new OpenH264Lib.Encoder(OpenH264DllName);
                    encoder.Setup(width, height, H264Bitrate * 1000, 15, 2f, H264EncoderCallback);
                }
                stream = client.GetStream();
            }
            await Task.Delay(-1);
        }

        static void CapturedEvent(object sender, OnScreenUpdatedEventArgs args)
        {
            using (Bitmap decoded = new Bitmap(args.Bitmap, new Size(width, height)))
            {
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

                encodeAndSend(decoded);
            }
        }

        static void receiverThreadProcessor()
        {
            while (true)
            {
                try
                {
                    if(codec == 0) {
                        while (stream != null)
                        {
                            stream.Write(new byte[4], 0, 4);
                            while (!stream.DataAvailable) continue;

                            {
                                using (MemoryStream recv = new MemoryStream())
                                {
                                    int totalLen = 0;
                                    using (MemoryStream lenStream = new MemoryStream())
                                    {
                                        int recvv = 0;
                                        byte[] buf = new byte[4];
                                        while (recvv < 4)
                                        {
                                            int r = stream.Read(buf, 0, 4);
                                            recvv += r;
                                            lenStream.Write(buf, 0, r);
                                        }

                                        totalLen = BitConverter.ToInt32(lenStream.ToArray(), 0);
                                    }

                                    int recvLen = 0;
                                    byte[] buffer = new byte[bufSize];
                                    while (recvLen < totalLen)
                                    {
                                        int rl = stream.Read(buffer, 0, buffer.Length);
                                        recv.Write(buffer, 0, rl);
                                        recvLen += rl;
                                    }

                                    decodeAndSet(recv.ToArray());
                                }
                            }
                        }
                    }
                    else if(codec == 1)
                    {
                        while (stream != null)
                        {
                            using (MemoryStream recv = new MemoryStream())
                            {
                                byte[] buf = new byte[8192];
                                while (stream.DataAvailable)
                                {
                                    int len = stream.Read(buf, 0, buf.Length);
                                    recv.Write(buf, 0, len);
                                }
                                decodeAndSet(recv.ToArray());
                            }
                        }
                    }
                }
                catch(Exception e)
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

        static void encodeAndSend(Bitmap image)
        {
            if (stream != null)
            {
                if (codec == 0)
                {
                    if (!stream.DataAvailable) return;
                    stream.Read(new byte[4], 0, 4);

                    using (MemoryStream encoder = new MemoryStream())
                    {
                        ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                        Encoder myEncoder = Encoder.Quality;
                        EncoderParameters myEncoderParameters = new EncoderParameters(1);
                        myEncoderParameters.Param[0] = new EncoderParameter(myEncoder, encodeQuality);
                        image.Save(encoder, jpgEncoder, myEncoderParameters);

                        stream.Write(BitConverter.GetBytes((int)encoder.Length), 0, 4);
                        encoder.Seek(0, SeekOrigin.Begin);
                        encoder.CopyTo(stream);
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
            if (stream != null)
            {
                stream.Write(data, 0, len);
            }
        }

        static void decodeAndSet(byte[] buffer)
        {
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
                using (Bitmap decoded = decoder.Decode(buffer, buffer.Length))
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
