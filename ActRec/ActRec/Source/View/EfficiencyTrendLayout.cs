using StatLists;
using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;

namespace ActivityRecommendation.View
{
    class EfficiencyTrendLayout : ContainerLayout
    {
        public EfficiencyTrendLayout(EfficiencyCorrelator correlator)
        {
            this.efficiencyCorrelator = correlator;
            this.efficiencyCorrelator.CorrelatorUpdated += EfficiencyCorrelator_CorrelatorUpdated;
        }

        private void EfficiencyCorrelator_CorrelatorUpdated()
        {
            this.AnnounceChange(true);
        }

        public override SpecificLayout GetBestLayout(LayoutQuery query)
        {
            this.SubLayout = this.makeSublayout();
            return base.GetBestLayout(query);
        }

        private LayoutChoice_Set makeSublayout()
        {
            Correlator correlator = this.efficiencyCorrelator.Correlator;
            if (correlator.HasCorrelation)
            {
                double slope = correlator.Slope;
                if (slope != 0)
                {
                    double numYearsPerDouble = 1.0 / slope;
                    double roundedNumYears = Math.Round(Math.Abs(numYearsPerDouble), 2);
                    string result;
                    if (numYearsPerDouble > 0)
                        result = "Your data indicates that your efficiency doubles every " + roundedNumYears + " years. Nice!";
                    else
                        result = "Your data indicates that your efficiency halves every " + roundedNumYears + " years. Keep trying anyway!";
                    result += "\n";
                    result += "\nTo see a graph of your efficiency over time, go back to Visualize one Activity.";
                    result += "\nTo find your most and least efficient participations, go back to Search Participations.";
                    return new TextblockLayout(result);
                }
                else
                {
                    return new TextblockLayout("Your efficiency has not changed yet. Collect more data by running more experiments!");
                }
            }
            else
            {
                return new TextblockLayout("We don't have enough efficiency data yet. Collect more data by running more experiments!");
            }
        }

        EfficiencyCorrelator efficiencyCorrelator;
    }
}
