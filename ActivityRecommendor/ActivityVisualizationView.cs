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
            this.SetTitle("Visualization of " + activityToShow.Name);
            //activityToShow.ParticipationProgression;
            //activityToShow.RatingProgression;
            this.exitButton = new Button();
            this.exitButton.Content = "Escape";
            this.KeyDown += new System.Windows.Input.KeyEventHandler(ActivityVisualizationView_KeyDown);
            this.activityToDisplay = activityToDisplay;
        }
        public void UpdateDrawing()
        {
            // draw the RatingProgression
            RatingProgression ratingProgression = this.activityToDisplay.RatingProgression;
            //ratingProgression.

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
