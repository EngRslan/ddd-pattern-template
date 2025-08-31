using System.Text;

namespace Engrslan;

public static class StringExtensions
{
    public static byte[] ToBytes(this string str) => Encoding.UTF8.GetBytes(str);
    public static byte[] FromBytes(this string str) => Encoding.UTF8.GetBytes(str);
    public static byte[] FromBase64String(this string str) => Convert.FromBase64String(str);
    public static string ToUrlSafeBase64(this string str) => str.Replace('+', '-').Replace('/', '_').TrimEnd('=');

    public static string FromBase64UrlSafe(this string str)
    {
        var base64 = str.Replace('-', '+').Replace('_', '/');
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }

        return base64;
    }
    
}