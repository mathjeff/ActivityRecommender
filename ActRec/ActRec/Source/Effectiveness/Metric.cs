using ActivityRecommendation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityRecommendation.Effectiveness
{
    // a Metric is something that can compute a floating-point effectiveness score for a particular Participation
    // However, there isn't yet any need for custom conversion-to-float logic; for now all that's needed is an indication that the Activity has a Metric that can be displayed to the user
    public interface Metric
    {
        string Name { get; }
        ActivityDescriptor ActivityDescriptor { get; }
    }

    public class CompletionMetric : Metric
    {
        public CompletionMetric(string name, Activity activity)
        {
            this.name = name;
            this.activity = activity;
        }
        public string Name
        {
            get
            {
                return this.name;
            }
        }
        public ActivityDescriptor ActivityDescriptor
        {
            get
            {
                return this.activity.MakeDescriptor();
            }
        }
        private string name;
        private Activity activity;
    }

    public class TodoMetric : CompletionMetric
    {
        public TodoMetric(ToDo todo) : base("Finish", todo)
        {
        }
    }

}
