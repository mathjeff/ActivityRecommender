using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    class Browse_RecentSignificantActivities_Layout : TitledControl
    {
        public Browse_RecentSignificantActivities_Layout(Engine engine, LayoutStack layoutStack)
        {
            this.SetTitle("Find most significant recent activities");
            this.engine = engine;
            this.layoutStack = layoutStack;

            Button okButton = new Button();
            okButton.Clicked += OkButton_Clicked;

            this.durationLayout = new DurationEntryView();

            Vertical_GridLayout_Builder gridBuilder = new Vertical_GridLayout_Builder().Uniform();
            gridBuilder.AddLayout(new TextblockLayout("This screen allows you to identify the activities that contributed the most total increase or decrease to your " +
                "happiness during the given time period."));
            gridBuilder.AddLayout(new TitledControl("In the last:", durationLayout));
            gridBuilder.AddLayout(new ButtonLayout(okButton, "OK"));
            this.SetContent(ScrollLayout.New(gridBuilder.Build()));
        }

        private void OkButton_Clicked(object sender, EventArgs e)
        {
            this.okClicked();
        }
        private void okClicked()
        { 
            if (!this.durationLayout.IsDurationValid())
                return;
            TimeSpan windowSize = this.durationLayout.GetDuration();
            DateTime windowEnd = DateTime.Now;
            DateTime windowStart = windowEnd.Subtract(windowSize);
            Activities_HappinessContributions activities = this.engine.GetMostSignificantRecentActivities(windowStart, 10);
            this.showResults(activities, windowStart, windowEnd);
        }
        private void showResults(Activities_HappinessContributions contributions, DateTime start, DateTime end)
        {
            Vertical_GridLayout_Builder gridBuilder = new Vertical_GridLayout_Builder().Uniform();
            // title
            string title = "Activities adding or subtracting the most happiness from " + start + " to " + end;
            gridBuilder.AddLayout(new TextblockLayout(title));
            // contents
            // Show the top activities from best to worst
            foreach (ActivityHappinessContribution item in contributions.Best)
            {
                gridBuilder.AddLayout(this.renderContribution(item));
            }
            // show a divider if we get all of them
            if (contributions.ActivitiesRemain)
                gridBuilder.AddLayout(new TextblockLayout("..."));
            // Show the bottom activities, also from best to worst
            for (int i = contributions.Worst.Count - 1; i >= 0; i--)
            {
                gridBuilder.AddLayout(this.renderContribution(contributions.Worst[i]));
            }
            // more details
            LayoutChoice_Set helpWindow = new HelpWindowBuilder()
                .AddMessage("Activities that you like more than average are accompanied by positive numbers.")
                .AddMessage("Activities that you like less than average are accompanied by negative numbers.")
                .AddMessage("Activities that you participated in for more total time are accompanied by numbers that are further from 0.")
                .AddMessage("Specifically, each of these numbers is calculated by looking at all of the participations in that activity during this time, " +
                "computing the difference between the happiness of that participation and your overall average happiness, " +
                "multiplying each by the duration of its participation, and adding up the results.")
                .Build();
            gridBuilder.AddLayout(new HelpButtonLayout(helpWindow, this.layoutStack));

            // TODO: each entry here should be probably be a button that lists the participations that contributed to it
            gridBuilder.AddLayout(new TextblockLayout("If you want to see more details about the participations in a specific activity, " +
                "you can go back and select Browse Participations"));
            this.layoutStack.AddLayout(gridBuilder.Build(), "Significant Activities");
        }

        private LayoutChoice_Set renderContribution(ActivityHappinessContribution contribution)
        {
            double extraSeconds = contribution.TotalHappinessIncreaseInSeconds;
            TimeSpan bonus = TimeSpan.FromSeconds(extraSeconds);
            string bonusText;
            if (extraSeconds > 0)
            {
                bonusText = "+" + bonus;
            }
            else
            {
                bonusText = "" + bonus;
            }
            string text = contribution.Activity.Name + ": " + bonusText;
            return new TextblockLayout(text);
        }

        private DurationEntryView durationLayout;
        private Engine engine;
        private LayoutStack layoutStack;
    }
}
