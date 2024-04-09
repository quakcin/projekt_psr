using Cloud;
using Cloud.Requests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Cache;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CloudClient
{
    internal class Client
    {
        private TcpClient tcpClient;
        private string ip = "127.0.0.1";
        private int port = 25565;

        private NetworkStream networkStream;
        private BinaryFormatter binaryFormatter;

        public Client ()
        {
            tcpClient = new TcpClient(ip, port);
            networkStream = tcpClient.GetStream();
            binaryFormatter = new BinaryFormatter();
            TestConnection();
        }

        public Response Communcate (Request request)
        {
            try
            {
                binaryFormatter.Serialize(networkStream, request);
                Console.WriteLine("Sent request");

                Response response = (Response)binaryFormatter.Deserialize(networkStream);
                Console.WriteLine("Response is: " + response.responseType.ToString());
                networkStream.Flush();
                return response;
            }
            catch (Exception ex)
            {
                networkStream.Close();
                tcpClient.Close();
            }
            return null;
        }


        public void TraverseDirTree (Dir dir, String padd)
        {
            foreach (Dir subdir in dir.subdirs)
            {
                Console.WriteLine(padd + subdir.name);
                TraverseDirTree(subdir, padd + "  ");
            }

            foreach (Cloud.Requests.File f in dir.files)
            {
                Console.WriteLine(padd + f.name);
            } 
        }

        public void TestConnection ()
        {
            Request request = new Request();
            request.requestType = RequestType.Test;
            request.body = (Object)"Hello World!";

            Communcate(request);

            request.requestType = RequestType.Listing;
            request.body = null;
            Response resp = Communcate(request);

            Dir listing = (Dir)resp.body;
            TraverseDirTree(listing, "  ");
            // Console.WriteLine(listing);
        }
    }
}
