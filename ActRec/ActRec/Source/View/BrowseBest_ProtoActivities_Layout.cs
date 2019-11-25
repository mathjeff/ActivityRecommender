using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    class BrowseBest_ProtoActivities_Layout : ContainerLayout
    {
        public BrowseBest_ProtoActivities_Layout(ProtoActivity_Database protoActivity_database, ActivityDatabase activityDatabase, LayoutStack layoutStack)
        {
            this.protoActivity_database = protoActivity_database;
            this.layoutStack = layoutStack;
            this.activityDatabase = activityDatabase;

            Button edit1Button = new Button();
            edit1Button.Text = "Edit";
            edit1Button.Clicked += Edit1Button_Clicked;

            Button edit2Button = new Button();
            edit2Button.Text = "Edit";
            edit2Button.Clicked += Edit2Button_Clicked;

            Button mark1Worse_button = new Button();
            mark1Worse_button.Text = "Worse";
            mark1Worse_button.Clicked += Mark1Worse_button_Clicked;

            Button mark2Worse_button = new Button();
            mark2Worse_button.Text = "Worse";
            mark2Worse_button.Clicked += Mark2Worse_button_Clicked;

            BoundProperty_List rowHeights = BoundProperty_List.Uniform(3);
            BoundProperty_List columnWidths = new BoundProperty_List(3);
            columnWidths.BindIndices(0, 1);
            columnWidths.BindIndices(0, 2);
            columnWidths.SetPropertyScale(0, 1);
            columnWidths.SetPropertyScale(1, 4);
            columnWidths.SetPropertyScale(2, 1);

            GridLayout grid = GridLayout.New(rowHeights, columnWidths, LayoutScore.Zero);

            this.numProtoactivitiesTextblock = new Label();
            grid.PutLayout(new TextblockLayout(this.numProtoactivitiesTextblock), 0, 0);
            
            grid.PutLayout(new TextblockLayout("Browse Best ProtoActivities"), 1, 0);
            LayoutChoice_Set helpButton = new HelpButtonLayout(
                new HelpWindowBuilder()
                .AddMessage("If you have entered any ProtoActivities (which are ideas that are not yet fully-formed enough for you to want them to be suggested), then you can browse them here.")
                .AddMessage("This screen allows you to see the ProtoActivites that ActivityRecommender thinks you consider to be most interesting, and also asks you to choose which one of the top " +
                "two is most interesting to you.")
                .AddMessage("If you want to modify a ProtoActivity, press its Edit button (note that if you make any changes, then this will temporarily dismiss it (by resetting its interest score)).")
                .AddMessage("If you want to see different ProtoActivities, then first you should choose which one (of the two visible ProtoActivities) you like less. Press the button marked " +
                "'Worse' next to the ProtoActivity that you like less. This will cause two new ProtoActivities to appear (by resetting the interest scores of the two currently visible " +
                "ProtoActivities to 0). This will also cause the one you marked 'Worse' to return less often (by decreasing the rate at which its interest score grows over time) and " +
                "the other one to return more often (by increasing the rate at which its interest score grows over time).")
                .AddMessage("Enjoy!")
                .Build(), 
                layoutStack);
            grid.PutLayout(helpButton, 2, 0);
            grid.PutLayout(new ButtonLayout(edit1Button), 0, 1);
            grid.PutLayout(new ButtonLayout(edit2Button), 0, 2);
            this.activity1Holder = new ContainerLayout();
            grid.PutLayout(this.activity1Holder, 1, 1);
            this.activity2Holder = new ContainerLayout();
            grid.PutLayout(this.activity2Holder, 1, 2);
            grid.PutLayout(new ButtonLayout(mark1Worse_button), 2, 1);
            grid.PutLayout(new ButtonLayout(mark2Worse_button), 2, 2);

            this.multiActivitiesLayout = grid;

            this.protoActivity_database.RatingsChanged += ProtoActivity_database_Changed;
            this.protoActivity_database.TextChanged += ProtoActivity_database_Changed;

            this.invalidate();
        }

        public override SpecificLayout GetBestLayout(LayoutQuery query)
        {
            if (this.SubLayout == null)
                this.update();

            return base.GetBestLayout(query);
        }

        private void ProtoActivity_database_Changed()
        {
            this.invalidate();
        }
        private void invalidate()
        {
            this.SubLayout = null;
        }

        private void update()
        {
            List <ProtoActivity> top_protoActivities = this.protoActivity_database.GetMostInteresting(2);
            if (top_protoActivities.Count == 0)
            {
                this.SubLayout = new TextblockLayout("No ProtoActivities found!");
            }
            else
            {
                if (top_protoActivities.Count == 1) 
                {
                    this.SubLayout = new TextblockLayout(top_protoActivities[0].Text);
                }
                else
                {
                    this.SubLayout = this.multiActivitiesLayout;
                    this.numProtoactivitiesTextblock.Text = "" + top_protoActivities.Count + "/" + this.protoActivity_database.Count;
                    this.setActivity1(top_protoActivities[0]);
                    this.setActivity2(top_protoActivities[1]);
                }
            }
        }
        private void setActivity1(ProtoActivity protoActivity)
        {
            this.activity1 = protoActivity;
            this.activity1Holder.SubLayout = this.summarize(this.activity1);
        }
        private void setActivity2(ProtoActivity protoActivity)
        {
            this.activity2 = protoActivity;
            this.activity2Holder.SubLayout = this.summarize(this.activity2);
        }

        private LayoutChoice_Set summarize(ProtoActivity protoActivity)
        {
            TextblockLayout option1 = new TextblockLayout(protoActivity.Text, true, 16);
            TextblockLayout option2 = new TextblockLayout(protoActivity.Text, 30);
            return new LayoutUnion(option1, option2);
        }

        private void Mark1Worse_button_Clicked(object sender, EventArgs e)
        {
            this.protoActivity_database.MarkWorse(activity1, activity2, DateTime.Now);
        }

        private void Mark2Worse_button_Clicked(object sender, EventArgs e)
        {
            this.protoActivity_database.MarkWorse(activity2, activity1, DateTime.Now);
        }

        private void Edit1Button_Clicked(object sender, EventArgs e)
        {
            this.edit(this.activity1);
        }
        private void Edit2Button_Clicked(object sender, EventArgs e)
        {
            this.edit(this.activity2);
        }

        private void edit(ProtoActivity protoActivity)
        {
            ProtoActivity_Editing_Layout layout = new ProtoActivity_Editing_Layout(protoActivity, this.protoActivity_database, this.activityDatabase, this.layoutStack);
            this.layoutStack.AddLayout(layout, layout);
        }

        private ProtoActivity_Database protoActivity_database;
        private ActivityDatabase activityDatabase;
        private ContainerLayout activity1Holder;
        private ContainerLayout activity2Holder;
        private Label numProtoactivitiesTextblock;
        private LayoutChoice_Set multiActivitiesLayout;
        private ProtoActivity activity1;
        private ProtoActivity activity2;
        private LayoutStack layoutStack;
    }
}
