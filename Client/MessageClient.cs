namespace Client
{
    using Imgur.API;
    using Imgur.API.Authentication.Impl;
    using Imgur.API.Endpoints.Impl;
    using Imgur.API.Models;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Windows;

    /// <summary>
    /// Defines the <see cref="MessageClient" />.
    /// </summary>
    internal class MessageClient
    {
        /// <summary>
        /// Gets a value indicating whether ShouldStop.
        /// </summary>
        public bool ShouldStop { get; private set; }

        /// <summary>
        /// Gets the ViewModel.
        /// </summary>
        private MainWindowViewModel ViewModel { get; }

        /// <summary>
        /// Defines the socket.
        /// </summary>
        private Socket socket;

        //private string ServerIP = "127.0.0.1";
        /// <summary>
        /// Defines the ServerIP.
        /// </summary>
        private string ServerIP = "2.56.212.56";

        /// <summary>
        /// Defines the port.
        /// </summary>
        private int port = 1998;

        /// <summary>
        /// Defines the MAX_MESSAGE_BYTES.
        /// </summary>
        private const int MAX_MESSAGE_BYTES = 10_000_000;

        /// <summary>
        /// Gets or sets the prefix.
        /// </summary>
        public string prefix { get; set; } = "User";

        /// <summary>
        /// Gets or sets the profileSource.
        /// </summary>
        public string profileSource { get; set; } = "";

        /// <summary>
        /// Gets or sets the nameColor.
        /// </summary>
        public string nameColor { get; set; } = "#FF0000";

        /// <summary>
        /// Defines the canSend.
        /// </summary>
        public bool canSend = true;

        /// <summary>
        /// Gets or sets a value indicating whether sendCommand.
        /// </summary>
        public bool sendCommand { get; set; } = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageClient"/> class.
        /// </summary>
        /// <param name="vm">The vm<see cref="MainWindowViewModel"/>.</param>
        public MessageClient(MainWindowViewModel vm)
        {
            ViewModel = vm;
        }

        /// <summary>
        /// The Connected.
        /// </summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool Connected()
        {
            try
            {
                bool part1 = socket.Poll(1000, SelectMode.SelectRead);
                bool part2 = (socket.Available == 0);
                if (part1 && part2)
                    return false;
                else
                    return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// The Connect.
        /// </summary>
        public void Connect()
        {
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ServerIP), port);
                socket.Connect(endPoint);
                if (!Connected())
                {
                    Disconnect();
                }
            }
            catch
            {
                Console.WriteLine("Client couldn't connect");
                Connect();
            }
        }

        /// <summary>
        /// The Send.
        /// </summary>
        /// <param name="message">The message<see cref="IMessage"/>.</param>
        public void Send(IMessage message)
        {
            if (Connected())
            {
                SendImplementation(message.GetBytes());
            }
            else
            {
                Disconnect();
            }
        }

        /// <summary>
        /// The Recieve.
        /// </summary>
        public void Recieve()
        {
            if (Connected())
            {
                ReceiveImplementation();
            }
            else
            {
                Disconnect();
            }
        }

        /// <summary>
        /// The SendImplementation.
        /// </summary>
        /// <param name="buffer">The buffer<see cref="byte[]"/>.</param>
        private void SendImplementation(byte[] buffer)
        {
            try
            {
                if (canSend)
                {
                    socket.Send(buffer, 0, buffer.Length, 0);
                    canSend = false;
                    Thread canSendThread = new Thread(CantSend);
                    canSendThread.Start();
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// The CantSend.
        /// </summary>
        private void CantSend()
        {
            Thread.Sleep(250);
            canSend = true;
        }

        /// <summary>
        /// The ReceiveImplementation.
        /// </summary>
        private void ReceiveImplementation()
        {
            byte[] recvBuffer = new byte[MAX_MESSAGE_BYTES];
            int received = socket.Receive(recvBuffer, recvBuffer.Length, SocketFlags.None);
            if (received == 0)
                return;

            byte[] contentBuffer = new byte[received - 1];
            MessageType messageType = (MessageType)recvBuffer[0];
            Array.Copy(recvBuffer, 1, contentBuffer, 0, contentBuffer.Length);

            IMessage message = messageType switch
            {
                MessageType.String => StringMessage.ParseData(contentBuffer),
                MessageType.Image => ImageMessage.ParseData(contentBuffer),
                MessageType.Command => CommandMessage.ParseData(contentBuffer),

                //// default
                _ => throw new Exception("Unrecognized message!")
            };
            if (message is CommandMessage msg && sendCommand)
            {
                checkCommand(msg);
                sendCommand = false;
            }
            Application.Current.Dispatcher.Invoke(() => ViewModel.Messages.Add(message));
        }

        /// <summary>
        /// The checkCommand.
        /// </summary>
        /// <param name="message">The message<see cref="CommandMessage"/>.</param>
        private void checkCommand(CommandMessage message)
        {
            message.commandType = message.Parameters[1];
            switch (message.commandType)
            {
                case Command.SetUserName:
                    prefix = message.Parameters[2];
                    break;

                case (string)Command.Clear:
                    Application.Current.Dispatcher.Invoke(() => ViewModel.Messages.Clear());
                    break;

                case Command.SetIp:

                    break;

                case Command.Help:

                    break;

                case Command.SetImage:
                    try
                    {
                        var client = new ImgurClient("3bfa9e553867a45");
                        var endpoint = new ImageEndpoint(client);
                        IImage image;
                        using (var fs = new FileStream(message.Parameters[2], FileMode.Open))
                        {
                            image = endpoint.UploadImageStreamAsync(fs).GetAwaiter().GetResult();
                        }
                        profileSource = image.Link;
                        Console.WriteLine(image.Link);
                    }
                    catch (ImgurException imgurEx)
                    {
                        Debug.Write(imgurEx.Message);
                    }     
                    break;

                case Command.SetColor:
                    if(message.Parameters.Count > 2)
                        nameColor = message.Parameters[2];
                    break;

                default:
                    StringMessage stringMessage = new StringMessage("Uknown command", "Client", null, null);
                    Application.Current.Dispatcher.Invoke(() => ViewModel.Messages.Clear());
                    break;
            }
        }

        /// <summary>
        /// The Disconnect.
        /// </summary>
        public void Disconnect()
        {
            if (socket != null)
            {
                ShouldStop = true;
                Console.WriteLine("Disonnecting");
                socket.Close();
            }
        }
    }
}
