/*
 * File: ExcelWorker.cs
 * ------------------------------------------------------------
 * This class works with excell files:
 *      1. Create sheet for lsData
 *      2. Read data
 *      3. Write data
 *      4. Release memory
 *      5. Have positions for data in file
 */

using System;
using System.IO;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using System.Threading;
using System.Threading.Tasks;

namespace LeastSquearsWpfVersion02
{
    class XlPosition
    {
        public int Column { get; set; }
        public int Row { get; set; }

        public XlPosition() : this(1, 1) { }
        public XlPosition(int row, int column)
        {
            Column = column;
            Row = row;        
        }
    }

    enum SaveType
    {
        All,
        Result,
        Data
    }

    static class ExcelWorker
    {
        public delegate void WriteData(Excel.Workbook xlWorkbook, LsData lsData);

        private static LsData lsData =
            LsDataBuilder.Build(
                2,
                10,
                Vector<double>.Build.Dense(2, 1).ToArray(),
                NoiseTypes.White,
                NoiseTypes.None);

        #region Excel Sheet Positions

        // *Sp means Starting position

        #region Parameters 


        private static XlPosition paramDHeadsSp = new XlPosition(2, 2);
        private static XlPosition paramDValsSp = new XlPosition(paramDHeadsSp.Row, paramDHeadsSp.Column + 1);
        private static XlPosition paramNsHeadSp = new XlPosition(paramDValsSp.Row, paramDValsSp.Column + 2);
        private static XlPosition paramNsTySp = new XlPosition(paramNsHeadSp.Row + 1, paramNsHeadSp.Column + 1);

        #region Default Parameters

        private static string[] paramDHeads =
        {
            "Number of experiments",
            "Number  of X values",
            "Type of noise for X values",
            "Type of noise for Y values",
            "Weight function Gamma coefficient",
            "Weight function Theta coefficient",
        };

        private static string[] paramNsHeads = { "Noise types", "None", "White", "Colorful" };
        private static string[] paramNsTys = { "0", "1", "2" };


        #endregion

        #endregion

        #region Data 

        private static XlPosition dataBHeadSp = new XlPosition(2, 2);
        private static XlPosition dataBValsSp = new XlPosition(dataBHeadSp.Row, dataBHeadSp.Column + 1);

        private static XlPosition dataXHeadsSp = new XlPosition(dataBValsSp.Row + 2, dataBValsSp.Column);
        private static XlPosition dataXValsSp = new XlPosition(dataXHeadsSp.Row + 1, dataXHeadsSp.Column);

        #endregion

        #region Result 

        private static XlPosition resBHeadSp = new XlPosition(2, 2);
        private static XlPosition resBValsSp = new XlPosition(resBHeadSp.Row + 1, resBHeadSp.Column);
        private static XlPosition resEHeadSp = new XlPosition(resBHeadSp.Row, resBHeadSp.Column + 1);
        private static XlPosition resEValsSp = new XlPosition(resEHeadSp.Row + 1, resEHeadSp.Column);
        private static XlPosition resDltHeadSp = new XlPosition(resEHeadSp.Row, resEHeadSp.Column + 1);
        private static XlPosition resDltValsSp = new XlPosition(resDltHeadSp.Row + 1, resDltHeadSp.Column);
        private static XlPosition resTbHeadSp = new XlPosition(resDltHeadSp.Row, resDltHeadSp.Column + 1);
        private static XlPosition resTbValsSp = new XlPosition(resTbHeadSp.Row + 1, resTbHeadSp.Column);


        private static XlPosition resSglHeadsSp = new XlPosition(resTbHeadSp.Row, resTbHeadSp.Column + 1);
        private static XlPosition resSglValsSp = new XlPosition(resSglHeadsSp.Row, resSglHeadsSp.Column + 1);

        #endregion

        #endregion

        #region Public Methods


        public static void Save(LsData lsData, SaveType saveType)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Excel File|*.xls";

            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            Save(lsData, sfd.FileName, saveType);
        }
        public static void Save(LsData lsData, string fileName, SaveType saveType)
        {
            switch (saveType)
            {
                case SaveType.All:
                    CreaeteWorksheet(fileName, lsData, WriteAllLsData);
                    return;
                case SaveType.Result:
                    CreaeteWorksheet(fileName, lsData, WriteResults);
                    return;
                case SaveType.Data:
                    CreaeteWorksheet(fileName, lsData, WriteParametersAndData);
                    return;
                default:
                    return;
            }
        }

        public static void CreateNewExcelWorksheet()
        {
            Save(null, SaveType.Data);
        }

        public static LsData GetData()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            // sfd.Filter = "Excel File|*.xls;*.xlsx;*.xlsm";
            ofd.Filter = "Excel File|*.xls";

