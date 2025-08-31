using Engrslan.Services;

namespace Engrslan.StorageProvider;

public class LocalFileStorageService : IFileStorageService
{
    public Task UploadAsync(string path, Stream data)
    {
        throw new NotImplementedException();
    }

    public Task<Stream> DownloadAsync(string path)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(string path)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<string>> ListFilesAsync(string path)
    {
        throw new NotImplementedException();
    }

    public Task<string> GetPresignedUrlAsync(string path, TimeSpan validFor)
    {
        throw new NotImplementedException();
    }
}