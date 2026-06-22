

using System.Text.Json;
using API.DTOs;
using DAOs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly FUNewsManagementContext _context;
    private readonly IConfiguration _config;

    public AuthController(FUNewsManagementContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO dto)
    {
        // Check admin account từ appsettings
        var adminEmail = _config["AdminAccount:Email"];
        var adminPassword = _config["AdminAccount:Password"];

        string role;
        short accountId;
        string accountName;

        if (dto.Email == adminEmail && dto.Password == adminPassword)
        {
            role = "Admin";
            accountId = 0;
            accountName = "Admin";
        }
        else
        {
            var account = await _context.SystemAccounts
                .FirstOrDefaultAsync(a => a.AccountEmail == dto.Email && a.AccountPassword == dto.Password);

            if (account == null)
                return Unauthorized(new { message = "Email hoặc mật khẩu không đúng." });

            role = account.AccountRole == 1 ? "Staff" : "Lecturer";
            accountId = account.AccountId;
            accountName = account.AccountName ?? "";
        }

        var token = GenerateToken(accountId, accountName, dto.Email, role);
        return Ok(new { token, role, accountId, accountName });
    }

    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDTO dto)
    {
        var clientId = _config["Google:ClientId"];

        using var http = new HttpClient();
        var verifyResponse = await http.GetAsync($"https://oauth2.googleapis.com/tokeninfo?id_token={dto.IdToken}");
        if (!verifyResponse.IsSuccessStatusCode)
            return Unauthorized(new { message = "Google token không hợp lệ." });

        var json = await verifyResponse.Content.ReadAsStringAsync();
        var payload = JsonSerializer.Deserialize<JsonElement>(json);

        var aud = payload.GetProperty("aud").GetString();
        if (aud != clientId)
            return Unauthorized(new { message = "Token không thuộc về ứng dụng này." });

        var email = payload.GetProperty("email").GetString()!;
        var name = payload.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? email : email;
        var sub = payload.GetProperty("sub").GetString()!;

        var account = await _context.SystemAccounts
            .FirstOrDefaultAsync(a => a.AccountEmail == email);

        if (account == null)
        {
            short newId = 1;
            if (await _context.SystemAccounts.AnyAsync())
                newId = (short)(await _context.SystemAccounts.MaxAsync(a => a.AccountId) + 1);

            account = new SystemAccount
            {
                AccountId = newId,
                AccountEmail = email,
                AccountName = name,
                AccountRole = 1,
                AccountPassword = "google-" + sub.Substring(0, Math.Min(sub.Length, 30))
            };
            _context.SystemAccounts.Add(account);
            await _context.SaveChangesAsync();
        }

        string role = account.AccountRole == 1 ? "Staff" : "Lecturer";
        var token = GenerateToken(account.AccountId, account.AccountName ?? name, email, role);
        return Ok(new { token, role, accountId = (int)account.AccountId, accountName = account.AccountName ?? name });
    }

    private string GenerateToken(short accountId, string accountName, string email, string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, accountId.ToString()),
            new Claim(ClaimTypes.Name, accountName),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role)
        };
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpireMinutes"]!)),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
