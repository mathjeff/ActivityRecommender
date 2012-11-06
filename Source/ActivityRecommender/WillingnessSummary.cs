using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// A WillingnessSummary describes how often, in the past, the user has:
// skipped a particular activity
// done the particular activity after being prompted
// done the particular activity without being prompted
namespace ActivityRecommendation
{
    public class WillingnessSummary
    {
        public WillingnessSummary()
        {
        }
        public WillingnessSummary(double unpromptedParticipationCount, double promptedParticipationCount, double skipCount)
        {
            this.numUnpromptedParticipations = unpromptedParticipationCount;
            this.numPromptedParticipations = promptedParticipationCount;
            this.numSkips = skipCount;
        }
        public WillingnessSummary Plus(WillingnessSummary other)
        {
            WillingnessSummary sum = new WillingnessSummary();
            sum.numUnpromptedParticipations = this.numUnpromptedParticipations + other.numUnpromptedParticipations;
            sum.numPromptedParticipations = this.numPromptedParticipations + other.numPromptedParticipations;
            sum.numSkips = this.numSkips + other.numSkips;

            return sum;
        }
        public WillingnessSummary Minus(WillingnessSummary other)
        {
            WillingnessSummary difference = new WillingnessSummary();
            difference.numUnpromptedParticipations = this.numUnpromptedParticipations - other.numUnpromptedParticipations;
            difference.numPromptedParticipations = this.numPromptedParticipations - other.numPromptedParticipations;
            difference.numSkips = this.numSkips - other.numSkips;

            return difference;
        }

        public Double NumUnpromptedParticipations
        {
            get
            {
                return this.numUnpromptedParticipations;
            }
            set
            {
                this.numUnpromptedParticipations = value;
            }
        }
        public double NumPromptedParticipations
        {
            get
            {
                return this.numPromptedParticipations;
            }
            set
            {
                this.numPromptedParticipations = value;
            }
        }
        public double NumSkips
        {
            get
            {
                return this.numSkips;
            }
            set
            {
                this.numSkips = value;
            }
        }

        private double numUnpromptedParticipations;
        private double numPromptedParticipations;
        private double numSkips;
    }
}
