using ClosedXML.Excel;
using Microsoft.Win32;
using Reveal.Sdk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using WpfReveal3.Models;

namespace WpfReveal3
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
            ConvertExcelToList();
        }

        private void ConvertExcelToList()
        {

            string path = @"Data\Healthcare.xlsx";

            // 0.テーブル名を指定
            string tableName1 = "Hospital Performance";
            // 1.Excel を開く            
            using (var wb = new XLWorkbook(path))
            {
                var sh = wb.Worksheets.FirstOrDefault(t => t.Name == tableName1);
                // 2.シートからEFに読み込み
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

                    this._hospitalPerformances.Add(item);
                    row++;
                }

                this.XamDataGridHospitalPerformance.DataSource = this._hospitalPerformances;
            }


            // 0.テーブル名を指定
            string tableName2 = "Patient Dashboard";
            // 1.Excel を開く           
            using (var wb = new XLWorkbook(path))
            {
                var sh = wb.Worksheets.FirstOrDefault(t => t.Name == tableName2);
                // 2.シートからEFに読み込み
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

                    this._patientDashboard.Add(item);
                    row++;
                }

                this.XamDataGridPatientDashboard.DataSource = this._patientDashboard;
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
    }


}

