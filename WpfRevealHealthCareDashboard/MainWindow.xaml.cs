using ClosedXML.Excel;
using Microsoft.Win32;
using Reveal.Sdk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WpfRevealHealthCareDashboard.Models;

namespace WpfRevealHealthCareDashboard
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {

        string _defaultDirectory = Path.Combine(Environment.CurrentDirectory, "Dashboards");

        IList<HospitalPerformance> _hospitalPerformances = new List<HospitalPerformance>();
        IList<PatientDashboard> _patientDashboard = new List<PatientDashboard>();

        public MainWindow()
        {
            InitializeComponent();

            //Reveal関連の初期化処理
            _revealView.Dashboard = new RVDashboard();

            RevealSdkSettings.LocalDataFilesRootFolder = Path.Combine(Environment.CurrentDirectory, "Data");

            var filePath = Path.Combine(Environment.CurrentDirectory, "Dashboards/HealthCare.rdash");
            _revealView.Dashboard = new RVDashboard(filePath);

            //Excelファイルを読み込んで、DataGridにBindする
            BindExcelToXamDataGrid();
        }

        private void BindExcelToXamDataGrid()
        {

            string path = @"Data\Healthcare.xlsx";

            // Hospital Performanceの読み込み
            string tableName1 = "Hospital Performance";

            using (var wb = new XLWorkbook(path))
            {
                var sh = wb.Worksheets.FirstOrDefault(t => t.Name == tableName1);

                int row = 2;
                while (sh.Cell(row, 1).Value.ToString() != "")
                {
                    var item = new HospitalPerformance();
                    item.Date = DateTime.Parse(sh.Cell(row, 1).Value.ToString());
                    item.Patients = int.Parse(sh.Cell(row, 2).Value.ToString());
                    item.Gender = sh.Cell(row, 3).Value.ToString();
                    item.PatientType = sh.Cell(row, 4).Value.ToString();
                    item.BedOccupancyRate = double.Parse(sh.Cell(row, 5).Value.ToString());
                    item.Doctor = sh.Cell(row, 6).Value.ToString();
                    item.Specialist = sh.Cell(row, 7).Value.ToString();

                    _hospitalPerformances.Add(item);
                    row++;
                }

                XamDataGridHospitalPerformance.DataSource = _hospitalPerformances;
            }


            // Patient Dashboardの読み込み
            string tableName2 = "Patient Dashboard";

            using (var wb = new XLWorkbook(path))
            {
                var sh = wb.Worksheets.FirstOrDefault(t => t.Name == tableName2);

                int row = 2;
                while (sh.Cell(row, 1).Value.ToString() != "")
                {
                    var item = new PatientDashboard();
                    item.Date = DateTime.Parse(sh.Cell(row, 1).Value.ToString());
                    item.Gender = sh.Cell(row, 2).Value.ToString();
                    item.PatientType = sh.Cell(row, 3).Value.ToString();
                    item.Patient = sh.Cell(row, 4).Value.ToString();
                    item.Weight = int.Parse(sh.Cell(row, 5).Value.ToString());
                    item.HeartRate = double.Parse(sh.Cell(row, 6).Value.ToString());
                    item.Age = sh.Cell(row, 7).Value.ToString();
                    item.VisitReason = sh.Cell(row, 8).Value.ToString();
                    item.MedicationGiven = sh.Cell(row, 9).Value.ToString();

                    _patientDashboard.Add(item);
                    row++;
                }

                XamDataGridPatientDashboard.DataSource = _patientDashboard;
            }
        }

        private void RevealView_DataSourcesRequested(object sender, Reveal.Sdk.DataSourcesRequestedEventArgs e)
        {
            var dataSources = new List<RVDashboardDataSource>();
            var items = new List<RVDataSourceItem>();


            var localFileItem = new RVLocalFileDataSourceItem();
            localFileItem.Uri = "local:/Healthcare.xlsx";

            var excelDataSourceItem = new RVExcelDataSourceItem(localFileItem);
            excelDataSourceItem.Title = "Local Excel File";
            items.Add(excelDataSourceItem);


            e.Callback(new RevealDataSources(dataSources, items, true));
        }

        private async void RevealView_SaveDashboard(object sender, Reveal.Sdk.DashboardSaveEventArgs e)
        {
            if (e.IsSaveAs)
            {
                var saveDialog = new SaveFileDialog()
                {
                    DefaultExt = ".rdash",
                    FileName = e.Name + ".rdash",
                    Filter = "Reveal Dashboard (*.rdash)|*.rdash",
                    InitialDirectory = _defaultDirectory
                };

                if (saveDialog.ShowDialog() == true)
                {
                    using (var stream = new FileStream(saveDialog.FileName, FileMode.Create, FileAccess.Write))
                    {
                        var name = Path.GetFileNameWithoutExtension(saveDialog.FileName);
                        var data = await e.SerializeWithNewName(name);
                        await stream.WriteAsync(data, 0, data.Length);
                    }
                }
            }
            else
            {
                var path = Path.Combine(_defaultDirectory, $"{e.Name}.rdash");
                var data = await e.Serialize();
                using (var output = File.Open(path, FileMode.Open))
                {
                    output.Write(data, 0, data.Length);
                }
            }

            e.SaveFinished();
        }

        private DateTime _from;
        private DateTime _to;


        private void toDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_revealView == null)
            {
                return;
            }
            var value = toDate.SelectedDate.Value;

            var timer = new DispatcherTimer();
            timer.Tick += (s, args) =>
            {
                _to = value;
                UpdateDateFilter();
                timer.Stop();
            };
            timer.Start();
        }

        private void fromDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_revealView == null)
            {
                return;
            }
            var value = fromDate.SelectedDate.Value;

            var timer = new DispatcherTimer();
            timer.Tick += (s, args) =>
            {
                _from = value;
                UpdateDateFilter();
                timer.Stop();
            };
            timer.Start();
        }


        private void UpdateDateFilter()
        {
            var from = AdjustFromDate(_from);
            var to = AdjustToDate(_to);
            var range = new RVDateRange(from, to);
            var filter = new RVDateDashboardFilter(RVDateFilterType.CustomRange, range);

            _from = from;
            _to = to;
            //fromThumb.Value = _from;
            //toThumb.Value = _to;

            _revealView.Dashboard.DateFilter = filter;
        }

        private DateTime AdjustFromDate(DateTime from)
        {
            return new DateTime(from.Year, from.Month, 1);
        }

        private DateTime AdjustToDate(DateTime to)
        {
            return new DateTime(to.Year, to.Month, 1).AddMonths(1).AddDays(-1);
        }
    }


}

