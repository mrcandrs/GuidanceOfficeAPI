namespace GuidanceOfficeAPI.Dtos
{
    public class StudentUpdateWithImageDto
    {
        public int StudentId { get; set; }
        public string Email { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;

        public IFormFile? ProfileImage { get; set; }
    }

}
