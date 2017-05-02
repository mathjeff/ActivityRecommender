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
                // = (sum(xy) / n - 2 * sum(x) * sum(y) + mean(x) * mean(y)) / stdDev(x) / stdDev(y)
                return (this.sumXY / this.count - 2 * this.xs.Mean * this.ys.Mean + this.xs.Mean * this.ys.Mean) / this.xs.StdDev / this.ys.StdDev;
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

        double count;
        double sumXY;
        Distribution xs = new Distribution();
        Distribution ys = new Distribution();
    }
}
