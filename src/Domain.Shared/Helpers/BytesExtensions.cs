using System.Text;

namespace Engrslan;

public static class BytesExtensions
{
    public static string ToBase64String(this byte[] bytes) => Convert.ToBase64String(bytes);
    public static string AsString(this byte[] bytes) => Encoding.UTF8.GetString(bytes);
}