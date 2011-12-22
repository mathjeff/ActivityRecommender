using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;

// The PlotControl shows a visual representation of some (x,y) points
namespace ActivityRecommendation
{
    class PlotControl : Canvas
    {
        public PlotControl()
        {
            //this.childCanvas = new Canvas();
            //this.AddVisualChild(childCanvas);
        }
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            // figure out how much room it will have
            Size size = base.ArrangeOverride(arrangeSize);
            if (!size.Equals(this.drawingDimensions))
            {
                this.drawingDimensions = size;
                this.UpdatePoints();
            }
            return size;
        }

        public void SetData(List<Point> points)
        {
            this.pointsToPlot = points;
        }
        public double? MinX { get; set; }
        public double? MaxX { get; set; }

        private void UpdatePoints()
        {
            this.Children.Clear();
            // make sure there's at least one data point
            double minX, maxX, minY, maxY;
            if (this.pointsToPlot.Count == 0)
            {
                return;
            }
            // find the min and max coordinates
            minX = maxX = this.pointsToPlot[0].X;
            minY = maxY = this.pointsToPlot[0].Y;
            double x, y;
            foreach (Point point in this.pointsToPlot)
            {
                x = point.X;
                if (x < minX)
                {
                    minX = x;
                }
                if (x > maxX)
                {
                    maxX = x;
                }
                y = point.Y;
                if (y < minY)
                {
                    minY = y;
                }
                if (y > maxY)
                {
                    maxY = y;
                }
            }

            // fill in any required values
            if (this.MinX != null)
            {
                minX = (double)this.MinX;
            }
            if (this.MaxX != null)
            {
                maxX = (double)this.MaxX;
            }

            // remove the previous canvas
            //this.RemoveVisualChild(this.childCanvas);
            // make a new canvas to draw on
            //this.childCanvas = new Canvas();
            int i;
            double scaleX = 0;
            double scaleY = 0;
            if (maxX > minX)
            {
                scaleX = this.drawingDimensions.Width / (maxX - minX);
            }
            if (maxY > minY)
            {
                scaleY = this.drawingDimensions.Height / (maxY - minY);
            }
            double x1, y1, x2, y2;
            x1 = (this.pointsToPlot[0].X - minX) * scaleX;
            y1 = (maxY - this.pointsToPlot[0].Y) * scaleY;
            for (i = 0; i < this.pointsToPlot.Count; i++)
            {
                x2 = (this.pointsToPlot[i].X - minX) * scaleX;
                y2 = (maxY - this.pointsToPlot[i].Y) * scaleY;

                Line newLine = new Line();
                newLine.X1 = x1;
                newLine.Y1 = y1;
                newLine.X2 = x2;
                newLine.Y2 = y2;
                newLine.Stroke = Brushes.Black;
                this.Children.Add(newLine);

                x1 = x2;
                y1 = y2;

            }
            /*
            Line testLine = new Line();
            testLine.X1 = 100;
            testLine.Y1 = 200;
            testLine.X2 = 300;
            testLine.Y2 = 400;
            testLine.Stroke = Brushes.Black;
            testLine.StrokeThickness = 10;
            //childCanvas.Children.Add(testLine);

            this.Children.Add(testLine);
            */
            //this.AddVisualChild(testLine);

            //this.AddVisualChild(childCanvas);
            //this.AddVisualChild();

        }
        //Canvas childCanvas;

        private List<Point> pointsToPlot;
        private Size drawingDimensions;
    }
}
