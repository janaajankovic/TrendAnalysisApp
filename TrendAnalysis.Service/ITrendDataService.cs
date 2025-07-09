using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace TrendAnalysis.Service
{
    [ServiceContract]
    public interface ITrendDataService
    {
        [OperationContract]
        List<TrendDataPoint> GetTrendData(DateTime startTime, DateTime endTime);
    }
}
