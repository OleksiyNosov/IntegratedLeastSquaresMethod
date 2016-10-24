/*
 * File: CalculateWindow.xaml.cs
 * --------------------------------------------------
 * In this window user can:
 *      1. Create worksheet in excel
 *      2. Load data from excel file
 *      3. Calculate result data and open DataTable
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WinForms = System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

namespace LeastSquearsWpfVersion02
{
    /// <summary>
    /// Interaction logic for CalculateWindow.xaml
    /// </summary>
    public partial class CalculateWindow : Window
    {
        LsData lsData;
        string fileName;

        public CalculateWindow()
        {
            InitializeComponent();
        }

        private void BtnCreateExcelWorksheet_Click(object sender, RoutedEventArgs e)
        {
            ExcelWorker.CreateNewExcelWorksheet();
        }

        private void BtnLoadData_Click(object sender, RoutedEventArgs e)
        {
            WinForms.OpenFileDialog ofd = new WinForms.OpenFileDialog();
            ofd.Filter = "Excel File|*.xls";

            lsData = 
                (ofd.ShowDialog() == WinForms.DialogResult.OK) 
                    ? ExcelWorker.GetData(fileName = ofd.FileName) 
                    : null;
        }
        
        private void BtnLsMethod_Click(object sender, RoutedEventArgs e)
        {
            if (lsData == null)
                return;

            lsData.CalcLs();

            DataTableWindow dataTableWindow = new DataTableWindow(lsData, lsData.ResultLs, fileName);
            dataTableWindow.Show();
        }
        private void BtnIlsMethod_Click(object sender, RoutedEventArgs e)
        {
            lsData.CalcIls();

            DataTableWindow dataTableWindow = new DataTableWindow(lsData, lsData.ResultIls, fileName);
            dataTableWindow.Show();
        }
    }
}
