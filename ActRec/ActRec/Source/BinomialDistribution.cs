using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActivityRecommendation
{
    public class BinomialDistribution
    {
        public BinomialDistribution(double numZeros, double numOnes)
        {
            this.NumZeros = numZeros;
            this.NumOnes = numOnes;
            this.validate();
        }
        public BinomialDistribution(BinomialDistribution original)
        {
            this.CopyFrom(original);
        }

        public BinomialDistribution(Distribution distribution)
        {
            this.NumOnes = distribution.Weight * distribution.Mean;
            this.NumZeros = distribution.Weight - this.NumOnes;
            this.validate();
        }
        public void CopyFrom(BinomialDistribution original)
        {
            this.NumOnes = original.NumOnes;
            this.NumZeros = original.NumZeros;
        }
        private void validate()
        {
            if (this.NumZeros < 0)
                throw new ArgumentException("numZeros (" + this.NumZeros + ") in a BinomialDistribution cannot be negative");
            if (this.NumOnes < 0)
                throw new ArgumentException("numOnes (" + this.NumOnes + ") in a BinomialDistribution cannot be negative");
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
