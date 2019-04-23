using ActivityRecommendation.View;
using System;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation
{
    class InheritanceEditingLayout : TitledControl
    {
        public InheritanceEditingLayout(ActivityDatabase activityDatabase, LayoutStack layoutStack)
        {
            this.activityDatabase = activityDatabase;
            this.layoutStack = layoutStack;

            this.SetTitle("Relate Two Existing Activities");

            GridLayout bottomGrid = GridLayout.New(BoundProperty_List.Uniform(2), BoundProperty_List.Uniform(2), LayoutScore.Zero);

            this.childNameBox = new ActivityNameEntryBox("Activity Name");
            this.childNameBox.Database = activityDatabase;
            this.childNameBox.AutoAcceptAutocomplete = false;
            bottomGrid.AddLayout(this.childNameBox);

            this.parentNameBox = new ActivityNameEntryBox("Parent Name");
            this.parentNameBox.Database = activityDatabase;
            this.parentNameBox.AutoAcceptAutocomplete = false;
            bottomGrid.AddLayout(this.parentNameBox);

            this.okButton = new Button();
            this.okButton.Clicked += OkButton_Clicked;
            bottomGrid.AddLayout(new LayoutCache(new ButtonLayout(this.okButton, "OK")));


            LayoutChoice_Set helpWindow = (new HelpWindowBuilder()).AddMessage("This screen is for you to enter activities, to use as future suggestions.")
                .AddMessage("The text box on the left is where you type the activity name.")
                .AddMessage("The text box on the right is where you type another activity that you want to make be a parent of the given activity.")
                .AddMessage("For example, you might specify that Gaming is a child activity of the Fun activity. Grouping activities like this is helpful for two reasons. It gives " +
                "ActivityRecommender more understanding about the relationships between activities and can help it to notice trends. It also means that you can later request a suggestion " +
                "from within Activity \"Fun\" and ActivityRecommender will know what you mean, and might suggest \"Gaming\".")
                .AddMessage("If you haven't created the parent activity yet, you'll have to create it first. The only activity that exists at the beginning is the built-in activity " +
                "named \"Activity\".")
                .AddMessage("While typing you can press Enter to fill in the autocomplete suggestion.")
                .Build();

            HelpButtonLayout helpLayout = new HelpButtonLayout(helpWindow, layoutStack);

            bottomGrid.AddLayout(helpLayout);

            this.feedbackView = new Label();

            GridLayout mainGrid = new Vertical_GridLayout_Builder()
                .AddLayout(new TextblockLayout(this.feedbackView))
                .AddLayout(bottomGrid)
                .Build();

            this.SetContent(mainGrid);
        }

        private void OkButton_Clicked(object sender, EventArgs e)
        {
            ActivityDescriptor childDescriptor = this.childNameBox.ActivityDescriptor;
            ActivityDescriptor parentDescriptor = this.parentNameBox.ActivityDescriptor;
            Inheritance inheritance = new Inheritance(parentDescriptor, childDescriptor);
            inheritance.DiscoveryDate = DateTime.Now;

            string error = this.activityDatabase.AddParent(inheritance);
            if (error == "")
            {
                this.childNameBox.Clear();
                this.parentNameBox.Clear();
                this.feedbackView.Text = childDescriptor.ActivityName + " now inherits from " + parentDescriptor.ActivityName;
            }
            else
            {
                this.feedbackView.Text = error;
            }
        }

        private ActivityNameEntryBox childNameBox;
        private ActivityNameEntryBox parentNameBox;
        private Button okButton;
        private LayoutStack layoutStack;
        private ActivityDatabase activityDatabase;
        private Label feedbackView;
    }
}
