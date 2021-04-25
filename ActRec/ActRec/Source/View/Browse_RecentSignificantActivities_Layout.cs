using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    class Browse_RecentSignificantActivities_Layout : TitledControl
    {
        public Browse_RecentSignificantActivities_Layout(Engine engine, ScoreSummarizer scoreSummarizer, LayoutStack layoutStack)
        {
            this.SetTitle("Significant recent activities");
            this.engine = engine;
            this.scoreSummarizer = scoreSummarizer;
            this.layoutStack = layoutStack;

            Button okButton = new Button();
            okButton.Clicked += OkButton_Clicked;

            this.durationLayout = new DurationEntryView();

            GridLayout_Builder gridBuilder = new Vertical_GridLayout_Builder().Uniform();
            gridBuilder.AddLayout(new TextblockLayout("What changed your happiness most"));
            gridBuilder.AddLayout(new TitledControl("in the last:", durationLayout));
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
            Activities_HappinessContributions activities = this.engine.GetMostSignificantRecentActivities(windowStart, 6);
            this.showResults(activities, windowStart, windowEnd);
        }
        private void showResults(Activities_HappinessContributions contributions, DateTime start, DateTime end)
        {
            GridLayout_Builder gridBuilder = new Vertical_GridLayout_Builder().Uniform();
            // title
            string title = "Activities adding or subtracting the most happiness from " + start + " to " + end;
            gridBuilder.AddLayout(new TextblockLayout(title));
            // contents
            // Show the top activities from best to worst
            foreach (ActivityHappinessContribution item in contributions.Best)
            {
                gridBuilder.AddLayout(this.renderContribution(item, start));
            }
            // show a divider if we get all of them
            if (contributions.ActivitiesRemain)
                gridBuilder.AddLayout(new TextblockLayout("..."));
            // Show the bottom activities, also from best to worst
            for (int i = contributions.Worst.Count - 1; i >= 0; i--)
            {
                gridBuilder.AddLayout(this.renderContribution(contributions.Worst[i], start));
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

            this.layoutStack.AddLayout(gridBuilder.Build(), "Significant Activities");
        }

        private LayoutChoice_Set renderContribution(ActivityHappinessContribution contribution, DateTime start)
        {
            double extraHours = contribution.TotalHappinessIncreaseInSeconds / 3600;
            double roundedHours = Math.Round(extraHours, 2);
            string bonusText;
            if (extraHours > 0)
            {
                bonusText = "+" + roundedHours + " hours";
            }
            else
            {
                bonusText = "" + roundedHours + " hours";
            }
            string text = contribution.Activity.Name + ": " + bonusText;
            return new SignificantActivity_Layout(text, contribution.Activity, start, this.engine, this.scoreSummarizer, this.layoutStack);
        }

        private DurationEntryView durationLayout;
        private ScoreSummarizer scoreSummarizer;
        private Engine engine;
        private LayoutStack layoutStack;
    }

    class SignificantActivity_Layout : ContainerLayout
    {
        public SignificantActivity_Layout(string text, Activity activity, DateTime start, Engine engine, ScoreSummarizer scoreSummarizer,  LayoutStack layoutStack)
        {
            this.engine = engine;
            this.activity = activity;
            this.start = start;
            this.scoreSummarizer = scoreSummarizer;
            this.layoutStack = layoutStack;

            Button button = new Button();
            button.Clicked += Button_Clicked;
            ButtonLayout buttonLayout = new ButtonLayout(button, text);
            this.SubLayout = buttonLayout;
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            this.show();
        }
        private void show()
        {
            List<Participation> participations = this.activity.getParticipationsSince(this.start);
            // we reduce the number of visible participations
            string title;
            int maxNumParticipations = 20;
            if (participations.Count > maxNumParticipations)
            {
                title = "Last " + maxNumParticipations + " participations in " + this.activity.Name;
                participations = participations.GetRange(participations.Count - maxNumParticipations, maxNumParticipations);
            }
            else
            {
                title = "Participations in " + this.activity.Name + " since " + this.start;
            }
            ListParticipations_Layout content = new ListParticipations_Layout(participations, this.engine, this.scoreSummarizer, this.layoutStack);
            TitledControl results = new TitledControl(title, content, 20);
            this.layoutStack.AddLayout(results, "Participations");
        }
        Activity activity;
        DateTime start;
        LayoutStack layoutStack;
        ScoreSummarizer scoreSummarizer;
        Engine engine;
    }
}
