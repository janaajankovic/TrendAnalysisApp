using System.Collections.ObjectModel;
using System.Diagnostics;
using TrendAnalysis.Contracts;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using System.IO;
namespace TrendAnalysis.ViewModel
{
    public class MainViewModel : BaseViewModel
    {
        private readonly ITrendDataService _client;
        private readonly IFileService _fileService;
        private ObservableCollection<TrendDataPoint> _currentTrendData;
        private DateTime? _startDate;
        private DateTime? _endDate;
        private string _statusMessage;
        private bool _isLoading;
        private string _selectedRenderMode;
        private ObservableCollection<string> _renderModes;
        private string _chartTitle;
        private int _loadingProgress;
        public RelayCommand ExportMeasurementsCommand { get; private set; }
        public int LoadingProgress
        {
            get => _loadingProgress;
            set => SetProperty(ref _loadingProgress, value);
        }

        private bool _isIndeterminateProgress;
        public bool IsIndeterminateProgress
        {
            get => _isIndeterminateProgress;
            set => SetProperty(ref _isIndeterminateProgress, value);
        }

        private TimeSpan _chartRenderDuration;
        public TimeSpan ChartRenderDuration
        {
            get => _chartRenderDuration;
            set => SetProperty(ref _chartRenderDuration, value);
        }

        private ObservableCollection<string> _renderingMethods;
        public ObservableCollection<string> RenderingMethods
        {
            get => _renderingMethods;
            set => SetProperty(ref _renderingMethods, value);
        }

        private string _selectedRenderingMethod;
        public string SelectedRenderingMethod
        {
            get => _selectedRenderingMethod;
            set
            {
                if (SetProperty(ref _selectedRenderingMethod, value))
                {
                    if (CurrentTrendData != null && CurrentTrendData.Any())
                    {
                        StatusMessage = "";
                        DataLoadedAndReadyForRender = true;
                        RenderChartBasedOnMethod();
                    }

                    else
                    {
                        StatusMessage = $"Selected rendering method: {value}. Please load data first.";
                    }
                }
            }
        }

        private PlotModel _oxyPlotModel;
        public PlotModel OxyPlotModel
        {
            get => _oxyPlotModel;
            set => SetProperty(ref _oxyPlotModel, value);
        }

        private object _renderTrigger;
        public object RenderTrigger
        {
            get { return _renderTrigger; }
            set
            {
                if (SetProperty(ref _renderTrigger, value)) 
                {
                    System.Diagnostics.Debug.WriteLine($"RenderTrigger in ViewModel changed to: {value}");
                }
            }
        }

        private ObservableCollection<RenderMeasurement> _performanceMeasurements;
        public ObservableCollection<RenderMeasurement> PerformanceMeasurements
        {
            get => _performanceMeasurements;
            set => SetProperty(ref _performanceMeasurements, value);
        }

        public MainViewModel(ITrendDataService client, IFileService fileService)
        {
            _client = client;
            _fileService = fileService;
            _currentTrendData = new ObservableCollection<TrendDataPoint>();
            _endDate = DateTime.Today;
            _startDate = DateTime.Today.AddMonths(-1);
            IsLoading = false;
            LoadingProgress = 0;
            IsIndeterminateProgress = false;
            LoadDataCommand = new RelayCommand(async () => await LoadData(), () => CanLoadData());

            RenderModes = new ObservableCollection<string> { "Line Chart", "Bar Chart" };
            SelectedRenderMode = "Line Chart";

            RenderingMethods = new ObservableCollection<string> { "Software (Canvas)", "Hardware (OxyPlot)", "Hardware (DrawingVisual)" };
            SelectedRenderingMethod = "Software (Canvas)";

            StatusMessage = "Welcome to Trend Analysis App!";
            LoadDataCommand = new RelayCommand(async () => await LoadData(), () => CanLoadData());
            LoadDataCommand.RaiseCanExecuteChanged();
            ExportMeasurementsCommand = new RelayCommand(ExportMeasurements, () => CanExportMeasurements());

            OxyPlotModel = new PlotModel { Title = ChartTitle };
          
            _performanceMeasurements = new ObservableCollection<RenderMeasurement>();
            _performanceMeasurements.CollectionChanged += (s, e) => ExportMeasurementsCommand?.RaiseCanExecuteChanged();


            TrendDataPoint.CanvasChartRenderCompleted += (s, e) =>
            {
                ChartRenderDuration = e;
                Debug.WriteLine($"ViewModel Updated (Canvas): {e.TotalMilliseconds:F2} ms");
                PerformanceMeasurements.Add(new RenderMeasurement
                {
                    Timestamp = DateTime.Now,
                    RenderingMethod = "Software (Canvas)",
                    ChartType = SelectedRenderMode, 
                    NumberOfPoints = CurrentTrendData?.Count ?? 0,
                    RenderDurationMs = e.TotalMilliseconds
                });

                StatusMessage = $"Loaded {CurrentTrendData.Count} records. Chart rendered in {e.TotalMilliseconds:F2} ms.";
            };
        }

