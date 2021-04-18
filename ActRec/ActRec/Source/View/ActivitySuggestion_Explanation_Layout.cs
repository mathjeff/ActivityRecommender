using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    public class ActivitySuggestion_Explanation_Layout : ContainerLayout
    {
        private int maxFontSize = 30;
        public ActivitySuggestion_Explanation_Layout(ActivitySuggestion_Explanation explanation)
        {
            this.explanation = explanation;
            ActivitySuggestion suggestion = explanation.Suggestion;
            Vertical_GridLayout_Builder builder = new Vertical_GridLayout_Builder();
            builder.AddLayout(this.newTextBlock("Why I suggested " + suggestion.ActivityDescriptor.ActivityName + " at " +
                explanation.Suggestion.StartDate.ToString("HH:mm") + "\n"));
            builder.AddLayout(this.newTextBlock("I had time to consider " + suggestion.NumActivitiesConsidered + " activities"));
            if (suggestion.ParticipationProbability != null)
                builder.AddLayout(this.newTextBlock("Participation probability: " + Math.Round(suggestion.ParticipationProbability.Value, 3) + "\n"));
            if (suggestion.PredictedScoreDividedByAverage != null)
                builder.AddLayout(this.newTextBlock("Rating: " + Math.Round(explanation.Score, 3) + " (" +
                    Math.Round(suggestion.PredictedScoreDividedByAverage.Value, 3) + " x avg)\n"));

            Button explainQuality_button = new Button();
            explainQuality_button.Clicked += ExplainQuality_button_Clicked;
            explainQuality_button.Text = "Suggestion quality: " + Math.Round(explanation.SuggestionValue, 3);
            this.suggestionQuality_container = new ContainerLayout();
            builder.AddLayout(this.suggestionQuality_container);
            this.suggestionQuality_container.SubLayout = new ButtonLayout(explainQuality_button);

            this.SubLayout = ScrollLayout.New(builder.Build());
        }

        private void ExplainQuality_button_Clicked(object sender, EventArgs e)
        {
            this.explainSuggestionQuality();
        }

        private void explainSuggestionQuality()
        {
            Vertical_GridLayout_Builder builder = new Vertical_GridLayout_Builder();
            builder.AddLayout(new TextblockLayout("Suggestion quality: " + Math.Round(explanation.SuggestionValue, 3)));
            foreach (Justification child in this.explanation.Reasons)
            {
                builder.AddLayouts(this.renderJustification(child, 1));
            }
            this.suggestionQuality_container.SubLayout = builder.BuildAnyLayout();
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
        private List<LayoutChoice_Set> renderJustification(Justification justification, int indent)
        {
            string prefix = this.times("| ", indent - 1);

            // add any custom description
            List<LayoutChoice_Set> results = new List<LayoutChoice_Set>();
            // add the specific values
            double currentMean = Math.Round(justification.Value.Mean, 3);
            double stddev = Math.Round(justification.Value.StdDev, 3);
            double weight = Math.Round(justification.Value.Weight, 1);
            string whatText = justification.Label + " = " + currentMean + " +/- " + stddev + " (weight = " + weight + ")";

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
                    whatText += c;
                }
            }
            results.Add(this.newTextBlock(prefix + "|-", whatText, indent));

            Composite_SuggestionJustification compositeJustification = justification as Composite_SuggestionJustification;
            if (compositeJustification != null)
            {
                // if there are multiple children, explain that this value was computed based on the children
                int childIndent;
                results.Add(this.newTextBlock(prefix + "|\\- Why:", "" + compositeJustification.Children.Count + " reasons:", indent));
                childIndent = indent + 1;
                foreach (Justification child in compositeJustification.Children)
                {
                    List<LayoutChoice_Set> childLayouts = this.renderJustification(child, childIndent);
                    results.AddRange(childLayouts);
                }
            }
            return results;
        }

        private ContainerLayout suggestionQuality_container;
        private ActivitySuggestion_Explanation explanation;
    }
}
