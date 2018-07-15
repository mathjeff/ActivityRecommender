using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StatLists;
using AdaptiveLinearInterpolation;

// A LinearProgresion is a bunch of x,y points connected by straight lines
namespace ActivityRecommendation
{
    public class LinearProgression : IProgression, IComparer<DateTime>, ICombiner<double>
    {
        public LinearProgression()
        {
            this.searchHelper = new StatList<DateTime, double>(this, this);
        }

        #region General public members
        public void RemoveAllAfter(DateTime when)
        {
            while (true)
            {
                if (this.searchHelper.NumItems <= 0)
                    break;
                ListItemStats<DateTime, double> item = this.searchHelper.GetLastValue();
                if (item.Key.CompareTo(when) < 0)
                    return;
                this.searchHelper.Remove(item.Key);
            }
        }
        public void RemoveAllBefore(DateTime when)
        {
            while (true)
            {
                if (this.searchHelper.NumItems <= 0)
                    break;
                ListItemStats<DateTime, double> item = this.searchHelper.GetFirstValue();
                if (item.Key.CompareTo(when) > 0)
                    return;
                this.searchHelper.Remove(item.Key);
            }
        }
        public LinearProgression Shifted(TimeSpan offset)
        {
            LinearProgression shifted = new LinearProgression();
            foreach (ListItemStats<DateTime, double> stats in this.searchHelper.AllItems)
            {
                shifted.Add(stats.Key.Add(offset), stats.Value);
            }
            return shifted;
        }
        public LinearProgression Minus(LinearProgression other)
        {
            StatList<DateTime, double> union = this.searchHelper.Union(other.searchHelper);
            LinearProgression diff = new LinearProgression();
            DateTime prevDate = new DateTime();
            foreach (ListItemStats<DateTime, double> item in union.AllItems)
            {
                DateTime when = item.Key;
                if (when == prevDate)
                {
                    // skip duplicates
                    continue;
                }
                // this relies on this.searchHelper.optimizeForLocality = true and other.searchHelper.optimizeForLocality = true for it to be fast
                double ourValue, theirValue;
                ourValue = this.GetValueAt(when, false).Value.Mean;
                theirValue = other.GetValueAt(when, false).Value.Mean;
                diff.Add(item.Key, ourValue - theirValue);
            }
            return diff;
        }
        public IEnumerable<DateTime> Keys
        {
            get
            {
                LinkedList<DateTime> keys = new LinkedList<DateTime>();
                foreach (ListItemStats<DateTime, double> item in this.searchHelper.AllItems)
                {
                    keys.AddLast(item.Key);
                }
                return keys;
            }
        }


        #endregion

        #region IProgression
        public void Add(DateTime when, double value)
        {
            this.searchHelper.Add(when, value);
        }

        public ProgressionValue GetValueAt(DateTime when, bool strictlyAfter)
        {
            ListItemStats<DateTime, double> prev = this.searchHelper.FindPreviousItem(when, false);
            if (prev == null)
                return this.DefaultValue(when);
            ListItemStats<DateTime, double> next = this.searchHelper.FindNextItem(when, false);
            if (next == null)
                return this.DefaultValue(when);
            double prevDuration = when.Subtract(prev.Key).TotalSeconds;
            double nextDuration = next.Key.Subtract(when).TotalSeconds;
            double totalWeight = prevDuration + nextDuration;
            if (totalWeight <= 0)
            {
                Distribution avg = new Distribution();
                avg.Add(prev.Value);
                avg.Add(next.Value);
                return new ProgressionValue(when, avg);
            }
            double prevWeight = nextDuration / totalWeight;
            double nextWeight = 1 - prevWeight;
            Distribution average = new Distribution();
            average.Add(prev.Value, prevWeight);
            average.Add(next.Value, nextWeight);
            return new ProgressionValue(when, average);
        }
        private ProgressionValue DefaultValue(DateTime when)
        {
            return new ProgressionValue(when, new Distribution());
        }

        public Doable Owner
        {
            get
            {
                return null;
            }
        }
        public int NumItems
        {
            get
            {
                return this.searchHelper.NumItems;
            }
        }
        public IEnumerable<ProgressionValue> GetValuesAfter(int index)
        {
            throw new NotImplementedException();
        }
        public IEnumerable<double> GetNaturalSubdivisions(double min, double max)
        {
            throw new NotImplementedException();
        }
        public FloatRange EstimateOutputRange()
        {
            throw new NotImplementedException();
        }
        public string Description
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        #endregion


        #region IComparer
        public int Compare(DateTime a, DateTime b)
        {
            return DateTime.Compare(a, b);
        }
        #endregion

        #region ICombiner
        public double Combine(double a, double b)
        {
            return a + b;
        }
        public double Default()
        {
            return 0;
        }
        #endregion

        private StatList<DateTime, double> searchHelper;
    }
}
