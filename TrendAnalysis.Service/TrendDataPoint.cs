using System;
using System.Runtime.Serialization; 

namespace TrendAnalysis.Service
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