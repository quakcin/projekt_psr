using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ArchExplorer
{
    public partial class Form1 : Form
    {
        /**
         * <value>
         * Adres <c>URL</c> do serwera z informacją/stroną o archiwum
         * </value>
         */
        private string archViewPath = "http://127.0.0.1:8888";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadArchViewPath();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        /**
         * <summary>
         * Metoda ładująca plik konfiguracyjny, niestety ze względu na użytą wersję
         * .NET Frameowrk 3.5, nie wspiera ona biblioteki <c>YamlDotNet</c>, więc
         * plik konfiguracyjny musimy sami ręcznie parsować. Mogli byśmy ewentualnie
         * podać te informacje po przez parametry w trakcie uruchamiania aplikacji
         * z poziomu menu konekstowego w Tray'u windows'a
         * </summary>
         */
        private void LoadArchViewPath ()
        {
            if (!File.Exists(".config.yml"))
            {
                MessageBox.Show("Nie znaleziono pliku konfiguracyjnego");
                return;
            }

            // 3.5 nie wspiera naszego parsera yml
            // ale nie trudno jest napisać własny

            string yaml = File.ReadAllText(".config.yml");
            string[] lines = yaml.Split('\n');

            foreach (string line in lines)
            {
                string[] toks = line.Split(':');
                if (toks[0] == "ip")
                {
                    archViewPath = "http://" + toks[1].Trim() + ":8888/";
                    break;
                }
            }


            webBrowser1.Navigate(archViewPath);
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            webBrowser1.Refresh();
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            this.Text = webBrowser1.Url.ToString();
        }
    }
}
