using Microsoft.Win32;
using Reveal.Sdk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace WpfReveal1
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {

        string _defaultDirectory = Path.Combine(Environment.CurrentDirectory, "Dashboards");

        public MainWindow()
        {
            InitializeComponent();

            _revealView.Dashboard = new RVDashboard();

            RevealSdkSettings.LocalDataFilesRootFolder = Path.Combine(Environment.CurrentDirectory, "Data");


            var filePath = Path.Combine(Environment.CurrentDirectory, "Dashboards/HealthCare.rdash");
            _revealView.Dashboard = new RVDashboard(filePath);
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

