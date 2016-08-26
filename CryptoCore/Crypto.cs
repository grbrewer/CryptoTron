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

namespace CryptoCore
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

        public static string GenerateRandomString(int size, bool lowerCase)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch ;
            for(int i=0; i<size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65))) ;
                builder.Append(ch); 
            }
            if(lowerCase)
                return builder.ToString().ToLower();

            return builder.ToString();
        }

        public static string GenerateKeyPair(string targetFile, bool secure, string password)
        {
            RsaKeyPairGenerator r = new RsaKeyPairGenerator();
            r.Init(new KeyGenerationParameters(new SecureRandom(), 1024));
            AsymmetricCipherKeyPair keys = r.GenerateKeyPair();

            AsymmetricKeyParameter private_key = keys.Private;

            //Convert the private key to PEM text
            string privateKeyText = ConvertToPEM(private_key);

            //Encrypt the contents using AES with password
            if (secure == true)
                EncryptFile(targetFile, password);

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
        public static string RsaEncrypt(string plainText, AsymmetricKeyParameter publicKey)
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
            string cipherText = utf8enc.GetString(cipheredBytes);

            return cipherText;
        }

        /// <summary>
        /// Perform the decryption at the lowest level
        /// </summary>
        /// <param name="cypherText">the cipher text to decrypt</param>
        /// <param name="privateKey">the private key to use</param>
        /// <returns>the decrypted text</returns>
        public static string RsaDecrypt(string cypherText, AsymmetricKeyParameter privateKey)
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
        /// Encrypt a file using DES Algorithm
        /// </summary>
        /// <param name="path"></param>
        /// <param name="sKey"></param>
        public static void EncryptFile(string path, string sKey)
        {
            //Output filename should end in .enc
            string target = "";

            if (path.Contains(".enc") == false && 
                path.Contains(".cpk") == false)
                target = path + ".enc";
            else
                target = path;

            FileStream fsInput = new FileStream(path, FileMode.Open, FileAccess.Read);
            FileStream fsEncrypted = new FileStream(target, FileMode.Create, FileAccess.Write);

            AesCryptoServiceProvider AES = new AesCryptoServiceProvider();
            AES.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
            AES.IV = ASCIIEncoding.ASCII.GetBytes(sKey);
            ICryptoTransform rsaencrypt = AES.CreateEncryptor();

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
        public static void EncryptDirectory(string path, string sKey)
        {
            string target = path + ".enc";

            ZipFile.CreateFromDirectory(path, target);
            EncryptFile(target, sKey);
        }

        //  Call this function to remove the key from memory after use for security.
        [System.Runtime.InteropServices.DllImport("KERNEL32.DLL", EntryPoint = "RtlZeroMemory")]
        public static extern bool ZeroMemory(ref string Destination, int Length);

        // Function to Generate a 64 bits Key.
        static string GenerateKey()
        {
            // Create an instance of Symetric Algorithm. Key and IV is generated automatically.
            DESCryptoServiceProvider desCrypto = (DESCryptoServiceProvider)DESCryptoServiceProvider.Create();

            // Use the Automatically generated key for Encryption. 
            return ASCIIEncoding.ASCII.GetString(desCrypto.Key);
        }

        public static string CalculateHash(string clearTextPassword, string salt)
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
