using System;
using System.Collections.Generic;
using System.Text;

namespace AsyncSocket
{
    public class ClientEventArgs : EventArgs
    {
        public string Client { get; set; }
    }

    public class ServerEventArgs : EventArgs
    {
        public string Server { get; set; }
    }

    public class ServerStatusEventArgs : EventArgs
    {
        public bool Started { get; set; }
    }

    public class MessageEventArgs : EventArgs
    {
        public string Client { get; set; }
        public string Message { get; set; }
    }
}
