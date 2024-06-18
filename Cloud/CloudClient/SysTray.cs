using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CloudClient
{
    internal class SysTray
    {
        private Client client;

        /**
         * <summary>
         * Metoda dodająca ikonkę dysku oraz menu kontekstowe w tray'u
         * systemu windows. Dodane opcje to: "Otwarcie Dysku",
         * "Otwarcie Archiwum", "Wyłączenie programu klienta".
         * </summary>
         */
        private void SysTrayThread ()
        {
            NotifyIcon notifyIcon = new NotifyIcon();
            notifyIcon.Text = "Super Cloud";
            notifyIcon.Icon = new Icon("cloudy.ico");
            notifyIcon.Visible = true;

            notifyIcon.Visible = true;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItemMount = new MenuItem("Otwórz Dysk");
            MenuItem menuItemArch = new MenuItem("Otwórz Archiwum");
            MenuItem menuItemExit = new MenuItem("Wyjdź");


            menuItemExit.Click += (sender, e) =>
            {
                notifyIcon.Visible = false;
                this.client.KillSelf();
                Application.Exit();
            };

            menuItemMount.Click += (sender, e) =>
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = this.client.GetConfig().GetCloudLocalPath(),
                    UseShellExecute = true,
                    Verb = "open"
                });
            };

            menuItemArch.Click += (sender, e) =>
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "ArchExplorer.exe",
                    UseShellExecute = true,
                    Verb = "open"
                });
            };

            contextMenu.MenuItems.Add(menuItemMount);
            contextMenu.MenuItems.Add(menuItemArch);
            contextMenu.MenuItems.Add(menuItemExit);
            notifyIcon.ContextMenu = contextMenu;
            Application.Run();
        }

        public SysTray (Client client)
        {
            this.client = client;
            Task tray = new Task(() =>
            {
                SysTrayThread();
            });
            tray.Start();
        }

    }
}
