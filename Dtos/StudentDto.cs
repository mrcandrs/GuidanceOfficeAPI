namespace GuidanceOfficeAPI.Dtos
{
    // DTO for student data (if not already existing)
    public class StudentDto
    {
        public int StudentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string StudentNumber { get; set; } = string.Empty;
    }
}
