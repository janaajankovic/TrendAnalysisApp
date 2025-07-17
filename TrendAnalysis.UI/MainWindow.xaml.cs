using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.ObjectModel; 
using TrendAnalysis.ViewModel;     
using System.Diagnostics;

namespace TrendAnalysis.UI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            DrawingCanvas.SizeChanged += DrawingCanvas_SizeChanged;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel viewModel)
            {
                viewModel.DataReadyForChartRender -= ViewModel_DataReadyForChartRender;
                viewModel.DataReadyForChartRender += ViewModel_DataReadyForChartRender;

                viewModel.PropertyChanged -= ViewModel_PropertyChanged; 
                viewModel.PropertyChanged += ViewModel_PropertyChanged;

                if (viewModel.SelectedRenderingMethod == "Softverski (Canvas)" &&
                   viewModel.CurrentTrendData != null && viewModel.CurrentTrendData.Any())
                {
                    RenderChart();
                }
            }
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (this.DataContext is MainViewModel viewModel)
            {
                if (e.PropertyName == nameof(MainViewModel.SelectedRenderingMethod) ||
                    e.PropertyName == nameof(MainViewModel.SelectedRenderMode) ||
                    e.PropertyName == nameof(MainViewModel.CurrentTrendData))
                {
                    DrawingCanvas.Children.Clear();
                }
            }
        }

        private void ViewModel_DataReadyForChartRender()
        {
            if (this.DataContext is MainViewModel viewModel)
            {
                if (viewModel.CurrentTrendData != null && viewModel.CurrentTrendData.Any())
                {
                    RenderChart();
                }
                else
                {
                    DrawingCanvas.Children.Clear();
                }
            }
        }


        private void DrawingCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.DataContext is MainViewModel viewModel)
            {
                if (viewModel.SelectedRenderingMethod == "Softverski (Canvas)" &&
                    viewModel.CurrentTrendData != null && viewModel.CurrentTrendData.Any())
                {
                    RenderChart();
                }
            }

        }

        private void RenderChart()
        {
            if (!(this.DataContext is MainViewModel viewModel)) return;

            ObservableCollection<TrendAnalysis.Contracts.TrendDataPoint> rawTrendData = viewModel.CurrentTrendData;
            string renderMode = viewModel.SelectedRenderMode;

            Debug.WriteLine($"RenderChart called. Raw Data points: {rawTrendData?.Count ?? 0}. Render Mode: {renderMode}");


            if (rawTrendData == null || !rawTrendData.Any())
            {
                DrawingCanvas.Children.Clear();
                return;
            }

            List<TrendAnalysis.Contracts.TrendDataPoint> aggregatedData = new List<TrendAnalysis.Contracts.TrendDataPoint>();

            if (rawTrendData != null && rawTrendData.Any())
            {
                aggregatedData = rawTrendData
                    .GroupBy(dp => new DateTime(dp.Timestamp.Year, dp.Timestamp.Month, dp.Timestamp.Day, dp.Timestamp.Hour, 0, 0)) 
                    .Select(g => new TrendAnalysis.Contracts.TrendDataPoint
                    {
                        Timestamp = g.Key,
                        Value = g.Average(dp => dp.Value) 
                    })
                    .OrderBy(dp => dp.Timestamp) 
                    .ToList();

                Debug.WriteLine($"Aggregated data points: {aggregatedData.Count}");
            }
            else
            {
                DrawingCanvas.Children.Clear();
                return;
            }

            var trendData = aggregatedData;


            DrawingCanvas.Children.Clear();

            double canvasWidth = DrawingCanvas.ActualWidth;
            double canvasHeight = DrawingCanvas.ActualHeight;

            if (canvasWidth == 0 || canvasHeight == 0)
            {
                Debug.WriteLine("Canvas has no size. Aborting render.");
                return;
            }

            double margin = 40;
            double plotAreaWidth = canvasWidth - 2 * margin;
            double plotAreaHeight = canvasHeight - 2 * margin;

            if (plotAreaWidth <= 0 || plotAreaHeight <= 0)
            {
                Debug.WriteLine("Plot area too small. Aborting render.");
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
            DrawingCanvas.Children.Add(xAxis);

            Line yAxis = new Line
            {
                X1 = margin,
                Y1 = margin,
                X2 = margin,
                Y2 = canvasHeight - margin,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            DrawingCanvas.Children.Add(yAxis);

            int numberOfYLabels = 5;
            for (int i = 0; i <= numberOfYLabels; i++)
            {
                double yValue = minY + (maxY - minY) * i / numberOfYLabels;
                double yPos = canvasHeight - margin - (yValue - minY) / (maxY - minY) * plotAreaHeight;

                Line tick = new Line
                {
                    X1 = margin - 5,
                    Y1 = yPos,
                    X2 = margin,
                    Y2 = yPos,
                    Stroke = Brushes.Gray,
                    StrokeThickness = 0.5
                };
                DrawingCanvas.Children.Add(tick);

                TextBlock label = new TextBlock
                {
                    Text = yValue.ToString("F1"),
                    FontSize = 9,
                    Foreground = Brushes.Black
                };
                Canvas.SetLeft(label, margin - label.ActualWidth - 20);
                Canvas.SetTop(label, yPos - label.ActualHeight / 2);
                DrawingCanvas.Children.Add(label);
            }

            int numberOfXLabels = 7;
            double xInterval = (maxX - minX) / numberOfXLabels;

            for (int i = 0; i <= numberOfXLabels; i++)
            {
                double xOADate = minX + xInterval * i;
                DateTime xDateTime = DateTime.FromOADate(xOADate);
                double xPos = margin + (xOADate - minX) / (maxX - minX) * plotAreaWidth;

                Line tick = new Line
                {
                    X1 = xPos,
                    Y1 = canvasHeight - margin,
                    X2 = xPos,
                    Y2 = canvasHeight - margin + 5,
                    Stroke = Brushes.Gray,
                    StrokeThickness = 0.5
                };
                DrawingCanvas.Children.Add(tick);

                TextBlock label = new TextBlock
                {
                    Text = xDateTime.ToString("dd.MM.yyyy\nHH:mm"), 
                    FontSize = 9,
                    Foreground = Brushes.Black
                };

                Canvas.SetLeft(label, xPos - (label.ActualWidth / 2));
                Canvas.SetTop(label, canvasHeight - margin + 10);
                DrawingCanvas.Children.Add(label);
            }


            if (renderMode == "Line Chart")
            {
                Debug.WriteLine("Rendering Line Chart with AGGREGATED data.");
                for (int i = 0; i < trendData.Count - 1; i++)
                {
                    TrendAnalysis.Contracts.TrendDataPoint p1 = trendData[i];
                    TrendAnalysis.Contracts.TrendDataPoint p2 = trendData[i + 1];

                    Point linePoint1 = new Point(
                        margin + (p1.Timestamp.ToOADate() - minX) / (maxX - minX) * plotAreaWidth,
                        canvasHeight - margin - (p1.Value - minY) / (maxY - minY) * plotAreaHeight
                    );
                    Point linePoint2 = new Point(
                        margin + (p2.Timestamp.ToOADate() - minX) / (maxX - minX) * plotAreaWidth,
                        canvasHeight - margin - (p2.Value - minY) / (maxY - minY) * plotAreaHeight
                    );

                    Line line = new Line
                    {
                        X1 = linePoint1.X,
                        Y1 = linePoint1.Y,
                        X2 = linePoint2.X,
                        Y2 = linePoint2.Y,
                        Stroke = Brushes.Blue,
                        StrokeThickness = 1.5
                    };
                    DrawingCanvas.Children.Add(line);

                    Ellipse pointCircle1 = new Ellipse
                    {
                        Width = 4,
                        Height = 4,
                        Fill = Brushes.Red
                    };
                    Canvas.SetLeft(pointCircle1, linePoint1.X - pointCircle1.Width / 2);
                    Canvas.SetTop(pointCircle1, linePoint1.Y - pointCircle1.Height / 2);
                    DrawingCanvas.Children.Add(pointCircle1);
                }
                if (trendData.Any())
                {
                    TrendAnalysis.Contracts.TrendDataPoint lastPoint = trendData.Last();
                    Point lastLinePoint = new Point(
                        margin + (lastPoint.Timestamp.ToOADate() - minX) / (maxX - minX) * plotAreaWidth,
                        canvasHeight - margin - (lastPoint.Value - minY) / (maxY - minY) * plotAreaHeight
                    );
                    Ellipse pointCircleLast = new Ellipse
                    {
                        Width = 4,
                        Height = 4,
                        Fill = Brushes.Red
                    };
                    Canvas.SetLeft(pointCircleLast, lastLinePoint.X - pointCircleLast.Width / 2);
                    Canvas.SetTop(pointCircleLast, lastLinePoint.Y - pointCircleLast.Height / 2);
                    DrawingCanvas.Children.Add(pointCircleLast);
                }
            }
            else if (renderMode == "Bar Chart")
            {
                Debug.WriteLine("Rendering Bar Chart with AGGREGATED data.");
                double barWidthFactor = 0.8;
                double individualBarWidth;

                double xIntervalForBars = (maxX - minX) / trendData.Count; 
                individualBarWidth = (plotAreaWidth / trendData.Count) * barWidthFactor;


                if (individualBarWidth < 1) individualBarWidth = 1;


                for (int i = 0; i < trendData.Count; i++)
                {
                    TrendAnalysis.Contracts.TrendDataPoint p = trendData[i];

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

                    DrawingCanvas.Children.Add(bar);
                }
            }
        }


        protected override void OnClosed(EventArgs e)
        {
            if (this.DataContext is MainViewModel viewModel)
            {
                viewModel.DisposeClient();
            }
            base.OnClosed(e);
        }
    }
}