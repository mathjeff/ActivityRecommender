using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityRecommendation
{
    public class ProtoActivity_Database
    {
        public event ProtoActivity_TextChanged_Handler TextChanged;
        public delegate void ProtoActivity_TextChanged_Handler();

        public event ProtoActivity_RatingsChanged_Handler RatingsChanged;
        public delegate void ProtoActivity_RatingsChanged_Handler();

        public ProtoActivity_Database()
        {
            this.Synchronized = true;
        }
        public void Put(ProtoActivity protoActivity)
        {
            if (protoActivity.Id < 0)
                protoActivity.Id = this.protoActivities.Count;
            while (this.protoActivities.Count <= protoActivity.Id)
            {
                this.protoActivities.Add(null);
            }
            this.protoActivities[protoActivity.Id] = protoActivity;

            if (protoActivity.Text != null && protoActivity.Text != "")
                this.ProtoActivity_TextChanged(protoActivity);
            protoActivity.TextChanged += ProtoActivity_TextChanged;
        }

        public ProtoActivity Get(int id)
        {
            if (id >= this.protoActivities.Count)
                return null;
            return this.protoActivities[id];
        }

        public void Remove(ProtoActivity activity)
        {
            if (activity.Id >= 0)
                this.protoActivities[activity.Id] = null;
        }

        public IEnumerable<ProtoActivity> ProtoActivities
        {
            get
            {
                List<ProtoActivity> results = new List<ProtoActivity>();
                foreach (ProtoActivity activity in this.protoActivities)
                {
                    if (activity != null)
                        results.Add(activity);
                }
                return results;
            }
        }

        public void MarkWorse(ProtoActivity worseActivity, ProtoActivity betterActivity, DateTime when)
        {
            worseActivity.MarkWorse(when);
            betterActivity.MarkBetter(when);
            if (this.RatingsChanged != null)
            {
                this.RatingsChanged.Invoke();
            }
        }

        private void ProtoActivity_TextChanged(ProtoActivity activity)
        {
            this.Synchronized = false;
            if (this.TextChanged != null)
            {
                this.TextChanged.Invoke();
            }
        }

        // Whether the contents of this database have been saved to storage
        public bool Synchronized { get; set; }

        // sorts the ProtoActivities by score and returns the highest-scoring activities
        public List<ProtoActivity> GetMostInteresting(int count)
        {
            List<ProtoActivity> candidates = new List<ProtoActivity>(this.ProtoActivities);
            ProtoActivity_InterestComparer comparer = new ProtoActivity_InterestComparer(DateTime.Now);
            candidates.Sort(comparer);
            int minIndex = candidates.Count - count;
            if (minIndex < 0)
                minIndex = 0;
            List<ProtoActivity> results = new List<ProtoActivity>();
            for (int i = candidates.Count - 1; i >= minIndex; i--)
            {
                results.Add(candidates[i]);
            }
            for (int i = 0; i < candidates.Count; i++)
            {
                System.Diagnostics.Debug.WriteLine("Score [" + i + "] = " + comparer.computeInterest(candidates[i]));
            }
            return results;
        }
        List<ProtoActivity> protoActivities = new List<ProtoActivity>();
    }

    class ProtoActivity_InterestComparer : IComparer<ProtoActivity>
    {
        public ProtoActivity_InterestComparer(DateTime when)
        {
            this.when = when;
        }
        public int Compare(ProtoActivity a, ProtoActivity b)
        {
            return this.computeInterest(a).CompareTo(this.computeInterest(b));
        }

        // Gives a score telling how soon we want to show the given ProtoActivity next
        // Higher scores indicate to show the given ProtoActivity sooner
        public double computeInterest(ProtoActivity activity)
        {
            Distribution distribution = activity.Ratings;
            double ratingMean = distribution.Plus(Distribution.MakeDistribution(0.5, 0.5, 2)).Mean;
            double numIdleSeconds = this.when.Subtract(activity.LastInteractedWith).TotalSeconds;
            return ratingMean * numIdleSeconds;
        }

        private DateTime when;

    }
}
