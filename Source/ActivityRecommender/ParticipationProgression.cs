using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StatLists;

// A ParticipationProgression how much of an Activity the user has done recently
// It is intended to model brain activity and it uses exponential curves to do so
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
        public List<Participation> Participations
        {
            get
            {
                List<ListItemStats<DateTime, Participation>> items = this.searchHelper.AllItems;
                List<Participation> results = new List<Participation>();
                foreach (ListItemStats<DateTime, Participation> stats in items)
                {
                    results.Add(stats.Value);
                }
                return results;
            }
        }
        public Participation SummarizeParticipationsBetween(DateTime startDate, DateTime endDate)
        {
            Participation result = this.searchHelper.SumBetweenKeys(startDate, true, endDate, false);
            return result;
        }
        public List<ProgressionValue> GetValuesAfter(int indexInclusive)
        {
            List<ListItemStats<DateTime, Participation>> items = this.searchHelper.AllItems;
            List<ProgressionValue> results = new List<ProgressionValue>();
            int i;
            for (i = indexInclusive; i < items.Count; i++)
            {
                DateTime when = items[i].Key;
                ProgressionValue value = this.GetValueAt(when, false);
                results.Add(value);
            }
            return results;
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
            // if one participation is empty, return the other one
            if (participation1.Duration.TotalSeconds == 0)
            {
                return participation2;
            }
            if (participation2.Duration.TotalSeconds == 0)
            {
                return participation1;
            }
            // make sure participation1 is the one that starts first
            if (participation1.StartDate.CompareTo(participation2.StartDate) > 0)
            {
                // swap the participations so participation1 is the one that starts before participation2
                Participation temp = participation1;
                participation1 = participation2;
                participation2 = temp;
            }
            // set the start date to the earliest of the two start dates
            DateTime startDate = participation1.StartDate;

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

            // calculate the amount of time between the two participations
            double numIdleSeconds = 0;
            if (participation1.EndDate.CompareTo(participation2.StartDate) < 0)
            {
                TimeSpan idleTime = participation2.StartDate.Subtract(participation1.EndDate);
                numIdleSeconds = idleTime.TotalSeconds;
            }
            Distribution logIdleTime = new Distribution(0, 0, 0);
            if (numIdleSeconds > 0)
            {
                logIdleTime = Distribution.MakeDistribution(Math.Log(numIdleSeconds), 0, 1);
            }
            logIdleTime = participation1.LogIdleTime.Plus(logIdleTime);
            logIdleTime = logIdleTime.Plus(participation2.LogIdleTime);

            // create the Participation and fill the data in
            Participation result = new Participation();
            result.StartDate = startDate;
            result.EndDate = endDate;
            result.TotalIntensity = combinedIntensity;
            result.LogIdleTime = logIdleTime;
            result.LogActiveTime = participation1.LogActiveTime.Plus(participation2.LogActiveTime);

            return result;
        }

        public Participation Zero()
        {
            return new Participation();
        }

        #endregion

        #region Functions for IProgression

        public Activity Owner { get; set; }

        public ProgressionValue GetValueAt(DateTime when, bool strictlyEarlier)
        {
            // This linear calculation has shown during testing to be useful to have
            return this.GetValueLinearly(when, strictlyEarlier);
            // The exponential calculation has shown during testing to make the predictions worse
            //return this.GetValueExponentially(when, strictlyEarlier);
        }
        // returns basically the fraction of the user's time that was spent performing that activity recently at that date
        public ProgressionValue GetValueExponentially(DateTime when, bool strictlyEarlier)
        {
            // get a summary of all of the data, to estimate the duration of the "recent" past
            Participation cumulativeParticipation = this.searchHelper.SumBeforeKey(when, true);
            // make sure that we have enough data
            if (cumulativeParticipation.Duration.TotalSeconds == 0)
            {
                ProgressionValue defaultValue = new ProgressionValue(when, new Distribution(), -1);
                return defaultValue;
            }
            Distribution logIdleTime = cumulativeParticipation.LogIdleTime;
            Distribution logActiveTime = cumulativeParticipation.LogActiveTime;
            // make sure that we have enough data
            if (logIdleTime.Mean == 0 || logActiveTime.Mean == 0)
            {
                ProgressionValue defaultValue = new ProgressionValue(when, Distribution.MakeDistribution(0.5, 0.25, 0), -1);
                return defaultValue;
            }
            // now we estimate the value of the exponentially moving average
            // the half-life is typicalIdleSeconds when decaying toward 0
            // the half-life is typicalActiveSeconds when decaying toward 1
            double typicalIdleSeconds = Math.Exp(logIdleTime.Mean);
            double typicalActiveSeconds = Math.Exp(logActiveTime.Mean);

            // figure out which participations could be most relevant
            int endingIndexExclusive = this.searchHelper.CountBeforeKey(when, !strictlyEarlier);
            int startingIndex = Math.Max(0, endingIndexExclusive - 10);
            double currentValue = 0;
            int i;
            DateTime latestDate = new DateTime();
            TimeSpan idleDuration;
            double numIdleSeconds;
            double power;
            // iterate through the last few participations to calculate the recent participation intensity
            for (i = startingIndex; i < endingIndexExclusive; i++)
            {
                ListItemStats<DateTime, Participation> stats = this.searchHelper.GetValueAtIndex(i);
                Participation participation = stats.Value;
                // calculate the statistics for the idle duration
                idleDuration = participation.StartDate.Subtract(latestDate);
                numIdleSeconds = idleDuration.TotalSeconds;
                power = -numIdleSeconds / typicalIdleSeconds;
                // clamp to a reasonable range
                if (power < 0)
                {
                    // move towards 0 for the idle duration
                    currentValue = currentValue * Math.Pow(Math.E, power);
                }


                // calculate the statistics for the active duration
                double numActiveSeconds = participation.Duration.TotalSeconds;
                power = -numActiveSeconds / typicalActiveSeconds;
                // clamp to a reasonable range
                if (power < 0)
                {
                    // move towards 1 for the active duration
                    currentValue = 1 - (1 - currentValue) * Math.Pow(Math.E, power); ;
                }

                // advance the current date
                if (participation.EndDate.CompareTo(latestDate) > 0)
                    latestDate = participation.EndDate;
            }
            // add the final idle time
            idleDuration = when.Subtract(latestDate);
            numIdleSeconds = idleDuration.TotalSeconds;
            power = -numIdleSeconds / typicalIdleSeconds;
            // clamp to a reasonable range
            if (power < 0)
            {
                // move towards 0 for the idle duration
                currentValue = currentValue * Math.Pow(Math.E, power);
            }

            Distribution recentIntensity = Distribution.MakeDistribution(currentValue, 0, 1);
            ProgressionValue result = new ProgressionValue(when, recentIntensity, endingIndexExclusive);
            return result;


        }
        public ProgressionValue GetValueLinearly(DateTime when, bool strictlyEarlier)
        {
            // find the most recent participation before the given date
            ListItemStats<DateTime, Participation> latestItem = this.searchHelper.FindPreviousItem(when, true);
            if (latestItem == null)
            {
                ProgressionValue defaultValue = new ProgressionValue(when, new Distribution(0, 0, 0), -1);
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
            ProgressionValue result = new ProgressionValue(when, totalParticipation, previousCount);
            return result;
        }

        public ProgressionValue GetCurrentValue(DateTime when)
        {
            return this.GetValueAt(when, false);
        }

        public string Description
        {
            get
            {
                return "How much you've done this activity recently";
            }
        }


        public int NumItems
        {
            get
            {
                return this.searchHelper.NumItems;
            }
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
