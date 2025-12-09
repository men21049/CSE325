using DocumentManagementSystem.Model;
using Npgsql;

namespace DocumentManagementSystem.Services
{
    public class OfficeService
    {
        private readonly DatabaseConnection _dbConnection;
        private List<OfficeInfo> _offices = new();

        public class OfficeInfo
        {
            public int OfficeID { get; set; }
            public string OfficeName { get; set; } = string.Empty;
        }

        public OfficeService(DatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        private async Task LoadOfficesAsync()
        {
            try
            {
                var sql = "SELECT \"OfficeId\", \"OfficeName\" FROM \"DocMS\".\"Offices\" ORDER BY \"OfficeName\"";
                var results = await _dbConnection.ExecuteQueryAsync(sql, new Dictionary<string, object>());

                if (results.Count > 0)
                {
                    _offices = results
                        .Where(row => row["OfficeId"] != null && row["OfficeName"] != null)
                        .Select(row => new OfficeInfo
                        {
                            OfficeID = Convert.ToInt32(row["OfficeId"]),
                            OfficeName = row["OfficeName"].ToString()!
                        })
                        .ToList();
                }
            }
            catch
            {
                _offices = new List<OfficeInfo>();
                throw;
            }
        }

        public IEnumerable<OfficeInfo> GetOffices() => _offices;

        public async Task RefreshOfficesAsync()
        {
            await LoadOfficesAsync();
        }

        public async Task AddOfficeAsync(string officeName)
        {
            if (string.IsNullOrWhiteSpace(officeName))
                return;

            if (!_offices.Any(o => o.OfficeName == officeName))
            {
                try
                {
                    var sql = "INSERT INTO \"DocMS\".\"Offices\" (\"OfficeName\") VALUES (@OfficeName)";
                    var parameters = new[] { new NpgsqlParameter("@OfficeName", officeName) };
                    await _dbConnection.ExecuteNonQueryAsync(sql, parameters);

                    await LoadOfficesAsync();
                }
                catch
                {
                    throw;
                }
            }
        }

        public async Task RemoveOfficeAsync(string officeName)
        {
            if (string.IsNullOrWhiteSpace(officeName))
                return;

            try
            {
                var sql = "DELETE FROM \"DocMS\".\"Offices\" WHERE \"OfficeName\" = @OfficeName";
                var parameters = new[] { new NpgsqlParameter("@OfficeName", officeName) };
                await _dbConnection.ExecuteNonQueryAsync(sql, parameters);

                var officeToRemove = _offices.FirstOrDefault(o => o.OfficeName == officeName);
                if (officeToRemove != null)
                {
                    _offices.Remove(officeToRemove);
                }
            }
            catch
            {
                var officeToRemove = _offices.FirstOrDefault(o => o.OfficeName == officeName);
                if (officeToRemove != null)
                {
                    _offices.Remove(officeToRemove);
                }
            }
        }

    }
}