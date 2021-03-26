using StatLists;
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
                List<ProtoActivity> results = new List<ProtoActivity>(this.protoActivities.Count);
                foreach (ProtoActivity activity in this.protoActivities)
                {
                    if (activity != null)
                        results.Add(activity);
                }
                return results;
            }
        }

        public int Count
        {
            get
            {
                return this.ProtoActivities.Count();
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
        public List<ProtoActivity_EstimatedInterest> GetMostInteresting(int count)
        {
            List<ProtoActivity> candidates = new List<ProtoActivity>(this.ProtoActivities);
            List<ProtoActivity_EstimatedInterest> interests = new ProtoActivity_InterestCalculator(DateTime.Now).Analyze(candidates);
            int minIndex = candidates.Count - count;
            if (minIndex < 0)
                minIndex = 0;
            List<ProtoActivity_EstimatedInterest> results = new List<ProtoActivity_EstimatedInterest>(count);
            for (int i = interests.Count - 1; i>= minIndex; i--)
            {
                results.Add(interests[i]);
            }
            return results;
        }

        public ProtoActivity TextSearch(string query)
        {
            List<ProtoActivity> matches = this.TextSearch(query, 1);
            if (matches.Count > 0)
                return matches[0];
            return null;
        }

        public List<ProtoActivity> TextSearch(string query, int count)
        {
            if (query == null || query == "" || count < 1)
                return new List<ProtoActivity>(0);

            StatList<double, ProtoActivity> sortedItems = new StatList<double, ProtoActivity>(new ReverseDoubleComparer(), new NoopCombiner<ProtoActivity>());
            foreach (ProtoActivity protoActivity in this.ProtoActivities)
            {
                double textQuality = this.stringQueryMatcher.StringScore(protoActivity.Text, query);
                if (textQuality > 0)
                {
                    double matchQuality = textQuality + protoActivity.IntrinsicInterest;

                    sortedItems.Add(matchQuality, protoActivity);
                }
            }
            count = Math.Min(count, sortedItems.NumItems);
            List<ProtoActivity> top = new List<ProtoActivity>(count);
            for (int i = 0; i < count; i++)
            {
                top.Add(sortedItems.GetValueAtIndex(i).Value);
            }
            return top;
        }

        private List<ProtoActivity> protoActivities = new List<ProtoActivity>();
        private StringQueryMatcher stringQueryMatcher = new StringQueryMatcher();
    }

    class ProtoActivity_InterestCalculator : IComparer<ProtoActivity_EstimatedInterest>
    {
        public ProtoActivity_InterestCalculator(DateTime when)
        {
            this.when = when;
        }

        public List<ProtoActivity_EstimatedInterest> Analyze(List<ProtoActivity> protoActivities)
        {
            List<ProtoActivity_EstimatedInterest> interests = new List<ProtoActivity_EstimatedInterest>();
            foreach (ProtoActivity protoActivity in protoActivities)
            {
                ProtoActivity_EstimatedInterest estimate = this.computeInterest(protoActivity);
                interests.Add(estimate);
            }
            interests.Sort(this);
            return interests;
        }

        // Gives a score telling how soon we want to show the given ProtoActivity next
        // Higher scores indicate to show the given ProtoActivity sooner
        public ProtoActivity_EstimatedInterest computeInterest(ProtoActivity protoactivity)
        {
            ProtoActivity_EstimatedInterest interest = new ProtoActivity_EstimatedInterest();
            interest.ProtoActivity = protoactivity;
            interest.IntrinsicInterest = protoactivity.IntrinsicInterest;
            interest.NumIdleSeconds = this.when.Subtract(protoactivity.LastInteractedWith).TotalSeconds;
            interest.CurrentInterest = interest.IntrinsicInterest * interest.NumIdleSeconds;

            return interest;
        }
        public int Compare(ProtoActivity_EstimatedInterest a, ProtoActivity_EstimatedInterest b)
        {
            return a.CurrentInterest.CompareTo(b.CurrentInterest);
        }

        private DateTime when;
    }

    public class ProtoActivity_EstimatedInterest
    {
        // The ProtoActivity being described
        public ProtoActivity ProtoActivity;
        // How interesting we believe this ProtoActivity to be now
        public double CurrentInterest;
        // How long since the user last interacted with this protoactivity
        public double NumIdleSeconds;
        // How interesting we believe this protoactivity is to the user in general
        public double IntrinsicInterest;
    }
}
