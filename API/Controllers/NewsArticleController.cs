
using API.DTOs;
using DAOs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsArticleController : ControllerBase
{
    private readonly FUNewsManagementContext _context;
    public NewsArticleController(FUNewsManagementContext context) => _context = context;

    // Public: xem bài active (không cần đăng nhập)
    [HttpGet("public")]
    [EnableQuery]
    public ActionResult<IQueryable<NewsArticle>> GetPublic()
    {
        return Ok(_context.NewsArticles.Where(n => n.NewsStatus == true));
    }

    // Lấy tất cả (có auth)
    [HttpGet]
    [Authorize]
    [EnableQuery]
    public ActionResult<IQueryable<NewsArticle>> GetAll()
    {
        return Ok(_context.NewsArticles);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var news = await _context.NewsArticles
            .Include(n => n.Category)
            .Include(n => n.CreatedBy)
            .Include(n => n.Tags)
            .FirstOrDefaultAsync(n => n.NewsArticleId == id);
        return news == null ? NotFound() : Ok(news);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? title, [FromQuery] short? categoryId)
    {
        var query = _context.NewsArticles
            .Include(n => n.Category)
            .Include(n => n.Tags)
            .AsQueryable();
        if (!string.IsNullOrEmpty(title)) query = query.Where(n => n.NewsTitle!.Contains(title));
        if (categoryId.HasValue) query = query.Where(n => n.CategoryId == categoryId);
        return Ok(await query.ToListAsync());
    }

    // Staff: xem lịch sử bài viết của mình
    [HttpGet("my-articles")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> MyArticles()
    {
        var userId = short.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var news = await _context.NewsArticles
            .Include(n => n.Category)
            .Where(n => n.CreatedById == userId)
            .OrderByDescending(n => n.CreatedDate)
            .ToListAsync();
        return Ok(news);
    }

    [HttpPost]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> Create([FromBody] NewsArticleDTO dto)
    {
        var userId = short.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Tự sinh ID: lấy max ID hiện tại + 1
        var maxId = await _context.NewsArticles
            .Select(n => n.NewsArticleId)
            .ToListAsync();
        var nextId = maxId
            .Select(id => int.TryParse(id, out var num) ? num : 0)
            .DefaultIfEmpty(0)
            .Max() + 1;

        var news = new NewsArticle
        {
            NewsArticleId = nextId.ToString(),
            NewsTitle = dto.NewsTitle,
            Headline = dto.Headline,
            NewsContent = dto.NewsContent,
            NewsSource = dto.NewsSource,
            CategoryId = dto.CategoryId,
            NewsStatus = dto.NewsStatus,
            CreatedDate = DateTime.Now,
            CreatedById = userId,
            UpdatedById = userId,
            ModifiedDate = DateTime.Now
        };
        _context.NewsArticles.Add(news);
        news.Tags = _context.Tags.Where(t => dto.TagIds.Contains(t.TagId)).ToList();

        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = news.NewsArticleId }, news);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> Update(string id, [FromBody] NewsArticleDTO dto)
    {
        var news = await _context.NewsArticles
            .Include(n => n.Tags)
            .FirstOrDefaultAsync(n => n.NewsArticleId == id);
        if (news == null) return NotFound();

        var userId = short.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        news.NewsTitle = dto.NewsTitle;
        news.Headline = dto.Headline;
        news.NewsContent = dto.NewsContent;
        news.NewsSource = dto.NewsSource;
        news.CategoryId = dto.CategoryId;
        news.NewsStatus = dto.NewsStatus;
        news.UpdatedById = userId;
        news.ModifiedDate = DateTime.Now;

        news.Tags.Clear();
        news.Tags = _context.Tags.Where(t => dto.TagIds.Contains(t.TagId)).ToList();

        await _context.SaveChangesAsync();
        return Ok(news);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> Delete(string id)
    {
        var news = await _context.NewsArticles
            .Include(n => n.Tags)
            .FirstOrDefaultAsync(n => n.NewsArticleId == id);
        if (news == null) return NotFound();
        news.Tags.Clear();
        _context.NewsArticles.Remove(news);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
