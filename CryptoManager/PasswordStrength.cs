using System;
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoManager
{
    public class PasswordStrength
    {
        private string m_Complexity;
        private string m_PreviousPassword = "";

        public string Complexity
        {
            get { return m_Complexity; }
        }

        /// <summary>
        /// Set the password
        /// </summary>
        /// <param name="pwd"></param>
        public void SetPassword(string pwd)
        {
            if (m_PreviousPassword != pwd)
            {
                m_PreviousPassword = pwd;
                CheckPasswordWithDetails(m_PreviousPassword);
            }
        }

        /// <summary>
        /// Wrapper for private password checking function
        /// </summary>
        public void CheckPassword()
        {
            CheckPasswordWithDetails(m_PreviousPassword);
        }

        /// <summary>
        /// This is the method which checks the password and determines the score.
        /// </summary>
        /// <param name="pwd"></param>
        private void CheckPasswordWithDetails(string pwd)
        {
            // Init Vars
            int nScore = 0;
            int iUpperCase = 0;
            int iLowerCase = 0;
            int iDigit = 0;
            int iSymbol = 0;
            int iRepeated = 1;
            int iMiddle = 0;
            int iMiddleEx = 1;
            int ConsecutiveMode = 0;
            int iConsecutiveUpper = 0;
            int iConsecutiveLower = 0;
            int iConsecutiveDigit = 0;
            string sAlphas = "abcdefghijklmnopqrstuvwxyz";
            string sNumerics = "01234567890";
            int nSeqAlpha = 0;
            int nSeqChar = 0;
            int nSeqNumber = 0;

            Hashtable htRepeated = new Hashtable();

            
            // Scan password
            foreach (char ch in pwd.ToCharArray())
            {
                // Count digits
                if (Char.IsDigit(ch))
                {
                    iDigit++;

                    if (ConsecutiveMode == 3)
                        iConsecutiveDigit++;
                    ConsecutiveMode = 3;
                }

                // Count uppercase characters
                if (Char.IsUpper(ch))
                {
                    iUpperCase++;
                    if (ConsecutiveMode == 1)
                        iConsecutiveUpper++;
                    ConsecutiveMode = 1;
                }

                // Count lowercase characters
                if (Char.IsLower(ch))
                {
                    iLowerCase++;
                    if (ConsecutiveMode == 2)
                        iConsecutiveLower++;
                    ConsecutiveMode = 2;
                }

                // Count symbols
                if (Char.IsSymbol(ch) || Char.IsPunctuation(ch))
                {
                    iSymbol++;
                    ConsecutiveMode = 0;
                }

                // Count repeated letters 
                if (Char.IsLetter(ch))
                {
                    if (htRepeated.Contains(Char.ToLower(ch))) iRepeated++;
                    else htRepeated.Add(Char.ToLower(ch), 0);

                    if (iMiddleEx > 1)
                        iMiddle = iMiddleEx - 1;
                }

                if (iUpperCase > 0 || iLowerCase > 0)
                {
                    if (Char.IsDigit(ch) || Char.IsSymbol(ch))
                        iMiddleEx++;
                }
            }

            // Check for sequential alpha string patterns (forward and reverse) 
            for (int s = 0; s < 23; s++)
            {
                string sFwd = sAlphas.Substring(s, 3);
                string sRev = strReverse(sFwd);
                if (pwd.ToLower().IndexOf(sFwd) != -1 || pwd.ToLower().IndexOf(sRev) != -1)
                {
                    nSeqAlpha++;
                    nSeqChar++;
                }
            }

            // Check for sequential numeric string patterns (forward and reverse)
            for (int s = 0; s < 8; s++)
            {
                string sFwd = sNumerics.Substring(s, 3);
                string sRev = strReverse(sFwd);
                if (pwd.ToLower().IndexOf(sFwd) != -1 || pwd.ToLower().IndexOf(sRev) != -1)
                {
                    nSeqNumber++;
                    nSeqChar++;
                }
            }

            // Calcuate score
        
            // Score += 4 * Password Length
            nScore = 4 * pwd.Length;
        
            // if we have uppercase letetrs Score +=(number of uppercase letters *2)
            if (iUpperCase > 0)
            {
                nScore += ((pwd.Length - iUpperCase) * 2);
            }
            else
        
            // if we have lowercase letetrs Score +=(number of lowercase letters *2)
            if (iLowerCase > 0)
            {
                nScore += ((pwd.Length - iLowerCase) * 2);
            }
        

            // Score += (Number of digits *4)
            nScore += (iDigit * 4);
        
            // Score += (Number of Symbols * 6)
            nScore += (iSymbol * 6);
        
            // Score += (Number of digits or symbols in middle of password *2)
            nScore += (iMiddle * 2);
        
            //requirments
            int requirments = 0;
            if (pwd.Length >= 8) requirments++;     // Min password length
            if (iUpperCase > 0) requirments++;      // Uppercase letters
            if (iLowerCase > 0) requirments++;      // Lowercase letters
            if (iDigit > 0) requirments++;          // Digits
            if (iSymbol > 0) requirments++;         // Symbols

            // If we have more than 3 requirments then
            if (requirments > 3)
            {
                // Score += (requirments *2) 
                nScore += (requirments * 2);
            }
      
            //
            // Deductions
            //
      
            // If only letters then score -=  password length
            if (iDigit == 0 && iSymbol == 0)
            {
                nScore -= pwd.Length;
            }
      
            // If only digits then score -=  password length
            if (iDigit == pwd.Length)
            {
                nScore -= pwd.Length;
            }
      
            // If repeated letters used then score -= (iRepeated * (iRepeated - 1));
            if (iRepeated > 1)
            {
                nScore -= (iRepeated * (iRepeated - 1));
            }

            // If Consecutive uppercase letters then score -= (iConsecutiveUpper * 2);
            nScore -= (iConsecutiveUpper * 2);
      
            // If Consecutive lowercase letters then score -= (iConsecutiveUpper * 2);
            nScore -= (iConsecutiveLower * 2);
      
            // If Consecutive digits used then score -= (iConsecutiveDigits* 2);
            nScore -= (iConsecutiveDigit * 2);

            // If password contains sequence of letters then score -= (nSeqAlpha * 3)
            nScore -= (nSeqAlpha * 3);

            // If password contains sequence of digits then score -= (nSeqNumber * 3)
            nScore -= (nSeqNumber * 3);

            /* Determine complexity based on overall score */
            if (nScore > 100) { nScore = 100; } else if (nScore < 0) { nScore = 0; }
            if (nScore >= 0 && nScore < 20) { m_Complexity = "Very Weak"; }
            else if (nScore >= 20 && nScore < 40) { m_Complexity = "Weak"; }
            else if (nScore >= 40 && nScore < 60) { m_Complexity = "Good"; }
            else if (nScore >= 60 && nScore < 80) { m_Complexity = "Strong"; }
            else if (nScore >= 80 && nScore <= 100) { m_Complexity = "Very Strong"; }

        }

        /// <summary>
        /// Helper string function to reverse string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private String strReverse(String str)
        {
            string newstring = "";
            for (int s = 0; s < str.Length; s++)
            {
                newstring = str[s] + newstring;
            }
            return newstring;
        }

    }
}
