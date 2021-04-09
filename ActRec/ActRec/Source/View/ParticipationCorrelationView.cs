﻿using StatLists;
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

            LinearProgression progressionToPredict = activityToPredict.ParticipationsSmoothed(windowSize);

            StatList<double, Activity> results = new StatList<double, Activity>(new DoubleComparer(), new NoopCombiner<Activity>());

            foreach (Activity activity in activitiesToPredictFrom)
            {
                System.Diagnostics.Debug.WriteLine("comparing " + activity + " and " + activityToPredict.Name);
                List<Datapoint> datapoints = activity.compareParticipations(TimeSpan.FromSeconds(1), progressionToPredict, now.Subtract(windowSize));

                // now compute the value of the formula
                Correlator correlator = new Correlator();
                foreach (Datapoint datapoint in datapoints)
                {
                    correlator.Add(datapoint);
                }

                double bonusSecondsPerWindow = correlator.Slope;
                if (!double.IsNaN(bonusSecondsPerWindow))
                {
                    double bonusSecondsPerDay = bonusSecondsPerWindow / windowSize.TotalDays;
                    results.Add(bonusSecondsPerDay / 60, activity);
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
                if (numPositives > 0 && numNegatives > 0)
                {
                    string title = "Activities whose participations are strongly correlated with participations in " + activityToPredict.Name + " over the next " +
                        Math.Round(windowSize.TotalDays, 0) + " days";
                    layoutBuilder.AddLayout(new TextblockLayout(title));
                }

                if (numPositives > 0)
                {
                    if (numPositives > 1)
                        layoutBuilder.AddLayout(new TextblockLayout("These activities add to the number of minutes spent per day:"));
                    else
                        layoutBuilder.AddLayout(new TextblockLayout("This activity adds to the number of minutes spent per day:"));
                    foreach (ListItemStats<double, Activity> result in mostPositivelyCorrelated)
                    {
                        double correlation = result.Key;
                        Activity activity = result.Value;
                        String message = activity.ToString() + ": " + Math.Round(correlation, 5);
                        layoutBuilder.AddLayout(new TextblockLayout(message));
                    }
                }

                if (numNegatives > 0)
                {
                    if (numNegatives > 1)
                        layoutBuilder.AddLayout(new TextblockLayout("These activities subtract from the number of minutes spent per day:"));
                    else
                        layoutBuilder.AddLayout(new TextblockLayout("This activity subtracts from the number of minutes spent per day:"));
                    foreach (ListItemStats<double, Activity> result in mostNegativelyCorrelated)
                    {
                        double correlation = result.Key;
                        Activity activity = result.Value;
                        String message = activity.ToString() + ": " + Math.Round(correlation, 5);
                        layoutBuilder.AddLayout(new TextblockLayout(message));
                    }
                }

                this.SubLayout = layoutBuilder.Build();
            }
        }
    }
}
