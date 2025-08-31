using Engrslan.DependencyInjection;
using Engrslan.Files.Entities;
using Engrslan.Repositories;

namespace Engrslan.Files.Repositories;

public class FileRecordRepository : Repository<FileRecord,long>, IFileRecordRepository, IScopedService
{
    public FileRecordRepository(ApplicationDataContext context) : base(context)
    {
    }
}