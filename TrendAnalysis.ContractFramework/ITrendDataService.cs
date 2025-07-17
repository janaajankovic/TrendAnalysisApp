using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace TrendAnalysis.ContractFramework
{
    [ServiceContract]
    public interface ITrendDataService
    {
        [OperationContract(Name = "GetTrendData")]
        Task<List<TrendDataPoint>> GetTrendDataAsync(DateTime startTime, DateTime endTime);

        [OperationContract(Name = "GetData")]
        Task<string> GetDataAsync(int value);
    }

}
