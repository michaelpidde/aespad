using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace AesPad {
    class Encryption {
        static int keySize = 256;
        static int saltSize = 8;
        private string pwd;
        private byte[] salt;

        public Encryption(string pwd) {
            this.pwd = pwd;
        }

        private byte[] getKey() {
            salt = new byte[saltSize];
            Random rand = new Random();
            rand.NextBytes(salt);

            PasswordDeriveBytes key = new PasswordDeriveBytes(Encoding.Unicode.GetBytes(pwd), salt, "SHA256", 2);
            return key.GetBytes(keySize / 8);
        }

        public byte[] encrypt(string text) {
            byte[] key = getKey();
            byte[] ret = new byte[0];
            using(AesManaged aes = new AesManaged()) {
                aes.KeySize = keySize;
                ret.Concat(aes.IV);
                ret.Concat(salt);
                ret.Concat(encryptToAes(text, key, aes.IV));
            }
            return ret;
        }

        static byte[] encryptToAes(string text, byte[] key, byte[] iv) {
            if(text == null || text.Length <= 0)
                throw new ArgumentNullException("text");
            if(key == null || key.Length <= 0)
                throw new ArgumentNullException("key");
            if(iv == null || iv.Length <= 0)
                throw new ArgumentNullException("iv");

            byte[] encrypted;

            using(AesManaged aes = new AesManaged()) {
                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using(MemoryStream stream = new MemoryStream()) {
                    using(CryptoStream cryptoStream = new CryptoStream(stream, encryptor, CryptoStreamMode.Write)) {
                        using(StreamWriter writer = new StreamWriter(cryptoStream)) {
                            writer.Write(text);
                        }
                        encrypted = stream.ToArray();
                    }
                }
            }

            return encrypted;
        }

        public string decrypt(byte[] cypher) {
            
        }

        private string decryptFromAes(byte[] encrypted, byte[] key, byte[] iv) {
            if(encrypted == null || encrypted.Length <= 0)
                throw new ArgumentNullException("encrypted");
            if(key == null || key.Length <= 0)
                throw new ArgumentNullException("key");
            if(iv == null || iv.Length <= 0)
                throw new ArgumentNullException("iv");

            string ret;

            using(AesManaged aes = new AesManaged()) {
                aes.Key = key;
                aes.IV = iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using(MemoryStream stream = new MemoryStream(encrypted)) {
                    using(CryptoStream cryptoStream = new CryptoStream(stream, decryptor, CryptoStreamMode.Read)) {
                        using(StreamReader reader = new StreamReader(cryptoStream)) {
                            ret = reader.ReadToEnd();
                        }
                    }
                }
            }

            return ret;
        }
    }
}
