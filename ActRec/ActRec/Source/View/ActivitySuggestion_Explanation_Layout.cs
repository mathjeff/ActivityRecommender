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
            builder.AddLayout(this.newTextBlock("I suggested " + suggestion.ActivityDescriptor.ActivityName + " at " +
                explanation.Suggestion.StartDate.ToString("HH:mm") + "\n"));
            builder.AddLayout(this.newTextBlock("I had time to consider " + suggestion.NumActivitiesConsidered + " activities."));
            if (suggestion.ParticipationProbability != null)
                builder.AddLayout(this.newTextBlock("Participation probability: " + Math.Round(suggestion.ParticipationProbability.Value, 3) + "\n"));
            if (suggestion.PredictedScoreDividedByAverage != null)
                builder.AddLayout(this.newTextBlock("Rating: " + Math.Round(explanation.Score, 3) + " (" +
                    Math.Round(suggestion.PredictedScoreDividedByAverage.Value, 3) + " x avg)\n"));

            Button explainQuality_button = new Button();
            explainQuality_button.Clicked += ExplainQuality_button_Clicked;
            this.suggestionQuality_container = new ContainerLayout();
            builder.AddLayout(this.suggestionQuality_container);
            this.suggestionQuality_container.SubLayout = new ButtonLayout(explainQuality_button, "Suggestion quality: " + Math.Round(explanation.SuggestionValue, 3));

            this.SubLayout = ScrollLayout.New(builder.Build());
        }

        private void ExplainQuality_button_Clicked(object sender, EventArgs e)
        {
            this.explainSuggestionQuality();
        }

        private void explainSuggestionQuality()
        {
            Vertical_GridLayout_Builder builder = new Vertical_GridLayout_Builder();
            builder.AddLayout(new TextblockLayout("Why:", this.maxFontSize));
            builder.AddLayout(new TextblockLayout("Suggestion quality: " + Math.Round(explanation.SuggestionValue, 3), this.maxFontSize));
            int childIndex = 0;
            foreach (Justification child in this.explanation.Reasons)
            {
                builder.AddLayouts(this.renderJustification(child, 0, this.explanation.Suggestion.ActivityDescriptor, childIndex));
                childIndex++;
            }
            this.suggestionQuality_container.SubLayout = builder.BuildAnyLayout();
        }

        private TextblockLayout newTextBlock(string text)
        {
            return new TextblockLayout(text, this.maxFontSize);
        }
        private LayoutChoice_Set newTextBlock(string prefix, string indexString, string text, int indent)
        {
            Horizontal_GridLayout_Builder builder = new Horizontal_GridLayout_Builder();
            if (prefix.Length > 0)
            {
                TextblockLayout prefixBlock = new TextblockLayout(prefix, this.maxFontSize, false, false);
                prefixBlock.setTextColor(Color.FromRgba(0, 0, 0, 0));
                builder.AddLayout(prefixBlock);
            }

            double fontSize = this.maxFontSize;
            for (int i = 0; i < indent; i++)
            {
                fontSize = Math.Ceiling(fontSize * 4 / 5);
            }
            TextblockLayout indexLayout = new TextblockLayout(indexString, fontSize);
            indexLayout.AlignVertically(TextAlignment.Start);
            builder.AddLayout(indexLayout);

            TextblockLayout contentBlock = new TextblockLayout(text, fontSize);
            contentBlock.AlignVertically(TextAlignment.Start);
            builder.AddLayout(contentBlock);

            return builder.BuildAnyLayout();
        }
        private string times(string value, int count)
        {
            string result = "";
            for (int i = 0; i < count; i++)
                result += value;
            return result;                
        }
        private List<LayoutChoice_Set> renderJustification(Justification justification, int indent, ActivityDescriptor activityDescriptor, int indexInParent)
        {
            string prefix = this.times("....", indent);

            // add any custom description
            List<LayoutChoice_Set> results = new List<LayoutChoice_Set>();
            // add the specific values
            double currentMean = Math.Round(justification.Value.Mean, 3);
            double stddev = Math.Round(justification.Value.StdDev, 3);
            double weight = Math.Round(justification.Value.Weight, 1);
            string whatText = this.getLabel(justification, activityDescriptor) + " = " + currentMean + " +/- " + stddev + " (weight = " + weight + ")";

            // add additional information for some specific types of justifications
            InterpolatorSuggestion_Justification interpolatorJustification = justification as InterpolatorSuggestion_Justification;
            // determine the appropriate list index to show
            string indexString;
            if (indent % 2 == 0)
                indexString = "" + (indexInParent + 1);
            else
                indexString = "abcdefghijklmnopqrstuvwxyz".Substring(indexInParent, 1);
            results.Add(this.newTextBlock(prefix, indexString + ": ", whatText, indent));

            Composite_SuggestionJustification compositeJustification = justification as Composite_SuggestionJustification;
            if (compositeJustification != null)
            {
                // if there are multiple children, explain that this value was computed based on the children
                int childIndent = indent + 1;
                // We show non-composite children first to make the output easier to read
                List<Justification> compositeChildren = new List<Justification>();
                List<Justification> plainChildren = new List<Justification>();
                foreach (Justification child in compositeJustification.Children)
                {
                    if (child is Composite_SuggestionJustification)
                        compositeChildren.Add(child);
                    else
                        plainChildren.Add(child);
                }
                int childIndex = 0;
                foreach (Justification child in plainChildren.Concat(compositeChildren))
                {
                    List<LayoutChoice_Set> childLayouts = this.renderJustification(child, childIndent, activityDescriptor, childIndex);
                    childIndex++;
                    results.AddRange(childLayouts);
                }
            }
            return results;
        }

        // Given a Justification, return an appropriate label string, replacing the activity name with "this" if appropriate
        private string getLabel(Justification justification, ActivityDescriptor activityDescriptor)
        {
            string text = justification.Label;
            string name = activityDescriptor.ActivityName;
            // In practice, the activity name should only ever appear once in this label.
            // This check is just to make sure that if the user types a weird activity name like "How", then a label like "How much you should enjoy How" doesn't turn into "this much you should enjoy this".
            // Although it would be more robust to pass around message builders and resolve the final message here, that would also be more confusing. It is nice for the justifications to simply be strings.
            // This check isn't completely perfect because it doesn't account for cases where the activity name doesn't appear in the justification at all,
            // and it can also get confused if the justification contains an activity name that contains this activity name,
            // but in practice this should be fine, and even if it works incorrectly it just creates a confusing message.
            if (this.containsExactlyOneInstance(text, name))
                return text.Replace(name, "this");
            return text;
        }

        private bool containsExactlyOneInstance(string longer, string shorter)
        {
            int firstIndex = longer.IndexOf(shorter);
            if (firstIndex < 0)
                return false;
            if (longer.IndexOf(shorter, firstIndex + 1) >= 0)
                return false;
            return true;
        }

        private ContainerLayout suggestionQuality_container;
        private ActivitySuggestion_Explanation explanation;
    }
}
