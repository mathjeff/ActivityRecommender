using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StatLists;

namespace ActivityRecommendation
{
    public class ParticipationProgression : IComparer<DateTime>, IAdder<Participation>, IProgression
    {
        #region Constructor

        public ParticipationProgression(Activity owner)
        {
            this.Owner = owner;
            this.searchHelper = new StatList<DateTime, Participation>(this, this);
        }

        #endregion

        #region Public Member Functions

        public void AddParticipation(Participation newParticipation)
        {
            if (this.ShouldIncludeParticipation(newParticipation))
            {
                // for now, we add the participation as a single datapoint, where the key knows the start date but not the end date
                // Eventually, the StatList should be improved to allow intervals as keys
                DateTime startDate = newParticipation.StartDate;
                this.searchHelper.Add(startDate, newParticipation);
            }
        }

        // this function is a filter that tells whether this ParticipationProgression cares about this Participation
        public bool ShouldIncludeParticipation(Participation newParticipation)
        {
            return true;
        }

        #endregion

        #region Functions for IComparer<Participation>

        // Compare based on end date
        public int Compare(DateTime date1, DateTime date2)
        {
            return date1.CompareTo(date2);
        }

        #endregion

        #region Functions for IAdder<Participation>

        public Participation Sum(Participation participation1, Participation participation2)
        {
            // if one participation is empty, don't adjust the start/end dates
            if (participation1.Duration.TotalSeconds == 0)
            {
                return participation2;
            }
            if (participation2.Duration.TotalSeconds == 0)
            {
                return participation1;
            }
            // set the start date to the earliest of the two start dates
            DateTime start1 = participation1.StartDate;
            DateTime start2 = participation2.StartDate;
            DateTime startDate;
            if (start1.CompareTo(start2) < 0)
            {
                startDate = start1;
            }
            else
            {
                startDate = start2;
            }

            // set the end date to the latest of the two end dates
            DateTime end1 = participation1.EndDate;
            DateTime end2 = participation2.EndDate;
            DateTime endDate;
            if (end1.CompareTo(end2) > 0)
            {
                endDate = end1;
            }
            else
            {
                endDate = end2;
            }

            // add up the intensities of the two participations
            Distribution intensity1 = participation1.TotalIntensity;
            Distribution intensity2 = participation2.TotalIntensity;
            Distribution combinedIntensity = intensity1.Plus(intensity2);

            Participation result = new Participation();
            result.StartDate = startDate;
            result.EndDate = endDate;
            result.TotalIntensity = combinedIntensity;

            return result;
        }

        public Participation Zero()
        {
            return new Participation();
        }

        #endregion

        #region Functions for IProgression

        public Activity Owner { get; set; }
        // returns basically the fraction of the user's time that was spent performing that activity recently at that date
        public ProgressionValue GetValueAt(DateTime when, bool strictlyEarlier)
        {
            // find the most recent participation before the given date
            ListItemStats<DateTime, Participation> latestItem = this.searchHelper.FindPreviousItem(when, true);
            if (latestItem == null)
            {
                ProgressionValue defaultValue = new ProgressionValue(new Distribution(0, 0, 0), -1);
                return defaultValue;
            }
            Participation latestParticipation = latestItem.Value;
            DateTime latestStartDate = latestParticipation.StartDate;

            // compute how long ago that participation began
            TimeSpan duration = when.Subtract(latestStartDate);
            // create another date that is twice as far in the past
            DateTime earlierDate = latestStartDate.Subtract(duration);

            // find the previous participation item that started before that
            ListItemStats<DateTime, Participation> previousItem = this.searchHelper.FindPreviousItem(earlierDate, true);
            // get the startDate from the previous item
            DateTime previousStartDate;
            if (previousItem != null)
            {
                previousStartDate = previousItem.Value.StartDate;
            }
            else
            {
                ListItemStats<DateTime, Participation> firstItem = this.searchHelper.GetValueAtIndex(0);
                previousStartDate = firstItem.Value.StartDate;
            }

            
            
            // Ask the searchHelper for the sum of all the participations from earlierDate to latestStartDate
            Participation sumParticipations = this.searchHelper.SumBetweenKeys(previousStartDate, true, latestStartDate, false);
            Distribution totalIntensity = sumParticipations.TotalIntensity;
            // determine how much time is encompassed by these participations
            TimeSpan totalDuration = latestStartDate.Subtract(previousStartDate);
            double totalWeight = this.GetWeight(totalDuration);
            double activeWeight = sumParticipations.TotalIntensity.Weight;
            // determine how much time is unaccounted for
            double idleWeight = totalWeight - activeWeight;
            if (idleWeight < 0)
            {
                idleWeight = 0;
            }
            // make another Distribution telling how much idle time took place between earlierDate and latestStartDate and 
            Distribution primaryIdleTime = new Distribution(0, 0, idleWeight);
            Distribution totalParticipation = totalIntensity.Plus(primaryIdleTime);

            // figure out how much extra intensity was added by the final participation
            if (when.CompareTo(latestParticipation.EndDate) < 0)
            {
                // if we get here, then 'when' is in the middle of the final participation
                // Figure out what fraction of the participation was 
                double fullNumSeconds = latestParticipation.Duration.TotalSeconds;
                TimeSpan finalDuration = when.Subtract(latestParticipation.StartDate);
                double actualNumSeconds = finalDuration.TotalSeconds;
                if (fullNumSeconds == 0)
                    fullNumSeconds = 1;
                double scale = actualNumSeconds / fullNumSeconds;
                Distribution finalDistribution = latestParticipation.TotalIntensity.CopyAndStretchBy(scale);
                totalParticipation = totalParticipation.Plus(finalDistribution);
            }
            else
            {
                // if we get here, then 'when' is after the final participation
                // add the final participation
                totalParticipation = totalParticipation.Plus(latestParticipation.TotalIntensity);
                // add the idle time after the final participation
                TimeSpan finalIdleTime = when.Subtract(latestParticipation.EndDate);
                Distribution finalIdleDistribution = new Distribution(0, 0, this.GetWeight(finalIdleTime));
                totalParticipation = totalParticipation.Plus(finalIdleDistribution);
            }

            // now totalParticipation is a distribution of the intensities of the participations of the activity over the recent past
            int previousCount = this.searchHelper.CountBeforeKey(when, true);
            ProgressionValue result = new ProgressionValue(totalParticipation, previousCount);
            return result;
        }

        #endregion

        #region Private Member Functions

        // Given the duration of a Participation, returns the weight for that Participation
        // This simply allows the units to match all the time, and changing this from seconds to minutes would not cause inconsistencies
        double GetWeight(TimeSpan duration)
        {
            return duration.TotalSeconds;
        }

        #endregion

        private StatList<DateTime, Participation> searchHelper; // in the future the StatList may be improved to properly support intervals.
    }
}
