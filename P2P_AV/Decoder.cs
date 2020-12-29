using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H264
{
    class Decoder
    {
        int _width, _height;
        Bitmap _prevFrame;

        public Decoder(int width, int height)
        {
            _width = width;
            _height = height;
            _prevFrame = new Bitmap(width, height);
        }

        private Bitmap Add(Bitmap prev, Bitmap curr)
        {
            try
            {
                byte[] p = H264.BitmapToRGBA(prev, _width, _height);
                byte[] c = H264.BitmapToRGBA(curr, _width, _height);
                byte[] o = new byte[_width * _height * 3];

                for (int i = 0; i < o.Length; i++)
                {
                    o[i] = (byte)(p[i] + c[i] - 127);
                }

                return H264.RGBtoBitmap(o, _width, _height);
            }
            catch (Exception) { return null; }
        }

        public Bitmap Decode(Bitmap data, EncodedFrameType type)
        {
            Bitmap decoded = (Bitmap)data.Clone();
            {
                if (type == EncodedFrameType.I)
                {
                    if (_prevFrame != null)
                        _prevFrame.Dispose();
                    _prevFrame = (Bitmap)decoded.Clone();
                    return decoded;
                }
                else if (type == EncodedFrameType.P)
                {
                    Bitmap bmp = Add(_prevFrame, decoded);
                    {
                        if (_prevFrame != null)
                            _prevFrame.Dispose();
                        _prevFrame = bmp;
                        return bmp;
                    }
                }
            }
            return _prevFrame;
        }
    }
}
