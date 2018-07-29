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
            ListInheritancesView listView = new ListInheritancesView(activityDatabase);
            ActivitySearchView searchView = new ActivitySearchView(activityDatabase, "View Activity Inheritances");
            listView.ActivityChosen += this.activityChosen;
            searchView.ActivityChosen += this.activityChosen;

            this.menuLayout = new MenuLayoutBuilder(layoutStack)
                .AddLayout("List All Activities", listView)
                .AddLayout("Find Activity By Name", searchView)
                .Build();

            this.SubLayout = this.menuLayout;
            this.layoutStack = layoutStack;
            this.activityDatabase = activityDatabase;
        }

        private void activityChosen(object sender, Category activity)
        {
            this.layoutStack.AddLayout(new ActivityInheritancesView(activity, this.activityDatabase));
        }

        public override SpecificLayout GetBestLayout(LayoutQuery query)
        {
            return this.menuLayout.GetBestLayout(query);
        }

        LayoutChoice_Set menuLayout;
        LayoutStack layoutStack;
        ActivityDatabase activityDatabase;

    }

    class ListInheritancesView : TitledControl
    {
        public event ActivityChosenHandler ActivityChosen;
        public delegate void ActivityChosenHandler(object sender, Category activity);

        public ListInheritancesView(ActivityDatabase activityDatabase)
        {
            this.SetTitle("Activities You Have Entered");
            this.activityDatabase = activityDatabase;

            this.activityDatabase.ActivityAdded += ActivityDatabase_ActivityAdded;
        }

        private void ActivityDatabase_ActivityAdded(object sender, Category a)
        {
            this.invalidateChildren();
        }

        public override SpecificLayout GetBestLayout(LayoutQuery query)
        {
            if (this.GetContent() == null)
                this.generateChildren();
            return base.GetBestLayout(query);
        }

        private void generateChildren()
        {
            IEnumerable<Category> activities = this.activityDatabase.AllActivities;
            Vertical_GridLayout_Builder builder = new Vertical_GridLayout_Builder().Uniform();
            this.activityButtons = new Dictionary<Button, Category>();
            foreach (Category activity in activities)
            {
                Button button = new Button();
                button.Clicked += Button_Clicked;
                this.activityButtons[button] = activity;
                builder.AddLayout(new ButtonLayout(button, activity.Name, 24));
            }
            /*for (int i = 0; i < 250; i++)
            {
                builder.AddLayout(new TextblockLayout("line" + i, 64));
            }*/

            this.SetContent(ScrollLayout.New(builder.Build()));
        }

        private void Button_Clicked(object sender, System.EventArgs e)
        {
            Button button = sender as Button;
            Category activity = this.activityButtons[button];
            this.selected(activity);
        }

        private void selected(Category activity)
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


        Dictionary<Button, Category> activityButtons;
        ActivityDatabase activityDatabase;
    }

    class ActivitySearchView : TitledControl
    {
        public event ActivityChosenHandler ActivityChosen;
        public delegate void ActivityChosenHandler(object sender, Category activity);


        public ActivitySearchView(ActivityDatabase activityDatabase, string name)
        {
            this.SetTitle("");
            nameBox = new ActivityNameEntryBox("Activity");
            nameBox.Database = activityDatabase;

            Button button = new Button();
            button.Clicked += Button_Clicked;
            ButtonLayout buttonLayout = new ButtonLayout(button, name);

            this.SetContent(new Vertical_GridLayout_Builder().AddLayout(nameBox).AddLayout(buttonLayout).Build());
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            if (this.ActivityChosen != null)
            {
                Category activity = this.nameBox.Activity;
                if (activity != null)
                    this.ActivityChosen.Invoke(this, activity);
            }
        }

        ActivityNameEntryBox nameBox;
    }
}
