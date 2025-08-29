using Engrslan.Dtos;
using Engrslan.Interfaces;
using Engrslan.Sample.Dtos;
using Engrslan.Sample.Entities;

namespace Engrslan.Mappers;

public static class PagedMappers
{
    public static PagedResultDto<TDto> ToPagedResultDto<TDto,TEntity>(this PagedResult<TEntity> pagedResult, Func<TEntity, TDto> mapper)
    {
        return new PagedResultDto<TDto>
        {
            Items = pagedResult.Items.Select(mapper),
            TotalCount = pagedResult.TotalCount,
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize
        };
    }
}