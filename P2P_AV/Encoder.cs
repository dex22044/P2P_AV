using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace H264
{
    class Encoder
    {
        int _width, _height;
        int _keyframeInterval;
        int _toKeyframe;
        Bitmap _prevFrame;
        ImageCodecInfo jpgEncoder;
        EncoderParameters myEncoderParameters;

        public Encoder(int width, int height, int keyframeInterval)
        {
            _width = width;
            _height = height;
            _keyframeInterval = keyframeInterval;
            _toKeyframe = 0;
            jpgEncoder = GetEncoder(ImageFormat.Png);
            myEncoderParameters = new EncoderParameters(1);
        }

        private Bitmap Compare(Bitmap prev, Bitmap curr)
        {
            try
            {
                byte[] p = H264.BitmapToRGBA(prev, prev.Width, prev.Height);
                byte[] c = H264.BitmapToRGBA(curr, curr.Width, curr.Height);
                byte[] o = new byte[p.Length];

                for(int i = 0; i < o.Length; i++)
                {
                    o[i] = (byte)((p[i] - c[i]) + 127);
                }

                return H264.RGBtoBitmap(o, prev.Width, prev.Height);
            }
            catch (Exception) { return null; }
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

        public EncodedFrame Encode(Bitmap frame)
        {
            if (_toKeyframe == 0)
            {
                _toKeyframe = _keyframeInterval;
                if (_prevFrame != null)
                    _prevFrame.Dispose();
                _prevFrame = (Bitmap)frame.Clone();
                using (MemoryStream encoder = new MemoryStream())
                {
                    myEncoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
                    frame.Save(encoder, jpgEncoder, myEncoderParameters);

                    return new EncodedFrame { data = encoder.ToArray(), type = EncodedFrameType.I };
                }
            }
            else
            {
                using (Bitmap compared = Compare(_prevFrame, frame))
                {
                    _toKeyframe--;
                    //if (compared == null) return new EncodedFrame { data = null, type = EncodedFrameType.Skip };
                    using (MemoryStream encoder = new MemoryStream())
                    {
                        myEncoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
                        compared.Save(encoder, jpgEncoder, myEncoderParameters);

                        if (_prevFrame != null)
                            _prevFrame.Dispose();
                        _prevFrame = (Bitmap)frame.Clone();

                        return new EncodedFrame { data = encoder.ToArray(), type = EncodedFrameType.P };
                    }
                }
            }
        }
    }
}
