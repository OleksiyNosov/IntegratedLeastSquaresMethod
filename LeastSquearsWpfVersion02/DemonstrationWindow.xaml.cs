/*
 * File: DemonstrationWindow.xaml.cs
 * --------------------------------------------------
 * This window demostrate Integrated Least Squears method
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.CompilerServices;


namespace LeastSquearsWpfVersion02
{
    /// <summary>
    /// Interaction logic for DemonstrationWindow.xaml
    /// </summary>
    public partial class DemonstrationWindow : Window
    {
        private NoiseTypes noiseX;
        private NoiseTypes noiseY;

        private ScrollViewerTbx scrollViewerTbx; 

        LsData lsData;

        public DemonstrationWindow()
        {
            InitializeComponent();
            InitializeScrollViewer();
        }

        /* 
         * Creating ScrollViewer and adding it to the grid
         * Here user can set values of beta 
         * which has influence for Y 
         */
        private void InitializeScrollViewer()
        {
            gridParams.Children.Add(scrollViewerTbx = new ScrollViewerTbx(LsParameters.BetaName, "1"));
            scrollViewerTbx.SetValue(Grid.ColumnProperty, 1);
        }

        #region Noise
        /*
         * This section determines what type of noise user have selected 
         * and authomatically set result to the noiseX and noiseY
         */

        private void DetermineNoiseType()
        {
            if (TgBtnWhiteNoise.IsChecked == true)
            {
                noiseX = CbxNoiseInXValues.IsChecked == true ? NoiseTypes.White : NoiseTypes.None;
                noiseY = CbxNoiseOutYValues.IsChecked == true ? NoiseTypes.White : NoiseTypes.None;
            }
            else if (TgBtnColorfulNoise.IsChecked == true)
            {
                noiseX = CbxNoiseInXValues.IsChecked == true ? NoiseTypes.Colorful : NoiseTypes.None;
                noiseY = CbxNoiseOutYValues.IsChecked == true ? NoiseTypes.Colorful : NoiseTypes.None;
            }
            else
                noiseX = noiseY = NoiseTypes.None;
        }

        private void BtnColorfulNoise_Click(object sender, RoutedEventArgs e)
        {
            EnableCbxWithNoiseValues((bool)(sender as ToggleButton).IsChecked);

            if ((bool)TgBtnWhiteNoise.IsChecked)
                TgBtnWhiteNoise.IsChecked = false;
        }
        private void BtnWhiteNoise_Click(object sender, RoutedEventArgs e)
        {
            EnableCbxWithNoiseValues((bool)(sender as ToggleButton).IsChecked);

            if ((bool)TgBtnColorfulNoise.IsChecked)
                TgBtnColorfulNoise.IsChecked = false;
        }


        /*
         * Enable or disable CheckBoxes depends on 
         * if ToggleButton is pushed or not
         */
        private void EnableCbxWithNoiseValues(bool value)
        {
            CbxNoiseInXValues.IsEnabled = value;
            CbxNoiseOutYValues.IsEnabled = value;
        }

        #endregion

        #region Events

        /*
         * Authomatically add Labels and TextBoxes to the ScrollViewerTbx
         * depends on value which set on the slider
         */
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = sender as Slider;

            scrollViewerTbx.AutoChangeItensInScrollViewer(slider.Value);
        }

        /*
         * Pre-condition:
         *      numberOfXValues, numberOfExperiments,
         *      beta and noise are set
         * Post-condition:
         *      Create lsData which can be used in further calculations     
         */
        private void BtnGenerateData_Click(object sender, RoutedEventArgs e)
        {
            if (SldNumberOfXValues.Value < 2)
                return;

            int numberOfXValues = (int)SldNumberOfXValues.Value;
            int numberOfExperiments = Convert.ToInt32(TbxNumberOfExperiments.Text);
            double[] beta = scrollViewerTbx.GetBeta();

            DetermineNoiseType();

            lsData =
                LsDataBuilder.Build(
                    numberOfXValues,
                    numberOfExperiments,
                    beta,
                    noiseX,
                    noiseY,
                    Convert.ToDouble(TbxGamma.Text),
                    Convert.ToDouble(TbxTheta.Text));
        }
        /*
         * Open FileDialog and
         * save data to excel file
         */
        private void BtnDataSaveToExcel_Click(object sender, RoutedEventArgs e)
        {
            if (lsData == null)
                return;

            ExcelWorker.Save(lsData, SaveType.All);
        }
        /*
         * Pre-condition: 
         *      lsData is initialized
         * Post-Condition:
         *      Calculate data with LS and ILS methods
         *      and show chart with builded data        
         */
        private void BtnIntegratedLeastSquares_Click(object sender, RoutedEventArgs e)
        {
            if (lsData == null)
                return;

            lsData.CalcResults();

            ChartWindow chartWindow = new ChartWindow(lsData);
            chartWindow.Show();
        } 

        #endregion

    }
}
