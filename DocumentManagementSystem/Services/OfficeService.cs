using DocumentManagementSystem.Model;
using Microsoft.Data.SqlClient;

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
            // Inicializar con oficinas por defecto
            _offices = new List<OfficeInfo>
            {
                new OfficeInfo { OfficeID = 1, OfficeName = "HR" },
                new OfficeInfo { OfficeID = 2, OfficeName = "Accounting" },
                new OfficeInfo { OfficeID = 3, OfficeName = "Admin" },
                new OfficeInfo { OfficeID = 4, OfficeName = "Supply" }
            };
        }

        private async Task LoadOfficesAsync()
        {
            try
            {
                var sql = "SELECT OfficeID, OfficeName FROM Offices ORDER BY OfficeName";
                var results = await _dbConnection.ExecuteQueryAsync(sql);
                
                if (results.Count > 0)
                {
                    _offices = results
                        .Where(row => row["OfficeID"] != null && row["OfficeName"] != null)
                        .Select(row => new OfficeInfo
                        {
                            OfficeID = Convert.ToInt32(row["OfficeID"]),
                            OfficeName = row["OfficeName"].ToString()!
                        })
                        .ToList();
                }
            }
            catch
            {
                // Si falla, mantener las oficinas por defecto
                // No lanzar excepción para evitar que el servicio falle
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
                    var sql = "INSERT INTO Offices (OfficeName) VALUES (@OfficeName)";
                    var parameters = new[] { new SqlParameter("@OfficeName", officeName) };
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
                var sql = "DELETE FROM Offices WHERE OfficeName = @OfficeName";
                var parameters = new[] { new SqlParameter("@OfficeName", officeName) };
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

        public void AddOffice(string office)
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
        }
    }
}