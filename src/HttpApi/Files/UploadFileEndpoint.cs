using Engrslan.Files.Dtos;
using Engrslan.Files.Services;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Engrslan.Files;

public class UploadFileEndpoint : Endpoint<UploadFileRequest,FileRecordDto>
{
    private readonly IFileRecordAppService _fileAppService;

    public UploadFileEndpoint(IFileRecordAppService fileAppService)
    {
        _fileAppService = fileAppService;
    }
    public override void Configure()
    {
        Post("/files");
        Options(o=>o.WithTags("Files").WithName("Upload File").WithDescription("Upload a temp file to the server."));
        AllowAnonymous();
        AllowFileUploads();
    }

    public override async Task HandleAsync(UploadFileRequest req, CancellationToken ct)
    {
        var file = req.File;

        var uploadedFileResult = await _fileAppService.UploadTempFile(file.FileName,file.ContentType,file.OpenReadStream(),null, ct);
        if (uploadedFileResult.IsError)
        {
            foreach (var error in uploadedFileResult.Errors)
            {
                AddError(error.Code,error.Description);
            }
        }
        
        ThrowIfAnyErrors();
        
        await Send.OkAsync(uploadedFileResult.Value, ct);
    }
}

public class UploadFileRequest
{
    public IFormFile File { get; set; } = null!;
}