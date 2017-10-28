/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;

namespace ActivityRecommendation
{
    interface IResizable
    {
        Resizability GetHorizontalResizability();
        Resizability GetVerticalResizability();
        Size PreliminaryMeasure(Size constraint);    // This replaces all Measure() calls except the final one
        Size FinalMeasure(Size finalSize);         // This replaces the final Measure() call
    }

    // a Resizability describes a bunch of UIElements
    // It tells how well the UIElements can actually use any blank space, and also how many pieces the blank space will be split into
    class Resizability
    {
        public Resizability(int priority, double weight)
        {
            this.Priority = priority;
            this.Weight = weight;
        }

        // any Resizability of a higher priority will get all of the extra space
        public int Priority { get; set; }
        // any Resizability's of the same priority will be weighted by their weights
        public double Weight { get; set; }

        // adds two Resizability objects
        public Resizability Plus(Resizability other)
        {
            // if the priorities are the same, then add the weights
            if (this.Priority == other.Priority)
                return new Resizability(this.Priority, this.Weight + other.Weight);
            // if the priorities are different, then simply use one of higher priority
            return this.Max(other);
        }
        // returns the larger of two Resizability objects
        public Resizability Max(Resizability other)
        {
            if (this.Priority > other.Priority)
                return this;
            if (this.Priority < other.Priority)
                return other;
            if (this.Weight > other.Weight)
                return this;
            return other;
        }
        // division
        public double DividedBy(Resizability other)
        {
            if (this.Priority < other.Priority)
                return 0;
            if (other.Weight != 0)
                return this.Weight / other.Weight;
            return 0;
        }
    }

}

*/