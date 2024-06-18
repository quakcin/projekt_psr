using MySqlX.XDevAPI;
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
    /**
     * <summary>
     * Klasa odpowiedzialna za obłsugę konkretnego klienta.
     * Zawiera ona interfejs komunikacyjny, jak i metody służące
     * do tworzenia osobnych wątków dla każdego klienta aplikacji.
     * </summary>
     */
    internal class ClientHandler
    {
        /**
         * <value>
         * Połączenie z klientem
         * </value>
         */
        private TcpClient tcpClient;

        /**
         * <value>
         * Strumień połączeniowy z klientem
         * </value>
         */
        private NetworkStream networkStream;

        /**
         * <value>
         * Serializator binarny obiektów
         * </value>
         */
        private BinaryFormatter binaryFormatter;

        /**
         * <value>
         * Informacja o tym czy istnieje połączenie z klientem
         * </value>
         */
        private bool isConnected = true;

        /**
         * <summary>
         * Konstrukor nie powinien być wyowyływany przez inne metody niż
         * statyczna metoda <c>Spawn</c>, w osobistym wątku klienta.
         * </summary>
         */
        public ClientHandler (TcpClient tcpClient)
        {
            this.tcpClient = tcpClient;
        }

        /**
         * <summary>
         * Statyczna metoda tworząca osobisty wątek dla każdego klienta,
         * dzięki niej, aplikacja pozwala na dostęp wielu klientów w tym
         * samym czasie do zasobów na dysku.
         * </summary>
         */
        public static void Spawn (TcpClient tcpClient)
        {
            Console.WriteLine("Spawned ClientHandler");
            Task.Run(() =>
            {
                ClientHandler clientHandler = new ClientHandler(tcpClient);
                clientHandler.networkStream = tcpClient.GetStream();
                clientHandler.binaryFormatter = new BinaryFormatter();

                IPEndPoint remoteEndPoint = tcpClient.Client.RemoteEndPoint as IPEndPoint;
                string clientIp = remoteEndPoint.Address.ToString();


                while (true)
                {
                    if (!clientHandler.isConnected)
                    {
                        break;
                    }

                    clientHandler.Communicate(clientIp);
                }

            });
        }

        /**
         * <summary>
         * Interfejs do wymiany danych między klientem a serwerem.
         * Komunikacja odbywa się naprzemiennie. Najpierw klient
         * wysyła swoje zapytanie <c>Request</c>, następnie zapytanie
         * jest obłsugiwane przez klasę <c>RequestHandler</c>, końcowa
         * odpowiedź pochodząca z tej klasy jest odsyłana do klienta
         * w formie <c>Response</c>.
         * <param name="clientIp">Adres ip clienta dla archiwum</param>
         * </summary>
         */
        public void Communicate (string clientIp)
        {

            try
            {
                Request request = (Request) binaryFormatter.Deserialize(networkStream);
                Metrics requestMetrics = new Metrics("Communicate_" + request.requestType.ToString());
                networkStream.Flush();

                Response response = RequestHandler.Handle(request, clientIp);
                binaryFormatter.Serialize(networkStream, response);
                requestMetrics.Finish();

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
