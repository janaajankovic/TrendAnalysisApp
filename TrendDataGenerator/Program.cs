using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics; 

namespace TrendDataGenerator
{
    internal class Program
    {
        // --- KONFIGURACIJA ---
        private const string ConnectionString = "Server=localhost;Database=TrendDataDB;Integrated Security=True;";
        private const int NumberOfRecordsToGenerate = 1_000_000;
        private const int BatchSize = 50_000; 

        static void Main(string[] args)
        {
            Console.WriteLine("Pocetak generisanja i ubacivanja testnih podataka...");
            Console.WriteLine($"Broj redova za generisanje: {NumberOfRecordsToGenerate:N0}");
            Console.WriteLine($"Velicina batch-a za ubacivanje: {BatchSize:N0}");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                DataTable trendDataTable = new DataTable();
                trendDataTable.Columns.Add("TimeStamp", typeof(DateTime));
                trendDataTable.Columns.Add("Value", typeof(double)); 

                Console.WriteLine("Generisanje podataka u memoriju...");

                Random random = new Random();
                DateTime startTime = DateTime.UtcNow.AddYears(-1); 

                for (int i = 0; i < NumberOfRecordsToGenerate; i++)
                {
                    DataRow row = trendDataTable.NewRow();

                    TimeSpan randomTimeSpan = new TimeSpan((long)(random.NextDouble() * (DateTime.UtcNow - startTime).Ticks));
                    row["Timestamp"] = startTime + randomTimeSpan;

                    row["Value"] = random.NextDouble() * 100.0;

                    trendDataTable.Rows.Add(row);

                    if ((i + 1) % 100_000 == 0) 
                    {
                        Console.WriteLine($"Generisano {i + 1:N0} redova...");
                    }
                }

                Console.WriteLine($"Generisanje podataka zavrseno. Ukupno generisano: {trendDataTable.Rows.Count:N0} redova.");

                Console.WriteLine("Pocetak ubacivanja podataka u bazu pomocu SqlBulkCopy...");

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                    {
                        bulkCopy.DestinationTableName = "dbo.TrendData"; 

                        
                        bulkCopy.ColumnMappings.Add("TimeStamp", "TimeStamp");
                        bulkCopy.ColumnMappings.Add("Value", "Value");

                        bulkCopy.BatchSize = BatchSize; 
                        bulkCopy.NotifyAfter = BatchSize; 
                        bulkCopy.SqlRowsCopied += (sender, e) =>
                        {
                            Console.WriteLine($"Ubaceno {e.RowsCopied:N0} redova...");
                        };

                        bulkCopy.WriteToServer(trendDataTable); 
                    }
                }

                stopwatch.Stop();
                Console.WriteLine($"\nUbacivanje svih {NumberOfRecordsToGenerate:N0} redova zavrseno!");
                Console.WriteLine($"Ukupno vrijeme: {stopwatch.Elapsed.TotalSeconds:F2} sekundi ({stopwatch.Elapsed.TotalMinutes:F2} minuta)");
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nDoslo je do greske:");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
                }
            }

            Console.WriteLine("Pritisni bilo koji taster za izlaz...");
            Console.ReadKey();
        }
    }
}