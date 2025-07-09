using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace TrendAnalysis.Service
{
    public class TrendDataService : ITrendDataService
    {
        private readonly string _connectionString;
        public TrendDataService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["TrendDataDBConnection"].ConnectionString;
        }

        public List<TrendDataPoint> GetTrendData(DateTime startTime, DateTime endTime)
        {
            List<TrendDataPoint> data = new List<TrendDataPoint>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT Timestamp, Value FROM dbo.TrendData WHERE Timestamp >= @StartTime AND Timestamp <= @EndTime ORDER BY Timestamp ASC";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StartTime", startTime);
                    command.Parameters.AddWithValue("@EndTime", endTime);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            data.Add(new TrendDataPoint // OVDJE KORISTIMO NOVU KLASU
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
        
    }
}
