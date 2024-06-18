using Cloud.Requests;
using Cloud;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using SFile = System.IO.File;
using CFile = Cloud.Requests.File;
using System.Threading;
using System.Reflection;

namespace CloudClient
{
    /**
     * <summary>
     * Klasa synchronizujaca stan lokalnego dysku klienta z
     * dyskiem sieciowym cloud'a
     * </summary>
     */
    internal class Syncro
    {
        /**
         * <value>
         * Obiekt klienta z wszystkimi potrzebnymi innymi obiektami
         * </value>
         */
        private Client client;

        /**
         * <value>
         * Lista nazw ścieżek które nie mogą zostać usunięte, nawet
         * jeżeli stan dysku serwerowego twierdzi inaczej.
         * </value>
         */
        private List<String> noDelete;

        /**
         * Lista nazw ścieżek które nie mogą zostać odtworzone z
         * dysku ściecowego z różych względów
         */
        public List<String> noCreate;

        /**
         * <summary>
         * Czas życia ścieżek w liście <c>noDelete</c>, po tym czasie
         * każdy z tych plików/folderów może zostąc spokojnie usunięty
         * </summary>
         */
        private Dictionary<String, int> noDelteLife;

        /**
         * <summary>
         * Konstrukor tworzy instancję wszystkich kolekcji potrzebnych
         * synchronizatorowi do prawidłowego działania.
         * </summary>
         */
        public Syncro (Client client)
        {
            this.client = client;
            this.noDelete = new List<string>();
            this.noCreate = new List<string>();
            this.noDelteLife = new Dictionary<string, int>();
        }


        /**
         * <summary>
         * Metoda odpowiedzialna za synchronizację stanu lokalnego dyski klienckiego
         * z serwerowym dyskiem cloud'a. Najpier usuwa ona pliki których nie ma na
         * serwerze i które nie znajdują się w <c>noDelte</c>, a następnie odtwrza
         * ścieżki których brakuje na lokalnym dysku i pobiera pliki który klient
         * jeszcze nie posiada.
         * </summary>
         */
        public void Synchronize ()
        {
            Dir listing = FetchListing();
            if (listing == null)
            {
                return;
            }

            // DeleteAbundantFiles(listing);

            CFile missingFile = FindAnyMissingFile(listing);
            if (missingFile == null)
            {
                return;
            }

            FetchFile(missingFile);
        }

        /**
         * <summary>
         * Metoda zamienia drzewo katalogu w prostą listę ścieżek.
         * </summary>
         */
        private List<string> ConvertDirTreeToListOfPaths (Dir listing)
        {
            List<string> lst = new List<string>();
            ConvertTraverseDirs(listing, lst);
            return lst;
        }

        /**
         * <summary>
         * Metoda wykorzystywana do trawersacji drzewa ścierzek przez:
         * <c>ConvertDirTreeToListOfPaths</c>
         * </summary>
         */
        private void ConvertTraverseDirs (Dir subdir, in List<string> lst)
        {
            foreach (CFile cf in subdir.files)
            {
                lst.Add(cf.name);
            }

            foreach (Dir dr in subdir.subdirs)
            {
                lst.Add(dr.name);
                ConvertTraverseDirs(dr, lst);
            }
        }

        /**
         * <summary>
         * Funkcja trawersująca drzewo ścieżek na bazie ścieżki bazowej, usuwająca
         * niepotrzebne pliki, tzn. takie których nie ma na serwerze i takie które
         * nie znajdują się w <c>noDelte</c>.
         * </summary>
         */
        private void DeleteAbundantTraverse (string basedir, List<string> paths)
        {
            string[] files = Directory.GetFiles(basedir);
            foreach (string file in files)
            {
                String path = file.Replace(client.GetConfig().GetCloudLocalPath(), "");

                if (paths.Contains(path))
                {
                    continue;
                }

                // noDelete.ForEach((l) => Console.WriteLine(l));

                if (noDelete.Contains(file))
                {
                    Console.WriteLine("SAFE NO DEL: " + file);

                    noDelteLife[file] -= 1;
                    if (noDelteLife[file] <= 0)
                    {
                        noDelteLife[file] = 0;
                        noDelete.Remove(file);
                    }
                    continue;
                }
                /**
                 * Lokalnego pliku nie ma w indexie serwera
                 * a więc go usuwamy
                 */

                Console.WriteLine("NIELEGALNY PLIK, USUWAM: " + file);

                FSObserver.WaitForAccess(file);
                SFile.Delete(file);
                
            }

            string[] dirs = Directory.GetDirectories(basedir);
            foreach (string dir in dirs)
            {
                // jezeli sciezki nie ma w listingu, to delete
                string localDirPath = dir.Replace(client.GetConfig().GetCloudLocalPath(), "");
                if (paths.Contains(localDirPath) == false)
                {
                    if (noDelete.Contains(localDirPath))
                    {
                        Console.WriteLine("SAFE NO DEL DIR: " + localDirPath);

                        noDelteLife[localDirPath] -= 1;
                        if (noDelteLife[localDirPath] <= 0)
                        {
                            noDelteLife[localDirPath] = 0;
                            noDelete.Remove(localDirPath);
                        }
                        continue;
                    }

                    Directory.Delete(dir, true);
                    continue;
                }

                DeleteAbundantTraverse(dir, paths);
            }
        }

