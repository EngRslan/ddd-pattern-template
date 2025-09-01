using Engrslan.DependencyInjection;
using Engrslan.Files.Entities;
using Engrslan.Repositories;
using Engrslan.Services;
using Microsoft.EntityFrameworkCore;

namespace Engrslan.Files.Repositories;

public class FileRecordRepository : Repository<FileRecord,long>, IFileRecordRepository, IScopedService
{
    private readonly IDateTimeService _dateTimeService;

    public FileRecordRepository(ApplicationDataContext context, IDateTimeService dateTimeService) : base(context)
    {
        _dateTimeService = dateTimeService;
    }

    public async Task<ICollection<FileRecord>> GetExpired(TimeSpan tempFileMaxAge)
    {
        var expirationDate = _dateTimeService.Now.Subtract(tempFileMaxAge);
        return await Context.Files
            .Where(x => x.IsTemporary && x.CreatedAt < expirationDate)
            .ToListAsync();
    }
}