using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActivityRecommendation
{
    class BinomialDistribution
    {
        public BinomialDistribution(double numZeros, double numOnes)
        {
            this.NumZeros = numZeros;
            this.NumOnes = numOnes;
        }
        public BinomialDistribution(BinomialDistribution original)
        {
            this.CopyFrom(original);
        }
        public void CopyFrom(BinomialDistribution original)
        {
            this.NumOnes = original.NumOnes;
            this.NumZeros = original.NumZeros;
        }
        public double NumZeros { get; set; }
        public double NumOnes { get; set; }
        public double NumItems
        {
            get
            {
                return this.NumZeros + this.NumOnes;
            }
        }
        public double Mean
        {
            get
            {
                return (this.NumOnes + 1) / (this.NumZeros + this.NumOnes + 2);
            }
        }
    }
}
