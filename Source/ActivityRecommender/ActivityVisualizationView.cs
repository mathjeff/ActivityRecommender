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
        public ActivityVisualizationView(Activity activityToShow)
        {
            // setup the title
            this.SetTitle("Here is a visualization of " + activityToShow.Name + " over time. Press [ESCAPE] to return to the main screen.");
            //this.KeyDown += new System.Windows.Input.KeyEventHandler(ActivityVisualizationView_KeyDown);
            this.activityToDisplay = activityToShow;
            this.displayGrid = new DisplayGrid(1, 2);
            this.SetContent(this.displayGrid);

            DisplayGrid graphGrid = new DisplayGrid(2, 1);
            // setup a graph of the ratings
            this.ratingsView = new TitledControl("This is a graph of the ratings of " + this.activityToDisplay.Name + " over time.");
            graphGrid.AddItem(this.ratingsView);

            // setup a graph of the participations
            this.participationsView = new TitledControl("This is a graph of the participations in " + this.activityToDisplay.Name + " over time.");
            graphGrid.AddItem(this.participationsView);

            this.displayGrid.AddItem(graphGrid);


            // setup a display for some statistics
            DisplayGrid statsDisplayGrid = new DisplayGrid(7, 1);

            // setup an exit button
            this.exitButton = new Button();
            this.exitButton.Content = "Escape";
            this.exitButton.VerticalAlignment = VerticalAlignment.Center;
            this.exitButton.Width = 100;
            this.exitButton.HorizontalAlignment = HorizontalAlignment.Center;
            this.exitButton.Height = 30;
            statsDisplayGrid.AddItem(this.exitButton);

            // show some statistics of the participations
            this.participationDataDisplay = new TitledControl("Stats:");
            this.queryStartDateDisplay = new DateEntryView("Between");
            this.queryStartDateDisplay.SetDate(activityToShow.GetEarliestInteractionDate());
            this.queryStartDateDisplay.AddTextchangedHandler(new TextChangedEventHandler(this.DateTextChanged));
            statsDisplayGrid.AddItem(this.queryStartDateDisplay);
            this.queryEndDateDisplay = new DateEntryView("and");
            this.queryEndDateDisplay.SetDate(DateTime.Now);
            this.queryEndDateDisplay.AddTextchangedHandler(new TextChangedEventHandler(this.DateTextChanged));
            statsDisplayGrid.AddItem(this.queryEndDateDisplay);

            // display the total time spanned by the current window
            this.totalTimeDisplay = new ResizableTextBlock();
            statsDisplayGrid.AddItem(this.totalTimeDisplay);
            this.timeFractionDisplay = new ResizableTextBlock();
            statsDisplayGrid.AddItem(this.timeFractionDisplay);

            this.mostPopularChild_View = new TitledTextblock("Your most common immediate subactivity:");
            this.mostPopularChild_View.Text = "[There are none]";
            statsDisplayGrid.AddItem(this.mostPopularChild_View);

            this.mostPopularDescendent_View = new TitledTextblock("Your most common subactivity,\n among those having no subactivities:");
            this.mostPopularDescendent_View.Text = "[There are none]";
            statsDisplayGrid.AddItem(this.mostPopularDescendent_View);

            // put the stats view into the main view
            this.displayGrid.AddItem(statsDisplayGrid);

            
            this.UpdateParticipationStatsView();
            this.UpdateDrawing();
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
            RatingProgression ratingProgression = this.activityToDisplay.RatingProgression;
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
            ParticipationProgression participationProgression = this.activityToDisplay.ParticipationProgression;
            IEnumerable<Participation> participations = participationProgression.Participations;
            DateTime firstDate = this.queryStartDateDisplay.GetDate();
            List<Datapoint> points = new List<Datapoint>();
            double sumY = 0;
            PlotControl newPlot = new PlotControl();
            newPlot.ShowRegressionLine = true;
            newPlot.MinX = 0;
            double maxX = this.queryEndDateDisplay.GetDate().Subtract(firstDate).TotalSeconds;
            newPlot.MaxX = maxX;
            double x1 = 0;
            double x2 = 0;
            foreach (Participation participation in participations)
            {
                TimeSpan duration1 = participation.StartDate.Subtract(firstDate);
                // calculate x1 and x2
                x1 = duration1.TotalSeconds;
                TimeSpan duration2 = participation.EndDate.Subtract(firstDate);
                x2 = duration2.TotalSeconds;
                // make sure that we care about this point
                if (x1 <= newPlot.MaxX && x2 >= newPlot.MinX)
                {
                    // calculate y1
                    points.Add(new Datapoint(x1, sumY, x2 - x1));
                    sumY += participation.TotalIntensity.Mean * participation.TotalIntensity.Weight;
                    points.Add(new Datapoint(x2, sumY, x2 - x1));
                }
            }
            points.Add(new Datapoint(maxX, sumY, maxX - x2));
            newPlot.SetData(points);

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
            Participation summary = this.activityToDisplay.SummarizeParticipationsBetween(startDate, endDate);
            double numDaysSpent = summary.TotalIntensity.Weight / 3600 / 24;
            // figure out how much time there was between these dates
            TimeSpan availableDuration = endDate.Subtract(startDate);
            double totalNumDays = availableDuration.TotalDays;
            double participationFraction = numDaysSpent / totalNumDays;

            // now update the text blocks
            //this.availableTimeDisplay.Text = "Have known about this activity for " + Environment.NewLine + availableDuration.TotalDays + " days";
            this.totalTimeDisplay.Text = "You've spent " + Environment.NewLine + numDaysSpent + " days on this activity";
            this.timeFractionDisplay.Text = "Or " + Environment.NewLine + 100 * participationFraction + "% of your total time" + Environment.NewLine + " Or " + (participationFraction * 24 * 60).ToString() + " minutes per day";
            Activity bestChild = null;
            Distribution bestTotal = new Distribution();
            foreach (Activity child in this.activityToDisplay.Children)
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
            foreach (Activity child in this.activityToDisplay.GetAllSubactivities())
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

        Button exitButton;
        RoutedEventHandler exitHandler;
        Activity activityToDisplay;
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
    }
}
