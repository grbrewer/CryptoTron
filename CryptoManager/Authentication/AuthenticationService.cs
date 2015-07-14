using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace CryptoManager
{
    public interface IAuthenticationService
    {
        User AuthenticateUser(string username, string password);
    }

    public class AuthenticationService : IAuthenticationService
    {
        
        public User AuthenticateUser(string username, string clearTextPassword)
        {
            CryptoDataClasses1DataContext cdb = new CryptoDataClasses1DataContext();
           
            //Check if the user name is in the DB
            User userData = cdb.Users.FirstOrDefault(u => u.email.Equals(username));

            if (userData == null)
                throw new UnauthorizedAccessException("Access denied. Please provide some valid credentials.");

            //If it is, hash the password and compare with entry in DB
            var salt = userData.email;
            var hashedPassword = CalculateHash(clearTextPassword, salt);
            
            if (userData.hashedPassword.Equals(hashedPassword) == false)
                throw new UnauthorizedAccessException("Access denied. Please provide some valid credentials.");

            //If everything checks out, we have valid credentials
            return userData; 
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
