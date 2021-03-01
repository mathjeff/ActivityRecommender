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
        private static WillingnessSummary skipped;
        public static WillingnessSummary Skipped
        {
            get
            {
                if (skipped == null)
                    skipped = new WillingnessSummary(0, 0, 1);
                return skipped;
            }
        }
        private static WillingnessSummary prompted;
        public static WillingnessSummary Prompted
        {
            get
            {
                if (prompted == null)
                    prompted = new WillingnessSummary(0, 1, 0);
                return prompted;
            }
        }
        private static WillingnessSummary unprompted;
        public static WillingnessSummary Unprompted
        {
            get
            {
                if (unprompted == null)
                    unprompted = new WillingnessSummary(1, 0, 0);
                return unprompted;
            }
        }
        private static WillingnessSummary empty;
        public static WillingnessSummary Empty
        {
            get
            {
                if (empty == null)
                    empty = new WillingnessSummary(0, 0, 0);
                return empty;
            }
        }

        public WillingnessSummary()
        {
        }
        public WillingnessSummary(int unpromptedParticipationCount, int promptedParticipationCount, int skipCount)
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

        public int NumUnpromptedParticipations
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
        public int NumPromptedParticipations
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
        public int NumSkips
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

        private int numUnpromptedParticipations;
        private int numPromptedParticipations;
        private int numSkips;
    }
}
