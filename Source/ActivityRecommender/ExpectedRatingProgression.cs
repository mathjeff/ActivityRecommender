﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActivityRecommendation
{
    public class ExpectedRatingProgression : IProgression
    {
        public ExpectedRatingProgression(Activity owner)
        {
            this.Owner = owner;
        }
        public ProgressionValue GetValueAt(DateTime when, bool strictlyEarlier)
        {
            ProgressionValue value = new ProgressionValue(when, Owner.PredictedScore.Distribution, Owner.NumRatings);
            return value;
        }
        public List<ProgressionValue> GetValuesAfter(int indexInclusive)
        {
            return null;
        }
        public int NumItems
        {
            get
            {
                return 1;
            }
        }
        public string Description
        {
            get
            {
                return "The current rating of " + this.Owner.Description;
            }
        }
        public Activity Owner { get; set; }

    }
}
