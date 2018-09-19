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
        string Describe();
    }

    public class TodoMetric : Metric
    {
        public TodoMetric(ToDo todo)
        {
            this.todo = todo;
        }
        public string Describe()
        {
            return "Complete " + this.todo.Name;
        }
        private ToDo todo;
    }
}
