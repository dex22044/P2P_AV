using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H264
{
    class H264
    {
		public static Bitmap RGBtoBitmap(byte[] rgb, int width, int height)
		{
            unsafe
			{
				int pixelSize = 3;
				Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
				BitmapData bmpDate = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, bmp.PixelFormat);
				byte* ptr = (byte*)bmpDate.Scan0.ToPointer();

				int cnt = 0;
				for (int y = 0; y <= height - 1; y++)
				{
					for (int x = 0; x <= width - 1; x++)
					{
						//ѓsѓNѓZѓ‹ѓfЃ[ѓ^‚Е‚МѓsѓNѓZѓ‹(x,y)‚МЉJЋn€К’u‚рЊvЋZ‚·‚й
						int pos = y * bmpDate.Stride + x * pixelSize;

						ptr[pos + 0] = rgb[cnt + 0]; // r
						ptr[pos + 1] = rgb[cnt + 1]; // g
						ptr[pos + 2] = rgb[cnt + 2]; // b
						cnt += 3;
					}
				}

				bmp.UnlockBits(bmpDate);

				return bmp;
			}
		}

		public static byte[] BitmapToRGBA(Bitmap bmp, int width, int height)
		{
            unsafe
			{
				try
				{
					int pixelSize = 3;
                    switch (bmp.PixelFormat)
                    {
						case PixelFormat.Format24bppRgb:
							pixelSize = 3;
							break;
						case PixelFormat.Format32bppArgb:
						case PixelFormat.Format32bppRgb:
						case PixelFormat.Format32bppPArgb:
							pixelSize = 4;
							break;
						default:
							return null;
                    }

					BitmapData bmpDate = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
					byte* ptr = (byte*)bmpDate.Scan0.ToPointer();

					byte[] buffer = new byte[width * height * 3];

					int cnt = 0;
					for (int y = 0; y <= height - 1; y++)
					{
						for (int x = 0; x <= width - 1; x++)
						{
							int pos = y * bmpDate.Stride + x * pixelSize;

							buffer[cnt + 0] = ptr[pos + 0]; // r
							buffer[cnt + 1] = ptr[pos + 1]; // g
							buffer[cnt + 2] = ptr[pos + 2]; // b
							cnt += 3;
						}
					}

					bmp.UnlockBits(bmpDate);

					return buffer;
                }
                catch (Exception) { return null; }
			}
		}
	}

	class EncodedFrame
    {
		public byte[] data;
		public EncodedFrameType type;
	}

	public enum EncodedFrameType
    {
		I, P, Skip
    }
}
