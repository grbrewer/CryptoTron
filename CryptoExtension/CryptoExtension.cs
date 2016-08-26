using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

using SharpShell.SharpContextMenu;

using CryptoCore;
using System.Runtime.InteropServices;
using SharpShell.Attributes;

namespace CryptoExtension
{
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.ClassOfExtension,".txt")]
    public class CryptoExtension : SharpContextMenu
    {

        protected override bool CanShowMenu()
        {
            //We will always show the menu
            return true;
        }

        protected override ContextMenuStrip CreateMenu()
        {
            var menu = new ContextMenuStrip();

            var itemEncrypt = new ToolStripMenuItem
            {
                Text = "Encrypt"
            };

            itemEncrypt.Click += (sender, args) => EncyptItem();

            menu.Items.Add(itemEncrypt);

            return menu;
        }

        /// <summary>
        /// Encrypt, Compress and Archive the current selection
        /// </summary>
        private void EncyptItem()
        {
            foreach(var filePath in SelectedItemPaths)
            {
                string filename = Path.GetFileName(filePath);
                
                //Simply Encrypt if we are dealing with a file
                if (File.Exists(filename))
                {
                    //Create a random key and encrypt
                    string skey = CryptoEngine.GenerateRandomString(20, true);
                    CryptoEngine.EncryptFile(filename, skey);
     
                    //Encrypt the random key with current RSA public key
                    //string ekey = CryptoEngine.RsaEncrypt(skey, myPubkey);

                    //Save encrypted random key
                }

                //If this is a Directory, we want to compress as well..
                else if (Directory.Exists(filename))
                {
                    CryptoEngine.EncryptDirectory(filename, "adswerasd");
                }

                else
                {
                    //return some error condition
                }

            }

        }
    }
}
