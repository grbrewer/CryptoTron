using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

using SharpShell.SharpContextMenu;

using CryptoCore;

namespace CryptoExtension
{
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
                    CryptoEngine.EncryptFile(filename, "adswerasd");
                }

                //If this is a Directory, we want to compress as well..
                else if (Directory.Exists(filename))
                {
                    CryptoEngine.EncryptDirectory(filename);
                }

                else
                {
                    //return some error condition
                }

            }

        }
    }
}
