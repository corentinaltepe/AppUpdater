using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AppLib.Test
{
    [TestClass]
    public class StringCipherTest
    {
        [TestMethod]
        public void EncryptDecryptTest()
        {
            string id = "ujrWZlyKQ4FLAS49";
            string key = "tSzmfr1C35YAYI6r";

            string encryptedID = StringCipher.Encrypt(id, key);
            string decryptedID = StringCipher.Decrypt(encryptedID, key);

            Assert.AreEqual(id, decryptedID);
        }
    }
}
