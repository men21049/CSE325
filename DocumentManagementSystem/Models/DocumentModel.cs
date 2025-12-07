namespace DocumentManagementSystem.Models
{
    public class DocumentModel
    {
        public int DocumentId { get; set; }
        public string FileName { get; set; } = "";
        public int OfficeId { get; set; }
        public string OfficeName { get; set; } = ""; // Joined from Offices table
        public DateTime DateUploaded { get; set; }
        public string FilePath { get; set; } = "";
    }
}
