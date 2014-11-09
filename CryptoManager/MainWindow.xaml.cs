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

using CryptoManager.OceanReference1;
using System.Xml;

namespace CryptoManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            OceanInterfaceClient oc = new OceanInterfaceClient();
            string[] serialNumbers = oc.getSerialNumbers("kevin@encom.com");
            string publicKeyText = oc.downloadPublicKey("kevin@encom.com", serialNumbers[0]);

            string testPublicKey = CryptoEngine.GenerateKeyPair("c:\\KeyStore\\keytest.gef");

            
            CryptoDataClasses1DataContext cdb = new CryptoDataClasses1DataContext();

            pubkey publicKey = new pubkey()
            {
                email = "kevin@encom.com",
                publicKey = testPublicKey,
                serialNumber = serialNumbers[0]
            };

  
            cdb.pubkeys.InsertOnSubmit(publicKey);

            try
            {
                cdb.SubmitChanges();
            }

            catch (System.Exception e)
            {
                Console.WriteLine(e);
            }

            cdb.Dispose();
            

            //oc.publishPublicKey("lora@encom.com", testPublicKey);

            InitializeComponent();
        }
    }
}
