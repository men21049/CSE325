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
                    // En PostgreSQL, los nombres de tablas y columnas sin comillas se convierten a minúsculas
                    var sql = "INSERT INTO \"DocMS\".\"Offices\" (\"OfficeName\") VALUES (@OfficeName)";
                    var parameters = new[] { new NpgsqlParameter("@OfficeName", officeName) };
                    await _dbConnection.ExecuteNonQueryAsync(sql, parameters);

                    // Recargar las oficinas para obtener el nuevo ID
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
                // En PostgreSQL, los nombres de tablas y columnas sin comillas se convierten a minúsculas
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
                // Si falla la eliminación en BD, solo remover de la lista en memoria
                var officeToRemove = _offices.FirstOrDefault(o => o.OfficeName == officeName);
                if (officeToRemove != null)
                {
                    _offices.Remove(officeToRemove);
                }
            }
        }

        /*public void AddOffice(string office)
        {
            if (!_offices.Any(o => o.OfficeName == office))
            {
                var maxId = _offices.Count > 0 ? _offices.Max(o => o.OfficeID) : 0;
                _offices.Add(new OfficeInfo { OfficeID = maxId + 1, OfficeName = office });
            }
        }

        public void RemoveOffice(string office)
        {
            var officeToRemove = _offices.FirstOrDefault(o => o.OfficeName == office);
            if (officeToRemove != null)
            {
                _offices.Remove(officeToRemove);
            }
        }*/
    }
}