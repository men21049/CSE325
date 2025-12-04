namespace DocumentManagementSystem.Model
{
    public class DocumentModel
    {
        public int DocumentID { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public DateTime UploadDate { get; set; }
        public int OfficeID { get; set; }
        public string OfficeName { get; set; } = string.Empty;
        
        // Propiedades de compatibilidad
        public string Office => OfficeName;
        public DateTime DateUploaded => UploadDate;
    }
}