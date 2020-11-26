using System;
using System.Collections.Generic;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    public class BrowseInheritancesView : ContainerLayout
    {
        public BrowseInheritancesView(ActivityDatabase activityDatabase, ProtoActivity_Database protoActivity_database, LayoutStack layoutStack)
        {
            ListInheritancesView listView = new ListAllActivitiesView(activityDatabase);
            ListInheritancesView todosView = new ListOpenTodosView(activityDatabase);
            ActivitySearchView searchView = new ActivitySearchView(activityDatabase, protoActivity_database, layoutStack);
            listView.ActivityChosen += this.activityChosen;
            todosView.ActivityChosen += this.activityChosen;
            searchView.ActivityChosen += this.activityChosen;
            searchView.ProtoActivity_Chosen += this.protoActivity_chosen;

            this.menuLayout = new MenuLayoutBuilder(layoutStack)
                .AddLayout(new StackEntry(listView, "List All Activities", listView))
                .AddLayout(new StackEntry(todosView, "List Open ToDos", todosView))
                .AddLayout("Find Activity By Name", searchView)
                .Build();

            this.SubLayout = this.menuLayout;
            this.layoutStack = layoutStack;
            this.activityDatabase = activityDatabase;
            this.protoActivity_database = protoActivity_database;
        }

        private void activityChosen(object sender, Activity activity)
        {
            this.layoutStack.AddLayout(new ActivityInheritancesView(activity, this.activityDatabase), "Activity");
        }

        private void protoActivity_chosen(object sender, ProtoActivity protoActivity)
        {
            ProtoActivity_Editing_Layout layout = new ProtoActivity_Editing_Layout(protoActivity, this.protoActivity_database, this.activityDatabase, this.layoutStack);
            this.layoutStack.AddLayout(layout, "Proto", layout);
        }

        public override SpecificLayout GetBestLayout(LayoutQuery query)
        {
            return this.menuLayout.GetBestLayout(query);
        }

        LayoutChoice_Set menuLayout;
        LayoutStack layoutStack;
        ActivityDatabase activityDatabase;
        ProtoActivity_Database protoActivity_database;
    }

    abstract class ListInheritancesView : TitledControl, OnBack_Listener
    {
        public event ActivityChosenHandler ActivityChosen;
        public delegate void ActivityChosenHandler(object sender, Activity activity);

        public ListInheritancesView(ActivityDatabase activityDatabase) : base("Activities", 30)
        {
            this.activityDatabase = activityDatabase;

            this.activityDatabase.ActivityAdded += ActivityDatabase_ActivityAdded;
        }

        private void ActivityDatabase_ActivityAdded(Activity a)
        {
            this.invalidateChildren();
        }

        public void OnBack(LayoutChoice_Set layout)
        {
            if (!this.isCacheable())
            {
                this.invalidateChildren();
            }
        }
        public override SpecificLayout GetBestLayout(LayoutQuery query)
        {
            if (this.GetContent() == null)
                this.generateChildren();
            return base.GetBestLayout(query);
        }

        private void generateChildren()
        {
            IEnumerable<Activity> activities = this.getActivities(this.activityDatabase);
            Vertical_GridLayout_Builder builder = new Vertical_GridLayout_Builder().Uniform();
            this.activityButtons = new Dictionary<Button, Activity>();
            foreach (Activity activity in activities)
            {
                Button button = new Button();
                button.Clicked += Button_Clicked;
                this.activityButtons[button] = activity;
                builder.AddLayout(new ButtonLayout(button, activity.Name, 24));
            }
            this.SetContent(ScrollLayout.New(builder.Build()));
        }

        protected abstract IEnumerable<Activity> getActivities(ActivityDatabase activityDatabase);

        private void Button_Clicked(object sender, System.EventArgs e)
        {
            Button button = sender as Button;
            Activity activity = this.activityButtons[button];
            this.selected(activity);
        }

        private void selected(Activity activity)
        {
            if (this.ActivityChosen != null)
            {
                this.ActivityChosen.Invoke(this, activity);
            }
        }

        private void invalidateChildren()
        {
            this.SetContent(null);
        }

        protected virtual bool isCacheable()
        {
            return true;
        }


        Dictionary<Button, Activity> activityButtons;
        ActivityDatabase activityDatabase;
    }

    class ListAllActivitiesView: ListInheritancesView
    {
        public ListAllActivitiesView(ActivityDatabase activityDatabase) : base(activityDatabase)
        {
            this.SetTitle("Activities You Have Entered");
        }
        protected override IEnumerable<Activity> getActivities(ActivityDatabase activityDatabase)
        {
            return activityDatabase.AllActivities;
        }
    }

    class ListOpenTodosView: ListInheritancesView
    {
        public ListOpenTodosView(ActivityDatabase activityDatabase) : base(activityDatabase)
        {
            this.SetTitle("Remaining ToDos");
        }
        protected override IEnumerable<Activity> getActivities(ActivityDatabase activityDatabase)
        {
            return activityDatabase.AllOpenTodos;
        }
        protected override bool isCacheable()
        {
            return false;
        }
    }

    class ActivitySearchView : ContainerLayout
    {
        public event ActivityChosenHandler ActivityChosen;
        public delegate void ActivityChosenHandler(object sender, Activity activity);

        public event ProtoActivityChosen_Handler ProtoActivity_Chosen;
        public delegate void ProtoActivityChosen_Handler(object sender, ProtoActivity protoActivity);


        public ActivitySearchView(ActivityDatabase activityDatabase, ProtoActivity_Database protoActivity_database, LayoutStack layoutStack)
        {
            this.activityDatabase = activityDatabase;
            this.protoActivity_database = protoActivity_database;
            this.textBox = new Editor();
            this.textBox.TextChanged += TextBox_TextChanged;
            this.largeFont_autocomplete_gridLayout = GridLayout.New(new BoundProperty_List(this.maxNumResults), new BoundProperty_List(1), LayoutScore.Zero);
            this.smallFont_autocomplete_gridLayout = GridLayout.New(new BoundProperty_List(this.maxNumResults), new BoundProperty_List(1), LayoutScore.Zero);

            this.SubLayout = new Vertical_GridLayout_Builder()
                .AddLayout(
                    new LayoutUnion(
                        this.largeFont_autocomplete_gridLayout,
                        this.smallFont_autocomplete_gridLayout)
                )
                .AddLayout(new TextboxLayout(this.textBox))
                .BuildAnyLayout();

            this.titleLayout = new TextblockLayout("Activity name (and/or ProtoActivity name):");
            this.updateAutocomplete();
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
                this.largeFont_autocomplete_gridLayout.PutLayout(this.titleLayout, 0, 0);
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

            List<LayoutChoice_Set> layouts = new List<LayoutChoice_Set>();

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
            largeLayout.setText(text);
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
                    if (this.ActivityChosen != null)
                        this.ActivityChosen.Invoke(this, activity);
                }
                else
                {
                    ProtoActivity protoActivity = this.protoActivities_byButton[button];
                    if (this.ProtoActivity_Chosen != null)
                        this.ProtoActivity_Chosen.Invoke(this, protoActivity);
                }
            }
        }

        Editor textBox;
        ActivityDatabase activityDatabase;
        ProtoActivity_Database protoActivity_database;
        Dictionary<Button, Activity> activitiesByButton = new Dictionary<Button, Activity>();
        Dictionary<Button, ProtoActivity> protoActivities_byButton = new Dictionary<Button, ProtoActivity>();
        List<Button> buttons = new List<Button>();
        
        List<ButtonLayout> largeFont_buttonLayouts = new List<ButtonLayout>();
        GridLayout largeFont_autocomplete_gridLayout;
        
        List<ButtonLayout> smallFont_buttonLayouts = new List<ButtonLayout>();
        GridLayout smallFont_autocomplete_gridLayout;
        
        private TextblockLayout titleLayout;
        private int maxNumResults = 6;
    }
}
