using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/**
 * <summary>
 * Klasa <c>Program</c> stanowi najbardziej podstawową klasę w
 * całym systemie. Zawiera ona funkcję <c>Main</c> która powołuje
 * instancję klasy <c>Serwer</c> będącą 
 * </summary>
 */
namespace Cloud
{
    internal class Program
    {
        /**
         * <value>
         * Jedyna aktywna instnacja klasy <c>Serwer</c> świadczącej
         * usłgui dysku w chmurze.
         * </value>
         */
        Server server;

        /**
         * <summary>
         * Uwaga, instancję serwera powołujemy od razu w konstruktorze
         * </summary>
         */
        public Program ()
        {
            server = new Server();
        }

        /**
         * <summary>
         * Main tworzy od razu instancję tej klasy
         * </summary>
         */
        static void Main(string[] args)
        {
            new Program();
        }
    }
}
