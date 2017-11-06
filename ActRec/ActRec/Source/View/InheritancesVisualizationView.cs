using System.Collections.Generic;
using VisiPlacement;

namespace ActivityRecommendation
{
    class InheritancesVisualizationView : TitledControl
    {
        public InheritancesVisualizationView(ActivityDatabase activityDatabase)
        {
            this.SetTitle("Leaf Activities You Have Entered");
            //this.SetTitle("");
            this.activityDatabase = activityDatabase;
            this.activityDatabase.ActivityAdded += ActivityDatabase_ActivityAdded;
        }

        private void ActivityDatabase_ActivityAdded(object sender, System.EventArgs e)
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
            List<Activity> leafActivities = this.activityDatabase.LeafActivities;
            Vertical_GridLayout_Builder builder = new Vertical_GridLayout_Builder().Uniform();
            foreach (Activity activity in leafActivities)
            {
                builder.AddLayout(new TextblockLayout(activity.Name, 16));
            }
            /*for (int i = 0; i < 250; i++)
            {
                builder.AddLayout(new TextblockLayout("line" + i, 64));
            }*/

            this.SetContent(ScrollLayout.New(builder.Build()));
        }


        private void invalidateChildren()
        {
            this.SetContent(null);
        }

        ActivityDatabase activityDatabase;
    }
}
