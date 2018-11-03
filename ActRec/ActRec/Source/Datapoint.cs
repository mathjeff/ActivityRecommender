using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActivityRecommendation
{
    public class Datapoint
    {
        public Datapoint(double inputValue, double outputValue, double weightValue)
        {
            this.input = inputValue;
            this.output = outputValue;
            this.weight = weightValue;
        }

        public double Input
        {
            get
            {
                return this.input;
            }
            set
            {
                this.input = value;
            }
        }
        public double Output
        {
            get
            {
                return this.output;
            }
            set
            {
                this.output = value;
            }
        }
        public double Weight
        {
            get
            {
                return this.weight;
            }
            set
            {
                this.weight = value;
            }
        }

        private double input;
        private double output;
        private double weight;
    }
}
