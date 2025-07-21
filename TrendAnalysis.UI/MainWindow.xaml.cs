using System.Windows;
using System.Windows.Controls;
using TrendAnalysis.ViewModel;
using TrendAnalysis.UI.AttachedProperties;

namespace TrendAnalysis.UI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel viewModel)
            {
                viewModel.PropertyChanged -= ViewModel_PropertyChanged; 
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
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