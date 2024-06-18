using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

namespace Cloud
{
    /**
     * <summary>
     * Klasa pomocznicza do odczytania pliku konfiguracyjnego, w pliku koniguracyjnym
     * wyróżniamy dwie główne grupy konfiguracyjne: <c>ConfigStorage</c> oraz <c>ConfigDatabase</c>
     * </summary>
     */
    class ConfigFile
    {
        public ConfigStorage Storage { get; set; }
        public ConfigDatabse Db { get; set; }
    }

    /**
     * <summary>
     * Klasa pomocznicza do odczytania pliku konfiguracyjnego, a dokładnie
     * grupy <c>storage</c> - czli informacji o konfiguracji w systemie plików.
     * </summary>
     */
    class ConfigStorage
    {
        public string Path { get; set; }
        public string Archive { get; set; }
    }

    /**
     * <summary>
     * Klasa pomocznicza do odczytania pliku konfiguracyjnego, a dokładnie
     * grupy <c>db</c> - czyli informacji o połączeniu z bazą danych.
     * </summary>
     */
    class ConfigDatabse
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
    }


    /**
     * <summary>
     * Klasa zawiera wszelkie informacje związane z konfiguracą serwera,
     * zbudowana jest w operaciu o wzorzec projektowy <c>Singleton</c>,
     * aby dostać jej instancję należy wywołać statyczną metodę <c>getInstnace()</c>
     * Nie należy powoływąc jej przy pomocy konstruktora.
     * </summary>
     */
    class Config
    {
        /**
         * <value>
         * Jedyna dozwolona instancja klasy, otrzymywana i tworzona przez
         * metodę <c>getInstance()</c>
         * </value>
         */
        static Config instance = null;

        /**
         * <value>
         * Pełna lokalizacja folderu w którym będzie prechowywany aktualny stan
         * dysku w chmurze. 
         * </value>
         */
        private string serverStorage = @"";

        /**
         * <value>
         * Pełna lokalizacja folderu w którym będą przechowywane <c>slaby</c>
         * archiwum - tj. pojedyńcze archiwalne stany dystku.
         * </value>
         */
        private string archiveStorage = @"";

        /**
         * <value>
         * Informację o konfiguracji dostępu do bazy danych
         * </value>
         */
        public ConfigDatabse db;

        /**
         * <summary>
         * Konstruktor powinien być wywoływany tylko i wyłącznie po przez
         * statyczną metodę <c>getInstance</c>.
         * 
         * Korzystajac z biblioteki <c>YamlDotNet</c> odczytuje plik
         * konfiguracyjny i ustawia odpowiednie wartości w polach obiektu.
         * 
         * Plik konfiguracyjny jest czytany z obecnej ścieżki środowiskowej
         * z pliku o nazwie <c>.cloud.yml</c>
         * </summary>
         */
        public Config ()
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            string yamlStr = File.ReadAllText(".cloud.yml");
            ConfigFile config = deserializer.Deserialize<ConfigFile>(yamlStr);

            this.serverStorage = config.Storage.Path;
            this.archiveStorage = config.Storage.Archive;
            this.db = config.Db;
        }

        /**
         * <summary>
         * Metoda zwracjąca ścieżkę do folder z aktualnym stanem dysku
         * </summary>
         */
        public string GetServerStorageDir ()
        {
            return serverStorage;
        }

        /**
         * <summary>
         * Metoda zwracjąca ścieżkę do folder z <c>slabami</c> archiwum.
         * </summary>
         */
        public string GetArchiveDir ()
        {
            return archiveStorage;
        }

        /**
         * <summary>
         * Metoda zwracająca instancję singletona klasy <c>Config</c>
         * </summary>
         */
        public static Config getInstance ()
        {
            if (Config.instance == null)
            {
                Config.instance = new Config();
            }
            return Config.instance;
        }

    }
}
