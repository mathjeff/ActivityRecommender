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
    class ParticipationsCommentView : ContainerLayout
    {
        public ParticipationsCommentView(ActivityDatabase activityDatabase, LayoutStack layoutStack)
        {
            this.activityDatabase = activityDatabase;
            this.layoutStack = layoutStack;

            TextblockLayout helpLayout = new TextblockLayout("Browse the highest-scored participations among those having comments");

            ActivityNameEntryBox categoryBox = new ActivityNameEntryBox("From category (Optional)");
            categoryBox.Database = activityDatabase;
            this.categoryBox = categoryBox;

            Button browseTopActivities_button = new Button();
            ButtonLayout browseTopActivities_layout = new ButtonLayout(browseTopActivities_button, "Browse top " + this.maxNumTopActivitiesToShow + " participations");
            browseTopActivities_button.Clicked += BrowseTopActivities_Button_Clicked;

            Button seeRandomActivity_button = new Button();
            ButtonLayout seeRandomActivity_layout = new ButtonLayout(seeRandomActivity_button, "See random participation (better participations are shown more often)");
            seeRandomActivity_button.Clicked += SeeRandomActivity_button_Clicked;

            this.SubLayout = new Vertical_GridLayout_Builder().Uniform()
                .AddLayout(helpLayout)
                .AddLayout(categoryBox)
                .AddLayout(new Horizontal_GridLayout_Builder().Uniform().AddLayout(browseTopActivities_layout).AddLayout(seeRandomActivity_layout).BuildAnyLayout())
                .Build();
            this.randomGenerator = new Random();
        }

        private void SeeRandomActivity_button_Clicked(object sender, EventArgs e)
        {
            Activity activity = this.categoryBox.Activity;
            if (activity == null)
                activity = this.activityDatabase.RootActivity;
            this.layoutStack.AddLayout(this.chooseParticipation(activity));
        }

        private void BrowseTopActivities_Button_Clicked(object sender, EventArgs e)
        {
            Activity activity = this.categoryBox.Activity;
            if (activity == null)
                activity = this.activityDatabase.RootActivity;

            this.layoutStack.AddLayout(this.summarizeActivity(activity));
        }

        private LayoutChoice_Set Get_NoParticipations_Layout(Activity activity)
        {
            return new TextblockLayout("No commented participations exist for " + activity.Name + "! First record a participation and enter a comment.");
        }

        private LayoutChoice_Set summarizeActivity(Activity activity)
        {
            List<Participation> participations = activity.CommentedParticipationsSortedByDecreasingScore;
            int availableCount = participations.Count;
            if (participations.Count < 1)
                return this.Get_NoParticipations_Layout(activity);

            if (participations.Count > this.maxNumTopActivitiesToShow)
                participations = participations.GetRange(0, maxNumTopActivitiesToShow);

            TitledControl mainView = new TitledControl("Top " + participations.Count + " (of " + availableCount + ") commented participations in " + activity.Name);
            Vertical_GridLayout_Builder gridBuilder = new Vertical_GridLayout_Builder().Uniform();
            foreach (Participation participation in participations)
            {
                gridBuilder.AddLayout(new ParticipationView(participation));
            }
            mainView.SetContent(ScrollLayout.New(gridBuilder.Build()));
            return mainView;
        }

        private LayoutChoice_Set chooseParticipation(Activity activity)
        {
            List<Participation> participations = activity.CommentedParticipationsSortedByDecreasingScore;
            Participation participation = this.chooseInterestingParticipation(participations);
            if (participation == null)
                return this.Get_NoParticipations_Layout(activity);

            TitledControl result = new TitledControl("Remember this?");
            result.SetContent(new ParticipationView(participation));
            return result;
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
        private int maxNumTopActivitiesToShow = 10;
    }
}
