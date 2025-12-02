namespace DocumentManagementSystem.Services
{
    public class OfficeService
    {
        private List<string> Offices = new()
        {
            "HR", "Accounting", "Admin", "Supply"
        };

        public IEnumerable<string> GetOffices() => Offices;

        public void AddOffice(string office)
        {
            if (!Offices.Contains(office))
                Offices.Add(office);
        }

        public void RemoveOffice(string office)
        {
            Offices.Remove(office);
        }
    }
}