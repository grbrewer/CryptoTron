using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Asn1.Pkcs;

namespace CryptoManager
{
    public class CryptoEngine
    {
        public CryptoEngine()
        { }

        public static AsymmetricKeyParameter readPrivateKey(string privateKeyFileName)
        {
            AsymmetricKeyParameter privateKey = new AsymmetricKeyParameter(true);

            var fileStream = System.IO.File.OpenText(privateKeyFileName);
            var pemReader = new PemReader(fileStream);
            var privateKeyParameter = (AsymmetricKeyParameter)pemReader.ReadObject();
            return privateKeyParameter;
        }

        public static string ConvertToPEM(AsymmetricKeyParameter pubKey)
        {
            TextWriter textWriter = new StringWriter();
            PemWriter pemWriter = new PemWriter(textWriter);
            pemWriter.WriteObject(pubKey);
            pemWriter.Writer.Flush();

            string publicKeyText = textWriter.ToString();

            return publicKeyText;
        }

        public static string GenerateKeyPair(string targetFile)
        {
            RsaKeyPairGenerator r = new RsaKeyPairGenerator();
            r.Init(new KeyGenerationParameters(new SecureRandom(), 1024));
            AsymmetricCipherKeyPair keys = r.GenerateKeyPair();

            AsymmetricKeyParameter private_key = keys.Private;

            //Convert the private key to PEM text
            string privateKeyText = ConvertToPEM(private_key);

            //Save the PEM encoded string to file
            using (FileStream fs = File.OpenWrite(targetFile))
            {
                Byte[] privateKeyData =
                new UTF8Encoding(true).GetBytes(privateKeyText);

                fs.Write(privateKeyData, 0, privateKeyData.Length);
            }

            //Return the Public Key
            AsymmetricKeyParameter public_key = keys.Public;

            return ConvertToPEM(public_key);
        }

        /// <summary>
        /// Perform the encryption as the most basic level
        /// </summary>
        /// <param name="plainText">the text to encrypt</param>
        /// <param name="publicKey">the public key to encrypt to</param>
        /// <returns>the encrypted bytestream</returns>
        public byte[] encrypt(string plainText, AsymmetricKeyParameter publicKey)
        {
            UTF8Encoding utf8enc = new UTF8Encoding();

            // Converting the string message to byte array
            byte[] inputBytes = utf8enc.GetBytes(plainText);

            // Creating the RSA algorithm object
            IAsymmetricBlockCipher cipher = new RsaEngine();

            // Initializing the RSA object for Encryption with RSA public key. 
            cipher.Init(true, publicKey);

            //Encrypting the input bytes
            byte[] cipheredBytes = cipher.ProcessBlock(inputBytes, 0, plainText.Length);

            return cipheredBytes;
        }

        /// <summary>
        /// Perform the decryption at the lowest level
        /// </summary>
        /// <param name="cypherText">the cipher text to decrypt</param>
        /// <param name="privateKey">the private key to use</param>
        /// <returns>the decrypted text</returns>
        public string decrypt(string cypherText, AsymmetricKeyParameter privateKey)
        {
            UTF8Encoding utf8enc = new UTF8Encoding();

            // Converting the string message to byte array
            byte[] encryptedBytes = utf8enc.GetBytes(cypherText);

            PemReader pemReader = new PemReader(new StringReader(cypherText));

            // Create the RSA algorithm object
            IAsymmetricBlockCipher cipher = new RsaEngine();

            //Do the encryption and return the plaintext
            cipher.Init(false, privateKey);

            byte[] decipheredBytes = cipher.ProcessBlock(encryptedBytes, 0, encryptedBytes.Length);
            string plainText = utf8enc.GetString(decipheredBytes);

            return plainText;
        }

        /// <summary>
        /// Use DES to Encrypt a file
        /// </summary>
        /// <param name="path">file path</param>
        /// <param name="sKey">DES key</param>
        public void EncryptFile(string path, string sKey)
        {
            //Output filename should end in .cao
            string target = "";

            if (path.Contains(".cao") == false)
                target = path + ".cao";
            else
                target = path;

            //Set up input and output filestreams
            FileStream fsInput = new FileStream(path, FileMode.Open, FileAccess.Read);
            FileStream fsEncrypted = new FileStream(target, FileMode.Create, FileAccess.Write);
            
            DESCryptoServiceProvider DES = new DESCryptoServiceProvider();
            DES.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
            DES.IV = ASCIIEncoding.ASCII.GetBytes(sKey);
            ICryptoTransform rsaencrypt = DES.CreateEncryptor();

            CryptoStream cryptostream = new CryptoStream(fsEncrypted,
               rsaencrypt,
               CryptoStreamMode.Write);

            byte[] bytearrayinput = new byte[fsInput.Length];
            fsInput.Read(bytearrayinput, 0, bytearrayinput.Length);
            cryptostream.Write(bytearrayinput, 0, bytearrayinput.Length);
            cryptostream.Close();
            fsInput.Close();
            fsEncrypted.Close();
        }

        /// <summary>
        /// Encrypt, Compress and Archive a given directory
        /// </summary>
        /// <param name="path">The directory we wish to encrypt and compress</param>
        public void EncryptDirectory(string path)
        {
            string target = path + ".cao";

            string skey = "sadflkjwenafsndm"; 

            ZipFile.CreateFromDirectory(path, target);
            EncryptFile(target, skey);
        }

    }
}
