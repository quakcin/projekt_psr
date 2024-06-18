using MySqlX.XDevAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using SFile = System.IO.File;

namespace Cloud.Requests
{
    /**
     * <summary>
     * Odpowiedź o sukcesywnym wysłaniu pliku na serwer.
     * </summary>
     */
    [Serializable]
    public class UploadFileResponse
    {
        public Boolean success { get; set; }
    }


    /**
     * <summary>
     * Żądanie o wysłanie pliku na serwer, zawiera nazwę pliku <c>name</c> oraz jego zawartość
     * w postaci ciągu bajtów <c>content</c>.
     * </summary>
     */
    [Serializable]
    public class UploadFileRequest
    {
        public string name { get; set; }
        public byte[] content { get; set; }
    }

    /**
     * <summary>
     * Kontroler obsługujący rządania dodania nowego pliku, lub też
     * nadpisania starego pliku nową zawartością w folerze dyskowym.
     * </summary>
     */
    class UploadFile
    {

        public static Response Exec (Request request, string clientIp)
        {
            Response response = new Response();
            response.responseType = ResponseType.UploadFile;

            UploadFileRequest uploadFileRequest = (UploadFileRequest)request.body;

            String effectivePath = Config.getInstance().GetServerStorageDir() + uploadFileRequest.name;

            if (SFile.Exists(effectivePath))
            {
                /**
                 * UPDATE PLIKU
                 */
                Archive.GetInstance().UpdateFile(effectivePath, clientIp);
            }

            System.IO.File.WriteAllBytes(effectivePath, uploadFileRequest.content);

            UploadFileResponse uploadFileResponse = new UploadFileResponse();
            uploadFileResponse.success = true;
            response.body = uploadFileResponse;

            return response;
        }
    }
}
