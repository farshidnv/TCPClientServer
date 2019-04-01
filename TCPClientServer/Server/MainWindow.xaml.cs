using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Windows;
using AsyncSocket;

namespace Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SocketServer asyncSocketServer;
        ObservableCollection<ClientInformation> data;
        public MainWindow()
        {
            InitializeComponent();
            data = new ObservableCollection<ClientInformation>();
            DataContext = data;
            asyncSocketServer = new SocketServer();
            asyncSocketServer.ClientConnected += AsyncSocketServer_ClientConnected;
            asyncSocketServer.ClientDisconnected += AsyncSocketServer_ClientDisconnected;
            asyncSocketServer.MessageReceived += AsyncSocketServer_MessageReceived;
            asyncSocketServer.ServerStatusChanged += AsyncSocketServer_ServerStatusChanged;
        }

        private void AsyncSocketServer_ServerStatusChanged(object sender, ServerStatusEventArgs e)
        {
            _start.IsEnabled = !e.Started;
            _stop.IsEnabled = e.Started;
        }

        private void AsyncSocketServer_MessageReceived(object sender, MessageEventArgs e)
        {
            var ci = new ClientInformation
            {
                ConnectionStatusText = e.Client,
                Message = e.Message.TrimEnd('\0').TrimEnd()
            };
            data.Add(ci);
        }

        private void AsyncSocketServer_ClientDisconnected(object sender, ClientEventArgs e)
        {
            var ci = new ClientInformation
            {
                ConnectionStatusText = e.Client + " disconnected",
                Connected = "false"
            };
            data.Add(ci);
        }

        private void AsyncSocketServer_ClientConnected(object sender, ClientEventArgs e)
        {
            var ci = new ClientInformation
            {
                ConnectionStatusText = e.Client + " connected",
                Connected = "true"
            };
            data.Add(ci);
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            asyncSocketServer.StartListeningForIncomingConnection(null, Convert.ToInt32(ConfigurationManager.AppSettings["ServerPort"]));

        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            asyncSocketServer.StopServer();
        }
    }
}
