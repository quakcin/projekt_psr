using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Cloud
{
    internal class Server
    {
        private TcpListener tcpListener;
        private int port = 25565;

        public Server ()
        {
            SpawnListener();
            Listen();
        }


        private void SpawnListener ()
        {
            tcpListener = new TcpListener(IPAddress.Any, port);
            tcpListener.Start();
        }

        private void Listen ()
        {
            while (true)
            {
                TcpClient client = tcpListener.AcceptTcpClient();
                SpawnClientHandler(client);
            }
        }

        private void SpawnClientHandler (TcpClient client)
        {
            ClientHandler.Spawn(client);
        }
    }
}
