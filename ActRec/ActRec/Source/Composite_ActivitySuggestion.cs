using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// the Composite_ActivitySuggestion class is essentially a tree of suggestions for what the user should do
namespace ActivityRecommendation
{
    class Composite_ActivitySuggestion : ActivitySuggestion
    {
        public Composite_ActivitySuggestion(ActivityDescriptor activityDescriptor, List<ActivitySuggestion> childSuggestions)
            : base(activityDescriptor)
        {
            if (childSuggestions != null)
                this.ChildSuggestions = childSuggestions;
            else
                this.ChildSuggestions = new List<ActivitySuggestion>();
        }
        // different ideas for what the user should do next
        public List<ActivitySuggestion> ChildSuggestions { get; set; }
        public void AddChild(ActivitySuggestion newChild)
        {
            this.ChildSuggestions.Add(newChild);
        }
        public override int CountNumLeaves()
        {
            int count = 0;
            foreach (ActivitySuggestion suggestion in this.ChildSuggestions)
            {
                count += suggestion.CountNumLeaves();
            }
            if (count == 0)
            {
                if (this.ActivityDescriptor != null)
                    count++;
            }
            return count;
        }
        public override int CountNumLevelsFromLeaf()
        {
            int numLevels = -1;
            foreach (ActivitySuggestion suggestion in this.ChildSuggestions)
            {
                numLevels = Math.Max(suggestion.CountNumLevelsFromLeaf(), numLevels);
            }
            return numLevels + 1;

        }
    }
}
