using API.Controllers;
using API.DTOs;
using DAOs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;



namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly FUNewsManagementContext _context;
    public CategoryController(FUNewsManagementContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _context.Categories.ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(short id)
    {
        var cat = await _context.Categories.FindAsync(id);
        return cat == null ? NotFound() : Ok(cat);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? name)
    {
        var query = _context.Categories.AsQueryable();
        if (!string.IsNullOrEmpty(name)) query = query.Where(c => c.CategoryName.Contains(name));
        return Ok(await query.ToListAsync());
    }

    [HttpPost]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> Create([FromBody] CategoryDTO dto)
    {
        var cat = new Category
        {
            CategoryName = dto.CategoryName,
            CategoryDesciption = dto.CategoryDesciption,
            ParentCategoryId = dto.ParentCategoryId,
            IsActive = dto.IsActive
        };
        _context.Categories.Add(cat);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = cat.CategoryId }, cat);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> Update(short id, [FromBody] CategoryDTO dto)
    {
        var cat = await _context.Categories.FindAsync(id);
        if (cat == null) return NotFound();
        cat.CategoryName = dto.CategoryName;
        cat.CategoryDesciption = dto.CategoryDesciption;
        cat.ParentCategoryId = dto.ParentCategoryId;
        cat.IsActive = dto.IsActive;
        await _context.SaveChangesAsync();
        return Ok(cat);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> Delete(short id)
    {
        var cat = await _context.Categories.FindAsync(id);
        if (cat == null) return NotFound();
        bool hasNews = await _context.NewsArticles.AnyAsync(n => n.CategoryId == id);
        if (hasNews) return BadRequest(new { message = "Category đang được dùng, không thể xóa." });
        _context.Categories.Remove(cat);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
