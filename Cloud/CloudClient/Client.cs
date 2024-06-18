using Cloud;
using Cloud.Requests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Cache;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;


namespace CloudClient
{

    /**
     * <summary>
     * Klasa klient reprezentująca aplikację kliencką.
     * Poza interfejsowaniem z API systemu windows,
     * powołuje ona instancję klasy konfiguracyjnej <c>Config</c>,
     * oraz wszystkich innych klas potrzebnych aplikacji do
     * prawidłowego funkcjonowania, takich jak:
     * <c>FSObserver</c>, <c>Syncro</c>
     * 
     * Ponad to, tworzy ona menu konrekstowe w Tray'u systemu.
     * </summary>
     */
    internal class Client
    {
        /**
         * <summary>
         * Importujemy funkcję systemową służącą do pobierania
         * wskaźnika na okno konsolowe aplikacji, na którym przy
         * pomocy innych funkcji systemowych możemy dokonowyać
         * różnych operacji.
         * </summary>
         */
        [DllImport("kernel32.dll", SetLastError = true)]

        private static extern IntPtr GetConsoleWindow();
        /**
         * <summary>
         * Importujemy funkcję systemową służącą do pobierania
         * chowania i pokazywania okien, referencjonowanych
         * przy pomocy ich wskaźników systemowych.
         * </summary>
         */

        [DllImport("user32.dll")]

        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        /**
         * <value>
         * Otwarte połączenie tcp z de-facto serwerem.
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
         * Obserwator plików, obsługujący nasłuchiwanie na zmainy
         * w lokalnej instancji dysku sieciowego
         * </value>
         */
        private FSObserver fsObvserver;

        /**
         * <value>
         * Klasa służąca do synchronizacji zawartości lokalnego dusku
         * z dyskiem śieciowym.
         * </value>
         */
        private Syncro syncro;

        /**
         * <value>
         * Klasa zawierająca informację o konfiguracji klienta
         * </value>
         */
        private Config config;


        /**
         * <summary>
         * Konstrukor tworzy instancję witalnych kompoentów programu klieckeigo,
         * tworzy połączenie z serwerem oraz wywołuje metode twrozącą menu
         * kontekstowe w trayu.
         * </summary>
         */
        public Client ()
        {
            IntPtr handle = GetConsoleWindow();
            ShowWindow(handle, 0);

            config = new Config();
            tcpClient = new TcpClient(config.GetServerIP(), config.GetServerPort());
            networkStream = tcpClient.GetStream();
            binaryFormatter = new BinaryFormatter();
            fsObvserver = new FSObserver(this);
            syncro = new Syncro(this);
            new SysTray(this);

            MainLoop();
        }

        /**
         * <summary>
         * Główna pętla programu, wykonująca krokowo następujące czynności:
         * - synchronizacja zawartości loklanego dysku z dyskiem siecowym
         * - wywołanie następnego zapytania z kolejki obserwatora
         * - reset flag potrzebnych aplikacji w innych miejsach
         * </summary>
         */
        private void MainLoop ()
        {
            while (true)
            {
                syncro.Synchronize();
                fsObvserver.AdvanceRequestPipe();
                config.EndOfMainLoop();
            }
        }

        /**
         * <summary>
         * Podstawowy interfej do komunikacji, komunikacja działa w obie strony na przemiennie.
         * To znaczy, najpierw klient wysyła swoje zapytanie a następnie oczekuje na odpowiedź
         * od serwera.
         * </summary>
         */
        public Response Communcate (Request request)
        {
            Thread.Sleep(100);
            if (request.requestType != RequestType.Listing)
            {
                Console.WriteLine("[] [] [] REQUESTING: " + request.requestType);
            }

            try
            {
                binaryFormatter.Serialize(networkStream, request);

                Response response = (Response)binaryFormatter.Deserialize(networkStream); 
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


        /**
         * <summary>
         * Metoda potrzeban w łańcuchu zależności do pobrania instancji
         * klasy konfiguracyjnej
         * </summary>
         */
        public Config GetConfig ()
        {
            return config;
        }

        /**
         * <summary>
         * Metoda potrzeban w łańcuchu zależności do pobrania instancji
         * klasy synchoronizacyjnej.
         * </summary>
         */
        public Syncro GetSyncro ()
        {
            return syncro;
        }

        public void KillSelf ()
        {
            Application.Exit();
        }
    }
}
