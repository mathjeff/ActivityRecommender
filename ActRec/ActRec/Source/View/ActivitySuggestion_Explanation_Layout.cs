using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisiPlacement;

namespace ActivityRecommendation.View
{
    public class ActivitySuggestion_Explanation_Layout : ContainerLayout
    {
        private int maxFontSize = 30;
        public ActivitySuggestion_Explanation_Layout(ActivitySuggestion_Explanation justification)
        {
            ActivitySuggestion suggestion = justification.Suggestion;
            Vertical_GridLayout_Builder builder = new Vertical_GridLayout_Builder();
            builder.AddLayout(this.newTextBlock("Suggesting " + suggestion.ActivityDescriptor.ActivityName + " " +
                justification.Suggestion.StartDate));
            if (suggestion.ParticipationProbability != null)
                builder.AddLayout(this.newTextBlock("Participation probability: " + Math.Round(suggestion.ParticipationProbability.Value, 3)));
            if (suggestion.PredictedScoreDividedByAverage != null)
                builder.AddLayout(this.newTextBlock("Rating: " + Math.Round(justification.Score, 3) + " (" +
                    Math.Round(suggestion.PredictedScoreDividedByAverage.Value, 3) + " x avg)"));
            builder.AddLayout(this.newTextBlock("\nExplanations:"));
            foreach (SuggestionJustification child in justification.Reasons)
            {
                builder.AddLayouts(this.renderJustification(child, 0));
            }
            this.SubLayout = ScrollLayout.New(builder.Build());
        }

        private TextblockLayout newTextBlock(string text)
        {
            return new TextblockLayout(text, this.maxFontSize);
        }
        private LayoutChoice_Set newTextBlock(string prefix, string text, int indent)
        {
            double fontSize = this.maxFontSize;
            for (int i = 0; i < indent; i++)
            {
                fontSize = Math.Ceiling(fontSize * 3 / 4);
            }
            return new Horizontal_GridLayout_Builder().AddLayout(this.newTextBlock(prefix)).AddLayout(new TextblockLayout(text, fontSize)).Build();
        }
        private string times(string value, int count)
        {
            string result = "";
            for (int i = 0; i < count; i++)
                result += value;
            return result;                
        }
        private List<LayoutChoice_Set> renderJustification(SuggestionJustification justification, int indent)
        {
            string prefix = this.times("| ", indent);

            // add any custom description
            List<LayoutChoice_Set> results = new List<LayoutChoice_Set>();
            results.Add(this.newTextBlock(prefix + ">", justification.Label, indent));
            // add the specific values
            double currentMean = Math.Round(justification.Value.Mean, 3);
            double stddev = Math.Round(justification.Value.StdDev, 3);
            double weight = Math.Round(justification.Value.Weight, 1);
            results.Add(this.newTextBlock(prefix + "::", "val = " + currentMean + " +/- " + stddev + "(" + weight + ")", indent));

            // add additional information for some specific types of justifications
            InterpolatorSuggestion_Justification interpolatorJustification = justification as InterpolatorSuggestion_Justification;
            if (interpolatorJustification != null)
            {
                double overallMean = Math.Round(interpolatorJustification.PredictionWithoutCurrentCoordinates.Mean, 3);
                if (currentMean != overallMean)
                {
                    string c;
                    if (currentMean > overallMean)
                        c = " (up from average of " + overallMean + ")";
                    else
                        c = " (down from average of " + overallMean + ")";
                    results.Add(this.newTextBlock(prefix + "??", c, indent));
                }
            }

            Composite_SuggestionJustification compositeJustification = justification as Composite_SuggestionJustification;
            if (compositeJustification != null)
            {
                if (compositeJustification.Children.Count == 1)
                {
                    // if there's exactly one child then don't bother displaying the combined value because it should match its child
                    results.RemoveAt(results.Count - 1);
                }
                foreach (SuggestionJustification child in compositeJustification.Children)
                {
                    List<LayoutChoice_Set> childLayouts = this.renderJustification(child, indent + 1);
                    results.AddRange(childLayouts);
                }
            }
            return results;
        }

    }
}
