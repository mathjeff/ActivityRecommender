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
            this.SetTitle("Here is a visualization of " + activityToShow.Name + " over time. Press [ESCAPE] to return to the main screen.");
            this.exitButton = new Button();
            this.exitButton.Content = "Escape";
            this.KeyDown += new System.Windows.Input.KeyEventHandler(ActivityVisualizationView_KeyDown);
            this.activityToDisplay = activityToShow;
            this.displayGrid = new DisplayGrid(2, 1);
            this.SetContent(this.displayGrid);

            this.ratingsView = new TitledControl("This is a graph of the ratings of " + this.activityToDisplay.Name + " over time.");
            this.displayGrid.SetItem(this.ratingsView, 0, 0);
            this.participationsView = new TitledControl("This is a graph of the participations in " + this.activityToDisplay.Name + " over time.");
            this.displayGrid.SetItem(this.participationsView, 1, 0);

            this.UpdateDrawing();
        }
        public void UpdateDrawing()
        {
            DateTime now = DateTime.Now;
            this.UpdateRatingsPlot(now);
            this.UpdateParticipationsPlot(now);
        }
        public void UpdateRatingsPlot(DateTime when)
        {
            // draw the RatingProgression
            RatingProgression ratingProgression = this.activityToDisplay.RatingProgression;
            List<AbsoluteRating> ratings = ratingProgression.GetRatingsInDiscoveryOrder();
            DateTime firstDate = this.activityToDisplay.DiscoveryDate;
            List<Point> points = new List<Point>();

            // create the datapoints
            foreach (AbsoluteRating rating in ratings)
            {
                TimeSpan duration = rating.Date.Subtract(firstDate);
                // put the x-coordinate in the list
                double x = duration.TotalDays;
                // put the y-coordinate in the list
                double y = rating.Score;
                points.Add(new Point(x, y));
            }
            // make a plot
            PlotControl newPlot = new PlotControl();
            newPlot.SetData(points);
            newPlot.MinX = 0;
            newPlot.MaxX = when.Subtract(firstDate).TotalDays;

            this.ratingsView.SetContent(newPlot);
        }
        public void UpdateParticipationsPlot(DateTime when)
        {
            // draw the ParticipationProgression
            ParticipationProgression participationProgression = this.activityToDisplay.ParticipationProgression;
            List<Participation> participations = participationProgression.Participations;
            DateTime firstDate = this.activityToDisplay.DiscoveryDate;
            List<Point> points = new List<Point>();
            double sumY = 0;
            foreach (Participation participation in participations)
            {
                TimeSpan duration1 = participation.StartDate.Subtract(firstDate);
                // calculate x1
                double x1 = duration1.TotalDays;
                // calculate y1
                points.Add(new Point(x1, sumY));
                TimeSpan duration2 = participation.EndDate.Subtract(firstDate);
                // put x2 in the list
                double x2 = duration2.TotalDays;
                sumY += participation.TotalIntensity.Mean * participation.TotalIntensity.Weight;
                points.Add(new Point(x2, sumY));                    
            }
            PlotControl newPlot = new PlotControl();
            newPlot.SetData(points);
            newPlot.MinX = 0;
            newPlot.MaxX = when.Subtract(firstDate).TotalDays;

            this.participationsView.SetContent(newPlot);
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
        PlotControl ratingsPlot;
        PlotControl participationsPlot;
        TitledControl ratingsView;
        TitledControl participationsView;
        DisplayGrid displayGrid;
    }
}
