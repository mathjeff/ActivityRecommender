﻿#if true // this will be really cool when it works
using StatLists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using VisiPlacement;

// The ActivityVisualizationView shows a visual representation of the Participations and Ratings for an Activity
namespace ActivityRecommendation
{
    class ActivityVisualizationView : TitledControl
    {
        public ActivityVisualizationView(IProgression participationXAxis, Activity yAxisActivity, TimeSpan smoothingWindow, RatingSummarizer summarizer)
        {
            if (summarizer != null)
            {
                this.ratingSummarizer = summarizer;
            }
            else
            {
                this.ratingSummarizer = new ExponentialRatingSummarizer(smoothingWindow);
                foreach (AbsoluteRating rating in yAxisActivity.RatingProgression.GetRatingsInDiscoveryOrder())
                {
                    Participation ratingSource = null;
                    if (rating.Source != null)
                        ratingSource = rating.Source.ConvertedAsParticipation;
                    if (ratingSource != null)
                    {
                        this.ratingSummarizer.AddRating(ratingSource.StartDate, ratingSource.EndDate, rating.Score);
                        // TODO: figure out what to do here about skips
                        this.ratingSummarizer.AddParticipationIntensity(ratingSource.StartDate, ratingSource.EndDate, 1);
                    }
                }
            }

            this.xAxisProgression = participationXAxis;
            //this.xAxisActivity = null;
            this.yAxisActivity = yAxisActivity;
            this.smoothingDuration = smoothingWindow;

            // setup the title
            this.SetTitle(this.YAxisLabel + " vs. " + this.XAxisLabel);
            //this.KeyDown += new System.Windows.Input.KeyEventHandler(ActivityVisualizationView_KeyDown);

            // explicitly call out an even split as a possibility, to improve the runtime performance of finding the best the layout
            //GridLayout flexibleGrid = GridLayout.New(new BoundProperty_List(1), new BoundProperty_List(2), LayoutScore.Zero);
            GridLayout fixedGrid = GridLayout.New(new BoundProperty_List(1), BoundProperty_List.Uniform(2), LayoutScore.Zero);
            //this.SetContent(new LayoutUnion(fixedGrid, flexibleGrid));
            this.SetContent(fixedGrid);

            GridLayout graphGrid = GridLayout.New(BoundProperty_List.Uniform(2), new BoundProperty_List(1), LayoutScore.Zero);
            // setup a graph of the ratings
            this.ratingsView = new TitledControl("This is a graph of the ratings of " + this.YAxisLabel + " vs. Time");
            graphGrid.AddLayout(this.ratingsView);

            // setup a graph of the participations
            this.participationsView = new TitledControl("This is a graph of the participations in " + this.YAxisLabel + " vs. " + this.XAxisLabel);
            graphGrid.AddLayout(this.participationsView);

            //flexibleGrid.AddLayout(graphGrid);
            fixedGrid.AddLayout(graphGrid);

            // put the stats view into the main view
            //flexibleGrid.AddLayout(this.Make_StatsView());
            fixedGrid.AddLayout(this.Make_StatsView());
            //this.Make_StatsView();
            //fixedGrid.AddLayout(new TextblockLayout("stats here"));


            this.UpdateParticipationStatsView();
            this.UpdateDrawing();

            //CalculateExtraStats();
        }
        private LayoutChoice_Set Make_StatsView()
        {
            // many of the entries in this function are disabled because they're probably not that useful but might be worth re-adding somehow

            // setup a display for some statistics
            Vertical_GridLayout_Builder builder = new Vertical_GridLayout_Builder().Uniform();

#if false
            // setup an exit button
            this.exitButton = new Button();
            builder.AddLayout(new ButtonLayout(this.exitButton, "Escape"));
#endif


            // display an editable date range
            this.participationDataDisplay = new TitledControl("Stats:");
            this.queryStartDateDisplay = new DateEntryView("Between");
            this.queryStartDateDisplay.SetDate(this.yAxisActivity.GetEarliestInteractionDate());
            this.queryStartDateDisplay.Add_TextChanged_Handler(new TextChangedEventHandler(this.DateTextChanged));
            builder.AddLayout(this.queryStartDateDisplay);
            this.queryEndDateDisplay = new DateEntryView("and");
            this.queryEndDateDisplay.SetDate(DateTime.Now);
            this.queryEndDateDisplay.Add_TextChanged_Handler(new TextChangedEventHandler(this.DateTextChanged));
            builder.AddLayout(this.queryEndDateDisplay);

            // display the total time spanned by the current window
            this.totalTimeDisplay = new TextBlock();
            this.totalTimeDisplay.TextAlignment = TextAlignment.Center;
            builder.AddLayout(new TextblockLayout(this.totalTimeDisplay));
            this.timeFractionDisplay = new TextBlock();
            this.timeFractionDisplay.TextAlignment = TextAlignment.Center;
            builder.AddLayout(new TextblockLayout(this.timeFractionDisplay));

#if false
            this.thinkingTime_Display = new TitledTextblock("You often spend");
            this.thinkingTime_Display.Text = this.yAxisActivity.ThinkingTimes.Mean.ToString() + " seconds considering this activity";
            builder.AddLayout(this.thinkingTime_Display);
#endif

#if false
            this.mostPopularChild_View = new TitledTextblock("Your most common immediate subactivity:");
            this.mostPopularChild_View.Text = "[There are none]";
            builder.AddLayout(this.mostPopularChild_View);
#endif

#if false
            this.mostPopularDescendent_View = new TitledTextblock("Your most common subactivity,\n among those having no subactivities:");
            this.mostPopularDescendent_View.Text = "[There are none]";
            builder.AddLayout(this.mostPopularDescendent_View);
#endif

            // display rating statistics
            this.ratingWhenSuggested_Display = new TitledTextblock("Mean rating when suggested:");
            this.ratingWhenSuggested_Display.Text = this.yAxisActivity.ScoresWhenSuggested.Mean.ToString();
            builder.AddLayout(this.ratingWhenSuggested_Display);

            this.ratingWhenNotSuggested_Display = new TitledTextblock("Mean rating when not suggested:");
            this.ratingWhenNotSuggested_Display.Text = this.yAxisActivity.ScoresWhenNotSuggested.Mean.ToString();
            builder.AddLayout(this.ratingWhenNotSuggested_Display);
            
            return builder.Build();
        }


