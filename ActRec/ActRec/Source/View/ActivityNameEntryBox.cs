﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    class ActivityNameEntryBox : TitledControl
    {
        public ActivityNameEntryBox(string startingTitle, ReadableActivityDatabase activityDatabase, LayoutStack layoutStack, bool createNewActivity = false) : base(startingTitle)
        {
            // some settings
            this.layoutStack = layoutStack;
            this.AutoAcceptAutocomplete = true;
            this.createNewActivity = createNewActivity;
            this.database = activityDatabase;
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
            GridLayout evenBox = GridLayout.New(new BoundProperty_List(1), BoundProperty_List.WithRatios(new List<double>() { 7, 1 }), LayoutScore.Zero);
            GridLayout unevenBox = GridLayout.New(new BoundProperty_List(1), new BoundProperty_List(2), LayoutScore.Get_UnCentered_LayoutScore(1));
            evenBox.AddLayout(this.nameBox_layout);
            evenBox.AddLayout(this.sideLayout);
            unevenBox.AddLayout(this.nameBox_layout);
            unevenBox.AddLayout(this.sideLayout);
            this.nameBoxWithSideLayout = new LayoutUnion(evenBox, unevenBox);

            // the autocomplete above the text box
            this.autocompleteLayout = new TextblockLayout();

            // button that gives help with autocomplete
            this.helpWindow = new HelpWindowBuilder()
                .AddMessage("This screen explains how to enter " + startingTitle + " in the previous screen. " +
                "Your input must match an activity that you have previously entered.")
                .AddMessage("The most basic way to input an activity name is to type it in using the letters on the keyboard.")
                .AddMessage("However, while you type the activity name, ActivityRecommender will try to guess which activity you mean, and " +
                "that guess will appear above. If the autocomplete suggestion is what you want, then you can press " +
                "[enter] to use the autocomplete suggestion.")
                .AddMessage("Autocomplete does not require you to type full words but it does require spaces between words.")
                .AddMessage("Autocomplete does not require that you type letters using the correct case but it is more effective if you do.")
                .AddMessage("If autocomplete encounters a misspelled word, autocomplete will ignore the misspelled word.")
                .AddMessage("Consider the following example.")
                .AddMessage("If you have already entered an activity named \"Taking out the Garbage\", " +
                "here are some things you can type that might cause it to become the autocomplete suggestion:")
                .AddMessage("Taking out the")
                .AddMessage("Taking out the MisspelledWord")
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
                .AddMessage("Note, of course, that the longer and more unique your text, the more likely that it will be matched with the activity that you intend.")
                .AddLayout(new CreditsButtonBuilder(layoutStack)
                    .AddContribution(ActRecContributor.ANNI_ZHANG, new DateTime(2020, 6, 7), "Pointed out that completed ToDos should have a very low autocomplete priority")
                    .Build()
                 )
                .Build();
            this.autocomplete_longHelpLayout = new HelpButtonLayout(this.helpWindow, layoutStack);

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

            base.SetContent(content);
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

            this.UpdateFeedback();
            this.updateSideButton();

            if (this.NameMatchesSuggestion)
            {
                if (this.NameMatchedSuggestion != null)
                    this.NameMatchedSuggestion.Invoke(this, new TextChangedEventArgs(oldText, this.nameText));
            }
        }

        private void updateSideButton()
        {
            if (this.NameText != "" && this.NameText != null)
            {
                // Box has text; show X button
                this.sideLayout.SubLayout = this.sideButtonLayout;
                this.sideButton.Text = "X";
            }
            else
            {
                // Box has no text, show "?" if it's helpful, otherwise show no button
                if (this.createNewActivity)
                    this.sideLayout.SubLayout = null;
                else
                    this.sideButton.Text = "?";
            }
        }

        private void userEnteredText(string oldText, string newText)
        {
            // System.Diagnostics.Debug.WriteLine("User entered text. Old text = '" + oldText + "', new text = '" + newText + "'");
            List<String> markers = new List<string> { "\n", "\t" };
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
            if (!this.database.ContainsCustomActivity())
            {
                this.Set_NameText(this.database.GetRootActivity().Name);
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
                this.nameBox.Text = text;
        }
        bool NameMatchesSuggestion
        {
            get
            {
                return (this.NameText == this.suggestedActivityName);
            }
        }

        // update the UI based on a change in the text that the user has typed
        void UpdateFeedback()
        {
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
                return this.database.ResolveDescriptor(descriptor);
            }
        }
        public IEnumerable<Activity> Autocompletes
        {
            get
            {
                ActivityDescriptor descriptor = this.WorkInProgressActivityDescriptor;
                if (descriptor == null)
                    return new List<Activity>(0);
                IEnumerable<Activity> matches = this.database.FindBestMatches(descriptor, this.numAutocompleteRowsToShow);
                return matches;
            }
        }
        public event NameMatchedSuggestionHandler NameMatchedSuggestion;

        Button sideButton;
        ButtonLayout sideButtonLayout;
        LayoutChoice_Set helpWindow;
        LayoutChoice_Set nameBoxWithSideLayout;
        ContainerLayout sideLayout;
        TextboxLayout nameBox_layout;
        string nameText;
        Editor nameBox;
        LayoutChoice_Set autocomplete_longHelpLayout;
        TextblockLayout autocompleteLayout;
        ContainerLayout responseLayout = new ContainerLayout();
        ReadableActivityDatabase database;
        string suggestedActivityName = "";
        bool createNewActivity;
        LayoutStack layoutStack;
    }

    public delegate void NameMatchedSuggestionHandler(object sender, TextChangedEventArgs e);

}
