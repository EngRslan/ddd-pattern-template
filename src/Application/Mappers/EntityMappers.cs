using Engrslan.Dtos;
using Engrslan.Interfaces;
using Engrslan.Types;

namespace Engrslan.Mappers;

public static class EntityMappers
{
    public static void MapFullAuditedBaseFrom(this FullAuditedEntityDto entityDto, IFullAuditedEntity entityBase)
    {
        entityDto.CreatedBy = entityBase.CreatedBy;
        entityDto.CreatedAt = entityBase.CreatedAt;
        entityDto.ModifiedBy = entityBase.ModifiedBy;
        entityDto.ModifiedAt = entityBase.ModifiedAt;
        entityDto.DeletedBy = entityBase.DeletedBy;
        entityDto.DeletedAt = entityBase.DeletedAt;
        entityDto.IsDeleted = entityBase.IsDeleted;
    }
    public static void MapFullAuditedFrom<TEntityKey>(this FullAuditedEntityDto<TEntityKey> entityDto, IFullAuditedEntity<TEntityKey> entityBase)
    {
        entityDto.Id = entityBase.Id;
        MapFullAuditedBaseFrom(entityDto, entityBase);
    }
    
    public static void MapFullAuditedFrom(this FullAuditedEntityDto<EncryptedInt> entityDto, IFullAuditedEntity<int> entityBase)
    {
        entityDto.Id = entityBase.Id;
        MapFullAuditedBaseFrom(entityDto, entityBase);
    }
    
    public static void MapFullAuditedFrom(this FullAuditedEntityDto<EncryptedLong> entityDto, IFullAuditedEntity<long> entityBase)
    {
        entityDto.Id = entityBase.Id;
        MapFullAuditedBaseFrom(entityDto, entityBase);
    }
}