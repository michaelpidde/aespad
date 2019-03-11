using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace AesPad {
    class Encryption {
        private int keySize = 256;
        private int saltSize = 8;
        private int ivSize = 16;
        private string pwd;
        private byte[] salt;

        public Encryption(string pwd) {
            this.pwd = pwd;
        }

        private byte[] getKey(byte[] inSalt = null) {
            if(inSalt == null) {
                salt = new byte[saltSize];
                Random rand = new Random();
                rand.NextBytes(salt);
            } else {
                salt = inSalt;
            }

            PasswordDeriveBytes key = new PasswordDeriveBytes(Encoding.Unicode.GetBytes(pwd), salt, "SHA256", 2);
            return key.GetBytes(keySize / 8);
        }

        public byte[] encrypt(string text) {
            byte[] key = getKey();
            byte[] ret = new byte[0];
            using(AesManaged aes = new AesManaged()) {
                aes.KeySize = keySize;
                aes.BlockSize = 128;
                aes.Mode = CipherMode.CBC;
                byte[] cypher = encryptToAes(text, key, aes.IV);
                ret = new byte[aes.IV.Length + salt.Length + cypher.Length];
                Buffer.BlockCopy(aes.IV, 0, ret, 0, ivSize);
                Buffer.BlockCopy(salt, 0, ret, ivSize, saltSize);
                Buffer.BlockCopy(cypher, 0, ret, ivSize + saltSize, cypher.Length);
            }
            return ret;
        }

        private byte[] encryptToAes(string text, byte[] key, byte[] iv) {
            if(text == null)
                throw new ArgumentNullException("text");
            if(key == null || key.Length <= 0)
                throw new ArgumentNullException("key");
            if(iv == null || iv.Length <= 0)
                throw new ArgumentNullException("iv");

            byte[] encrypted;

            using(AesManaged aes = new AesManaged()) {
                aes.KeySize = keySize;
                aes.BlockSize = 128;
                aes.Mode = CipherMode.CBC;
                aes.Key = key;
                aes.IV = iv;
                aes.Padding = PaddingMode.PKCS7;

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
            byte[] iv = new byte[ivSize];
            Buffer.BlockCopy(cypher, 0, iv, 0, ivSize);
            byte[] salt = new byte[saltSize];
            Buffer.BlockCopy(cypher, ivSize, salt, 0, saltSize);
            byte[] cleanCypher = new byte[cypher.Length - ivSize - saltSize];
            Buffer.BlockCopy(cypher, ivSize + saltSize, cleanCypher, 0, cleanCypher.Length);
            byte[] key = getKey(salt);
            return decryptFromAes(cleanCypher, key, iv);
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
                aes.Padding = PaddingMode.PKCS7;

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
