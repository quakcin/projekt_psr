using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cloud
{
    /**
     * <summary>
     * Podobnie jak w zapytaniu, w odpowiedzi również mamy rodzaj odpowiedzi,
     * definiują go wartości enumeratora <c>ResponseType</c>. W celu zachowania
     * zwięzłości w kodzie, nazwy odpowiedzi są tożsame z nazwami w zapytanich.
     * Należy zwrócić uwage że takowa zależność nie musi występować aby system
     * funkcjonował poprawnie.
     * </summary>
     */
    [Serializable]
    public enum ResponseType
    {
        Test,
        Listing,
        FetchFile,
        UploadFile,
        DelFile,
        RenameFile,
        MkDir
    }

    /**
     * <summary>
     * Odpowiedź serwer dla klienta zawiera rodzaj odpowiedzi <c>responseType</c>,
     * oraz ciało/szczegóły zawarte w polu <b>body</b>. Obłsugą odpowiedzi zajmuje
     * się już klient.
     * </summary>
     */
    [Serializable]
    public class Response
    {
        public ResponseType responseType { get; set; }
        public Object body { get; set; }
    }
}
