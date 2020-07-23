using System;
using System.IO;
using System.Security.Cryptography;

namespace EnDecrypt
{
  static class EnDeCrypt
  {
    // Key
    static byte[] desKey;
    static byte[] desIV;

    static void Init()
    {
      if (desKey == null)
      {
        // Create a new instance of the RijndaelManaged
        // class.  This generates a new key and initialization
        // vector (IV).
        using (RijndaelManaged myRijndael = new RijndaelManaged())
        {
          myRijndael.GenerateKey();
          myRijndael.GenerateIV();

          desKey = myRijndael.Key;
          desIV = myRijndael.IV;
        }
      }
    }

    internal static byte[] EncryptStringToBytes(string plainText)
    {
      Init(); // init Key and initialization vector (IV) if not set
      byte[] encrypted;
      // Create an RijndaelManaged object
      // with the specified key and IV.
      using (RijndaelManaged rijAlg = new RijndaelManaged())
      {
        rijAlg.Key = desKey;
        rijAlg.IV = desIV;

        // Create an encryptor to perform the stream transform.
        ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

        // Create the streams used for encryption.
        using (MemoryStream msEncrypt = new MemoryStream())
        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        {
          using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
            swEncrypt.Write(plainText); // Write all data to the stream.
          encrypted = msEncrypt.ToArray();
        }
      }

      // Return the encrypted bytes from the memory stream.
      return encrypted;
    }

    internal static string DecryptStringFromBytes(byte[] cipherText)
    {
      Init(); // init Key and initialization vector (IV) if not set
      // Check arguments.
      if (cipherText == null || cipherText.Length <= 0) throw new ArgumentNullException("cipherText");

      // Declare the string used to hold
      // the decrypted text.
      string plaintext = null;

      // Create an RijndaelManaged object
      // with the specified key and IV.
      using (RijndaelManaged rijAlg = new RijndaelManaged())
      {
        rijAlg.Key = desKey;
        rijAlg.IV = desIV;

        // Create a decryptor to perform the stream transform.
        ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

        // Create the streams used for decryption.
        using (MemoryStream msDecrypt = new MemoryStream(cipherText))
        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
          // Read the decrypted bytes from the decrypting stream
          // and place them in a string.
          plaintext = srDecrypt.ReadToEnd();
      }

      return plaintext;
    }
  }

}
