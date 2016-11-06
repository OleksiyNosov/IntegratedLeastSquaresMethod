/*
 * File: LsData.cs
 * --------------------------------------------------
 * This class contains:
 *      1. Methods of calculating regression analysis
 *      through the standart method of Least Squears (LS) 
 *      and new one Integrated Least Squears (ILS).
 *      2. Data for:
 *          Pre-condition (Parameters)
 *          Post-condition (Results)
 */

using System;
using System.Collections.Generic;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using System.Linq;

using Excel = Microsoft.Office.Interop.Excel;

namespace LeastSquearsWpfVersion02
{    
    public class LsData
    {                  
        /*
         * Matrices used for caluculations
         */ 
        private Matrix<double> AMtx1;
        private Matrix<double> AMtx ;
        private Matrix<double> AMtxM;
        private Vector<double> BVtr;

        private int MCrit;
        private double DetAMtxM;
        private double DetAMtx1;

        public LsParameters Parameters { get; set; }
        public LsResult ResultLs { get; set; }
        public LsResult ResultIls { get; set; }

        
        public LsData(LsParameters parameters)
        {
            Parameters = parameters;
            ResultLs = new LsResult();
            ResultIls = new LsResult();
        }
        public LsData() : this(new LsParameters()) { }


        /*
         * Pre-condition: 
         *      NumbOfXVal and NumbOfTests are initialized
         *      
         * Post-condition: 
         *      Initialize matrices and vectors in Parameters             
         *      and sets its values to zero,           
         *      for the possibility of their use
         */
        private void CreateArrays()
        {
            Parameters.XValsNames = new string[Parameters.NumbOfXVal];

            Parameters.XMtx = Parameters.XMtxNoise = Matrix<double>.Build.Dense(Parameters.NumbOfTests, Parameters.NumbOfXVal, 0);
            Parameters.YVtr = Parameters.YVtrNoise = Vector<double>.Build.Dense(Parameters.NumbOfTests, 0);
            Parameters.Beta = Vector<double>.Build.Dense(Parameters.NumbOfXVal);

            Parameters.XMtxNoise = Matrix<double>.Build.Dense(Parameters.NumbOfTests, Parameters.NumbOfXVal, 0);
        }

        /*
         * Calculate result for least squears 
         * and integrated least squears method
         */
        public void CalcResults()
        {
            CalcLs();
            CalcIls();
        }

        #region Result Calculations
        /*
         * This section makes analysis of 
         * LS or ILS method and set it to Result
         */


        /*
         * Pre-condition: 
         *      Takes Result with calculated Beta vector
         *      
         * Post-condition: 
         *      Makes analysis of Beta
         */
        private void CalcResult(LsResult result)
        {
            result.YRes = CalcYVtrRes(result.Beta);

            result.E = CalcEVtr(result.YRes);
            result.Delta = CalcDeltaVtr(result.E);
            result.Tbeta = CalcTbetaVtr();

            result.SigmaE = CalcSigmaE(result);
            result.Rp2 = CalcRp2(result);
            result.R = CalcR(result);

            result.IsCalculated = true;
        }

        /*
         * Determines approximate Y result     
         * from calculated Beta
         */
        private Vector<double> CalcYVtrRes(Vector<double> beta)
        {
            var yRes = Vector<double>.Build.Dense(Parameters.NumbOfTests, 0);

            for (int i = 0; i < Parameters.XMtx.RowCount; i++)
            {
                double sum = 0;
                for (int j = 0; j < Parameters.XMtx.ColumnCount; j++)
                    sum += Parameters.XMtx[i, j] * beta[j];

                yRes[i] = sum;
            }

            return yRes;
        }

