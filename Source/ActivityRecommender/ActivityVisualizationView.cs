using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

// The ActivityVisualizationView shows a visual representation of the Participations and Ratings for an Activity
namespace ActivityRecommendation
{
    class ActivityVisualizationView : TitledControl
    {
        public ActivityVisualizationView(IProgression participationXAxis, Activity yAxisActivity, TimeSpan smoothingWindow)
        {
            this.xAxisProgression = participationXAxis;
            //this.xAxisActivity = null;
            this.yAxisActivity = yAxisActivity;
            this.smoothingDuration = smoothingWindow;

            // setup the title
            this.SetTitle("Here is a visualization of " + this.YAxisLabel + " vs. " + this.XAxisLabel + ". Press [ESCAPE] to return to the main screen.");
            //this.KeyDown += new System.Windows.Input.KeyEventHandler(ActivityVisualizationView_KeyDown);
            this.displayGrid = new DisplayGrid(1, 2);
            this.SetContent(this.displayGrid);

            DisplayGrid graphGrid = new DisplayGrid(2, 1);
            // setup a graph of the ratings
            this.ratingsView = new TitledControl("This is a graph of the ratings of " + this.YAxisLabel + " vs. Time");
            graphGrid.AddItem(this.ratingsView);

            // setup a graph of the participations
            this.participationsView = new TitledControl("This is a graph of the participations in " + this.YAxisLabel + " vs. " + this.XAxisLabel);
            graphGrid.AddItem(this.participationsView);

            this.displayGrid.AddItem(graphGrid);


            // setup a display for some statistics
            DisplayGrid statsDisplayGrid = new DisplayGrid(10, 1);

            // setup an exit button
            this.exitButton = new ResizableButton();
            this.exitButton.Content = "Escape";
            this.exitButton.VerticalAlignment = VerticalAlignment.Center;
            this.exitButton.Width = 100;
            this.exitButton.HorizontalAlignment = HorizontalAlignment.Center;
            this.exitButton.Height = 30;
            statsDisplayGrid.AddItem(this.exitButton);

            // show some statistics of the participations
            this.participationDataDisplay = new TitledControl("Stats:");
            this.queryStartDateDisplay = new DateEntryView("Between");
            this.queryStartDateDisplay.SetDate(this.yAxisActivity.GetEarliestInteractionDate());
            this.queryStartDateDisplay.Add_TextChanged_Handler(new TextChangedEventHandler(this.DateTextChanged));
            statsDisplayGrid.AddItem(this.queryStartDateDisplay);
            this.queryEndDateDisplay = new DateEntryView("and");
            this.queryEndDateDisplay.SetDate(DateTime.Now);
            this.queryEndDateDisplay.Add_TextChanged_Handler(new TextChangedEventHandler(this.DateTextChanged));
            statsDisplayGrid.AddItem(this.queryEndDateDisplay);

            // display the total time spanned by the current window
            this.totalTimeDisplay = new ResizableTextBlock();
            statsDisplayGrid.AddItem(this.totalTimeDisplay);
            this.timeFractionDisplay = new ResizableTextBlock();
            statsDisplayGrid.AddItem(this.timeFractionDisplay);

            this.thinkingTime_Display = new TitledTextblock("You often spend");
            this.thinkingTime_Display.Text = this.yAxisActivity.ThinkingTimes.Mean.ToString() + "\n seconds considering this activity";
            statsDisplayGrid.AddItem(this.thinkingTime_Display);

            this.mostPopularChild_View = new TitledTextblock("Your most common immediate subactivity:");
            this.mostPopularChild_View.Text = "[There are none]";
            statsDisplayGrid.AddItem(this.mostPopularChild_View);

            this.mostPopularDescendent_View = new TitledTextblock("Your most common subactivity,\n among those having no subactivities:");
            this.mostPopularDescendent_View.Text = "[There are none]";
            statsDisplayGrid.AddItem(this.mostPopularDescendent_View);

            // display rating statistics
            this.ratingWhenSuggested_Display = new TitledTextblock("Mean rating when suggested:");
            this.ratingWhenSuggested_Display.Text = this.yAxisActivity.ScoresWhenSuggested.Mean.ToString();
            statsDisplayGrid.AddItem(this.ratingWhenSuggested_Display);

            this.ratingWhenNotSuggested_Display = new TitledTextblock("Mean rating when not suggested:");
            this.ratingWhenNotSuggested_Display.Text = this.yAxisActivity.ScoresWhenNotSuggested.Mean.ToString();
            statsDisplayGrid.AddItem(this.ratingWhenNotSuggested_Display);

            // put the stats view into the main view
            this.displayGrid.AddItem(statsDisplayGrid);


            this.UpdateParticipationStatsView();
            this.UpdateDrawing();

            //CalculateExtraStats();
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
            if (!this.queryStartDateDisplay.IsDateValid() || !this.queryEndDateDisplay.IsDateValid())
                return;
            // draw the RatingProgression
            RatingProgression ratingProgression = this.yAxisActivity.RatingProgression;
            List<AbsoluteRating> ratings = ratingProgression.GetRatingsInDiscoveryOrder();
            DateTime firstDate = this.queryStartDateDisplay.GetDate();
            List<Datapoint> points = new List<Datapoint>();

            // make a plot
            PlotControl newPlot = new PlotControl();
            newPlot.ShowRegressionLine = true;
            newPlot.MinX = 0;
            newPlot.MaxX = this.queryEndDateDisplay.GetDate().Subtract(firstDate).TotalDays;
            newPlot.MinY = 0;
            newPlot.MaxY = 1;

            // create the datapoints
            foreach (AbsoluteRating rating in ratings)
            {
                if (rating.Date != null)
                {
                    TimeSpan duration = ((DateTime)rating.Date).Subtract(firstDate);
                    // calculate x
                    double x = duration.TotalDays;
                    // calculate y
                    double y = rating.Score;
                    // make sure we want to include it in the plot (and therefore the regression line)
                    if (x >= newPlot.MinX && x <= newPlot.MaxX)
                        points.Add(new Datapoint(x, y, 1));
                }
            }
            newPlot.SetData(points);

            this.ratingsView.SetContent(newPlot);
        }
        public void UpdateParticipationsPlot()
        {
            if (!this.queryStartDateDisplay.IsDateValid() || !this.queryEndDateDisplay.IsDateValid())
                return;
            // draw the ParticipationProgression
            ParticipationProgression participationProgression = this.yAxisActivity.ParticipationProgression;
            IEnumerable<Participation> participations = participationProgression.Participations;
            DateTime firstDate = this.queryStartDateDisplay.GetDate();
            DateTime lastDate = this.queryEndDateDisplay.GetDate();
            List<Datapoint> points = new List<Datapoint>();
            //double sumY = 0;
            PlotControl newPlot = new PlotControl();
            newPlot.ShowRegressionLine = true;

            
            newPlot.MinX = 0;
            newPlot.MaxX = 1;

            double x1, x2, y;
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
            }
            startXs.Sort();
            endXs.Sort();
            y = 0;
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
                    points.Add(new Datapoint(x1, y, weight));
                    y += weight * numActiveIntervals;
                    numActiveIntervals++;
                    // add a datapoint denoting the end of the interval
                    points.Add(new Datapoint(x2, y, weight));
                    startXs.RemoveAt(0);
                }
                else
                {
                    x2 = endX;
                    double weight = x2 - x1;
                    // add a datapoint denoting the start of the interval
                    points.Add(new Datapoint(x1, y, weight));
                    y += weight * numActiveIntervals;
                    numActiveIntervals--;
                    // add a datapoint denoting the end of the interval
                    points.Add(new Datapoint(x2, y, weight));
                    endXs.RemoveAt(0);
                }
                maxXPlotted = x2;
                x1 = x2;
            }
            /*
            foreach (DateTime date in dates)
            {
                // calculate x1 and x2
                x2 = this.GetParticipationXCoordinate(date);
                // make sure that we care about this point
                if (x2 <= newPlot.MaxX && x2 >= newPlot.MinX)
                {
                    y = this.GetParticipationYCoordinate(date);
                    double weight = x2 - x1;
                    if (this.xAxisProgression != null)
                    {
                        weight = 1;
                    }
                    points.Add(new Datapoint(x2, y, weight));
                    //sumY += participation.TotalIntensity.Mean * participation.TotalIntensity.Weight;
                    //points.Add(new Datapoint(x2, sumY, x2 - x1));
                    maxXPlotted = x2;
                    x1 = x2;
                }
            }*/
            /*
            if (maxX > maxXPlotted)
                points.Add(new Datapoint(maxX, sumY, maxX - maxXPlotted));
            */
            newPlot.SetData(points);

            /*if (this.xAxisActivity != null)
                newPlot.Connected = false;
            
            */
            this.participationsView.SetContent(newPlot);

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
            this.timeFractionDisplay.Text = "Or " + Environment.NewLine + 100 * participationFraction + "% of your total time" + Environment.NewLine + " Or " + (participationFraction * 24 * 60).ToString() + " minutes per day";
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

        ResizableButton exitButton;
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
        DisplayGrid displayGrid;

        TitledControl participationDataDisplay;
        DateEntryView queryStartDateDisplay;
        DateEntryView queryEndDateDisplay;
        ResizableTextBlock totalTimeDisplay;
        ResizableTextBlock timeFractionDisplay;
        Duration smoothingDuration;
        TitledTextblock ratingWhenSuggested_Display;
        TitledTextblock ratingWhenNotSuggested_Display;
        TitledTextblock thinkingTime_Display;
    }
}