        public string ChartTitle
        {
            get => _chartTitle;
            set => SetProperty(ref _chartTitle, value);
        }

        public ObservableCollection<TrendDataPoint> CurrentTrendData
        {
            get => _currentTrendData;
            set => SetProperty(ref _currentTrendData, value);
        }

        private bool _dataLoadedAndReadyForRender;
        public bool DataLoadedAndReadyForRender
        {
            get => _dataLoadedAndReadyForRender;
            set => SetProperty(ref _dataLoadedAndReadyForRender, value);
        }
        public event Action DataReadyForChartRender;

        public DateTime? StartDate
        {
            get { return _startDate; }
            set
            {
                if (_startDate != value)
                {
                    _startDate = value;
                    OnPropertyChanged(nameof(StartDate));
                    ResetChartData();
                }
            }
        }

        public DateTime? EndDate
        {
            get { return _endDate; }
            set
            {
                if (_endDate != value)
                {
                    _endDate = value;
                    OnPropertyChanged(nameof(EndDate));
                    ResetChartData();
                }
            }
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
            set
            {
                if (SetProperty(ref _selectedRenderMode, value))
                {
                    if (SelectedRenderingMethod == "Software (Canvas)")
                    {
                        StatusMessage = "";
                        DataLoadedAndReadyForRender = true;
                        RenderChartBasedOnMethod();
                    }

                    if (SelectedRenderingMethod == "Hardware (OxyPlot)")
                    {
                        RenderChartWithOxyPlot();
                    }
                }
            }

        }

