using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Linq;
using TrendAnalysis.ViewModel;
using System.Collections.ObjectModel;
using TrendAnalysis.Service; // Zadrži ovo ako se TrendDataPoint nalazi ovdje

namespace TrendAnalysis.UI
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new MainViewModel();
            this.DataContext = _viewModel;

            _viewModel.DataLoadedAndReadyForRender += RenderChart;

            DrawingCanvas.SizeChanged += DrawingCanvas_SizeChanged;
        }

        private void DrawingCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_viewModel.CurrentTrendData != null && _viewModel.CurrentTrendData.Any())
            {
                RenderChart(_viewModel.CurrentTrendData);
            }
        }

        private void RenderChart(ObservableCollection<TrendDataPoint> trendData)
        {
            DrawingCanvas.Children.Clear();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            int pointsToRender = trendData.Count;

            double canvasWidth = DrawingCanvas.ActualWidth;
            double canvasHeight = DrawingCanvas.ActualHeight;

            if (canvasWidth == 0 || canvasHeight == 0)
            {
                return;
            }

            double margin = 60;
            double plotAreaWidth = canvasWidth - 2 * margin;
            double plotAreaHeight = canvasHeight - 2 * margin;

            if (plotAreaWidth <= 0 || plotAreaHeight <= 0)
            {
                return;
            }

            if (!trendData.Any())
            {
                return;
            }

            DateTime minTimestamp = trendData.Min(p => p.Timestamp);
            DateTime maxTimestamp = trendData.Max(p => p.Timestamp);
            double minValue = trendData.Min(p => p.Value);
            double maxValue = trendData.Max(p => p.Value);

            if (Math.Abs(maxValue - minValue) < Double.Epsilon) maxValue += 1;
            if (maxTimestamp == minTimestamp) maxTimestamp = minTimestamp.AddSeconds(1);

            double scaleX = plotAreaWidth / (maxTimestamp - minTimestamp).TotalSeconds;
            double scaleY = plotAreaHeight / (maxValue - minValue);

            Pen gridPen = new Pen(new SolidColorBrush(Color.FromArgb(50, 100, 100, 100)), 0.5);

            int numHorizontalLines = 5;
            for (int i = 0; i <= numHorizontalLines; i++)
            {
                double yGrid = margin + (plotAreaHeight / numHorizontalLines) * i;
                Line gridLine = new Line { X1 = margin, Y1 = yGrid, X2 = canvasWidth - margin, Y2 = yGrid, Stroke = gridPen.Brush, StrokeThickness = gridPen.Thickness };
                DrawingCanvas.Children.Add(gridLine);
            }

            int numVerticalLines = 6;
            for (int i = 0; i <= numVerticalLines; i++)
            {
                double xGrid = margin + (plotAreaWidth / numVerticalLines) * i;
                Line gridLine = new Line { X1 = xGrid, Y1 = margin, X2 = xGrid, Y2 = canvasHeight - margin, Stroke = gridPen.Brush, StrokeThickness = gridPen.Thickness };
                DrawingCanvas.Children.Add(gridLine);
            }

            Pen axisPen = new Pen(Brushes.DarkGray, 1.5);

            Line xAxis = new Line { X1 = margin, Y1 = canvasHeight - margin, X2 = canvasWidth - margin, Y2 = canvasHeight - margin, Stroke = axisPen.Brush, StrokeThickness = axisPen.Thickness };
            Line yAxis = new Line { X1 = margin, Y1 = margin, X2 = margin, Y2 = canvasHeight - margin, Stroke = axisPen.Brush, StrokeThickness = axisPen.Thickness };
            DrawingCanvas.Children.Add(xAxis);
            DrawingCanvas.Children.Add(yAxis);

            Polyline trendLine = new Polyline();
            trendLine.Stroke = Brushes.DodgerBlue;
            trendLine.StrokeThickness = 2;

            foreach (var point in trendData.Take(pointsToRender))
            {
                double x = margin + (point.Timestamp - minTimestamp).TotalSeconds * scaleX;
                double y = canvasHeight - margin - ((point.Value - minValue) * scaleY);

                trendLine.Points.Add(new Point(x, y));
            }

            DrawingCanvas.Children.Add(trendLine);

            TextBlock title = new TextBlock
            {
                Text = _viewModel.ChartTitle, // Koristi svojstvo iz ViewModela
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Black,
                TextAlignment = TextAlignment.Center,
                Background = Brushes.LightCoral
            };
            Canvas.SetLeft(title, (canvasWidth / 2) - (title.DesiredSize.Width / 2));
            Canvas.SetTop(title, 15);
            DrawingCanvas.Children.Add(title);

            TextBlock xLabel = new TextBlock { Text = "Timestamp", Foreground = Brushes.DarkGray, FontSize = 13, FontWeight = FontWeights.SemiBold };
            Canvas.SetLeft(xLabel, canvasWidth / 2 - xLabel.DesiredSize.Width / 2);
            Canvas.SetTop(xLabel, canvasHeight - margin + 25);
            DrawingCanvas.Children.Add(xLabel);

            TextBlock yLabel = new TextBlock { Text = "Value", Foreground = Brushes.DarkGray, FontSize = 13, FontWeight = FontWeights.SemiBold };
            yLabel.RenderTransform = new RotateTransform(-90);
            Canvas.SetLeft(yLabel, 15);
            Canvas.SetTop(yLabel, canvasHeight / 2 - yLabel.DesiredSize.Height / 2);
            DrawingCanvas.Children.Add(yLabel);

            TextBlock minTsLabel = new TextBlock { Text = minTimestamp.ToString("yyyy-MM-dd\nHH:mm"), Foreground = Brushes.DarkGray, FontSize = 10, TextAlignment = TextAlignment.Center };
            Canvas.SetLeft(minTsLabel, margin - (minTsLabel.DesiredSize.Width / 2));
            Canvas.SetTop(minTsLabel, canvasHeight - margin + 5);
            DrawingCanvas.Children.Add(minTsLabel);

            TextBlock maxTsLabel = new TextBlock { Text = maxTimestamp.ToString("yyyy-MM-dd\nHH:mm"), Foreground = Brushes.DarkGray, FontSize = 10, TextAlignment = TextAlignment.Center };
            Canvas.SetLeft(maxTsLabel, canvasWidth - margin - (maxTsLabel.DesiredSize.Width / 2));
            Canvas.SetTop(maxTsLabel, canvasHeight - margin + 5);
            DrawingCanvas.Children.Add(maxTsLabel);

            TextBlock minValLabel = new TextBlock { Text = minValue.ToString("F2"), Foreground = Brushes.DarkGray, FontSize = 10 };
            Canvas.SetLeft(minValLabel, margin - minValLabel.DesiredSize.Width - 5);
            Canvas.SetTop(minValLabel, canvasHeight - margin - (minValLabel.DesiredSize.Height / 2));
            DrawingCanvas.Children.Add(minValLabel);

            TextBlock maxValLabel = new TextBlock { Text = maxValue.ToString("F2"), Foreground = Brushes.DarkGray, FontSize = 10 };
            Canvas.SetLeft(maxValLabel, margin - maxValLabel.DesiredSize.Width - 5);
            Canvas.SetTop(maxValLabel, margin - (maxValLabel.DesiredSize.Height / 2));
            DrawingCanvas.Children.Add(maxValLabel);

            for (int i = 1; i < numHorizontalLines; i++)
            {
                double yValue = minValue + (maxValue - minValue) * (numHorizontalLines - i) / numHorizontalLines;
                TextBlock valLabel = new TextBlock { Text = yValue.ToString("F2"), Foreground = Brushes.Gray, FontSize = 9 };
                Canvas.SetLeft(valLabel, margin - valLabel.DesiredSize.Width - 5);
                Canvas.SetTop(valLabel, margin + (plotAreaHeight / numHorizontalLines) * i - (valLabel.DesiredSize.Height / 2));
                DrawingCanvas.Children.Add(valLabel);
            }

            for (int i = 1; i < numVerticalLines; i++)
            {
                DateTime dtValue = minTimestamp.AddSeconds((maxTimestamp - minTimestamp).TotalSeconds * i / numVerticalLines);
                TextBlock dtLabel = new TextBlock { Text = dtValue.ToString("HH:mm"), Foreground = Brushes.Gray, FontSize = 9, TextAlignment = TextAlignment.Center };
                Canvas.SetLeft(dtLabel, margin + (plotAreaWidth / numVerticalLines) * i - (dtLabel.DesiredSize.Width / 2));
                Canvas.SetTop(dtLabel, canvasHeight - margin + 5);
                DrawingCanvas.Children.Add(dtLabel);
            }

            stopwatch.Stop();
            long renderingTime = stopwatch.ElapsedMilliseconds;

            MessageBox.Show($"Renderovanje grafikona (softverski, {pointsToRender:N0} tačaka): {renderingTime} ms.", "Informacija", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        protected override void OnClosed(EventArgs e)
        {
            _viewModel?.DisposeClient();
            base.OnClosed(e);
        }
    }
}