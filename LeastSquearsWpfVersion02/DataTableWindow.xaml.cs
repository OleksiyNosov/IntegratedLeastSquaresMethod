/*
 * File: DataTableWindow.xaml.cs
 * --------------------------------------------------
 * In this window user can:
 *      1. View calculated data
 *      2. Save it to excel file
 *      3. Plot data
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LeastSquearsWpfVersion02
{
    /// <summary>
    /// Interaction logic for DataTableWindow.xaml
    /// </summary>
    public partial class DataTableWindow : Window
    {
        private LsData lsData;
        private LsResult lsResult;

        private string filePath;

        private ScrollViewerTbx svBeta;
        private ScrollViewerTbx svE;
        private ScrollViewerTbx svDelta;
        private ScrollViewerTbx svTbeta;
        private ScrollViewerTbx svSingleResults;

        public DataTableWindow(LsData lsData, LsResult lsResult, string filePath)
        {
            InitializeComponent();

            this.lsData = lsData;
            this.lsResult = lsResult;
            this.filePath = filePath;

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            GrResults.Children.Add(svBeta = new ScrollViewerTbx(LsResult.BetaName, lsResult.Beta));
            svBeta.SetValue(Grid.ColumnProperty, 0);

            GrResults.Children.Add(svE = new ScrollViewerTbx(LsResult.EName, lsResult.E));
            svE.SetValue(Grid.ColumnProperty, 1);

            GrResults.Children.Add(svDelta = new ScrollViewerTbx(LsResult.DeltaName, lsResult.Delta));
            svDelta.SetValue(Grid.ColumnProperty, 2);

            GrResults.Children.Add(svTbeta = new ScrollViewerTbx(LsResult.TbetaName, lsResult.Tbeta));
            svTbeta.SetValue(Grid.ColumnProperty, 3);

            GrResults.Children.Add(svSingleResults = new ScrollViewerTbx(string.Empty, string.Empty, false));
            svSingleResults.SetValue(Grid.ColumnProperty, 4);
            svSingleResults.AddItem(LsResult.BetaAvgName, lsResult.BetaAvg.ToString("N7"));
            svSingleResults.AddItem(LsResult.SigmaName, lsResult.SigmaE.ToString("N7"));
            svSingleResults.AddItem(LsResult.RName, lsResult.R.ToString("N7"));
            svSingleResults.AddItem(LsResult.Rp2Name, lsResult.Rp2.ToString("N7"));
            svSingleResults.AddItem(LsResult.FName, lsResult.F.ToString("N7"));
            svSingleResults.AddItem(LsResult.FTableName, lsResult.Ftable.ToString("N7"));

        }

        #region Button click events

        private void BtnChart_Click(object sender, RoutedEventArgs e)
        {
            ChartWindow chartWindow = new ChartWindow(lsData);
            chartWindow.Show();
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            ExcelWorker.Save(lsData, filePath, SaveType.Result);
        }
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        } 

        #endregion
    }
}
