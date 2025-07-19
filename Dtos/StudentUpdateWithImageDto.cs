using Microsoft.AspNetCore.Mvc;

namespace GuidanceOfficeAPI.Dtos
{
    public class StudentUpdateWithImageDto
    {
        [FromForm(Name = "studentId")] //avoiding multipart errors
        public int StudentId { get; set; }
        [FromForm(Name = "email")]
        public string Email { get; set; } = null!;
        [FromForm(Name = "username")]
        public string Username { get; set; } = null!;
        [FromForm(Name = "password")]
        public string? Password { get; set; } // <-- nullable!
        [FromForm(Name = "ProfileImage")]
        public IFormFile? ProfileImage { get; set; }
    }

}
