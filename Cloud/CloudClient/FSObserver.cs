using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.IO;
using Cloud;
using Cloud.Requests;

using SFile = System.IO.File;
using System.Threading;
using System.Runtime.CompilerServices;
using Google.Protobuf.Collections;

namespace CloudClient
{

    /**
     * <summary>
     * Klasa odpowiedzialna za obserwacje systemu plikow i moniotorwanie 
     * zmian i wysylanie zmian do clouda. Zmiany wpisywane są w kolejkę
     * <c>requestPipe</c>, i wykonywane są iteracyjnie. Klasa ekstensywnie
     * wykorzystuje systemowe API <c>FileSystemWatcher</c> do obserwoawania
     * zmian w systemie plików w katalogu dyskowym na kliencie.
     * </summary>
     */
    internal class FSObserver
    {
        /**
         * <value>
         * Obiekt klienta z wsyzsktimi ważnymi obiektami.
         * </value>
         */
        private Client client;

        /**
         * <value>
         * Kolejka żądań do serwera zbudowana w opraciu o prostą listę.
         * </value>
         */
        private List<Request> requestPipe = new List<Request>();

        /**
         * Konstrukotr klasy powołuje nową instancję API <c>FileSystemWatcher</c>,
         * konfuguruję ją do obserwowania folderu dysku klienta oraz przypina
         * metody odpowiedzialne za obsłógę różnych wydarzeń w systemie plików.
         */
        public FSObserver (Client client)
        {
            this.client = client;

            var watcher = new FileSystemWatcher(client.GetConfig().GetCloudLocalPath());
            watcher.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;


            watcher.Changed += (sender, e) => OnChanged(sender, e, this);
            watcher.Created += (sender, e) => OnCreated(sender, e, this);
            watcher.Deleted += (sender, e) => OnDeleted(sender, e, this);
            watcher.Renamed += (sender, e) => OnRenamed(sender, e, this);

            watcher.Filter = "*.*";
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;
        }

        /**
         * <summary>
         * API Windoswa wywoluje 3000x eventów podczas uploadowania większego pliku
         * event na każdy chunk pliku, ale tutaj uploadujemy już cały plik, niestety
         * w kolejce mamy 10x requestów o upload tego samego pliku, nie ma sensu tego
         * robić jeżeli jego rozmiar się nie zmienił
         * </summary>
         */
        private Dictionary<String, long> fileUploadEntry = new Dictionary<string, long>();

        /**
         * <summary>
         * Funkcja wykonująca kolejne żądanie w kolejce <c>requestPipe</c>, jeżeli
         * kolejka jest pusta, to nic nie robi.
         * </summary>
         */
        public void AdvanceRequestPipe ()
        {
            if (requestPipe.Count == 0)
            {
                return;
            }

            Request request = requestPipe[0];
            requestPipe.RemoveAt(0);

            /**
             * Wczytujemy zawartosc pliku przed wyslaniem
             */
            if (request.requestType == RequestType.UploadFile)
            {
                UploadFileRequest uploadFileRequest = (UploadFileRequest)request.body;

                String path = uploadFileRequest.name;
                WaitForAccess(path);
                uploadFileRequest.name = path.Replace(client.GetConfig().GetCloudLocalPath(), "");
                FileInfo fileInfo = new FileInfo(path);

                if (fileUploadEntry.ContainsKey(uploadFileRequest.name) && fileUploadEntry[uploadFileRequest.name] == fileInfo.Length)
                {
                    // nie ma sensu 2x wysyłać tego samego pliku
                    Console.WriteLine("Skipping duplicate upload for: " + path);
                    return;
                }

                byte[] bytes = SFile.ReadAllBytes(path);
                uploadFileRequest.content = bytes;
                request.body = uploadFileRequest;
                Console.WriteLine("UPLOADING: " + uploadFileRequest.name + " with " + bytes.Length.ToString());

                if (!fileUploadEntry.ContainsKey(uploadFileRequest.name))
                {
                    fileUploadEntry.Add(uploadFileRequest.name, bytes.Length);
                }
                else
                {
                    fileUploadEntry[uploadFileRequest.name] = bytes.Length;
                }
            }

            Response response = client.Communcate(request);
            // TODO: Handle response somehow

        }

        /**
         * <summary>
         * Metoda wywoływana w przypadku gdy w systemie plików dojdzie do jakieś zmiany.
         * Zazwyczaj jest to aktualzacja stanu pliku, niestety jest to też szum generowany
         * przez losowe eventy np. w trakcie przeglądania katalogów.
         * 
         * Takowe zmiany plików dodajemy do kolejki żądań jako wysłanie pliku na serwer.
         * </summary>
         */
        private static void OnChanged(object sender, FileSystemEventArgs e, FSObserver fsObserver)
        {

            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }

            Console.WriteLine("CHANED TRIGGERED ON : " + e.FullPath);

            /**
             * Zmiany plików wywołują zmiany folderów,
             * nie musimy sie tym martwić
             */
            if (Directory.Exists(e.FullPath))
            {
                return;
            }

            QueFileUpload(fsObserver, e.FullPath);
        }

