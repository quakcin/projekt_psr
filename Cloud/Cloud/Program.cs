using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/**
 * Program odpalany na serwerze
 */
namespace Cloud
{
    internal class Program
    {
        Server server;

        public Program ()
        {
            server = new Server();

        }

        static void Main(string[] args)
        {
            new Program();
            // 1. startuje serwer
            // 2. czeka na klientow
            //   2.a. pobiera info od klientow
            //   2.b. odsyla klienta pliki
        }
    }
}
