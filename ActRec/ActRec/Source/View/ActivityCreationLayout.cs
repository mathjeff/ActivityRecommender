using ActivityRecommendation.View;
using System;
using System.Collections.Generic;
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

            this.SetTitle("New Activity");
            SingleSelect typeSelector = new SingleSelect("Type:", this.typeChoices);

            GridLayout mainGrid = GridLayout.New(new BoundProperty_List(3), new BoundProperty_List(1), LayoutScore.Zero);
            mainGrid.AddLayout(typeSelector);
            this.feedbackLayout = new TextblockLayout("", 18).AlignVertically(TextAlignment.Center);
            mainGrid.AddLayout(this.feedbackLayout);

            GridLayout bottomGrid = GridLayout.New(new BoundProperty_List(2), BoundProperty_List.Uniform(2), LayoutScore.Zero);
            mainGrid.AddLayout(bottomGrid);

            this.typePicker = typeSelector;
            typeSelector.Updated += TypeSelector_Clicked;

            this.childNameBox = new ActivityNameEntryBox("Activity Name", activityDatabase, layoutStack, true);
            this.childNameBox.AutoAcceptAutocomplete = false;
            bottomGrid.AddLayout(this.childNameBox);

            this.parentNameBox = new ActivityNameEntryBox("Parent Name", activityDatabase, layoutStack);
            this.parentNameBox.AutoAcceptAutocomplete = false;
            // for first-time users, make it extra obvious that the root activity exists
            this.parentNameBox.autoselectRootActivity_if_noCustomActivities();
            bottomGrid.AddLayout(this.parentNameBox);

            this.okButton = new Button();
            this.okButton.Clicked += OkButton_Clicked;
            bottomGrid.AddLayout(new ButtonLayout(this.okButton, "OK"));

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
                    .AddContribution(ActRecContributor.DAGOBERT_RENOUF, new DateTime(2021, 06, 14), "Mentioned that it was difficult to determine how to create a new activity without " +
                    "visual hierarchy among the elements on the screen")
                    .Build()
                )
                .Build();

            HelpButtonLayout helpLayout = new HelpButtonLayout(helpWindow, layoutStack);

            bottomGrid.AddLayout(helpLayout);

            this.explainActivityType();

            this.SetContent(mainGrid);
        }

        private List<SingleSelect_Choice> typeChoices
        {
            get
            {
                SingleSelect_Choice categoryType = new SingleSelect_Choice("Category", Color.FromRgb(181, 255, 254));
                SingleSelect_Choice todoType = new SingleSelect_Choice("ToDo", Color.FromRgb(179, 172, 166));
                SingleSelect_Choice problemType = new SingleSelect_Choice("Problem", Color.FromRgb(235, 228, 134));
                List<SingleSelect_Choice> choices = new List<SingleSelect_Choice>() { categoryType, todoType, problemType };
                // If the user has already entered a solution, then they probably already know that a category can be used as a solution,
                // so we don't need to remind them. They can check the Help if they'd like a reminder.
                // If the user hasn't yet entered a solution, we may need to explicitly call out Solution as a separate menu option
                if (!this.activityDatabase.HasSolution)
                {
                    SingleSelect_Choice solutionType = new SingleSelect_Choice("Solution", Color.FromRgb(48, 237, 138));
                    choices.Add(solutionType);
                }
                return choices;
            }
        }

        // returns true if the type of Activity to create is Category
        bool SelectedActivityTypeIsCategory
        {
            get
            {
                return this.typePicker.SelectedIndex == 0;
            }
        }
        public bool SelectedActivityTypeIsToDo
        {
            get
            {
                return this.typePicker.SelectedIndex == 1;
            }
            set
            {
                this.typePicker.SelectedIndex = 1;
                this.explainActivityType();
            }
        }
        public bool SelectedActivityTypeIsProblem
        {
            get
            {
                return this.typePicker.SelectedIndex == 2;
            }
        }
        public bool SelectedActivityTypeIsSolution
        {
            get
            {
                return this.typePicker.SelectedIndex == 3;
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

        public bool GoBackAfterCreation { get; set; }


        public List<AppFeature> GetFeatures()
        {
            return new List<AppFeature>() {
                new CreateActivity_Feature(this.activityDatabase),
                new CreateTodo_Feature(this.activityDatabase),
                new CreateProblem_Feature(this.activityDatabase),
                new CreateSolution_Feature(this.activityDatabase)
            };
        }

        private void OkButton_Clicked(object sender, EventArgs e)
        {
            ActivityDescriptor childDescriptor = this.childNameBox.ActivityDescriptor;
            ActivityDescriptor parentDescriptor = this.parentNameBox.ActivityDescriptor;
            Inheritance inheritance = new Inheritance(parentDescriptor, childDescriptor);
            inheritance.DiscoveryDate = DateTime.Now;

            string error;
            if (this.SelectedActivityTypeIsCategory || this.SelectedActivityTypeIsSolution)
            {
                error = this.activityDatabase.CreateCategory(inheritance);
            }
            else
            {
                if (this.SelectedActivityTypeIsToDo)
                {
                    error = this.activityDatabase.CreateToDo(inheritance);
                }
                else
                {
                    error = this.activityDatabase.CreateProblem(inheritance);
                }
            }
            if (error == "")
            {
                this.childNameBox.Clear();
                this.parentNameBox.Clear();
                this.showMessage("Created " + childDescriptor.ActivityName);
                if (this.GoBackAfterCreation)
                    this.layoutStack.GoBack();
            }
            else
            {
                this.showError(error);
            }
        }

        private void showError(string text)
        {
            this.feedbackLayout.setText(text);
            this.feedbackLayout.setTextColor(Color.Red);
        }

        private void showMessage(string text)
        {
            this.feedbackLayout.setText(text);
            this.feedbackLayout.resetTextColor();
        }

        private void TypeSelector_Clicked(SingleSelect singleSelect)
        {
            if (this.SelectedActivityTypeIsSolution)
            {
                if (!this.activityDatabase.HasProblem)
                {
                    // If the user hasn't entered a Problem yet, then it doesn't make sense to try to enter a solution yet
                    this.typePicker.Advance();
                }
            }
            this.explainActivityType();
        }

        private void explainActivityType()
        {
            string text;
            if (this.SelectedActivityTypeIsCategory)
            {
                text = "A Category is a class of things you may do multiple times. A Category may have other activities as children.";
            }
            else
            {
                if (this.SelectedActivityTypeIsToDo)
                {
                    text = "A ToDo is a specific thing that you complete once. A ToDo can't be given children.";
                }
                else
                {
                    if (this.SelectedActivityTypeIsProblem)
                    {
                        text = "A Problem is something you may want to fix multiple times. Its children may be other Problems or may be other Categories (Solutions).";
                    }
                    else
                    {
                        text = "A Solution is another name for a Category. If you assign a Category as a child of a Problem, the Category will be considered a solution to that Problem.";
                    }
                }
            }
            this.showMessage(text);
        }

        private ActivityNameEntryBox childNameBox;
        private ActivityNameEntryBox parentNameBox;
        private Button okButton;
        private LayoutStack layoutStack;
        private ActivityDatabase activityDatabase;
        private TextblockLayout feedbackLayout;
        private VisiPlacement.SingleSelect typePicker;
    }

    class CreateActivity_Feature : AppFeature
    {
        public CreateActivity_Feature(ActivityDatabase activityDatabase)
        {
            this.activityDatabase = activityDatabase;
        }
        public string GetDescription()
        {
            return "Create an activity";
        }
        public bool GetIsUsable()
        {
            return true;
        }
        public bool GetHasBeenUsed()
        {
            return this.activityDatabase.ContainsCustomActivity();
        }
        ActivityDatabase activityDatabase;
    }
    class CreateTodo_Feature : AppFeature
    {
        public CreateTodo_Feature(ActivityDatabase activityDatabase)
        {
            this.activityDatabase = activityDatabase;
        }
        public string GetDescription()
        {
            return "Create a Todo";
        }
        public bool GetIsUsable()
        {
            return true;
        }

        public bool GetHasBeenUsed()
        {
            return this.activityDatabase.HasTodo;
        }
        ActivityDatabase activityDatabase;
    }
    class CreateProblem_Feature : AppFeature
    {
        public CreateProblem_Feature(ActivityDatabase activityDatabase)
        {
            this.activityDatabase = activityDatabase;
        }
        public string GetDescription()
        {
            return "Declare a Problem";
        }
        public bool GetIsUsable()
        {
            return true;
        }

        public bool GetHasBeenUsed()
        {
            return this.activityDatabase.HasProblem;
        }
        ActivityDatabase activityDatabase;
    }
    class CreateSolution_Feature : AppFeature
    {
        public CreateSolution_Feature(ActivityDatabase activityDatabase)
        {
            this.activityDatabase = activityDatabase;
        }
        public string GetDescription()
        {
            return "Declare a Solution";
        }
        public bool GetIsUsable()
        {
            return true;
        }

        public bool GetHasBeenUsed()
        {
            return this.activityDatabase.HasSolution;
        }
        ActivityDatabase activityDatabase;
    }
}
