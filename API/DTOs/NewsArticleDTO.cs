namespace API.DTOs;
public class NewsArticleDTO
{
    public string? NewsTitle { get; set; }
    public string Headline { get; set; } = null!;
    public string? NewsContent { get; set; }
    public string? NewsSource { get; set; }
    public short? CategoryId { get; set; }
    public bool? NewsStatus { get; set; }
    public List<int> TagIds { get; set; } = new();
}