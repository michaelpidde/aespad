using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Security.Cryptography;

namespace AesPad {
    public partial class MainWindow : Window {
        private string fullFilePath = "";
        private bool contentChanged = false;
        private bool isNewFile = true;
        private string title = "AesPad";
        public string sessionPassword = "";

        public MainWindow() {
            InitializeComponent();
        }

        #region Menu Events
        private void close_Click(object sender, RoutedEventArgs e) {
            if(discardCondition())
                this.Close();
        }

        private void mainContent_TextChanged(object sender, TextChangedEventArgs e) {
            if(mainContent.Text.Length != 0)
                contentChanged = true;
            else
                if(isNewFile)
                    contentChanged = false;
                else
                    contentChanged = true;
        }

        private void openFile_Click(object sender, RoutedEventArgs e) {
            if(discardCondition())
                fileOpen();
        }

        private void newFile_Click(object sender, RoutedEventArgs e) {
            if(discardCondition())
                resetEditor();
        }

        private void saveFile_Click(object sender, RoutedEventArgs e) {
            Encryption enc = new Encryption(sessionPassword);
            byte[] cypher = enc.encrypt(mainContent.Text);
            System.IO.File.WriteAllBytes(fullFilePath, cypher);
        }

        private void Window_Initialized(object sender, EventArgs e) {
            showPasswordPrompt();
        }

        private void ResetPassword_Click(object sender, RoutedEventArgs e) {
            showPasswordPrompt();
        }
        #endregion

        #region Helpers
        private void showPasswordPrompt() {
            Application.Current.MainWindow.Hide();
            PasswordPromp pwd = new PasswordPromp();
            pwd.Show();
        }

        private MessageBoxResult showDiscardWarning() {
            return MessageBox.Show("Discard unsaved changes?", "Unsaved Changes",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning,
                MessageBoxResult.Cancel,
                MessageBoxOptions.DefaultDesktopOnly);
        }

        private bool discardCondition() {
            if((contentChanged && showDiscardWarning() == MessageBoxResult.OK) || !contentChanged)
                return true;
            else
                return false;
        }

        private void resetEditor() {
            fullFilePath = "";
            isNewFile = true;
            mainContent.Text = "";
            Title = title;
            contentChanged = false;
        }

        private void fileOpen() {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = "C:\\";
            dialog.Filter = "AesPad files (*.apd)|*.apd";
            dialog.CheckFileExists = true;
            dialog.Multiselect = false;
            bool? result = dialog.ShowDialog();

            if(result == true) {
                fullFilePath = dialog.FileName;
                bool loaded = false;
                Encryption enc = new Encryption(sessionPassword);
                string content = "";
                try {
                    content = enc.decrypt(System.IO.File.ReadAllBytes(fullFilePath));
                    loaded = true;
                } catch(CryptographicException e) {
                    MessageBox.Show(
                        "Error decrypting file. The session password may be incorrect.", 
                        "Error decrypting file.", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Error);
                }
                if(loaded) {
                    isNewFile = false;
                    mainContent.Text = content;
                    Title = title + " - " + fullFilePath;
                    contentChanged = false;
                    saveFile.IsEnabled = true;
                } else {
                    fullFilePath = "";
                }
            }
        }
        #endregion
    }
}
