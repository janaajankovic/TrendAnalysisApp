# Trend Analysis Application

## Overview

The Trend Analysis Application is a WPF (Windows Presentation Foundation) desktop application designed to visualize time-series data, specifically focusing on trend analysis. It allows users to load historical data within a specified date range, display it using different chart types (Line and Bar), and critically, compare the performance of various rendering methods for the charts (Software (Canvas), Hardware (OxyPlot), and Hardware (DrawingVisual)). The application also provides functionalities to export performance measurements for further analysis.

## Features

* **Data Loading:** Load trend data for a user-defined date range from a backend service.
* **Dynamic Charting:**
    * Display data as Line Charts or Bar Charts.
    * Dynamically switch between chart types.
* **Multiple Rendering Methods:** Compare the performance of:
    * **Software Rendering (Canvas):** Pure WPF Canvas drawing.
    * **Hardware Rendering (OxyPlot):** Utilizes the popular OxyPlot library for high-performance plotting.
    * **Hardware Rendering (DrawingVisual):** Custom low-level drawing using `DrawingVisual` for optimized rendering.
* **Performance Measurement:**
    * Automatically measures and displays the rendering duration for each chart.
* **Measurement Export:** Export collected performance measurements to a CSV file for external analysis.
* **User Feedback:** Provides status messages, loading indicators, and progress updates.
* **MVVM Architecture:** Built following the Model-View-ViewModel (MVVM) design pattern for clean separation of concerns and maintainability.

## Technologies Used

* **WPF (.NET 6/8):** For the desktop application UI.
* **MVVM Light Toolkit (or similar):** For implementing the MVVM pattern (RelayCommand, BaseViewModel/ObservableObject).
* **OxyPlot:** A cross-platform plotting library for .NET, used for one of the hardware rendering methods.
* **Dependency Injection:** Likely used for managing services (`ITrendDataService`, `IFileService`).
* **C#:** The primary programming language.

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes.

### Prerequisites

* [.NET SDK (6.0 or higher recommended)](https://dotnet.microsoft.com/download)
* [Visual Studio 2022 (or Rider)](https://visualstudio.microsoft.com/downloads/)

### Installation

1.  **Clone the repository:**
    ```bash
    git clone [Your Repository URL Here]
    cd TrendAnalysisApp
    ```
2.  **Open in Visual Studio/Rider:**
    Open the `TrendAnalysis.sln` solution file in your preferred IDE.

3.  **Restore NuGet Packages:**
    Visual Studio or Rider should automatically restore the necessary NuGet packages upon opening the solution. If not, you can manually restore them:
    ```bash
    dotnet restore
    ```
4.  **Build the project:**
    Build the solution to ensure all dependencies are resolved and the project compiles correctly.
    `Build > Build Solution` in Visual Studio or `Build > Build Solution` in Rider.

## Running the Application

This application is configured to run both the UI (client) and the backend service simultaneously.

1.  **Configure Multiple Startup Projects:**
    * In Visual Studio/Rider, right-click on your **Solution** (the very top item in Solution Explorer, usually named `TrendAnalysis.sln`).
    * Select **"Set Startup Projects..."** (or "Properties" and then "Startup Project" tab).
    * In the dialog box, choose the **"Multiple startup projects"** option.
    * For the `TrendAnalysis.Service` (or your specific backend project name, e.g., `TrendAnalysis.Api`, `TrendAnalysis.Host`) and `TrendAnalysis.UI` projects, set their "Action" to **"Start"**.
    * You might want to ensure the service project starts before the UI project. You can arrange the order in the list.
    * Click "Apply" and "OK".

2.  **Run the Application:**
    * Press `F5` or click the "Start" button in your IDE.
    * This will launch both the backend service and the WPF UI application.

## How to Use

1.  **Set Date Range:** Select a "Start Date" and "End Date" using the date pickers.
2.  **Load Data:** Click the "Load Data" button. The application will fetch and display data.
3.  **Change Chart Type:** Use the "Render Mode" dropdown to switch between "Line Chart" and "Bar Chart".
4.  **Compare Rendering Methods:** Use the "Rendering Method" dropdown to select "Software (Canvas)", "Hardware (OxyPlot)", or "Hardware (DrawingVisual)". Observe the "Chart Render Duration" and "Status Message" for performance feedback.
5.  **Export Measurements:** After performing several rendering operations, click the "Export Measurements" button to save the collected performance data to a CSV file.

## Project Structure

* `TrendAnalysis.Contracts`: Defines data contracts and service interfaces.
* `TrendAnalysis.Service` (or similar): Implementation of `ITrendDataService`, potentially simulating data retrieval.
* `TrendAnalysis.TrendDataGenerator`: A console application or library used to generate simulated trend data.

* `TrendAnalysis.ViewModel`: Contains ViewModel classes, handling application logic and data binding. `MainViewModel.cs` is the primary ViewModel.
* `TrendAnalysis.UI`: The WPF UI project, containing Views (XAML files) and Attached Properties (`CanvasChartExtensions.cs`).