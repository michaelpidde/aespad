using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

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
            enc.encrypt(mainContent.Text);
        }

        private void Window_Initialized(object sender, EventArgs e) {
            this.Hide();
            PasswordPromp pwd = new PasswordPromp();
            pwd.Show();
        }
        #endregion

        #region Helpers
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
                isNewFile = false;
                mainContent.Text = System.IO.File.ReadAllText(fullFilePath);
                Title = title + " - " + fullFilePath;
                contentChanged = false;
                saveFile.IsEnabled = true;
            }
        }
        #endregion
    }
}
