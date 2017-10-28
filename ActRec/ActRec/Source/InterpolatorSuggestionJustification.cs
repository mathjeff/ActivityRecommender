using AdaptiveLinearInterpolation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisiPlacement;

// Justifies a suggestion by saying that the user tends to be happy when a certain set of circumstances occur
namespace ActivityRecommendation
{
    class InterpolatorSuggestionJustification : ActivitySuggestionJustification
    {
        public InterpolatorSuggestionJustification(ActivitySuggestion suggestion)
        {
            this.Suggestion = suggestion;
        }
        public void AddInput(string label, double value, FloatRange relevantNeighborhood)
        {
            this.inputValuesByLabel[label] = value;
            this.inputNeighborhoodsByLabel[label] = relevantNeighborhood;
        }
        public void AddOutput(string label, double value)
        {
            this.outputValuesByLabel[label] = value;
        }
        public override string Summarize()
        {
            string result = "";
            string when = "At " + this.Suggestion.StartDate + ",\n";
            string inputs = "Having\n";
            foreach (String key in this.inputValuesByLabel.Keys)
            {
                double value = this.inputValuesByLabel[key];
                FloatRange neighborhood = this.inputNeighborhoodsByLabel[key];
                inputs += key + " = " + value + ", in " + neighborhood.ToString() + "\n";
            }
            string outputs = "Predict:\n";
            foreach (KeyValuePair<string, double> item in this.outputValuesByLabel)
            {
                outputs += item.Key + " = " + item.Value + "\n";
            }
            result = when + inputs + outputs;
            return result;
        }

        public override LayoutChoice_Set Visualize()
        {
            throw new NotImplementedException();
        }

        // the value of each input for the suggestion
        Dictionary<string, double> inputValuesByLabel = new Dictionary<string, double>();
        // the width of the neighborhood in each input
        Dictionary<string, FloatRange> inputNeighborhoodsByLabel = new Dictionary<string, FloatRange>();
        // the value of each output
        Dictionary<string, double> outputValuesByLabel = new Dictionary<string, double>();
        //int neighborhoodCount = 0; // number of datapoints from which this suggestion was made
    }
}
