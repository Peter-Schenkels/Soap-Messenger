namespace Client
{
    using Imgur.API;
    using Imgur.API.Authentication.Impl;
    using Imgur.API.Endpoints.Impl;
    using Imgur.API.Models;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// Defines the <see cref="Command" />.
    /// </summary>
    public class Command
    {
        /// <summary>
        /// Defines the setUserName, clear, setIp, help....
        /// </summary>
        public const string
            SetUserName = "/setusername",
            Clear = "/clear",
            SetIp = "/setip",
            Help = "/help",
            SetImage = "/setimage",
            SetColor = "/setcolor",
            UnknownCommand = "?";

        /// <summary>
        /// The IsCommand.
        /// </summary>
        /// <param name="token">The token<see cref="string"/>.</param>
        /// <returns>The <see cref="String"/>.</returns>
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

    /// <summary>
    /// Defines the MessageType.
    /// </summary>
    public enum MessageType : byte
    {
        /// <summary>
        /// Defines the Image.
        /// </summary>
        Image = (byte)'i',
        /// <summary>
        /// Defines the String.
        /// </summary>
        String = (byte)'s',
        /// <summary>
        /// Defines the Command.
        /// </summary>
        Command = (byte)'c'
    }

    /// <summary>
    /// Defines the <see cref="IMessage" />.
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// The GetBytes.
        /// </summary>
        /// <returns>The <see cref="byte[]"/>.</returns>
        public byte[] GetBytes();
    }

    /// <summary>
    /// Defines the <see cref="StringMessage" />.
    /// </summary>
    public class StringMessage : IMessage
    {
        /// <summary>
        /// Gets or sets the ColorCode.
        /// </summary>
        public string ColorCode { get; set; } = "#FF0000";

        /// <summary>
        /// Gets the Username.
        /// </summary>
        public string Username { get; }

        /// <summary>
        /// Gets the Message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the Time.
        /// </summary>
        public string Time { get; }

        /// <summary>
        /// Gets or sets the Profile.
        /// </summary>
        public string Profile { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringMessage"/> class.
        /// </summary>
        /// <param name="message">The message<see cref="string"/>.</param>
        /// <param name="username">The username<see cref="string"/>.</param>
        /// <param name="userprofile">The userprofile<see cref="string"/>.</param>
        /// <param name="color">The color<see cref="string"/>.</param>
        public StringMessage(string message, string username, string userprofile, string color)
        {

            Message = message;
            Username = username;
            Time = DateTime.Now.ToString("h:mm tt");
            Profile = userprofile;
            ColorCode = color;
        }

        /// <summary>
        /// The GetBytes.
        /// </summary>
        /// <returns>The <see cref="byte[]"/>.</returns>
        public byte[] GetBytes()
        {
            string json = JsonConvert.SerializeObject(this);
            byte[] strBuffer = Encoding.UTF32.GetBytes(json);
            byte[] buffer = new byte[strBuffer.Length + 1];
            buffer[0] = (byte)MessageType.String;
            Array.Copy(strBuffer, 0, buffer, 1, strBuffer.Length);
            return buffer;
        }

        /// <summary>
        /// The ParseData.
        /// </summary>
        /// <param name="buffer">The buffer<see cref="byte[]"/>.</param>
        /// <returns>The <see cref="StringMessage"/>.</returns>
        public static StringMessage ParseData(byte[] buffer)
        {
            var message = Encoding.UTF32.GetString(buffer);
            return JsonConvert.DeserializeObject<StringMessage>(message);
        }
    }

    /// <summary>
    /// Defines the <see cref="CommandMessage" />.
    /// </summary>
    public class CommandMessage : IMessage
    {
        /// <summary>
        /// Gets or sets the Username.
        /// </summary>
        public static string Username { get; set; } = "Peter Jenkels";

        /// <summary>
        /// Gets or sets the commandType.
        /// </summary>
        public string commandType { get; set; } = Command.UnknownCommand;

        /// <summary>
        /// Gets the Parameters.
        /// </summary>
        public List<string> Parameters { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandMessage"/> class.
        /// </summary>
        /// <param name="parameters">The parameters<see cref="string"/>.</param>
        /// <param name="username">The username<see cref="string"/>.</param>
        public CommandMessage(string parameters, string username)
        {
            Parameters = parameters.Split(' ').ToList();
            commandType = Command.IsCommand(Parameters[0]);
            Username = username;
        }

        /// <summary>
        /// The GetBytes.
        /// </summary>
        /// <returns>The <see cref="byte[]"/>.</returns>
        public byte[] GetBytes()
        {
            string Message = "";
            foreach (string parameter in Parameters)
            {
                Message += " " + parameter;
            }
            byte[] strBuffer = Encoding.UTF32.GetBytes(Message);
            byte[] buffer = new byte[strBuffer.Length + 1];
            buffer[0] = (byte)MessageType.Command;
            Array.Copy(strBuffer, 0, buffer, 1, strBuffer.Length);
            return buffer;
        }

        /// <summary>
        /// The ParseData.
        /// </summary>
        /// <param name="buffer">The buffer<see cref="byte[]"/>.</param>
        /// <returns>The <see cref="CommandMessage"/>.</returns>
        public static CommandMessage ParseData(byte[] buffer)
        {
            var parameters = Encoding.UTF32.GetString(buffer);
            return new CommandMessage(parameters, Username);
        }

        /// <summary>
        /// The ToString.
        /// </summary>
        /// <returns>The <see cref="string"/>.</returns>
        public override string ToString()
        {
            return string.Join(" ", Parameters);
        }
    }

    /// <summary>
    /// Defines the <see cref="ImageMessage" />.
    /// </summary>
    public class ImageMessage : IMessage
    {
        /// <summary>
        /// Gets or sets the Username.
        /// </summary>
        public string Username { get; } = "Peter Jenkels";

        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        public string Source { get; }


        public string Time { get; }


        public string ColorCode { get; }
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageMessage"/> class.
        /// </summary>
        /// <param name="username">The username<see cref="string"/>.</param>
        public ImageMessage(string source, string username, string colorCode)
        {
            Username = username;
            Source = source;
            Time = DateTime.Now.ToString("h:mm tt");
            ColorCode = colorCode;
        }

        /// <summary>
        /// The GetBytes.
        /// </summary>
        /// <returns>The <see cref="byte[]"/>.</returns>
        public byte[] GetBytes()
        {
            string json = JsonConvert.SerializeObject(this);
            byte[] strBuffer = Encoding.UTF32.GetBytes(json);
            byte[] buffer = new byte[strBuffer.Length + 1];
            buffer[0] = (byte)MessageType.Image;
            Array.Copy(strBuffer, 0, buffer, 1, strBuffer.Length);
            return buffer;
        }

        /// <summary>
        /// The ParseData.
        /// </summary>
        /// <param name="buffer">The buffer<see cref="byte[]"/>.</param>
        /// <returns>The <see cref="ImageMessage"/>.</returns>
        public static ImageMessage ParseData(byte[] buffer)
        {
            var message = Encoding.UTF32.GetString(buffer);
            return JsonConvert.DeserializeObject<ImageMessage>(message);
        }
    }
}
