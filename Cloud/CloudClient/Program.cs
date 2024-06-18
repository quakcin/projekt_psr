using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/**
 * <summary>
 * Klasa <c>Program</c> stanowi najbardziej podstawową klasę w
 * całym systemie po stronie kleinta. Zawiera ona funkcję <c>Main</c> 
 * która powołuje instancję klasy <c>Program</c> a ta z koleji powołuje
 * instancje klasy <c>Client</c> odpowiedzialnej za stanowienie aplikacji
 * klienckiej.
 * </summary>
 */
namespace CloudClient
{
    internal class Program
    {
        /**
         * <summary>
         * Konstrukor klasy <c>Program</c> jedynie powołuje nową
         * instancje  klasy <c>Client</c>
         * </summary>
         */
        public Program ()
        {
            new Client();
        }

        /**
         * <summary>
         * Statyczna metoda <c>Main</c> wywoływana zaraz po
         * starice aplikacji. Tworzy instancję klasy <c>Program</c>
         * </summary>
         */
        static void Main(string[] args)
        {
            new Program();
        }
    }
}
