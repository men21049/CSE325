using DocumentManagementSystem.Model;

namespace DocumentManagementSystem.Services
{
    public class DocumentService
    {
        private List<DocumentModel> Documents = new();

        public IEnumerable<DocumentModel> GetAll() => Documents;

        public void AddDocument(DocumentModel doc)
        {
            Documents.Add(doc);
        }

        public void DeleteDocument(DocumentModel doc)
        {
            Documents.Remove(doc);
        }

        public IEnumerable<DocumentModel> Search(string keyword)
        {
            return Documents.Where(d =>
                d.FileName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                d.Office.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }
    }
}