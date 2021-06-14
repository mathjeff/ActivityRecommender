using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    class BrowseAll_ProtoActivities_Layout : ContainerLayout
    {
        public BrowseAll_ProtoActivities_Layout(ProtoActivity_Database protoActivity_database, ActivityDatabase activityDatabase, LayoutStack layoutStack)
        {
            this.protoActivity_database = protoActivity_database;
            this.activityDatabase = activityDatabase;
            this.layoutStack = layoutStack;

            Button nextButton = new Button();
            nextButton.Clicked += NextButton_Clicked;
            this.nextButtonLayout = new ButtonLayout(nextButton, "Older");

            Button prevButton = new Button();
            prevButton.Clicked += PrevButton_Clicked;
            this.prevButtonLayout = new ButtonLayout(prevButton, "Newer");

            this.protoActivity_database.TextChanged += ProtoActivity_database_TextChanged;
            this.invalidate();
        }

        private void PrevButton_Clicked(object sender, EventArgs e)
        {
            this.startIndex -= pageCount;
            this.update();
        }

        private void NextButton_Clicked(object sender, EventArgs e)
        {
            this.startIndex += this.pageCount;
            this.update();
        }

        private void invalidate()
        {
            this.SubLayout = null;
            this.startIndex = 0;
            this.protoActivities_by_button = new Dictionary<Button, ProtoActivity>();
        }

        public override SpecificLayout GetBestLayout(LayoutQuery query)
        {
            if (this.SubLayout == null)
                this.update();
            return base.GetBestLayout(query);
        }

        private void update()
        {
            if (this.protoActivity_database.ProtoActivities.Count() < 1)
            {
                this.SubLayout = new TextblockLayout("No protoactivities!");
            }
            else
            {
                GridLayout_Builder builder = new Vertical_GridLayout_Builder().Uniform();
                List<ProtoActivity> protoActivities = new List<ProtoActivity>(protoActivity_database.ProtoActivities);
                protoActivities.Reverse();
                if (this.startIndex < 0)
                    this.startIndex = 0;
                if (startIndex >= protoActivities.Count)
                    startIndex = protoActivities.Count - 1;
                int endIndex = Math.Min(startIndex + this.pageCount, protoActivities.Count);
                if (this.startIndex > 0)
                    builder.AddLayout(this.prevButtonLayout);
                for (int i = startIndex; i < endIndex; i++)
                {
                    builder.AddLayout(this.summarize(protoActivities[i]));
                }
                if (endIndex < protoActivities.Count)
                    builder.AddLayout(this.nextButtonLayout);
                this.SubLayout = ScrollLayout.New(builder.BuildAnyLayout());
            }
        }

        private LayoutChoice_Set summarize(ProtoActivity protoActivity)
        {
            Button button = new Button();
            button.Clicked += Button_Clicked;

            string summary = protoActivity.Summarize();
            this.protoActivities_by_button[button] = protoActivity;
            return new ButtonLayout(button, summary, 16);
        }

        private void ProtoActivity_database_TextChanged()
        {
            this.invalidate();
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            Button button = sender as Button;
            this.edit(this.protoActivities_by_button[button]);
        }

        private void edit(ProtoActivity protoActivity)
        {
            ProtoActivity_Editing_Layout layout = new ProtoActivity_Editing_Layout(protoActivity, this.protoActivity_database, this.activityDatabase, this.layoutStack);
            this.layoutStack.AddLayout(layout, "Browse", layout);
        }

        private ProtoActivity_Database protoActivity_database;
        private ActivityDatabase activityDatabase;
        private LayoutStack layoutStack;
        private Dictionary<Button, ProtoActivity> protoActivities_by_button;
        private int startIndex;
        private ButtonLayout nextButtonLayout;
        private ButtonLayout prevButtonLayout;
        private int pageCount = 10;
    }
}
