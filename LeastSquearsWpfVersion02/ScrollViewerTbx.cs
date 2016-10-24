/*
 * File: ScrollViewerTbx.cs
 * ----------------------------------------
 * This class is used a UI for easier 
 * representing of data in columns
 */

using System;
using System.Windows;
using System.Windows.Controls;
using MathNet.Numerics.LinearAlgebra;
using System.Text;

namespace LeastSquearsWpfVersion02
{
    public class ScrollViewerTbx : ScrollViewer
    {
        private Grid mGrid;
        private ColumnDefinition[] mGridColDef;
        private StackPanel stPanLabels;
        private StackPanel stPanTextBoxes;

        private bool isCount;

        private string defLbContent;
        private string defTbxValue;

        private double minTbxHeight;

        public ScrollViewerTbx(string defLbContent = "", string defTbxValue = "", bool isCount = true, double minTbxHeight = 20)
        {
            AddChild(mGrid = new Grid());
            AddElementsToGrid();

            this.defLbContent = defLbContent;
            this.defTbxValue = defTbxValue;

            this.minTbxHeight = minTbxHeight;

            this.isCount = isCount;

        }

        public ScrollViewerTbx(string defLbContent, Vector<double> beta) : this(defLbContent) 
        {
            foreach (var val in beta)
                AddItem(defLbContent, val.ToString("N7"));
        }

        private void AddElementsToGrid()
        {
            // Adding columns
            mGridColDef = new ColumnDefinition[2];
            for (int i = 0; i < mGridColDef.Length; i++)
                mGrid.ColumnDefinitions.Add(mGridColDef[i] = new ColumnDefinition());

            // Set columns characteristics
            mGridColDef[0].Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Auto);
            mGridColDef[1].Width = new System.Windows.GridLength(3, System.Windows.GridUnitType.Star);

            // Adding StackPanels
            mGrid.Children.Add(stPanLabels = new StackPanel());
            mGrid.Children.Add(stPanTextBoxes = new StackPanel());

            // Set StackPanels columns
            stPanLabels.SetValue(Grid.ColumnProperty, 0);
            stPanTextBoxes.SetValue(Grid.ColumnProperty, 1);

            stPanLabels.HorizontalAlignment = HorizontalAlignment.Left;
        }

        public double[] GetBeta(double defVar = 1)
        {
            double[] arrayOfBetaValues = new double[stPanTextBoxes.Children.Count];

            for (int i = 0; i < stPanTextBoxes.Children.Count; i++)
            {
                try
                {
                    arrayOfBetaValues[i] = Convert.ToDouble((stPanTextBoxes.Children[i] as TextBox).Text);
                }
                catch (Exception)
                {
                    arrayOfBetaValues[i] = 1;
                }
            }

            return arrayOfBetaValues;
        }

        public void AutoChangeItensInScrollViewer(double sliderValue)
        {
            if (stPanLabels == null)
                return;

            if (stPanLabels.Children.Count < sliderValue)
            {
                do
                {
                    AddItem();
                } while (stPanLabels.Children.Count < sliderValue);
                return;
            }

            if (stPanLabels.Children.Count > sliderValue)
            {
                do
                {
                    DeleteItemsFromScrollViewer();
                } while (stPanLabels.Children.Count > sliderValue);
                return;
            }
        }

        public void AddItem(string lbContent, string tbxValue)
        {
            AddNewLabelToScrollViewer(lbContent);
            AddNewTextBoxToScrollViewer(tbxValue);
        }
        public void AddItem()
        {
            AddNewLabelToScrollViewer(defLbContent);
            AddNewTextBoxToScrollViewer(defTbxValue);
        }

        private void AddNewLabelToScrollViewer(string lbContent)
        {
            var content = new StringBuilder();
            content.Append(lbContent);
            if (isCount)
                content.Append(stPanLabels.Children.Count);

            stPanLabels.Children.Add(new Label()
            {
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,

                MinHeight = minTbxHeight + 10,
                Content = content.ToString(),
            });
        }
        private void AddNewTextBoxToScrollViewer(string tbxValue)
        {
            stPanTextBoxes.Children.Add(new TextBox()
            {
                VerticalContentAlignment = VerticalAlignment.Center,
                MinHeight = minTbxHeight,
                Margin = new Thickness(0, 5, 5, 5),
                Text = tbxValue,
                HorizontalContentAlignment = HorizontalAlignment.Center,
            });
        }

        private void DeleteItemsFromScrollViewer()
        {
            DeleteLastItemFrom(stPanLabels);
            DeleteLastItemFrom(stPanTextBoxes);
        }
        private void DeleteLastItemFrom(StackPanel stackPanel)
        {
            if (stackPanel.Children.Count > 0)
                stackPanel.Children.RemoveAt(stackPanel.Children.Count - 1);
        }
    }
}