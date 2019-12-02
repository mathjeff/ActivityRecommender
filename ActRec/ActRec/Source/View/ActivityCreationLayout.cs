using ActivityRecommendation.View;
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

            GridLayout mainGrid = GridLayout.New(new BoundProperty_List(3), BoundProperty_List.Uniform(2), LayoutScore.Zero);

            CheckBox checkbox = new CheckBox("Type = ToDo", Color.FromRgb(179, 172, 166), "Type = Category", Color.FromRgb(181, 255, 254));
            checkbox.Checked = true;

            this.typePicker = checkbox;
            mainGrid.AddLayout(ButtonLayout.WithoutBevel(checkbox));

            this.feedbackView = new Label();
            mainGrid.AddLayout(new TextblockLayout(this.feedbackView));

            this.childNameBox = new ActivityNameEntryBox("Activity Name", activityDatabase, layoutStack, true);
            this.childNameBox.AutoAcceptAutocomplete = false;
            mainGrid.AddLayout(this.childNameBox);

            this.parentNameBox = new ActivityNameEntryBox("Parent Name", activityDatabase, layoutStack);
            this.parentNameBox.AutoAcceptAutocomplete = false;
            // for first-time users, make it extra obvious that the root activity exists
            this.parentNameBox.autoselectRootActivity_if_noCustomActivities();
            mainGrid.AddLayout(this.parentNameBox);

            this.okButton = new Button();
            this.okButton.Clicked += OkButton_Clicked;
            mainGrid.AddLayout(new ButtonLayout(this.okButton, "OK"));

            LayoutChoice_Set helpWindow = (new HelpWindowBuilder()).AddMessage("This screen is for you to enter activities to do, to use as future suggestions.")
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

        // returns true if the type of Activity to create is Category (otherwise the type is ToDo)
        public bool SelectedActivityTypeIsCategory
        {
            get
            {
                return this.typePicker.Checked;
            }
            set
            {
                this.typePicker.Checked = value;
            }
        }

        public string ActivityName
        {
            get
            {
                return this.childNameBox.NameText;
            }
            set
            {
                this.childNameBox.Set_NameText(value);
            }
        }

        private void OkButton_Clicked(object sender, EventArgs e)
        {
            ActivityDescriptor childDescriptor = this.childNameBox.ActivityDescriptor;
            ActivityDescriptor parentDescriptor = this.parentNameBox.ActivityDescriptor;
            Inheritance inheritance = new Inheritance(parentDescriptor, childDescriptor);
            inheritance.DiscoveryDate = DateTime.Now;

            string error;
            if (this.SelectedActivityTypeIsCategory)
                error = this.activityDatabase.CreateCategory(inheritance);
            else
                error = this.activityDatabase.CreateToDo(inheritance);
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
        private CheckBox typePicker;
    }
}
