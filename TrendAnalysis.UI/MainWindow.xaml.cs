using System.Windows;
using System.Windows.Controls;
using TrendAnalysis.ViewModel;
using TrendAnalysis.UI.AttachedProperties;
using System.Diagnostics;

namespace TrendAnalysis.UI
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            _viewModel = DataContext as MainViewModel;
            if (MyDrawingVisualChart != null) 
            {
                MyDrawingVisualChart.RenderCompleted += MyDrawingVisualChart_RenderCompleted;
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel viewModel)
            {
                _viewModel = this.DataContext as MainViewModel;
                viewModel.PropertyChanged -= ViewModel_PropertyChanged; 
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
            if (MyDrawingVisualChart != null)
            {
                MyDrawingVisualChart.RenderCompleted -= MyDrawingVisualChart_RenderCompleted;
                MyDrawingVisualChart.RenderCompleted += MyDrawingVisualChart_RenderCompleted;
                Debug.WriteLine("Successfully subscribed to MyDrawingVisualChart.RenderCompleted.");
            }
            else
            {
                Debug.WriteLine("WARNING: MyDrawingVisualChart is NULL at MainWindow_Loaded! Check x:Name in XAML.");
            }
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (this.DataContext is MainViewModel viewModel)
            {     
                if (sender is Canvas canvas)
                {
                    CanvasChartExtensions.SetChartData(canvas, CanvasChartExtensions.GetChartData(canvas));
                }
            }
        }

        private void MyDrawingVisualChart_RenderCompleted(object sender, TimeSpan e)
        {
            if (_viewModel != null)
            {
                _viewModel.ChartRenderDuration = e;
                Debug.WriteLine($"ViewModel Updated (DrawingVisual): {e.TotalMilliseconds:F2} ms");

                _viewModel.PerformanceMeasurements.Add(new RenderMeasurement
                {
                    Timestamp = DateTime.Now,
                    RenderingMethod = "Hardware (DrawingVisual)",
                    ChartType = _viewModel.SelectedRenderMode, 
                    NumberOfPoints = _viewModel.CurrentTrendData?.Count ?? 0,
                    RenderDurationMs = e.TotalMilliseconds
                });
                _viewModel.StatusMessage = $"Loaded {_viewModel.CurrentTrendData.Count} records. Chart rendered in {e.TotalMilliseconds:F2} ms.";

            }
            else
            {
                Debug.WriteLine("ERROR: ViewModel is NULL when MyDrawingVisualChart_RenderCompleted fired!");
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