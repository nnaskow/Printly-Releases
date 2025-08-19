using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Printly
{
    /// <summary>
    /// Interaction logic for DeveloperLogin.xaml
    /// </summary>
    public partial class DeveloperLogin : Window
    {
        public DeveloperLogin()
        {
            InitializeComponent();
        }
        string developerUsername = "dev00";
        string developerPass = "dev001!@";
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Password;

            if (username == developerUsername && password == developerPass)
            {
                ((DbManagementWindow)Owner).developerMode = true;
                MessageBox.Show($"Успешен вход! Добре дошли, {developerUsername}!", "Добре дошли!");
                this.Close(); 
            }
            else
            {
                MessageBox.Show("Грешни данни за вход.", "ГРЕШКА");
            }
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key==Key.Space) LoginButton_Click(sender, e);
        }
    }
}
