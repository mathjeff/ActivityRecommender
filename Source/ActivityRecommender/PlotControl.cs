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
    class PlotControl : Canvas, IResizable
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
            // find the min and max coordinates
            if (this.pointsToPlot.Count == 0)
            {
                return;
            }
            this.minXPresent = this.maxXPresent = this.pointsToPlot[0].X;
            this.minYPresent = this.maxYPresent = this.pointsToPlot[0].Y;
            double x, y;
            foreach (Point point in this.pointsToPlot)
            {
                x = point.X;
                if (x < this.minXPresent)
                {
                    this.minXPresent = x;
                }
                if (x > this.maxXPresent)
                {
                    this.maxXPresent = x;
                }
                y = point.Y;
                if (y < this.minYPresent)
                {
                    this.minYPresent = y;
                }
                if (y > this.minYPresent)
                {
                    this.maxYPresent = y;
                }
            }
        }
        public double? MinX { get; set; }
        public double? MaxX { get; set; }
        public double? MinY { get; set; }
        public double? MaxY { get; set; }

        public Size PreliminaryMeasure(Size constraint)
        {
            return new Size();
        }
        public Size FinalMeasure(Size arrangeSize)
        {
            this.Measure(arrangeSize);
            return arrangeSize;
        }
        public Resizability GetHorizontalResizability()
        {
            return new Resizability(2, 1);
        }
        public Resizability GetVerticalResizability()
        {
            return new Resizability(2, 1);
        }

        private void UpdatePoints()
        {
            this.Children.Clear();

            // determine what domain and range we want to display
            double minimumX = this.MinX.GetValueOrDefault(this.minXPresent);
            double maximumX = this.MaxX.GetValueOrDefault(this.maxXPresent);
            double minimumY = this.MinY.GetValueOrDefault(this.minYPresent);
            double maximumY = this.MaxY.GetValueOrDefault(this.maxYPresent);

            int i;
            double scaleX = 0;
            double scaleY = 0;
            if (maximumX > minimumX)
            {
                scaleX = this.drawingDimensions.Width / (maximumX - minimumX);
            }
            if (maximumY > minimumY)
            {
                scaleY = this.drawingDimensions.Height / (maximumY - minimumY);
            }
            double x1, y1, x2, y2;
            x1 = (this.pointsToPlot[0].X - minimumX) * scaleX;
            y1 = (maximumY - this.pointsToPlot[0].Y) * scaleY;
            for (i = 0; i < this.pointsToPlot.Count; i++)
            {
                x2 = (this.pointsToPlot[i].X - minimumX) * scaleX;
                y2 = (maximumY - this.pointsToPlot[i].Y) * scaleY;

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
        }

        private List<Point> pointsToPlot;
        private Size drawingDimensions;
        // bounds on the data
        private double minXPresent;
        private double maxXPresent;
        private double minYPresent;
        private double maxYPresent;
    }
}
