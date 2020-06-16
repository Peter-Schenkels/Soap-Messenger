namespace Server
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    /// <summary>
    /// Defines the <see cref="ClientConnection" />.
    /// </summary>
    public class ClientConnection
    {
        /// <summary>
        /// Gets or sets the socket.
        /// </summary>
        public Socket socket { get; set; }

        /// <summary>
        /// Gets or sets the thread.
        /// </summary>
        public Thread thread { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public String name { get; set; }

        /// <summary>
        /// Defines the done.
        /// </summary>
        public bool done = false;

        /// <summary>
        /// Defines the message.
        /// </summary>
        public byte[] message;

        /// <summary>
        /// The connected.
        /// </summary>
        /// <returns>The <see cref="bool"/>.</returns>
        public bool connected()
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
        /// The Recieve.
        /// </summary>
        public void Recieve()
        {
            if (connected())
            {
                RecieveImplementation();
            }
            else
            {
                disconnect();
            }
        }
        /// <summary>
        /// The Broadcast.
        /// </summary>
        /// <param name="buffer">The buffer<see cref="byte[]"/>.</param>
        public void Broadcast(byte[] buffer)
        {
            if (connected())
            {
                BroadcastImplementation(buffer);
            }
            else
            {
                disconnect();
            }
        }

        /// <summary>
        /// The BroadcastImplementation.
        /// </summary>
        /// <param name="message">The message<see cref="byte[]"/>.</param>
        private void BroadcastImplementation(byte[] message)
        {
            try
            {
                socket.Send(message, 0, message.Length, 0);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                disconnect();
            }
        }

        /// <summary>
        /// The RecieveImplementation.
        /// </summary>
        private void RecieveImplementation()
        {
            byte[] buffer = new byte[10_000_000];
            try
            {
                Console.WriteLine("Waiting for a message from client:" + name);
                int recieved = socket.Receive(buffer, 0, buffer.Length, 0);
                Console.WriteLine("Message recieved from client:" + name);
                Array.Resize(ref buffer, recieved);
                message = buffer;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                disconnect();
            }
        }

        /// <summary>
        /// The disconnect.
        /// </summary>
        public void disconnect()
        {
            if (socket != null)
            {
                Console.WriteLine("Disonnecting client: " + name);
                socket.Close();
            }
            if (thread != null)
            {
                Console.WriteLine("Closing thread: " + thread.Name);
                thread.Interrupt();
                done = true;
            }
        }
    }

;

    /// <summary>
    /// Defines the <see cref="Server" />.
    /// </summary>
    internal class Server
    {
        /// <summary>
        /// Defines the listener.
        /// </summary>
        private static Socket listener;

        /// <summary>
        /// Defines the port.
        /// </summary>
        private static int port = 1998;

        /// <summary>
        /// Defines the maxConnections.
        /// </summary>
        private static int maxConnections = 0;

        /// <summary>
        /// Defines the Clients.
        /// </summary>
        private static List<ClientConnection> Clients = new List<ClientConnection>();

        /// <summary>
        /// Defines the buffer.
        /// </summary>
        public byte[] buffer = new byte[10_000_000];

        /// <summary>
        /// The bindSocket.
        /// </summary>
        public static void bindSocket()
        {
            try
            {
                listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(new IPEndPoint(IPAddress.Any, port));
                listener.Listen(maxConnections);
                Console.WriteLine("Binded to port: " + port);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                bindSocket();
            }
        }

        /// <summary>
        /// The LookForClient.
        /// </summary>
        public static void LookForClient()
        {
            while (true)
            {
                var rand = new Random();
                ClientConnection client = new ClientConnection();
                Console.WriteLine("Waiting for a client to connect...");
                try
                {
                    client.socket = listener.Accept();
                    client.name = rand.Next(100).ToString();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }
                ParameterizedThreadStart thread = new ParameterizedThreadStart(ListenForMessages);
                client.thread = new Thread(thread);
                client.thread.IsBackground = true;
                client.thread.Start(client);
                Clients.Add(client);
                Console.WriteLine("Client: " + client.name + " Connected.");
            }
        }

        /// <summary>
        /// The ListenForMessages.
        /// </summary>
        /// <param name="Object">The Object<see cref="object"/>.</param>
        public static void ListenForMessages(object Object)
        {
            ClientConnection client = (ClientConnection)Object;
            Console.WriteLine("Thread start");
            while (!client.done)
            {
                client.Recieve();
                foreach (var connection in Clients)
                {
                    Console.WriteLine("sending message from client: " + client.name + " to client: " + connection.name);
                    connection.Broadcast(client.message);
                }
            }
        }
    }

    /// <summary>
    /// Defines the <see cref="Program" />.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Defines the server.
        /// </summary>
        internal Server server;

        /// <summary>
        /// The Main.
        /// </summary>
        /// <param name="args">The args<see cref="string[]"/>.</param>
        internal static void Main(string[] args)
        {
            Console.WriteLine("==================================================================");
            Console.WriteLine("Welcome");
            Console.WriteLine("==================================================================");
            Server.bindSocket();
            Server.LookForClient();
            Console.WriteLine("==================================================================");
            Console.WriteLine("Goodbye");
            Console.WriteLine("==================================================================");
        }
    }
}
