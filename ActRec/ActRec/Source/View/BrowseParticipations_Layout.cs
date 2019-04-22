using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisiPlacement;
using Xamarin.Forms;

// a ParticipationsCommentView lets the user see the comments for their favorite participations
namespace ActivityRecommendation.View
{
    class BrowseParticipations_Layout : ContainerLayout
    {
        public BrowseParticipations_Layout(ActivityDatabase activityDatabase, LayoutStack layoutStack)
        {
            this.activityDatabase = activityDatabase;
            this.layoutStack = layoutStack;

            TextblockLayout helpLayout = new TextblockLayout("Browse participations");

            ActivityNameEntryBox categoryBox = new ActivityNameEntryBox("From category (Optional)");
            categoryBox.Database = activityDatabase;
            this.categoryBox = categoryBox;

            this.displayRatings_box = new CheckBox("No", "Yes");
            this.displayRatings_box.Checked = true;
            LayoutChoice_Set displayRatings_layout = new Horizontal_GridLayout_Builder()
                .Uniform()
                .AddLayout(new TextblockLayout("Show ratings?"))
                .AddLayout(new ButtonLayout(this.displayRatings_box))
                .BuildAnyLayout();

            Button browseTopActivities_button = new Button();
            ButtonLayout browseTopActivities_layout = new ButtonLayout(browseTopActivities_button, "Browse top " + this.maxNumTopParticipationsToShow + " highest rated participations");
            browseTopActivities_button.Clicked += BrowseTopActivities_Button_Clicked;

            Button seeGoodRandomParticipation_button = new Button();
            ButtonLayout seeGoodRandomParticipation_layout = new ButtonLayout(seeGoodRandomParticipation_button, "See one good participation (better participations appear more often)");
            seeGoodRandomParticipation_button.Clicked += SeeGoodRandomParticipation_Clicked;

            Button seeRandomParticipations_button = new Button();
            ButtonLayout seeRandomParticipations_layout = new ButtonLayout(seeRandomParticipations_button, "See " + this.maxNumRandomActivitiesToShow + " participations chosen (uniformly) at random");
            seeRandomParticipations_button.Clicked += SeeRandomParticipations_button_Clicked;

            this.SubLayout = new Vertical_GridLayout_Builder().Uniform()
                .AddLayout(helpLayout)
                .AddLayout(categoryBox)
                .AddLayout(displayRatings_layout)
                .AddLayout(
                    new Horizontal_GridLayout_Builder()
                    .Uniform()
                    .AddLayout(browseTopActivities_layout)
                    .AddLayout(seeGoodRandomParticipation_layout)
                    .AddLayout(seeRandomParticipations_layout)
                    .BuildAnyLayout()
                )
                .Build();
            this.randomGenerator = new Random();
        }

        private Activity Category
        {
            get
            {
                Activity activity = this.categoryBox.Activity;
                if (activity == null)
                    activity = this.activityDatabase.RootActivity;
                return activity;
            }
        }

        private bool ShowRatings
        {
            get
            {
                return this.displayRatings_box.Checked;
            }
        }

        private void SeeRandomParticipations_button_Clicked(object sender, EventArgs e)
        {
            this.layoutStack.AddLayout(this.make_randomParticipations_layout(this.Category, this.ShowRatings));
        }

        private void SeeGoodRandomParticipation_Clicked(object sender, EventArgs e)
        {
            this.layoutStack.AddLayout(this.make_goodRandomParticipation_layout(this.Category, this.ShowRatings));
        }

        private void BrowseTopActivities_Button_Clicked(object sender, EventArgs e)
        {
            this.layoutStack.AddLayout(this.make_topParticipations_layout(this.Category, this.ShowRatings));
        }

        private LayoutChoice_Set Get_NoParticipations_Layout(Activity activity)
        {
            return new TextblockLayout("No matching participations exist for " + activity.Name + "! First record a participation.");
        }

        private LayoutChoice_Set make_topParticipations_layout(Activity activity, bool showRatings)
        {
            List<Participation> participations = activity.CommentedParticipationsSortedByDecreasingScore;
            int availableCount = participations.Count;
            if (participations.Count < 1)
                return this.Get_NoParticipations_Layout(activity);

            if (participations.Count > this.maxNumTopParticipationsToShow)
                participations = participations.GetRange(0, this.maxNumTopParticipationsToShow);

            TitledControl mainView = new TitledControl("Top " + participations.Count + " (of " + availableCount + ") matching participations in " + activity.Name);
            mainView.SetContent(new ListParticipations_Layout(participations, showRatings, this.randomGenerator));
            return mainView;
        }

        private LayoutChoice_Set make_goodRandomParticipation_layout(Activity activity, bool showRatings)
        {
            List<Participation> participations = activity.CommentedParticipationsSortedByDecreasingScore;
            Participation participation = this.chooseInterestingParticipation(participations);
            if (participation == null)
                return this.Get_NoParticipations_Layout(activity);

            TitledControl result = new TitledControl("Remember this?");
            result.SetContent(new ListParticipations_Layout(new List<Participation>() { participation }, showRatings, this.randomGenerator));
            return result;
        }

        private LayoutChoice_Set make_randomParticipations_layout(Activity activity, bool showRatings)
        {
            List<Participation> participations = activity.CommentedParticipations;

            int availableCount = participations.Count;
            if (participations.Count < 1)
                return this.Get_NoParticipations_Layout(activity);

            // randomize the first few participations
            int numParticipationsToShow = Math.Min(this.maxNumRandomActivitiesToShow, participations.Count);
            for (int i = 0; i < numParticipationsToShow; i++)
            {
                Participation temp = participations[i];
                int j = this.randomGenerator.Next(i, participations.Count);
                participations[i] = participations[j];
                participations[j] = temp;
            }

            if (participations.Count > numParticipationsToShow)
                participations = participations.GetRange(0, numParticipationsToShow);

            TitledControl mainView = new TitledControl("" + participations.Count + " random participations (of " + availableCount + " matches) in " + activity.Name);
            mainView.SetContent(new ListParticipations_Layout(participations, showRatings, this.randomGenerator));
            return mainView;
        }


        // Chooses a random participation to return
        // Is more likely to return a participation earlier in the list
        private Participation chooseInterestingParticipation(List<Participation> sortedParticipations)
        {
            if (sortedParticipations.Count < 1)
                return null;
            // Try to only report a participation in the first half of the list if possible
            int maxCount = sortedParticipations.Count / 2;
            if (maxCount < 1)
                maxCount = 1;
            // Choose a random participation, weighting the front of the list more heavily
            double random = this.randomGenerator.NextDouble();
            double skewedRandom = 1 - Math.Sqrt(random);
            int index = (int)((double)maxCount * skewedRandom);
            return sortedParticipations[index];
        }

        private ActivityDatabase activityDatabase;
        private LayoutStack layoutStack;
        private ActivityNameEntryBox categoryBox;
        private Random randomGenerator;
        private int maxNumTopParticipationsToShow = 10;
        private int maxNumRandomActivitiesToShow = 2;
        private CheckBox displayRatings_box;
    }
}
