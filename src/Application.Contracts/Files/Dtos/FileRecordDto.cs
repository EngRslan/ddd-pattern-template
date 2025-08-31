using Engrslan.Dtos;
using Engrslan.Types;

namespace Engrslan.Files.Dtos;

public class FileRecordDto : FullAuditedEntityDto<EncryptedLong>
{
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long Size { get; set; }
    public string? Description { get; set; }
    public bool IsTemporary { get; set; }
}