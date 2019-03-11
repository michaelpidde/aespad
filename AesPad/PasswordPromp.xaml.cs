using System.Windows;

namespace AesPad {
    public partial class PasswordPromp : Window {
        public PasswordPromp() {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            password.Focus();
        }

        private void enter_Click(object sender, RoutedEventArgs e) {
            ((MainWindow)Application.Current.MainWindow).sessionPassword = password.Password;
            if(((MainWindow)Application.Current.MainWindow).loadedFromAssociation)
                ((MainWindow)Application.Current.MainWindow).loadAndDecrypt();
            ((MainWindow)Application.Current.MainWindow).Show();
            this.Close();
        }
    }
}
