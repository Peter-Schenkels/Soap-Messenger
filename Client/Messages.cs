using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace Client
{

    public class Command
    {
        /// <summary>
        /// Defines the setUserName, clear, setIp, help..
        /// </summary>
        public const string
            setUserName = "/setusername",
            clear = "/clear",
            setIp = "/setip",
            help = "/help",
            unknownCommand = "?";
    }
    public enum MessageType : byte
    {
        Image = (byte)'i',
        String = (byte)'s',
        Command = (byte)'c'
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
            byte[] strBuffer = Encoding.UTF32.GetBytes(Message);
            byte[] buffer = new byte[strBuffer.Length + 1];
            buffer[0] = (byte)MessageType.String;
            Array.Copy(strBuffer, 0, buffer, 1, strBuffer.Length);
            return buffer;
        }

        public static StringMessage ParseData(byte[] buffer)
        {
            var message = Encoding.UTF32.GetString(buffer);
            return new StringMessage(message);
        }
    }

    public class CommandMessage : IMessage
    {
        public string[] parameters { get; }

        public CommandMessage(string Message)
        {
            parameters = Message.Split(' ');   
        }
        public byte[] GetBytes()
        {
            string Message = "";
            foreach(string parameter in parameters)
            {
                Message += " " + parameter;
            }
            byte[] strBuffer = Encoding.UTF8.GetBytes(Message);
            byte[] buffer = new byte[strBuffer.Length + 1];
            buffer[0] = (byte)MessageType.Command;
            Array.Copy(strBuffer, 0, buffer, 1, strBuffer.Length);
            return buffer;
        }

        public static StringMessage ParseData(byte[] buffer)
        {
            var message = Encoding.UTF32.GetString(buffer);
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
            byte[] buffer = new byte[data.Length + 1];
            buffer[0] = (byte)MessageType.Image;
            Array.Copy(data, 0, buffer, 1, data.Length);
            return buffer;
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
            }
            image.Freeze();
            return new ImageMessage(image);
        }
    }
}
