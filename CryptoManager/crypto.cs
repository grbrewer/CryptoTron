using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CryptoManager
{
    public class CryptoEngine
    {
        public CryptoEngine()
        { }

        public static string GenerateKeyPair(string targetFile)
        {
            RSACryptoServiceProvider algorithm = new RSACryptoServiceProvider();

            //Save the private key
            string completeKey = algorithm.ToXmlString(true);
            byte[] keyBytes = Encoding.UTF8.GetBytes(completeKey);

            keyBytes = ProtectedData.Protect(keyBytes, null, DataProtectionScope.CurrentUser);
            
            using (FileStream fs = new FileStream(targetFile, FileMode.Create))
            {
                fs.Write(keyBytes, 0, keyBytes.Length);
            }

            //return the public key
            return algorithm.ToXmlString(false);
        }
    }
}
