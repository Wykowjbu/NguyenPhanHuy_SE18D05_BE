
using API.DTOs;
using DAOs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AccountController : ControllerBase
{
    private readonly FUNewsManagementContext _context;

    public AccountController(FUNewsManagementContext context) => _context = context;

    [HttpGet]
    [EnableQuery]
    public ActionResult<IQueryable<SystemAccount>> GetAll()
    {
        return Ok(_context.SystemAccounts);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(short id)
    {
        var acc = await _context.SystemAccounts.FindAsync(id);
        return acc == null ? NotFound() : Ok(acc);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? name, [FromQuery] string? email)
    {
        var query = _context.SystemAccounts.AsQueryable();
        if (!string.IsNullOrEmpty(name)) query = query.Where(a => a.AccountName!.Contains(name));
        if (!string.IsNullOrEmpty(email)) query = query.Where(a => a.AccountEmail!.Contains(email));
        return Ok(await query.ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AccountDTO dto)
    {
        if (await _context.SystemAccounts.AnyAsync(a => a.AccountEmail == dto.AccountEmail))
            return BadRequest(new { message = "Email đã tồn tại." });

        // Tự sinh ID: lấy max ID hiện tại + 1
        short nextId = 1;
        if (await _context.SystemAccounts.AnyAsync())
        {
            nextId = (short)(await _context.SystemAccounts.MaxAsync(a => a.AccountId) + 1);
        }

        var acc = new SystemAccount
        {
            AccountId = nextId,
            AccountName = dto.AccountName,
            AccountEmail = dto.AccountEmail,
            AccountRole = dto.AccountRole,
            AccountPassword = dto.AccountPassword
        };
        _context.SystemAccounts.Add(acc);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = acc.AccountId }, acc);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(short id, [FromBody] AccountDTO dto)
    {
        var acc = await _context.SystemAccounts.FindAsync(id);
        if (acc == null) return NotFound();
        acc.AccountName = dto.AccountName;
        acc.AccountEmail = dto.AccountEmail;
        acc.AccountRole = dto.AccountRole;
        acc.AccountPassword = dto.AccountPassword;
        await _context.SaveChangesAsync();
        return Ok(acc);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(short id)
    {
        var acc = await _context.SystemAccounts.FindAsync(id);
        if (acc == null) return NotFound();
        bool hasNews = await _context.NewsArticles.AnyAsync(n => n.CreatedById == id);
        if (hasNews) return BadRequest(new { message = "Tài khoản đã có bài viết, không thể xóa." });
        _context.SystemAccounts.Remove(acc);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // Report: thống kê bài viết theo khoảng thời gian (Admin)
    [HttpGet("report")]
    public async Task<IActionResult> Report([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var data = await _context.NewsArticles
            .Where(n => n.CreatedDate >= startDate && n.CreatedDate <= endDate)
            .OrderByDescending(n => n.CreatedDate)
            .Include(n => n.CreatedBy)
            .Select(n => new {
                n.NewsArticleId,
                n.NewsTitle,
                n.CreatedDate,
                CreatedBy = n.CreatedBy!.AccountName,
                n.NewsStatus
            })
            .ToListAsync();
        return Ok(data);
    }
}
