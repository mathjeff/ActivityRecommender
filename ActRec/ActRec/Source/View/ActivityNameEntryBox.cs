using System;
using System.Collections.Generic;
using System.ComponentModel;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    class ActivityNameEntryBox : ContainerLayout, OnBack_Listener
    {
        public ActivityNameEntryBox(string title, ActivityDatabase activityDatabase, LayoutStack layoutStack, bool createNewActivity = false, bool placeTitleAbove = true)
        {
            // some settings
            this.layoutStack = layoutStack;
            this.AutoAcceptAutocomplete = true;
            this.createNewActivity = createNewActivity;
            this.activityDatabase = activityDatabase;
            this.numAutocompleteRowsToShow = 1;

            // the box the user is typing in
            this.nameBox = new Editor();
            this.nameBox.TextChanged += NameBox_TextChanged;
            this.nameBox_layout = new TextboxLayout(this.nameBox);

            // "X"/"?" button for clearing text or getting help
            // We use one button for both purposes so that the layout doesn't relayout (and shift focus) when this button switches from one to the other
            this.sideButton = new Button();
            this.sideButton.Clicked += SideButton_Clicked;
            this.sideButtonLayout = new ButtonLayout(this.sideButton);

            // layouts controlling the alignment of the main text box and the side button
            this.sideLayout = new ContainerLayout();
            this.sideLayout.SubLayout = this.sideButtonLayout;
            
            GridView gridView = new GridView();
            GridLayout evenBox = GridLayout.New(new BoundProperty_List(1), BoundProperty_List.WithRatios(new List<double>() { 7, 1 }), LayoutScore.Zero, 1, gridView);
            GridLayout unevenBox = GridLayout.New(new BoundProperty_List(1), new BoundProperty_List(2), LayoutScore.Get_UnCentered_LayoutScore(1), 1, gridView);

            evenBox.AddLayout(this.nameBox_layout);
            evenBox.AddLayout(this.sideLayout);
            unevenBox.AddLayout(this.nameBox_layout);
            unevenBox.AddLayout(this.sideLayout);
            this.nameBoxWithSideLayout = new LayoutUnion(evenBox, unevenBox);

            // the autocomplete above the text box
            this.autocompleteLayout = new TextblockLayout();
            this.autocompleteLayout.ScoreIfEmpty = false;

            // button that gives help with autocomplete
            this.helpWindow = new HelpWindowBuilder()
                .AddMessage("This screen explains how to enter " + title + " in the previous screen. " +
                "If you haven't already created the activity that you want to enter here, you will have to go back and create it first in the Activities screen.")
                .AddMessage("")
                .AddMessage("To input an activity name, you may type it in using the letters on the keyboard.")
                .AddMessage("While you do this, ActivityRecommender will try to guess which activity you mean, and " +
                "that autocomplete guess will appear above. If this autocomplete suggestion is what you want, then you can press " +
                "[enter] to use the autocomplete suggestion.")
                .AddMessage("Autocomplete does not require you to type full words but it does require spaces between words.")
                .AddMessage("Autocomplete does not require that you type letters using the correct case but it is more effective if you do.")
                .AddMessage("Consider the following example:")
                .AddMessage("If you have already entered an activity named \"Taking out the Garbage\", " +
                "here are some things you can type that might cause it to become the autocomplete suggestion:")
                .AddMessage("Taking out the")
                .AddMessage("Taking")
                .AddMessage("out")
                .AddMessage("Garbage")
                .AddMessage("garbage")
                .AddMessage("Taking o t G")
                .AddMessage("T o t G")
                .AddMessage("T")
                .AddMessage("G")
                .AddMessage("t")
                .AddMessage("")
                .AddMessage("Note, of course, that the longer and more unique your text, the more likely that it will be matched with the activity that you intend, rather than " +
                "with another activity, like for example, 'Talking on the Phone'")
                .AddLayout(new CreditsButtonBuilder(layoutStack)
                    .AddContribution(ActRecContributor.ANNI_ZHANG, new DateTime(2020, 6, 7), "Pointed out that completed ToDos should have a very low autocomplete priority")
                    .Build()
                 )
                .Build();

            // help buttons that appear when the user types something invalid
            Button help_createNew_button = new Button();
            help_createNew_button.Clicked += help_createNew_button_Clicked;
            this.autocomplete_longHelpLayout = new Horizontal_GridLayout_Builder().Uniform()
                .AddLayout(new ButtonLayout(help_createNew_button, "Create"))
                .AddLayout(new HelpButtonLayout(this.helpWindow, layoutStack))
                .Build();

            // the main layout that contains everything
            LayoutChoice_Set content;
            if (createNewActivity)
            {
                content = unevenBox;
            }
            else
            {
                GridLayout contentWithFeedback = GridLayout.New(new BoundProperty_List(2), new BoundProperty_List(1), LayoutScore.Get_UnCentered_LayoutScore(1));
                contentWithFeedback.AddLayout(this.responseLayout);
                contentWithFeedback.AddLayout(this.nameBoxWithSideLayout);

                content = contentWithFeedback;

                this.UpdateFeedback();
            }

            this.updateSideButton();

            TextblockLayout titleLayout = new TextblockLayout(title);
            titleLayout.AlignHorizontally(TextAlignment.Center);
            if (placeTitleAbove)
            {
                this.SubLayout = new Vertical_GridLayout_Builder()
                    .AddLayout(titleLayout)
                    .AddLayout(content)
                    .Build();
            }
            else
            {
                GridLayout evenGrid = GridLayout.New(new BoundProperty_List(1), BoundProperty_List.Uniform(2), LayoutScore.Zero);
                GridLayout unevenGrid = GridLayout.New(new BoundProperty_List(1), new BoundProperty_List(2), LayoutScore.Get_UnCentered_LayoutScore(1));
                evenGrid.AddLayout(titleLayout);
                unevenGrid.AddLayout(titleLayout);
                evenGrid.AddLayout(content);
                unevenGrid.AddLayout(content);
                this.SubLayout = new LayoutUnion(evenGrid, unevenGrid);
            }
        }

        private void help_createNew_button_Clicked(object sender, EventArgs e)
        {
            ActivityCreationLayout creationLayout = new ActivityCreationLayout(this.activityDatabase, this.layoutStack);
            creationLayout.ActivityName = this.nameText;
            creationLayout.GoBackAfterCreation = true;
            this.layoutStack.AddLayout(creationLayout, "New Activity", this);
        }

        public void Placeholder(string text)
        {
            this.nameBox.Placeholder = text;
        }

        private void SideButton_Clicked(object sender, EventArgs e)
        {
            if (this.sideButton.Text == "X")
                this.Set_NameText("");
            else
                this.layoutStack.AddLayout(this.helpWindow, "Help");
        }

        public void AddTextChangedHandler(EventHandler<TextChangedEventArgs> h)
        {
            this.nameBox.TextChanged += h;
        }

        private void NameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string oldText = e.OldTextValue;
            if (oldText == null)
                oldText = "";
            string newText = this.nameBox.Text;

            if (newText != this.nameText)
                this.userEnteredText(oldText, newText);

            this.updateSideButton();
            this.UpdateFeedback();

            if (this.NameTextChanged != null)
                this.NameTextChanged.Invoke();
        }

        private void updateSideButton()
        {
            if (this.NameText != "" && this.NameText != null)
            {
                // Box has text; show X button
                this.sideLayout.SubLayout = this.sideButtonLayout;
                this.sideButtonLayout.setText("X");
            }
            else
            {
                // Box has no text, show "?" if it's helpful, otherwise show no button
                if (this.createNewActivity)
                    this.sideLayout.SubLayout = null;
                else
                    this.sideButtonLayout.setText("?");
            }
        }

        private void userEnteredText(string oldText, string newText)
        {
            // System.Diagnostics.Debug.WriteLine("User entered text. Old text = '" + oldText + "', new text = '" + newText + "'");
            List<String> markers = new List<string> { "\n", "\t" , "\r"};
            bool addedMarker = false;
            foreach (String marker in markers)
            {
                if (newText.Contains(marker) && !oldText.Contains(marker))
                {
                    addedMarker = true;
                    break;
                }
            }
            if (addedMarker)
            {
                if (this.createNewActivity)
                {
                    // reject illegal characters
                    this.Set_NameText(oldText);
                }
                else
                {
                    // automatically fill the suggestion text into the box
                    this.Set_NameText(this.suggestedActivityName);
                    this.nameBox.Unfocus();
                }
            }
            else
            {
                this.nameText = newText;
            }
        }
        public string NameText
        {
            get
            {
                return this.nameText;
            }
        }
        public void autoselectRootActivity_if_noCustomActivities()
        {
            if (!this.activityDatabase.ContainsCustomActivity())
            {
                this.Set_NameText(this.activityDatabase.GetRootActivity().Name);
            }
        }
        public void Clear()
        {
            this.Set_NameText(null);
        }
        public void Set_NameText(string text)
        {
            this.nameText = text;
            if (this.nameBox.Text != text)
            {
                // this triggers NameBox_TextChanged
                this.nameBox.Text = text;
            }
        }
        // Equal to <true> if the entered activity name matches the autocomplete suggestion
        // If the entered name matches the autocomplete suggestion, that is interpreted as having chosen that activity
        bool NameMatchesSuggestion
        {
            get
            {
                if (this.suggestedActivityName == "")
                {
                    // If the activity name is "" and the suggestion is "", we don't count that as matching the suggestion
                    return false;
                }
                return (this.NameText == this.suggestedActivityName);
            }
        }

        // update feedback based on a change in the text
        void UpdateFeedback()
        {
            // update user interface
            ActivityDescriptor descriptor = this.WorkInProgressActivityDescriptor;
            string oldText = this.suggestedActivityName;
            if (descriptor == null)
            {
                this.suggestedActivityName = "";
                this.autocompleteLayout.setText("");
                // hide the Help button if it's there
                this.responseLayout.SubLayout = null;
            }
            else
            {
                IEnumerable<Activity> autocompletes = this.Autocompletes;

                List<String> autocompleteNames = new List<string>();
                foreach (Activity suggestion in autocompletes)
                {
                    autocompleteNames.Add(suggestion.Name);
                }
                if (autocompleteNames.Count > 0)
                {
                    String firstActivity_name = autocompleteNames[0];
                    this.suggestedActivityName = firstActivity_name;

                    if (firstActivity_name == descriptor.ActivityName)
                    {
                        // perfect match, so display nothing
                        this.autocompleteLayout.setText("");
                    }
                    else
                    {
                        // Update suggestion
                        if (!this.AutoAcceptAutocomplete)
                        {
                            // Not auto-accepting the autocomplete; remind the user that they have to push enter
                            for (int i = 0; i < autocompleteNames.Count; i++)
                            {
                                autocompleteNames[i] = autocompleteNames[i] + "?";
                            }
                        }
                        string suggestionText = String.Join("\n\n", autocompleteNames);
                        this.autocompleteLayout.setText(suggestionText);
                    }
                    // make the autocomplete suggestion appear
                    this.responseLayout.SubLayout = this.autocompleteLayout;
                }
                else
                {
                    // no matches
                    this.suggestedActivityName = "";
                    // show a help button
                    this.responseLayout.SubLayout = this.autocomplete_longHelpLayout;
                }
            }

            // inform the layout engine that there was a change in the text
            if (oldText == null || oldText == "")
            {
                this.AnnounceChange(true);
            }
        }

        public bool AutoAcceptAutocomplete { get; set; } // Whether to treat a partially entered activity as equivalent to the one recommended by autocomplete

        public bool PreferSuggestibleActivities { get; set; }

        public int NumAutocompleteRowsToShow
        {
            get
            {
                return this.numAutocompleteRowsToShow;
            }
            set
            {
                this.numAutocompleteRowsToShow = value;
            }
        }
        private int numAutocompleteRowsToShow;

        // this is the ActivityDescriptor to be returned to outside callers, which represents the selection that the user made
        public ActivityDescriptor ActivityDescriptor
        {
            get
            {
                ActivityDescriptor activityDescriptor = this.WorkInProgressActivityDescriptor;
                if (activityDescriptor == null)
                    return null;
                if (!this.AutoAcceptAutocomplete)
                {
                    activityDescriptor.RequiresPerfectMatch = true;
                    activityDescriptor.Suggestible = null;
                }
                return activityDescriptor;
            }
        }

        // this is the ActivityDescriptor that models what the user is in the middle of typing
        private ActivityDescriptor WorkInProgressActivityDescriptor
        {
            get
            {
                string text = this.nameText;
                if (text == null || text == "")
                {
                    return null;
                }
                ActivityDescriptor descriptor = new ActivityDescriptor();
                descriptor.ActivityName = text;
                descriptor.PreferMorePopular = true;
                descriptor.RequiresPerfectMatch = this.createNewActivity;
                descriptor.PreferAvoidCompletedToDos = true;
                if (this.PreferSuggestibleActivities && !descriptor.RequiresPerfectMatch)
                    descriptor.Suggestible = true;
                return descriptor;
            }
        }
        public Activity Activity
        {
            get
            {
                ActivityDescriptor descriptor = this.ActivityDescriptor;
                if (descriptor == null)
                    return null;
                return this.activityDatabase.ResolveDescriptor(descriptor);
            }
        }
        public IEnumerable<Activity> Autocompletes
        {
            get
            {
                ActivityDescriptor descriptor = this.WorkInProgressActivityDescriptor;
                if (descriptor == null)
                    return new List<Activity>(0);
                IEnumerable<Activity> matches = this.activityDatabase.FindBestMatches(descriptor, this.numAutocompleteRowsToShow);
                return matches;
            }
        }

        public void OnBack(LayoutChoice_Set layout)
        {
            this.UpdateFeedback();
        }

        public event NameTextChangedHandler NameTextChanged;

        // X/? button on the side
        Button sideButton;
        ButtonLayout sideButtonLayout;
        ContainerLayout sideLayout;

        // the box the user is typing into
        Editor nameBox;
        string nameText;
        TextboxLayout nameBox_layout;
        LayoutChoice_Set nameBoxWithSideLayout;

        // autocomplete suggestion
        string suggestedActivityName = "";
        TextblockLayout autocompleteLayout;
        LayoutChoice_Set autocomplete_longHelpLayout;
        ContainerLayout responseLayout = new ContainerLayout();

        // layout that appears if the user requests help
        LayoutChoice_Set helpWindow;

        // some more information
        ActivityDatabase activityDatabase;
        bool createNewActivity;
        LayoutStack layoutStack;
    }

    public delegate void NameTextChangedHandler();

}
