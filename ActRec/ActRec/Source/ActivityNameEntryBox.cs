using System;
using System.Collections.Generic;
using System.ComponentModel;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation
{
    class ActivityNameEntryBox : TitledControl
    {
        public ActivityNameEntryBox(string startingTitle, bool createNewActivity = false) : base(startingTitle)
        {

            this.nameBox = new Editor();
            this.nameBox.TextChanged += NameBox_TextChanged;
            this.nameBox_layout = new TextboxLayout(this.nameBox);

            this.AutoAcceptAutocomplete = true;

            Button xButton = new Button();
            xButton.Text = "X";
            xButton.Clicked += XButton_Clicked;
            this.nameBoxWithX = new Horizontal_GridLayout_Builder().AddLayout(this.nameBox_layout).AddLayout(new ButtonLayout(xButton)).Build();

            this.nameLayout = new ContainerLayout();

            this.suggestionBlock = new Label();
            LayoutChoice_Set content;
            this.createNewActivity = createNewActivity;


            if (createNewActivity)
            {
                content = nameLayout;
            }
            else
            {
                GridLayout contentWithSuggestion = GridLayout.New(new BoundProperty_List(2), new BoundProperty_List(1), LayoutScore.Get_UnCentered_LayoutScore(1));
                contentWithSuggestion.AddLayout(new TextblockLayout(this.suggestionBlock));
                contentWithSuggestion.AddLayout(nameLayout);

                content = new LayoutCache(contentWithSuggestion);
            }

            this.updateXButton();
            this.UpdateSuggestions();

            base.SetContent(content);
        }

        private void XButton_Clicked(object sender, EventArgs e)
        {
            this.NameText = "";
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

            this.updateXButton();
        }

        private void updateXButton()
        {
            if (this.NameText != "" && this.NameText != null)
                this.nameLayout.SubLayout = this.nameBoxWithX;
            else
                this.nameLayout.SubLayout = this.nameBox_layout;
        }

        private void userEnteredText(string oldText, string newText)
        {
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
                    this.NameText = oldText;
                }
                else
                {
                    // automatically fill the suggestion text into the box
                    this.NameText = this.suggestedActivityName;
                    this.nameBox.Unfocus();
                }
            }
            else
            {
                this.NameText = newText;
            }
        }
        public string NameText
        {
            get
            {
                return this.nameText;
            }
            set
            {
                string oldText = this.nameText;

                this.nameText = value;
                this.nameBox.Text = value;
                this.UpdateSuggestions();

                if (this.NameMatchesSuggestion)
                {
                    if (this.NameMatchedSuggestion != null)
                        this.NameMatchedSuggestion.Invoke(this, new TextChangedEventArgs(oldText, this.nameText));
                }
            }
        }
        public void Clear()
        {
            this.Set_NameText(null);
        }
        public void Set_NameText(string text)
        {
            this.NameText = text;
        }
        public ReadableActivityDatabase Database
        {
            set
            {
                this.database = value;
            }
        }
        bool NameMatchesSuggestion
        {
            get
            {
                return (this.NameText == this.suggestedActivityName);
            }
        }

        string doBackspace(string oldText)
        {
            string newText = "";
            // look for the activity with the longest common substring that is strictly shorter than the current text
            foreach (Activity activity in this.database.AllActivities)
            {
                string prefix = commonPrefix(activity.Name, oldText);
                if (prefix.Length > newText.Length && prefix.Length < oldText.Length)
                    newText = prefix;
            }
            // if no activity had an characters in common, then we couldn't have gotten to this string via autocomplete, so only remove one character
            if (newText.Length < 1 && oldText.Length > 1)
                newText = oldText.Substring(0, oldText.Length - 1);
            return newText;
        }

        // fill in more text based on the valid choices
        string autocomplete(string currentText)
        {
            // find the common prefix of all matching activities
            List<String> matchTexts = new List<string>();
            foreach (Activity activity in this.database.AllActivities)
            {
                if (activity.Name.StartsWith(currentText))
                    matchTexts.Add(activity.Name);
            }
            string prefix = this.commonPrefix(matchTexts);
            if (prefix.Length < 1)
            {
                // there was no case-sensitive match; try a case-insensitive match
                matchTexts = new List<string>();
                foreach (Activity activity in this.database.AllActivities)
                {
                    if (activity.Name.ToLower().StartsWith(currentText.ToLower()))
                        matchTexts.Add(activity.Name);
                }
                prefix = this.commonPrefix(matchTexts);
            }

            if (prefix.Length > currentText.Length)
            {
                return prefix;
            }
            return currentText;
        }

        string commonPrefix(string a, string b)
        {
            int i;
            int max = Math.Min(a.Length, b.Length);
            for (i = 0; i < max; i++)
            {
                if (a[i] != b[i])
                    break;
            }
            string prefix = a.Substring(0, i);
            return prefix;
        }

        string commonPrefix(List<string> texts)
        {
            if (texts.Count < 1)
                return "";
            string prefix = texts[0];
            foreach (string text in texts)
            {
                prefix = commonPrefix(prefix, text);
            }
            return prefix;
        }

        // update the UI based on a change in the text that the user has typed
        void UpdateSuggestions()
        {
            ActivityDescriptor descriptor = this.WorkInProgressActivityDescriptor;
            string oldText = this.suggestedActivityName;
            if (descriptor == null)
            {
                this.suggestedActivityName = "";
                this.suggestionBlock.Text = "";
            }
            else
            {
                Activity activity = this.database.ResolveDescriptor(descriptor);
                if (activity != null)
                {
                    // Also consider using color to prompt users to accept the suggestion
                    this.suggestedActivityName = activity.Name;
                    if (activity.Name == descriptor.ActivityName)
                    {
                        // perfect match, so display nothing
                        this.suggestionBlock.Text = "";
                    }
                    else
                    {
                        // Update suggestion
                        string suggestionText;
                        if (this.AutoAcceptAutocomplete)
                            suggestionText = this.suggestedActivityName;
                        else
                            suggestionText = this.suggestedActivityName + "?";
                        this.suggestionBlock.Text = suggestionText;
                    }
                }
                else
                {
                    this.suggestedActivityName = "";
                    this.suggestionBlock.Text = "";
                }
            }
            if (oldText == null || oldText == "")
            {
                this.AnnounceChange(true);
            }
        }

        public bool AutoAcceptAutocomplete { get; set; } // Whether to treat a partially entered activity as equivalent to the one recommended by autocomplete

        // this is the ActivityDescriptor to be returned to outside callers, which represents the selection that the user made
        public ActivityDescriptor ActivityDescriptor
        {
            get
            {
                ActivityDescriptor activityDescriptor = this.WorkInProgressActivityDescriptor;
                if (activityDescriptor == null)
                    return null;
                if (!this.AutoAcceptAutocomplete)
                    activityDescriptor.RequiresPerfectMatch = true;
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
        public event NameMatchedSuggestionHandler NameMatchedSuggestion;

        ContainerLayout nameLayout;
        GridLayout nameBoxWithX;
        TextboxLayout nameBox_layout;
        string nameText;
        Editor nameBox;
        Label suggestionBlock;
        ReadableActivityDatabase database;
        string suggestedActivityName = "";
        bool createNewActivity;
    }

    // Summary:
    //     Represents the method that will handle the ActivityRecommendation.NameMatchedSuggestion
    //      routed event.
    //
    // Parameters:
    //   sender:
    //     The object where the event handler is attached.
    //
    //   e:
    //     The event data.
    public delegate void NameMatchedSuggestionHandler(object sender, TextChangedEventArgs e);

}
