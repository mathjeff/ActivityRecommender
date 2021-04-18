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
            RelativeEfficiencyMeasurement later = experiment.SecondParticipation.RelativeEfficiencyMeasurement;
            string earlierName = earlier.ActivityDescriptor.ActivityName;
            string laterName = later.ActivityDescriptor.ActivityName;

            builder.AddMessage("In your experiment at " + earlier.StartDate + ", you agreed to several choices, including:");
            builder.AddMessage("A: " + earlierName);
            builder.AddMessage("B: " + laterName);

            double difficultyRatio = experiment.Earlier.DifficultyEstimate.EstimatedSuccessesPerSecond / experiment.Later.DifficultyEstimate.EstimatedSuccessesPerSecond;
            builder.AddMessage("You predicted that B would be " + this.round(difficultyRatio) + " times as difficult as A.");

            TimeSpan earlierDuration = earlier.EndDate.Subtract(earlier.StartDate);
            double earlierMinutes = earlierDuration.TotalMinutes;
            TimeSpan laterDuration = later.EndDate.Subtract(later.StartDate);
            double laterMinutes = laterDuration.TotalMinutes;

            double earlierHelpFraction = experiment.FirstParticipation.HelpFraction;
            string earlierHelpDescription = "";
            if (earlierHelpFraction > 0)
                earlierHelpDescription = " with help, and you personally completed " + this.round(1 - earlierHelpFraction) + " of it";
            builder.AddMessage("Then, you spent " + this.round(earlierDuration.TotalMinutes) + "m on A" + earlierHelpDescription);

            double laterHelpFraction = experiment.SecondParticipation.HelpFraction;
            string laterHelpDescription = "";
            if (laterHelpFraction > 0)
                laterHelpDescription = " with help, and you personally completed " + this.round(1 - laterHelpFraction) + " of it";
            builder.AddMessage("Later, you spent " + this.round(laterDuration.TotalMinutes) + "m on B at " + later.StartDate + laterHelpDescription);

            double earlierTotalEffectiveness = earlier.RecomputedEfficiency.Mean * earlierMinutes / (1 - earlierHelpFraction);
            double laterTotalEffectiveness = later.RecomputedEfficiency.Mean * laterMinutes / (1 - laterHelpFraction);

            string earlierHelpDivision = "";
            if (earlierHelpFraction > 0)
                earlierHelpDivision = " / " + this.round(1 - earlierHelpFraction);
            builder.AddMessage("I estimated that your efficiency on A would be about " + this.round(earlier.RecomputedEfficiency.Mean) +
                ", making the difficulty of A be " + this.round(earlier.RecomputedEfficiency.Mean) + " * " + this.round(earlierMinutes) + "m" + earlierHelpDivision + " = " +
                this.round(earlierTotalEffectiveness) + "m");

            builder.AddMessage("So, the difficulty of B should be " + this.round(earlierTotalEffectiveness) + "m * "
                + this.round(difficultyRatio) + " = " + this.round(laterTotalEffectiveness) + "m");

            string laterHelpDivision = "";
            if (laterHelpFraction > 0)
                laterHelpDivision = " / " + this.round(1 - laterHelpFraction);
            builder.AddMessage("And your efficiency on B should be " + this.round(laterTotalEffectiveness) + "m / " +
                this.round(laterMinutes) + "m" + laterHelpDivision + " = " + this.round(later.RecomputedEfficiency.Mean));

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
