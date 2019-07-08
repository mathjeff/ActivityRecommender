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
            //this.canvas.PaintSurface += Canvas_PaintSurface;
            this.Content = this.canvas;

            this.canvas.BackgroundColor = Color.Black;
            //this.canvas.InvalidateSurface();
        }

        public void AddSeries(List<Datapoint> data, bool drawRegressionLine)
        {
            this.AddSeries(this.NewPlotRequest(data, drawRegressionLine));
        }
        public void AddSeries(PlotRequest request)
        {
            this.plotRequests.Add(request);

            foreach (Datapoint datapoint in request.Data)
            {
                if (this.minXPresent > datapoint.Input)
                    this.minXPresent = datapoint.Input;
                if (this.maxXPresent < datapoint.Input)
                    this.maxXPresent = datapoint.Input;
                if (this.minYPresent > datapoint.Output)
                    this.minYPresent = datapoint.Output;
                if (this.maxYPresent < datapoint.Output)
                    this.maxYPresent = datapoint.Output;
            }


        }
        private PlotRequest NewPlotRequest(List<Datapoint> data, bool drawRegressionLine)
        {
            SKColor drawingColor = this.GetNextColor();
            if (drawRegressionLine)
            {
                SKColor regressionColor = this.GetNextColor();
                return new PlotRequest(data, drawingColor, regressionColor);
            }
            return new PlotRequest(data, drawingColor);
        }
        private SKColor GetNextColor()
        {
            this.numColors++;
            switch (this.numColors)
            {
                case 1:
                    return SKColors.DarkGreen;
                case 2:
                    return SKColors.Red;
                case 3:
                    return SKColors.Blue;
                case 4:
                    return SKColors.Yellow;
                case 5:
                    return SKColors.DarkCyan;
            }
            throw new InvalidOperationException("Not enough colors configured to add another series to this PlotView");
        }
        public double? MinX { get; set; }
        public double? MaxX { get; set; }
        public double? MinY { get; set; }
        public double? MaxY { get; set; }
        //public int? NumXAxisTickMarks { get; set; }
        public IEnumerable<double> XAxisSubdivisions { get; set; }
        public IEnumerable<double> YAxisSubdivisions { get; set; }

        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            SizeRequest request = base.OnMeasure(widthConstraint, heightConstraint);
            return request;
        }

        private void Canvas_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            this.UpdateDrawing(e.Surface.Canvas, e.Info.Size);
        }
        
        // updates the locations at which to draw the provided points
        private void UpdateDrawing(SKCanvas canvas, SkiaSharp.SKSizeI displaySize)
        {
            if (this.plotRequests.Count < 1)
                return;

            DateTime start = DateTime.Now;

            double resolution = 1;

            // determine what domain and range we want to display
            double minimumX = this.MinX.GetValueOrDefault(this.minXPresent);
            double maximumX = this.MaxX.GetValueOrDefault(this.maxXPresent);
            double minimumY = this.MinY.GetValueOrDefault(this.minYPresent);
            double maximumY = this.MaxY.GetValueOrDefault(this.maxYPresent);

            int i;
            double scaleX = 0;
            double scaleY = 0;
            if (maximumX > minimumX)
                scaleX = displaySize.Width / (maximumX - minimumX);
            if (maximumY > minimumY)
                scaleY = displaySize.Height / (maximumY - minimumY);

            // plot all of the provided points
            double x, y, x2, y2;

            foreach (PlotRequest plotRequest in this.plotRequests)
            {
                List<Datapoint> pointList = plotRequest.Data;

                if (pointList.Count < 1)
                    continue;

                x = (pointList[0].Input - minimumX) * scaleX;
                y = (maximumY - pointList[0].Output) * scaleY;

                SKPaint brush = makeBrush(plotRequest.DrawColor);
                for (i = 0; i < pointList.Count; i++)
                {
                    x2 = (pointList[i].Input - minimumX) * scaleX;
                    y2 = (maximumY - pointList[i].Output) * scaleY;

                    if (Math.Abs(x2 - x) < resolution && Math.Abs(y2 - y) < resolution)
                        continue; // skip any lines that the user won't be able to see

                    canvas.DrawLine((float)x, (float)y, (float)x2, (float)y2, brush);
                    x = x2;
                    y = y2;
                }

                if (plotRequest.Draw_RegressionLine)
                {
                    Correlator correlator = plotRequest.Correlator;
                    if (correlator.StdDevX != 0)
                    {
                        // if stdDevX == 0, we'll skip it
                        if (correlator.StdDevX != 0)
                        {
                            SKPaint correlatorBrush = makeBrush(plotRequest.RegressionLine_Color);
                            // compute the equation of the least-squares regression line
                            double correlation = correlator.Correlation;
                            // plot the least-squares regression line
                            double slope = correlator.Slope;
                            x = minimumX;
                            y = (x - correlator.MeanX) * slope + correlator.MeanY;
                            x2 = maximumX;
                            y2 = (x2 - correlator.MeanX) * slope + correlator.MeanY;

                            double renderX1 = (x - minimumX) * scaleX;
                            double renderY1 = (maximumY - y) * scaleY;
                            double renderX2 = (x2 - minimumX) * scaleX;
                            double renderY2 = (maximumY - y2) * scaleY;

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


                            canvas.DrawLine((float)renderX1, (float)renderY1, (float)renderX2, (float)renderY2, correlatorBrush);
                        }
                    }
                }
            }
            // now draw some tick marks
            if (this.XAxisSubdivisions != null)
            {
                y = displaySize.Height - 1;
                y2 = displaySize.Height * 19 / 20;
                foreach (double tickX in this.XAxisSubdivisions)
                {
                    canvas.DrawLine((float)((tickX - minimumX) * scaleX), (float)y, (float)((tickX - minimumX) * scaleX), (float)y2, cyanBrush);
                }
            }
            if (this.YAxisSubdivisions != null)
            {
                x = 0;
                x2 = displaySize.Width / 20;
                foreach (double tickY in this.YAxisSubdivisions)
                {
                    canvas.DrawLine((float)x, (float)((maximumY - tickY) * scaleY), (float)x2, (float)((maximumY - tickY) * scaleY), cyanBrush);
                }
            }
            // draw some axes
            canvas.DrawLine(0, 0, 0, displaySize.Height, cyanBrush);
            canvas.DrawLine(0, displaySize.Height - 1, displaySize.Width, displaySize.Height - 1, cyanBrush);

            DateTime end = DateTime.Now;
            System.Diagnostics.Debug.WriteLine("spent " + end.Subtract(start) + " in PlotView.UpdatePoints");
        }


        private List<PlotRequest> plotRequests = new List<PlotRequest>();

        private SKCanvasView canvas;
        // bounds on the data
        private double minXPresent = double.PositiveInfinity;
        private double maxXPresent = double.NegativeInfinity;
        private double minYPresent = double.PositiveInfinity;
        private double maxYPresent = double.NegativeInfinity;

        private int numColors = 0;

    }

    public class PlotRequest
    {
        public PlotRequest(List<Datapoint> data, SKColor drawColor)
        {
            this.initialize(data, drawColor, SKColors.Empty);
        }

        public PlotRequest(List<Datapoint> data, SKColor drawColor, SKColor regressionColor)
        {
            this.initialize(data, drawColor, regressionColor);
        }
        private void initialize(List<Datapoint> data, SKColor drawColor, SKColor regressionLine_color)
        {
            this.Data = data;
            this.DrawColor = drawColor;
            this.RegressionLine_Color = regressionLine_color;
            if (this.Draw_RegressionLine)
            {
                foreach (Datapoint datapoint in data)
                {
                    this.Correlator.Add(datapoint.Input, datapoint.Output, datapoint.Weight);
                }
            }
        }
        public bool Draw_RegressionLine
        {
            get
            {
                return this.RegressionLine_Color != SKColors.Empty;
            }
        }
        public List<Datapoint> Data;
        public SKColor DrawColor;
        public SKColor RegressionLine_Color;
        public Correlator Correlator = new Correlator();
    }
}
