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

        // assigns some datapoints to the plot
        public void SetData(List<Datapoint> points)
        {
            this.pointsToPlot = points;
            // find the min and max coordinates
            if (this.pointsToPlot.Count == 0)
            {
                return;
            }
            this.minXPresent = this.maxXPresent = this.pointsToPlot[0].Input;
            this.minYPresent = this.maxYPresent = this.pointsToPlot[0].Output;
            double x, y, weight;
            this.totalWeight = this.sumX = this.sumX2 = this.sumXY = this.sumY = this.sumY2 = 0;
            foreach (Datapoint point in this.pointsToPlot)
            {
                // update the statistics for determining the proper zoom
                x = point.Input;
                if (x < this.minXPresent)
                {
                    this.minXPresent = x;
                }
                if (x > this.maxXPresent)
                {
                    this.maxXPresent = x;
                }
                y = point.Output;
                if (y < this.minYPresent)
                {
                    this.minYPresent = y;
                }
                if (y > this.minYPresent)
                {
                    this.maxYPresent = y;
                }
                weight = point.Weight;
                // update the statistics for drawing the Least-Squares-RegressionLine
                this.totalWeight += weight;
                this.sumX += x * weight;
                this.sumX2 += x * x * weight;
                this.sumXY += x * y * weight;
                this.sumY += y * weight;
                this.sumY2 += y * y * weight;
            }
        }
        public double? MinX { get; set; }
        public double? MaxX { get; set; }
        public double? MinY { get; set; }
        public double? MaxY { get; set; }
        public bool ShowRegressionLine { get; set; }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            // figure out how much room it will have
            Size size = base.ArrangeOverride(arrangeSize);
            return size;
        }


        public Size PreliminaryMeasure(Size constraint)
        {
            return new Size();
        }
        public Size FinalMeasure(Size arrangeSize)
        {
            this.UpdatePoints(arrangeSize);
            this.Measure(arrangeSize);
            //this.drawingDimensions = arrangeSize;
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

        // updates the locations at which to draw the provided points
        private void UpdatePoints(Size displaySize)
        {
            this.Children.Clear();
            if (this.pointsToPlot.Count == 0)
                return;

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
                scaleX = displaySize.Width / (maximumX - minimumX);
            }
            if (maximumY > minimumY)
            {
                scaleY = displaySize.Height / (maximumY - minimumY);
            }

            // plot all of the provided points
            double x1, y1, x2, y2;
            x1 = (this.pointsToPlot[0].Input - minimumX) * scaleX;
            y1 = (maximumY - this.pointsToPlot[0].Output) * scaleY;
            for (i = 0; i < this.pointsToPlot.Count; i++)
            {
                x2 = (this.pointsToPlot[i].Input - minimumX) * scaleX;
                y2 = (maximumY - this.pointsToPlot[i].Output) * scaleY;

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
            if (this.ShowRegressionLine)
            {
                // compute the equation of the least-squares regression line
                Distribution xs = new Distribution(sumX, sumX2, this.totalWeight);
                Distribution ys = new Distribution(sumY, sumY2, this.totalWeight);
                double stdDevX = xs.StdDev;
                double stdDevY = ys.StdDev;
                double correlation;
                if (stdDevX != 0 && stdDevY != 0)
                {
                    double n = this.totalWeight;
                    // calculate the correlation as follows:
                    // sum((x - mean(x)) * (y - mean(y))) / stdDev(x) / stdDev(y) / n
                    // = (sum(xy - mean(x) * y - x * mean(y) + mean(x) * mean(y))) / stdDev(x) / stdDev(y) / n
                    // = (sum(xy) - 2 * mean(x) * mean(y) * n + mean(x) * mean(y) * n) / stdDev(x) / stdDev(y) / n
                    // = (sum(xy) / n - 2 * sum(x) * sum(y) + mean(x) * mean(y)) / stdDev(x) / stdDev(y)
                    correlation = (this.sumXY / n - 2 * xs.Mean * ys.Mean + xs.Mean * ys.Mean) / stdDevX / stdDevY;
                }
                else
                {
                    // the datapoints form a vertical or a horizontal line, so the correlation is 1
                    correlation = 1;
                }
                // if stdDevX == 0, we'll skip it
                if (stdDevX != 0)
                {
                    // plot the least-squares regression line
                    double slope = correlation * stdDevY / stdDevX;
                    x1 = minimumX;
                    y1 = (x1 - xs.Mean) * slope + ys.Mean;
                    x2 = maximumX;
                    y2 = (x2 - xs.Mean) * slope + ys.Mean;

                    Line newLine = new Line();
                    newLine.X1 = (x1 - minimumX) * scaleX;
                    newLine.Y1 = (maximumY - y1) * scaleY;
                    newLine.X2 = (x2 - minimumX) * scaleX;
                    newLine.Y2 = (maximumY - y2) * scaleY;
                    newLine.Stroke = Brushes.Red;
                    this.Children.Add(newLine);
                }
            }
        }
        /*
        public int NumPointsToPlot
        {
            get
            {
                return this.pointsToPlot.Count;
            }
        }
        */

        private List<Datapoint> pointsToPlot;
        //private Size drawingDimensions;
        // bounds on the data
        private double minXPresent;
        private double maxXPresent;
        private double minYPresent;
        private double maxYPresent;

        private double sumX;
        private double sumY;
        private double sumX2;
        private double sumY2;
        private double sumXY;
        private double totalWeight;
    }
}
