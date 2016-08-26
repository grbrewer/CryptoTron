using System;
using System.IO;
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
using System.Text.RegularExpressions;

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
            string path = @"c:\cryptotron\"; 
            if (!Directory.Exists(path))
            {
                DirectoryInfo di = Directory.CreateDirectory(path);
                di.Attributes = FileAttributes.Directory | FileAttributes.Hidden | FileAttributes.Encrypted;
            }

            Random rnd = new Random();
            string salt = rnd.Next(0, 1000).ToString();
            string filename = path + "untitled_" + salt + ".cpk";
            string keyID = "untitled_" + salt + ".cpk";

            // Generate a temporary name for the new key
            string pubKeyText = CryptoEngine.GenerateKeyPair(filename, false, null);

            pubkey publicKey = new pubkey()
            {
                email = m_userName,
                publicKey = pubKeyText,
                serialNumber = keyID
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

            // Refresh the Keys view
            var keyRows = (from pubKey in m_cdb.pubkeys
                          select pubKey);

            keyGrid.ItemsSource = keyRows;
            keyGrid.Items.Refresh();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            keyGrid.ItemsSource = m_cdb.pubkeys;
            
            //Only display the userRolesGrid if the current user is Administrator
            if (m_adminUser == false) {
                userRolesGrid.IsEnabled = false;
                userRolesGrid.Visibility = Visibility.Collapsed;
            }

            //Don't show current user role in roles list
            //(Used to avoid locking yourself out!)
            var userList = (from Users in m_cdb.Users
                            where Users.email != m_userName
                            select Users);

            userRolesGrid.ItemsSource = userList;

            adminRole.ItemsSource = adminTypes;
            mainKeyArea.DataContext = m_cdb.pubkeys;

            //Only show Publish buttons for unpublished keys
            //setupPublishButtons();
        }
      
        private void MetroWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            m_cdb.SubmitChanges();
            m_cdb.Dispose();
        }

        private void userRolesGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            m_cdb.SubmitChanges();
        }

        private void publishKey_Click(object sender, RoutedEventArgs e)
        {
            
            string pubKeyIdText = "";
            string userName = "";

            //Get the information from the current row
            for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
            {
                if (vis is DataGridRow)
                {
                    var currentRow = (DataGridRow)vis;

                    DataGridCell userCell = Utility.StaticDataGrid.GetCell(keyGrid, currentRow, 0);
                    DataGridCell keyIdCell = Utility.StaticDataGrid.GetCell(keyGrid, currentRow, 1);

                    var userCellTextContent = userCell.Content as TextBlock;
                    userName = userCellTextContent.Text;

                    var keyIdCellTextContent = keyIdCell.Content as TextBlock;
                    pubKeyIdText = keyIdCellTextContent.Text;
                    
                    break;
                }
            }

            // Do the actual SOAP publishing
            OceanInterfaceClient oc = new OceanInterfaceClient();
            string newSerialNumber = oc.publishPublicKey(userName, pubKeyIdText);

            // Save the new context to DB
            var keyRow = (from pubKey in m_cdb.pubkeys
                           where pubKey.email == m_userName
                           select pubKey).SingleOrDefault();

            keyRow.serialNumber = newSerialNumber;

            m_cdb.SubmitChanges();

        }

        private void setupPublishButtons()
        {
            //For all relevant rows of Datagrid, turn on Publish button
            for (int i = 0; i < keyGrid.Items.Count; i++)
            {
                DataGridRow myRow = Utility.StaticDataGrid.GetRow(keyGrid, i);

                DataGridCell keyIdCell = Utility.StaticDataGrid.GetCell(keyGrid, myRow, 1);
                DataGridCell pubBtnCell = Utility.StaticDataGrid.GetCell(keyGrid, myRow, 3);

                var keyIdCellTextContent = keyIdCell.Content as TextBlock;
                string pubKeyIdText = keyIdCellTextContent.Text;

                Match result = Regex.Match(pubKeyIdText, @"^(untitled)");

                var pubBtnPresenter = pubBtnCell.Content as ContentPresenter;
                var pubBtn = pubBtnPresenter.ContentTemplate.FindName("publishKey", pubBtnPresenter) as Button;

                if (result.Success)
                {
                    pubBtn.Visibility = Visibility.Visible;    
                }
                
            }

        }

        private void keyGrid_Loaded(object sender, RoutedEventArgs e)
        {
            setupPublishButtons();
        }

        
    }
}
