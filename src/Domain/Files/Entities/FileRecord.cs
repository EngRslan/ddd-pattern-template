using Engrslan.Entities;

namespace Engrslan.Files.Entities;

public class FileRecord : FullAuditedEntity<long>
{
    public string FilePath { get; private set; } = null!;
    public string FileName { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;
    public long Size { get; private set; }
    public string? Description { get; private set; }
    public bool IsTemporary { get; private set; }

    public static FileRecord CreateTemp(string filePath,string fileName,string contentType,long size, string? description = null)
    {
        return new FileRecord
        {
            FilePath = filePath,
            FileName = fileName,
            ContentType = contentType,
            Size = size,
            Description = description,
            IsTemporary = true
        };
    }

    public static FileRecord Create(string filePath,string fileName, string contentType, long size,
        string? description = null)
    {
        return new FileRecord
        {
            FilePath = filePath,
            FileName = fileName,
            ContentType = contentType,
            Size = size,
            Description = description,
            IsTemporary = false
        };
    }
}