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
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace P2P_AV
{
    class VideoStreamer
    {
        static Socket udpSock;
        static IPEndPoint remote;

        public static int width = 1366;
        public static int height = 768;
        public static long encodeQuality = 10L;
        static int role;

        static Bitmap currImage;
        static Graphics currImageDraw;
        static WriteableBitmap writeableBitmap;

        static Rectangle[] divideRects;
        static EncoderParameters encoderParameters;
        static ImageCodecInfo imageCodecInfo;

        static int divideWidth = 8;
        static int divideHeight = 9;

        static List<byte[]> sendBuffer;

        public async static Task MainAsync(int role, string addr, int port)
        {
            encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, encodeQuality);
            imageCodecInfo = GetEncoder(ImageFormat.Jpeg);

            VideoStreamer.role = role;
            udpSock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            divideRects = new Rectangle[divideWidth * divideHeight];
            int blockWidth = width / divideWidth;
            int blockHeight = height / divideHeight;
            for (int x = 0; x < divideWidth; x++)
            {
                for (int y = 0; y < divideHeight; y++)
                {
                    int pos = y * divideWidth + x;
                    divideRects[pos] = new Rectangle(x * blockWidth, y * blockHeight, blockWidth, blockHeight);
                }
            }

            currImage = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            currImageDraw = Graphics.FromImage(currImage);

            MainWindow.current.Dispatcher.Invoke(() =>
            {
                writeableBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
                MainWindow.current.VideoObject.Source = writeableBitmap;
            });

            if (role == 0)
            {
                udpSock.Bind(new IPEndPoint(IPAddress.Any, port));
                Thread rcv = new Thread(new ThreadStart(receiverThreadProcessor));
                rcv.IsBackground = false;
                rcv.Priority = ThreadPriority.Highest;
                rcv.Start();
            }
            else
            {
                udpSock.Bind(new IPEndPoint(IPAddress.Any, 0));
                remote = new IPEndPoint(IPAddress.Parse(addr), port);
                ScreenCapturer.OnScreenUpdated += CapturedEvent;
                ScreenCapturer.StartCapture();
            }
            await Task.Delay(-1);
        }

        static void senderThreadProcessor()
        {
            while (true)
            {

            }
        }

        static unsafe void CapturedEvent(object sender, OnScreenUpdatedEventArgs args)
        {
            using (Bitmap decoded = new Bitmap(args.Bitmap, new Size(width, height)))
            {
                MainWindow.current.Dispatcher.Invoke(() =>
                {
                    var rect = new System.Drawing.Rectangle(0, 0, width, height);

                    var bitmapData = decoded.LockBits(
                    rect,
                    ImageLockMode.ReadWrite,
                    System.Drawing.Imaging.PixelFormat.Format32bppRgb);

                    writeableBitmap.WritePixels(new System.Windows.Int32Rect(0, 0, width, height), bitmapData.Scan0, width * height * 4, width * 4);

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
                    byte[] buffer = new byte[65500];
                    int len = udpSock.Receive(buffer);

                    if (len > 0)
                    {
                        Console.WriteLine("recv " + len);
                        Bitmap decoded = new Bitmap(new MemoryStream(buffer, 1, len - 1));
                        currImageDraw.DrawImage(decoded, divideRects[buffer[0] - 1]);
                        resetImg();
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
            for (int i = 0; i < divideWidth * divideHeight; i++) {
                Bitmap part = image.Clone(divideRects[i], image.PixelFormat);
                MemoryStream ms = new MemoryStream();
                ms.Write(new byte[] { (byte)i }, 0, 1);
                part.Save(ms, imageCodecInfo, encoderParameters);
                byte[] enc = ms.ToArray();
                udpSock.SendTo(enc, remote);
            }
        }

        static void resetImg()
        {
            MainWindow.current.Dispatcher.Invoke(() =>
            {
                var rect = new System.Drawing.Rectangle(0, 0, width, height);

                var bitmapData = currImage.LockBits(
                rect,
                ImageLockMode.ReadWrite,
                System.Drawing.Imaging.PixelFormat.Format32bppRgb);

                writeableBitmap.WritePixels(new System.Windows.Int32Rect(0, 0, width, height), bitmapData.Scan0, width * height * 4, width * 4);

                currImage.UnlockBits(bitmapData);
            });
        }
    }
}
