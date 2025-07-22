using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using TrendAnalysis.Contracts;
using System.Diagnostics;
using Brushes = System.Windows.Media.Brushes;
using Pen = System.Windows.Media.Pen;
using Point = System.Windows.Point;
using Brush = System.Windows.Media.Brush;


namespace TrendAnalysis.UI.Controls 
{
    public class HardwareAcceleratedChartControl : FrameworkElement
    {
        public event EventHandler<TimeSpan> RenderCompleted;

        public static readonly DependencyProperty TrendDataProperty =
            DependencyProperty.Register(
                "TrendData",
                typeof(ObservableCollection<TrendDataPoint>),
                typeof(HardwareAcceleratedChartControl),
                 new FrameworkPropertyMetadata(
                    null,
                    OnTrendDataChanged));


        public static readonly DependencyProperty RenderTriggerProperty =
         DependencyProperty.Register(
             "RenderTrigger",
             typeof(object),
             typeof(HardwareAcceleratedChartControl),
             new FrameworkPropertyMetadata(
                 null,
             OnRenderTriggerChanged));

        public object RenderTrigger
        {
            get { return GetValue(RenderTriggerProperty); }
            set { SetValue(RenderTriggerProperty, value); }
        }

        private bool _isReadyToRender = false;
        public ObservableCollection<TrendDataPoint> TrendData
        {
            get { return (ObservableCollection<TrendDataPoint>)GetValue(TrendDataProperty); }
            set { SetValue(TrendDataProperty, value); }
        }

        public static readonly DependencyProperty ChartTypeProperty =
            DependencyProperty.Register(
                "ChartType",
                typeof(string),
                typeof(HardwareAcceleratedChartControl),
                new FrameworkPropertyMetadata(
                 "Line Chart",
             OnRenderTriggerChanged));


        public string ChartType
        {
            get { return (string)GetValue(ChartTypeProperty); }
            set { SetValue(ChartTypeProperty, value); }
        }

        private static void OnTrendDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (HardwareAcceleratedChartControl)d;
            if (e.OldValue is ObservableCollection<TrendDataPoint> oldCollection)
            {
                oldCollection.CollectionChanged -= control.OnTrendDataCollectionChanged;
            }
            if (e.NewValue is ObservableCollection<TrendDataPoint> newCollection)
            {
                newCollection.CollectionChanged += control.OnTrendDataCollectionChanged;
            }
            control.InvalidateVisual();
        }

