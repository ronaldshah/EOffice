using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Security
{
    public class EncryptIT
    {
        /// <summary>
        /// Return Key block from key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public const string Seprate = "|||";
        private static byte[] GetKey()
        {
            return new byte[] { 125, 180, 94, 41, 193, 38, 216, 251, 0, 31, 166, 182, 71, 95, 179, 160, 10, 29, 43, 6, 17, 202, 20, 223, 28, 53, 65, 212, 150, 100, 70, 152 };
        }

        private static byte[] GetIV()
        {
            return new byte[] { 166, 133, 210, 29, 40, 117, 221, 245, 75, 160, 132, 244, 142, 35, 33, 21 };
        }


        /// <summary>
        /// Uses Rijndael Managed Class to Encrypt, Resulting data is transportable over HTTP
        /// </summary>
        /// <param name="key"></param>
        /// <param name="Data"></param>
        /// <returns></returns>
        public string Encrypt(string Data, bool useKey)
        {
            if (Data == null)
                return null;

            using (RijndaelManaged myRijndael = new RijndaelManaged())
            {
                UnicodeEncoding textConverter = new UnicodeEncoding();
                byte[] encrypted;
                byte[] toEncrypt;
                byte[] key = GetKey();
                byte[] IV = GetIV();


                //  myRijndael.Padding = PaddingMode.None;

                //Get an encryptor.
                ICryptoTransform encryptor = myRijndael.CreateEncryptor(key, IV);

                //Encrypt the data.
                MemoryStream msEncrypt = new MemoryStream();
                CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);

                //Convert the data to a byte array.
                Data = Data + Seprate + DateTime.Now;
                toEncrypt = textConverter.GetBytes(Data);

                //Write all data to the crypto stream and flush it.
                csEncrypt.Write(toEncrypt, 0, toEncrypt.Length);
                csEncrypt.FlushFinalBlock();

                //Get encrypted array of bytes.
                encrypted = msEncrypt.ToArray();
                msEncrypt.Dispose();

                return Convert.ToBase64String(encrypted);
            }
        }



        public string Decrypt(string Data, bool usekey)
        {
            string[] res;


            if (Data == null)
                return null;

            using (RijndaelManaged myRijndael = new RijndaelManaged())
            {
                UnicodeEncoding textConverter = new UnicodeEncoding();
                byte[] key = GetKey();
                byte[] IV = GetIV();
                byte[] fromEncrypt;
                byte[] encrypted = Convert.FromBase64String(Data.Replace(" ", "+"));

                myRijndael.Padding = PaddingMode.PKCS7;
                //Get a decryptor that uses the same key and IV as the encryptor.
                ICryptoTransform decryptor = myRijndael.CreateDecryptor(key, IV);

                //Now decrypt the previously encrypted message using the decryptor
                // obtained in the above step.
                MemoryStream msDecrypt = new MemoryStream(encrypted);
                CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);

                fromEncrypt = new byte[encrypted.Length];

                //Read the data out of the crypto stream.
                csDecrypt.Read(fromEncrypt, 0, fromEncrypt.Length);

                //Convert the byte array back into a string.
                res = textConverter.GetString(fromEncrypt).Split(Seprate.ToCharArray());

                return textConverter.GetString(fromEncrypt);
            }
        }
    }
}
