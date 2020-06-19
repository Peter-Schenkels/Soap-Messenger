using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
            SetUserName = "/setusername",
            Clear = "/clear",
            SetIp = "/setip",
            Help = "/help",
            UnknownCommand = "?";

        public static String IsCommand(string token)
        {
            if (
                SetUserName == token || 
                Clear == token || 
                SetIp == token || 
                Help == token
                )
            {
                return token;
            }
            return Command.UnknownCommand;
        }
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
        public string ColorCode { get; } = "#FF0000";
        public string Username { get; } = "User";
        public string Message { get; }

        public StringMessage(string message, string username)
        {
            Message = message;
            Username = username;
        }

        public byte[] GetBytes()
        {
            string json = JsonConvert.SerializeObject(this);
            byte[] strBuffer = Encoding.UTF32.GetBytes(json);
            byte[] buffer = new byte[strBuffer.Length + 1];
            buffer[0] = (byte)MessageType.String;
            Array.Copy(strBuffer, 0, buffer, 1, strBuffer.Length);
            return buffer;
        }

        public static StringMessage ParseData(byte[] buffer)
        {
            var message = Encoding.UTF32.GetString(buffer);
            
            return JsonConvert.DeserializeObject<StringMessage>(message);
            
        }
    }

    public class CommandMessage : IMessage
    {
        public static string Username { get; set; } = "Peter Jenkels";
        public string commandType { get; set; } = Command.UnknownCommand;
        public List<string> Parameters { get; }

        public CommandMessage(string parameters, string username)
        {
            Parameters = parameters.Split(' ').ToList();
            commandType = Command.IsCommand(Parameters[0]);
            Username = username;
        }
        public byte[] GetBytes()
        {
            string Message = "";
            foreach(string parameter in Parameters)
            {
                Message += " " + parameter;
            }
            byte[] strBuffer = Encoding.UTF32.GetBytes(Message);
            byte[] buffer = new byte[strBuffer.Length + 1];
            buffer[0] = (byte)MessageType.Command;
            Array.Copy(strBuffer, 0, buffer, 1, strBuffer.Length);
            return buffer;
        }

        public static CommandMessage ParseData(byte[] buffer)
        {
            var parameters = Encoding.UTF32.GetString(buffer);
            return new CommandMessage(parameters, Username);
        }

        public override string ToString()
        {
            return string.Join(" ", Parameters);
        }
    }

    public class ImageMessage : IMessage
    {
        public static string Username { get; set; } = "Peter Jenkels";
        public BitmapImage Image { get; }

        public ImageMessage(BitmapImage image, string username)
        {
            Image = image;
            Username = username;
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
            return new ImageMessage(image, Username);
        }
    }
}
