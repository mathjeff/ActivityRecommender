using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;

namespace ActivityRecommendation.View
{
    class ActivityEditingLayout : ContainerLayout
    {
        public ActivityEditingLayout(ActivityDatabase activityDatabase, LayoutStack layoutStack)
        {
            this.activityDatabase = activityDatabase;

            this.activityCreationLayout = new ActivityCreationLayout(activityDatabase, layoutStack);
            this.inheritanceCreationLayout = new NewInheritanceLayout(activityDatabase, layoutStack);
            this.metricEditingLayout = new MetricEditingLayout(activityDatabase, layoutStack);

            LayoutChoice_Set helpLayout = (new HelpWindowBuilder()
                .AddMessage("If you want to assign an activity as the child of multiple parents, you can do that here.")
                .AddMessage("Additionally, if you plan to ask ActivityRecommender to measure how quickly (your Effectiveness) you complete various Activities, you have to enter a " +
                "Metric for those activities here, so ActivityRecommender can know that it makes sense to measure (for example, it wouldn't make sense to measure how quickly you sleep at " +
                "once: it wouldn't count as twice effective to do two sleeps of half duration each).")
                .AddMessage("To undo, remove, or modify an entry, you have to edit the data file directly. Go back to the Export screen and export all of your data as a .txt file. " +
                "Then make some changes, and go to the Import screen to load your changed file.")
                .AddLayout(new CreditsButtonBuilder(layoutStack)
                    .AddContribution(ActRecContributor.CORY_JALBERT, new DateTime(2017, 12, 14), "Suggested having pre-chosen activities available for easy import")
                    .AddContribution(ActRecContributor.ANNI_ZHANG, new DateTime(2020, 3, 8), "Pointed out that linebreaks in buttons didn't work correctly on iOS")
                    .Build()
                )
                .Build()
            );

            this.SubLayout = new MenuLayoutBuilder(layoutStack)
                .AddLayout(
                    new AppFeatureCount_ButtonName_Provider("New Relationship Between Activities", this.inheritanceCreationLayout.GetFeatures()),
                    new StackEntry(this.inheritanceCreationLayout, "New Relationship", null)
                )
                .AddLayout(
                    new AppFeatureCount_ButtonName_Provider("New Completion Metric", this.metricEditingLayout.GetFeatures()),
                    new StackEntry(this.metricEditingLayout, "New Completion Metric", null)
                )
                .AddLayout("Help", helpLayout)
                .Build();
        }

        public List<AppFeature> GetFeatures()
        {
            List<AppFeature> features = new List<AppFeature>();
            features.AddRange(this.inheritanceCreationLayout.GetFeatures());
            features.AddRange(this.metricEditingLayout.GetFeatures());
            return features;
        }

        ActivityDatabase activityDatabase;

        ActivityCreationLayout activityCreationLayout;
        NewInheritanceLayout inheritanceCreationLayout;
        MetricEditingLayout metricEditingLayout;
        LayoutChoice_Set activityDeletionLayout;
    }
}
