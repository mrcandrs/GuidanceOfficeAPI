namespace GuidanceOfficeAPI.Dtos
{
    public class RegisterStudentDto
    {
        public int StudentId { get; set; }
        public string StudentNumber { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public DateTime DateRegistered { get; set; }
    }

}
