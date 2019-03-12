using Microsoft.Win32;
using System;
using System.Windows;
using System.IO;
using System.Diagnostics;
using System.IO.Compression;

namespace AesPad {
    public partial class Backup : Window {
        public Backup() {
            InitializeComponent();
        }

        private string createdZip = "";

        private string getSetting(string name) {
            return Properties.Settings.Default[name].ToString();
        }

        private void setSetting(string setting, string value) {
            Properties.Settings.Default[setting] = value;
        }

        private void Window_Initialized(object sender, EventArgs e) {
            backupHost.Text = getSetting("backupHost");
            backupPort.Text = getSetting("backupPort");
            backupUser.Text = getSetting("backupUser");
            backupServerPath.Text = getSetting("backupServerPath");
            backupLocalPath.Text = getSetting("backupLocalPath");
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e) {
            setSetting("backupHost", backupHost.Text);
            setSetting("backupPort", backupPort.Text);
            setSetting("backupUser", backupUser.Text);
            setSetting("backupServerPath", backupServerPath.Text);
            setSetting("backupLocalPath", backupLocalPath.Text);
            Properties.Settings.Default.Save();

            output.Text += "Settings saved.\n";
        }

        private void RunBackup_Click(object sender, RoutedEventArgs e) {
            string cmd = "pscp -P " +
                getSetting("backupPort") +
                " -pw " +
                backupPassword.Password +
                " -r " +
                getSetting("backupLocalPath") +
                "*.aspd " +
                getSetting("backupUser") +
                "@" +
                getSetting("backupHost") +
                ":" +
                getSetting("backupServerPath");

            Process process = new Process();
            ProcessStartInfo start = new ProcessStartInfo();
            start.WindowStyle = ProcessWindowStyle.Hidden;
            start.CreateNoWindow = true;
            start.FileName = "cmd.exe";
            start.Arguments = "/C " + cmd;
            start.UseShellExecute = false;
            start.RedirectStandardError = true;
            start.RedirectStandardOutput = true;
            process.StartInfo = start;
            process.Start();
            bool exited = process.WaitForExit((int)Properties.Settings.Default["backupTimeoutMs"]);
            if(exited) {
                output.Text += process.StandardError.ReadToEnd() + "\n";
                output.Text += process.StandardOutput.ReadToEnd() + "\n";
                output.Text += "\nComplete.\n";
            } else {
                process.Kill();
                output.Text += "Process timed out after 10 seconds.\n";
            }
        }

        private void LocalPathSelect_Click(object sender, RoutedEventArgs e) {
            // This is hacked up shit because WPF is a dick.
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.InitialDirectory = backupLocalPath.Text;
            dialog.Title = "Select Folder";
            dialog.Filter = "Folder|*.directory";
            dialog.FileName = "select";
            if(dialog.ShowDialog() == true) {
                string path = dialog.FileName;
                path = path.Replace("select.directory", "");
                if(!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                backupLocalPath.Text = path;
            }
        }
    }
}
