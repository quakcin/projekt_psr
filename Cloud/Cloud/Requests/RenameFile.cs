using MySqlX.XDevAPI;
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
     * Odpowiedź informująca klienta o sukcesynwej zmianie nazwy pliku w fodlerze dyskowym.
     * </summary>
     */
    [Serializable]
    public class RenameFileResponse
    {
        public bool success { get; set; }
    }

    /**
     * <summary>
     * Żądanie o zmianę nazwy w folderze dyskowym jakiegoś pliku lub katalogu.
     * <param name="oldName">Stara nazwa</param>
     * <param name="newName">Nowa nazwa</param>
     * </summary>
     */
    [Serializable]
    public class RenameFileRequest
    {
        public string oldName { get; set; }
        public string newName { get; set; }
    }

    /**
     * Kontroler odpowiedzaliny za obsługę żądań o zmiany nazwy (plikow i katalogów).
     */
    class RenameFile
    {

        /**
         * Obsługa rżadania.
         */
        public static Response Exec (Request request, string clientIp)
        {
            String sourceDir = Config.getInstance().GetServerStorageDir();

            Response response = new Response();
            response.responseType = ResponseType.RenameFile;

            RenameFileRequest renameFileRequest = (RenameFileRequest)request.body;

            String oldName = sourceDir + renameFileRequest.oldName;
            String newName = sourceDir + renameFileRequest.newName;

            Console.WriteLine("REN: " + oldName + " -> " + newName);

            /**
             * W teori zmiana nazwy spowoduje usuniecie starego pliku
             * i pobranie nowego bez walki z FSObserverem
             */
            if (SFile.GetAttributes(oldName).HasFlag(System.IO.FileAttributes.Directory))
            {
                Archive.GetInstance().RenameDir(oldName, clientIp);
                Directory.Move(oldName, newName);
            }
            else
            {
                Archive.GetInstance().RenameFile(oldName, clientIp);
                SFile.Move(oldName, newName);
            }


            RenameFileResponse renameFileResponse = new RenameFileResponse();
            renameFileResponse.success = true;

            response.body = renameFileResponse;
            return response;
        }
    }
}
