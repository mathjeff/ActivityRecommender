using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Drawing;

namespace ActivityRecommendation
{
    class DisplayGrid : Grid
    {
        public DisplayGrid(int rowCount, int columnCount)
        {
            // setup dimensions
            this.numRows = rowCount;
            this.numColumns = columnCount;
            int i;
            for (i = 0; i < rowCount; i++)
            {
                this.RowDefinitions.Add(new RowDefinition());
            }
            for (i = 0; i < columnCount; i++)
            {
                this.ColumnDefinitions.Add(new ColumnDefinition());
            }
            // keep track of which locations have been used
            this.items = new UIElement[rowCount, columnCount];

        }
        
        public void AddItem(UIElement newControl)
        {
            int row, column;
            for (row = 0; row < this.numRows; row++)
            {
                for (column = 0; column < this.numColumns; column++)
                {
                    if (this.items[row, column] == null)
                    {
                        this.SetItem(newControl, row, column);
                        return;
                    }
                }
            }
        }

        public void SetItem(UIElement newControl, int row, int column)
        {
            UIElement previousChild = this.items[row, column];
            if (previousChild != null)
            {
                this.Children.Remove(previousChild);
            }
            Grid.SetRow(newControl, row);
            Grid.SetColumn(newControl, column);
            this.Children.Add(newControl);
            this.items[row, column] = newControl;
        }
        int numRows;
        int numColumns;
        UIElement[,] items;
    }
}
