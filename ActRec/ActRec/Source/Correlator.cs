using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityRecommendation
{
    public class Correlator
    {
        public Correlator()
        {
        }

        public void Add(double x, double y)
        {
            this.Add(x, y, 1);
        }

        public void Add(double x, double y, double weight)
        {
            this.xs.Add(x, weight);
            this.ys.Add(y, weight);
            this.count += weight;
            this.sumXY += x * y * weight;
        }

        public double Correlation
        {
            get
            {
                // calculate the correlation as follows:
                // sum((x - mean(x)) * (y - mean(y))) / stdDev(x) / stdDev(y) / n
                // = (sum(xy - mean(x) * y - x * mean(y) + mean(x) * mean(y))) / stdDev(x) / stdDev(y) / n
                // = (sum(xy) - 2 * mean(x) * mean(y) * n + mean(x) * mean(y) * n) / stdDev(x) / stdDev(y) / n
                // = (sum(xy) / n - 2 * mean(x) * mean(y) + mean(x) * mean(y)) / stdDev(x) / stdDev(y)
                // = (sum(xy) / n - mean(x) * mean(y) ) / stdDev(x) / stdDev(y)
                return (this.sumXY / this.count - this.xs.Mean * this.ys.Mean) / this.xs.StdDev / this.ys.StdDev;
            }
        }

        public double Slope
        {
            get
            {
                return this.Correlation * this.ys.StdDev / this.xs.StdDev;
            }
        }

        public double MeanX
        {
            get
            {
                return this.xs.Mean;
            }
        }
        public double StdDevX
        {
            get
            {
                return this.xs.StdDev;
            }
        }

        public double MeanY
        {
            get
            {
                return this.ys.Mean;
            }
        }
        public double StdDevY
        {
            get
            {
                return this.ys.StdDev;
            }
        }

        public double GetYForX(double x)
        {
            double deltaX = x - this.xs.Mean;
            double deltaY = deltaX * this.Slope;
            double y = this.ys.Mean + deltaY;
            return y;
        }

        public Correlator Plus(Correlator other)
        {
            Correlator result = new Correlator();
            result.count = this.count + other.count;
            result.sumXY = this.sumXY + other.sumXY;
            result.xs = this.xs.Plus(other.xs);
            result.ys = this.ys.Plus(other.ys);
            return result;
        }

        public Correlator CopyAndShiftUp(double y)
        {
            // xs don't change, count doesn't change, correlation doesn't change, but ys each shift up and sumXY changes            
            Distribution newYs = this.ys.CopyAndShiftBy(y);
            // old correlation = (sumXY / count - xs.Mean * ys.Mean) / xs.StdDev / ys.StdDev
            // new correlation = (newSumXY / count - xs.Mean * newYs.Mean) / xs.StdDev / ys.StdDev
            // (newSumXY / count - xs.Mean * newYs.Mean) / xs.StdDev / ys.StdDev = (sumXY / count - xs.Mean * ys.Mean) / xs.StdDev / ys.StdDev
            // (newSumXY / count - xs.Mean * newYs.Mean) = (sumXY / count - xs.Mean * ys.Mean)
            // newSumXY / count = sumXY / count - xs.Mean * ys.Mean + xs.Mean * newYs.Mean
            // newSumXY = (sumXY / count - xs.Mean * ys.Mean + xs.Mean * newYs.Mean) * count
            // newSumXY = (sumXY / count + xs.Mean * (newYs.Mean - ys.mean)) * count
            // newSumXY = sumXY + xs.Mean * (newYs.Mean - ys.mean) * count
            // newSumXY = sumXY + xs.Mean * shiftY * count
            double shiftedSumXY = this.sumXY + this.xs.Mean * y * this.count;

            Correlator shifted = new Correlator();
            shifted.count = this.count;
            shifted.sumXY = shiftedSumXY;
            shifted.xs = this.xs.Clone();
            shifted.ys = newYs;
            return shifted;
        }
        public Correlator CopyAndShiftRight(double x)
        {
            Distribution newXs = this.xs.CopyAndShiftBy(x);
            double shiftedSumXY = this.sumXY + x * this.ys.Mean * this.count;

            Correlator shifted = new Correlator();
            shifted.count = this.count;
            shifted.sumXY = shiftedSumXY;
            shifted.xs = newXs;
            shifted.ys = this.ys.Clone();
            return shifted;

        }

        public Correlator Clone()
        {
            return this.CopyAndShiftUp(0);
        }

        double count;
        double sumXY;
        Distribution xs = new Distribution();
        Distribution ys = new Distribution();
    }
}
