using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Cloud.Requests
{
    /**
     * <summary>
     * Węzeł reprezentujący plik, zawiera jego nazwę <c>name</c>
     * oraz hasz md5 <c>md5</c>.
     * </summary>
     */
    [Serializable]
    public class File
    {
        public string name { get; set; }
        public string md5;
    }

    /**
     * <summary>
     * Węzeł reprezentujący folder, zawiera nazwę folderu <c>name</c>,
     * listę podfolderów <c>subdirs</c>, oraz listę plików <c>files</c>.
     * </summary>
     */
    [Serializable]
    public class Dir
    {
        public string name { get; set; }
        public List<Dir> subdirs { get; set; }
        public List<File> files { get; set; }
    }

    /**
     * <summary>
     * Korzeń zawierający jako pierwszy węzeł główny folder dysku.
     * </summary>
     */
    [Serializable]
    class ListingResponse
    {
        Dir dir { get; set; }
    }

    /**
     * <summary>
     * Kontroler obsługujący żądnie o listing plików.
     * Odsyła użytkownikowi drzewo folderów i plików w folderze dysku.
     * </summary>
     */
    class Listing
    {
        /**
         * <summary>
         * Zwraca drzewo reprezentujące system plików - tj. zbiór folderów
         * i plików w podanej ścieżce <c>dir</c>.
         * </summary>
         */
        private static Dir GetSpecificDirectory (String dir)
        {
            string sourceDir = Config.getInstance().GetServerStorageDir();

            string effectiveName = dir.Replace(sourceDir, "");
            Dir current = new Dir();
            current.subdirs = new List<Dir>();
            current.files = new List<File>();
            current.name = effectiveName;

            string[] dirs = Directory.GetDirectories(dir);
            foreach (string subdir in dirs)
            {
                current.subdirs.Add(GetSpecificDirectory(subdir));
            }

            string[] files = Directory.GetFiles(dir);
            foreach (string file in files)
            {
                File f = new File();
                f.name = file.Replace(sourceDir, "");
                f.md5 = FileMD5(file);
                current.files.Add(f);
            }

            return current;
        }


        /**
         * <summary>
         * Obsługą żądania o listing plików w folderze dyskowym.
         * </summary>
         */
        public static Response Exec (Request request)
        {
            Response response = new Response();
            response.responseType = ResponseType.Listing;

            Dir listing = GetSpecificDirectory(Config.getInstance().GetServerStorageDir());
            response.body = listing;

            return response;
        }


        /**
         * <summary>
         * Zwraca hasz w formacie <c>md5</c> podanego pliku <c>file</c>.
         * </summary>
         */
        public static string FileMD5 (string file)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = System.IO.File.OpenRead(file))
                {
                    byte[] hashBytes = md5.ComputeHash(stream);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}
