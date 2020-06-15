using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Client
{
    class MessageClient
    {
        public bool ShouldStop { get; private set; }

        private Socket socket;
        private string ServerIP = "127.0.0.1";
        private int port = 1998;
        private const int MAX_MESSAGE_BYTES = 1_000_000;

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

        private void SendImplementation(byte[] buffer)
        {
            socket.Send(buffer, 0, buffer.Length, 0);
        }

        private void ReceiveImplementation()
        {
            byte[] recvBuffer = new byte[MAX_MESSAGE_BYTES];
            int received = socket.Receive(recvBuffer, recvBuffer.Length, SocketFlags.None);

            byte[] contentBuffer = new byte[received - 1];
            MessageType messageType = (MessageType)recvBuffer[0];
            Array.Copy(recvBuffer, 1, contentBuffer, 0, contentBuffer.Length);

            IMessage message = messageType switch
            {
                MessageType.String => StringMessage.ParseData(contentBuffer),
                MessageType.Image => ImageMessage.ParseData(contentBuffer),

                // default
                _ => throw new Exception("Unrecognized message!")
            };

            if (message is StringMessage strMessage)
                Debug.WriteLine("Recieved string message: " + strMessage.Message);
            else if (message is ImageMessage)
                Debug.WriteLine("Received image message");

            // TODO: ViewModel gamer
        }

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
