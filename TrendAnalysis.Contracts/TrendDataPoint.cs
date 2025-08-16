using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TrendAnalysis.Contracts
{
    [DataContract]
    public class TrendDataPoint
    {
        public static event EventHandler<TimeSpan> CanvasChartRenderCompleted;

        public static void OnCanvasChartRenderCompleted(TimeSpan duration)
        {
            CanvasChartRenderCompleted?.Invoke(null, duration);
        }

        [DataMember(Order = 1)]
        public DateTime Timestamp { get; set; }

        [DataMember(Order = 2)]
        public double Value { get; set; }

    }
}
