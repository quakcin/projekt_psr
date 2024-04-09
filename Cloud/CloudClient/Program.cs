using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/**
 * Program odpalany w tle na kliencie
 */
namespace CloudClient
{
    internal class Program
    {
        public Program ()
        {
            new Client();
        }

        static void Main(string[] args)
        {
            // 1. laczy sie z serwerem
            // 2. startuje obserwator systemu pliku
            //   2.a. loguje zmiany na serwerze
            //   2.b. pobiera pliki z serwera
            new Program();
        }
    }
}
