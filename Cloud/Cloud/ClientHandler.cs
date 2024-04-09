using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Cloud
{
    internal class ClientHandler
    {
        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private BinaryFormatter binaryFormatter;
        private bool isConnected = true;

        public ClientHandler (TcpClient tcpClient)
        {
            this.tcpClient = tcpClient;
        }

        public static void Spawn (TcpClient tcpClient)
        {
            Console.WriteLine("Spawned ClientHandler");
            Task.Run(() =>
            {
                ClientHandler clientHandler = new ClientHandler(tcpClient);
                clientHandler.networkStream = tcpClient.GetStream();
                clientHandler.binaryFormatter = new BinaryFormatter();

                while (true)
                {
                    if (!clientHandler.isConnected)
                    {
                        break;
                    }

                    Console.WriteLine("Awaiting comms");
                    clientHandler.Communicate();
                }

            });
        }

        public void Communicate ()
        {

            try
            {
                Request request = (Request) binaryFormatter.Deserialize(networkStream);
                networkStream.Flush();

                Response response = RequestHandler.Handle(request);
                Console.WriteLine("Received: " + request.requestType.ToString());

                binaryFormatter.Serialize(networkStream, response);
                Console.WriteLine("Sent response");

            }
            catch (Exception ex)
            {
                networkStream.Close();
                tcpClient.Close();
                isConnected = false;
                Console.WriteLine("Error: " + ex.Message);
            }
        }


    }
}
