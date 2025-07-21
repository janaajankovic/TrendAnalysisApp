using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using TrendAnalysis.Contracts;
using System.Diagnostics;

namespace TrendAnalysis.UI.AttachedProperties
{
    public static class CanvasChartExtensions
    {
        public static readonly DependencyProperty ChartDataProperty =
            DependencyProperty.RegisterAttached(
                "ChartData",
                typeof(ObservableCollection<TrendDataPoint>),
                typeof(CanvasChartExtensions),
                new FrameworkPropertyMetadata(
                    null,
                    OnChartDataChanged));

        public static ObservableCollection<TrendDataPoint> GetChartData(DependencyObject obj)
        {
            return (ObservableCollection<TrendDataPoint>)obj.GetValue(ChartDataProperty);
        }

        public static void SetChartData(DependencyObject obj, ObservableCollection<TrendDataPoint> value)
        {
            obj.SetValue(ChartDataProperty, value);
        }


        public static readonly DependencyProperty ChartRenderModeProperty =
            DependencyProperty.RegisterAttached(
                "ChartRenderMode",
                typeof(string),
                typeof(CanvasChartExtensions),
                new FrameworkPropertyMetadata(
                    "Line Chart",
                    OnChartRenderModeChanged));

        public static string GetChartRenderMode(DependencyObject obj)
        {
            return (string)obj.GetValue(ChartRenderModeProperty);
        }

        public static void SetChartRenderMode(DependencyObject obj, string value)
        {
            obj.SetValue(ChartRenderModeProperty, value);
        }