        /* 
         * Finding differences between the observed 
         * and calculated values
         *  e[i] = y[i] - ŷ[i] 
         */
        private Vector<double> CalcEVtr(Vector<double> yRes)
        {
            return Vector<double>.Build.Dense(Parameters.NumbOfTests, (i) => Parameters.YVtr[i] - yRes[i]);
        }
        /*
         * Finding the relative error
         * δ[i] = e[i] / y[i]
         */
        private Vector<double> CalcDeltaVtr(Vector<double> e)
        {
            return Vector<double>.Build.Dense(Parameters.NumbOfTests, (i) => e[i] / Parameters.YVtr[i]);
        }
        /*
         * Check significance coefficients
         */
        private Vector<double> CalcTbetaVtr()
        {
            // TODO: Calculation of significance coefficients
            return Vector<double>.Build.Dense(Parameters.NumbOfTests, 0);
        }

        /*
         * Finding the mean square error variance disturbances
         * √((∑ e[i]^2 ) / (NumbOfTests - NumbOfXVal - 1))
         */
        private double CalcSigmaE(LsResult result)
        {
            return Math.Sqrt(
                result.E.Sum(
                    (i) => i * i / (Parameters.NumbOfTests - Parameters.NumbOfXVal - 1)));
        }
        private double CalcR(LsResult result)
        {
            return Math.Sqrt(result.R);
        }
        /*
         * Finding the coefficient of determination
         */
        private double CalcRp2(LsResult result)
        {
            var yAvg = Parameters.YVtr.Average();

            return
                1
                - (result.E.Sum((i) => i * i))
                / (Parameters.YVtr.Sum((i) => Math.Pow(i - yAvg, 2)));
        }

        /*
         * Check the adequacy of the model 
         * by using Fischer statistics
         * ang getting F and F-table
         */
        private double CalcFTable()
        {
            return 0;
        }
        private double CalcF()
        {
            return 0;
        }

        #endregion

        #region Least Squears Method

        /*
         * Pre-condition: 
         *      X (XMtx) and Y (XMtx) values are initialized
         *      
         * Post-condition: 
         *      Calculate Beta by using LS method 
         *      and set it to the ResultLs
         */
        public void CalcLs()
        {
            
            (ResultLs = new LsResult()).
                // β = ((X^T) ∙ X)^(-1) ∙ (X^T) ∙ Y
                Beta = Parameters.XMtx.TransposeThisAndMultiply(Parameters.XMtx).Inverse().Multiply(Parameters.XMtx.Transpose()).Multiply(Parameters.YVtr);

            CalcResult(ResultLs);
        }

        #endregion

        #region Intagrated Least Squears Method


        /*
         * Pre-condition: 
         *      X (XMtx) and Y (XMtx) values are initialized
         *      
         * Post-condition: 
         *      Calculate Beta by using ILS method 
         *      and set it to the ResultIls
         */
        public void CalcIls()
        {
            CalcMCrit();
            CalcAMtx();
            CalcBVtr();

            (ResultIls = new LsResult()).Beta = AMtx.Inverse().Multiply(BVtr);

            CalcResult(ResultIls);
        }

        private void CalcMCrit()
        {
            MCrit = 1;

            AMtx1 = Matrix<double>.Build.Dense(Parameters.NumbOfXVal, Parameters.NumbOfXVal, (i, j) => CalcMtxElem(i, j, EtaStd, AMtxElemFunc));

            Normalize(AMtx1);

            DetAMtx1 = AMtx1.Determinant();

            do
            {
                MCrit++;

                AMtxM = Matrix<double>.Build.Dense(Parameters.NumbOfXVal, Parameters.NumbOfXVal, (i, j) => CalcMtxElem(i, j, EtaStd, AMtxElemFunc));

                Normalize(AMtxM);

                DetAMtxM = AMtxM.Determinant();

            } while (Math.Abs(DetAMtxM) > 0.1 * Math.Abs(DetAMtx1));
        }

        private void Normalize(Matrix<double> matrix)
        {
            for (int i = 0; i < matrix.RowCount; i++)
            {
                var divElem = matrix[i, i];
                for (int j = 0; j < matrix.ColumnCount; j++)
                    matrix[i, j] /= divElem;
            }
        }

