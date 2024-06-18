using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cloud
{
    /**
     * <summary>
     * Klasa odpowiedzialna za utrzymywanie archiwum. Rejestruje ona zamiany
     * zarówno w <c>Bazie Danych</c> jaki i w fizycznych <c>slabach</c> na 
     * dysku w folderze archiwum.
     * 
     * Zaprojektowana jest w oparciu o wzorzec projektowy <c>Singleton</c>.
     * Jej instancja powoływana jest przy użyciu metody <c>GetInstance()</c>
     * </summary>
     */
    internal class Archive
    {
        /**
         * <value>
         * Instancja singletonu.
         * </value>
         */
        static Archive instance = null;

        /**
         * <summary>
         * Metoda zapisującą zmianę stanu archiwum w <c>Bazie Danych</c>
         * <param name="slabId">Unikalny identyfikator slabu na dysku</param>
         * <param name="ip">Adres ip klienta którego zapytanie wywołało zmianę</param>
         * <param name="operation">Nazwa operacji do jakiej doszło</param>
         * </summary>
         */
        public void LogChange (String slabId, String ip, String operation)
        {

            ConfigDatabse db = Config.getInstance().db;

            string connectionString = "server=" + db.Host + ";user=" + db.Login + ";database=" + db.Login + ";port=" + db.Port + ";password=" + db.Password;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    String sql = "INSERT INTO archive VALUES (@slab, DEFAULT, @operation, @ip)";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@slab", slabId);
                        cmd.Parameters.AddWithValue("@operation", operation);
                        cmd.Parameters.AddWithValue("@ip", ip);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        // Console.WriteLine($"Rows affected: {rowsAffected}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        /**
         * <summary>
         * Metoda służąca do otrzymania instancji klasy.
         * </summary>
         */
        public static Archive GetInstance ()
        {
            if (instance == null)
            {
                instance = new Archive();
            }
            return instance;
        }
        

        /**
         * <summary>
         * Metoda tworząca katalog <c>Slabu</c> na dysku w fodlerze z archiwum.
         * Zwraca UUID <c>slabu</c> w formie ciągu zanków.
         * 
         * Tworzy ona również podkatalogi potrzebne do przechowania pliku/folderu
         * ze ścierzki dyskowje <c>effectivePath</c>.
         * </summary>
         */
        private string AllocTimeSlab (String effectivePath)
        {
            string slabId;

            /** Szanse na kolizje są niskie, ale nie zerowe */
            do
            {
                slabId = Guid.NewGuid().ToString();
            }
            while (Directory.Exists(Config.getInstance().GetArchiveDir() + "\\" + slabId));

            Directory.CreateDirectory(Config.getInstance().GetArchiveDir() + "\\" + slabId);

            return slabId;
        }

        /**
         * <summary>
         * Jeśli slab zawiera zagnieżdzone ścieżki, to metoda ta je utworzy.
         * <param name="effectivePath">Ścieżka relatywna do dyskowej</param>
         * <param name="slabId">identyfiaktor UUID slabu</param>
         * </summary>
         */
        private void AllocDirSpaceInSlab (String effectivePath, String slabId)
        {
            String localPath = effectivePath.Replace(Config.getInstance().GetServerStorageDir(), "");

            String[] toks = localPath.Split('\\');
            String path = Config.getInstance().GetArchiveDir() + "\\" + slabId + "\\";

            for (int i = 1; i < toks.Length; i++)
            {
                if (Directory.Exists(path) == false)
                {
                    Directory.CreateDirectory(path);
                }
                path += toks[i] + "\\";
            }

        }

        /**
         * <summary>
         * Zamienia ścieżkę efektywną z dysku na ścieżkę do slabu.
         * <example>
         * C:\storage\folder\file.txt na C:\archive\UUID\folder\file.txt
         * </example>
         * </summary>
         */
        private string ConvertEffectiveToSlab (String effective, String slabId)
        {
            /**
             * Konwertujemy ścierzke efektywną na slabową
             */
            String localPath = effective.Replace(Config.getInstance().GetServerStorageDir(), "");
            String slabPath = Config.getInstance().GetArchiveDir() + "\\" + slabId + localPath;
            return slabPath;
        }


        /**
         * <summary>
         * Metoda wywoływana jeżeli dojedzie do jakiej kolwiek zmiany w 
         * którymś z plików na dysku (w folderze dysku).
         * </summary>
         */
        public void UpdateFile (String path, string clientIp)
        {
            String slabId = AllocTimeSlab(path);
            String slabPath = ConvertEffectiveToSlab(path, slabId);
            AllocDirSpaceInSlab(path, slabId);
            File.Move(path, slabPath);
            LogChange(slabId, clientIp, "UPDATE");
        }

        /**
         * <summary>
         * Metoda wywoływana w przypadku usunięcia jakiegoś pliku
         * z folderu dyskowego.
         * </summary>
         */
        public void DeleteFile (String path, string clientIp)
        {
            String slabId = AllocTimeSlab(path);
            String slabPath = ConvertEffectiveToSlab(path, slabId);
            AllocDirSpaceInSlab(path, slabId);
            File.Move(path, slabPath);
            LogChange(slabId, clientIp, "DELETE");
        }

        /**
         * <summary>
         * Metoda wywoływana w przypadku usunięcia całego folderu
         * z folderu dyskowego.
         * </summary>
         */
        public void DeleteDir (String path, string clientIp)
        {
            String slabId = AllocTimeSlab(path);
            String slabPath = ConvertEffectiveToSlab(path, slabId);
            AllocDirSpaceInSlab(path, slabId);

            Directory.Move(path, slabPath);
            LogChange(slabId, clientIp, "DIR_DELETE");
        }

        /**
         * <summary>
         * Metoda wywoływana w przypadku zmiany nazwy jakiegoś pliku
         * w folderze dyskowym.
         * </summary>
         */
        public void RenameFile (String path, string clientIp)
        {
            String slabId = AllocTimeSlab(path);
            String slabPath = ConvertEffectiveToSlab(path, slabId);
            AllocDirSpaceInSlab(path, slabId);

            File.Copy(path, slabPath);
            LogChange(slabId, clientIp, "RENAME");
        }
        /**
         * <summary>
         * Metoda wywoływana w przypadku zmiany nazwy jakiegoś folderu
         * w folderze dyskowym.
         * </summary>
         */
        public void RenameDir (String path, string clientIp)
        {
            String slabId = AllocTimeSlab(path);
            String slabPath = ConvertEffectiveToSlab(path, slabId);
            AllocDirSpaceInSlab(path, slabId);
            LogChange(slabId, clientIp, "DIR_RENAME");
        }
    }
}
