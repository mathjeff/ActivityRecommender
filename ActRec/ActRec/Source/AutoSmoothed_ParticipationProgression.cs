﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StatLists;
using AdaptiveLinearInterpolation;

// A ParticipationProgression how much of an Doable the user has done recently
// It is intended to model brain Doable and it uses exponential curves to do so
namespace ActivityRecommendation
{
    public class ParticipationAndSummary
    {
        public ParticipationAndSummary() { }

        public Participation Participation { get; set; }
        public ParticipationsSummary Summary { get; set; }

    }
    public class AutoSmoothed_ParticipationProgression : IComparer<DateTime>, ICombiner<ParticipationAndSummary>, IProgression
    {
        #region Constructor

        public AutoSmoothed_ParticipationProgression(Doable owner)
        {
            this.Owner = owner;
            this.searchHelper = new StatList<DateTime, ParticipationAndSummary>(this, this);
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
                this.searchHelper.Add(startDate, this.Summarize(newParticipation));
            }
        }
        public void RemoveParticipation(Participation participation)
        {
            // make sure that we actually added it in the first place
            if (this.ShouldIncludeParticipation(participation))
            {
                // for now, the searchHelper identifies the item solely based on key, but it's okay, because ties are broken in a last-in-first-out manner
                DateTime startDate = participation.StartDate;
                this.searchHelper.Remove(startDate);
            }
        }

        // this function is a filter that tells whether this ParticipationProgression cares about this Participation
        public bool ShouldIncludeParticipation(Participation newParticipation)
        {
            return true;
        }
        public IEnumerable<Participation> Participations
        {
            get
            {
                IEnumerable<ListItemStats<DateTime, ParticipationAndSummary>> items = this.searchHelper.AllItems;
                LinkedList<Participation> results = new LinkedList<Participation>();
                foreach (ListItemStats<DateTime, ParticipationAndSummary> stats in items)
                {
                    results.AddLast(stats.Value.Participation);
                }
                return results;
            }
        }
        public Participation LatestParticipation
        {
            get
            {
                ListItemStats<DateTime, ParticipationAndSummary> stats = this.searchHelper.GetLastValue();
                if (stats != null)
                    return stats.Value.Participation;
                return null;
            }
        }
        public ParticipationsSummary SummarizeParticipationsBetween(DateTime startDate, DateTime endDate)
        {
            ParticipationAndSummary result = this.searchHelper.CombineBetweenKeys(startDate, true, endDate, false);
            if (result == null)
            {
                ParticipationsSummary summary = new ParticipationsSummary();
                summary.Start = startDate;
                summary.End = endDate;
                summary.CumulativeIntensity = new TimeSpan();
                summary.LogActiveTime = Distribution.MakeDistribution(0, 0, 0);
                summary.Trend = new Correlator();
                return summary;
            }
            return result.Summary;
        }
        public IEnumerable<ProgressionValue> GetValuesAfter(int indexInclusive)
        {
            IEnumerable<ListItemStats<DateTime, ParticipationAndSummary>> items = this.searchHelper.ItemsFromIndex(indexInclusive);
            List<ProgressionValue> results = new List<ProgressionValue>();
            foreach (ListItemStats<DateTime, ParticipationAndSummary> item in items)
            {
                DateTime when = item.Key;
                ProgressionValue value = this.GetValueAt(when, false);
                results.Add(value);
            }
            return results;
        }
        public LinearProgression Smoothed(TimeSpan windowSize, DateTime maxDate)
        {
            // make a LinkedList of the cumulative time spent
            DateTime minDate = this.Owner.DiscoveryDate;
            IEnumerable<ListItemStats<DateTime, ParticipationAndSummary>> items = this.searchHelper.AllItems;
            LinearProgression cumulatives = new LinearProgression();
            DateTime when = minDate;
            double sum = 0;
            foreach (ListItemStats<DateTime, ParticipationAndSummary> item in items)
            {
                cumulatives.Add(item.Value.Participation.StartDate, sum);
                sum += item.Value.Participation.Duration.TotalSeconds;
                cumulatives.Add(item.Value.Participation.EndDate, sum);
            }
            // find what's in the sliding window by subtracting the cumulative from the shifted cumulative
            LinearProgression shiftedCumulatives = cumulatives.Shifted((new TimeSpan()).Subtract(windowSize));
//            shiftedCumulatives.RemoveAllBefore(minDate);
            LinearProgression result = shiftedCumulatives.Minus(cumulatives);
            result.RemoveAllBefore(minDate);

            return result;
        }

        #endregion

        #region Functions for IComparer<Participation>

        // Compare based on end date
        public int Compare(DateTime date1, DateTime date2)
        {
            return date1.CompareTo(date2);
        }

        #endregion

        #region Functions for ICombiner<Participation>

