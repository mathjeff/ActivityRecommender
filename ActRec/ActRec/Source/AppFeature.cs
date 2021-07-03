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

        // Whether all of the prerequisites of this feature have been completed
        bool GetIsUsable();
    }

    class AppFeatureCount_ButtonName_Provider : ValueProvider<MenuItem>
    {
        public AppFeatureCount_ButtonName_Provider(string name, List<AppFeature> features)
            : this(new ConstantValueProvider<string>(name), features)
        {
        }
        public AppFeatureCount_ButtonName_Provider(ValueProvider<string> nameProvider, List<AppFeature> features)
        {
            this.nameProvider = nameProvider;
            this.newUsableFeatures = new List<AppFeature>();
            this.lockedFeatures = features;
        }

        // ValueProvider.Get
        public MenuItem Get()
        {
            // re-ask each Feature what its status is
            this.updateStatuses();

            // some counts
            // number of features that are usable and have never been used
            int numNewUsableFeatures = this.newUsableFeatures.Count;
            // number of features that have never been used
            int numNewFeatures = numNewUsableFeatures + this.lockedFeatures.Count;
            // number of features that have been used
            int numUsedFeatures = this.numUsedFeatures;
            // number of features
            int numFeatures = numUsedFeatures + numNewFeatures;
            // number of features that can be used
            int numUsableFeatures = numUsedFeatures + numNewUsableFeatures;

            // Determine how to describe the number of unused features
            string numFeaturesLabel;
            // Determine how to describe a sample, unused feature, if any
            string sampleFeatureLabel = "";

            // Do we have any new features we should tell the user about?
            if (numNewUsableFeatures > 0)
            {
                // Determine how to count the new features
                // We have at least one feature that the user can try! How many features has the user not tried?
                if (numNewFeatures == 1)
                {
                    // We have exactly 1 new feature (at least 1 of which is usable). Are there any non-new features?
                    if (numUsedFeatures == 0)
                    {
                        // There is just 1 new feature, and there are no existing features. So, the entire screen is new to the user
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
                    // There are multiple new features in this screen (at least 1 of which is usable)
                    numFeaturesLabel = "" + numNewFeatures + " new features!";
                }
                // Determine how to give an example of an unused feature
                if (numUsedFeatures > 0)
                {
                    // The user has tried at least one feature but not all of them
                    // We list one feature that the user hasn't tried, in case they didn't realize that this feature existed
                    sampleFeatureLabel = "\n" + this.newUsableFeatures[0].GetDescription() + "?";
                }
                else
                {
                    // The user hasn't used even one feature
                    // We think our description should be good enough for the user to identify at least one feature, so we don't need to also list an example feature
                    // Extra text can be distracting, especially when it duplicates other text
                }
            }
            else
            {
                // There are no features that are both new and usable. Are there any features that are new and unusable?
                int numNewUnusableFeatures = this.lockedFeatures.Count;
                if (numNewUnusableFeatures == 0)
                {
                    // All features have been used! We don't need to inform the user about anything new
                    return new MenuItem(this.nameProvider.Get(), "");
                }
                else
                {
                    // There are no features that are both new and usable, but there are new features that are locked
                    // We tell the user about one of the locked features
                    if (numNewUnusableFeatures == 1)
                    {
                        // There is exactly 1 locked feasture
                        numFeaturesLabel = "1 locked feature!";
                    }
                    else
                    {
                        numFeaturesLabel = "" + numNewUnusableFeatures + " locked features!";
                    }
                    // Do we want to tell the user about a sample locked feature?
                    if (numUsedFeatures > 0)
                    {
                        // The user has tried at least one feature but not all of them
                        // How many features are locked?
                        if (numNewUnusableFeatures == 1)
                        {
                            // 1 feature is locked
                            sampleFeatureLabel = "\n(" + this.lockedFeatures[0].GetDescription() + ")";
                        }
                        else
                        {
                            // multiple features are locked
                            sampleFeatureLabel = "\n(" + this.lockedFeatures[0].GetDescription() + ", ...)";
                        }
                    }
                    else
                    {
                        // The user hasn't used even one feature
                        // We think our description should be good enough for the user to identify at least one feature, so we don't need to also list an example feature
                        // Extra text can be distracting, especially when it duplicates other text
                    }
                }
            }
            // Allow the user to click the menu item if it doesn't declare any features or if there is at least one usable feature
            bool buttonEnabled = (numFeatures < 1 || numUsableFeatures > 0);
            return new MenuItem(this.nameProvider.Get(), numFeaturesLabel + sampleFeatureLabel, buttonEnabled);
        }

        private void updateStatuses()
        {
            // check for features that have become usable
            for (int i = 0; i < this.lockedFeatures.Count; )
            {
                AppFeature feature = this.lockedFeatures[i];
                if (feature.GetIsUsable())
                {
                    this.newUsableFeatures.Add(feature);
                    this.lockedFeatures.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
            // check for features that have become used
            for (int i = 0; i < this.newUsableFeatures.Count; )
            {
                AppFeature feature = this.newUsableFeatures[i];
                if (feature.GetHasBeenUsed())
                {
                    this.numUsedFeatures++;
                    this.newUsableFeatures.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }

        private int numUsedFeatures;
        private ValueProvider<string> nameProvider;
        // features that can be used but that haven't yet been used
        private List<AppFeature> newUsableFeatures;
        // features that can't be used because of dependencies
        private List<AppFeature> lockedFeatures;
    }
}
