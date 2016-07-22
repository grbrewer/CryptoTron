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
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;

using System.Security.Permissions;
using CryptoManager.OceanReference1;
using CryptoCore;
using System.Xml;

namespace CryptoManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [PrincipalPermission(SecurityAction.Demand)]
    public partial class MainWindow : MetroWindow, IView
    {
        public MainWindow(string userName)
        {
            InitializeComponent();
        }

        #region IView Members

        public IViewModel ViewModel
        {
            get
            {
                return DataContext as IViewModel;
            }
            set
            {
                DataContext = value;
            }
        }

        #endregion

        private void genKeyPair_Click(object sender, RoutedEventArgs e)
        {
            string pubKeyText = CryptoEngine.GenerateKeyPair("c:\\temp\\temp.gef");

            //TODO: Replace lora@encom.com with current user email identity..
            OceanInterfaceClient oc = new OceanInterfaceClient();
            string newSerialNumber = oc.publishPublicKey("lora@encom.com", pubKeyText);

            pubkey publicKey = new pubkey()
            {
                email = "lora@encom.com",
                publicKey = pubKeyText,
                serialNumber = newSerialNumber
            };

            CryptoDataClasses1DataContext cdb = new CryptoDataClasses1DataContext();

            cdb.pubkeys.InsertOnSubmit(publicKey);

            try
            {
                cdb.SubmitChanges();
            }

            catch (System.Exception error)
            {
                Console.WriteLine(error);
            }

            cdb.Dispose();

        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CryptoDataClasses1DataContext cdb = new CryptoDataClasses1DataContext();
            keyGrid.ItemsSource = cdb.pubkeys;
            userRolesGrid.ItemsSource = cdb.Users;
            mainKeyArea.DataContext = cdb.pubkeys;
            
        }

    }
}
