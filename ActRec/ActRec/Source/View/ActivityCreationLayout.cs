using System;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation
{
    class ActivityCreationLayout : TitledControl
    {
        public ActivityCreationLayout(ActivityDatabase activityDatabase, LayoutStack layoutStack)
        {
            this.activityDatabase = activityDatabase;
            this.layoutStack = layoutStack;

            this.SetTitle("Add New Activity to Choose From");

            GridLayout mainGrid = GridLayout.New(BoundProperty_List.Uniform(3), BoundProperty_List.Uniform(2), LayoutScore.Zero);

            Picker picker = new Picker();
            picker.Items.Add("Category");
            picker.Items.Add("ToDo");
            picker.Title = "Type";
            picker.SelectedItem = "Category";
            mainGrid.AddLayout(new PickerLayout(picker));

            this.feedbackView = new Label();
            mainGrid.AddLayout(new TextblockLayout(this.feedbackView));

            this.childNameBox = new ActivityNameEntryBox("Activity Name", true);
            this.childNameBox.Database = activityDatabase;
            this.childNameBox.AutoAcceptAutocomplete = false;
            mainGrid.AddLayout(this.childNameBox);

            this.parentNameBox = new ActivityNameEntryBox("Parent Name");
            this.parentNameBox.Database = activityDatabase;
            this.parentNameBox.AutoAcceptAutocomplete = false;
            mainGrid.AddLayout(this.parentNameBox);

            this.okButton = new Button();
            this.okButton.Clicked += OkButton_Clicked;
            mainGrid.AddLayout(new LayoutCache(new ButtonLayout(this.okButton, "OK")));

            LayoutChoice_Set helpWindow = (new HelpWindowBuilder()).AddMessage("This page is for you to enter activities to do, to use as future suggestions.")
                .AddMessage("In the left text box, choose a name for the activity.")
                .AddMessage("In the right text box, specify another activity to assign as its parent.")
                .AddMessage("For example, you might specify that Gaming is a child activity of the Fun activity. Grouping activities like this is helpful for two reasons. It gives " +
                "ActivityRecommender more understanding about the relationships between activities and can help it to notice trends. It also means that you can later request a suggestion " +
                "from within Activity \"Fun\" and ActivityRecommender will know what you mean, and might suggest \"Gaming\".")
                .AddMessage("If you haven't created the parent activity yet, you'll have to create it first. The only activity that exists at the beginning is the built-in activity " +
                "named \"Activity\".")
                .AddMessage("While typing you can press Enter to fill in the autocomplete suggestion.")
                .AddMessage("If the thing you're creating is something you plan to do many times (or even if you want it to be able to be the parent of another Activity), then select the type " +
                "Category. For example, Sleeping would be a Category.")
                .AddMessage("If the thing you're creating is something you plan to complete once and don't plan to do again, then select the type ToDo. For example, \"Reading " +
                "ActivityRecommender's Built-In Features Overview\" would be a ToDo.")
                .Build();

            HelpButtonLayout helpLayout = new HelpButtonLayout(helpWindow, layoutStack);

            mainGrid.AddLayout(helpLayout);


            this.SetContent(mainGrid);
        }

        private void OkButton_Clicked(object sender, EventArgs e)
        {
            ActivityDescriptor childDescriptor = this.childNameBox.ActivityDescriptor;
            ActivityDescriptor parentDescriptor = this.parentNameBox.ActivityDescriptor;
            Inheritance inheritance = new Inheritance(parentDescriptor, childDescriptor);
            inheritance.DiscoveryDate = DateTime.Now;

            string error = this.activityDatabase.CreateActivity(inheritance);
            if (error == "")
            {
                this.childNameBox.Clear();
                this.parentNameBox.Clear();
                this.feedbackView.Text = "Created " + childDescriptor.ActivityName;
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
