using AdaptiveLinearInterpolation;
using StatLists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisiPlacement;

// A Participation_BinComparison_View shows the activities that generally precede another activity
namespace ActivityRecommendation.View
{
    class Participation_BinComparison_View : ContainerLayout
    {
        public Participation_BinComparison_View(Engine engine, IEnumerable<Activity> activitiesToPredictFrom, Activity activityToPredict, TimeSpan windowSize)
        {
            DateTime now = DateTime.Now;
            engine.EnsureRatingsAreAssociated();

            activityToPredict.ApplyPendingData();
            AutoSmoothed_ParticipationProgression participationProgression = activityToPredict.ParticipationProgression;
            LinearProgression progressionToPredict = participationProgression.Smoothed(windowSize);

            StatList<NeighborhoodInterpolation, Activity> results = new StatList<NeighborhoodInterpolation, Activity>(new Neighborhood_MiddleOutputMean_Comparer(), new NoopCombiner<Activity>());
            foreach (Activity activity in activitiesToPredictFrom)
            {
                System.Diagnostics.Debug.WriteLine("comparing " + activity + " and " + activityToPredict.Name);
                List<Datapoint> datapoints = activity.compareParticipations(windowSize, progressionToPredict, now.Subtract(windowSize));

                // put data into the interpolator
                FloatRange inputRange = new FloatRange(0, true, windowSize.TotalSeconds, true);
                AdaptiveLinearInterpolator<Distribution> interpolator = new AdaptiveLinearInterpolator<Distribution>(new HyperBox<Distribution>(new FloatRange[] { inputRange }), new DistributionAdder());
                foreach (Datapoint datapoint in datapoints)
                {
                    interpolator.AddDatapoint(new AdaptiveLinearInterpolation.Datapoint<Distribution>(datapoint.Input, Distribution.MakeDistribution(datapoint.Output, 0, datapoint.Weight)));
                }

                // ask the interpolator which input has the highest average output
                IEnumerable<double[]> representativePoints = interpolator.FindRepresentativePoints();
                if (representativePoints.Count() > 0)
                {
                    double[] bestInput = new double[1];
                    AdaptiveLinearInterpolation.Distribution bestOutput = null;
                    foreach (double[] coordinates in representativePoints)
                    {
                        AdaptiveLinearInterpolation.Distribution output = interpolator.Interpolate(coordinates);
                        if (bestOutput == null || output.Mean > bestOutput.Mean)
                        {
                            bestInput = coordinates;
                            bestOutput = output;
                        }
                    }

                    // Check nearby regions for their outputs too
                    HyperBox<Distribution> inputNeighborhood = interpolator.FindNeighborhoodCoordinates(bestInput);
                    double inputNeighborhoodWidth = inputNeighborhood.Coordinates[0].Width;
                    double lowerInput = Math.Max(inputNeighborhood.Coordinates[0].LowCoordinate - inputNeighborhoodWidth, inputRange.LowCoordinate);
                    double higherInput = Math.Min(inputNeighborhood.Coordinates[0].HighCoordinate + inputNeighborhoodWidth, inputRange.HighCoordinate);

                    NeighborhoodInterpolation result = new NeighborhoodInterpolation();
                    result.Middle = new Interpolation(bestInput[0], bestOutput);
                    result.Left = new Interpolation(lowerInput, interpolator.Interpolate(new double[] { lowerInput }));
                    result.Right = new Interpolation(higherInput, interpolator.Interpolate(new double[] { higherInput }));

                    results.Add(result, activity);
                }
            }

            IEnumerable<ListItemStats<NeighborhoodInterpolation, Activity>> resultList = results.AllItems;

            if (resultList.Count() <= 0)
            {
                // This shouldn't be able to happen unless we disallow predicting the activity from itself
                this.SubLayout = new TextblockLayout("No activities found!");
            }
            else
            {
                int numWindowDays = (int)windowSize.TotalDays;
                Vertical_GridLayout_Builder layoutBuilder = new Vertical_GridLayout_Builder().Uniform();
                if (resultList.Count() > 1)
                {
                    string title = "Activities that when done most strongly predict future participations in " + activityToPredict.Name;
                    layoutBuilder.AddLayout(new TextblockLayout(title));
                }
                else
                {
                    string title = "Predicting future participations in " + activityToPredict.Name;
                    layoutBuilder.AddLayout(new TextblockLayout(title));
                }

                string explanation = "A block of the form a->{b,c,d} means that when you have spent an average of <a> min/day on the given activity over the last " + numWindowDays + " days, " +
                    "you have spent on average <c> min/day on " + activityToPredict.Name + " over the next " + numWindowDays + " days, with <b> and <d> marking -1 and +1 standard deviation, respectively";
                layoutBuilder.AddLayout(new TextblockLayout(explanation));

                int count = 0;
                foreach (ListItemStats<NeighborhoodInterpolation, Activity> result in resultList.Reverse())
                {
                    count++;
                    if (count > 3)
                        break;
                    Activity activity = result.Value;
                    NeighborhoodInterpolation neighborhood = result.Key;
                    layoutBuilder.AddLayout(new TextblockLayout("After " + activity.Name + ":"));
                    foreach (Interpolation interpolation in neighborhood.Items)
                    {
                        double inputSecondsPerWindow = interpolation.Input;
                        double inputMinutesPerDay = inputSecondsPerWindow / windowSize.TotalDays / 60;

                        double avgOutputSecondsPerWindow = interpolation.Output.Mean;
                        double avgOutputMinutesPerDay = avgOutputSecondsPerWindow / windowSize.TotalDays / 60;

                        double stddevOutputSecondsPerWindow = interpolation.Output.StdDev;
                        double stddevOutputMinutesPerDay = stddevOutputSecondsPerWindow / windowSize.TotalDays / 60;

                        string itemLine = Math.Round(inputMinutesPerDay, 1) + " -> {" +
                            Math.Round((avgOutputMinutesPerDay - stddevOutputMinutesPerDay), 1) + "," +
                            Math.Round(avgOutputMinutesPerDay, 1) + "," +
                            Math.Round((avgOutputMinutesPerDay + stddevOutputMinutesPerDay), 1) + "}";
                        layoutBuilder.AddLayout(new TextblockLayout(itemLine));
                    }
                }

                this.SubLayout = layoutBuilder.Build();
            }

        }
    }

    class Interpolation
    {
        public Interpolation(double input, AdaptiveLinearInterpolation.Distribution output)
        {
            this.Input = input;
            this.Output = output;
        }
        public double Input;
        public AdaptiveLinearInterpolation.Distribution Output;
    
    }

    class NeighborhoodInterpolation
    {
        public Interpolation Left;
        public Interpolation Middle;
        public Interpolation Right;
        public IEnumerable<Interpolation> Items
        {
            get
            {
                return new List<Interpolation> { this.Left, this.Middle, this.Right };
            }
        }
    }

    class Neighborhood_MiddleOutputMean_Comparer : IComparer<NeighborhoodInterpolation>
    {
        public int Compare(NeighborhoodInterpolation a, NeighborhoodInterpolation b)
        {
            return a.Middle.Output.Mean.CompareTo(b.Middle.Output.Mean);
        }
    }
}
