using StatLists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisiPlacement;

// A ParticipationCorrelationView shows the activities that generally precede another activity
namespace ActivityRecommendation.View
{
    class ParticipationCorrelationView : ContainerLayout
    {
        public ParticipationCorrelationView(Engine engine, IEnumerable<Activity> activitiesToPredictFrom, Activity activityToPredict, TimeSpan windowSize)
        {
            DateTime now = DateTime.Now;
            engine.EnsureRatingsAreAssociated();

            LinearProgression progressionToPredict = activityToPredict.ParticipationsSmoothed((new TimeSpan()).Subtract(windowSize));

            StatList<double, Activity> results = new StatList<double, Activity>(new DoubleComparer(), new NoopCombiner<Activity>());

            foreach (Activity activity in activitiesToPredictFrom)
            {
                System.Diagnostics.Debug.WriteLine("comparing " + activity + " and " + activityToPredict.Name);
                List<Datapoint> datapoints = activity.compareParticipations(windowSize, progressionToPredict, now.Subtract(windowSize));

                // now compute the value of the formula
                Correlator correlator = new Correlator();
                foreach (Datapoint datapoint in datapoints)
                {
                    correlator.Add(datapoint);
                }

                double outputIncreasePerInputIncrease = correlator.Slope;
                if (!double.IsNaN(outputIncreasePerInputIncrease))
                {
                    results.Add(outputIncreasePerInputIncrease, activity);
                }
            }

            IEnumerable<ListItemStats<double, Activity>> resultList = results.AllItems;

            GridLayout_Builder layoutBuilder = new Vertical_GridLayout_Builder().Uniform();
            List<ListItemStats<double, Activity>> mostPositivelyCorrelated = new List<ListItemStats<double, Activity>>();
            List<ListItemStats<double, Activity>> mostNegativelyCorrelated = new List<ListItemStats<double, Activity>>();
            int i = 0;
            int numPositives = Math.Min(4, resultList.Count());
            foreach (ListItemStats<double, Activity> result in resultList.Reverse())
            {
                mostPositivelyCorrelated.Add(result);
                i++;
                if (i > numPositives)
                    break;
            }
            i = 0;
            int numNegatives = Math.Min(4, resultList.Count() - numPositives);
            foreach (ListItemStats<double, Activity> result in resultList)
            {
                mostNegativelyCorrelated.Add(result);
                i++;
                if (i > numNegatives)
                    break;
            }

            if (resultList.Count() <= 0)
            {
                // This shouldn't be able to happen unless we disallow predicting the activity from itself
                this.SubLayout = new TextblockLayout("No activities found!");
            }
            else
            {
                string title = "Things you do that are correlated with doing more or less of " + activityToPredict.Name + " over the following " +
                    Math.Round(windowSize.TotalDays, 0) + " days";
                layoutBuilder.AddLayout(new TextblockLayout(title));

                if (numPositives > 0)
                {
                    if (numPositives > 1)
                        layoutBuilder.AddLayout(new TextblockLayout("Doing one minute of these activities adds this many minutes:"));
                    else
                        layoutBuilder.AddLayout(new TextblockLayout("Doing one minute of this activity adds this many minutes:"));
                    foreach (ListItemStats<double, Activity> result in mostPositivelyCorrelated)
                    {
                        double correlation = result.Key;
                        Activity activity = result.Value;
                        String message = activity.Name + ": " + Math.Round(correlation, 5);
                        layoutBuilder.AddLayout(new TextblockLayout(message));
                    }
                }

                if (numNegatives > 0)
                {
                    if (numNegatives > 1)
                        layoutBuilder.AddLayout(new TextblockLayout("Doing one minute of these activities subtracts this many minutes:"));
                    else
                        layoutBuilder.AddLayout(new TextblockLayout("Doing one minute of this activity subtracts this many minutes:"));
                    foreach (ListItemStats<double, Activity> result in mostNegativelyCorrelated)
                    {
                        double correlation = result.Key;
                        Activity activity = result.Value;
                        String message = activity.Name + ": " + Math.Round(correlation, 5);
                        layoutBuilder.AddLayout(new TextblockLayout(message));
                    }
                }

                this.SubLayout = layoutBuilder.Build();
            }
        }
    }
}
