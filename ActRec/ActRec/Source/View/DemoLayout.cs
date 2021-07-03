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
            this.requestStartDemo();
        }

        private void requestStartDemo()
        {
            if (this.activityDatabase.ContainsCustomActivity())
            {
                this.feedbackLabel.setText("Running the demo requires having no activities, to ensure that all of the expected buttons exist. Sorry!");
            }
            else
            {
                this.startDemo();
            }
        }
        private void startDemo()
        {
            this.steps = this.buildSteps();
            this.stepNumber = 0;
            this.viewManager.LayoutCompleted += ViewManager_LayoutCompleted;
            this.advance();
        }

        private void advance()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                this.advanceOnThisThread();
            });
        }

        private void advanceOnThisThread()
        {
            this.feedbackLabel.setText("See what it may look like to use ActivityRecommender!");
            this.timer = null;
            WorkflowNode node = null;
            try
            {
                node = this.steps[stepNumber];
                node.Process();
                if (this.viewManager.NeedsRelayout)
                {
                    System.Diagnostics.Debug.WriteLine("DemoLayout waiting for ViewManager to complete layout");
                }
                else
                {
                    this.sleepAndStep();
                }
                this.stepNumber++;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("DemoLayout error processing node " + node + ": " + ex);
                this.viewManager.LayoutCompleted -= ViewManager_LayoutCompleted;
            }
        }

        private void ViewManager_LayoutCompleted(ViewManager_LayoutStats layoutStats)
        {
            System.Diagnostics.Debug.Write("DemoLayout sees ViewManager completed layout; waiting slightly before advancing");
            this.sleepAndStep();
        }

        private void sleepAndStep()
        {
            System.Diagnostics.Debug.Write("DemoLayout sleepAndStep");
            Timer timer = this.timer;
            if (timer != null)
            {
                System.Diagnostics.Debug.WriteLine("DemoLayout cancelling existing timer");
                timer.Stop();
            }
            timer = new Timer();
            timer.AutoReset = false;
            timer.Interval = 1000;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.advance();
        }

        private List<WorkflowNode> buildSteps()
        {
            List<WorkflowNode> steps = new List<WorkflowNode>();
            steps.AddRange(this.clickButton("Debugging"));
            steps.AddRange(this.clickButton("Welcome"));
            steps.AddRange(this.clickButton("Appearance"));
            steps.AddRange(this.clickButton("Color/Font"));
            steps.AddRange(this.clickButton("Vampire"));
            steps.AddRange(this.clickButton("Appearance"));
            steps.AddRange(this.clickButton("Welcome"));
            steps.AddRange(this.clickButton("Start!"));
            steps.AddRange(this.clickButton("Organize Activities"));
            steps.AddRange(this.clickButton("Import Some Premade Activities"));
            steps.AddRange(this.clickButton("I like this!"));
            steps.AddRange(this.clickButton("I like all kinds! (17 ideas)"));
            steps.AddRange(this.clickButton("I like all kinds! (60 ideas)"));
            steps.AddRange(this.clickButton("Organize Activities"));
            steps.AddRange(this.clickButton("Home"));
            steps.AddRange(this.clickButton("Suggest/Experiment"));
            steps.AddRange(this.clickButton("Suggest"));
            steps.AddRange(this.clickButton("X"));
            steps.AddRange(this.clickButton("Suggest"));
            steps.AddRange(this.clickButton("Doing it?"));
            steps.AddRange(this.clickButton("Start = now"));
            steps.AddRange(this.clickButton("End = now"));
            steps.AddRange(this.clickButton("OK"));
            steps.AddRange(this.clickButton("Home"));
            steps.AddRange(this.clickButton("Suggest/Experiment"));
            steps.AddRange(this.clickButton("Suggest"));
            steps.AddRange(this.clickButton("Suggest"));
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
