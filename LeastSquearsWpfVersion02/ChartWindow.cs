/*
 * File: ChartWindow.cs
 * --------------------------------------------------
 * This window create chart with that plot:
 *      Given X, Y,
 *      Calculated Y LS and Y ILS
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using MathNet.Numerics.LinearAlgebra;

namespace LeastSquearsWpfVersion02
{
    public partial class ChartWindow : Form
    {
        private LsData lsData;

        public ChartWindow(LsData lsData)
        {
            InitializeComponent();
            
            this.lsData = lsData;

            ClearChart();

            DrawX();
            DrawY();
            DrawYResults();
        }       

        private void ClearChart()
        {
            chart.Series.Clear();
        }

        private void DrawX()
        {
            for (int j = 0; j < lsData.Parameters.XMtx.ColumnCount; j++)
                DrawSeries(lsData.Parameters.XValsNames[j], lsData.Parameters.XMtx.Column(j), SeriesChartType.Point);
        }
        private void DrawY()
        {
            DrawSeries(lsData.Parameters.YValName, lsData.Parameters.YVtr);
            chart.Series[lsData.Parameters.YValName].Color = Color.Red;
        }
        private void DrawYResults()
        {
            DrawSeriesResults("Y LS", lsData.ResultLs, Color.Blue);
            DrawSeriesResults("Y ILS", lsData.ResultLs, Color.Green);
        }

        private void DrawSeriesResults(string seriesName, LsResult result, Color color)
        {
            if (!result.IsCalculated)
                return;

            DrawSeries(seriesName, result.YRes);
            chart.Series[seriesName].Color = color;
        }

        private void DrawSeries(string seriesName, Vector<double> vtr, SeriesChartType seriesChartType = SeriesChartType.Line)
        {
            chart.Series.Add(seriesName);
            chart.Series[seriesName].ChartType = seriesChartType;

            for (int i = 0; i < lsData.Parameters.YVtr.Count; i++)
                chart.Series[seriesName].Points.AddXY(i, vtr[i]);
        }
    }
}
