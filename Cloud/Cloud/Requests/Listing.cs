using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cloud.Requests
{
    [Serializable]
    public class File
    {
        public string name { get; set; }
    }

    [Serializable]
    public class Dir
    {
        public string name { get; set; }
        public List<Dir> subdirs { get; set; }
        public List<File> files { get; set; }
    }

    [Serializable]
    class ListingResponse
    {
        Dir dir { get; set; }
    }

    class Listing
    {
        private static string sourceDir = "C:\\Users\\marty\\Desktop\\PSR\\CLOUD\\Cloud\\storage";

        private static Dir GetSpecificDirectory (String dir, String source)
        {
            string effectiveName = dir.Replace(source, "");
            Dir current = new Dir();
            current.subdirs = new List<Dir>();
            current.files = new List<File>();
            current.name = effectiveName;

            string[] dirs = Directory.GetDirectories(dir);
            foreach (string subdir in dirs)
            {
                current.subdirs.Add(GetSpecificDirectory(subdir, dir));
            }

            string[] files = Directory.GetFiles(dir);
            foreach (string file in files)
            {
                File f = new File();
                f.name = file.Replace(dir, "");
                current.files.Add(f);
            }

            return current;
        }



        public static Response Exec (Request request)
        {
            Response response = new Response();
            response.responseType = ResponseType.Listing;

            Console.WriteLine("Read file tree");
            Dir listing = GetSpecificDirectory(sourceDir, sourceDir);
            response.body = listing;

            return response;
        }
    }
}
