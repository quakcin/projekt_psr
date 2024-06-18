using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Cloud
{
    /**
     * <summary>
     * Swerwer tworzy aktywny <c>TcpListener</c>, który nasłuchuje na połaczenia
     * od klientów. Ponad to, preinstancjonowywuje obiekt <c>Config</c>, tak aby
     * w marię wczesnie wykryć jakieś problemy z konfiguracją.
     * </summary>
     */
    internal class Server
    {
        /**
         * <value>
         * Nasłuchiwacza na połączenia
         * </value>
         */
        private TcpListener tcpListener;

        /**
         * <value>
         * Domyślny port na którym serwer świadczy swoje usługi
         * </value>
         */
        private int port = 25565;


        /**
         * <summary>
         * Konstruktor serwera poza utworzeniem aktywnego <c>TcpListener</c> dodatkowo
         * preinstancjonowywuje obiekt <c>Config</c>, oraz też tworzy instancję <c>ArchiveServer</c>
         * czyli serwera świadczącego podgląd archiwum protokołem <c>HTTP</c>
         * </summary>
         */
        public Server ()
        {
            Config.getInstance();
            new ArchiveServer();
            SpawnListener();
            Listen();
        }


        /**
         * <summary>
         * Tworzy obiekt typu <c>TcpListener</c>, nasłuchujący na adresie
         * <c>IPAddress.Any</c> oraz domyślnym porice <c>port</c>.
         * Rozpoczyna nasłuchiwanie na tym obiekcie.
         * </summary>
         */
        private void SpawnListener ()
        {
            tcpListener = new TcpListener(IPAddress.Any, port);
            tcpListener.Start();
        }

        /**
         * <summary>
         * Akceptuje połączenia przychodzące z <c>tcpListener</c>, dla nowych
         * klientów wywołuje metodę do tworzenia nowych Handlerów <c>SpawnClientHandler</c>
         * </summary>
         */
        private void Listen ()
        {
            while (true)
            {
                TcpClient client = tcpListener.AcceptTcpClient();
                SpawnClientHandler(client);
            }
        }

        /**
         * <summary>
         * Wywołuje statyczna metodę <c>Spawn</c> w kontrolerze klientów <c>ClientHandler</c>
         * tworzącą wątek do obłsugi konkretnego klienta.
         * <param name="client"/>Klient TCP</paramref>
         * </summary>
         */
        private void SpawnClientHandler (TcpClient client)
        {
            ClientHandler.Spawn(client);
        }
    }
}
