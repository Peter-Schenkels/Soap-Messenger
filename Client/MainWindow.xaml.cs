using AdonisUI.Controls;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Client
{
    public partial class MainWindow : AdonisWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        /// 
        private MessageClient client = new MessageClient();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Console.WriteLine("Start Connection");
            Thread thread = new Thread(StartConnection);
            thread.Start();
        }

        void StartConnection()
        {
            client.Connect();
            while (!client.ShouldStop)
            {
                try
                {
                    client.Recieve();
                }
                catch (SocketException ex)
                {
                    if (!client.ShouldStop)
                        throw ex;
                }
            }
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                StringMessage message = new StringMessage(messageBox.Text);
                client.Send(message);
            });
        }

        private void AdonisWindow_Drop(object sender, System.Windows.DragEventArgs e)
        {

            throw new NotImplementedException();

        }

        private void AdonisWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            client.Disconnect();
        }
    }
}
