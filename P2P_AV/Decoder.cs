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
                byte[] p = H264.BitmapToRGBA(prev, prev.Width, prev.Height);
                byte[] c = H264.BitmapToRGBA(curr, curr.Width, curr.Height);
                byte[] o = new byte[c.Length];

                for (int i = 0; i < o.Length; i++)
                {
                    o[i] = p[i]; //(byte)(p[i] + ((c[i] - 127) * 2));
                }

                return H264.RGBtoBitmap(o, prev.Width, prev.Height);
            }
            catch (Exception) { return null; }
        }

        public Bitmap Decode(EncodedFrame data)
        {
            using(MemoryStream str=new MemoryStream(data.data))
            {
                using (Bitmap decoded = new Bitmap(str))
                {
                    if (data.type == EncodedFrameType.I)
                    {
                        if (_prevFrame != null)
                            _prevFrame.Dispose();
                        _prevFrame = (Bitmap)decoded.Clone();
                        return decoded;
                    }
                    else if (data.type == EncodedFrameType.P)
                    {
                        using (Bitmap bmp = Add(_prevFrame, decoded))
                        {
                            if (_prevFrame != null)
                                _prevFrame.Dispose();
                            _prevFrame = (Bitmap)bmp.Clone();
                            return bmp;
                        }
                    }
                }
            }
            return _prevFrame;
        }
    }
}
