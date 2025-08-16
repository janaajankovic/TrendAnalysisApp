using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TrendAnalysis.ContractFramework
{
    [DataContract]
    public class TrendDataPoint
    {

        [DataMember(Order = 1)]
        public DateTime Timestamp { get; set; }

        [DataMember(Order = 2)]
        public double Value { get; set; }

    }
}
