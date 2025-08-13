using GuidanceOfficeAPI.Data;
using GuidanceOfficeAPI.Dtos;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize] // everything here requires a valid JWT unless an action overrides it
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
            try
            {
                Console.WriteLine($"Login attempt for {dto.Email}");

                var counselor = await _context.Counselors
                    .FirstOrDefaultAsync(c => c.Email == dto.Email);

                if (counselor == null)
                {
                    Console.WriteLine("Counselor not found.");
                    return Unauthorized(new { message = "Invalid email or password." });
                }

                if (counselor.Password != dto.Password)
                {
                    Console.WriteLine("Invalid password.");
                    return Unauthorized(new { message = "Invalid email or password." });
                }

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

                Console.WriteLine("Login successful.");

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    counselor = new
                    {
                        id = counselor.CounselorId,
                        name = counselor.Name,
                        email = counselor.Email
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Login exception: " + ex.Message);
                return StatusCode(500, new { message = "Server error", error = ex.Message });
            }
        }

        // GET: api/counselor/me
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentCounselor()
        {
            try
            {
                var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(idStr)) return Unauthorized(new { message = "Invalid token." });

                int counselorId = int.Parse(idStr);

                var counselor = await _context.Counselors
                    .Where(c => c.CounselorId == counselorId)
                    .Select(c => new
                    {
                        id = c.CounselorId,
                        email = c.Email,
                        ame = c.Name,
                    })
                    .FirstOrDefaultAsync();

                if (counselor == null)
                    return NotFound(new { message = "Counselor not found." });

                return Ok(counselor);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Server error", error = ex.Message });
            }
        }

    }
}
