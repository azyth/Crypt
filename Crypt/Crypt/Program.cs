using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Crypt {
    class Program {
        static void Main(string[] args) {
            string task = "Q";
            string file = "";
            string password = "";
            byte[] passbyte;
            bool zipped = false;

            try {
                //prompt Encrypt/decrypt
                Console.WriteLine("Welcome to Crypt Encryption Software");
                Console.WriteLine("------------------------------------");
                Console.WriteLine("----------------//\\\\----------------");
                Console.WriteLine("--------------_||__||_--------------");
                Console.WriteLine("-------------|        |-------------");
                Console.WriteLine("-------------|   ()   |-------------");
                Console.WriteLine("-------------|________|-------------");
                Console.WriteLine("------------------------------------");
                while (!zipped) {
                    Console.WriteLine("Task: E=Encrpytion, D=Decryption \r\nQ=Quit");
                    task = Console.ReadLine();
                    if (task.ToUpper() == "Q") {
                        return;
                    }
                    //select File/Directory
                    Console.WriteLine("Enter Filename or Directory");
                    file = Console.ReadLine();
                    if (task.ToUpper() == "E") {
                        //Zip it to a destination file
                        if (File.Exists(file)) {
                            zipped = true;
                            Directory.CreateDirectory(Path.GetFileNameWithoutExtension(file));
                            File.Move(file, Path.GetFileNameWithoutExtension(file) + "/" + file);
                            ZipFile.CreateFromDirectory(Path.GetFileNameWithoutExtension(file), "temp.zip");
                            file = Path.GetFileNameWithoutExtension(file);
                        } else if (Directory.Exists(file)) {
                            zipped = true;
                            ZipFile.CreateFromDirectory(file, "temp.zip");
                        } else {
                            Console.WriteLine("Sorry No File or Directory found of name: " + file);
                        }
                    }
                    if (task.ToUpper() == "D") {
                        if (File.Exists(file) || File.Exists(file + ".crypt")) {
                            zipped = true;
                            if (File.Exists(file + ".crypt")) { file = file + ".crypt"; }
                        } else {
                            Console.WriteLine("Sorry No File or Directory found of name: " + file);
                        }
                    }
                }//end while !zipped
                 //Get a password from user to encrypt/decrypt ZipFile 
                while (password.Length != 8) {
                    Console.WriteLine("Enter your encryption password (must be 8 characters long):");
                    password = ReadPassword('*');
                }
                //System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
                //passbyte = encoding.GetBytes(password);
                //password = Convert.ToString(passbyte);
                //password = "";
                //foreach(byte b in passbyte) {
                //    int i = (int)b;
                //    password = password + i;
                //}
                if (task.ToUpper() == "E") {
                    //encrypt file 
                    EncryptFile("temp.zip", file + ".crypt", password);

                }
                if (task.ToUpper() == "D") {
                    DecryptFile(file, "temp.zip", Convert.ToString(password));
                    ZipFile.ExtractToDirectory("temp.zip", Path.GetFileNameWithoutExtension(file));
                }
                
                //close program
                Console.WriteLine("Your Files are now protected in the .crypt file do not delete it! \r\nRemember your password it is your only chance to recover them.");
                Console.ReadLine();
            } finally {
                //delete origional file/directory
                File.Delete("temp.zip");
                Directory.Delete(file);
            }
        }

        /* origionally authored by Shermy - stack overflow user
         * Like System.Console.ReadLine(), only with a mask.
         * <param name="mask">a <c>char</c> representing your choice of console mask</param>
         * <returns>the string the user typed in </returns>
         */
        public static string ReadPassword(char mask) {
            const int ENTER = 13, BACKSP = 8, CTRLBACKSP = 127;
            int[] FILTERED = { 0, 27, 9, 10 /*, 32 space, if you care */ }; // const

            var pass = new Stack<char>();
            char chr = (char)0;

            while ((chr = System.Console.ReadKey(true).KeyChar) != ENTER) {
                if (chr == BACKSP) {
                    if (pass.Count > 0) {
                        System.Console.Write("\b \b");
                        pass.Pop();
                    }
                } else if (chr == CTRLBACKSP) {
                    while (pass.Count > 0) {
                        System.Console.Write("\b \b");
                        pass.Pop();
                    }
                } else if (FILTERED.Count(x => chr == x) > 0) { } else {
                    pass.Push((char)chr);
                    System.Console.Write(mask);
                }
            }

            System.Console.WriteLine();

            return new string(pass.Reverse().ToArray());
        }

        /* encrypts a file
         * skey must be 8 char long
         */
        static void EncryptFile(string sInputFilename, string sOutputFilename, string sKey) {
            FileStream fsInput = new FileStream(sInputFilename,
                FileMode.Open,
                FileAccess.Read);
            FileStream fsEncrypted = new FileStream(sOutputFilename,
                            FileMode.Create,
                            FileAccess.Write);
            DESCryptoServiceProvider DES = new DESCryptoServiceProvider();

            DES.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
            DES.IV = ASCIIEncoding.ASCII.GetBytes(sKey);

            ICryptoTransform desencrypt = DES.CreateEncryptor();
            CryptoStream cryptostream = new CryptoStream(fsEncrypted,
                                desencrypt,
                                CryptoStreamMode.Write);

            byte[] bytearrayinput = new byte[fsInput.Length - 1];
            fsInput.Read(bytearrayinput, 0, bytearrayinput.Length);
            cryptostream.Write(bytearrayinput, 0, bytearrayinput.Length);


        }

        static void DecryptFile(string sInputFilename, string sOutputFilename, string sKey) {
            DESCryptoServiceProvider DES = new DESCryptoServiceProvider();
            //A 64 bit key and IV is required for this provider.
            //Set secret key For DES algorithm.
            DES.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
            //Set initialization vector.
            DES.IV = ASCIIEncoding.ASCII.GetBytes(sKey);

            //Create a file stream to read the encrypted file back.
            FileStream fsread = new FileStream(sInputFilename,
                                           FileMode.Open,
                                           FileAccess.Read);
            //Create a DES decryptor from the DES instance.
            ICryptoTransform desdecrypt = DES.CreateDecryptor();
            //Create crypto stream set to read and do a 
            //DES decryption transform on incoming bytes.
            CryptoStream cryptostreamDecr = new CryptoStream(fsread,
                                                         desdecrypt,
                                                         CryptoStreamMode.Read);
            //Print the contents of the decrypted file.
            StreamWriter fsDecrypted = new StreamWriter(sOutputFilename);
            fsDecrypted.Write(new StreamReader(cryptostreamDecr).ReadToEnd());
            fsDecrypted.Flush();
            fsDecrypted.Close();
        }
    }
}
