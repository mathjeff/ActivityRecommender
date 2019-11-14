using ActivityRecommendation.Effectiveness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    // a MetricEditingLayout allows the user to edit Metrics for an Activity
    class MetricEditingLayout : ContainerLayout
    {
        public MetricEditingLayout(ActivityDatabase activityDatabase, LayoutStack layoutStack)
        {
            this.activityDatabase = activityDatabase;
            this.nameBox = new ActivityNameEntryBox("Activity", activityDatabase, layoutStack);
            Button okButton = new Button();
            okButton.Clicked += OkButton_Clicked;
            this.metricBox = new TitledTextbox("Metric Name");
            this.errorMessageHolder = new Label();

            LayoutChoice_Set helpWindow = new HelpWindowBuilder()
                .AddMessage("This screen lets you add a Metric to an existing activity.")
                .AddMessage("A metric is a way of measuring how well a participation accomplishes a goal.")
                .AddMessage("At the moment, ActivityRecommender only supports adding Metrics that classify a participation as a success or failure.")
                .AddMessage("(You record the success/failure status when you record having participated in the Activity)")
                .AddMessage("For example, if you have an Activity named Making Food, one possible metric would be 'Make 1 gallon of smoothie'.")
                .AddMessage("Alternatively, if you have a computer game you'd like to beat, another possible metric would be 'Beat 1 Level'.")
                .AddMessage("The reason you might want to create a Metric is to allow ActivityRecommender to know it can measure your effectiveness on this task.")
                .AddMessage("Any Activity with a Metric is eligible to take part in effectiveness experiments.")
                .AddMessage("You can only assign one metric to any Activity at the moment. If you want to assign more, you may want to make sub-activities instead.")
                .AddMessage("Also note that any Activity of type ToDo already starts with a built-in metric, which is to complete the ToDo.")
                .Build();
            
            GridLayout mainGrid = GridLayout.New(new BoundProperty_List(3), new BoundProperty_List(1), LayoutScore.Zero);
            mainGrid.AddLayout(new TextblockLayout("Add Metric to Existing Activity"));
            mainGrid.AddLayout(new TextblockLayout(this.errorMessageHolder));

            GridLayout bottomGrid = GridLayout.New(BoundProperty_List.Uniform(2), BoundProperty_List.Uniform(2), LayoutScore.Zero);
            bottomGrid.AddLayout(this.metricBox);
            bottomGrid.AddLayout(this.nameBox);
            bottomGrid.AddLayout(new ButtonLayout(okButton, "OK"));
            bottomGrid.AddLayout(new HelpButtonLayout(helpWindow, layoutStack));

            mainGrid.AddLayout(bottomGrid);

            this.SubLayout = mainGrid;
        }

        private void OkButton_Clicked(object sender, EventArgs e)
        {
            Activity activity = this.nameBox.Activity;
            if (activity == null)
            {
                this.setError("Activity \"" + this.nameBox.NameText + "\" does not exist");
                return;
            }

            string metricName = this.metricBox.Text;
            if (metricName == null || metricName == "")
            {
                this.setError("Metric name is required");
                return;
            }
            Metric metric = new CompletionMetric(metricName, activity);
            string error = this.activityDatabase.AddMetric(activity, metric);
            if (error != "")
                this.setError(error);
            else
                this.clear();
        }
        private void setError(string error)
        {
            this.errorMessageHolder.Text = error;
        }
        private void clear()
        {
            this.setError("");
            this.nameBox.Set_NameText("");
            this.metricBox.Text = "";
        }

        private ActivityNameEntryBox nameBox;
        private TitledTextbox metricBox;
        private ActivityDatabase activityDatabase;
        private Label errorMessageHolder;
    }
}
