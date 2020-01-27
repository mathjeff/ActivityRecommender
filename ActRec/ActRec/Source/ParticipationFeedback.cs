using ActivityRecommendation.TextSummary;
using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation
{
    class ParticipationFeedback
    {
        public ParticipationFeedback(Activity activity, string summary, ValueProvider<LayoutChoice_Set> detailsProvider)
        {
            this.Activity = activity;
            this.Summary = summary;
            this.detailsProvider = detailsProvider;
        }
        public Activity Activity { get; set; }

        public string Summary { get; set; }
        private ValueProvider<LayoutChoice_Set> detailsProvider;
        public LayoutChoice_Set Details
        {
            get
            {
                return this.detailsProvider.Get();
            }
        }
    }

    class ParticipationNumericFeedback : ValueProvider<LayoutChoice_Set>
    {
        public ParticipationNumericFeedback()
        {
        }
        public LayoutChoice_Set Get()
        {
            if (this.layout == null)
                this.layout = this.MakeLayout();
            return this.layout;
        }

        public LayoutChoice_Set MakeLayout()
        {
            BoundProperty_List rowHeights = new BoundProperty_List(8);
            // row 0 is a summary
            rowHeights.BindIndices(1, 3); // 1, 3, and 5 are titles
            rowHeights.BindIndices(1, 5);
            rowHeights.BindIndices(2, 4); // 2, 4, and 6 are details
            rowHeights.BindIndices(2, 6);
            // row 7 is a different activity
            GridLayout detailsGrid = GridLayout.New(rowHeights, new BoundProperty_List(1), LayoutScore.Zero); ;
            detailsGrid.AddLayout(new TextblockLayout(ChosenActivity.Name + " " +
                TimeFormatter.summarizeTimespan(this.StartDate, this.EndDate) + ": " +
                "You spent " + ParticipationDurationDividedByAverage + " as long as average. I predict:"
                ));

            detailsGrid.AddLayout(new TextblockLayout("Current fun:"));
            GridLayout funWhileDoingIt_layout = GridLayout.New(BoundProperty_List.Uniform(3), new BoundProperty_List(2), LayoutScore.Zero);
            funWhileDoingIt_layout.AddLayout(coloredRatio(PredictedCurrentValueForThisActivity));
            funWhileDoingIt_layout.AddLayout(new TextblockLayout("* avg fun while doing it at this time"));
            funWhileDoingIt_layout.AddLayout(new TextblockLayout("" + PredictedCurrentValueForThisActivity));
            funWhileDoingIt_layout.AddLayout(new TextblockLayout("stddev"));
            funWhileDoingIt_layout.AddLayout(coloredRatio(PredictedAverageValueForThisActivity));
            funWhileDoingIt_layout.AddLayout(new TextblockLayout("overall average for this activity"));
            detailsGrid.AddLayout(funWhileDoingIt_layout);

            detailsGrid.AddLayout(new TextblockLayout("Future fun:"));
            GridLayout futureFun_layout = GridLayout.New(BoundProperty_List.Uniform(3), new BoundProperty_List(2), LayoutScore.Zero);
            futureFun_layout.AddLayout(signedColoredValue(ExpectedFutureFunAfterDoingThisActivityNow));
            futureFun_layout.AddLayout(new TextblockLayout("days future fun (over next " + Math.Round(UserPreferences.DefaultPreferences.HalfLife.TotalDays / Math.Log(2), 0) + " days)"));
            futureFun_layout.AddLayout(new TextblockLayout("" + ExpectedFutureFunStddev));
            futureFun_layout.AddLayout(new TextblockLayout("stddev (days)"));
            futureFun_layout.AddLayout(signedColoredValue(ExpectedFutureFunAfterDoingThisActivitySometime));
            futureFun_layout.AddLayout(new TextblockLayout("overall average (days) after having done this activity"));
            detailsGrid.AddLayout(futureFun_layout);

            detailsGrid.AddLayout(new TextblockLayout("Future efficiency:"));
            GridLayout futureEfficiency_layout = GridLayout.New(BoundProperty_List.Uniform(3), new BoundProperty_List(2), LayoutScore.Zero);
            futureEfficiency_layout.AddLayout(signedColoredValue(ExpectedEfficiencyAfterDoingThisActivityNow));
            futureEfficiency_layout.AddLayout(new TextblockLayout("hours future efficiency (over next " + Math.Round(UserPreferences.DefaultPreferences.EfficiencyHalflife.TotalDays / Math.Log(2), 0) + " days) "));
            futureEfficiency_layout.AddLayout(new TextblockLayout("" + ExpectedEfficiencyStddev));
            futureEfficiency_layout.AddLayout(new TextblockLayout("stddev (hours)"));
            futureEfficiency_layout.AddLayout(signedColoredValue(ExpectedEfficiencyAfterDoingThisActivitySometime));
            futureEfficiency_layout.AddLayout(new TextblockLayout("overall average (hours) after having done this activity"));
            detailsGrid.AddLayout(futureEfficiency_layout);

            ActivitySuggestion suggestion = this.engine.MakeRecommendation((Activity)null, this.ChosenActivity, this.StartDate, TimeSpan.FromSeconds(0.5));
            Activity betterActivity = this.ActivityDatabase.ResolveDescriptor(suggestion.ActivityDescriptor);
            Prediction betterPrediction = this.engine.Get_OverallHappiness_ParticipationEstimate(betterActivity, this.StartDate);
            Prediction current = this.engine.Get_OverallHappiness_ParticipationEstimate(this.ChosenActivity, this.StartDate);
            Label redirectionLabel = new Label();
            detailsGrid.AddLayout(new TextblockLayout(redirectionLabel));
            if (betterPrediction.Distribution.Mean <= current.Distribution.Mean)
            {
                redirectionLabel.Text = "This activity is a reasonable choice at this time.";
                redirectionLabel.TextColor = Color.Green;
            }
            else
            {
                redirectionLabel.Text = "I recommend " + betterActivity.Name + " instead.";
                redirectionLabel.TextColor = Color.Red;
            }

            return detailsGrid;
        }

        static TextblockLayout signedColoredValue(double value)
        {
            Label label = new Label();
            TextblockLayout layout = new TextblockLayout(label);
            if (value > 0)
            {
                label.TextColor = Color.Green;
                label.Text = "+" + value;
            }
            else
            {
                if (value < 0)
                    label.TextColor = Color.Red;
                label.Text = "" + value;
            }
            return layout;
        }

        static TextblockLayout coloredRatio(double value)
        {
            Label label = new Label();
            TextblockLayout layout = new TextblockLayout(label);
            if (value > 1)
            {
                label.TextColor = Color.Green;
            }
            else
            {
                if (value < 1)
                    label.TextColor = Color.Red;
            }
            label.Text = "" + value;
            return layout;

        }


        public DateTime StartDate;
        public DateTime EndDate;
        public double ParticipationDurationDividedByAverage;

        public double PredictedCurrentValueForThisActivity;
        public double PredictedAverageValueForThisActivity;

        public double ExpectedFutureFunAfterDoingThisActivityNow;
        public double ExpectedFutureFunStddev;
        public double ExpectedFutureFunAfterDoingThisActivitySometime;

        public double ExpectedEfficiencyAfterDoingThisActivityNow;
        public double ExpectedEfficiencyStddev;
        public double ExpectedEfficiencyAfterDoingThisActivitySometime;

        public Activity ChosenActivity;

        public Engine engine;
        public ActivityDatabase ActivityDatabase;

        private LayoutChoice_Set layout;
    }
}
