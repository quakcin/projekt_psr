using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using System.Threading;
using System.Management.Instrumentation;

namespace CloudClient
{
    /**
     * <summary>
     * Obiekt wymagany do załadowania danyh z pliku konfiguracyjnego
     * po przez bibliotekę <c>YamlDotNet</c>.
     * </summary>
     */
    class ConfigFile
    {
        public string mount { get; set; }
        public string ip { get; set; }
        public int port { get; set; }
    }

    /**
     * <summary>
     * Klasa konfiguracyjna zawierająca wszystkie niezbędne informacje o aplikacji
     * klienckiej do jej prawidłowego funkcjonowania.
     * </summary>
     */
    internal class Config
    {
        /**
         * <value>
         * Informacja czy aplikacja dopiero wystartowała i jeszcze nie dokonała
         * synchronizacji z serwerem.
         * </value>
         */
        private bool isStarting;

        /**
         * <value>
         * Ścieżka do lokalnej instancji katalogu sieciowego/cloud'a
         * </value>
         */
        private string cloudLocalPath;

        /**
         * <value>
         * Adres IP serwera dysku w chmurze
         * </value>
         */
        private string serverIp;

        /**
         * <value>
         * Port serwera
         * </value>
         */
        private int serverPort;

        /**
         * <summary>
         * Konstruktor powołujący metodę do odycztania pliku konfigracyjnego
         * </summary>
         */
        public Config()
        {
            isStarting = true;
            ReadConfigFile();
        }

        /**
         * <summary>
         * Metoda odczytująca konfigurację z pliku .config.yml
         * </summary>
         */
        private void ReadConfigFile()
        {
            var deserializer = new DeserializerBuilder()
              .WithNamingConvention(CamelCaseNamingConvention.Instance)
              .Build();
            string yamlStr = File.ReadAllText(".config.yml");
            ConfigFile config = deserializer.Deserialize<ConfigFile>(yamlStr);
            cloudLocalPath = config.mount;
            serverIp = config.ip;
            serverPort = config.port;
            Console.WriteLine("Mount point: " + cloudLocalPath);
        }


        /**
         * <summary>
         * Metoda wywoływana po pierwszej synchronizacjia po uruchomieniu aplikacji
         * </summary>
         */
        public void EndOfMainLoop()
        {
            isStarting = false;
        }

        /**
         * <summary>
         * Metoda wywoływana przed pierwszą synchornizacją po uruchomienu.
         * </summary>
         */
        public bool IsStarting()
        {
            return isStarting;
        }

        /**
         * <summary>
         * Metoda zwracająca lokalną ścieżkę do folderu z dyskiem cloud'a
         * </summary>
         */
        public string GetCloudLocalPath()
        {
            return cloudLocalPath;
        }

        /**
         * <summary>
         * Metoda zwracająca adres IP serwera z konfiguracji
         * </summary>
         */
        public string GetServerIP()
        {
            return serverIp;
        }

        /**
         * <summary>
         * Metoda zwracająca adres port serwera z konfiguracji
         * </summary>
         */
        public int GetServerPort ()
        {
            return serverPort;
        }

    }
}
