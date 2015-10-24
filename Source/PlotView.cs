#if true // This will be really cool once it gets updated to work with VisiPlacement
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;
using VisiPlacement;

// The PlotControl shows a visual representation of some (x,y) points
namespace ActivityRecommendation
{
    class PlotView : Panel
    {
        public PlotView()
        {
            this.canvas = new Canvas();
            this.Children.Add(this.canvas);
            
            //this.AddVisualChild(childCanvas);
            this.Connected = true;
            this.canvas.Background = new SolidColorBrush(Colors.LightGray);
        }

        // assigns some datapoints to the plot
        public void SetData(List<List<Datapoint>> points)
        {
            //this.SetData(new List
            this.pointsToPlot = points;
            // find the min and max coordinates
            if (this.pointsToPlot.Count == 0)
            {
                return;
            }
            foreach (List<Datapoint> pointList in this.pointsToPlot)
            {
                if (pointList.Count > 0)
                {
                    this.minXPresent = this.maxXPresent = pointList[0].Input;
                    this.minYPresent = this.maxYPresent = pointList[0].Output;
                }
            }
            double x, y, weight;
            this.totalWeight = this.sumX = this.sumX2 = this.sumXY = this.sumY = this.sumY2 = 0;
            int sequenceNumber;
            for (sequenceNumber = 0; sequenceNumber < this.pointsToPlot.Count; sequenceNumber++)
            {
                List<Datapoint> pointList = this.pointsToPlot[sequenceNumber];
                foreach (Datapoint point in pointList)
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

                    if (sequenceNumber == 0)
                    {
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
            }
        }
        // assigns some datapoints to the plot
        public void SetData(List<Datapoint> points)
        {
            List<List<Datapoint>> sets = new List<List<Datapoint>>();
            sets.Add(points);
            this.SetData(sets);
        }
        public double? MinX { get; set; }
        public double? MaxX { get; set; }
        public double? MinY { get; set; }
        public double? MaxY { get; set; }
        //public int? NumXAxisTickMarks { get; set; }
        public IEnumerable<double> XAxisSubdivisions { get; set; }
        public bool ShowRegressionLine { get; set; }
        public bool Connected { get; set; }

#if true
        protected override Size MeasureOverride(Size availableSize)
        {
            this.UpdatePoints(availableSize);
            return base.MeasureOverride(availableSize);
        }
#endif
                
        // updates the locations at which to draw the provided points
        private void UpdatePoints(Size displaySize)
        {
            this.canvas.Children.Clear();
            if (this.pointsToPlot.Count == 0)
                return;

            // determine what domain and range we want to display
            double minimumX = this.MinX.GetValueOrDefault(this.minXPresent);
            double maximumX = this.MaxX.GetValueOrDefault(this.maxXPresent);
            double minimumY = this.MinY.GetValueOrDefault(this.minYPresent);
            double maximumY = this.MaxY.GetValueOrDefault(this.maxYPresent);

            int i, j;
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
            for (i = 0; i < this.pointsToPlot.Count; i++)
            {
                List<Datapoint> pointList = this.pointsToPlot[i];

                if (pointList.Count < 1)
                    continue;

                x1 = (pointList[0].Input - minimumX) * scaleX;
                y1 = (maximumY - pointList[0].Output) * scaleY;

                Brush brush;
                switch (i)
                {
                    case 0:
                        brush = new SolidColorBrush(Colors.Green);
                        break;
                    default:
                        brush = new SolidColorBrush(Colors.Blue);
                        break;
                }

                for (j = 0; j < pointList.Count; j++)
                {
                    x2 = (pointList[j].Input - minimumX) * scaleX;
                    y2 = (maximumY - pointList[j].Output) * scaleY;

                    Line newLine = new Line();
                    if (this.Connected)
                    {
                        newLine.X1 = x1;
                        newLine.Y1 = y1;
                        newLine.X2 = x2;
                        newLine.Y2 = y2;
                    }
                    else
                    {
                        newLine.X1 = x1;
                        newLine.Y1 = y1;
                        newLine.X2 = x1 + 1;
                        newLine.Y2 = y1 + 1;
                        //newLine.Width = 1;
                        //newLine.StrokeStartLineCap = PenLineCap.Round;
                    }
                    newLine.Stroke = brush;
                    this.canvas.Children.Add(newLine);

                    x1 = x2;
                    y1 = y2;
                }

            
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
                    newLine.Stroke = new SolidColorBrush(Colors.Red);
                    this.canvas.Children.Add(newLine);
                }
            }
            // now draw some tick marks
            if (this.XAxisSubdivisions != null)
            {
                y1 = displaySize.Height - 1;
                y2 = displaySize.Height * 19 / 20;
                foreach (double x in this.XAxisSubdivisions)
                {
                    x1 = x;
                    x2 = x1;

                    Line newLine = new Line();
                    newLine.X1 = (x1 - minimumX) * scaleX;
                    newLine.Y1 = y1;
                    newLine.X2 = (x2 - minimumX) * scaleX;
                    newLine.Y2 = y2;
                    newLine.Stroke = new SolidColorBrush(Colors.Black);
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

        private List<List<Datapoint>> pointsToPlot;

        private Canvas canvas;
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

#endif