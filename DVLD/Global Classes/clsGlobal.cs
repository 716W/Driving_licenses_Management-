using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DVLD_Buisness;


namespace DVLD.Classes
{
    internal static  class clsGlobal
    {
        public static clsUser CurrentUser;

        /// <summary>
        /// Persists only the username for Remember-Me convenience.
        /// The password is NEVER written to disk — the user must always type it.
        /// Pass an empty Username to clear the stored credential.
        /// </summary>
        public static bool RememberUsernameAndPassword(string Username, string Password)
        {
            // NOTE: the Password parameter is intentionally ignored for security.
            // Plaintext passwords must never be written to disk.
            try
            {
                string currentDirectory = System.IO.Directory.GetCurrentDirectory();
                string filePath = currentDirectory + "\\data.txt";

                // Clear: if username is empty, delete the file
                if (Username == "" && File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }

                // Store only the username — no password
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine(Username);
                    return true;
                }
            }
            catch (Exception ex)
            {
               MessageBox.Show($"An error occurred: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Returns the stored username (if any) for Remember-Me pre-fill.
        /// Password is always returned as empty string — the user must type it.
        /// </summary>
        public static bool GetStoredCredential(ref string Username, ref string Password)
        {
            // Password is never stored on disk — always return empty.
            Password = "";
            try
            {
                string currentDirectory = System.IO.Directory.GetCurrentDirectory();
                string filePath  = currentDirectory + "\\data.txt";

                if (File.Exists(filePath))
                {
                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        string? line = reader.ReadLine();
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            Username = line.Trim();
                            return true;
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
                return false;
            }
        }
    }
}
