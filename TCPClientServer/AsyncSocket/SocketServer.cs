using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace AsyncSocket
{
    public class SocketServer
    {
        private IPAddress _ipAddress;
        private int _port;
        private TcpListener _tcpListener;
        private List<TcpClient> _clients;

        //TODO: Might be used by Windows Service
        public bool KeepRunning { get; set; }

        public event EventHandler<ClientEventArgs> ClientConnected;
        public event EventHandler<MessageEventArgs> MessageReceived;
        public event EventHandler<ClientEventArgs> ClientDisconnected;
        public event EventHandler<ServerStatusEventArgs> ServerStatusChanged;

        public SocketServer()
        {
            _clients = new List<TcpClient>();
        }

        protected virtual void OnServerStatusChanged(ServerStatusEventArgs e)
        {
            ServerStatusChanged?.Invoke(this, e);
        }

        protected virtual void OnClientConnected(ClientEventArgs e)
        {
            ClientConnected?.Invoke(this, e);
        }

        protected virtual void OnMessageReceived(MessageEventArgs e)
        {
            MessageReceived?.Invoke(this, e);
        }

        protected virtual void OnClientDisconnected(ClientEventArgs e)
        {
            ClientDisconnected?.Invoke(this, e);
        }

        public async void StartListeningForIncomingConnection(IPAddress ipAddress = null, int port = 8090)
        {
            if (ipAddress == null)
            {
                ipAddress = IPAddress.Any;
            }

            if (port <= 0)
            {
                port = 8090;
            }

            _ipAddress = ipAddress;
            _port = port;

            Debug.WriteLine(string.Format("IP Address: {0} - Port: {1}", _ipAddress.ToString(), _port));

            _tcpListener = new TcpListener(_ipAddress, _port);

            try
            {
                _tcpListener.Start();
                OnServerStatusChanged(new ServerStatusEventArgs
                {
                    Started = true
                });

                KeepRunning = true;

                while (KeepRunning)
                {
                    var tcpClient = await _tcpListener.AcceptTcpClientAsync();

                    _clients.Add(tcpClient);

                    Debug.WriteLine(
                        string.Format("Client connected successfully to server, count: {0} - {1}",
                        _clients.Count, tcpClient.Client.RemoteEndPoint)
                        );

                    ProcessClientMessages(tcpClient);

                    OnClientConnected(
                        new ClientEventArgs
                        {
                            Client = tcpClient.Client.RemoteEndPoint.ToString()
                        });
                }            
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                OnServerStatusChanged(new ServerStatusEventArgs
                {
                    Started = false
                });
            }
        }

        public void StopServer()
        {
            try
            {
                if (_tcpListener != null)
                {
                    _tcpListener.Stop();
                }

                foreach (TcpClient c in _clients)
                {
                    c.Close();
                }

                _clients.Clear();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            finally
            {
                OnServerStatusChanged(new ServerStatusEventArgs
                {
                    Started = false
                });
            }
        }

        private async void ProcessClientMessages(TcpClient paramClient)
        {
            NetworkStream stream = null;
            StreamReader reader = null;
            string clientEndPoint = paramClient.Client.RemoteEndPoint.ToString();

            try
            {
                stream = paramClient.GetStream();
                reader = new StreamReader(stream);

                //TODO: Use a better way for buffering data 
                char[] buff = new char[1024];

                while (KeepRunning)
                {
                    Debug.WriteLine("Ready to read data");

                    int ret = await reader.ReadAsync(buff, 0, buff.Length);

                    //Socket is disconnected , remove the client and send the event
                    if (ret == 0)
                    {
                        RemoveClient(paramClient);
                        OnClientDisconnected(
                          new ClientEventArgs
                          {
                              Client = clientEndPoint
                          });

                        Debug.WriteLine("Socket disconnected");
                        break;
                    }

                    //Store the received text
                    string receivedText = new string(buff);

                    Debug.WriteLine("Received text: " + receivedText);

                    //Send the event with message details
                    OnMessageReceived(new MessageEventArgs
                    {
                        Client = paramClient.Client.RemoteEndPoint.ToString(),
                        Message = receivedText
                    });

                    //Clear the buffer
                    Array.Clear(buff, 0, buff.Length);
                }
            }
            catch (Exception ex)
            {
                RemoveClient(paramClient);
                OnClientDisconnected(
                     new ClientEventArgs
                     {
                         Client = clientEndPoint
                     });

                Debug.WriteLine(ex.ToString());
            }

        }

        private void RemoveClient(TcpClient paramClient)
        {
            if (_clients.Contains(paramClient))
            {
                _clients.Remove(paramClient);
                Debug.WriteLine(String.Format("Client removed from server, count: {0}", _clients.Count));
            }
        }
    }
}