        private static void OnChartDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Canvas canvas)
            {
                RedrawChart(canvas);
            }
        }

        private static void OnChartRenderModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Canvas drawingCanvas)
            {
                Debug.WriteLine($"OnChartDataChanged called for Canvas: {drawingCanvas.Name}. NewValue: {e.NewValue?.GetType().Name}, OldValue: {e.OldValue?.GetType().Name}");

                RedrawChart(drawingCanvas);
            }
        }

        private static void RedrawChart(Canvas drawingCanvas)
        {
            Debug.WriteLine($"RedrawChart called for Canvas: {drawingCanvas.Name}. ActualWidth: {drawingCanvas.ActualWidth}, ActualHeight: {drawingCanvas.ActualHeight}");

            drawingCanvas.Children.Clear(); 

            ObservableCollection<TrendDataPoint> rawTrendData = GetChartData(drawingCanvas);
            string renderMode = GetChartRenderMode(drawingCanvas);

            if (rawTrendData == null)
            {
                Debug.WriteLine("rawTrendData is NULL when GetChartData is called.");
                return;
            }
            if (!rawTrendData.Any())
            {
                Debug.WriteLine("rawTrendData is EMPTY when GetChartData is called. Count: 0");
                return; 
            }
            Debug.WriteLine($"rawTrendData count: {rawTrendData.Count} (This should be > 0)");
            List<TrendDataPoint> aggregatedData = rawTrendData
                .GroupBy(dp => new DateTime(dp.Timestamp.Year, dp.Timestamp.Month, dp.Timestamp.Day, dp.Timestamp.Hour, 0, 0))
                .Select(g => new TrendDataPoint
                {
                    Timestamp = g.Key,
                    Value = g.Average(dp => dp.Value)
                })
                .OrderBy(dp => dp.Timestamp)
                .ToList();

            var trendData = aggregatedData; 

            Debug.WriteLine($"Aggregated data points: {trendData.Count}");

            if (!trendData.Any())
            {
                Debug.WriteLine("Aggregated data is EMPTY after grouping. This means grouping logic failed or raw data caused it to be empty.");
                return;
            }


            double canvasWidth = drawingCanvas.ActualWidth;
            double canvasHeight = drawingCanvas.ActualHeight;

            if (canvasWidth == 0 || canvasHeight == 0)
            {
                Debug.WriteLine("Canvas has zero width or height. Aborting redraw. This might happen on initial load.");
                return;
            }

            double margin = 40;
            double plotAreaWidth = canvasWidth - 2 * margin;
            double plotAreaHeight = canvasHeight - 2 * margin;

            if (plotAreaWidth <= 0 || plotAreaHeight <= 0)
            {
                Debug.WriteLine("Plot area too small. Aborting redraw.");
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


            Line xAxis = new Line
            {
                X1 = margin,
                Y1 = canvasHeight - margin,
                X2 = canvasWidth - margin,
                Y2 = canvasHeight - margin,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            drawingCanvas.Children.Add(xAxis);

            Line yAxis = new Line
            {
                X1 = margin,
                Y1 = margin,
                X2 = margin,
                Y2 = canvasHeight - margin,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            drawingCanvas.Children.Add(yAxis);

            int numberOfYLabels = 5;
            for (int i = 0; i <= numberOfYLabels; i++)
            {
                double yValue = minY + (maxY - minY) * i / numberOfYLabels;
                double yPos = canvasHeight - margin - (yValue - minY) / (maxY - minY) * plotAreaHeight;

                Line tick = new Line { X1 = margin - 5, Y1 = yPos, X2 = margin, Y2 = yPos, Stroke = Brushes.Gray, StrokeThickness = 0.5 };
                drawingCanvas.Children.Add(tick);

                TextBlock label = new TextBlock
                {
                    Text = yValue.ToString("F1"),
                    FontSize = 9,
                    Foreground = Brushes.Black
                };
                drawingCanvas.Children.Add(label);
                label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                label.Arrange(new Rect(label.DesiredSize));
                Canvas.SetLeft(label, margin - label.ActualWidth - 20);
                Canvas.SetTop(label, yPos - label.ActualHeight / 2);
            }

            int numberOfXLabels = 7;
            double xInterval = (maxX - minX) / numberOfXLabels;

            for (int i = 0; i <= numberOfXLabels; i++)
            {
                double xOADate = minX + xInterval * i;
                DateTime xDateTime = DateTime.FromOADate(xOADate);
                double xPos = margin + (xOADate - minX) / (maxX - minX) * plotAreaWidth;

                Line tick = new Line { X1 = xPos, Y1 = canvasHeight - margin, X2 = xPos, Y2 = canvasHeight - margin + 5, Stroke = Brushes.Gray, StrokeThickness = 0.5 };
                drawingCanvas.Children.Add(tick);

                TextBlock label = new TextBlock
                {
                    Text = xDateTime.ToString("dd.MM.yyyy\nHH:mm"),
                    FontSize = 9,
                    Foreground = Brushes.Black
                };
                drawingCanvas.Children.Add(label);
                label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                label.Arrange(new Rect(label.DesiredSize));
                Canvas.SetLeft(label, xPos - (label.ActualWidth / 2));
                Canvas.SetTop(label, canvasHeight - margin + 10);
            }


            if (renderMode == "Line Chart")
            {
                Debug.WriteLine("Rendering Line Chart for Canvas via Attached Property.");
                for (int i = 0; i < trendData.Count - 1; i++)
                {
                    TrendDataPoint p1 = trendData[i];
                    TrendDataPoint p2 = trendData[i + 1];

                    Point linePoint1 = TransformDataToScreen(p1, minX, maxX, minY, maxY, plotAreaWidth, plotAreaHeight, margin, canvasHeight);
                    Point linePoint2 = TransformDataToScreen(p2, minX, maxX, minY, maxY, plotAreaWidth, plotAreaHeight, margin, canvasHeight);

                    Line line = new Line
                    {
                        X1 = linePoint1.X,
                        Y1 = linePoint1.Y,
                        X2 = linePoint2.X,
                        Y2 = linePoint2.Y,
                        Stroke = Brushes.Blue,
                        StrokeThickness = 1.5
                    };
                    drawingCanvas.Children.Add(line);

                    Ellipse pointCircle1 = new Ellipse { Width = 4, Height = 4, Fill = Brushes.Red };
                    Canvas.SetLeft(pointCircle1, linePoint1.X - pointCircle1.Width / 2);
                    Canvas.SetTop(pointCircle1, linePoint1.Y - pointCircle1.Height / 2);
                    drawingCanvas.Children.Add(pointCircle1);
                }
                if (trendData.Any())
                {
                    TrendDataPoint lastPoint = trendData.Last();
                    Point lastLinePoint = TransformDataToScreen(lastPoint, minX, maxX, minY, maxY, plotAreaWidth, plotAreaHeight, margin, canvasHeight);
                    Ellipse pointCircleLast = new Ellipse { Width = 4, Height = 4, Fill = Brushes.Red };
                    Canvas.SetLeft(pointCircleLast, lastLinePoint.X - pointCircleLast.Width / 2);
                    Canvas.SetTop(pointCircleLast, lastLinePoint.Y - pointCircleLast.Height / 2);
                    drawingCanvas.Children.Add(pointCircleLast);
                }
            }
            else if (renderMode == "Bar Chart")
            {
                Debug.WriteLine("Rendering Bar Chart for Canvas via Attached Property.");
                double barWidthFactor = 0.8;
                double individualBarWidth;

                double xIntervalForBars = (maxX - minX) / trendData.Count;
                individualBarWidth = (plotAreaWidth / trendData.Count) * barWidthFactor;
                if (individualBarWidth < 1) individualBarWidth = 1;


                for (int i = 0; i < trendData.Count; i++)
                {
                    TrendDataPoint p = trendData[i];

                    double xPos = margin + (p.Timestamp.ToOADate() - minX) / (maxX - minX) * plotAreaWidth;

                    double barHeight = (p.Value - minY) / (maxY - minY) * plotAreaHeight;
                    double yPos = canvasHeight - margin - barHeight;

                    Rectangle bar = new Rectangle
                    {
                        Width = individualBarWidth,
                        Height = barHeight,
                        Fill = Brushes.Green,
                        Stroke = Brushes.DarkGreen,
                        StrokeThickness = 1
                    };

                    Canvas.SetLeft(bar, xPos - individualBarWidth / 2);
                    Canvas.SetTop(bar, yPos);
                    drawingCanvas.Children.Add(bar);
                }
            }
            Debug.WriteLine("Finished RedrawChart.");
        }

        private static void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is Canvas canvas)
            {
                Debug.WriteLine($"CanvasChartExtensions: Canvas_SizeChanged fired for Canvas: {canvas.Name}. New Size: {e.NewSize.Width}x{e.NewSize.Height}.");
                RedrawChart(canvas);
            }
        }

        private static Point TransformDataToScreen(TrendDataPoint dataPoint, double minX, double maxX, double minY, double maxY, double plotAreaWidth, double plotAreaHeight, double margin, double canvasHeight)
        {
            return new Point(
                margin + (dataPoint.Timestamp.ToOADate() - minX) / (maxX - minX) * plotAreaWidth,
                canvasHeight - margin - (dataPoint.Value - minY) / (maxY - minY) * plotAreaHeight
            );
        }
    }
}