using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrendAnalysis.ViewModel
{
    public class RenderMeasurement
    {
        public DateTime Timestamp { get; set; }
        public string RenderingMethod { get; set; }
        public string ChartType { get; set; }
        public int NumberOfPoints { get; set; }
        public double RenderDurationMs { get; set; }
    }
}
