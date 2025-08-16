using System.Windows;
using TrendAnalysis.Contracts;
using TrendAnalysis.UI.Services;
using TrendAnalysis.ViewModel;

namespace TrendAnalysis.UI 
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ITrendDataService trendService = new TrendDataServiceProxy();
            IFileService fileService = new WpfFileService();
            var mainViewModel = new MainViewModel(trendService, fileService);
            var mainWindow = new MainWindow { DataContext = mainViewModel };
            mainWindow.Show();
        }
    }
}