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
            this.xs = new Distribution();
            this.ys = new Distribution();
        }

        public Correlator(double count, double sumXY, Distribution xs, Distribution ys)
        {
            this.count = count;
            this.sumXY = sumXY;
            this.xs = xs;
            this.ys = ys;
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
        public void Add(Datapoint datapoint)
        {
            this.Add(datapoint.Input, datapoint.Output, datapoint.Weight);
        }

        public bool HasCorrelation
        {
            get
            {
                if (this.count <= 0)
                    return false;
                if (this.xs.StdDev <= 0)
                    return false;
                if (this.ys.StdDev <= 0)
                    return false;
                return true;
            }
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
        public double Weight
        {
            get
            {
                return this.xs.Weight;
            }
        }

        public Correlator Plus(Correlator other)
        {
            return new Correlator(this.count + other.count, this.sumXY + other.sumXY, this.xs.Plus(other.xs), this.ys.Plus(other.ys));
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

            return new Correlator(this.count, shiftedSumXY, this.xs.Clone(), newYs);
        }
        public Correlator CopyAndShiftRight(double x)
        {
            Distribution newXs = this.xs.CopyAndShiftBy(x);
            double shiftedSumXY = this.sumXY + x * this.ys.Mean * this.count;

            return new Correlator(this.count, shiftedSumXY, newXs, this.ys.Clone());
        }

        public Correlator CopyAndShiftRightAndUp(double x, double y)
        {
            // shift x
            Distribution newXs = this.xs.CopyAndShiftBy(x);
            double shiftedSumXY = this.sumXY + x * this.ys.Mean * this.count;

            // shift y
            Distribution newYs = this.ys.CopyAndShiftBy(y);
            shiftedSumXY = shiftedSumXY + newXs.Mean * y * this.count;

            return new Correlator(this.count, shiftedSumXY, newXs, newYs);
        }

        public Correlator Clone()
        {
            return this.CopyAndShiftUp(0);
        }

        double count;
        double sumXY;
        Distribution xs;
        Distribution ys;
    }
}
