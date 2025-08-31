using Engrslan.Files.Dtos;
using Engrslan.Files.Entities;
using Engrslan.Mappers;

namespace Engrslan.Files.Mappers;

public static class FileRecordMappers
{
    public static FileRecordDto ToDto(this FileRecord fileRecord)
    {
        var dto = new FileRecordDto
        {
            FileName = fileRecord.FileName,
            ContentType = fileRecord.ContentType,
            Size = fileRecord.Size,
            Description = fileRecord.Description,
            IsTemporary = fileRecord.IsTemporary
        };
        
        dto.MapFullAuditedFrom(fileRecord);

        return dto;
    }
}