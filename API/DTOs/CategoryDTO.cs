namespace API.DTOs;
public class CategoryDTO
{
    public string CategoryName { get; set; } = null!;
    public string CategoryDesciption { get; set; } = null!;
    public short? ParentCategoryId { get; set; }
    public bool? IsActive { get; set; }
}