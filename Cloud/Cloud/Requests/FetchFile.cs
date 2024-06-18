using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFile = System.IO.File;

namespace Cloud.Requests
{
    /**
     * <summary>
     * Odpowiedź zawierająca plik w postaci tablicy bajtów <c>content</c>.
     * Zawiera też nazwę pliku <c>name</c>.
     * </summary>
     */
    [Serializable]
    public class FetchFileResponse
    {
        public string name { get; set; }
        public byte[] content { get; set; }
    }

    /**
     * <summary>
     * Zapytanie o przesłanie zawartości konkretnego pliku pod 
     * nazwą <c>name</c>.
     * </summary>
     */

    [Serializable]
    public class FetchFileRequest
    {
        public string name { get; set; }
    }

    /**
     * <summary>
     * Kontroler obsłgujący żądanie klienta, o przesłanie mu konkretnego
     * pliku o podanej w zaptyaniu ścieżce.
     * 
     * Plik jest odsyłąny cały, binarnie, w odpoiwedzi - tj. klasie <c>FetchFileResponse</c>
     * </summary>
     */
    class FetchFile
    {

        /**
         * <summary>
         * Obsługa zapytania o przesłanie pliku.
         * </summary>
         */
        public static Response Exec (Request request)
        {
            Response response = new Response();
            response.responseType = ResponseType.FetchFile;

            FetchFileRequest fetchFileRequest = (FetchFileRequest)request.body;
            FetchFileResponse fetchFileResponse = new FetchFileResponse();
            fetchFileResponse.name = fetchFileRequest.name;

            /**
             * Czytamy binarny plik i wstawiamy go do response.content
             */
            byte[] bytes = SFile.ReadAllBytes(Config.getInstance().GetServerStorageDir() + fetchFileRequest.name);
            fetchFileResponse.content = bytes;

            response.body = fetchFileResponse;
            return response;
        }
    }
}
