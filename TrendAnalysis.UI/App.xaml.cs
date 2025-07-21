using System.Windows;
using TrendAnalysis.Contracts;
using TrendAnalysis.UI.Services;
using TrendAnalysis.ViewModel;

namespace TrendAnalysis.UI 
{
    public partial class App : Application 
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ITrendDataService trendService = new TrendDataServiceProxy();
            var mainViewModel = new MainViewModel(trendService);
            var mainWindow = new MainWindow { DataContext = mainViewModel };
            mainWindow.Show();
        }
    }
}