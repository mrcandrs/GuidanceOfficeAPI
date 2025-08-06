using GuidanceOfficeAPI.Data;
using GuidanceOfficeAPI.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GuidanceOfficeAPI.Controllers
{
    [ApiController]
    [Route("api/counselor")]
    public class CounselorAuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public CounselorAuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] CounselorLoginDto dto)
        {
            var counselor = await _context.Counselors
                .FirstOrDefaultAsync(c => c.Email == dto.Email);

            if (counselor == null || counselor.Password != dto.Password)
                return Unauthorized(new { message = "Invalid email or password." });

            // Create JWT token
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, counselor.CounselorId.ToString()),
                new Claim(ClaimTypes.Email, counselor.Email),
                new Claim(ClaimTypes.Name, counselor.Name)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                name = counselor.Name
            });
        }
    }
}
