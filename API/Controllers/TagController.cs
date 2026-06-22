
using API.DTOs;
using DAOs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TagController : ControllerBase
{
    private readonly FUNewsManagementContext _context;
    public TagController(FUNewsManagementContext context) => _context = context;

    [HttpGet]
    [EnableQuery]
    public ActionResult<IQueryable<Tag>> GetAll()
    {
        return Ok(_context.Tags);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var tag = await _context.Tags.FindAsync(id);
        return tag == null ? NotFound() : Ok(tag);
    }

    [HttpPost]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> Create([FromBody] TagDTO dto)
    {
        // Tự sinh ID: lấy max ID hiện tại + 1
        int nextId = 1;
        if (await _context.Tags.AnyAsync())
        {
            nextId = await _context.Tags.MaxAsync(t => t.TagId) + 1;
        }

        var tag = new Tag
        {
            TagId = nextId,
            TagName = dto.TagName,
            Note = dto.Note
        };
        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = tag.TagId }, tag);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> Update(int id, [FromBody] TagDTO dto)
    {
        var tag = await _context.Tags.FindAsync(id);
        if (tag == null) return NotFound();
        tag.TagName = dto.TagName;
        tag.Note = dto.Note;
        await _context.SaveChangesAsync();
        return Ok(tag);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> Delete(int id)
    {
        var tag = await _context.Tags.FindAsync(id);
        if (tag == null) return NotFound();
        _context.Tags.Remove(tag);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
