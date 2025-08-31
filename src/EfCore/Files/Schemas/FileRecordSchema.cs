using Engrslan.Files.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Engrslan.Files.Schemas;

public class FileRecordSchema : IEntityTypeConfiguration<FileRecord>
{
    public void Configure(EntityTypeBuilder<FileRecord> builder)
    {
        builder.Config("Files");
        
        builder.Property(f => f.FilePath)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(f => f.FileName)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.Property(f => f.ContentType)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(f => f.Size)
            .IsRequired();
        
        builder.Property(f => f.Description)
            .HasMaxLength(1000);
        
        builder.Property(f => f.IsTemporary)
            .IsRequired()
            .HasDefaultValue(false);
        
        builder.HasIndex(f => f.FileName)
            .HasDatabaseName("IX_Files_FileName");
        
        builder.HasIndex(f => f.IsTemporary)
            .HasDatabaseName("IX_Files_IsTemporary");
        
        builder.HasIndex(f => f.CreatedAt)
            .HasDatabaseName("IX_Files_CreatedOn");
    }
}