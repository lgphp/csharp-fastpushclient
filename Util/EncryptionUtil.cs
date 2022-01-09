using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using EasyEncryption;

namespace FastLivePushClient.Util
{
    /**
     * 改编自 EasyEncryption.AES.cs
     */
    public class EncryptionUtil
    {
        public static byte[] AESEncrypt(string data, byte[] key, byte[] iv)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            using (AesCryptoServiceProvider cryptoServiceProvider = new AesCryptoServiceProvider())
            {
                cryptoServiceProvider.Key = key;
                cryptoServiceProvider.IV = iv;
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoServiceProvider.CreateEncryptor(),
                        CryptoStreamMode.Write);
                    cryptoStream.Write(bytes, 0, bytes.Length);
                    cryptoStream.Close();
                    memoryStream.Close();
                    return memoryStream.ToArray();
                }
            }
        }


        public static byte[] AESDecrypt(byte[] data, byte[] key, byte[] iv)
        {
            
            using (AesCryptoServiceProvider cryptoServiceProvider = new AesCryptoServiceProvider())
            {
                cryptoServiceProvider.Key =  key;
                cryptoServiceProvider.IV =  iv;
                ICryptoTransform decryptor = cryptoServiceProvider.CreateDecryptor(cryptoServiceProvider.Key, cryptoServiceProvider.IV);
                using (MemoryStream memoryStream = new MemoryStream(data))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream) memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream) cryptoStream))
                            return Encoding.UTF8.GetBytes(streamReader.ReadToEnd());
                    }
                }
            }
        }
    }
}