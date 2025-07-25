namespace GuidanceOfficeAPI.Dtos
{
    public class JournalEntryDto
    {
        public int StudentId { get; set; }
        public string Date { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Mood { get; set; }
    }

}