        /**
         * <summary>
         * Funkcja dodająca żądanie o wysłanie pliku na serwer do kolejki.
         * Początkowo kolejkowała pliki z zawartością, ale w przypadku
         * dużej ilośći plików pamieć szybko się kończyła, więc został 
         * dorobiony mechanizm pobierania zawartości pliku tylko w tedy
         * kiedy jest on już wysyłany iteracyjnie, nie na etapie dodania
         * do kolejki.
         * </summary>
         */
        private static void QueFileUpload (FSObserver fsObserver, String path)
        {
            fsObserver.client.GetSyncro().SecureFile(path);

            Request request = new Request();
            request.requestType = RequestType.UploadFile;

            UploadFileRequest uploadFileRequest = new UploadFileRequest();
            request.body = uploadFileRequest;
            uploadFileRequest.name = path;
            fsObserver.requestPipe.Add(request);
        }

        /**
         * <summary>
         * Metoda wywoływana w przypadku gdy w systemie plików dojdzie do utworszenia czegoś.
         * Np. nowego pliku lub folder. W przypadku folderu wysyłamy żądanie o stowrzenie
         * folderu a w przypadku pliku żadanie o wysłanie pliku na serwer.
         * </summary>
         */
        private static void OnCreated(object sender, FileSystemEventArgs e, FSObserver fsObserver)
        {

            /**
             * Jeżeli został utworzony plik to go wysyłamy,
             * jeżeli folder to inny pakiet
             */
            Console.WriteLine("     [] CREATED\n");
            if (!(SFile.Exists(e.FullPath) || Directory.Exists(e.FullPath)))
            {
                return;
            }

            if (SFile.GetAttributes(e.FullPath).HasFlag(FileAttributes.Directory) == false)
            {
                Console.WriteLine("     [] NO DIR \n");
                QueFileUpload(fsObserver, e.FullPath);
                return;
            }

            string localPath = e.FullPath.Replace(fsObserver.client.GetConfig().GetCloudLocalPath(), "");
            fsObserver.client.GetSyncro().SecureFile(localPath);
            Request request = new Request();
            request.requestType = RequestType.MkDir;
            MkDirRequest mkDirRequest = new MkDirRequest();
            mkDirRequest.name = localPath;
            request.body = mkDirRequest;
            fsObserver.requestPipe.Add(request);
        }

        /**
         * <summary>
         * Metoda wywoływana w przypadku gdy w systemie plików dojdzie do usunięcia czego,
         * np. pliku lub folderu. W jednym i drugim przypadku wysyłamy żądanie o usunięcie
         * pliku -- gdyż załatwia to też usunięcie folderu. TOOD: może osobne żądanie do
         * usunięcia folder, albo refaktor nazw na ogólne żądanie o usunięcie zawartości
         * </summary>
         */
        private static void OnDeleted(object sender, FileSystemEventArgs e, FSObserver fsObserver)
        {
            Console.WriteLine($"Deleted: {e.FullPath}");

            fsObserver.client.GetSyncro().noCreate.Add(e.FullPath);

            Request request = new Request();
            request.requestType = RequestType.DelFile;
            DelFileRequest delFileRequest = new DelFileRequest();
            delFileRequest.name = e.FullPath.Replace(fsObserver.client.GetConfig().GetCloudLocalPath(), "");

            request.body = delFileRequest;
            fsObserver.requestPipe.Add(request);
        }

        /**
         * <summary>
         * Metoda wywoływana w przypadku gdy w systemie plików dojdzie do zmiany nazwy czegoś,
         * np. pliku lub folder. W obu przypadkiach wysylane jest to samo żądanie o zmianę
         * nazwy do serwera.
         * </summary>
         */
        private static void OnRenamed(object sender, RenamedEventArgs e, FSObserver fsObserver)
        {
            Console.WriteLine($"Renamed:");
            Console.WriteLine($"    Old: {e.OldFullPath}");
            Console.WriteLine($"    New: {e.FullPath}");

            // fsObserver.locker.EventsOmmitFile(e.FullPath);

            if (!(SFile.Exists(e.FullPath) || Directory.Exists(e.FullPath)))
            {
                return;
            }

            if (SFile.GetAttributes(e.FullPath).HasFlag(FileAttributes.Directory))
            {
                fsObserver.client.GetSyncro().SecureFile(e.FullPath.Replace(fsObserver.client.GetConfig().GetCloudLocalPath(), ""));
            }


            Request request = new Request();
            request.requestType = RequestType.RenameFile;
            RenameFileRequest renameFileRequest = new RenameFileRequest();
            renameFileRequest.oldName = e.OldFullPath.Replace(fsObserver.client.GetConfig().GetCloudLocalPath(), "");
            renameFileRequest.newName = e.FullPath.Replace(fsObserver.client.GetConfig().GetCloudLocalPath(), "");

            request.body = renameFileRequest;
            fsObserver.requestPipe.Add(request);
        }


        /**
         * <summary>
         * Metoda czekająca blokowo na dostep do zawartości w podanje ścieżce <c>filePath</c>.
         * </summary>
         */
        public static bool WaitForAccess (string filePath)
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
