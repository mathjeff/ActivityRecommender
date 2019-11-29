using System;
using System.Collections.Generic;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    public class BrowseInheritancesView : ContainerLayout
    {
        public BrowseInheritancesView(ActivityDatabase activityDatabase, LayoutStack layoutStack)
        {
            ListInheritancesView listView = new ListAllActivitiesView(activityDatabase);
            ListInheritancesView todosView = new ListOpenTodosView(activityDatabase);
            ActivitySearchView searchView = new ActivitySearchView(activityDatabase, "View Activity Inheritances", layoutStack);
            listView.ActivityChosen += this.activityChosen;
            todosView.ActivityChosen += this.activityChosen;
            searchView.ActivityChosen += this.activityChosen;

            this.menuLayout = new MenuLayoutBuilder(layoutStack)
                .AddLayout("List All Activities", listView)
                .AddLayout("List Open ToDos", todosView)
                .AddLayout("Find Activity By Name", searchView)
                .Build();

            this.SubLayout = this.menuLayout;
            this.layoutStack = layoutStack;
            this.activityDatabase = activityDatabase;
        }

        private void activityChosen(object sender, Activity activity)
        {
            this.layoutStack.AddLayout(new ActivityInheritancesView(activity, this.activityDatabase), "Activity");
        }

        public override SpecificLayout GetBestLayout(LayoutQuery query)
        {
            return this.menuLayout.GetBestLayout(query);
        }

        LayoutChoice_Set menuLayout;
        LayoutStack layoutStack;
        ActivityDatabase activityDatabase;

    }

    abstract class ListInheritancesView : TitledControl
    {
        public event ActivityChosenHandler ActivityChosen;
        public delegate void ActivityChosenHandler(object sender, Activity activity);

        public ListInheritancesView(ActivityDatabase activityDatabase)
        {
            this.activityDatabase = activityDatabase;

            this.activityDatabase.ActivityAdded += ActivityDatabase_ActivityAdded;
        }

        private void ActivityDatabase_ActivityAdded(Activity a)
        {
            this.invalidateChildren();
        }

        public override SpecificLayout GetBestLayout(LayoutQuery query)
        {
            if (!this.isCacheable() || this.GetContent() == null)
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

    class ActivitySearchView : TitledControl
    {
        public event ActivityChosenHandler ActivityChosen;
        public delegate void ActivityChosenHandler(object sender, Activity activity);


        public ActivitySearchView(ActivityDatabase activityDatabase, string name, LayoutStack layoutStack)
        {
            this.SetTitle("");
            nameBox = new ActivityNameEntryBox("Activity", activityDatabase, layoutStack);
            nameBox.NumAutocompleteRowsToShow = 2;

            Button button = new Button();
            button.Clicked += Button_Clicked;
            ButtonLayout buttonLayout = new ButtonLayout(button, name);

            this.SetContent(new Vertical_GridLayout_Builder().AddLayout(nameBox).AddLayout(buttonLayout).Build());
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            if (this.ActivityChosen != null)
            {
                Activity activity = this.nameBox.Activity;
                if (activity != null)
                    this.ActivityChosen.Invoke(this, activity);
            }
        }

        ActivityNameEntryBox nameBox;
    }
}
