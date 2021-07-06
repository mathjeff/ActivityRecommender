using StatLists;
using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;

namespace ActivityRecommendation.View
{
    class NumberOfActivities_Graph : ContainerLayout
    {
        public NumberOfActivities_Graph(ActivityDatabase activityDatabase)
        {
            this.activityDatabase = activityDatabase;
            this.SubLayout = this.plot();
        }

        private LayoutChoice_Set plot()
        {
            // sort activities by their discovery date
            // make a plot
            PlotView plotView = new PlotView();
            List<Datapoint> activitiesDatapoints = this.getDatapoints(this.activityDatabase.AllActivities);
            plotView.AddSeries(activitiesDatapoints, false);
            List<Datapoint> categoriesDatapoints = this.getDatapoints(this.activityDatabase.AllCategories);
            plotView.AddSeries(categoriesDatapoints, false);
            // add tick marks for years
            if (activitiesDatapoints.Count > 0)
                plotView.XAxisSubdivisions = TimeProgression.AbsoluteTime.GetNaturalSubdivisions(activitiesDatapoints[0].Input, activitiesDatapoints[activitiesDatapoints.Count - 1].Input);

            // add description
            string todayText = DateTime.Now.ToString("yyyy-MM-dd");
            LayoutChoice_Set result = new Vertical_GridLayout_Builder()
                .AddLayout(new TextblockLayout("Number of activities over time"))
                .AddLayout(new ImageLayout(plotView, LayoutScore.Get_UsedSpace_LayoutScore(1)))
                .AddLayout(new TextblockLayout("You have " + activitiesDatapoints.Count + " activities, " + categoriesDatapoints.Count + " of which are categories. Today is " + todayText))
                .BuildAnyLayout();
            return result;
        }

        private List<Datapoint> getDatapoints(IEnumerable<Activity> activities)
        {
            StatList<DateTime, int> discoveredCounts = new StatList<DateTime, int>(new DateComparer(), new IntAdder());
            foreach (Activity activity in activities)
            {
                discoveredCounts.Add(activity.DiscoveryDate, 1);
            }
            DateTime minPossibleDate = TimeProgression.AbsoluteTime.StartDate;
            bool isFirst = false;
            List<Datapoint> datapoints = new List<Datapoint>();
            // compute cumulative values
            foreach (DateTime when in discoveredCounts.Keys)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                TimeSpan duration = when.Subtract(minPossibleDate);
                int cumulative = discoveredCounts.CombineBeforeKey(when, true);
                datapoints.Add(new Datapoint(duration.TotalSeconds, cumulative, 1));
            }
            return datapoints;
        }

        private ActivityDatabase activityDatabase;
    }
}
