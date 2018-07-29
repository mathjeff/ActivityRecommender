using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// a Metric is a measurement used within an experiment
// a Metric gives a numerical rating to a user's Participation
namespace ActivityRecommendation.Effectiveness
{
    public interface Metric
    {
        // for showing to the user
        string Describe();

        // the activity being measured
        Activity GetDoable();

        // the score of a Participation that was measured by this Metric
        double GetScore(Participation participation);
    }
}
