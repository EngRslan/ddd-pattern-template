using Engrslan.Files.Dtos;
using Engrslan.Files.Services;
using Engrslan.Types;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Engrslan.Files;
public class GetUploadedFileMetadataByIdRequest
{
    public EncryptedLong Id { get; set; }
}
public class GetUploadedFileMetadataById : Endpoint<GetUploadedFileMetadataByIdRequest,FileRecordDto>
{
    private readonly IFileRecordAppService _fileRecordAppService;

    public GetUploadedFileMetadataById(IFileRecordAppService fileRecordAppService)
    {
        _fileRecordAppService = fileRecordAppService;
    }

    public override void Configure()
    {
        Get("/files/{id}/metadata");
        AllowAnonymous();
        Options(x=>x.WithTags("Files"));
        Summary(x =>
        {
            x.Summary = "Get file metadata by ID";
            x.Description = "Gets a single file metadata by its ID";
            x.Responses[200] = "Success - Returns the file metadata";
            x.Responses[404] = "Not Found - File not found";
        });
    }

    public override async Task HandleAsync(GetUploadedFileMetadataByIdRequest req, CancellationToken ct)
    {
        var file = await _fileRecordAppService.GetFileMetadata(req.Id, ct);
        if (file.IsError)
        {
            await Send.NotFoundAsync(ct);
        }
        
        await Send.OkAsync(file.Value, ct);
        
    }
}