using System;
using System.Collections.Generic;
using System.Configuration; 
using System.Data.SqlClient;
using System.Threading.Tasks; 
using TrendAnalysis.ContractFramework;

namespace TrendAnalysis.Service
{

    public class TrendDataService : ITrendDataService
    {
        private readonly string _connectionString;

        public TrendDataService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["TrendDataDBConnection"].ConnectionString;
        }

        public async Task<List<TrendDataPoint>> GetTrendDataAsync(DateTime startTime, DateTime endTime)
        {
            List<TrendDataPoint> data = new List<TrendDataPoint>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = "SELECT Timestamp, Value FROM dbo.TrendData WHERE Timestamp >= @StartTime AND Timestamp <= @EndTime ORDER BY Timestamp ASC";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StartTime", startTime);
                    command.Parameters.AddWithValue("@EndTime", endTime); 

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            data.Add(new TrendDataPoint
                            {
                                Timestamp = reader.GetDateTime(reader.GetOrdinal("Timestamp")),
                                Value = reader.GetDouble(reader.GetOrdinal("Value"))
                            });
                        }
                    }
                }
            }
            return data;
        }

        public async Task<string> GetDataAsync(int value)
        {
            await Task.Delay(50); 
            return $"You entered: {value}. This is from GetDataAsync.";
        }
    }
}