using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cloud
{
    /**
     * <summary>
     * Lista wszystkich możliwych zapytań na jakie serwer
     * dysku w chumrze może odpowiedzieć
     * </summary>
     */
    [Serializable]
    public enum RequestType
    {
        Test, /* Debuggerskie */
        Listing, /* Klient chce otrzymać listing plików */
        FetchFile, /* Klient chce kopie konkretnego pliku */
        UploadFile, /* Klient chce wgrac plik */
        DelFile, /* Klient chce usunac plik */
        RenameFile, /* Klient zmienia nazwe pliku */
        MkDir /* Klient tworzy folder */
    }
    
    /**
     * <summary>
     * Każde zapytanie do serwera musi zostać obite w poniższą monadę,
     * rodzaj zapytania zależy od wartości w polu <c>requestType</c>,
     * natomiast samo ciało zapytania - tzn. obiekt z informacjami
     * i szczegółami zapytania zawarty jest w polu <c>body</c>.
     * 
     * Obiekt klasy <c>ReuqestHandler</c> przyjmuje takie zapytanie
     * i podaje je dalej do konkretnego obiektu odpowiedzalnego
     * za jego obsługę. 
     * </summary>
     */
    [Serializable]
    public class Request
    {
        public RequestType requestType { get; set; }
        public Object body { get; set; }
    }
}
