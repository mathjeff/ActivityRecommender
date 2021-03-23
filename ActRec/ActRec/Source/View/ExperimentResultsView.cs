using ActivityRecommendation.Effectiveness;
using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;

namespace ActivityRecommendation.View
{
    class ExperimentResultsView : ContainerLayout
    {
        public ExperimentResultsView(Participation participation)
        {
            HelpWindowBuilder builder = new HelpWindowBuilder();

            PlannedExperiment experiment = participation.RelativeEfficiencyMeasurement.Experiment;
            RelativeEfficiencyMeasurement earlier = experiment.FirstParticipation.RelativeEfficiencyMeasurement;
            RelativeEfficiencyMeasurement later = earlier.Later;
            string earlierName = earlier.ActivityDescriptor.ActivityName;
            string laterName = later.ActivityDescriptor.ActivityName;

            builder.AddMessage("You started an experiment at " + earlier.StartDate + "!");
            builder.AddMessage("You agreed to several choices, including:");
            builder.AddMessage("A: " + earlierName);
            builder.AddMessage("and");
            builder.AddMessage("B: " + laterName);

            double difficultyRatio = experiment.Earlier.DifficultyEstimate.EstimatedSuccessesPerSecond / experiment.Later.DifficultyEstimate.EstimatedSuccessesPerSecond;
            builder.AddMessage("You predicted that B would be " + this.round(difficultyRatio) + " times as difficult as A.");

            TimeSpan earlierDuration = earlier.EndDate.Subtract(earlier.StartDate);
            double earlierMinutes = earlierDuration.TotalMinutes;
            TimeSpan laterDuration = later.EndDate.Subtract(later.StartDate);
            double laterMinutes = laterDuration.TotalMinutes;

            builder.AddMessage("Then, you spent " + this.round(earlierDuration.TotalMinutes) + "m on A");

            builder.AddMessage("Later, you decided to do B at " + later.StartDate + ", which you spent " + this.round(laterDuration.TotalMinutes) + "m doing. ");

            double earlierEffectiveness = earlier.RecomputedEfficiency.Mean * earlierMinutes;
            double laterEffectiveness = later.RecomputedEfficiency.Mean * laterMinutes;

            // technically, the two efficiencies get estimated separately, but that's complicated to explain and we think it might confuse the user if we try to explain it
            builder.AddMessage("I estimated that your average efficiency on A would be about " + this.round(earlier.RecomputedEfficiency.Mean) +
                ", making your effectiveness " + this.round(earlier.RecomputedEfficiency.Mean) + " * " + this.round(earlierMinutes) + "m = " + this.round(earlierEffectiveness) + "m on A.");

            builder.AddMessage("That also means that your total effectiveness later on B was about " + this.round(earlierEffectiveness) + "m * "
                + this.round(difficultyRatio) + " = " + this.round(laterEffectiveness) + ", giving you an efficiency on B of " + this.round(laterEffectiveness) + " / " +
                this.round(laterMinutes) + " = " + this.round(later.RecomputedEfficiency.Mean));

            builder.AddMessage("Isn't that interesting?");

            this.SubLayout = builder.Build();
        }

        private double round(double value)
        {
            if (Math.Abs(value) >= 1)
                return Math.Round(value, 1);
            if (value == 0)
                return 0;
            return this.round(value * 10) / 10;
        }
    }
}
