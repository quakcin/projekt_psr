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
     * Odpowiedź o sukcesynwym utworzeniu folderu w folderze dyskowym.
     * </summary>
     */
    [Serializable]
    public class MkDirResponse
    {
        public bool success { get; set; }
    }

    /**
     * <summary>
     * Żadanie o utworzenie folderu w podanej o podanej nazwie <c>name</c>.
     * </summary>
     */

    [Serializable]
    public class MkDirRequest
    {
        public string name { get; set; }
    }

    /**
     * <summary>
     * Kontroler do obsługi rządań o utworzenie nowego folderu.
     * </summary>
     */
    class MkDir
    {
        /**
         * <summary>
         * Obsługa rzadania.
         * </summary>
         */
        public static Response Exec (Request request)
        {
            Response response = new Response();
            response.responseType = ResponseType.MkDir;

            MkDirRequest mkDirRequest = (MkDirRequest)request.body;

            Console.WriteLine("MKDIR: " + mkDirRequest);
            String effectivePath = Config.getInstance().GetServerStorageDir() + mkDirRequest.name;
            Directory.CreateDirectory(effectivePath);

            MkDirResponse mkDirResponse = new MkDirResponse();
            mkDirResponse.success = true;

            response.body = mkDirResponse;
            return response;
        }
    }



}
