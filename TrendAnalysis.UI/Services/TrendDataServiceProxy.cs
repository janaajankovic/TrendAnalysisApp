using System;
using System.Linq;
using System.Threading.Tasks;       
using System.Collections.Generic;   
using System.ServiceModel;         

using TrendAnalysis.Contracts; 

using GeneratedClient = TrendService;

namespace TrendAnalysis.UI.Services
{
    public class TrendDataServiceProxy : ITrendDataService, IDisposable
    {
        private readonly GeneratedClient.TrendDataServiceClient _wcfClient;

        public TrendDataServiceProxy()
        {
            _wcfClient = new GeneratedClient.TrendDataServiceClient();
        }

        public async Task<List<TrendDataPoint>> GetTrendDataAsync(DateTime startTime, DateTime endTime)
        {
            try
            {
                GeneratedClient.TrendDataPoint[] generatedData = await _wcfClient.GetTrendDataAsync(startTime, endTime);

                if (generatedData == null)
                {
                    return new List<TrendDataPoint>();
                }

                return generatedData.Select(gdp => new TrendDataPoint
                {
                    Timestamp = gdp.Timestamp,
                    Value = gdp.Value
                }).ToList(); 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling GetTrendDataAsync in proxy: {ex.Message}");
                throw; 
            }
        }

        public async Task<string> GetDataAsync(int value)
        {
            try
            {
                return await _wcfClient.GetDataAsync(value);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling GetDataAsync in proxy: {ex.Message}");
                throw; 
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_wcfClient != null)
                {
                    try
                    {
                        if (_wcfClient.State == System.ServiceModel.CommunicationState.Faulted) 
                        {
                            _wcfClient.Abort();
                        }
                        else
                        {
                            _wcfClient.Close(); 
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        _wcfClient.Abort();
                    }
                    catch (System.ServiceModel.CommunicationException)
                    {
                        _wcfClient.Abort();
                    }
                    catch (TimeoutException)
                    {
                        _wcfClient.Abort();
                    }
                    catch (Exception)
                    {
                        _wcfClient.Abort();
                    }
                }
            }
        }
    }
}