using Engrslan.Files.Entities;
using Engrslan.Interfaces;

namespace Engrslan.Files.Repositories;

public interface IFileRecordRepository : IRepository<FileRecord, long>
{
    Task<ICollection<FileRecord>> GetExpired(TimeSpan tempFileMaxAge);
}