            return (ofd.ShowDialog() == DialogResult.OK) ? GetData(ofd.FileName) : null;
        }
        public static LsData GetData(string fileName)
        {
            Excel.Application xlApp = new Excel.Application();
            object misValue = System.Reflection.Missing.Value;

            Excel.Workbook workbook = xlApp.Workbooks.Open(fileName, 0, true, 5, "", "", true, Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);

            return GetData(workbook);

        }


        #endregion


        #region Getting Data

        /*
         * Pre-condition:
         *      Gets workbook
         * 
         * Post-condition:
         *      Read Parameters and Data from excel file
         *      and returns then as LsData
         */
        private static LsData GetData(Excel.Workbook workbook)
        {
            LsData lsData = new LsData();

            GetParameters(lsData, workbook.Sheets["Parameters"]);
            GetData(lsData, workbook.Sheets["Data"]);

            return lsData;
        }

        private static void GetParameters(LsData lsData, Excel.Worksheet sheet)
        {
            lsData.Parameters.NumbOfTests = GetInt(sheet, new XlPosition(paramDValsSp.Row, paramDValsSp.Column));
            lsData.Parameters.NumbOfXVal = GetInt(sheet, new XlPosition(paramDValsSp.Row + 1, paramDValsSp.Column));
            lsData.Parameters.NoiseX = (NoiseTypes)GetInt(sheet, new XlPosition(paramDValsSp.Row + 2, paramDValsSp.Column));
            lsData.Parameters.NoiseY = (NoiseTypes)GetInt(sheet, new XlPosition(paramDValsSp.Row + 3, paramDValsSp.Column));
            lsData.Parameters.Gamma = GetDouble(sheet, new XlPosition(paramDValsSp.Row + 4, paramDValsSp.Column));
            lsData.Parameters.Theta = GetDouble(sheet, new XlPosition(paramDValsSp.Row + 5, paramDValsSp.Column));
        }
        private static void GetData(LsData lsData, Excel.Worksheet sheet)
        {
            lsData.Parameters.InitializeMatrices();

            // Read names of X and Y values
            ReadRow(sheet, lsData.Parameters.XValsNames, dataXHeadsSp);
            lsData.Parameters.YValName = sheet.Cells[dataXHeadsSp.Row, dataXHeadsSp.Column + lsData.Parameters.XMtx.ColumnCount].Value2.ToString();

            ReadRow(sheet, lsData.Parameters.Beta, dataBValsSp);
            ReadTable(sheet, lsData.Parameters.XMtx, dataXValsSp);
            ReadColumn(sheet, lsData.Parameters.YVtr, new XlPosition(dataXValsSp.Row, dataXValsSp.Column + lsData.Parameters.XMtx.ColumnCount));
        }

        #endregion

        private static void CreaeteWorksheet(string fileName, LsData lsData, WriteData writeData, bool visible = true)
        {
            

            Excel.Application xlApp = new Excel.Application();
            if (xlApp == null)
                return;

            if (lsData == null)
                lsData = ExcelWorker.lsData;

            object misValue = System.Reflection.Missing.Value;


            var xlWorkbook = (File.Exists(fileName)) 
                ? xlApp.Workbooks.Open(fileName)
                : xlApp.Workbooks.Add(misValue);

            xlApp.Visible = visible;

            Task.Factory.StartNew(() =>
                {
                    writeData(xlWorkbook, lsData);

                    Save(xlWorkbook, fileName);

                    ReleaseMemory(xlApp, xlWorkbook);
                });

            
        }
        private static void WriteAllLsData(Excel.Workbook xlWorkbook, LsData lsData)
        {
            WriteParametersAndData(xlWorkbook, lsData);
            WriteResults(xlWorkbook, lsData);
        }

        private static void WriteParametersAndData(Excel.Workbook xlWorkbook, LsData lsData)
        {
            CreateParametersWorksheet(xlWorkbook.Worksheets.Item[1], lsData, "Parameters");
            CreateDataWorksheet(xlWorkbook.Worksheets.Add(), lsData, "Data");
        }
        private static void WriteResults(Excel.Workbook xlWorkbook, LsData lsData)
        {
            WriteResults(xlWorkbook, lsData.ResultLs, "Result LS");
            WriteResults(xlWorkbook, lsData.ResultIls, "Result ILS");
        }

        private static void WriteResults(Excel.Workbook xlWorkbook, LsResult lsResult, string sheetName)
        {
            if ( ! lsResult.IsCalculated)
                return;

            try
            {
                xlWorkbook.Worksheets[sheetName].Delete();
            }
            catch (Exception) { }

            CreateReslutWorksheet(xlWorkbook.Worksheets.Add(), lsResult, sheetName);
        }

        private static void CreateParametersWorksheet(Excel.Worksheet sheet, LsData lsData, string sheetName)
        {
            sheet.Name = sheetName;

            sheet.Columns[(new XlPosition()).Column].ColumnWidth = 2.5;
            sheet.Columns[paramDHeadsSp.Column].ColumnWidth = 30;

            SetColumnValues(sheet, paramDHeads, paramDHeadsSp);
            string[] paramsDVals = {
                lsData.Parameters.NumbOfTests.ToString(),
                lsData.Parameters.NumbOfXVal.ToString(),
                ((int)lsData.Parameters.NoiseX).ToString(),
                ((int)lsData.Parameters.NoiseY).ToString(),
                lsData.Parameters.Gamma.ToString(),
                lsData.Parameters.Theta.ToString()};

            SetColumnValues(sheet, paramsDVals, paramDValsSp);


            SetColumnValues(sheet, paramNsHeads, paramNsHeadSp);
            SetColumnValues(sheet, paramNsTys, paramNsTySp);

            ReleaseMemory(sheet);
        }
        private static void CreateDataWorksheet(Excel.Worksheet sheet, LsData lsData, string sheetName)
        {
            sheet.Name = sheetName;

            sheet.Columns[(new XlPosition()).Column].ColumnWidth = 2.5;
            
            SetValue(sheet, LsParameters.BetaName, dataBHeadSp);
            SetRowValues(sheet, lsData.Parameters.XValsNames, dataXHeadsSp);
            SetValue(
                sheet, 
                lsData.Parameters.YValName, 
                new XlPosition(
                    dataXHeadsSp.Row, 
                    dataXHeadsSp.Column + lsData.Parameters.XMtx.ColumnCount));

            SetRowValues(sheet, lsData.Parameters.Beta, dataBValsSp);
            SetTableValues(sheet, lsData.Parameters.XMtx, dataXValsSp);
            SetColumnValues(
                sheet, 
                lsData.Parameters.YVtr, 
                new XlPosition(
                    dataXValsSp.Row, 
                    dataXValsSp.Column + lsData.Parameters.XMtx.ColumnCount));

            ReleaseMemory(sheet);
        }
        private static void CreateReslutWorksheet(Excel.Worksheet sheet, LsResult result, string sheetName)
        {
            sheet.Name = sheetName;
            sheet.Columns[(new XlPosition()).Column].ColumnWidth = 2.5;

            // Setting result values such as: 
            //      Beta, e, Delta, Tbeta            
            //      Sigma, R, R^2, F, F table     

            // Headers
            SetResultHeaders(sheet);

            // Values
            SetResultValues(sheet, result);

            ReleaseMemory(sheet);
        }

        private static void SetResultValues(Excel.Worksheet sheet, LsResult result)
        {
            SetColumnValues(sheet, result.Beta, resBValsSp);
            SetColumnValues(sheet, result.E, resEValsSp);
            SetColumnValues(sheet, result.Delta, resDltValsSp);
            SetColumnValues(sheet, result.Tbeta, resTbValsSp);

            double[] singleResultValues =
            {
                result.BetaAvg,
                result.SigmaE,
                result.R,
                result.Rp2,
                result.F,
                result.Ftable
            };

            SetColumnValues(sheet, Vector<double>.Build.Dense(singleResultValues), resSglValsSp);
        }
        private static void SetResultHeaders(Excel.Worksheet sheet)
        {
            SetValue(sheet, LsResult.BetaName, resBHeadSp);
            SetValue(sheet, LsResult.EName, resEHeadSp);
            SetValue(sheet, LsResult.DeltaName, resDltHeadSp);
            SetValue(sheet, LsResult.TbetaName, resTbHeadSp);
            SetColumnValues(sheet, LsResult.SglNames, resSglHeadsSp);
        }

        #region Reading methods
        /*
         * This code consist of methods that read data 
         * from cell, column, row or table
         * from excel sheet in specified position
         */

        private static void ReadRow(Excel.Worksheet sheet, string[] values, XlPosition position)
        {
            for (int i = 0; i < values.Length; i++)
                values[i] = GetString(sheet, new XlPosition(position.Row, position.Column + i));
        }
        private static void ReadTable(Excel.Worksheet sheet, Matrix<double> matrix, XlPosition position)
        {
            for (int i = 0; i < matrix.RowCount; i++)
                for (int j = 0; j < matrix.ColumnCount; j++)
                    matrix[i, j] = GetDouble(sheet, new XlPosition(position.Row + i, position.Column + j));
        }
        private static void ReadColumn(Excel.Worksheet sheet, Vector<double> values, XlPosition position)
        {
            ReadLine(sheet, values, position, 1, 0);
        }
        private static void ReadRow(Excel.Worksheet sheet, Vector<double> values, XlPosition position)
        {
            ReadLine(sheet, values, position, 0, 1);
        }
        private static void ReadLine(Excel.Worksheet sheet, Vector<double> values, XlPosition position, int rowShift, int columnShift)
        {
            for (int i = 0; i < values.Count; i++)
                values[i] = GetDouble(sheet, new XlPosition(position.Row + i * rowShift, position.Column + i * columnShift));
        }

        private static double GetDouble(Excel.Worksheet sheet, XlPosition position)
        {
            return Convert.ToDouble(GetString(sheet, position));
        }
        private static int GetInt(Excel.Worksheet sheet, XlPosition position)
        {
            return Convert.ToInt32(GetString(sheet, position));
        }

        /*
         * Pre-condition: 
         *      Gets sheets and position on it
         * 
         * Post-condition:
         *      Returns value of cell in specified in sheet
         */
        private static string GetString(Excel.Worksheet sheet, XlPosition position)
        {
            return sheet.Cells[position.Row, position.Column].Value2.ToString();
        }

        #endregion

        #region Writing methods
        /*
         * This code consist of methods that write (set) data 
         * to cell, column, row or table
         * in excel sheet in specified position
         */

        private static void SetColumnValues(Excel.Worksheet sheet, string[] values, XlPosition position)
        {
            SetLineValues(sheet, values, position, 1, 0);
        }
        private static void SetRowValues(Excel.Worksheet sheet, string[] values, XlPosition position)
        {
            SetLineValues(sheet, values, position, 0, 1);
        }
        private static void SetLineValues(Excel.Worksheet sheet, string[] values, XlPosition position, int rowShift, int columnShift)
        {
            for (int i = 0; i < values.Length; i++)
                sheet.Cells[
                    position.Row + i * rowShift,
                    position.Column + i * columnShift]
                        = values[i];
        }

        private static void SetValue(Excel.Worksheet sheet, double value, XlPosition position)
        {
            SetValue(sheet, value.ToString("N7"), position);
        }
        private static void SetValue(Excel.Worksheet sheet, string value, XlPosition position)
        {
            sheet.Cells[
                position.Row,
                position.Column]
                    = value;
        }
        

        private static void SetTableValues(Excel.Worksheet sheet, Matrix<double> dataXVals, XlPosition dataXValsSp)
        {
            for (int i = 0; i < dataXVals.ColumnCount; i++)
                SetColumnValues(sheet, dataXVals.Column(i), new XlPosition(dataXValsSp.Row, dataXValsSp.Column + i));
        }
        private static void SetColumnValues(Excel.Worksheet sheet, Vector<double> values, XlPosition position)
        {
            SetLineValues(sheet, values, position, 1, 0);
        }
        private static void SetRowValues(Excel.Worksheet sheet, Vector<double> values, XlPosition position)
        {
            SetLineValues(sheet, values, position, 0, 1);
        }
        private static void SetLineValues(Excel.Worksheet sheet, Vector<double> values, XlPosition position, int rowShift, int columnShift)
        {
            bool failed;
            for (int i = 0; i < values.Count; i++)
            {
                failed = false;
                do
                {
                    try
                    {
                        sheet.Cells[
                            position.Row + i * rowShift,
                            position.Column + i * columnShift]
                                = values[i].ToString("N7");

                        failed = false;
                    }
                    catch (System.Runtime.InteropServices.COMException)
                    {
                        failed = true;
                        Thread.Sleep(10);
                    }
                } while (failed); 
            }
        } 

        #endregion

        private static void Save(Excel.Workbook xlWorkbook, string fileName)
        {
            object misValue = System.Reflection.Missing.Value;

            try
            {
                xlWorkbook.SaveAs(
                        fileName,
                        Excel.XlFileFormat.xlWorkbookNormal,
                        misValue, misValue, misValue, misValue,
                        Excel.XlSaveAsAccessMode.xlExclusive,
                        misValue, misValue, misValue, misValue, misValue);
            }
            catch (Exception)
            {

            }
        }

        #region ReleaseMemory
        private static void ReleaseMemory(Excel.Application xlApp, Excel.Workbook xlWorkbook)
        {
            ReleaseMemory(xlWorkbook);
            ReleaseMemory(xlApp);
        }
        private static void ReleaseMemory(Excel.Worksheet[] sheets)
        {
            foreach (var s in sheets)
                ReleaseMemory(s);
        }
        private static void ReleaseMemory(object obj)
        {
            try
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
                obj = null;
            }
            catch (Exception ex)
            {
                obj = null;
                Console.WriteLine("Exception Occured while releasing object");
                Console.WriteLine(ex.Message);
            }
            finally
            {
                GC.Collect();
            }
        } 
        #endregion
    }
}