        /**
         * <summary>
         * Metoda pobierająca konkretny plik z serwera, jeżeli nie znajduje
         * się on na kliencie i nie jest on wpisany w listę <c>noCreate</c>.
         * </summary>
         */
        private void FetchFile (CFile file)
        {
            String dest = client.GetConfig().GetCloudLocalPath() + file.name;
            if (noCreate.Contains(dest))
            {
                noCreate.Remove(dest);
                return;
            }    

            Request request = new Request();
            FetchFileRequest fetchFileRequest = new FetchFileRequest();
            fetchFileRequest.name = file.name;

            request.requestType = RequestType.FetchFile;
            request.body = fetchFileRequest;

            Response resp = client.Communcate(request);
            FetchFileResponse fetchFileResponse = (FetchFileResponse)resp.body;

            /**
             * Pobralismy binarny plik,
             * zapisujemy go lokalnie,
             * nie nasluchujemy na zmiany
             */
            try
            {
                Console.WriteLine("Written file: " + dest + " with: " + fetchFileResponse.content.Length + " bytes");
                System.IO.File.WriteAllBytes(dest, fetchFileResponse.content);
            }
            catch (Exception e)
            {
                // To naturalne
            }
        }

        /**
         * <summary>
         * Metoda wyszukująca plików, które nie znajdują się w drzewie <c>root</c>.
         * Wykorzystywana do wykrywania plików które znajdują sie na serwerze ale
         * nie koniecznie na kliencie.
         * </summary>
         */

        private CFile FindAnyMissingFile (Dir root)
        {
            /**
             * Jeżeli ścieżka nie istnieje
             * tworzymy ją
             */
            String dir = client.GetConfig().GetCloudLocalPath() + root.name;

            if (Directory.Exists(dir) == false)
            {
                if (noCreate.Contains(dir))
                {
                    noCreate.Remove(dir);
                }
                else
                {
                    Directory.CreateDirectory(dir);
                }
            }

            foreach (Dir subdir in root.subdirs)
            {
                CFile found = FindAnyMissingFile(subdir);
                /**
                 * Zwracamy pierwszy nie istniejacy (lub zmieniony) 
                 * plik do synchronizacji
                 */
                if (found != null)
                {
                    return found;
                }
            }

            foreach (CFile file in root.files)
            {
                /**
                 * Synchronizujemy pliki ktore nie istnieja
                 */
                if (System.IO.File.Exists(client.GetConfig().GetCloudLocalPath() + file.name) == false)
                {
                    return file;
                }
            }    

            return null;
        }

        /**
         * <summary>
         * Metoda pobierająca listing plików - tzn. drzewo plików i ścieżek
         * obecnego stanu dysku serwerowego.
         * </summary>
         */
        private Dir FetchListing ()
        {
            Request request = new Request();

            request.requestType = RequestType.Listing;
            request.body = null;
            Response resp = client.Communcate(request);

            if (resp == null)
            {
                return null;
            }

            return (Dir)resp.body;
        }

        /**
         * <summary>
         * Metoda aktualizująca czasy życia w indeksie <c>noDelte</c>.
         * </summary>
         */
        public void SecureFile (String path)
        {
            if (noDelete.Contains(path) == false)
            {
                noDelete.Add(path);
            }

            if (noDelteLife.ContainsKey(path) == false)
            {
                noDelteLife[path] = 0;
            }

            noDelteLife[path] += 10;
        }
    }
}
