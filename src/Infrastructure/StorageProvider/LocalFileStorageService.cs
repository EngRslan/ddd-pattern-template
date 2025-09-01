using Engrslan.Services;
using Microsoft.Extensions.Configuration;

namespace Engrslan.StorageProvider;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;
    private readonly IConfiguration _configuration;
    private readonly IEncryptionService _encryptionService;

    public LocalFileStorageService(IConfiguration configuration, IEncryptionService encryptionService)
    {
        _configuration = configuration;
        _encryptionService = encryptionService;
        
        var configuredPath = _configuration["Storage:LocalFile:BasePath"] ;
        _basePath = string.IsNullOrWhiteSpace(configuredPath) 
            ? Path.Combine(Directory.GetCurrentDirectory(), "Storage") 
            : Path.IsPathFullyQualified(configuredPath) 
                ? configuredPath 
                : Path.Combine(Directory.GetCurrentDirectory(), configuredPath);
        
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }
    }

    public async Task UploadAsync(string path, Stream data)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or empty", nameof(path));
        
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        var fullPath = GetFullPath(path);
        var directory = Path.GetDirectoryName(fullPath);
        
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
        await data.CopyToAsync(fileStream);
        await fileStream.FlushAsync();
    }

    public async Task<Stream> DownloadAsync(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or empty", nameof(path));

        var fullPath = GetFullPath(path);
        
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"File not found: {path}", fullPath);

        var memoryStream = new MemoryStream();
        await using (var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
        {
            await fileStream.CopyToAsync(memoryStream);
        }
        memoryStream.Position = 0;
        return memoryStream;
    }

    public Task DeleteAsync(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or empty", nameof(path));

        var fullPath = GetFullPath(path);
        
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
        else if (Directory.Exists(fullPath))
        {
            Directory.Delete(fullPath, true);
        }

        return Task.CompletedTask;
    }

    public Task<IEnumerable<string>> ListFilesAsync(string path)
    {
        var searchPath = string.IsNullOrWhiteSpace(path) ? _basePath : GetFullPath(path);
        
        if (!Directory.Exists(searchPath))
        {
            return Task.FromResult(Enumerable.Empty<string>());
        }

        var files = Directory.GetFiles(searchPath, "*", SearchOption.AllDirectories)
            .Select(GetRelativePath)
            .Where(f => !string.IsNullOrEmpty(f));

        return Task.FromResult(files);
    }

    public Task<string> GetPresignedUrlAsync(string path, TimeSpan validFor)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or empty", nameof(path));

        var fullPath = GetFullPath(path);
        
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"File not found: {path}", fullPath);

        var expiry = DateTimeOffset.UtcNow.Add(validFor);
        var token = GenerateAccessToken(path, expiry).ToUrlSafeBase64();
        
        var baseUrl = _configuration["Storage:LocalFile:BaseUrl"] ?? "http://localhost";
        var presignedUrl = $"{baseUrl}/storage/{path}?token={token}&expires={expiry.ToUnixTimeSeconds()}";
        
        return Task.FromResult(presignedUrl);
    }

    private string GetFullPath(string path)
    {
        path = path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        
        while (path.StartsWith(Path.DirectorySeparatorChar))
        {
            path = path.Substring(1);
        }
        
        var fullPath = Path.Combine(_basePath, path);
        var normalizedPath = Path.GetFullPath(fullPath);
        
        if (!normalizedPath.StartsWith(Path.GetFullPath(_basePath), StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException($"Access to path '{path}' is denied");
        }
        
        return normalizedPath;
    }

    private string GetRelativePath(string fullPath)
    {
        var basePath = Path.GetFullPath(_basePath);
        var normalizedPath = Path.GetFullPath(fullPath);
        
        if (!normalizedPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }
        
        var relativePath = normalizedPath.Substring(basePath.Length);
        if (relativePath.StartsWith(Path.DirectorySeparatorChar))
        {
            relativePath = relativePath.Substring(1);
        }
        
        return relativePath.Replace(Path.DirectorySeparatorChar, '/');
    }

    private string GenerateAccessToken(string path, DateTimeOffset expiry)
    {
        var data = $"{path}|{expiry.ToUnixTimeSeconds()}";
        return _encryptionService.EncryptAES(data);
    }
}