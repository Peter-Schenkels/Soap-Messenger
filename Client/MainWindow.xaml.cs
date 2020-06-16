using AdonisUI.Controls;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Client
{
    public partial class MainWindow : AdonisWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        /// 
        private MessageClient client;

        public MainWindow()
        {
            InitializeComponent();
            var vm = new MainWindowViewModel();
            DataContext = vm;
            client = new MessageClient(vm);
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
        private void send()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if(messageBox.Text.Length < 255 && messageBox.Text.Length > 0)
                    if (messageBox.Text[0] != '/')
                    {
                        StringMessage message = new StringMessage(messageBox.Text);
                        client.Send(message);
                    }
                    else
                    {
                        CommandMessage message = new CommandMessage(messageBox.Text);
                        client.Send(message);
                    }
                    messageBox.Text = "";
            });
        }
        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            send();
        }

        private void AdonisWindow_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach(var file in files)
                {
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.UriSource = new Uri(file);
                    image.EndInit();
                    image.Freeze();
                    client.Send(new ImageMessage(image));
                }
            }
        }

        private void AdonisWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            client.Disconnect();
        }

        private void AdonisWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter && messageBox.Text != "")
            {
                //execute go button method
                send();

            }
        }
    }
}