        private void OnTrendDataCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            InvalidateVisual();
        }


        protected override void OnRender(DrawingContext drawingContext)
        {
            if (!_isReadyToRender)
            {
                return; 
            }

            base.OnRender(drawingContext);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            if (TrendData == null || !TrendData.Any())
            {
                stopwatch.Stop();
                return;
            }

            double canvasWidth = ActualWidth;
            double canvasHeight = ActualHeight;

            if (canvasWidth == 0 || canvasHeight == 0)
            {
                stopwatch.Stop();
                return;
            }

            double margin = 40; 
            double plotAreaWidth = canvasWidth - 2 * margin;
            double plotAreaHeight = canvasHeight - 2 * margin;

            if (plotAreaWidth <= 0 || plotAreaHeight <= 0)
            {
                stopwatch.Stop();
                return; 
            }

            List<TrendDataPoint> aggregatedData = TrendData
           .GroupBy(dp => new DateTime(dp.Timestamp.Year, dp.Timestamp.Month, dp.Timestamp.Day, dp.Timestamp.Hour, 0, 0))
           .Select(g => new TrendDataPoint
           {
               Timestamp = g.Key,
               Value = g.Average(dp => dp.Value)
           })
           .OrderBy(dp => dp.Timestamp)
           .ToList();

            List<TrendDataPoint> trendData = aggregatedData; 

            if (!trendData.Any()) 
            {
                stopwatch.Stop();
                return;
            }

            double minX = trendData.Min(p => p.Timestamp.ToOADate());
            double maxX = trendData.Max(p => p.Timestamp.ToOADate());
            double minY = trendData.Min(p => p.Value);
            double maxY = trendData.Max(p => p.Value);

            double yAxisPadding = (maxY - minY) * 0.1;
            minY -= yAxisPadding;
            maxY += yAxisPadding;

            if (Math.Abs(maxX - minX) < 1e-6) maxX = minX + 1;
            if (Math.Abs(maxY - minY) < 1e-6) maxY = minY + 1;

            
            Pen axisPen = new Pen(Brushes.Black, 2);
            drawingContext.DrawLine(axisPen, new Point(margin, canvasHeight - margin), new Point(canvasWidth - margin, canvasHeight - margin)); 
            drawingContext.DrawLine(axisPen, new Point(margin, margin), new Point(margin, canvasHeight - margin));
            
            int numberOfYLabels = 5;
            Pen tickPen = new Pen(Brushes.Gray, 0.5);
            Typeface labelTypeface = new Typeface("Segoe UI"); 
            double fontSize = 9;

            for (int i = 0; i <= numberOfYLabels; i++)
            {
                double yValue = minY + (maxY - minY) * i / numberOfYLabels;
                double yPos = canvasHeight - margin - (yValue - minY) / (maxY - minY) * plotAreaHeight;

                
                drawingContext.DrawLine(tickPen, new Point(margin - 5, yPos), new Point(margin, yPos));

                
                FormattedText yLabelText = new FormattedText(
                    yValue.ToString("F1"),
                    System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight,
                    labelTypeface,
                    fontSize,
                    Brushes.Black,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip 
                );
                drawingContext.DrawText(yLabelText, new Point(margin - yLabelText.Width - 10, yPos - yLabelText.Height / 2));
            }

            int numberOfXLabels = 7;
            double xInterval = (maxX - minX) / numberOfXLabels;

            for (int i = 0; i <= numberOfXLabels; i++)
            {
                double xOADate = minX + xInterval * i;
                DateTime xDateTime = DateTime.FromOADate(xOADate);
                double xPos = margin + (xOADate - minX) / (maxX - minX) * plotAreaWidth;

                drawingContext.DrawLine(tickPen, new Point(xPos, canvasHeight - margin), new Point(xPos, canvasHeight - margin + 5));

                FormattedText xLabelText = new FormattedText(
                    xDateTime.ToString("dd.MM.yyyy\nHH:mm"),
                    System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight,
                    labelTypeface,
                    fontSize,
                    Brushes.Black,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip
                );
                drawingContext.DrawText(xLabelText, new Point(xPos - xLabelText.Width / 2, canvasHeight - margin + 10));
            }


            if (ChartType == "Line Chart")
            {
                Pen linePen = new Pen(Brushes.Blue, 1.5);
                Brush pointBrush = Brushes.Red;
                double pointRadius = 2; 

                for (int i = 0; i < trendData.Count - 1; i++)
                {
                    TrendDataPoint p1 = trendData[i];
                    TrendDataPoint p2 = trendData[i + 1];

                    Point linePoint1 = TransformDataToScreen(p1, minX, maxX, minY, maxY, plotAreaWidth, plotAreaHeight, margin, canvasHeight);
                    Point linePoint2 = TransformDataToScreen(p2, minX, maxX, minY, maxY, plotAreaWidth, plotAreaHeight, margin, canvasHeight);

                    drawingContext.DrawLine(linePen, linePoint1, linePoint2);
                    drawingContext.DrawEllipse(pointBrush, null, linePoint1, pointRadius, pointRadius);
                }
                if (trendData.Any())
                {
                    Point lastLinePoint = TransformDataToScreen(trendData.Last(), minX, maxX, minY, maxY, plotAreaWidth, plotAreaHeight, margin, canvasHeight);
                    drawingContext.DrawEllipse(pointBrush, null, lastLinePoint, pointRadius, pointRadius);
                }
            }
            else if (ChartType == "Bar Chart")
            {
                Brush barBrush = Brushes.Green;
                Pen barOutlinePen = new Pen(Brushes.DarkGreen, 1);
                double barWidthFactor = 0.8;
                double individualBarWidth = (plotAreaWidth / trendData.Count) * barWidthFactor;

                if (individualBarWidth < 1) individualBarWidth = 1;


                for (int i = 0; i < trendData.Count; i++)
                {
                    TrendDataPoint p = trendData[i];

                    double xCenterPos = margin + (p.Timestamp.ToOADate() - minX + (maxX - minX) / (trendData.Count * 2)) / (maxX - minX) * plotAreaWidth;

                    double barHeight = (p.Value - minY) / (maxY - minY) * plotAreaHeight;
                    double yPos = canvasHeight - margin - barHeight;

                    drawingContext.DrawRectangle(barBrush, barOutlinePen, new Rect(xCenterPos - individualBarWidth / 2, yPos, individualBarWidth, barHeight));
                }
            }

            stopwatch.Stop();
            RenderCompleted?.Invoke(null, stopwatch.Elapsed);
            _isReadyToRender = false;
            Debug.WriteLine($"OnRender (HardwareAcceleratedChartControl) completed in {stopwatch.Elapsed.TotalMilliseconds:F2} ms.");
        }

        private static void OnRenderTriggerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (HardwareAcceleratedChartControl)d;
            control._isReadyToRender = true; 
            control.InvalidateVisual();
            System.Diagnostics.Debug.WriteLine("OnRenderTriggerChanged triggered. _isReadyToRender set to true."); 
        }

        private Point TransformDataToScreen(TrendDataPoint dataPoint, double minX, double maxX, double minY, double maxY, double plotAreaWidth, double plotAreaHeight, double margin, double canvasHeight)
        {
            return new Point(
                margin + (dataPoint.Timestamp.ToOADate() - minX) / (maxX - minX) * plotAreaWidth,
                canvasHeight - margin - (dataPoint.Value - minY) / (maxY - minY) * plotAreaHeight
            );
        }
    }
}