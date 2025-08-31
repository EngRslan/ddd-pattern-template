namespace Engrslan;

public static class IntegersExtensions
{
    public static string ToReadableSize(this int size)
    {
        return ToReadableSize((long)size);
    }
    
    public static string ToReadableSize(this long size)
    {
        string[] sizeSuffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
        
        if (size < 0) 
            return "-" + ToReadableSize(-size);
        
        if (size == 0) 
            return "0 B";
        
        int magnitude = (int)Math.Log(size, 1024);
        decimal adjustedSize = (decimal)size / (1L << (magnitude * 10));
        
        if (Math.Round(adjustedSize, 2) >= 1000)
        {
            magnitude += 1;
            adjustedSize /= 1024;
        }
        
        return string.Format("{0:n2} {1}", adjustedSize, sizeSuffixes[magnitude]);
    }
}