/*
 * File: MainWindow.xaml.cs
 * --------------------------------------------------
 * This window gives user six options:
 *      1. ILS theory               - Not implemented
 *      2. ILS demonstration
 *      3. ILS and LS calculator
 *      4. User guide               - Not implemented
 *      5. Credits                  - Not implemented
 *      6. Exit
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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LeastSquearsWpfVersion02
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnDemo_Click(object sender, RoutedEventArgs e)
        {
            DemonstrationWindow demoWnd = new DemonstrationWindow();
            demoWnd.Show();
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnCalculate_Click(object sender, RoutedEventArgs e)
        {
            CalculateWindow calculateWindow = new CalculateWindow();
            calculateWindow.Show();
        }
    }
}
