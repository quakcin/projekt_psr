using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cloud
{
    /**
     * <summary>
     * Klasa odpowiedzialna za świadczenie serwera HTTP z informacjami o 
     * archium - tj. do przeglądania archiwum. Działa na każdym adresie
     * sieciowym komputera, na porcie <c>8888</c>.
     * </summary>
     */
    internal class ArchiveServer
    {

        /**
         * <summary>
         * Wywołanie konstrukora tworzy wątek na którym działa aktywny
         * serwer <c>HTTP</c>.
         * </summary>
         */
        public ArchiveServer ()
        {
            Console.WriteLine("Archive Admin server instance");
            Thread t = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        Listener();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            });
            t.Start();
        }

        /**
         * <summary>
         * Nasłuchiwacz na zapytania od klientów <c>HTTP</c>. Po otrzymaniu
         * zapytania przekazuje jego kontekst do metody <c>Context</c>.
         * </summary>
         */
        public void Listener ()
        {
            HttpListener listener = new HttpListener();
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    listener.Prefixes.Add("http://" + ip.ToString() + ":8888/");
                }
            }

            // listener.Prefixes.Add("http://192.168.1.26:8888/");
            listener.Start();
            Console.WriteLine("Starting Listener...");
            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                Context(context);
            }
        }

        /**
         * <summary>
         * Metoda obłsugująca kontekst zapytania od klienta <c>HTTP</c>.
         * Przy pomocy metody <c>GenWebsite()</c> tworzy zawartość strony
         * w formacie <c>HTML</c> i odsyła ją klientowi z kodowaniem <c>UTF-8</c>.
         * </summary>
         */
        public void Context (HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            string responseString = GenWebiste();

            byte[] buffer = Encoding.UTF8.GetBytes(responseString);

            // Set the response headers
            response.ContentLength64 = buffer.Length;
            response.ContentType = "text/html";

            // Write the response content
            using (System.IO.Stream output = response.OutputStream)
            {
                output.Write(buffer, 0, buffer.Length);
            }
        }

        /**
         * <summary>
         * Metoda tworząca stronę w formacie <c>HTML</c> zrozumiałym dla
         * przeglądarek internetowych. Wywołuje metodę <c>FetchArchives()</c>
         * pobierającą informację o archiwum z <c>Serwera Bazodanowego</c>.
         * </summary>
         */
        public string GenWebiste ()
        {

            return "" +
                "<html>" +
                "   <head>" +
                "       <title>Super Cloud</title>" +
                "       <meta charset='UTF-8'/>" +
                "   </head>" +
                "   <body>" +
                "      <table border='1'>" +
                "        <tr>" +
                "          <td>Kod</td><td>Czas</td><td>Operacja</td><td>IP</td><td>Ścieżka</td>" +
                "        </tr>" +
                         FetchArchives() +
                "      </table>" +
                "   </body>" +
                "</html>";
        }

        /**
         * <summary>
         * Metoda pobierająca dane o archiwum z <c>Serwera Bazodanowego</c>.
         * Zwraca ona kod w formacie <c>HTML</c>, z już odpowiednio wyrenderowanymi
         * danymi w odpowiedniej formie.
         * </summary>
         */
        public string FetchArchives ()
        {
            ConfigDatabse db = Config.getInstance().db;
            String host = db.Host;
            String login = db.Login;
            String port = db.Port.ToString();
            String passw = db.Password;

            String res = "";

            string connectionString = "server=" + host + ";user=" + login + ";database=" + login + ";port=" + port + ";password=" + passw;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    String sql = "SELECT * FROM archive ORDER BY time DESC";
                    MySqlCommand command = new MySqlCommand(sql, conn);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                res += "<tr>";
                                res += "  <td>";
                                res += "    " + reader["slab"];
                                res += "  </td>";
                                res += "  <td>";
                                res += "    " + reader["time"];
                                res += "  </td>";
                                res += "  <td>";
                                res += "    <b>" + reader["operation"] + "</b>";
                                res += "  </td>";
                                res += "  <td>";
                                res += "    " + reader["ip"];
                                res += "  </td>";
                                res += "  <td>";
                                res += "    <pre>" + fetchContents(Config.getInstance().GetArchiveDir() + "\\" + reader["slab"]) + "</pre>";
                                res += "  </td>";
                                res += "</tr>";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }

            return res;
        }

        /**
         * <summary>
         * Metoda pobiera ścieżki w <c>slabie</c>, podanym jako już konkretna
         * ścieżka na dysku - do lepszego UX.
         * </summary>
         */
        public String fetchContents (String path)
        {
            List<string> files = new List<string>();
            if (!Directory.Exists(path))
            {
                return "MISSING";
            }
            fetchTraversal(path, files);
            return String.Join(", ", files);
        }

        /**
         * <summary>
         * Ze względnu na drzewistą naturę systemu plików, foldery najlpeij jest
         * traweroswać rekurencyjnie.
         * </summary>
         */
        public void fetchTraversal (String path, in List<string> lst)
        {
            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                lst.Add(file);
            }

            string[] subdirs = Directory.GetDirectories(path);
            foreach (string subdir in subdirs)
            {
                fetchTraversal(subdir, lst);
            }
        }

    }
}
