using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using SFile = System.IO.File;

namespace Cloud.Requests
{
    /**
     * <summary>
     * Odpowiedź informująca o sukcesywnym usunięciu pliku
     * </summary>
     */
    [Serializable]
    public class DelFileResponse
    {
        public bool success { get; set; }
    }

    /**
     * <summary>
     * Zapytanie zawierające nazwę pliku lub 
     * ścieżki do usunięcia, pole: <c>name</c>.
     * </summary>
     */
    [Serializable]
    public class DelFileRequest
    {
        public string name { get; set; }
    }

    /**
     * <summary>
     * Kontroler odpowiedzialny za obsługę evenu/żądania usunięcia
     * pliku lub folderu przez użytkownika na dysku.
     * </summary>
     */
    class DelFile
    {
        /**
         * <summary>
         * Metoda obsługująca zapytanie
         * </summary>
         */
        public static Response Exec (Request request, string clientIp)
        {
            Response response = new Response();
            response.responseType = ResponseType.DelFile;

            DelFileRequest delFileRequest = (DelFileRequest)request.body;

            String effectivePath = Config.getInstance().GetServerStorageDir() + delFileRequest.name;

            DelFileResponse delFileResponse = new DelFileResponse();
            delFileResponse.success = true;

            response.body = delFileResponse;

            Console.WriteLine("REQL: " + delFileRequest.name);
            if (SFile.Exists(effectivePath) == false && Directory.Exists(effectivePath) == false)
            {
                return response;
            }    

            if (SFile.GetAttributes(effectivePath).HasFlag(FileAttributes.Directory))
            {
                // Directory.Delete(effectivePath, true);
                Archive.GetInstance().DeleteDir(effectivePath, clientIp);
            }
            else
            {
                Archive.GetInstance().DeleteFile(effectivePath, clientIp);
                // SFile.Delete(effectivePath);
            }
 

            return response;
        }

        /**
         * <summary>
         * Jeżeli jakiś proces trzyma dostep do folderu/pliku, to
         * musimy zaczekać na swoją kolejkę.
         * <param name="filePath">Ścieżka do pliku/folderu</param>
         * </summary>
         */
        public static bool WaitForAccess(string filePath)
        {
            while (true)
            {
                try
                {
                    using (FileStream fileStream = SFile.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        return true;
                    }
                }
                catch (IOException)
                {
                    Thread.Sleep(100);
                }
            }
        }
    }



}
