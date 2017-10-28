/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
//using System.Drawing;

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
            this.actualColumnWidths = new double[numColumns];
            this.actualRowHeights = new double[numRows];
            // keep track of which locations have been used
            this.items = new UIElement[rowCount, columnCount];
            this.itemsAsIResizables = new IResizable[rowCount, columnCount];

            this.nextOpenRow = this.nextOpenColumn = 0;

            this.doneMeasuring = true;
        }

        #region Adding Items

        // puts a UIElement into the next available spot in the grid
        public void AddItem(UIElement newControl)
        {
            this.FindNextOpenLocation();
            this.PutItem(newControl, this.nextOpenRow, this.nextOpenColumn);
        }

        // sets this.nextOpenRow and this.nextOpenColumn
        private void FindNextOpenLocation()
        {
            int row = this.nextOpenRow;
            int column = this.nextOpenColumn;
            for (row = this.nextOpenRow; row < this.numRows; row++)
            {
                for (; column < this.numColumns; column++)
                {
                    if (this.items[row, column] == null)
                    {
                        this.nextOpenRow = row;
                        this.nextOpenColumn = column;
                        return;
                    }
                }
                column = 0;
            }
        }

        // puts the given item at the given spot
        public void PutItem(UIElement newControl, int rowIndex, int columnIndex)
        {
            UIElement previousChild = this.items[rowIndex, columnIndex];
            if (previousChild != null)
            {
                this.Children.Remove(previousChild);
            }
            if (newControl != null)
            {
                Grid.SetRow(newControl, rowIndex);
                Grid.SetColumn(newControl, columnIndex);
                this.Children.Add(newControl);
            }
            this.items[rowIndex, columnIndex] = newControl;
            // keep track of which items implement the interface IResizable, telling how to resize them
            if (newControl is IResizable)
                this.itemsAsIResizables[rowIndex, columnIndex] = (IResizable)newControl;
            else
                this.itemsAsIResizables[rowIndex, columnIndex] = null;
            // recalculate how to display everything
            this.InvalidateVisual();
        }

        #endregion

        #region Measuring

        // call this if it is not the final Measure
        public virtual Size PreliminaryMeasure(Size constraint)
        {
            // reset the desired sizes if we're not in the middle of measuring
            if (this.doneMeasuring)
            {
                this.InitializeDesiredSizes(constraint);
                this.doneMeasuring = false;
            }
            this.UpdateDesiredSizes(constraint);
            this.UpdateDesiredSizes(constraint);

            // for each row, find the maximum height of all elements in the row
            double totalDesiredHeight = 0;
            int rowIndex, columnIndex;
            for (rowIndex = 0; rowIndex < this.numRows; rowIndex++)
            {
                totalDesiredHeight += this.desiredRowHeights[rowIndex];
            }

            // for each column, find the maximum width of all elements in the row
            double totalDesiredWidth = 0;
            for (columnIndex = 0; columnIndex < this.numColumns; columnIndex++)
            {
                totalDesiredWidth += this.desiredColumnWidths[columnIndex];
            }
            // tell how much room this control would like to have
            Size result = new Size(totalDesiredWidth, totalDesiredHeight);
            return result;
        }
        // call this if it is the final Measure
        public Size FinalMeasure(System.Windows.Size finalSize)
        {
            // we must invalidate the measurement to make sure that this measurement actually takes place
            this.InvalidateMeasure();
            this.Measure(finalSize);
            this.finalMeasuredWidths = this.actualColumnWidths;
            return finalSize;
        }
        
        //
        protected override System.Windows.Size MeasureOverride(System.Windows.Size constraint)
        {
            this.PreliminaryMeasure(constraint);
            // calculate acceptable for each one
            this.CalculateActualSizes(constraint);
            // Now go tell each visual how much room it will actually have
            int rowIndex, columnIndex;
            for (rowIndex = 0; rowIndex < this.numRows; rowIndex++)
            {
                for (columnIndex = 0; columnIndex < this.numColumns; columnIndex++)
                {
                    UIElement element = this.items[rowIndex, columnIndex];
                    if (element != null)
                    {
                        IResizable converted = element as IResizable;
                        Size availableSize = new Size(this.actualColumnWidths[columnIndex], this.actualRowHeights[rowIndex]);
                        if (converted != null)
                            converted.FinalMeasure(availableSize);
                        else
                            element.Measure(availableSize);
                    }
                }
            }
            this.doneMeasuring = true;
            this.desiredSize = constraint;
            return constraint;
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            this.desiredSize = this.DesiredSize;
            this.CalculateActualSizes(arrangeSize);

            // now actually arrange everything
            double y = 0;
            int rowIndex, columnIndex;
            for (rowIndex = 0; rowIndex < this.numRows; rowIndex++)
            {
                double x = 0;
                for (columnIndex = 0; columnIndex < this.numColumns; columnIndex++)
                {
                    UIElement element = this.items[rowIndex, columnIndex];
                    if (element != null)
                    {
                        element.Arrange(new Rect(x, y, this.actualColumnWidths[columnIndex], this.actualRowHeights[rowIndex]));
                    }
                    x += this.actualColumnWidths[columnIndex];
                }
                y += this.actualRowHeights[rowIndex];
            }
            this.doneMeasuring = true;
            return arrangeSize;
        }

        protected override void OnChildDesiredSizeChanged(UIElement child)
        {
            DisplayGrid parent = this.VisualParent as DisplayGrid;
            this.InvalidateMeasure();
            if (parent != null)
            {
                parent.OnChildDesiredSizeChanged(this);
            }
        }

        private void InitializeDesiredSizes(System.Windows.Size constraint)
        {
            this.desiredColumnWidths = new double[this.numColumns];
            this.desiredRowHeights = new double[this.numRows];

            // calculate how big each cell would be if each was the same size
            double columnWidth = constraint.Width;
            double rowHeight = constraint.Height;
            
            int rowIndex, columnIndex;
            for (columnIndex = 0; columnIndex < this.numColumns; columnIndex++)
            {
                this.desiredColumnWidths[columnIndex] = columnWidth;
            }
            for (rowIndex = 0; rowIndex < this.numRows; rowIndex++)
            {
                this.desiredRowHeights[rowIndex] = rowHeight;
            }

        }
        // updates desiredRowHeights and desiredColumnWidths
        private void UpdateDesiredSizes(System.Windows.Size constraint)
        {
            // clear the maximum height of each row and the maximum width of each column
            int rowIndex, columnIndex;

            double[] maxRowHeights = new double[this.desiredRowHeights.Length];
            double[] maxColumnWidths = new double[this.desiredColumnWidths.Length];

            // have each child place itself into the blocks
            double heightScale = constraint.Height / this.TotalDesiredHeight;
            double widthScale = constraint.Width / this.TotalDesiredWidth;
            for (rowIndex = 0; rowIndex < this.numRows; rowIndex++)
            {
                for (columnIndex = 0; columnIndex < this.numColumns; columnIndex++)
                {
                    UIElement element = this.items[rowIndex, columnIndex];
                    if (element != null && this.desiredColumnWidths[columnIndex] > 0 && this.desiredRowHeights[rowIndex] > 0)
                    {
                        IResizable converted = element as IResizable;
                        Size availableSize = new Size(this.desiredColumnWidths[columnIndex] * widthScale, this.desiredRowHeights[rowIndex] * heightScale);
                        Size requestedSize;
                        if (converted != null)
                        {
                            // if we get here, then the we tell the object to do a Measure but that there will be more Measure calls coming soon
                            requestedSize = converted.PreliminaryMeasure(availableSize);
                        }
                        else
                        {
                            // if we get here, then we tell the object simply to Measure because it wouldn't understand PreliminaryMeasure
                            element.Measure(availableSize);
                            requestedSize = element.DesiredSize;
                        }
                        // update our statistics of the maximum width and height of each item
                        maxColumnWidths[columnIndex] = Math.Max(maxColumnWidths[columnIndex], requestedSize.Width);
                        maxRowHeights[rowIndex] = Math.Max(maxRowHeights[rowIndex], requestedSize.Height);
                    }
                }
            }
            // keep track of how much space is unwanted
            this.desiredRowHeights = maxRowHeights;
            this.desiredColumnWidths = maxColumnWidths;
        }
        private void CalculateActualSizes(System.Windows.Size arrangeSize)
        {
            //Size desiredSize = this.DesiredSize;
            int row, column;
            // figure out how much extra height to allocate to each row
            Resizability verticalResizability = this.GetVerticalResizability();
            double desiredHeight = this.TotalDesiredHeight;
            if (double.IsPositiveInfinity(arrangeSize.Height))
                arrangeSize.Height = desiredHeight;
            double extraHeight = (arrangeSize.Height - desiredHeight);

            if (extraHeight >= 0)
            {
                // allocate the desired size to each row
                for (row = 0; row < this.numRows; row++)
                {
                    double height = this.desiredRowHeights[row] + extraHeight * this.GetVerticalResizability(row).DividedBy(verticalResizability);
                    this.actualRowHeights[row] = height;
                }
            }
            else
            {
                double heightScale = desiredHeight / arrangeSize.Height;
                // allocate the desired size to each row
                for (row = 0; row < this.numRows; row++)
                {
                    double height = this.desiredRowHeights[row] * heightScale;
                    this.actualRowHeights[row] = height;
                }
            }

            // figure out how much extra width to allocate to each row
            Resizability horizontalResizability = this.GetHorizontalResizability();
            double desiredWidth = this.TotalDesiredWidth;
            if (double.IsPositiveInfinity(arrangeSize.Width))
                arrangeSize.Width = desiredWidth;
            double extraWidth = (arrangeSize.Width - desiredWidth);
            if (extraWidth > 0)
            {
                // allocate the desired size to each row
                for (column = 0; column < this.numColumns; column++)
                {
                    double width = this.desiredColumnWidths[column] + extraWidth * this.GetHorizontalResizability(column).DividedBy(horizontalResizability);
                    this.actualColumnWidths[column] = width;
                }
            }
            else
            {
                double widthScale = desiredWidth / arrangeSize.Width;
                // allocate the desired size to each row
                for (column = 0; column < this.numColumns; column++)
                {
                    double width = this.desiredColumnWidths[column] * widthScale;
                    this.actualColumnWidths[column] = width;
                }
            }
        }

        public Resizability GetHorizontalResizability()
        {
            // add up the weight of each column
            int columnIndex;
            Resizability resizability = new Resizability(0, 0);
            for (columnIndex = 0; columnIndex < this.numColumns; columnIndex++)
            {
                resizability = resizability.Plus(this.GetHorizontalResizability(columnIndex));
            }
            return resizability;
        }
        private Resizability GetHorizontalResizability(int rowIndex, int columnIndex)
        {
            UIElement element = this.items[rowIndex, columnIndex];
            if (element == null)
                return new Resizability(0, 0);
            IResizable converted = this.itemsAsIResizables[rowIndex, columnIndex];
            if (converted != null)
                return converted.GetHorizontalResizability();
            else
                return new Resizability(1, 1);
        }
        private Resizability GetHorizontalResizability(int columnIndex)
        {
            // calculate the maximum stretchiness of all items in the column
            Resizability columnResizability = new Resizability(0, 0);
            int rowIndex;
            for (rowIndex = 0; rowIndex < this.numRows; rowIndex++)
            {
                columnResizability = columnResizability.Max(this.GetHorizontalResizability(rowIndex, columnIndex));
            }
            return columnResizability;
        }

        public Resizability GetVerticalResizability()
        {
            // add up the weight of each row
            int rowIndex;
            Resizability resizability = new Resizability(0, 0);
            for (rowIndex = 0; rowIndex < this.numRows; rowIndex++)
            {
                resizability = resizability.Plus(this.GetVerticalResizability(rowIndex));
            }
            return resizability;
        }
        private Resizability GetVerticalResizability(int rowIndex, int columnIndex)
        {
            UIElement element = this.items[rowIndex, columnIndex];
            if (element == null)
                return new Resizability(0, 0);
            IResizable converted = this.itemsAsIResizables[rowIndex, columnIndex];
            if (converted != null)
                return converted.GetVerticalResizability();
            else
                return new Resizability(1, 1);
        }
        private Resizability GetVerticalResizability(int rowIndex)
        {
            Resizability rowResizability = new Resizability(0, 0);
            int columnIndex;
            for (columnIndex = 0; columnIndex < this.numColumns; columnIndex++)
            {
                rowResizability = rowResizability.Max(this.GetVerticalResizability(rowIndex, columnIndex));
            }
            return rowResizability;
        }

        private double TotalDesiredWidth
        {
            get
            {
                int columnIndex;
                double totalDesiredWidth = 0;
                for (columnIndex = 0; columnIndex < this.numColumns; columnIndex++)
                {
                    totalDesiredWidth += this.desiredColumnWidths[columnIndex];
                }
                return totalDesiredWidth;
            }
        }
        private double TotalDesiredHeight
        {
            get
            {
                int rowIndex;
                double totalDesiredHeight = 0;
                for (rowIndex = 0; rowIndex < this.numRows; rowIndex++)
                {
                    totalDesiredHeight += this.desiredRowHeights[rowIndex];
                }
                return totalDesiredHeight;
            }
        }
        #endregion


        int numRows;
        int numColumns;
        int nextOpenRow;
        int nextOpenColumn;


        double[] desiredRowHeights;
        double[] desiredColumnWidths;

        double[] actualRowHeights;
        double[] actualColumnWidths;

        double[] finalMeasuredWidths;


        UIElement[,] items;
        IResizable[,] itemsAsIResizables;
        bool doneMeasuring;
        Size desiredSize;
    }
}
*/