namespace Engrslan.Services;

public interface IFileStorageService
{
    Task UploadAsync(string path, Stream data);
    Task<Stream> DownloadAsync(string path);
    Task DeleteAsync(string path);
    Task<IEnumerable<string>> ListFilesAsync(string path);
    Task<string> GetPresignedUrlAsync(string path, TimeSpan validFor);
}