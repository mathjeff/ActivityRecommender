using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;

namespace ActivityRecommendation.View
{
    class InheritanceEditingLayout : ContainerLayout
    {
        public InheritanceEditingLayout(ActivityDatabase activityDatabase, LayoutStack layoutStack)
        {
            this.activityDatabase = activityDatabase;

            this.createCategory_layout = new CreateCategory_Layout(activityDatabase, layoutStack);
            this.createToDo_layout = new CreateToDo_Layout(activityDatabase, layoutStack);
            this.createProblem_layout = new CreateProblem_Layout(activityDatabase, layoutStack);
            this.createSolution_layout = new CreateSolution_Layout(activityDatabase, layoutStack);

            this.inheritanceCreationLayout = new NewInheritanceLayout(activityDatabase, layoutStack);
            this.metricEditingLayout = new MetricEditingLayout(activityDatabase, layoutStack);

            this.SubLayout = new MenuLayoutBuilder(layoutStack)
                .AddLayout(
                    new AppFeatureCount_ButtonName_Provider("New Category", this.createCategory_layout.GetFeatures()),
                    new StackEntry(this.createCategory_layout, "New Category", null)
                )
                .AddLayout(
                    new AppFeatureCount_ButtonName_Provider("New ToDo", this.createToDo_layout.GetFeatures()),
                    new StackEntry(this.createToDo_layout, "Enter New Activity", null)
                )
                .AddLayout(
                    new AppFeatureCount_ButtonName_Provider("New Problem", this.createProblem_layout.GetFeatures()),
                    new StackEntry(this.createProblem_layout, "New Problem", null)
                )
                .AddLayout(
                    new AppFeatureCount_ButtonName_Provider("New Solution", this.createSolution_layout.GetFeatures()),
                    new StackEntry(this.createSolution_layout, "New Solution", null)
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
            List<AppFeature> features = new List<AppFeature>();
            features.AddRange(this.createCategory_layout.GetFeatures());
            features.AddRange(this.createToDo_layout.GetFeatures());
            features.AddRange(this.createProblem_layout.GetFeatures());
            features.AddRange(this.createSolution_layout.GetFeatures());
            features.AddRange(this.inheritanceCreationLayout.GetFeatures());
            features.AddRange(this.metricEditingLayout.GetFeatures());
            return features;
        }

        public string ChildName
        {
            set
            {
                this.createToDo_layout.ActivityName = value;
                this.createCategory_layout.ActivityName = value;
                this.createProblem_layout.ActivityName = value;
                this.createSolution_layout.ActivityName = value;
            }
        }


        ActivityDatabase activityDatabase;

        CreateProblem_Layout createProblem_layout;
        CreateToDo_Layout createToDo_layout;
        CreateCategory_Layout createCategory_layout;
        CreateSolution_Layout createSolution_layout;

        NewInheritanceLayout inheritanceCreationLayout;
        MetricEditingLayout metricEditingLayout;
    }
}
