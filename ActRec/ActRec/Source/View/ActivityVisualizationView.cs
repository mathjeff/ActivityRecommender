using StatLists;
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using VisiPlacement;
using Xamarin.Forms;

// The ActivityVisualizationView shows a visual representation of the Participations and Ratings for an Activity
namespace ActivityRecommendation
{
    class ActivityVisualizationView : TitledControl
    {
        public ActivityVisualizationView(IProgression participationXAxis, Activity yAxisActivity, ScoreSummarizer overallRating_summarizer, ScoreSummarizer overallEfficiency_summarizer, LayoutStack layoutStack)
        {
            this.layoutStack = layoutStack;
            this.overallRating_summarizer = overallRating_summarizer;
            this.overallEfficiency_summarizer = overallEfficiency_summarizer;

            this.xAxisProgression = participationXAxis;
            this.yAxisActivity = yAxisActivity;

            // setup the title
            this.SetTitle(this.YAxisLabel + " vs. " + this.XAxisLabel);

            GridLayout fixedGrid = GridLayout.New(new BoundProperty_List(1), BoundProperty_List.Uniform(2), LayoutScore.Zero);
            this.SetContent(fixedGrid);

            GridLayout graphGrid = GridLayout.New(BoundProperty_List.Uniform(2), new BoundProperty_List(1), LayoutScore.Zero);
            // setup a graph of the ratings
            this.ratingsView = new TitledControl("Ratings vs. Time");
            graphGrid.AddLayout(this.ratingsView);

            // setup a graph of the participations
            this.participationsView = new TitledControl("Participations vs. " + this.XAxisLabel);
            graphGrid.AddLayout(this.participationsView);

            //flexibleGrid.AddLayout(graphGrid);
            fixedGrid.AddLayout(graphGrid);

            fixedGrid.AddLayout(this.Make_StatsView());

            this.UpdateDrawing();
        }
        private LayoutChoice_Set Make_StatsView()
        {
            // many of the entries in this function are disabled because they're probably not that useful but might be worth re-adding somehow

            // setup a display for some statistics
            Vertical_GridLayout_Builder builder = new Vertical_GridLayout_Builder().Uniform();

            // display an editable date range
            this.participationDataDisplay = new TitledControl("Stats:");
            this.queryStartDateDisplay = new DateEntryView("From", this.layoutStack);
            this.queryStartDateDisplay.SetDay(this.yAxisActivity.GetEarliestInteractionDate());
            this.queryStartDateDisplay.Add_TextChanged_Handler(new EventHandler<TextChangedEventArgs>(this.DateTextChanged));
            builder.AddLayout(this.queryStartDateDisplay);
            this.queryEndDateDisplay = new DateEntryView("until", this.layoutStack);
            this.queryEndDateDisplay.SetDay(DateTime.Today.AddDays(1));
            this.queryEndDateDisplay.Add_TextChanged_Handler(new EventHandler<TextChangedEventArgs>(this.DateTextChanged));
            builder.AddLayout(this.queryEndDateDisplay);

            // display the total time spanned by the current window
            this.totalTimeDisplay = new TextblockLayout();
            this.totalTimeDisplay.AlignHorizontally(TextAlignment.Center);
            builder.AddLayout(this.totalTimeDisplay);
            this.timeFractionDisplay = new TextblockLayout();
            this.timeFractionDisplay.AlignHorizontally(TextAlignment.Center);
            builder.AddLayout(this.timeFractionDisplay);

            // display rating statistics
            this.ratingWhenNotSuggested_Display = new TitledTextblock("Mean rating:");
            this.ratingWhenNotSuggested_Display.Text = Math.Round(this.yAxisActivity.Ratings.Mean, 5).ToString();
            builder.AddLayout(this.ratingWhenNotSuggested_Display);

            this.ratingWhenSuggested_Display = new TitledTextblock("Mean rating when suggested:");
            this.ratingWhenSuggested_Display.Text = Math.Round(this.yAxisActivity.RatingsWhenSuggested.Mean, 5).ToString();
            builder.AddLayout(this.ratingWhenSuggested_Display);


            builder.AddLayout(this.make_helpLayout());
            
            return builder.Build();
        }

