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
        public DateTime LastInteractedWith { get; set; }
        public Distribution Ratings { get; set; }
        private string text;
    }

}
