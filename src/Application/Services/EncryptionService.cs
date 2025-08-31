using System.Security.Cryptography;

namespace Engrslan.Services;

public class EncryptionService : IEncryptionService
{
    private readonly byte[] _defaultSalt;
    private readonly byte[] _defaultIv;

    public EncryptionService(string? defaultSalt = null, string? defaultIv = null)
    {
        _defaultSalt = defaultSalt?.ToBytes()??"MyDefaultSaltKey1234".ToBytes();
        _defaultIv = GetIv (defaultIv?.ToBytes() ?? "MyDefaultIvKey1234".ToBytes(),_defaultSalt);
    }
    
    private static byte[] GetIv(byte[] ivKey, byte[] saltKey)
    {
        var combined = new byte[ivKey.Length + saltKey.Length];
        Buffer.BlockCopy(ivKey, 0, combined, 0, ivKey.Length);
        Buffer.BlockCopy(saltKey, 0, combined, ivKey.Length, saltKey.Length);
        var hash = SHA256.HashData(combined);
        var iv = new byte[16];
        Array.Copy(hash, iv, 16);
        return iv;
    }

    private Aes CreateAes(string? ivKey = null, string? saltKey= null)
    {
        var aes = Aes.Create();
        aes.Key = saltKey?.ToBytes() ?? _defaultSalt;
        aes.IV = ivKey?.ToBytes() ?? _defaultIv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        return aes;
    }
    
    private byte[] EncryptAesInternal(byte[] data, string? ivKey = null, string? saltKey= null)
    {
        using var aes = CreateAes(ivKey,saltKey);
        using var encryptor = aes.CreateEncryptor();
        return encryptor.TransformFinalBlock(data, 0, data.Length);
    }

    private byte[] DecryptAesInternal(byte[] cipher, string? ivKey, string? saltKey)
    {
        using var aes = CreateAes(ivKey,saltKey);
        using var decryptor = aes.CreateDecryptor();
        return decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
    }
    
    public string EncryptAES(string plainText, string? ivKey = null, string? saltKey = null)
    {
        return EncryptAesInternal(plainText.ToBytes(), ivKey, saltKey).ToBase64String();
    }

    public string EncryptAES(int value, string? ivKey = null, string? saltKey = null)
    {
        return EncryptAesInternal(BitConverter.GetBytes(value), ivKey, saltKey).ToBase64String();
    }

    public string EncryptAES(long value, string? ivKey = null, string? saltKey = null)
    {
        return EncryptAesInternal(BitConverter.GetBytes(value), ivKey, saltKey).ToBase64String();
    }

    public string EncryptAES(byte[] value, string? ivKey = null, string? saltKey = null)
    {
        return EncryptAesInternal(value, ivKey, saltKey).ToBase64String();
    }

    public string DecryptAESToString(string cipherText, string? ivKey = null, string? saltKey = null)
    {
        return DecryptAesInternal(cipherText.FromBase64String(), ivKey, saltKey).AsString();
    }

    public int DecryptAESToInt(string cipherText, string? ivKey = null, string? saltKey = null)
    {
        var bytes = DecryptAesInternal(cipherText.FromBase64String(), ivKey, saltKey);
        return BitConverter.ToInt32(bytes, 0);
    }

    public long DecryptAESToLong(string cipherText, string? ivKey = null, string? saltKey = null)
    {
        var bytes = DecryptAesInternal(cipherText.FromBase64String(), ivKey, saltKey);
        return BitConverter.ToInt64(bytes, 0);
    }

    public byte[] DecryptAESToBytes(string cipherText, string? ivKey = null, string? saltKey = null)
    {
        return DecryptAesInternal(cipherText.FromBase64String(), ivKey, saltKey);
    }

    
    private string ToHashInternal(byte[] input, HashAlgorithm algo, string? saltKey = null)
    {
        var combined = input;
        if (!string.IsNullOrEmpty(saltKey))
        {
            var saltBytes = saltKey.ToBytes();
            combined = new byte[input.Length + saltBytes.Length];
            Buffer.BlockCopy(input, 0, combined, 0, input.Length);
            Buffer.BlockCopy(saltBytes, 0, combined, input.Length, saltBytes.Length);
        }
        var hash = algo.ComputeHash(combined);
        return hash.ToBase64String();
    }
    
    public string ToMD5(string input, string? saltKey = null) => ToHashInternal(input.ToBytes(), MD5.Create(), saltKey);

    public string ToMD5(byte[] input, string? saltKey = null) => ToHashInternal(input, MD5.Create(), saltKey);

    public bool VerifyMD5(string input, string hash, string? saltKey = null) => ToMD5(input, saltKey) == hash;

    public bool VerifyMD5(byte[] input, string hash, string? saltKey = null) => ToMD5(input, saltKey) == hash;
    

    public string ToSHA1(string input, string? saltKey = null) => ToHashInternal(input.ToBytes(), SHA1.Create(), saltKey);

    public string ToSHA1(byte[] input, string? saltKey = null) => ToHashInternal(input, SHA1.Create(), saltKey);


    public bool VerifySHA1(string input, string hash, string? saltKey = null) => ToSHA1(input, saltKey) == hash;

    public bool VerifySHA1(byte[] input, string hash, string? saltKey = null) => ToSHA1(input, saltKey) == hash;
    

    public string ToSHA256(string input, string? saltKey = null) => ToHashInternal(input.ToBytes(), SHA256.Create(), saltKey);

    public string ToSHA256(byte[] input, string? saltKey = null) => ToHashInternal(input, SHA256.Create(), saltKey);

    public bool VerifySHA256(string input, string hash, string? saltKey = null) => ToSHA256(input, saltKey) == hash;

    public bool VerifySHA256(byte[] input, string hash, string? saltKey = null) => ToSHA256(input, saltKey) == hash;

    public string ToSHA384(string input, string? saltKey = null) => ToHashInternal(input.ToBytes(), SHA384.Create(), saltKey);

    public string ToSHA384(byte[] input, string? saltKey = null) => ToHashInternal(input, SHA384.Create(), saltKey);
    

    public bool VerifySHA384(string input, string hash, string? saltKey = null) => ToSHA384(input, saltKey) == hash;

    public bool VerifySHA384(byte[] input, string hash, string? saltKey = null) => ToSHA384(input, saltKey) == hash;

    public string ToSHA512(string input, string? saltKey = null) => ToHashInternal(input.ToBytes(), SHA512.Create(), saltKey);

    public string ToSHA512(byte[] input, string? saltKey = null) => ToHashInternal(input, SHA512.Create(), saltKey);
   

    public bool VerifySHA512(string input, string hash, string? saltKey = null) => ToSHA512(input, saltKey) == hash;

    public bool VerifySHA512(byte[] input, string hash, string? saltKey = null) => ToSHA512(input, saltKey) == hash;
}