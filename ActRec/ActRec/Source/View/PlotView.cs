using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using VisiPlacement;
using Xamarin.Forms;

// The PlotControl shows a visual representation of some (x,y) points
namespace ActivityRecommendation
{
    class PlotView : ContainerView
    {
        static SKPaint makeBrush(byte red, byte green, byte blue)
        {
            SKColor color = new SKColor().WithRed(red).WithGreen(green).WithBlue(blue).WithAlpha(255);
            return makeBrush(color);
        }
        static SKPaint makeBrush(SKColor color)
        {
            //return null;
            SKPaint paint = new SKPaint();
            paint.StrokeWidth = 1;
            paint.StrokeCap = SKStrokeCap.Round;
            paint.Color = color;
            return paint;
        }
        static SKPaint redBrush = makeBrush(SKColors.Red);
        static SKPaint greenBrush = makeBrush(SKColors.DarkGreen);
        static SKPaint blueBrush = makeBrush(SKColors.Blue);
        static SKPaint cyanBrush = makeBrush(SKColors.DarkCyan);


        public PlotView()
        {
            this.canvas = new SKCanvasView();
            this.canvas.PaintSurface += Canvas_PaintSurface;
            this.Content = this.canvas;

            //this.AddVisualChild(childCanvas);
            this.Connected = true;
            this.canvas.BackgroundColor = Color.Black;
            this.canvas.InvalidateSurface();

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
                        this.correlator.Add(x, y, weight);
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

        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            //DateTime start = DateTime.Now;
            //Size availableSize = new Size(widthConstraint, heightConstraint);
            //this.UpdatePoints(this.canvas.PaintSurface, availableSize);
            //DateTime end = DateTime.Now;
            //System.Diagnostics.Debug.WriteLine("spent " + end.Subtract(start) + " in PlotView.OnMeasure");
            SizeRequest request = base.OnMeasure(widthConstraint, heightConstraint);
            return request;
        }

        private void Canvas_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            this.UpdatePoints(e.Surface.Canvas, e.Info.Size);
        }



        // updates the locations at which to draw the provided points
        private void UpdatePoints(SKCanvas canvas, SkiaSharp.SKSizeI displaySize)
        {
            //this.canvas.Children.Clear();
            if (this.pointsToPlot.Count == 0)
                return;

            DateTime start = DateTime.Now;

            double resolution = 1;

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
            double x, y, nextX, nextY;
            for (i = 0; i < this.pointsToPlot.Count; i++)
            {
                List<Datapoint> pointList = this.pointsToPlot[i];

                if (pointList.Count < 1)
                    continue;

                x = (pointList[0].Input - minimumX) * scaleX;
                y = (maximumY - pointList[0].Output) * scaleY;

                // TODO move the choice of color into the caller
                SKPaint brush;
                switch (i)
                {
                    case 0:
                        brush = greenBrush;
                        break;
                    default:
                        brush = blueBrush;
                        break;
                }

                for (j = 0; j < pointList.Count; j++)
                {
                    nextX = (pointList[j].Input - minimumX) * scaleX;
                    nextY = (maximumY - pointList[j].Output) * scaleY;

                    if (Math.Abs(nextX - x) < resolution && Math.Abs(nextY - y) < resolution)
                        continue; // skip any lines that the user won't be able to see

                    double x2, y2;
                    if (this.Connected)
                    {
                        x2 = nextX;
                        y2 = nextY;
                    }
                    else
                    {
                        x2 = x + resolution;
                        y2 = y + resolution;
                    }
                    canvas.DrawLine((float)x, (float)y, (float)x2, (float)y2, brush);

                    x = nextX;
                    y = nextY;
                }
            }
            // TODO move into the caller more of the configuration about which regression lines to draw
            if (this.ShowRegressionLine)
            {
                // if stdDevX == 0, we'll skip it
                if (this.correlator.StdDevX != 0)
                {
                    // compute the equation of the least-squares regression line
                    double correlation = this.correlator.Correlation;
                    // plot the least-squares regression line
                    double slope = this.correlator.Slope;
                    x = minimumX;
                    y = (x - this.correlator.MeanX) * slope + this.correlator.MeanY;
                    nextX = maximumX;
                    nextY = (nextX - this.correlator.MeanX) * slope + this.correlator.MeanY;

                    double renderX1 = (x - minimumX) * scaleX;
                    double renderY1 = (maximumY - y) * scaleY;
                    double renderX2 = (nextX - minimumX) * scaleX;
                    double renderY2 = (maximumY - nextY) * scaleY;

                    double rescaleRatio;
                    // cut off the right side of the line if it goes out of bounds
                    rescaleRatio = 1;
                    if (renderY2 > displaySize.Height)
                    {
                        rescaleRatio = (displaySize.Height - renderY1) / (renderY2 - renderY1);
                    }
                    else
                    {
                        if (renderY2 < 0)
                            rescaleRatio = (renderY1) / (renderY1 - renderY2);
                    }
                    renderY2 = renderY1 + (renderY2 - renderY1) * rescaleRatio;
                    renderX2 = renderX1 + (renderX2 - renderX1) * rescaleRatio;
                    // cut off the left side of the line if it goes out of bounds
                    rescaleRatio = 1;
                    if (renderY1 > displaySize.Height)
                    {
                        rescaleRatio = (displaySize.Height - renderY2) / (renderY1 - renderY2);
                    }
                    else
                    {
                        if (renderY1 < 0)
                            rescaleRatio = (renderY2) / (renderY2 - renderY1);
                    }
                    renderY1 = renderY2 - (renderY2 - renderY1) * rescaleRatio;
                    renderX1 = renderX2 - (renderX2 - renderX1) * rescaleRatio;


                    canvas.DrawLine((float)renderX1, (float)renderY1, (float)renderX2, (float)renderY2, redBrush);
                }
            }
            // now draw some tick marks
            if (this.XAxisSubdivisions != null)
            {
                y = displaySize.Height - 1;
                nextY = displaySize.Height * 19 / 20;
                foreach (double tickX in this.XAxisSubdivisions)
                {

                    canvas.DrawLine((float)((tickX - minimumX) * scaleX), (float)y,(float)((tickX - minimumX) * scaleX), (float)nextY, cyanBrush);
                }
            }

            // X
            //canvas.DrawLine(-100, -100, 100, 100, redBrush);
            //canvas.DrawLine(-100, 100, 100, -100, redBrush);

            // box
            //canvas.DrawLine(-100, -100, -100, 100, redBrush);
            //canvas.DrawLine(-100, 100, 100, 100, redBrush);
            //canvas.DrawLine(100, 100, 100, -100, redBrush);
            //canvas.DrawLine(100, -100, -100, -100, redBrush);

            //canvas.Flush();

            DateTime end = DateTime.Now;
            System.Diagnostics.Debug.WriteLine("spent " + end.Subtract(start) + " in PlotView.UpdatPoints");

        }


        private List<List<Datapoint>> pointsToPlot;

        private SKCanvasView canvas;
        //private Size drawingDimensions;
        // bounds on the data
        private double minXPresent;
        private double maxXPresent;
        private double minYPresent;
        private double maxYPresent;

        private Correlator correlator = new Correlator();
    }
}
