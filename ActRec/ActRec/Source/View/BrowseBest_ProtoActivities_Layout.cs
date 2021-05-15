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

            Button explainScore1Button = new Button();
            explainScore1Button.Text = "?";
            explainScore1Button.Clicked += ExplainScore1Button_Clicked;
            this.explainScore1Button = new ButtonLayout(explainScore1Button);

            Button mark2Worse_button = new Button();
            mark2Worse_button.Text = "Worse";
            mark2Worse_button.Clicked += Mark2Worse_button_Clicked;

            Button explainScore2Button = new Button();
            explainScore2Button.Text = "?";
            explainScore2Button.Clicked += ExplainScore2Button_Clicked;
            this.explainScore2Button = new ButtonLayout(explainScore2Button);

            BoundProperty_List rowHeights = BoundProperty_List.Uniform(3);
            BoundProperty_List columnWidths = new BoundProperty_List(3);
            columnWidths.BindIndices(0, 1);
            columnWidths.BindIndices(0, 2);
            columnWidths.SetPropertyScale(0, 1);
            columnWidths.SetPropertyScale(1, 4);
            columnWidths.SetPropertyScale(2, 1);

            GridLayout grid = GridLayout.New(rowHeights, columnWidths, LayoutScore.Zero);

            this.numBrowsesPerProtoactivity_Layout = new TextblockLayout();
            grid.PutLayout(this.numBrowsesPerProtoactivity_Layout, 0, 0);

            this.titleLayout = new TextblockLayout("Browse Best ProtoActivities");
            grid.PutLayout(this.titleLayout, 1, 0);
            LayoutChoice_Set helpButton = new HelpButtonLayout(
                new HelpWindowBuilder()
                .AddMessage("If you have entered any ProtoActivities (which are ideas that are not yet fully-formed enough for you to want them to be suggested), then you can browse them here.")
                .AddMessage("This screen allows you to see the ProtoActivities that ActivityRecommender thinks you consider to be most interesting, and also asks you to choose which one of the top " +
                "two is most interesting to you.")
                .AddMessage("If you want to modify a ProtoActivity, press its Edit button (note that if you make any changes, then this will temporarily dismiss it (by resetting its interest score)).")
                .AddMessage("If you want to see different ProtoActivities, then first you should choose which one (of the two visible ProtoActivities) you like less. Press the button marked " +
                "'Worse' next to the ProtoActivity that you like less. This will cause two new ProtoActivities to appear (by resetting the interest scores of the two currently visible " +
                "ProtoActivities to 0). This will also cause the one you marked 'Worse' to return less often and " +
                "the other one to return more often.")
                .AddMessage("The way that ProtoActivities are chosen in this screen is that each ProtoActivity has a score of how much you like it, and " +
                "a duration since the last time you interacted with that ProtoActivity. The product of the two is its interest score, and the " +
                "ProtoActivities with the highest interest scores are the ones that will be displayed.")
                .AddMessage("If you want to see the calculation of the interest scores of the two current ProtoActivities, press the \"?\" button.")
                .AddMessage("Enjoy!")
                .AddLayout(new CreditsButtonBuilder(layoutStack)
                    .AddContribution(ActRecContributor.ANNI_ZHANG, new DateTime(2020, 5, 28), "Suggested explaining the calculation of protoactivity sort score")
                    .Build()
                 )
                .Build(), 
                layoutStack);
            grid.PutLayout(helpButton, 2, 0);
            grid.PutLayout(new ButtonLayout(edit1Button), 0, 1);
            grid.PutLayout(new ButtonLayout(edit2Button), 0, 2);
            this.activity1Holder = new ContainerLayout();
            grid.PutLayout(this.activity1Holder, 1, 1);
            this.activity2Holder = new ContainerLayout();
            grid.PutLayout(this.activity2Holder, 1, 2);
            this.activity1ScoreBlock = new TextblockLayout();
            this.score1Holder = new ContainerLayout();
            this.score2Holder = new ContainerLayout();
            grid.PutLayout(
                new Vertical_GridLayout_Builder()
                .Uniform()
                .AddLayout(new ButtonLayout(mark1Worse_button))
                .AddLayout(this.score1Holder)
                .BuildAnyLayout()
                , 2, 1);
            this.activity2ScoreBlock = new TextblockLayout();
            grid.PutLayout(
                new Vertical_GridLayout_Builder()
                .Uniform()
                .AddLayout(new ButtonLayout(mark2Worse_button))
                .AddLayout(this.score2Holder)
                .BuildAnyLayout()
                , 2, 2);

            this.multiActivitiesLayout = grid;

            this.protoActivity_database.RatingsChanged += ProtoActivity_database_Changed;
            this.protoActivity_database.TextChanged += ProtoActivity_database_Changed;

            this.singleActivityButton = new Button();
            this.singleActivityButton.Clicked += SingleActivityButton_Clicked;
            this.singleActivityLayout = new ButtonLayout(this.singleActivityButton);

            this.invalidate();
        }

        public List<AppFeature> GetFeatures()
        {
            return new List<AppFeature>() { new CompareProtoactivities_Feature(this.protoActivity_database) };
        }
        private void ExplainScore2Button_Clicked(object sender, EventArgs e)
        {
            this.showScores();
        }

        private void ExplainScore1Button_Clicked(object sender, EventArgs e)
        {
            this.showScores();
        }

        private void showScores()
        {
            this.score1Holder.SubLayout = this.activity1ScoreBlock;
            this.score2Holder.SubLayout = this.activity2ScoreBlock;
        }

        private void SingleActivityButton_Clicked(object sender, EventArgs e)
        {
            this.edit(this.activity1);
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
            if (this.protoActivity_database.Count < 3)
            {
                HelpWindowBuilder builder = new HelpWindowBuilder();
                int numMissing = 3 - this.protoActivity_database.Count;
                if (this.protoActivity_database.Count > 0)
                    builder.AddMessage("Not enough protoactivities! Still need " + numMissing + " more.");

                builder.AddMessage("This screen will allow you to browse your ProtoActivities once you enter some.")
                    .AddMessage("A ProtoActivity is a note where you brainstorm ideas, and which you may later promote into an Activity if you like.")
                    .AddMessage("While you browse ProtoActivities in this screen, you will compare which ones you like more and which ones you like less " +
                    "(you will still be able to edit them, too). This allows ActivityRecommender to show you ideas that you like more often.")
                    .AddMessage("This is a super easy way to save hundreds of ideas for later!")
                    .AddMessage("To create a ProtoActivity, go back to the previous screen.");
                this.SubLayout = builder.Build();
            }
            else
            {
                List<ProtoActivity_EstimatedInterest> top_protoActivities = this.protoActivity_database.GetMostInteresting(2);
                this.SubLayout = this.multiActivitiesLayout;
                double numProtos = this.protoActivity_database.Count;
                this.titleLayout.setText("Protoactivity Tournament: compare 2");

                int numBrowses = this.computeNumBrowses();
                double numBrowsesPerProto = numBrowses / numProtos;
                string browsesPerProto_text = Math.Round(numBrowsesPerProto, 3).ToString();
                this.numBrowsesPerProtoactivity_Layout.setText(browsesPerProto_text + " browses per proto\n(" + numBrowses + " browses,\n" + numProtos + " protos)");

                this.setActivity1(top_protoActivities[0]);
                this.setActivity2(top_protoActivities[1]);
            }
        }
        private int computeNumBrowses()
        {
            // compute the total number of browses among all existent protoactivities
            double totalWeight = 0;
            foreach (ProtoActivity p in this.protoActivity_database.ProtoActivities)
            {
                totalWeight += p.Ratings.Weight;
            }
            return (int)(totalWeight / 2);
        }
        private void setActivity1(ProtoActivity_EstimatedInterest interest)
        {
            this.activity1 = interest.ProtoActivity;
            this.activity1Holder.SubLayout = this.summarize(this.activity1);
            this.activity1ScoreBlock.setText(this.describeScore(interest));
            this.score1Holder.SubLayout = this.explainScore1Button;
        }
        private void setActivity2(ProtoActivity_EstimatedInterest interest)
        {
            this.activity2 = interest.ProtoActivity;
            this.activity2Holder.SubLayout = this.summarize(this.activity2);
            this.activity2ScoreBlock.setText(this.describeScore(interest));
            this.score2Holder.SubLayout = this.explainScore2Button;
        }

        private LayoutChoice_Set summarize(ProtoActivity protoActivity)
        {
            TextblockLayout option1 = new TextblockLayout(protoActivity.Text, true, 16);
            TextblockLayout option2 = new TextblockLayout(protoActivity.Text, 30);
            return new LayoutUnion(option1, option2);
        }

        private string describeScore(ProtoActivity_EstimatedInterest interest)
        {
            double numIdleDays = interest.NumIdleSeconds / 60 / 60 / 24;
            double scoreInDays = interest.CurrentInterest / 60 / 60 / 24;
            return "Estimate score = " + Math.Round(scoreInDays, 0) + " (" + Math.Round(interest.IntrinsicInterest, 2) + " * " + Math.Round(numIdleDays) + " days)";
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
            this.layoutStack.AddLayout(layout, "Proto", layout);
        }

        private ProtoActivity_Database protoActivity_database;
        private ActivityDatabase activityDatabase;

        private ContainerLayout activity1Holder;
        private ContainerLayout activity2Holder;

        private ContainerLayout score1Holder;
        private ContainerLayout score2Holder;
        private TextblockLayout activity1ScoreBlock;
        private TextblockLayout activity2ScoreBlock;
        private ButtonLayout explainScore1Button;
        private ButtonLayout explainScore2Button;

        private TextblockLayout numBrowsesPerProtoactivity_Layout;
        private TextblockLayout titleLayout;
        private LayoutChoice_Set multiActivitiesLayout;
        private ProtoActivity activity1;
        private ProtoActivity activity2;

        private LayoutStack layoutStack;
        private Button singleActivityButton;
        private LayoutChoice_Set singleActivityLayout;
    }

    class CompareProtoactivities_Feature : AppFeature
    {
        public CompareProtoactivities_Feature(ProtoActivity_Database protoactivityDatabase)
        {
            this.protoactivityDatabase = protoactivityDatabase;
        }
        public string GetDescription()
        {
            return "Compare two ProtoActivities";
        }
        public bool GetHasBeenUsed()
        {
            if (this.protoactivityDatabase.Count < 1)
                return false;
            return this.protoactivityDatabase.Get(0).Ratings.Weight > 0;
        }

        public bool GetIsUsable()
        {
            return this.protoactivityDatabase.Count >= 3;
        }

        ProtoActivity_Database protoactivityDatabase;
    }

}
