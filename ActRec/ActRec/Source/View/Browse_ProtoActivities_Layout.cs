using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    class Browse_ProtoActivities_Layout : ContainerLayout
    {
        public Browse_ProtoActivities_Layout(ProtoActivity_Database protoActivity_database, LayoutStack layoutStack)
        {
            this.protoActivity_database = protoActivity_database;
            this.layoutStack = layoutStack;

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
            grid.PutLayout(new TextblockLayout("Browse ProtoActivities"), 1, 0);
            LayoutChoice_Set helpButton = new HelpButtonLayout(
                new HelpWindowBuilder()
                .AddMessage("If you have entered any ProtoActivities (which are ideas that are not yet fully-formed enough for you to want them to be " +
                "suggested), then you can browse them here.")
                .AddMessage("The ProtoActivities you see more often will be ones that seem most interesting to you")
                .AddMessage("If you want to modify a ProtoActivity, press its Edit button.")
                .AddMessage("If you want to see different ProtoActivities, then first you have to choose which one you like less. Press the button marked " +
                "'Worse' next to the ProtoActivity you like less. This will not only dismiss both ProtoActivities, but will also change the frequence of " +
                "the two ProtoActivities that were showing. The one you marked 'Worse' will show up on this screen less often and the other one will show " +
                "up more often.")
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
            TextblockLayout option1 = new TextblockLayout(protoActivity.Text, 16);
            option1.ScoreIfCropped = true;
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
            ProtoActivity_Editing_Layout layout = new ProtoActivity_Editing_Layout(protoActivity, this.protoActivity_database);
            this.layoutStack.AddLayout(layout, layout);
        }


        private ProtoActivity_Database protoActivity_database;
        private ContainerLayout activity1Holder;
        private ContainerLayout activity2Holder;
        private LayoutChoice_Set multiActivitiesLayout;
        private ProtoActivity activity1;
        private ProtoActivity activity2;
        private LayoutStack layoutStack;
    }
}
