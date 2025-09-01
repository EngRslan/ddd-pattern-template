using Engrslan.Files.Dtos;
using ErrorOr;

namespace Engrslan.Files.Services;

public interface IFileRecordAppService
{
    Task<ErrorOr<FileRecordDto>> UploadTempFile(string fileName,string contentType, Stream file,string? description = null, CancellationToken token = default);
    Task<ErrorOr<FileRecordDto>> GetFileMetadata(long id, CancellationToken token = default);
    Task<int> Purge(CancellationToken token = default);
}