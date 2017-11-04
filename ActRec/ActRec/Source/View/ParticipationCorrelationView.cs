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
        public ParticipationCorrelationView(Engine engine, ActivityDatabase activityDatabase, ActivityDescriptor activityDescriptor, TimeSpan windowSize)
        {

            engine.EnsureRatingsAreAssociated();

            // For the given activity, make a Progression telling the total time spent on that activity in the next <windowSize>
            // For each other activity, do a linear regression between its current intensity (0 or 1) and the other activity's upcoming avg intensity (during the given window)
            Activity activityToPredict = activityDatabase.ResolveDescriptor(activityDescriptor);
            activityToPredict.ApplyPendingData();
            AutoSmoothed_ParticipationProgression participationProgression = activityToPredict.ParticipationProgression;
            DateTime when = DateTime.Now;
            LinearProgression progressionToPredict = participationProgression.Smoothed(windowSize, when);

            StatList<double, Activity> results = new StatList<double, Activity>(new DoubleComparer(), new NoopCombiner<Activity>());
            foreach (Activity activity in activityDatabase.AllActivities)
            {
                System.Diagnostics.Debug.WriteLine("correlating " + activity + " and " + activityToPredict.Name);
                activity.ApplyPendingData();
                // smoothing with a short duration is a hacky way of getting a LinearProgression that models the instantaneous rate of participation
                // ideally we'll add support directly into the LinearProgression class itself
                LinearProgression predictor = activity.ParticipationProgression.Smoothed(TimeSpan.FromSeconds(1), when);

                // even if activity == activityToPredict, do the prediction anyway, because it's still meaningful to find that past participations in an activity predict future participations
                StatList<DateTime, bool> union = new StatList<DateTime, bool>(new DateComparer(), new NoopCombiner<bool>());




                // find all the keys that either one contains
                foreach (DateTime date in progressionToPredict.Keys)
                {
                    union.Add(date, true);
                }
                foreach (DateTime date in predictor.Keys)
                {
                    union.Add(date, true);
                }
                union.Add(when, true);

                // now compute the value of the formula
                Correlator correlator = new Correlator();
                DateTime prevDate = union.GetFirstValue().Key;
                foreach (ListItemStats<DateTime, bool> item in union.AllItems)
                {
                    DateTime nextDate = item.Key;
                    if (nextDate.CompareTo(prevDate) <= 0)
                    {
                        // skip duplicates
                        continue;
                    }
                    double weight = nextDate.Subtract(prevDate).TotalSeconds;
                    double x1 = predictor.GetValueAt(prevDate, false).Value.Mean;
                    double y1 = progressionToPredict.GetValueAt(prevDate, false).Value.Mean;
                    correlator.Add(x1, y1, weight);
                    double x2 = predictor.GetValueAt(nextDate, false).Value.Mean;
                    double y2 = progressionToPredict.GetValueAt(nextDate, false).Value.Mean;
                    correlator.Add(x2, y2, weight);
                }
                double correlation = correlator.Correlation;
                if (!double.IsNaN(correlation))
                {
                    results.Add(correlation, activity);
                }
            }

            IEnumerable<ListItemStats<double, Activity>> resultList = results.AllItems;

            Vertical_GridLayout_Builder layoutBuilder = new Vertical_GridLayout_Builder();
            LinkedList<ListItemStats<double, Activity>> itemsToShow = new LinkedList<ListItemStats<double, Activity>>();
            int i = 0;
            foreach (ListItemStats<double, Activity> result in resultList.Reverse())
            {
                itemsToShow.AddLast(result);
                i++;
                if (i > 4)
                    break;
            }
            i = 0;
            foreach (ListItemStats<double, Activity> result in resultList)
            {
                itemsToShow.AddLast(result);
                i++;
                if (i > 4)
                    break;
            }

            if (itemsToShow.Count <= 0)
            {
                // This shouldn't be able to happen unless we disallow predicting the activity from itself
                this.SubLayout = new TextblockLayout("No activities found!");
            }
            else
            {
                layoutBuilder.AddLayout(new TextblockLayout("Activities whose participations predict future participations in " + activityToPredict.Name));
                foreach (ListItemStats<double, Activity> result in itemsToShow)
                {
                    double correlation = result.Key;
                    Activity activity = result.Value;
                    String message = activity.ToString() + ": " + correlation.ToString();
                    layoutBuilder.AddLayout(new TextblockLayout(message));
                }

                this.SubLayout = layoutBuilder.Build();
            }
        }
    }
}
