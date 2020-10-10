using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;

namespace ActivityRecommendation.View
{
    class InheritanceEditingLayout : ContainerLayout
    {
        public InheritanceEditingLayout(ActivityDatabase activityDatabase, ProtoActivity_Database protoActivity_database, LayoutStack layoutStack)
        {
            this.activityDatabase = activityDatabase;
            this.protoActivity_database = protoActivity_database;

            this.activityCreationLayout = new ActivityCreationLayout(activityDatabase, layoutStack);
            this.inheritanceCreationLayout = new NewInheritanceLayout(activityDatabase, layoutStack);
            this.metricEditingLayout = new MetricEditingLayout(activityDatabase, layoutStack);

            this.SubLayout = new MenuLayoutBuilder(layoutStack)
                .AddLayout(
                    new AppFeatureCount_ButtonName_Provider("Enter New Activity", this.activityCreationLayout.GetFeatures()),
                    new StackEntry(this.activityCreationLayout, "Enter New Activity", null)
                )
                .AddLayout(
                    new AppFeatureCount_ButtonName_Provider("New Relationship Between Activities", this.inheritanceCreationLayout.GetFeatures()),
                    new StackEntry(this.inheritanceCreationLayout, "New Relationship", null)
                )
                .AddLayout(
                    new AppFeatureCount_ButtonName_Provider("New Completion Metric", this.metricEditingLayout.GetFeatures()),
                    new StackEntry(this.metricEditingLayout, "New Completion Metric", null)
                )
                .Build();
        }

        public List<AppFeature> GetFeatures()
        {
            List<AppFeature> features = new List<AppFeature>(this.activityCreationLayout.GetFeatures());
            features.AddRange(this.inheritanceCreationLayout.GetFeatures());
            features.AddRange(this.metricEditingLayout.GetFeatures());
            return features;
        }


        ActivityDatabase activityDatabase;
        ProtoActivity_Database protoActivity_database;

        ActivityCreationLayout activityCreationLayout;
        NewInheritanceLayout inheritanceCreationLayout;
        MetricEditingLayout metricEditingLayout;
    }
}
