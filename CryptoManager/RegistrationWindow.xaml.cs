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

using System.Security.Cryptography;

using MahApps.Metro.Controls;

namespace CryptoManager
{
    /// <summary>
    /// Interaction logic for RegistrationWindow.xaml
    /// </summary>
    public partial class RegistrationWindow : MetroWindow
    {
        public RegistrationWindow()
        {
            InitializeComponent();
            registerBtn.IsEnabled = false;
        }

        private void regPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ratingsBar.SetPassword(regPasswordBox.Password);
        }

        private void confirmRegPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (ratingsBar.Complexity >= PasswordMeter.GOOD && 
                confirmRegPasswordBox.Password == regPasswordBox.Password &&
                String.IsNullOrEmpty(userName.Text) == false)
            {
                registerBtn.IsEnabled = true;
            }

            else
            {
                registerBtn.IsEnabled = false;
            }
        }

        private void registerBtn_Click(object sender, RoutedEventArgs e)
        {
            //Encrypt the password, using email as salt
            string myHashedPassword = CalculateHash(confirmRegPasswordBox.Password, userName.Text);

            //Create a new User and save to the DB Context
            CryptoDataClasses1DataContext cdb1 = new CryptoDataClasses1DataContext();

            User newUser = new User()
            {
                username = userName.Text,
                email = userName.Text,
                hashedPassword = myHashedPassword,
                role = "Administrators"
            };

            cdb1.Users.InsertOnSubmit(newUser);

            try
            {
                cdb1.SubmitChanges();
            }

            catch (System.Exception ex)
            {
                Console.WriteLine(ex);
            }

            cdb1.Dispose();
            
        }

        private string CalculateHash(string clearTextPassword, string salt)
        {
            // Convert the salted password to a byte array
            byte[] saltedHashBytes = Encoding.UTF8.GetBytes(clearTextPassword + salt);
            // Use the hash algorithm to calculate the hash
            HashAlgorithm algorithm = new SHA256Managed();
            byte[] hash = algorithm.ComputeHash(saltedHashBytes);
            // Return the hash as a base64 encoded string to be compared to the stored password
            return Convert.ToBase64String(hash);
        }

    }
}
