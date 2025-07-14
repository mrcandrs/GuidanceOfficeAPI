namespace GuidanceOfficeAPI.Dtos
{
    public class LoginDto
    {
        public int StudentId { get; set; }
        public string Name { get; set; } // Must not be null
        public string StudentNumber { get; set; }
        public string Program { get; set; }
        public string YearLevel { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public DateTime DateRegistered { get; set; }
    }

}
