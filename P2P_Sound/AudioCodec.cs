using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2P_Sound
{
    class AudioCodec
    {
        Process ffmpeg;

        public delegate void DataAvailableEventHandler(DataAvailableEventArgs args);
        public event DataAvailableEventHandler DataOutAvailable;

        public class DataAvailableEventArgs
        {
            public byte[] data;
            public DataType type;
        }

        public enum DataType
        {
            Encoded,
            Decoded
        }

        public AudioCodec(DataType type)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                RedirectStandardOutput = true,
                RedirectStandardInput = true
            };
            if (type == DataType.Decoded)
            {
                startInfo.Arguments = $@"-i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1";
            }
            else
            {
                startInfo.Arguments = $@"-ac 2 -f s16le -ar 48000 -i pipe:0 -ac 2 -ar 44100 pipe:1";
            }
            ffmpeg = Process.Start(startInfo);
        }

        void FfmpegEncoderThreadProcessor()
        {
            while (true)
            {
                try
                {
                    byte[] buffer = new byte[1024];
                    if (ffmpeg.StandardOutput.BaseStream.CanRead && ffmpeg.StandardOutput.BaseStream.Read(buffer, 0, buffer.Length) > 0)
                    {
                        DataOutAvailable(new DataAvailableEventArgs
                        {
                            data = buffer,
                            type = DataType.Encoded
                        });
                    }
                }
                catch (Exception) { }
            }
        }

        void FfmpegDecoderThreadProcessor()
        {
            while (true)
            {
                try
                {

                }
                catch (Exception) { }
            }
        }

        public byte[] encode(byte[] data)
        {
            ffmpeg.StandardInput.Write(System.Text.Encoding.UTF8.GetString(data));
            return null;
        }

        public byte[] decode(byte[] data, int length)
        {
            ffmpeg.StandardInput.Write(System.Text.Encoding.UTF8.GetString(data));
            return null;
        }
    }
}
