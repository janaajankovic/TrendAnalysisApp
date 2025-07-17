using System;
using System.Collections.Generic;
using System.Configuration; // Potrebno za ConfigurationManager
using System.Data.SqlClient; // Potrebno za SQL klijent
using System.Linq;
using System.Runtime.Serialization; // Moguće da ne treba ako se ne koriste direktno atributi
using System.ServiceModel; // Moguće da ne treba ako se ne koriste direktno atributi
using System.ServiceModel.Web; // Moguće da ne treba ako se ne koriste direktno atributi
using System.Text;
using System.Threading.Tasks; // <-- OVO JE KRITIČNO za async/await

using TrendAnalysis.ContractFramework; // Koristi ugovore iz Framework projekta

namespace TrendAnalysis.Service
{
    // Važno: Proveri da li tvoj DataContract TrendDataPoint ima polje 'Timestamp' ili 'Date'
    // U interfejsu smo se dogovorili za List<TrendDataPoint> i pretpostavljam da imaš 'Timestamp' u bazi.
    // Tvoja TrendDataPoint klasa treba da ima public DateTime Timestamp { get; set; } ako tako čitaš iz baze.
    // Ako se property u DataContractu zove 'Date', onda ga moraš mapirati na 'Timestamp' iz readera.
    // U donjem kodu koristiću 'Timestamp' kako je bilo u tvom originalnom kodu.

    public class TrendDataService : ITrendDataService
    {
        private readonly string _connectionString;

        public TrendDataService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["TrendDataDBConnection"].ConnectionString;
        }

        // Implementacija ASINHRONE metode GetTrendDataAsync iz ITrendDataService
        // Ovu metodu prilagođavamo da bude asinhrona verzija TVOG koda za pristup bazi.
        public async Task<List<TrendDataPoint>> GetTrendDataAsync(DateTime startTime, DateTime endTime)
        {
            List<TrendDataPoint> data = new List<TrendDataPoint>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                // Koristimo asinhronu verziju Open()
                await connection.OpenAsync();

                string query = "SELECT Timestamp, Value FROM dbo.TrendData WHERE Timestamp >= @StartTime AND Timestamp <= @EndTime ORDER BY Timestamp ASC";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StartTime", startTime);
                    command.Parameters.AddWithValue("@EndTime", endTime); // Ovo je OK, ranije sam video gresku u tvom kodu ali ne ovde

                    // Koristimo asinhronu verziju ExecuteReader()
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        // Koristimo asinhronu verziju Read()
                        while (await reader.ReadAsync())
                        {
                            data.Add(new TrendDataPoint
                            {
                                // Koristim 'Timestamp' kako je bilo u tvom originalnom kodu za TrendDataPoint
                                Timestamp = reader.GetDateTime(reader.GetOrdinal("Timestamp")),
                                Value = reader.GetDouble(reader.GetOrdinal("Value"))
                            });
                        }
                    }
                }
            }
            return data;
        }

        // Implementacija ASINHRONE metode GetDataAsync iz ITrendDataService
        // Ovo je samo placeholder implementacija, ako nemaš bazu za nju.
        public async Task<string> GetDataAsync(int value)
        {
            await Task.Delay(50); // Simuliše asinhroni rad
            return $"You entered: {value}. This is from GetDataAsync.";
        }
    }
}