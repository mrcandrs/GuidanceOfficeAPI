using System.Text.Json.Serialization;

namespace GuidanceOfficeAPI.Dtos
{
    public class LoginDto
    {
        public int StudentId { get; set; }

        [JsonPropertyName("fullName")]
        public string FullName { get; set; }

        public string StudentNumber { get; set; }
        public string Program { get; set; }

        [JsonPropertyName("gradeYear")]
        public string YearLevel { get; set; }

        public string Username { get; set; }
        public string Email { get; set; }
        public DateTime DateRegistered { get; set; }
    }

}
