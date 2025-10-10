using System.ComponentModel.DataAnnotations;

namespace GuidanceOfficeAPI.Dtos
{
    public class CounselorLoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        // Optional fields for session management
        public string? DeviceId { get; set; }
        public string? SessionId { get; set; }
    }

}
