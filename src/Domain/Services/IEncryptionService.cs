

namespace Engrslan.Services;

public interface IEncryptionService
{
     // ===== Two-way Encryption (AES) =====
        string EncryptAES(string plainText, string ivKey = null, string saltKey = null);
        string EncryptAES(int value, string ivKey = null, string saltKey = null);
        string EncryptAES(long value, string ivKey = null, string saltKey = null);
        string EncryptAES(byte[] value, string ivKey = null, string saltKey = null);

        string DecryptAESToString(string cipherText, string ivKey = null, string saltKey = null);
        int DecryptAESToInt(string cipherText, string ivKey = null, string saltKey = null);
        long DecryptAESToLong(string cipherText, string ivKey = null, string saltKey = null);
        byte[] DecryptAESToBytes(string cipherText, string ivKey = null, string saltKey = null);

        // ===== Hashing (One-way) =====
        string ToMD5(string input, string saltKey = null);
        string ToMD5(byte[] input, string saltKey = null);
        bool VerifyMD5(string input, string hash, string saltKey = null);
        bool VerifyMD5(byte[] input, string hash, string saltKey = null);

        string ToSHA1(string input, string saltKey = null);
        string ToSHA1(byte[] input, string saltKey = null);
        bool VerifySHA1(string input, string hash, string saltKey = null);
        bool VerifySHA1(byte[] input, string hash, string saltKey = null);

        string ToSHA256(string input, string saltKey = null);
        string ToSHA256(byte[] input, string saltKey = null);
        bool VerifySHA256(string input, string hash, string saltKey = null);
        bool VerifySHA256(byte[] input, string hash, string saltKey = null);

        string ToSHA384(string input, string saltKey = null);
        string ToSHA384(byte[] input, string saltKey = null);
        bool VerifySHA384(string input, string hash, string saltKey = null);
        bool VerifySHA384(byte[] input, string hash, string saltKey = null);

        string ToSHA512(string input, string saltKey = null);
        string ToSHA512(byte[] input, string saltKey = null);
        bool VerifySHA512(string input, string hash, string saltKey = null);
        bool VerifySHA512(byte[] input, string hash, string saltKey = null);
}

