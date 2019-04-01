using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AsyncSocket;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SocketClient client;
        public MainWindow()
        {
            InitializeComponent();
            client = new SocketClient();       
            client.ServerConnected += Client_ServerConnected;
            client.ServerDisconnected += Client_ServerDisconnected;
        }

        private async void Connect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                client.SetServerIPAddress(ConfigurationManager.AppSettings["ServerIpAddress"]);
                client.SetServerPort(ConfigurationManager.AppSettings["ServerPort"]);
                await client.ConnectToServer();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Connecting to server failed" + ex.ToString());
                MessageBox.Show("Connecting to server failed");
            }
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                client.DisconnectFromServer();
            }
            catch (Exception ex)
            {

                Console.WriteLine("Disconnecting from server failed" + ex.ToString());
                MessageBox.Show("Disconnecting from server failed");
            }
          
        }

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await client.SendToServer(_message.Text);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Sending data failed" + ex.ToString());
                MessageBox.Show("Sending data failed");
            }
        }
        private void Client_ServerDisconnected(object sender, ServerEventArgs e)
        {
            _disconnect.IsEnabled = false;
            _send.IsEnabled = false;
            _connect.IsEnabled = true;
            MessageBox.Show("Disconnected from " + e.Server);
        }

        private void Client_ServerConnected(object sender, ServerEventArgs e)
        {
            _connect.IsEnabled = false;
            _disconnect.IsEnabled = true;
            _send.IsEnabled = true;
            MessageBox.Show("Connected to "+ e.Server);
        }
    }
}
