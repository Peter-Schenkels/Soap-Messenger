using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace Client
{
    public enum MessageType : byte
    {
        Image = (byte)'i',
        String = (byte)'s'
    }

    public interface IMessage
    {
        public byte[] GetBytes();
    }

    public class StringMessage : IMessage
    {
        public string Message { get; }

        public StringMessage(string message)
        {
            Message = message;
        }

        public byte[] GetBytes()
        {
            byte[] strBuffer = Encoding.UTF8.GetBytes(Message);
            byte[] buffer = new byte[strBuffer.Length + 1];
            buffer[0] = (byte)MessageType.String;
            Array.Copy(strBuffer, 0, buffer, 1, strBuffer.Length);
            return buffer;
        }

        public static StringMessage ParseData(byte[] buffer)
        {
            var message = Encoding.UTF8.GetString(buffer);
            return new StringMessage(message);
        }
    }

    public class ImageMessage : IMessage
    {
        public BitmapImage Image { get; }

        public ImageMessage(BitmapImage image)
        {
            Image = image;
        }

        public byte[] GetBytes()
        {
            byte[] data;
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(Image));
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                data = ms.ToArray();
            }
            return data;
        }

        public static ImageMessage ParseData(byte[] buffer)
        {
            if (buffer == null || buffer.Length == 0)
                return null;

            var image = new BitmapImage();
            using (var ms = new MemoryStream(buffer))
            {
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = ms;
                image.EndInit();
                image.Freeze();
            }

            return new ImageMessage(image);
        }
    }
}