        public void CalculateExtraStats()
        {
            // Sunday = index 0, Saturday = index 6
            TimeSpan[] timePerDay = new TimeSpan[7];
            foreach (Participation participation in this.yAxisActivity.ParticipationProgression.Participations)
            {
                DateTime start = participation.StartDate;
                int index = (int)start.DayOfWeek;
                TimeSpan duration = participation.Duration;
                timePerDay[index] += duration;
            }
            return;
        }
        public void UpdateDrawing()
        {
            this.UpdateRatingsPlot();
            this.UpdateParticipationsPlot();
        }
        public void UpdateRatingsPlot()
        {
            DateTime start = DateTime.Now;
            if (!this.queryStartDateDisplay.IsDateValid() || !this.queryEndDateDisplay.IsDateValid())
                return;
            // draw the RatingProgression
            RatingProgression ratingProgression = this.yAxisActivity.RatingProgression;
            List<AbsoluteRating> ratings = ratingProgression.GetRatingsInDiscoveryOrder();
            DateTime startDate = this.queryStartDateDisplay.GetDate();
            DateTime endDate = this.queryEndDateDisplay.GetDate();
            List<Datapoint> points = new List<Datapoint>();

            // make a plot
            PlotView newPlot = new PlotView();
            newPlot.ShowRegressionLine = true;
            newPlot.MinX = 0;
            newPlot.MaxX = endDate.Subtract(startDate).TotalDays;
            newPlot.MinY = 0;
            newPlot.MaxY = 1;

            // compute the rating at each relevant date
            foreach (AbsoluteRating rating in ratings)
            {
                if (rating.Date != null)
                {
                    TimeSpan duration = ((DateTime)rating.Date).Subtract(startDate);
                    // calculate x
                    double x = duration.TotalDays;
                    // calculate y
                    double y = rating.Score;
                    // make sure we want to include it in the plot (and therefore the regression line)
                    if (x >= newPlot.MinX && x <= newPlot.MaxX)
                        points.Add(new Datapoint(x, y, 1));
                }
            }

            // compute a smoothed version of the ratings so we can show which activities are actually worth doing
            // (for example, sleeping might feel boring but might increase later happiness)
            TimeSpan totalDuration = endDate.Subtract(startDate);
            double i;
            RatingSummary ratingSummary = new RatingSummary(endDate);
            List<Datapoint> smoothedPoints = new List<Datapoint>();
            for (i = 1; i >= 0; i -= 0.001)
            {
                TimeSpan currentDuration = new TimeSpan((long)((double)totalDuration.Ticks * (double)i));
                DateTime when = startDate.Add(currentDuration);
                ratingSummary.Update(this.ratingSummarizer, when, endDate);
                double x = currentDuration.TotalDays;
                double y = ratingSummary.Item.Mean;
                if (!double.IsNaN(y))
                    smoothedPoints.Add(new Datapoint(x, y, 1));
            }
            List<List<Datapoint>> allSequences = new List<List<Datapoint>>();
            allSequences.Add(points);
            allSequences.Add(smoothedPoints);
            newPlot.SetData(allSequences);

            DateTime end = DateTime.Now;

            this.ratingsView.SetContent(new ImageLayout(newPlot, LayoutScore.Get_UsedSpace_LayoutScore(1)));

            System.Diagnostics.Debug.WriteLine("spent " + end.Subtract(start) + " to update ratings plot");

        }
        public void UpdateParticipationsPlot()
        {
            DateTime start = DateTime.Now;

            if (!this.queryStartDateDisplay.IsDateValid() || !this.queryEndDateDisplay.IsDateValid())
                return;
            // draw the ParticipationProgression
            AutoSmoothed_ParticipationProgression participationProgression = this.yAxisActivity.ParticipationProgression;
            IEnumerable<Participation> participations = participationProgression.Participations;
            DateTime firstDate = this.queryStartDateDisplay.GetDate();
            DateTime lastDate = this.queryEndDateDisplay.GetDate();
            List<Datapoint> cumulativeParticipationDurations = new List<Datapoint>();
            List<Datapoint> cumulativeSuggestionCounts = new List<Datapoint>();
            List<double> suggestionDates = new List<double>();

            PlotView newPlot = new PlotView();
            newPlot.ShowRegressionLine = true;

            
            newPlot.MinX = 0;
            newPlot.MaxX = 1;

            double x1, x2, cumulativeParticipationDuration, cumulativeSuggestionCount;
            x1 = 0;

            if (this.xAxisProgression != null)
            {
                AdaptiveLinearInterpolation.FloatRange inputRange = this.xAxisProgression.EstimateOutputRange();
                if (inputRange == null)
                    inputRange = new AdaptiveLinearInterpolation.FloatRange(this.xAxisProgression.GetValueAt(firstDate, false).Value.Mean, true, this.xAxisProgression.GetValueAt(lastDate, false).Value.Mean, true);
                newPlot.XAxisSubdivisions = this.xAxisProgression.GetNaturalSubdivisions(inputRange.LowCoordinate, inputRange.HighCoordinate);
                newPlot.MinX = x1 = inputRange.LowCoordinate;
                newPlot.MaxX = inputRange.HighCoordinate;
            }


            double maxXPlotted = 0;
            List<double> startXs = new List<double>();
            List<double> endXs = new List<double>();

            // figure out which dates we care about
            double startX, endX;
            int numActiveIntervals = 0;
            DateTime startDate, endDate;
            
            foreach (Participation participation in participations)
            {
                // update some data about cumulative participation duration
                startDate = participation.StartDate;
                endDate = participation.EndDate;
                // make sure this participation is relevant
                if (startDate.CompareTo(firstDate) >= 0 && endDate.CompareTo(lastDate) <= 0)
                {
                    startX = this.GetParticipationXCoordinate(participation.StartDate);
                    endX = this.GetParticipationXCoordinate(participation.EndDate);
                    startXs.Add(startX);
                    endXs.Add(endX);
                    if (startX > endX)
                        numActiveIntervals++;
                }

                // update some data about cumulative num suggestions
                if (participation.Suggested.GetValueOrDefault(false))
                {
                    // this is slightly hacky - really there should be a method that just returns a list of every Suggestion for an Activity
                    suggestionDates.Add(this.GetParticipationXCoordinate(participation.StartDate));
                }
            }
            startXs.Sort();
            endXs.Sort();
            cumulativeParticipationDuration = 0;
            while (endXs.Count > 0 || startXs.Count > 0)
            {
                if (startXs.Count > 0)
                {
                    if (endXs.Count > 0)
                    {
                        startX = startXs[0];
                        endX = endXs[0];
                    }
                    else
                    {
                        startX = startXs[0];
                        endX = startX + 1;  // something larger that will be ignored
                    }
                }
                else
                {
                    endX = endXs[0];
                    startX = endX + 1;      // something larger that will be ignored
                }
                if (startX < endX)
                {
                    x2 = startX;
                    double weight = x2 - x1;
                    // add a datapoint denoting the start of the interval
                    cumulativeParticipationDurations.Add(new Datapoint(x1, cumulativeParticipationDuration, weight));
                    cumulativeParticipationDuration += weight * numActiveIntervals;
                    numActiveIntervals++;
                    // add a datapoint denoting the end of the interval
                    cumulativeParticipationDurations.Add(new Datapoint(x2, cumulativeParticipationDuration, weight));
                    startXs.RemoveAt(0);
                }
                else
                {
                    x2 = endX;
                    double weight = x2 - x1;
                    // add a datapoint denoting the start of the interval
                    cumulativeParticipationDurations.Add(new Datapoint(x1, cumulativeParticipationDuration, weight));
                    cumulativeParticipationDuration += weight * numActiveIntervals;
                    numActiveIntervals--;
                    // add a datapoint denoting the end of the interval
                    cumulativeParticipationDurations.Add(new Datapoint(x2, cumulativeParticipationDuration, weight));
                    endXs.RemoveAt(0);
                }
                maxXPlotted = x2;
                x1 = x2;
            }

            // rescale cumulativeParticipationDurations to total 1
            if (cumulativeParticipationDuration != 0)
            {
                foreach (Datapoint item in cumulativeParticipationDurations)
                {
                    item.Output = item.Output / cumulativeParticipationDuration;
                }
            }


            // We also want to plot the number of times that the activity was suggested
            cumulativeSuggestionCount = 0;
            foreach (ListItemStats<DateTime, WillingnessSummary> item in this.yAxisActivity.ConsiderationProgression.AllItems)
            {
                double x = this.GetParticipationXCoordinate(item.Key);
                if (item.Value.NumSkips > 0)
                {
                    // found a skip
                    suggestionDates.Add(x);
                }
            }
            suggestionDates.Sort();
            foreach (double x in suggestionDates)
            {
                cumulativeSuggestionCounts.Add(new Datapoint(x, cumulativeSuggestionCount, 1));
                cumulativeSuggestionCount += 1;
                cumulativeSuggestionCounts.Add(new Datapoint(x, cumulativeSuggestionCount, 1));
            }

            // rescale cumulativeSuggestionCounts to total 1
            if (cumulativeSuggestionCount != 0)
            {
                foreach (Datapoint item in cumulativeSuggestionCounts)
                {
                    item.Output /= cumulativeSuggestionCount;
                }
            }

            List<List<Datapoint>> plots = new List<List<Datapoint>>();
            plots.Add(cumulativeParticipationDurations);
            plots.Add(cumulativeSuggestionCounts);
            newPlot.SetData(plots);


            this.participationsView.SetContent(new ImageLayout(newPlot, LayoutScore.Get_UsedSpace_LayoutScore(1)));

            DateTime end = DateTime.Now;

            System.Diagnostics.Debug.WriteLine("spent " + end.Subtract(start) + " to update partipations plot");

        }
        private void DateTextChanged(object sender, RoutedEventArgs e)
        {
            this.UpdateParticipationStatsView();
            this.UpdateDrawing();
        }
        public void UpdateParticipationStatsView()
        {
            // make sure that the dates can be parsed before we update the display
            if (!this.queryStartDateDisplay.IsDateValid() || !this.queryEndDateDisplay.IsDateValid())
                return;
            // figure out how much time you spent on it between these dates
            DateTime startDate = this.queryStartDateDisplay.GetDate();
            DateTime endDate = this.queryEndDateDisplay.GetDate();
            Participation summary = this.yAxisActivity.SummarizeParticipationsBetween(startDate, endDate);
            double numHoursSpent = summary.TotalIntensity.Weight / 3600;
            // figure out how much time there was between these dates
            TimeSpan availableDuration = endDate.Subtract(startDate);
            double totalNumHours = availableDuration.TotalHours;
            double participationFraction = numHoursSpent / totalNumHours;

            // now update the text blocks
            //this.availableTimeDisplay.Text = "Have known about this activity for " + Environment.NewLine + availableDuration.TotalDays + " days";
            this.totalTimeDisplay.Text = "You've spent " + Environment.NewLine + numHoursSpent + " hours on " + this.YAxisLabel;
            //this.timeFractionDisplay.Text = "Or " + Environment.NewLine + 100 * participationFraction + "% of your total time" + Environment.NewLine + " Or " + (participationFraction * 24 * 60).ToString() + " minutes per day";
            this.timeFractionDisplay.Text = "Or " + Environment.NewLine + (participationFraction * 24 * 60).ToString() + " minutes per day";
            Activity bestChild = null;
            Distribution bestTotal = new Distribution();
            foreach (Activity child in this.yAxisActivity.Children)
            {
                Participation participation = child.SummarizeParticipationsBetween(startDate, endDate);
                Distribution currentTotal = participation.TotalIntensity;
                if (currentTotal.Weight > bestTotal.Weight)
                {
                    bestChild = child;
                    bestTotal = currentTotal;
                }
            }
            if (this.mostPopularChild_View != null)
            {
                if (bestChild != null)
                    this.mostPopularChild_View.Text = bestChild.Name;

                bestChild = null;
                bestTotal = new Distribution();
                foreach (Activity child in this.yAxisActivity.GetAllSubactivities())
                {
                    if (child.Children.Count == 0)
                    {
                        Participation participation = child.SummarizeParticipationsBetween(startDate, endDate);
                        Distribution currentTotal = participation.TotalIntensity;
                        if (currentTotal.Weight > bestTotal.Weight)
                        {
                            bestChild = child;
                            bestTotal = currentTotal;
                        }
                    }
                }
                if (bestChild != null)
                    this.mostPopularDescendent_View.Text = bestChild.Name;
            }
        }
        public void AddExitClickHandler(RoutedEventHandler e)
        {
            this.exitButton.Click += e;
            this.exitHandler = e;
        }
        void ActivityVisualizationView_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            this.exitHandler.Invoke(sender, e);
        }

