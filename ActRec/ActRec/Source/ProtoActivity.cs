using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityRecommendation
{
    // a ProtoActivity is an idea that the user is considering turning into an actual Activity
    public class ProtoActivity
    {
        public event ProtoActivity_TextChanged_Handler TextChanged;
        public delegate void ProtoActivity_TextChanged_Handler(ProtoActivity activity);

        public ProtoActivity(string text, DateTime lastInteractedWith, Distribution ratings)
        {
            this.Id = -1;
            this.text = text;
            this.LastInteractedWith = lastInteractedWith;
            this.Ratings = ratings;
        }
        public void MarkBetter(DateTime when)
        {
            this.Ratings = this.Ratings.Plus(Distribution.MakeDistribution(1, 0, 1));
            this.LastInteractedWith = when;
        }
        public void MarkWorse(DateTime when)
        {
            this.Ratings = this.Ratings.Plus(Distribution.MakeDistribution(0, 0, 1));
            this.LastInteractedWith = when;
        }
        public int Id { get; set; }
        public string Text
        {
            get
            {
                return this.text;
            }
            set
            {
                if (this.text != value)
                {
                    this.text = value;
                    if (this.TextChanged != null)
                        this.TextChanged.Invoke(this);
                }
            }
        }

        // An estimate of the relative probability that this protoactivity will be the next one to be promoted to a protoactivity.
        public double IntrinsicInterest
        {
            get
            {
                // We're not doing any fancy machine-learning at the moment, just some simple guesses based on how many times this has been marked better or worse.
                BinomialDistribution distribution = new BinomialDistribution(this.Ratings);
                // Being marked better indicates a higher probability, and being marked worse indicates a lower probability
                // Being marked better and then being marked worse overall indicates a lower probability, because it means the user is interacting with this protoactivity and not promoting it.
                // So, we use the number of worses to first compute a ceiling on the score, and then we incorporate the number of betters to decide where in that zone the score will be.
                // For example, if (numZeros,numOnes)=(0,0) then intrinsicInterest = 1*1/2=1/2 whereas for (1,1) intrinisicInterest = 1/2*2/3 = 1/3
                return (1.0 / (distribution.NumZeros + 1)) * (1.0 - 1.0 / (distribution.NumOnes + 2));
            }
        }
        public string Summarize()
        {
            string text = this.Text.Trim();
            int originalLength = text.Length;
            int maxLength = 300;
            if (text.Length > maxLength)
                text = text.Substring(0, maxLength);
            int newlineIndex = text.IndexOf("\n");
            if (newlineIndex >= 0)
                text = text.Substring(0, newlineIndex);
            if (text.Length < originalLength)
                text = text + "...";
            return text;
        }

        public DateTime LastInteractedWith { get; set; }
        public Distribution Ratings { get; set; }
        private string text;
    }

}