        public ParticipationAndSummary Combine(ParticipationAndSummary a, ParticipationAndSummary b)
        {
            if (a == null)
                return b;
            if (b == null)
                return a;
            ParticipationsSummary earlier = a.Summary;
            ParticipationsSummary later = b.Summary;
            if (earlier.Start.CompareTo(later.Start) > 0)
            {
                ParticipationsSummary temp = earlier;
                earlier = later;
                later = temp;
            }

            ParticipationsSummary summary = new ParticipationsSummary();

            summary.CumulativeIntensity = earlier.CumulativeIntensity.Add(later.CumulativeIntensity);
            summary.Start = earlier.Start;
            summary.End = later.End;
            summary.LogActiveTime = earlier.LogActiveTime.Plus(later.LogActiveTime);

            double earlierSum = this.GetWeight(earlier.CumulativeIntensity);
            Correlator correlator = earlier.Trend.Clone();
            TimeSpan idleDuration = later.Start.Subtract(earlier.End);
            double idleX1 = this.GetWeight(earlier.End.Subtract(earlier.Start));
            double idleX2 = this.GetWeight(later.Start.Subtract(earlier.Start));
            if (idleDuration.TotalSeconds > 0)
            {
                double idleY = earlierSum;
                double weight = this.GetWeight(idleDuration);
                correlator.Add(idleX1, idleY, weight);
                correlator.Add(idleX2, idleY, weight);
            }

            Correlator laterShifted = later.Trend.CopyAndShiftUp(earlierSum);
            laterShifted = laterShifted.CopyAndShiftRight(idleX2);
            correlator = correlator.Plus(laterShifted);
            summary.Trend = correlator;

            ParticipationAndSummary result = new ParticipationAndSummary();
            result.Summary = summary;

            if (summary.Trend.Correlation < 0)
            {
                System.Diagnostics.Debug.WriteLine("illegal correlation found: " + summary.Trend.Correlation + " for Doable " + this.Owner.Name);
            }
            // not bothering to assign a Participation because nothing will need it

            return result;
        }

        public ParticipationAndSummary Default()
        {
            return null;
        }

        #endregion

        #region Functions for IProgression

        public Doable Owner { get; set; }

        public ProgressionValue GetValueAt(DateTime when, bool strictlyEarlier)
        {
            return this.GetValueViaCorrelation(when, strictlyEarlier);
        }

        public ProgressionValue GetValueViaCorrelation(DateTime when, bool strictlyEarlier)
        {

            double resultNumber = 0.5;
            DateTime startDate;
            // The UI for logging data improved at this date. Any earlier data isn't used for computing correlation
            DateTime changeDate = new DateTime(2015, 9, 20);
            if (when.CompareTo(changeDate) > 0)
                startDate = changeDate;
            else
                startDate = DateTime.MinValue;
            
            ParticipationAndSummary info = this.searchHelper.CombineBetweenKeys(startDate, true, when, !strictlyEarlier);

            if (info != null)
            {
                ParticipationsSummary summary = info.Summary;

                Correlator correlator = summary.Trend;

                double slope = correlator.Slope;
                double x = this.GetWeight(when.Subtract(summary.Start));
                double predictedY = correlator.GetYForX(x);
                double actualY = this.GetWeight(summary.CumulativeIntensity);
                double deltaY = actualY - predictedY;
                // Ideally we'd just return deltaY, but we have to transform it into a predefined range so the interpolator can use it
                // Although it would be possible to divide by the total time elapsed, that would cause any deviation to decrease over time
                // So instead we just warp the space from (-infinity, infinity) into (0, 1)
                bool positive = true;
                if (deltaY < 0)
                {
                    deltaY *= -1;
                    positive = false;
                }

                // rescale such that a 32 hour deviation reaches halfway to the end of the space
                resultNumber = deltaY / this.GetWeight(TimeSpan.FromHours(32));

                // rescale such that nothing escapes the end of the space
                resultNumber = 1.0 - 1.0 / (1.0 + Math.Log(1 + resultNumber));

                if (positive)
                {
                    resultNumber = (resultNumber + 1.0) / 2.0;
                }
                else
                {
                    resultNumber = (1 - resultNumber) / 2.0;
                }
            }
            return new ProgressionValue(when, Distribution.MakeDistribution(resultNumber, 0, 0));
        }

        public IEnumerable<double> GetNaturalSubdivisions(double minSubdivision, double maxSubdivision)
        {
            throw new NotImplementedException();
        }


        public ProgressionValue GetCurrentValue(DateTime when)
        {
            return this.GetValueAt(when, false);
        }

        public string Description
        {
            get
            {
                return "How much you've done this Doable recently";
            }
        }

        public int NumItems
        {
            get
            {
                return this.searchHelper.NumItems;
            }
        }

        public FloatRange EstimateOutputRange()
        {
            return new FloatRange(0, true, 1, true);
        }


        #endregion

        #region Private Member Functions

        // Given the duration of a Participation, returns the weight for that Participation
        // This simply allows the units to match all the time, and changing this from seconds to minutes would not cause inconsistencies
        double GetWeight(TimeSpan duration)
        {
            return duration.TotalSeconds;
        }


        ParticipationAndSummary Summarize(Participation participation)
        {
            ParticipationAndSummary result = new ParticipationAndSummary();
            result.Participation = participation;

            ParticipationsSummary summary = new ParticipationsSummary();

            Correlator correlator = new Correlator();
            double x = this.GetWeight(participation.Duration);
            double weight = x / 2;
            correlator.Add(0, 0, weight);
            correlator.Add(x, x, weight);

            summary.Trend = correlator;

            Distribution logActiveTime = Distribution.MakeDistribution(Math.Log(participation.Duration.TotalSeconds), 0, 1);
            summary.LogActiveTime = logActiveTime;

            summary.Start = participation.StartDate;
            summary.End = participation.EndDate;

            summary.CumulativeIntensity = participation.Duration;

            result.Summary = summary;


            return result;

        }

        #endregion

        private StatList<DateTime, ParticipationAndSummary> searchHelper; // in the future the StatList may be improved to properly support intervals.
    }
}
