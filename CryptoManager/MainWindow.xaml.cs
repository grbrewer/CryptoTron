using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private string m_userName;
        private bool m_adminUser;
        public ObservableCollection<string> adminTypes { get; set; }
        CryptoDataClasses1DataContext m_cdb = new CryptoDataClasses1DataContext();

        public MainWindow(string userName, bool adminUser)
        {
            m_userName = userName;
            m_adminUser = adminUser;
            adminTypes = new ObservableCollection<string> { "standard", "Administrators" };

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

            OceanInterfaceClient oc = new OceanInterfaceClient();
            string newSerialNumber = oc.publishPublicKey(m_userName, pubKeyText);

            pubkey publicKey = new pubkey()
            {
                email = m_userName,
                publicKey = pubKeyText,
                serialNumber = newSerialNumber
            };

            m_cdb.pubkeys.InsertOnSubmit(publicKey);

            try
            {
                m_cdb.SubmitChanges();
            }

            catch (System.Exception error)
            {
                Console.WriteLine(error);
            }

        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            keyGrid.ItemsSource = m_cdb.pubkeys;

            //Only display the userRolesGrid if the current user is Administrator
            if (m_adminUser == false) {
                userRolesGrid.IsEnabled = false;
                userRolesGrid.Visibility = Visibility.Collapsed;
            }
            
            userRolesGrid.ItemsSource = m_cdb.Users;

            adminRole.ItemsSource = adminTypes;
            mainKeyArea.DataContext = m_cdb.pubkeys;
            
        }
      
        private void MetroWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            m_cdb.SubmitChanges();
            m_cdb.Dispose();
        }

        private void userRolesGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            var currentCellContent = e.EditingElement as ComboBox;

            DataGridRow currentRow = e.Row;

            

            
        }

        
    }
}
