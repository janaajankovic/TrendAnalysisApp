using System.Configuration; // Ako ti ovo treba, ostavi. Ako ne, obriši.
using System.Data;          // Ako ti ovo treba, ostavi. Ako ne, obriši.
using System.Windows;
using TrendAnalysis.Contracts;
using TrendAnalysis.UI.Services;
using TrendAnalysis.ViewModel;

namespace TrendAnalysis.UI // OVDE JE SAMO JEDAN NAMESPACE DEKLARACIJA!
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application // Mora biti partial
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // NEMA InitializeComponent() OVDE!

            ITrendDataService trendService = new TrendDataServiceProxy();
            var mainViewModel = new MainViewModel(trendService);
            var mainWindow = new MainWindow { DataContext = mainViewModel };
            mainWindow.Show();
        }
    }
}