using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;

namespace ActivityRecommendation
{
    interface AppFeature
    {
        // A description of this feature, like "Entering an Activity"
        string GetDescription();
        // Whether the user has used this feature
        bool GetHasBeenUsed();
        // Other features that must be used before this one
        // List<AppFeature> GetDependencies();
    }

    class AppFeatureCount_ButtonName_Provider : ValueProvider<MenuItem>
    {
        public AppFeatureCount_ButtonName_Provider(string name, List<AppFeature> features)
        {
            this.name = name;
            this.unusedFeatures = features;
        }

        // ValueProvider.Get
        public MenuItem Get()
        {
            string subtitle;

            int numUnusedFeatures = this.NumUnusedFeatures;
            if (numUnusedFeatures == 0)
            {
                subtitle = "";
            }
            else
            {
                string numFeaturesLabel = "";
                string sampleFeatureLabel = "";
                if (unusedFeatures.Count > 0)
                {
                    // An unused feature exists
                    if (unusedFeatures.Count == 1)
                    {
                        if (this.numUsedFeatures == 0)
                        {
                            // There is 1 new feature, and no existing features. So, the entire screen is new to the user
                            numFeaturesLabel = "New!";
                        }
                        else
                        {
                            // There is 1 new feature in a screen the user has already used before
                            numFeaturesLabel = "1 new feature!";
                        }
                    }
                    else
                    {
                        // There are multiple new features in this screen
                        numFeaturesLabel = "" + unusedFeatures.Count + " new features!";
                    }
                    if (this.numUsedFeatures > 0)
                    {
                        // The user has tried at least one feature but not all of them
                        // We list one feature that the user hasn't tried, in case they didn't realize that this feature existed
                        sampleFeatureLabel = "\n" + unusedFeatures[0].GetDescription() + "?";
                    }
                    else
                    {
                        // The user hasn't used even one feature
                        // We think our description should be good enough for the user to identify at least one feature, so we don't need to also list an example feature
                        // Extra text can be distracting, especially when it duplicates other text
                    }
                }
                subtitle = numFeaturesLabel + sampleFeatureLabel;
            }
            return new MenuItem(this.name, subtitle);
        }

        private int NumUnusedFeatures
        {
            get
            {
                for (int i = this.unusedFeatures.Count - 1; i >= 0; i--)
                {
                    AppFeature feature = this.unusedFeatures[i];
                    if (feature.GetHasBeenUsed())
                    {
                        this.numUsedFeatures++;
                        this.unusedFeatures.RemoveAt(i);
                    }
                }
                return this.unusedFeatures.Count;
            }
        }

        private int numUsedFeatures;
        private string name;
        private List<AppFeature> unusedFeatures;
    }
}