        public string XAxisLabel
        {
            get
            {
                /*
                if (this.xAxisActivity != null)
                    return this.xAxisActivity.Name;
                else
                    return "Time";
                */
                if (this.xAxisProgression != null)
                    return this.xAxisProgression.Description;
                else
                    return "Time";
            }
        }
        public string YAxisLabel
        {
            get
            {
                return this.yAxisActivity.Name;
            }
        }

        private double GetParticipationXCoordinate(DateTime when)
        {
            double x;
            DateTime startDate = this.queryStartDateDisplay.GetDate();
            if (this.xAxisProgression == null)
            {
                DateTime endDate = this.queryEndDateDisplay.GetDate();
                TimeSpan totalDuration = endDate.Subtract(startDate);
                TimeSpan duration = when.Subtract(startDate);
                x = duration.TotalSeconds / totalDuration.TotalSeconds;
            }
            else
            {
                ProgressionValue value = this.xAxisProgression.GetValueAt(when, true);
                if (value != null)
                    x = value.Value.Mean;
                else
                    x = 0;
                //Participation xParticipation = this.xAxisActivity.SummarizeParticipationsBetween(startDate, when);
                //x = xParticipation.TotalIntensity.Mean * xParticipation.TotalIntensity.Weight;
            }
            return x;
        }
        private double GetMin_ParticipationXCoordinate(DateTime firstDate, DateTime lastDate)
        {
            AdaptiveLinearInterpolation.FloatRange inputRange = this.xAxisProgression.EstimateOutputRange();
            if (inputRange == null)
                inputRange = new AdaptiveLinearInterpolation.FloatRange(this.xAxisProgression.GetValueAt(firstDate, false).Value.Mean, true, this.xAxisProgression.GetValueAt(lastDate, false).Value.Mean, true);
            return inputRange.LowCoordinate;
        }
        private double GetParticipationYCoordinate(DateTime when)
        {
            DateTime startDate = this.queryStartDateDisplay.GetDate();
            Participation yParticipation = this.yAxisActivity.SummarizeParticipationsBetween(startDate, when);
            double y = yParticipation.TotalIntensity.Mean * yParticipation.TotalIntensity.Weight;

            return y;
        }

        Button exitButton;
        RoutedEventHandler exitHandler;
        Activity yAxisActivity;
        IProgression xAxisProgression;
        //Activity xAxisActivity;
        //PlotControl ratingsPlot;
        //PlotControl participationsPlot;
        TitledControl ratingsView;
        TitledControl participationsView;
        TitledTextblock mostPopularChild_View;
        TitledTextblock mostPopularDescendent_View;
        //GridLayout displayGrid;
        RatingSummarizer ratingSummarizer;

        TitledControl participationDataDisplay;
        DateEntryView queryStartDateDisplay;
        DateEntryView queryEndDateDisplay;
        TextBlock totalTimeDisplay;
        TextBlock timeFractionDisplay;
        Duration smoothingDuration;
        TitledTextblock ratingWhenSuggested_Display;
        TitledTextblock ratingWhenNotSuggested_Display;
        TitledTextblock thinkingTime_Display;
    }
}

#endif