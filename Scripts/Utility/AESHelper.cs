using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// AES 대칭키 기반 문자열 암호화 및 복호화 유틸리티
/// </summary>
public static class AESHelper
{
    private static readonly string key = "YWJjZGVmZ2hpamtsbW5vcHFyc3R1dnd4";
    private static readonly string iv = "eHl6MTIzNDU2Nzg5";

    /// <summary>
    /// 평문 문자열을 AES로 암호화
    /// </summary>
    public static string Encrypt(string plainText)
    {
        using Aes aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.IV = Encoding.UTF8.GetBytes(iv);

            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

        using MemoryStream ms = new();
        using CryptoStream cs = new(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
        using StreamWriter sw = new(cs);
                
        sw.Write(plainText);
        sw.Close();

        return Convert.ToBase64String(ms.ToArray());
    }

    /// <summary>
    /// AES로 암호화된 문자열을 복호화
    /// </summary>
    public static string Decrypt(string cipherText)
    {
        using Aes aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.IV = Encoding.UTF8.GetBytes(iv);

        byte[] buffer = Convert.FromBase64String(cipherText);

        using MemoryStream ms = new(buffer);
        using CryptoStream cs = new(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
        using StreamReader sr = new(cs);
            
        return sr.ReadToEnd();
    }
}