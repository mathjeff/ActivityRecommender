﻿using ActivityRecommendation.View;
using System;
using System.Collections.Generic;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    abstract class ActivityCreationLayout : TitledControl
    {
        public ActivityCreationLayout(ActivityDatabase activityDatabase, LayoutStack layoutStack)
        {
            this.activityDatabase = activityDatabase;
            this.layoutStack = layoutStack;

            this.SetTitle("Add New Activity to Choose From");

            GridLayout mainGrid = GridLayout.New(new BoundProperty_List(3), new BoundProperty_List(1), LayoutScore.Zero);

            mainGrid.AddLayout(new TextblockLayout(this.getShortExplanation()));

            this.feedbackLayout = new TextblockLayout();
            mainGrid.AddLayout(this.feedbackLayout);

            GridLayout entryLayout = GridLayout.New(new BoundProperty_List(2), BoundProperty_List.Uniform(2), LayoutScore.Zero);
            mainGrid.AddLayout(entryLayout);

            this.childNameBox = new ActivityNameEntryBox("Activity Name", activityDatabase, layoutStack, true);
            this.childNameBox.AutoAcceptAutocomplete = false;
            entryLayout.AddLayout(this.childNameBox);

            this.parentNameBox = new ActivityNameEntryBox("Parent Name", activityDatabase, layoutStack);
            this.parentNameBox.AutoAcceptAutocomplete = false;
            // for first-time users, make it extra obvious that the root activity exists
            this.parentNameBox.autoselectRootActivity_if_noCustomActivities();
            entryLayout.AddLayout(this.parentNameBox);

            this.okButton = new Button();
            this.okButton.Clicked += OkButton_Clicked;
            entryLayout.AddLayout(new ButtonLayout(this.okButton, "OK"));

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
                .AddMessage("If the thing you're creating is something measureable that you might try to solve repeatedly but in different ways, then select the type Problem. For example, " +
                "\"Headache\" (or \"Fixing my Headache\") would be a Problem because it may be addressed in several ways: resting, drinking water, or adjusting your posture")
                .AddMessage("If the thing you're creating is something that solves a Problem, then select the type Category and choose the appropriate Problem as a parent. Note " +
                "that there is also a choice named \"Solution\", which is another name for Category, to emphasize this.")
                .AddLayout(new CreditsButtonBuilder(layoutStack)
                    .AddContribution(ActRecContributor.AARON_SMITH, new DateTime(2019, 8, 17), "Suggested that if Activity is the only valid choice then it should autopopulate")
                    .Build()
                )
                .Build();

            HelpButtonLayout helpLayout = new HelpButtonLayout(helpWindow, layoutStack);

            entryLayout.AddLayout(helpLayout);

            this.SetContent(mainGrid);
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

        public abstract List<AppFeature> GetFeatures();
        private void OkButton_Clicked(object sender, EventArgs e)
        {
            ActivityDescriptor childDescriptor = this.childNameBox.ActivityDescriptor;
            ActivityDescriptor parentDescriptor = this.parentNameBox.ActivityDescriptor;
            Inheritance inheritance = new Inheritance(parentDescriptor, childDescriptor);
            inheritance.DiscoveryDate = DateTime.Now;

            string error = this.doCreate(inheritance);
            if (error == "")
            {
                this.childNameBox.Clear();
                this.parentNameBox.Clear();
                this.feedbackLayout.setText("Created " + childDescriptor.ActivityName);
            }
            else
            {
                this.feedbackLayout.setText(error);
            }
        }
        protected abstract string doCreate(Inheritance inheritance);

        protected abstract string getShortExplanation();

        protected ActivityDatabase activityDatabase;

        private ActivityNameEntryBox childNameBox;
        private ActivityNameEntryBox parentNameBox;
        private Button okButton;
        private LayoutStack layoutStack;
        private TextblockLayout feedbackLayout;
        private VisiPlacement.SingleSelect typePicker;
    }

}
