/// <summary>
/// AES 암호화/복호화를 수행하는 유틸리티 클래스
/// </summary>
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class AESHelper
{
    private static readonly string Key = "60SecHeroSecretKey!";
    private static readonly string IV = "InitializationVe";

    public static string Encrypt(string plainText)
    {
        using Aes aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(Key);
        aes.IV = Encoding.UTF8.GetBytes(IV);

        ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using MemoryStream ms = new MemoryStream();
        using CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        using (StreamWriter writer = new StreamWriter(cs))
        {
            writer.Write(plainText);
        }

        return Convert.ToBase64String(ms.ToArray());
    }

    public static string Decrypt(string encryptedText)
    {
        using Aes aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(Key);
        aes.IV = Encoding.UTF8.GetBytes(IV);

        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        byte[] buffer = Convert.FromBase64String(encryptedText);

        using MemoryStream ms = new(buffer);
        using CryptoStream cs = new (ms, decryptor, CryptoStreamMode.Read);
        using StreamReader sr = new(cs);
        {
            return sr.ReadToEnd();
        }
    }
}