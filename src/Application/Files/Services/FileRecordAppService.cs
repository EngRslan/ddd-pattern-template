using Engrslan.DependencyInjection;
using Engrslan.Files.Dtos;
using Engrslan.Files.Entities;
using Engrslan.Files.Mappers;
using Engrslan.Files.Repositories;
using Engrslan.Services;
using ErrorOr;
using Microsoft.Extensions.Configuration;

namespace Engrslan.Files.Services;

public class FileRecordAppService : IFileRecordAppService, IScopedService
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IConfiguration _configuration;
    private readonly IFileRecordRepository _fileRecordRepository;
    private readonly TimeSpan _presignedLinkExpirationTimeSpan;

    public FileRecordAppService(IFileStorageService fileStorageService,IConfiguration configuration,IFileRecordRepository fileRecordRepository)
    {
        _fileStorageService = fileStorageService;
        _configuration = configuration;
        _fileRecordRepository = fileRecordRepository;
        _presignedLinkExpirationTimeSpan = configuration.GetValue<TimeSpan?>("FileUpload:PresignedUrlExpiration") ?? TimeSpan.FromMinutes(15);
    }

    private ErrorOr<bool> ValidateFileRequirements(Stream file, string fileName, string contentType)
    {
        var maxFileSize = _configuration.GetValue<long?>("FileUpload:MaxUploadFileSize") ?? 1024 * 1024 * 1024;
        var allowedExtensions = _configuration.GetSection("FileUpload:AllowedExtensions").Get<string[]>() ?? [".jpg", ".pdf"];
        var allowedTypes = _configuration.GetSection("FileUpload:AllowedTypes").Get<string[]>() ?? ["image/jpeg", "application/pdf"];
        var errors = new List<Error>();

        if (file.Length > maxFileSize)
        {
            errors.Add(Error.Validation("MAX_FILE_SIZE_EXCEEDED",
                $"File size must be less than {maxFileSize.ToReadableSize()}"));
        }
        
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
        {
            errors.Add(Error.Validation("EXTENSION_NOT_ALLOWED",
                $"Allowed extensions are: {string.Join(", ", allowedExtensions)}"));
        }

        if (!allowedTypes.Contains(contentType.ToLowerInvariant()))
        {
            errors.Add(Error.Validation("TYPE_NOT_ALLOWED",
                $"Allowed types are: {string.Join(", ", allowedTypes)}"));
        }

        return errors.Count > 0 ? ErrorOr<bool>.From(errors) : true;
    }
    
    public async Task<ErrorOr<FileRecordDto>> UploadTempFile(string fileName,string contentType, Stream file,string? description = null, CancellationToken token = default)
    {
         var validation = ValidateFileRequirements(file, fileName,  contentType);

        if (validation.IsError)
        {
            return ErrorOr<FileRecordDto>.From(validation.Errors);
        }

        var path = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
        var record = FileRecord.CreateTemp(path,fileName, contentType,file.Length,description);
        try
        {
            await _fileStorageService.UploadAsync(record.FilePath, file);
        }
        catch (Exception e)
        {
            return ErrorOr<FileRecordDto>.From([Error.Failure(e.Message)]);
        }
        
        await _fileRecordRepository.InsertAsync(record,true, token);
        
        var dto = record.ToDto();
        dto.DirectLink = await _fileStorageService.GetPresignedUrlAsync(record.FilePath, _presignedLinkExpirationTimeSpan);
        return dto;
    }

    public async Task<ErrorOr<FileRecordDto>> GetFileMetadata(long id, CancellationToken token = default)
    {
        var file = await _fileRecordRepository.GetByIdAsync(id,token);
        if (file is null)
        {
            return ErrorOr<FileRecordDto>.From([Error.NotFound("File not found")]);
        }
        
        var dto = file.ToDto();
        dto.DirectLink = await _fileStorageService.GetPresignedUrlAsync(file.FilePath, _presignedLinkExpirationTimeSpan);
        return dto;
    }

    public async Task<int> Purge(CancellationToken token = default)
    {
        var tempFileMaxAge = _configuration.GetValue<TimeSpan?>("FileUpload:TempFileMaxAge") ?? TimeSpan.FromHours(5);
        var expired = await _fileRecordRepository.GetExpired(tempFileMaxAge);
        foreach (var record in expired)
        {
            await _fileStorageService.DeleteAsync(record.FilePath);
            await _fileRecordRepository.DeleteAsync(record, false, token);
        }

        if (expired.Count > 0)
        {
            await _fileRecordRepository.SaveChangesAsync(token);
        }
        
        return expired.Count;
    }
}