        private LayoutChoice_Set make_helpLayout()
        {
            LayoutChoice_Set ratingsHelpLayout = new HelpWindowBuilder()
                .AddMessage("The Ratings graph shows four values over time.")
                .AddMessage("The green plot indicates how you have rated this activity during this time.")
                .AddMessage("The red plot is a straight line showing the trend of the green plot.")
                .AddMessage("The blue plot essentially estimates how happy you have been overall"
                +" (technically, for each point in time, the blue plot shows the net present value (an economics term) of all ratings after that point).")
                .AddMessage("Similarly, the yellow plot estimates approximately how efficient you have been overall (there will only be efficiency data if you have been using the experimentation feature)")
                .Build();
            LayoutChoice_Set participationsHelpLayout = new HelpWindowBuilder()
                .AddMessage("The Participations graph shows four values over time.")
                .AddMessage("The green plot shows the cumulative amount of time you've spent on this particular activity during this time.")
                .AddMessage("The red plot is a straight line showing the trend of the green plot.")
                .AddMessage("The blue plot shows the cumulative number of times that ActivityRecommender has recommended this activity to you.")
                .AddMessage("The yellow plot estimates your cumulative effectiveness in doing this activity over time. This plot will only be different from the green plot requires if you have been using the experimentation feature. ")
                .AddMessage("You'll also notice some short vertical lines near the bottom, denoting days or months based on how much time the graph encompasses.")
                .Build();
            LayoutChoice_Set dateHelpLayout = new HelpWindowBuilder()
                .AddMessage("Notice the two date boxes in the top-right corner of the screen.")
                .AddMessage("If you enter new dates into them, then the rest of the screen will update accordingly.")
                .AddMessage("By default, the start-date box will be set to the earliest date for which there is any data for this activity.")
                .AddMessage("By default, the end-date box will be set to tomorrow at midnight (the beginning of the day).")
                .Build();
            LayoutChoice_Set statsHelpLayout = new HelpWindowBuilder()
                .AddMessage("The Stats description will show assorted statistics about these activities.")
                .AddMessage("The statistics will only refer to data that falls within the duration specified by the date boxes above.")
                .Build();

            GridLayout helpGrid = GridLayout.New(BoundProperty_List.Uniform(2), BoundProperty_List.Uniform(2), LayoutScore.Zero);
            helpGrid.AddLayout(new HelpButtonLayout("Ratings graph", ratingsHelpLayout, this.layoutStack));
            helpGrid.AddLayout(new HelpButtonLayout("Date selection", dateHelpLayout, this.layoutStack));
            helpGrid.AddLayout(new HelpButtonLayout("Participations graph", participationsHelpLayout, this.layoutStack));
            helpGrid.AddLayout(new HelpButtonLayout("Text stats", statsHelpLayout, this.layoutStack));

            LayoutChoice_Set helpButton = new HelpButtonLayout("Explain",
                new TitledControl("Choose a View to Explain", helpGrid), this.layoutStack);
            return helpButton;
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
            newPlot.MinX = 0;
            newPlot.MaxX = endDate.Subtract(startDate).TotalDays;
            newPlot.MinY = 0;
            newPlot.MaxY = 1;

            if (this.xAxisProgression != null)
                this.configureXAxisSubdivisions(newPlot);

            newPlot.YAxisSubdivisions = new List<double> { 0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1 };

            // compute the rating at each relevant date
            foreach (AbsoluteRating rating in ratings)
            {
                if (rating.Date != null)
                {
                    double x = this.GetXCoordinate((DateTime)rating.Date);
                    // calculate y
                    double y = rating.Score;
                    // make sure we want to include it in the plot (and therefore the regression line)
                    if (x >= newPlot.MinX && x <= newPlot.MaxX)
                        points.Add(new Datapoint(x, y, 1));
                }
            }
            newPlot.AddSeries(points, true);

            // compute a smoothed version of the ratings so we can show which activities are actually worth doing
            // (for example, sleeping might feel boring but might increase later happiness)
            TimeSpan totalDuration = endDate.Subtract(startDate);
            List<ScoreSummarizer> ratingSummarizers = new List<ScoreSummarizer>();
            ratingSummarizers.Add(this.overallRating_summarizer);
            ratingSummarizers.Add(this.overallEfficiency_summarizer);
            foreach (ScoreSummarizer ratingSummarizer in ratingSummarizers)
            {
                double i;
                ScoreSummary ratingSummary = new ScoreSummary(endDate);
                List<Datapoint> smoothedRatings = new List<Datapoint>();
                double maxY = 0;
                for (i = 1; i >= 0; i -= 0.001)
                {
                    TimeSpan currentDuration = new TimeSpan((long)((double)totalDuration.Ticks * (double)i));
                    DateTime when = startDate.Add(currentDuration);
                    if (when.CompareTo(ratingSummarizer.EarliestKnownDate) < 0)
                        break;
                    ratingSummary.Update(ratingSummarizer, when, endDate);
                    double x = this.GetXCoordinate(when);
                    double y = ratingSummary.Item.Mean;
                    if (!double.IsNaN(y))
                    {
                        if (ratingSummarizer == this.overallEfficiency_summarizer)
                        {
                            // rescale so that the typical value for this rating summarizer (1) is moved to match the typical value for the other plotted values (0.5)
                            y /= 2;
                        }
                        if (y > maxY)
                            maxY = y;
                        smoothedRatings.Add(new Datapoint(x, y, 1));
                    }
                }
                smoothedRatings.Reverse();
                // if the plot overflowed, rescale it to fit
                if (maxY > 1)
                {
                    foreach (Datapoint datapoint in smoothedRatings)
                    {
                        datapoint.Output = datapoint.Output / maxY;
                    }
                }
                newPlot.AddSeries(smoothedRatings, false);

            }

            DateTime end = DateTime.Now;

            this.ratingsView.SetContent(new ImageLayout(newPlot, LayoutScore.Get_UsedSpace_LayoutScore(1)));

            System.Diagnostics.Debug.WriteLine("spent " + end.Subtract(start) + " to update ratings plot");

        }
        private DateTime MaxDate(DateTime a, DateTime b)
        {
            if (a.CompareTo(b) >= 0)
                return a;
            return b;
        }
        private DateTime MinDate(DateTime a, DateTime b)
        {
            if (a.CompareTo(b) <= 0)
                return a;
            return b;
        }
        public void UpdateParticipationsPlot()
        {
            DateTime start = DateTime.Now;

            if (!this.queryStartDateDisplay.IsDateValid() || !this.queryEndDateDisplay.IsDateValid())
                return;
            // draw the ParticipationProgression
            AutoSmoothed_ParticipationProgression participationProgression = this.yAxisActivity.ParticipationProgression;
            List<Participation> participations = participationProgression.Participations;
            DateTime firstDate = this.queryStartDateDisplay.GetDate();
            DateTime lastDate = this.queryEndDateDisplay.GetDate();
            //double firstCoordinate = this.GetXCoordinate(firstDate);
            //double lastCoordinate = this.GetXCoordinate(lastDate);
            List<Datapoint> cumulativeParticipationDurations = new List<Datapoint>();
            List<Datapoint> cumulativeEffectivenesses = new List<Datapoint>();
            List<Datapoint> cumulativeSuggestionCounts = new List<Datapoint>();
            List<double> suggestionDates = new List<double>();

            PlotView newPlot = new PlotView();

            
            newPlot.MinX = 0;
            newPlot.MaxX = 1;

            double x1, x2, cumulativeParticipationSeconds, cumulativeEffectiveness, cumulativeSuggestionCount;
            x1 = 0;

            if (this.xAxisProgression != null)
                this.configureXAxisSubdivisions(newPlot);
            if (newPlot.MinX.HasValue)
                x1 = newPlot.MinX.Value;


            List<DateTime> startXs = new List<DateTime>(participations.Count);
            List<DateTime> endXs = new List<DateTime>(participations.Count);

            // figure out which dates we care about
            int numActiveIntervals = 0;

            foreach (Participation participation in participations)
            {
                // update some data about cumulative participation duration
                // make sure this participation is relevant
                if (participation.EndDate.CompareTo(firstDate) >= 0 && participation.StartDate.CompareTo(lastDate) <= 0)
                {
                    startXs.Add(this.MaxDate(firstDate, participation.StartDate));
                    endXs.Add(this.MinDate(participation.EndDate, lastDate));

                    // update some data about cumulative num suggestions
                    // this is slightly hacky - really there should be a method that just returns a list of every Suggestion for an Activity
                    if (participation.Suggested)
                    {
                        // double-check that the participation was suggested more recently than the start DateTime
                        if (participation.StartDate.CompareTo(participation.StartDate) >= 0)
                            suggestionDates.Add(this.GetXCoordinate(participation.StartDate));
                    }
                }
            }
            startXs.Sort();
            endXs.Sort();
            cumulativeParticipationSeconds = 0;
            cumulativeEffectiveness = 0;
            DateTime prevDate = firstDate;
            while (endXs.Count > 0 || startXs.Count > 0)
            {
                DateTime participationStart;
                DateTime participationEnd;
                if (startXs.Count > 0)
                {
                    if (endXs.Count > 0)
                    {
                        participationStart = startXs[0];
                        participationEnd = endXs[0];
                    }
                    else
                    {
                        participationStart = startXs[0];
                        participationEnd = lastDate;  // something larger that will be ignored
                    }
                }
                else
                {
                    participationEnd = endXs[0];
                    participationStart = lastDate;      // something larger that will be ignored
                }
                if (participationStart.CompareTo(participationEnd) < 0)
                {
                    x2 = this.GetXCoordinate(participationStart);
                    double weight = x2 - x1;
                    // add datapoints denoting the start of the idle interval
                    cumulativeParticipationDurations.Add(new Datapoint(x1, cumulativeParticipationSeconds, weight));
                    cumulativeEffectivenesses.Add(new Datapoint(x1, cumulativeEffectiveness, weight));
                    // update cumulatives
                    cumulativeParticipationSeconds += weight * numActiveIntervals;
                    if (numActiveIntervals > 0)
                    {
                        Distribution efficiency = this.overallEfficiency_summarizer.GetValueDistributionForDates(prevDate, lastDate, true, false);
                        if (efficiency.Weight > 0)
                        {
                            cumulativeEffectiveness += efficiency.Mean * weight * numActiveIntervals;
                        }
                    }
                    numActiveIntervals++;
                    // add datapoints denoting the end of the idle interval
                    cumulativeParticipationDurations.Add(new Datapoint(x2, cumulativeParticipationSeconds, weight));
                    cumulativeEffectivenesses.Add(new Datapoint(x2, cumulativeEffectiveness, weight));
                    startXs.RemoveAt(0);
                    prevDate = participationStart;
                }
                else
                {
                    x2 = this.GetXCoordinate(participationEnd);
                    double weight = x2 - x1;
                    // add datapoints denoting the start of the active interval
                    cumulativeParticipationDurations.Add(new Datapoint(x1, cumulativeParticipationSeconds, weight));
                    cumulativeEffectivenesses.Add(new Datapoint(x1, cumulativeEffectiveness, weight));
                    cumulativeParticipationSeconds += weight * numActiveIntervals;
                    Distribution efficiency = this.overallEfficiency_summarizer.GetValueDistributionForDates(prevDate, lastDate, true, false);
                    if (efficiency.Weight > 0)
                    {
                        cumulativeEffectiveness += efficiency.Mean * weight * numActiveIntervals;
                    }
                    numActiveIntervals--;
                    // add datapoints denoting the end of the active interval
                    cumulativeParticipationDurations.Add(new Datapoint(x2, cumulativeParticipationSeconds, weight));
                    cumulativeEffectivenesses.Add(new Datapoint(x2, cumulativeEffectiveness, weight));
                    endXs.RemoveAt(0);
                    prevDate = participationEnd;
                }
                x1 = x2;
            }

            double cumulativeParticipationHours = cumulativeParticipationSeconds / 3600;
            double hoursPerTick = this.computeTickInterval(cumulativeParticipationHours);
            double spacePerTick = hoursPerTick * 3600;

            // rescale cumulativeParticipationDurations to total 1
            if (cumulativeParticipationSeconds != 0)
            {
                foreach (Datapoint item in cumulativeParticipationDurations)
                {
                    item.Output = item.Output / cumulativeParticipationSeconds;
                }
                spacePerTick = spacePerTick / cumulativeParticipationSeconds;
            }

            // rescale cumulativeEffectivenesses to total 1
            if (cumulativeEffectiveness != 0)
            {
                foreach (Datapoint item in cumulativeEffectivenesses)
                {
                    item.Output = item.Output / cumulativeEffectiveness;
                }
            }

            // We also want to plot the number of times that the activity was suggested
            cumulativeSuggestionCount = 0;
            foreach (ListItemStats<DateTime, WillingnessSummary> item in this.yAxisActivity.ConsiderationProgression.AllItems)
            {
                // make sure the participation is within the requested window
                if (item.Key.CompareTo(firstDate) >= 0 && item.Key.CompareTo(lastDate) <= 0)
                {
                    double x = this.GetXCoordinate(item.Key);
                    if (item.Value.NumSkips > 0)
                    {
                        // found a skip
                        suggestionDates.Add(x);
                    }
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

            newPlot.AddSeries(cumulativeParticipationDurations, true);
            newPlot.AddSeries(cumulativeSuggestionCounts, false);
            newPlot.AddSeries(cumulativeEffectivenesses, false);

            // assign y-axis tick marks
            if (spacePerTick > 0)
            {
                double y = 0;
                List<double> yTicks = new List<double>();
                while (y < 1)
                {
                    yTicks.Add(y);
                    y += spacePerTick;
                }
                newPlot.YAxisSubdivisions = yTicks;
            }

            this.participationsView.SetContent(new ImageLayout(newPlot, LayoutScore.Get_UsedSpace_LayoutScore(1)));

            DateTime end = DateTime.Now;

            System.Diagnostics.Debug.WriteLine("spent " + end.Subtract(start) + " to update partipations plot");

            this.UpdateParticipationStatsView(cumulativeParticipationSeconds);

            // debugging
            //this.SetContent(this.participationsView.GetContent());
        }

        // Given the size of some range, returns an appropriate interval for separating tick marks on a graph of that size
        private double computeTickInterval(double range)
        {
            return Math.Pow(10, (int)Math.Log10(range / 2));
        }
        private void configureXAxisSubdivisions(PlotView plotView)
        {
            AdaptiveLinearInterpolation.FloatRange inputRange = this.xAxisProgression.EstimateOutputRange();
            if (inputRange == null)
            {
                DateTime firstDate = this.queryStartDateDisplay.GetDate();
                DateTime lastDate = this.queryEndDateDisplay.GetDate();
                inputRange = new AdaptiveLinearInterpolation.FloatRange(this.GetXCoordinate(firstDate), true, GetXCoordinate(lastDate), true);
            }

            plotView.XAxisSubdivisions = this.xAxisProgression.GetNaturalSubdivisions(inputRange.LowCoordinate, inputRange.HighCoordinate);
            plotView.MinX = inputRange.LowCoordinate;
            plotView.MaxX = inputRange.HighCoordinate;
        }

        private void DateTextChanged(object sender, TextChangedEventArgs e)
        {
            this.UpdateDrawing();
        }
        public void UpdateParticipationStatsView(double numSeconds)
        {
            double numHoursSpent = numSeconds / 3600;
            DateTime startDate = this.queryStartDateDisplay.GetDate();
            DateTime endDate = this.queryEndDateDisplay.GetDate();

            // figure out how much time there was between these dates
            TimeSpan availableDuration = endDate.Subtract(startDate);
            double totalNumHours = availableDuration.TotalHours;
            double participationFraction = numHoursSpent / totalNumHours;

            // now update the text blocks
            this.totalTimeDisplay.setText("You've spent " + Environment.NewLine + Math.Round(numHoursSpent, 3) + " hours on " + this.YAxisLabel);
            this.timeFractionDisplay.setText("Or " + Environment.NewLine + Math.Round(participationFraction * 24 * 60, 3).ToString() + " minutes per day");
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

        private double GetXCoordinate(DateTime when)
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
            ParticipationsSummary yParticipation = this.yAxisActivity.SummarizeParticipationsBetween(startDate, when);
            double y = yParticipation.CumulativeIntensity.TotalSeconds;

            return y;
        }

        Activity yAxisActivity;
        IProgression xAxisProgression;
        TitledControl ratingsView;
        TitledControl participationsView;
        ScoreSummarizer overallRating_summarizer;
        ScoreSummarizer overallEfficiency_summarizer;

        TitledControl participationDataDisplay;
        DateEntryView queryStartDateDisplay;
        DateEntryView queryEndDateDisplay;
        TextblockLayout totalTimeDisplay;
        TextblockLayout timeFractionDisplay;
        TitledTextblock ratingWhenSuggested_Display;
        TitledTextblock ratingWhenNotSuggested_Display;
        LayoutStack layoutStack;
    }
}
