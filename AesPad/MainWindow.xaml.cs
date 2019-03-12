using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Security.Cryptography;
using System.IO;
using System.ComponentModel;
using System.Windows.Input;

namespace AesPad {
    public partial class MainWindow : Window, INotifyPropertyChanged {
        private string fullFilePath = "";
        private bool contentChanged = false;
        private bool isNewFile = true;
        private string title = "AesPad";
        private string filter = "AesPad files (*aspd)|*.aspd";
        private string hint = "Tip: Hit Alt+B to toggle text window blur for ultra paranoia mode!";

        // Bound property in XAML
        public bool blurContent { get; set; }
        public string statusText { get; set; }
        public event PropertyChangedEventHandler PropertyChanged = delegate {};

        // These public variables are accessed by the PasswordPrompt form.
        public string sessionPassword = "";
        public bool sessionPasswordChanged = false;
        public bool loadedFromAssociation = false;

        public MainWindow() {
            DataContext = this;
            blurContent = false;
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
            MessageBoxResult saveAnyway = MessageBoxResult.OK;

            if(sessionPasswordChanged)
                saveAnyway = showPasswordChangedWarning();

            if(saveAnyway == MessageBoxResult.OK) {
                fileSave();
                sessionPasswordChanged = false;
            }
        }

        private void Window_Initialized(object sender, EventArgs e) {
            if(Environment.GetCommandLineArgs().Length > 1) {
                fullFilePath = Environment.GetCommandLineArgs()[1];
                loadedFromAssociation = true;
            } else {
                mainContent.Text = hint;
            }

            showPasswordPrompt();
        }

        private void ResetPassword_Click(object sender, RoutedEventArgs e) {
            showPasswordPrompt();
            sessionPasswordChanged = true;
        }

        private void SaveFileAs_Click(object sender, RoutedEventArgs e) {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.InitialDirectory = "C:\\";
            dialog.Filter = filter;
            dialog.CreatePrompt = true;
            bool? result = dialog.ShowDialog();

            if(result == true) {
                fullFilePath = dialog.FileName;
                fileSave();
                Title = title + " - " + fullFilePath;
                saveFile.IsEnabled = true;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e) {
            MessageBox.Show("AesPad\n\u00a9 2019 Full Ration Productions LLC. All Rights Reserved.\n\n" +
                "This software is provided under no warranty expressed or implied.", "About " + title, 
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void Blur_Click(object sender, RoutedEventArgs e) {
            blurContent = !blurContent;
            OnPropertyChanged("blurContent");
        }
        private void BlurCommand_Executed(object sender, ExecutedRoutedEventArgs e) {
            Blur_Click(sender, e);
        }

        private void InsertTime_Click(object sender, RoutedEventArgs e) {
            string insert = systemDateTime();
            int index = mainContent.SelectionStart;
            mainContent.Text = mainContent.Text.Insert(index, insert);
            mainContent.SelectionStart = index + insert.Length;
        }
        private void TimeCommand_Executed(object sender, ExecutedRoutedEventArgs e) {
            InsertTime_Click(sender, e);
        }
        #endregion

        #region Helpers
        private void OnPropertyChanged(string property) {
            PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        private string systemTime() {
            return DateTime.Now.ToString("HH:mm:ss");
        }

        private string systemDateTime() {
            return DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
        }

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

        private MessageBoxResult showPasswordChangedWarning() {
            return MessageBox.Show("Session password has been changed. Save file with new encryption password?", 
                "Reset Encryption Password",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning,
                MessageBoxResult.Cancel,
                MessageBoxOptions.DefaultDesktopOnly);
        }

        private bool discardCondition() {
            bool discard = mainContent.Text == hint || 
                (contentChanged && showDiscardWarning() == MessageBoxResult.OK) ||
                !contentChanged;

            if(discard)
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
            sessionPasswordChanged = false;
            saveFile.IsEnabled = false;
        }

        private void fileSave() {
            Encryption enc = new Encryption(sessionPassword);
            byte[] cypher = enc.encrypt(mainContent.Text);
            File.WriteAllBytes(fullFilePath, cypher);
            contentChanged = false;
            isNewFile = false;
            statusText = "Saved at " + systemTime();
            OnPropertyChanged("statusText");
        }

        private void fileOpen() {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = "C:\\";
            dialog.Filter = filter;
            dialog.CheckFileExists = true;
            dialog.Multiselect = false;
            bool? result = dialog.ShowDialog();

            if(result == true) {
                fullFilePath = dialog.FileName;
                loadAndDecrypt();
            }
        }

        public void loadAndDecrypt() {
            bool loaded = false;
            Encryption enc = new Encryption(sessionPassword);
            string content = "";
            try {
                content = enc.decrypt(File.ReadAllBytes(fullFilePath));
                loaded = true;
            } catch(CryptographicException) {
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
        #endregion
    }
}
