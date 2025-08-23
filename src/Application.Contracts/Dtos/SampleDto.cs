namespace Engrslan.Dtos;

public class CreateSampleDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Email { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class UpdateSampleDto : CreateSampleDto
{
    public int Id { get; set; }
}

public class SampleDto : EntityDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Email { get; set; } = string.Empty;
    public decimal Price { get; set; }
}