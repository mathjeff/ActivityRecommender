using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdaptiveLinearInterpolation
{
    // represents an interval
    public class FloatRange
    {
        public FloatRange(double lowCoordinate, bool lowInclusive, double highCoordinate, bool highInclusive)
        {
            this.LowCoordinate = lowCoordinate;
            this.LowInclusive = lowInclusive;
            this.HighCoordinate = highCoordinate;
            this.HighInclusive = highInclusive;
        }
        public FloatRange(FloatRange source)
        {
            this.LowCoordinate = source.LowCoordinate;
            this.LowInclusive = source.LowInclusive;
            this.HighCoordinate = source.HighCoordinate;
            this.HighInclusive = source.HighInclusive;
        }
        public double LowCoordinate { get; set; }
        public bool LowInclusive { get; set; }
        public double HighCoordinate { get; set; }
        public bool HighInclusive { get; set; }
        public bool Contains(FloatRange other)
        {
            if (other.LowInclusive && !this.LowInclusive)
            {
                if (this.LowCoordinate >= other.LowCoordinate)
                    return false;
            }
            else
            {
                if (this.LowCoordinate > other.LowCoordinate)
                    return false;
            }
            if (other.HighInclusive && !this.HighInclusive)
            {
                if (this.HighCoordinate <= other.HighCoordinate)
                    return false;
            }
            else
            {
                if (this.HighCoordinate < other.HighCoordinate)
                    return false;
            }
            return true;
        }
        public bool Contains(double coordinate)
        {
            if (this.LowInclusive)
            {
                if (this.LowCoordinate > coordinate)
                    return false;
            }
            else
            {
                if (this.LowCoordinate >= coordinate)
                    return false;
            }
            if (this.HighInclusive)
            {
                if (this.HighCoordinate < coordinate)
                    return false;
            }
            else
            {
                if (this.HighCoordinate <= coordinate)
                    return false;
            }
            return true;
        }
        public bool Intersects(FloatRange other)
        {
            if (this.LowInclusive || other.HighInclusive)
            {
                if (this.LowCoordinate > other.HighCoordinate)
                    return false;
            }
            else
            {
                if (this.LowCoordinate >= other.HighCoordinate)
                    return false;
            }
            if (this.HighInclusive || other.LowInclusive)
            {
                if (this.HighCoordinate < other.LowCoordinate)
                    return false;
            }
            else
            {
                if (this.HighCoordinate <= other.LowCoordinate)
                    return false;
            }
            return true;
        }
        public void ExpandToInclude(double coordinate)
        {
            bool containsLow = true;
            if (this.LowInclusive)
            {
                if (this.LowCoordinate > coordinate)
                    containsLow = false;
            }
            else
            {
                if (this.LowCoordinate >= coordinate)
                    containsLow = false;
            }
            if (!containsLow)
            {
                this.LowCoordinate = coordinate;
                this.LowInclusive = true;
            }

            bool containsHigh = true;
            if (this.HighInclusive)
            {
                if (this.HighCoordinate < coordinate)
                    containsHigh = false;
            }
            else
            {
                if (this.HighCoordinate <= coordinate)
                    containsHigh = false;
            }
            if (!containsHigh)
            {
                this.HighCoordinate = coordinate;
                this.HighInclusive = true;
            }
        }
        public double Width
        {
            get
            {
                return this.HighCoordinate - this.LowCoordinate;
            }
        }
        public double Middle
        {
            get
            {
                return (this.HighCoordinate + this.LowCoordinate) / 2;
            }
        }
    }
}