        private void ResetChartData()
        {
            if (CurrentTrendData != null && CurrentTrendData.Any())
            {
                CurrentTrendData = new ObservableCollection<TrendDataPoint>();
            }
            OxyPlotModel.Series.Clear();
            OxyPlotModel.Axes.Clear();
            OxyPlotModel.InvalidatePlot(true);
            OxyPlotModel = null; 
            OnPropertyChanged(nameof(OxyPlotModel));

            StatusMessage = $"Selected rendering method: {SelectedRenderingMethod}. Please load data first.";
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

        public void DisposeClient()
        {
            if (_client is IDisposable disposableClient)
            {
                disposableClient.Dispose();
            }
        }

        private async Task LoadData()
        {
            IsLoading = true;
            IsIndeterminateProgress = true;
            DataLoadedAndReadyForRender = false;
            StatusMessage = "Loading data...";
            LoadingProgress = 0;
            CurrentTrendData.Clear();
            if (SelectedRenderingMethod == "Hardware (DrawingVisual)")
            {
                RenderTrigger = DateTime.Now;
            }

            try
            {
                var data = await _client.GetTrendDataAsync(StartDate.Value, EndDate.Value);
                IsIndeterminateProgress = false;

                
                if (data != null && data.Any())
                {
                    CurrentTrendData = new ObservableCollection<TrendDataPoint>(data);
                    LoadingProgress = 100;
                    RenderChartBasedOnMethod();
                    DataLoadedAndReadyForRender = true;
                }
                else
                {
                    StatusMessage = $"No data found for the selected period. Time elapsed: {ChartRenderDuration.TotalSeconds:F2} seconds.";
                    DataLoadedAndReadyForRender = false;
                    LoadingProgress = 100;
                    OxyPlotModel.Series.Clear();
                    OxyPlotModel.InvalidatePlot(true);

                    CurrentTrendData = new ObservableCollection<TrendDataPoint>();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading data: {ex.Message}. Time elapsed: {ChartRenderDuration.TotalSeconds:F2} seconds.";
                DataLoadedAndReadyForRender = false;
                LoadingProgress = 0;
                OxyPlotModel.Series.Clear();
                OxyPlotModel.InvalidatePlot(true);
                CurrentTrendData = new ObservableCollection<TrendDataPoint>();
            }
            finally
            {
                IsLoading = false;
                IsIndeterminateProgress = false;
                ExportMeasurementsCommand?.RaiseCanExecuteChanged();
                LoadDataCommand.RaiseCanExecuteChanged();
            }
        }

        private void RenderChartBasedOnMethod()
        {
            if (OxyPlotModel == null)
            {
                OxyPlotModel = new PlotModel { Title = ChartTitle };
            }

            if (CurrentTrendData == null || !CurrentTrendData.Any())
            {
                DataReadyForChartRender?.Invoke();
                OxyPlotModel.Series.Clear();
                OxyPlotModel.InvalidatePlot(true);
                return;
            }

            switch (SelectedRenderingMethod)
            {
                case "Software (Canvas)":
                    CurrentTrendData = new ObservableCollection<TrendDataPoint>(CurrentTrendData);
                    break;
                case "Hardware (OxyPlot)":
                    RenderChartWithOxyPlot();
                    break;
                case "Hardware (DrawingVisual)":
                    RenderTrigger = DateTime.Now; 
                    System.Diagnostics.Debug.WriteLine("DrawingVisual render triggered from RenderChartBasedOnMethod.");
                    break;
                default:
                    StatusMessage = "Unknown rendering method selected.";
                    System.Diagnostics.Debug.WriteLine("Unknown rendering method in RenderChartBasedOnMethod.");
                    break;
            }
        }

        private void RenderChartWithOxyPlot()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (OxyPlotModel == null)
            {
                OxyPlotModel = new PlotModel { Title = ChartTitle };
            }

            OxyPlotModel.Series.Clear();
            OxyPlotModel.Axes.Clear();
            OxyPlotModel.Title = ChartTitle;

            List<TrendDataPoint> aggregatedData = new List<TrendDataPoint>();

            if (CurrentTrendData != null && CurrentTrendData.Any())
            {
                aggregatedData = CurrentTrendData
                    .GroupBy(dp => new DateTime(dp.Timestamp.Year, dp.Timestamp.Month, dp.Timestamp.Day, dp.Timestamp.Hour, 0, 0)) 
                    .Select(g => new TrendDataPoint
                    {
                        Timestamp = g.Key,
                        Value = g.Average(dp => dp.Value) 
                    })
                    .OrderBy(dp => dp.Timestamp)
                    .ToList();
            }
            else
            {
                stopwatch.Stop();
                ChartRenderDuration = stopwatch.Elapsed;
                Debug.WriteLine($"RenderChartWithOxyPlot (OxyPlot) completed in {stopwatch.Elapsed.TotalMilliseconds:F2} ms (no data).");
                OxyPlotModel.InvalidatePlot(true);
                return;
            }


            if (SelectedRenderMode == "Line Chart")
            {
                var xAxis = new DateTimeAxis
                {
                    Position = AxisPosition.Bottom,
                    StringFormat = "dd.MM.yyyy",
                    Title = "Datum",
                    MajorGridlineStyle = LineStyle.Solid,
                    MinorGridlineStyle = LineStyle.Dot,
                    IntervalType = DateTimeIntervalType.Auto,
                    IsAxisVisible = true
                };
                OxyPlotModel.Axes.Add(xAxis);

                var yAxis = new LinearAxis
                {
                    Position = AxisPosition.Left,
                    Title = "Vrijednost",
                    MajorGridlineStyle = LineStyle.Solid,
                    MinorGridlineStyle = LineStyle.Dot,
                    IsAxisVisible = true
                };
                OxyPlotModel.Axes.Add(yAxis);

                var lineSeries = new LineSeries
                {
                    Title = "Trend",
                    Color = OxyColors.Blue,
                    StrokeThickness = 1.5,
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 2,
                    MarkerFill = OxyColors.Red
                };

                foreach (var dataPoint in aggregatedData)
                {
                    lineSeries.Points.Add(DateTimeAxis.CreateDataPoint(dataPoint.Timestamp, dataPoint.Value));
                }
                OxyPlotModel.Series.Add(lineSeries);
            }
            else if (SelectedRenderMode == "Bar Chart")
            {
                var categoryAxis = new CategoryAxis
                {
                    Position = AxisPosition.Bottom,
                    Title = "Datum",
                    Key = "CategoryAxisKey",
                    MajorGridlineStyle = LineStyle.Solid,
                    MinorGridlineStyle = LineStyle.Dot,
                    IsAxisVisible = false
                };

                categoryAxis.Labels.Clear();
                foreach (var dataPoint in aggregatedData)
                {
                    categoryAxis.Labels.Add(dataPoint.Timestamp.ToString("dd.MM.yyyy"));
                }
                OxyPlotModel.Axes.Add(categoryAxis);

                var valueAxis = new LinearAxis
                {
                    Position = AxisPosition.Left,
                    Title = "Vrijednost",
                    Key = "ValueAxisKey",
                    MajorGridlineStyle = LineStyle.Solid,
                    MinorGridlineStyle = LineStyle.Dot,
                    IsAxisVisible = true
                };
                OxyPlotModel.Axes.Add(valueAxis);

                var barSeries = new BarSeries
                {
                    Title = "Trend",
                    FillColor = OxyColors.Green,
                    StrokeColor = OxyColors.DarkGreen,
                    StrokeThickness = 1,
                    XAxisKey = valueAxis.Key,
                    YAxisKey = categoryAxis.Key
                };

                for (int i = 0; i < aggregatedData.Count; i++)
                {
                    barSeries.Items.Add(new BarItem(aggregatedData[i].Value, i));
                }
                OxyPlotModel.Series.Add(barSeries);
            }

            OxyPlotModel.InvalidatePlot(true);
            stopwatch.Stop();
            ChartRenderDuration = stopwatch.Elapsed;
            if(PerformanceMeasurements == null)
            {
                _performanceMeasurements = new ObservableCollection<RenderMeasurement>();
            }
            PerformanceMeasurements.Add(new RenderMeasurement
            {
                Timestamp = DateTime.Now,
                RenderingMethod = "Hardware (OxyPlot)",
                ChartType = SelectedRenderMode,
                NumberOfPoints = CurrentTrendData?.Count ?? 0,
                RenderDurationMs = stopwatch.Elapsed.TotalMilliseconds
            });

            StatusMessage = $"Loaded {CurrentTrendData.Count} records. Chart rendered in {ChartRenderDuration.TotalMilliseconds:F2} ms.";
        }

        private void ExportMeasurements()
        {
            string defaultFileName = $"PerformanceMeasurements_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            string filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";

            string filePath = _fileService.SaveFile(defaultFileName, filter);

            if (!string.IsNullOrEmpty(filePath))
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(filePath))
                    {
                        writer.WriteLine("Timestamp,RenderingMethod,ChartType,NumberOfPoints,RenderDurationMs");

                        foreach (var measurement in PerformanceMeasurements)
                        {
                            writer.WriteLine($"{measurement.Timestamp:yyyy-MM-dd HH:mm:ss},{measurement.RenderingMethod},{measurement.ChartType},{measurement.NumberOfPoints},{measurement.RenderDurationMs:F2}");
                        }
                    }
                    StatusMessage = $"Successfully exported to: {filePath}";
                    System.Diagnostics.Debug.WriteLine($"Export successful: {filePath}");
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Greška prilikom izvoza merenja: {ex.Message}";
                    System.Diagnostics.Debug.WriteLine($"Export error: {ex.Message}");
                }
            }
            else
            {
                StatusMessage = "Izvoz merenja otkazan.";
            }
        }

        private bool CanExportMeasurements()
        {
            return PerformanceMeasurements != null && PerformanceMeasurements.Any();
        }
    }

}