using System.Security.Cryptography;
using System.Text;

namespace GmhWorkshop.CommonTools;

public static class ObfuscationTools
{
    public static string Decrypt(this string textToDecrypt, string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Key must have valid value.", nameof(key));
        }

        var combined = Convert.FromBase64String(textToDecrypt);
        var buffer = new byte[combined.Length];
        var hash = SHA512.Create();
        var aesKey = new byte[24];
        Buffer.BlockCopy(hash.ComputeHash(Encoding.UTF8.GetBytes(key)), 0, aesKey, 0, 24);

        using var aes = Aes.Create();
        if (aes == null)
        {
            throw new ArgumentException("Parameter must not be null.", nameof(aes));
        }

        aes.Key = aesKey;

        var iv = new byte[aes.IV.Length];
        var cipherText = new byte[buffer.Length - iv.Length];

        Array.ConstrainedCopy(combined, 0, iv, 0, iv.Length);
        Array.ConstrainedCopy(combined, iv.Length, cipherText, 0, cipherText.Length);

        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var resultStream = new MemoryStream();
        using (var aesStream = new CryptoStream(resultStream, decryptor, CryptoStreamMode.Write))
        using (var plainStream = new MemoryStream(cipherText))
        {
            plainStream.CopyTo(aesStream);
        }

        return Encoding.UTF8.GetString(resultStream.ToArray());
    }

    public static string Encrypt(this string text, string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Key must have valid value.", nameof(key));
        }

        var buffer = Encoding.UTF8.GetBytes(text);
        var hash = SHA512.Create();
        var aesKey = new byte[24];
        Buffer.BlockCopy(hash.ComputeHash(Encoding.UTF8.GetBytes(key)), 0, aesKey, 0, 24);

        using var aes = Aes.Create();
        if (aes == null)
        {
            throw new ArgumentException("Parameter must not be null.", nameof(aes));
        }

        aes.Key = aesKey;

        using var decryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var resultStream = new MemoryStream();
        using (var aesStream = new CryptoStream(resultStream, decryptor, CryptoStreamMode.Write))
        using (var plainStream = new MemoryStream(buffer))
        {
            plainStream.CopyTo(aesStream);
        }

        var result = resultStream.ToArray();
        var combined = new byte[aes.IV.Length + result.Length];
        Array.ConstrainedCopy(aes.IV, 0, combined, 0, aes.IV.Length);
        Array.ConstrainedCopy(result, 0, combined, aes.IV.Length, result.Length);

        return Convert.ToBase64String(combined);
    }
}