using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
// Dodajte using za vaš generisani WCF servis klijent iz UI projekta
// PAZNJA: OVO CE VEROVATNO BITI CRVENO DOK NE DODAMO REFERENCU U KORAKU 5
using TrendAnalysis.UI.TrendService; // Primer: Vaš generisani WCF klijent namespace

namespace TrendAnalysis.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        private readonly TrendDataServiceClient _client; // Klijent se sada dobija putem Dependency Injectiona
        private ObservableCollection<TrendDataPoint> _currentTrendData;
        private DateTime? _startDate;
        private DateTime? _endDate;
        private string _statusMessage;
        private bool _isLoading;
        private string _selectedRenderMode;
        private ObservableCollection<string> _renderModes;

        public MainViewModel(TrendDataServiceClient client) // Konstruktor prima klijenta
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _currentTrendData = new ObservableCollection<TrendDataPoint>();
            _endDate = DateTime.Today;
            _startDate = DateTime.Today.AddMonths(-1); // Default period za mesec dana unazad

            LoadDataCommand = new RelayCommand(async () => await LoadData(), () => CanLoadData());
            RenderModes = new ObservableCollection<string> { "Line Chart", "Bar Chart" };
            SelectedRenderMode = "Line Chart"; // Default
            StatusMessage = "Welcome to Trend Analysis App!";
        }

        public ObservableCollection<TrendDataPoint> CurrentTrendData
        {
            get => _currentTrendData;
            set => SetProperty(ref _currentTrendData, value);
        }

        public DateTime? StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        public DateTime? EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string SelectedRenderMode
        {
            get => _selectedRenderMode;
            set => SetProperty(ref _selectedRenderMode, value);
        }

        public ObservableCollection<string> RenderModes
        {
            get => _renderModes;
            set => SetProperty(ref _renderModes, value);
        }

        public RelayCommand LoadDataCommand { get; private set; }

        private bool CanLoadData()
        {
            return !IsLoading && StartDate.HasValue && EndDate.HasValue && StartDate.Value <= EndDate.Value;
        }

        private async Task LoadData()
        {
            IsLoading = true;
            StatusMessage = "Loading data...";
            CurrentTrendData.Clear(); // Clear existing data
            try
            {
                var data = await _client.GetTrendDataAsync(StartDate.Value, EndDate.Value);
                if (data != null && data.Any())
                {
                    CurrentTrendData = new ObservableCollection<TrendDataPoint>(data);
                    StatusMessage = $"Loaded {data.Length} records from service.";
                }
                else
                {
                    StatusMessage = "No data found for the selected period.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading data: {ex.Message}";
                // Za detaljnije debugovanje, možete logovati ex.ToString()
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}