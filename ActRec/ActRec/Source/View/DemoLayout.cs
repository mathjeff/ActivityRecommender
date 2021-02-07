using ActivityRecommendation.Demo;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using VisiPlacement;
using Xamarin.Forms;

// A DemoLayout lets the user see a demo of how to use ActivityRecommender
namespace ActivityRecommendation.View
{
    class DemoLayout : ContainerLayout
    {
        public DemoLayout(ViewManager viewManager, ActivityDatabase activityDatabase)
        {
            this.viewManager = viewManager;
            this.activityDatabase = activityDatabase;

            Vertical_GridLayout_Builder builder = new Vertical_GridLayout_Builder();
            builder.AddLayout(new TextblockLayout("Usage Demo"));
            this.feedbackLabel = new TextblockLayout();
            this.feedbackLabel.setText("You probably don't want to use this feature because it will make changes to your data.");
            builder.AddLayout(this.feedbackLabel);
            Button okButton = new Button();
            okButton.Clicked += OkButton_Clicked;
            builder.AddLayout(new ButtonLayout(okButton, "See Demo!"));
            this.SubLayout = builder.Build();
        }

        private void OkButton_Clicked(object sender, EventArgs e)
        {
            this.startDemo();
        }

        private void startDemo()
        {
            this.steps = this.buildSteps();
            this.stepNumber = 0;
            this.timer = new Timer();
            this.timer.Interval = 1500;
            this.timer.Elapsed += Timer_Elapsed;
            this.timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.feedbackLabel.setText("See what it may look like to use ActivityRecommender!");
            Device.BeginInvokeOnMainThread(() =>
            {
                this.advance();
            });
        }

        private void advance()
        {
            WorkflowNode node = null;
            try
            {
                node = this.steps[stepNumber];
                node.Process();
                this.stepNumber++;
            }
            catch (Exception ex)
            {
                this.timer.Stop();
                System.Diagnostics.Debug.WriteLine("DemoLayout error processing node " + node + ": " + ex);
            }
        }

        private List<WorkflowNode> buildSteps()
        {
            List<WorkflowNode> steps = new List<WorkflowNode>();
            steps.AddRange(this.clickButton("Debugging"));
            steps.AddRange(this.clickButton("Welcome"));
            steps.AddRange(this.clickButton("Appearance"));
            steps.AddRange(this.clickButton("Color/Font"));
            steps.AddRange(this.clickButton("Dreams"));
            steps.AddRange(this.clickButton("Appearance"));
            steps.AddRange(this.clickButton("Welcome"));
            steps.AddRange(this.clickButton("Start!"));
            steps.AddRange(this.clickButton("Activities"));
            steps.AddRange(this.clickButton("Import Some Premade Activities"));
            steps.AddRange(this.clickButton("Sleeping (Activity)"));
            steps.AddRange(this.clickButton("Fun (Activity)"));
            steps.AddRange(this.clickButton("Game (Fun)"));
            steps.AddRange(this.clickButton("Reading (Activity)"));
            steps.AddRange(this.clickButton("Activities"));
            steps.AddRange(this.clickButton("Home"));
            steps.AddRange(this.clickButton("Suggest/Experiment"));
            steps.AddRange(this.clickButton("Suggest Best"));
            steps.AddRange(this.clickButton("X"));
            steps.AddRange(this.clickButton("Suggest Best"));
            steps.AddRange(this.clickButton("Doing it?"));
            steps.AddRange(this.clickButton("Start = now"));
            steps.AddRange(this.clickButton("End = now"));
            steps.AddRange(this.clickButton("OK"));
            steps.AddRange(this.clickButton("Home"));
            steps.AddRange(this.clickButton("Suggest/Experiment"));
            steps.AddRange(this.clickButton("Suggest Best"));
            steps.AddRange(this.clickButton("Suggest Best"));
            steps.AddRange(this.clickButton("Doing it?"));
            steps.AddRange(this.clickButton("End = now"));
            steps.AddRange(this.clickButton("OK"));
            return steps;
        }

        private List<WorkflowNode> clickButton(string buttonName)
        {
            return new List<WorkflowNode>() {
                new PressButton_Node(buttonName, this.viewManager),
                new ClickButton_Node(buttonName, this.viewManager)
            };
        }

        private List<WorkflowNode> steps;
        private int stepNumber;
        private Timer timer;
        private ViewManager viewManager;
        private ActivityDatabase activityDatabase;
        private TextblockLayout feedbackLabel;
    }
}
