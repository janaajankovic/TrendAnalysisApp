using System.Windows;
using TrendService;

namespace TrendAnalysis.UI
{
    public partial class MainWindow : Window
    {
        private TrendDataServiceClient _client;

        public MainWindow()
        {
            InitializeComponent();
            _client = new TrendDataServiceClient();

            EndDatePicker.SelectedDate = DateTime.Now;
            StartDatePicker.SelectedDate = DateTime.Now.AddMonths(-1); 
        }

        private async void LoadData_Click(object sender, RoutedEventArgs e)
        {
            if (!StartDatePicker.SelectedDate.HasValue || !EndDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Molimo odaberite pocetni i krajnji datum.", "Datum Missing", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime startTime = StartDatePicker.SelectedDate.Value.Date; 
            DateTime endTime = EndDatePicker.SelectedDate.Value.Date.AddDays(1).AddSeconds(-1); 

            TrendDataListBox.Items.Clear(); 

            try
            {
                List<TrendDataPoint> trendData = (await _client.GetTrendDataAsync(startTime, endTime)).ToList();

                if (trendData != null && trendData.Any())
                {
                    foreach (var point in trendData.Take(1000)) 
                    {
                        TrendDataListBox.Items.Add($"Timestamp: {point.Timestamp:yyyy-MM-dd HH:mm:ss.fff}, Value: {point.Value:F2}");
                    }

                    MessageBox.Show($"Ucitano {trendData.Count:N0} redova iz servisa.", "Uspjeh", MessageBoxButton.OK, MessageBoxImage.Information);

                }
                else
                {
                    MessageBox.Show("Nema podataka za odabrani vremenski opseg.", "Nema Podataka", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Greska prilikom ucitavanja podataka: {ex.Message}\n\nStackTrace:\n{ex.StackTrace}", "Greska", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_client != null && _client.State == System.ServiceModel.CommunicationState.Opened)
            {
                _client.Close();
            }
            base.OnClosed(e);
        }
    }
}