using System;
using System.Collections.Generic;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    class ActivitySearchView : ContainerLayout
    {
        public ActivitySearchView(ActivityDatabase activityDatabase, ProtoActivity_Database protoActivity_database, LayoutStack layoutStack)
        {
            this.layoutStack = layoutStack;
            this.activityDatabase = activityDatabase;
            this.protoActivity_database = protoActivity_database;
            this.textBox = new Editor();
            this.textBox.TextChanged += TextBox_TextChanged;
            this.largeFont_autocomplete_gridLayout = GridLayout.New(new BoundProperty_List(this.maxNumResults), new BoundProperty_List(1), LayoutScore.Zero);
            this.smallFont_autocomplete_gridLayout = GridLayout.New(new BoundProperty_List(this.maxNumResults), new BoundProperty_List(1), LayoutScore.Zero);

            Button rootActivity_button = new Button();
            rootActivity_button.Clicked += RootActivity_button_Clicked;
            this.rootActivity_buttonLayout = new ButtonLayout(rootActivity_button, "Or, see the root activity");
            this.footer = new ContainerLayout();

            this.SubLayout = new Vertical_GridLayout_Builder()
                .AddLayout(
                    new LayoutUnion(
                        this.largeFont_autocomplete_gridLayout,
                        this.smallFont_autocomplete_gridLayout)
                )
                .AddLayout(new TextboxLayout(this.textBox))
                .AddLayout(footer)
                .BuildAnyLayout();

            this.titleLayout = new TextblockLayout("Activity name (and/or ProtoActivity name):");
            this.updateAutocomplete();
        }

        private void RootActivity_button_Clicked(object sender, EventArgs e)
        {
            this.choseActivity(this.activityDatabase.RootActivity);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.updateAutocomplete();
        }
        private void updateAutocomplete()
        {
            string query = this.textBox.Text;
            List<ProtoActivity> protoActivities = new List<ProtoActivity>();
            List<Activity> activities = new List<Activity>();
            if (query != null && query != "")
            {
                ActivityDescriptor activityDescriptor = new ActivityDescriptor(query);
                activityDescriptor.RequiresPerfectMatch = false;
                // search for some protoactivities
                protoActivities = this.protoActivity_database.TextSearch(query, this.maxNumResults / 2);
                // try to fill out the results with activities
                activities = this.activityDatabase.FindBestMatches(activityDescriptor, this.maxNumResults - protoActivities.Count);
                // if we didn't find enough activities, try to fill out the rest with protoactivities
                if (activities.Count + protoActivities.Count < this.maxNumResults)
                    protoActivities = this.protoActivity_database.TextSearch(query, this.maxNumResults - activities.Count);
            }
            this.putAutocomplete(activities, protoActivities);
            if (activities.Count < 1 && protoActivities.Count < 1)
            {
                // If there are no results, show an explanatory title
                this.largeFont_autocomplete_gridLayout.PutLayout(this.titleLayout, 0, 0);

                // If there are no matches, also show a button for jumping directly to the root
                this.footer.SubLayout = this.rootActivity_buttonLayout;
            }
            else
            {
                // If there are matches, don't need to show the button for jumping to the root
                this.footer.SubLayout = null;
            }
        }
        private void putAutocomplete(List<Activity> activities, List<ProtoActivity> protoActivities)
        {
            bool changed = true;
            if (activities.Count == this.activitiesByButton.Count && protoActivities.Count == this.protoActivities_byButton.Count)
            {
                changed = false;
                foreach (Activity activity in this.activitiesByButton.Values)
                {
                    if (!activities.Contains(activity))
                    {
                        changed = true;
                        break;
                    }
                }
                foreach (ProtoActivity protoActivity in this.protoActivities_byButton.Values)
                {
                    if (!protoActivities.Contains(protoActivity))
                    {
                        changed = true;
                        break;
                    }
                }
            }
            if (!changed)
                return;
            this.activitiesByButton = new Dictionary<Button, Activity>();
            this.protoActivities_byButton = new Dictionary<Button, ProtoActivity>();

            for (int i = 0; i < activities.Count; i++)
            {
                Activity activity = activities[i];
                this.ensureButtonLayout(i);

                Button button = this.buttons[i];
                this.activitiesByButton[button] = activity;

                this.showResult("Activity: " + activity.Name, i);
            }
            for (int i = 0; i < protoActivities.Count; i++)
            {
                int y = i + activities.Count;
                ProtoActivity protoActivity = protoActivities[i];
                this.ensureButtonLayout(y);

                Button button = this.buttons[y];
                this.protoActivities_byButton[button] = protoActivity;
                this.showResult("ProtoActivity: " + protoActivity.Summarize(), y);

            }
            for (int i = activities.Count + protoActivities.Count; i < this.largeFont_autocomplete_gridLayout.NumRows; i++)
            {
                this.largeFont_autocomplete_gridLayout.PutLayout(null, 0, i);
                this.smallFont_autocomplete_gridLayout.PutLayout(null, 0, i);
            }
        }

        private void showResult(string text, int index)
        {
            ButtonLayout largeLayout = this.largeFont_buttonLayouts[index];
            largeLayout.setText(text);
            this.largeFont_autocomplete_gridLayout.PutLayout(largeLayout, 0, index);

            ButtonLayout smallLayout = this.smallFont_buttonLayouts[index];
            smallLayout.setText(text);
            this.smallFont_autocomplete_gridLayout.PutLayout(smallLayout, 0, index);
        }

        private void ensureButtonLayout(int index)
        {
            while (this.buttons.Count <= index)
            {
                Button button = new Button();
                button.Clicked += Button_Click;
                this.buttons.Add(button);
                this.largeFont_buttonLayouts.Add(new ButtonLayout(button, "", 24));
                this.smallFont_buttonLayouts.Add(new ButtonLayout(button, "", 12));
            }
        }

        private void Button_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                Activity activity;
                if (this.activitiesByButton.TryGetValue(button, out activity))
                {
                    this.choseActivity(activity);
                }
                else
                {
                    ProtoActivity protoActivity = this.protoActivities_byButton[button];

                    ProtoActivity_Editing_Layout layout = new ProtoActivity_Editing_Layout(protoActivity, this.protoActivity_database, this.activityDatabase, this.layoutStack);
                    this.layoutStack.AddLayout(layout, "Proto", layout);
                }
            }
        }
        private void choseActivity(Activity activity)
        {
            this.layoutStack.AddLayout(new ActivityInheritancesView(activity, this.activityDatabase), "Activity");
        }

        Editor textBox;
        LayoutStack layoutStack;
        ActivityDatabase activityDatabase;
        ProtoActivity_Database protoActivity_database;
        Dictionary<Button, Activity> activitiesByButton = new Dictionary<Button, Activity>();
        Dictionary<Button, ProtoActivity> protoActivities_byButton = new Dictionary<Button, ProtoActivity>();
        List<Button> buttons = new List<Button>();
        
        List<ButtonLayout> largeFont_buttonLayouts = new List<ButtonLayout>();
        GridLayout largeFont_autocomplete_gridLayout;
        
        List<ButtonLayout> smallFont_buttonLayouts = new List<ButtonLayout>();
        GridLayout smallFont_autocomplete_gridLayout;
        
        TextblockLayout titleLayout;
        ButtonLayout rootActivity_buttonLayout;
        ContainerLayout footer;
        int maxNumResults = 6;
    }
}
