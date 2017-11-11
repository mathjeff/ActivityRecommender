using System.Collections.Generic;
using VisiPlacement;

namespace ActivityRecommendation.View
{
    class ActivityInheritancesView : TitledControl
    {
        public ActivityInheritancesView(Activity activity)
        {
            List<Activity> children = activity.Children;
            List<Activity> parents = activity.Parents;
            string title;
            if (children.Count < 1)
            {
                title = activity.Name + " is an activity";
            }
            else
            {
                title = activity.Name + " is a category";
            }
            this.SetTitle(title);
            this.TitleBlock.HorizontalTextAlignment = Xamarin.Forms.TextAlignment.Start;


            Horizontal_GridLayout_Builder gridBuilder = new Horizontal_GridLayout_Builder().Uniform();
            if (children.Count > 0)
                gridBuilder.AddLayout(new ActivityListView("Children:", children));
            if (parents.Count > 0)
                gridBuilder.AddLayout(new ActivityListView("Parents:", parents));

            this.SetContent(gridBuilder.Build());
        }
    }

    class ActivityListView : TitledControl
    {
        public ActivityListView(string name, List<Activity> activities)
        {
            Vertical_GridLayout_Builder gridBuilder = new Vertical_GridLayout_Builder();
            foreach (Activity activity in activities)
            {
                gridBuilder.AddLayout(new TextblockLayout(activity.Name, 16));
            }
            LayoutChoice_Set scrollLayout = ScrollLayout.New(gridBuilder.Build());

            this.SetTitle(name);
            this.SetContent(scrollLayout);
        }
    }
}