        private void CalcAMtx()
        {
            AMtx = Matrix<double>.Build.Dense(Parameters.NumbOfXVal, Parameters.NumbOfXVal, (i, j) => CalcMtxElem(i, j, EtaNew, AMtxElemFunc));
        }
        private void CalcBVtr()
        {
            BVtr = Vector<double>.Build.Dense(Parameters.NumbOfXVal, (j) => CalcMtxElem(j, j, EtaNew, BVtrElemFunc));
        }

        private double CalcMtxElem(int i, int j, Func<int, double> eta, Func<int, int, int, int, double> elemFunc)
        {
            int N1 = Parameters.NumbOfTests - MCrit;
            double retval = 0;

            for (int k = MCrit; k < N1; k++)
                for (int l = -MCrit; l < MCrit; l++)
                    retval += eta(l) * elemFunc(k, l, i, j);

            return retval;
        }

        private double AMtxElemFunc(int k, int l, int i, int j)
        {
            return (Parameters.XMtx[k, j] * Parameters.XMtx[k + l, i]) + (Parameters.XMtx[k, i] * Parameters.XMtx[k + l, j]);
        }
        private double BVtrElemFunc(int k, int l, int i, int j)
        {
            return (Parameters.YVtr[k + l] * Parameters.XMtx[k, j]) + (Parameters.YVtr[k] * Parameters.XMtx[k + l, j]);
        }    
         
        private double EtaStd(int l)
        {
            return (Math.Abs(l) < MCrit) ? 0 : 1;
        }
        private double EtaNew(int l)
        {
            return
                  Math.Pow(1 + Math.Abs(l), Parameters.Theta)
                * Math.Pow(1 - Math.Cos((2 * Math.PI * Math.Abs(l)) / MCrit), Parameters.Gamma);
        }
               
        #endregion

        #region ILsDataWriteToExcel
        public void Save(string path)
        {
            Excel.Application excelApp = new Excel.Application();

            if (excelApp == null)
            {
                return;
            }

            Excel.Workbook excelWorkbook;
            Excel.Worksheet excelWorksheet;
            object misValue = System.Reflection.Missing.Value;

            excelWorkbook = excelApp.Workbooks.Add(misValue);
            excelWorksheet = (Excel.Worksheet)excelWorkbook.Worksheets.Item[1];


            WriteLsDataHeadersToExcel(excelWorksheet);
            WriteLsDataToExcel(excelWorksheet);

            excelWorkbook.SaveAs(
                path,
                Excel.XlFileFormat.xlWorkbookNormal,
                misValue, misValue, misValue, misValue,
                Excel.XlSaveAsAccessMode.xlExclusive,
                misValue, misValue, misValue, misValue, misValue);

            excelApp.Visible = true;

            ReleaseMemory(excelWorksheet);
            ReleaseMemory(excelWorkbook);
            ReleaseMemory(excelApp);

        }

        private void WriteLsDataHeadersToExcel(Excel.Worksheet excelWorksheet)
        {

            for (int i = 0; i < Parameters.XMtx.ColumnCount; i++)
                excelWorksheet.Cells[1, 1 + i] = $"X{i}";

            excelWorksheet.Cells[1, Parameters.XMtx.ColumnCount + 1] = "Y";
        }

        private void WriteLsDataToExcel(Excel.Worksheet excelWorksheet)
        {
            for (int j = 0; j < Parameters.XMtx.ColumnCount; j++)
            {
                for (int i = 0; i < Parameters.XMtx.RowCount; i++)
                {
                    excelWorksheet.Cells[2 + i, 1 + j] = Parameters.XMtx[i, j];
                }
            }

            for (int i = 0; i < Parameters.YVtr.Count; i++)
            {
                excelWorksheet.Cells[2 + i, Parameters.XMtx.ColumnCount + 1] = Parameters.XMtx[i, 0];
            }
        }

        public void ReleaseMemory(object obj)
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