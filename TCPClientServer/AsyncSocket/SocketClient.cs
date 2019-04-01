using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AsyncSocket
{
    public class SocketClient
    {
        private TcpClient _clinet = null;
        private IPAddress _serverIpAddress;
        private int _serverPort;

        public event EventHandler<ServerEventArgs> ServerDisconnected;
        public event EventHandler<ServerEventArgs> ServerConnected;

        protected virtual void OnServerDisconnected(ServerEventArgs e)
        {
            ServerDisconnected?.Invoke(this, e);
        }

        protected virtual void OnServerConnected(ServerEventArgs e)
        {
            ServerConnected?.Invoke(this, e);
        }

        public void SetServerIPAddress(string ipAddress)
        {
            IPAddress ipaddr = null;

            if (!IPAddress.TryParse(ipAddress, out ipaddr))
                throw new ArgumentException("Invalid server IP Address.");

            _serverIpAddress = ipaddr;
        }

        public void SetServerPort(string port)
        {
            int portNumber = 0;

            if (!int.TryParse(port.Trim(), out portNumber))
                throw new ArgumentException("Invalid port number supplied");

            if (portNumber <= 0 || portNumber > 65535)
                throw new ArgumentException("Port number must be between 0 and 65535.");

            _serverPort = portNumber;

        }

        public void DisconnectFromServer()
        {
            if (_clinet == null)
                return;

            if (!_clinet.Connected)
                return;

            string serverAddress = _clinet.Client.RemoteEndPoint.ToString();
            _clinet.Close();

            OnServerDisconnected(new ServerEventArgs
            {
                Server = serverAddress
            });
        }

        public async Task SendToServer(string message)
        {
            if (string.IsNullOrEmpty(message))
                throw new ArgumentException("message can't be empty");

            if (_clinet == null)
                return;

            if (!_clinet.Connected)
                return;

            StreamWriter clientStreamWriter = new StreamWriter(_clinet.GetStream());
            clientStreamWriter.AutoFlush = true;

            await clientStreamWriter.WriteAsync(message);
            //Log message
            Console.WriteLine("Data has been sent to server");
        }

        public async Task ConnectToServer()
        {
            _clinet = new TcpClient();
            await _clinet.ConnectAsync(_serverIpAddress, _serverPort);
            Console.WriteLine(string.Format("Connected to server IP/Port: {0} / {1}",
                _serverIpAddress, _serverIpAddress));

            OnServerConnected(new ServerEventArgs
            {
                Server = _clinet.Client.RemoteEndPoint.ToString()
            });
        }
    }
}
