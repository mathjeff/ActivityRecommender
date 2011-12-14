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
            this.SetTitle("Here is a visualization of the ratings of " + activityToShow.Name + " over time. Press [ESCAPE] to return to the main screen.");
            //activityToShow.ParticipationProgression;
            //activityToShow.RatingProgression;
            this.exitButton = new Button();
            this.exitButton.Content = "Escape";
            this.KeyDown += new System.Windows.Input.KeyEventHandler(ActivityVisualizationView_KeyDown);
            this.activityToDisplay = activityToShow;
            this.UpdateDrawing();
        }
        public void UpdateDrawing()
        {
            // draw the RatingProgression
            RatingProgression ratingProgression = this.activityToDisplay.RatingProgression;
            List<AbsoluteRating> ratings = ratingProgression.GetRatingsInDiscoveryOrder();
            if (ratings.Count > 0)
            {
                AbsoluteRating firstRating = ratings[0];
                DateTime firstDate = firstRating.Date;
                List<Point> points = new List<Point>();

                foreach (AbsoluteRating rating in ratings)
                {
                    DateTime when = rating.Date;
                    TimeSpan duration = when.Subtract(firstDate);
                    // put the x-coordinate in the list
                    double x = duration.TotalDays;
                    // put the y-coordinate in the list
                    double y = rating.Score;
                    points.Add(new Point(x, y));
                }
                PlotControl newPlot = new PlotControl();
                newPlot.SetData(points);
                //TextBox testBox = new TextBox();
                this.SetContent(newPlot);
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
        Button exitButton;
        RoutedEventHandler exitHandler;
        Activity activityToDisplay;
        
    }
}
