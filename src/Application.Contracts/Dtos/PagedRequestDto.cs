using System.ComponentModel;

namespace Engrslan.Dtos;

public abstract class PagedRequestDto
{
    [DefaultValue(1)] 
    public int Page { get; set; } = 1;
    
    [DefaultValue(10)]
    public int PageSize { get; set; } = 10;
}

public abstract class PagedAndSortedRequestDto : PagedRequestDto
{
    public string? Sorting { get; set; }
}