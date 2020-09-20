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

            ActivityNameEntryBox categoryBox = new ActivityNameEntryBox("Category", activityDatabase, layoutStack);
            categoryBox.Placeholder("(Optional)");
            this.categoryBox = categoryBox;

            this.sinceDate_box = new DateEntryView("Since", layoutStack);
            this.sinceDate_box.Placeholder("(Optional)");

            this.displayRatings_box = new VisiPlacement.CheckBox("No", "Yes");
            this.displayRatings_box.Checked = true;
            LayoutChoice_Set displayRatings_layout = new Horizontal_GridLayout_Builder()
                .Uniform()
                .AddLayout(new TextblockLayout("Show ratings?"))
                .AddLayout(new ButtonLayout(this.displayRatings_box))
                .BuildAnyLayout();
            this.requireComments_box = new VisiPlacement.CheckBox("No", "Yes");
            this.requireComments_box.Checked = true;
            LayoutChoice_Set requireComments_layout = new Horizontal_GridLayout_Builder()
                .Uniform()
                .AddLayout(new TextblockLayout("Require comments?"))
                .AddLayout(new ButtonLayout(this.requireComments_box))
                .BuildAnyLayout();
            this.requireSuccessful_box = new VisiPlacement.SingleSelect(new List<string>() { "Any", "No Metric", "Successful", "Failed" });
            LayoutChoice_Set requireSuccessful_layout = new Horizontal_GridLayout_Builder()
                .Uniform()
                .AddLayout(new TextblockLayout("Require success status ="))
                .AddLayout(new ButtonLayout(this.requireSuccessful_box))
                .BuildAnyLayout();

            Button browseTopParticipations_button = new Button();
            ButtonLayout browseTopParticipations_layout = new ButtonLayout(browseTopParticipations_button, "Browse top " + this.maxNumTopParticipationsToShow + " highest rated");
            browseTopParticipations_button.Clicked += BrowseTopParticipations_Button_Clicked;

            Button browseExtremeParticipations_button = new Button();
            ButtonLayout browseExtremeParticipations_layout = new ButtonLayout(browseExtremeParticipations_button, "Browse " + this.maxNumTopParticipationsToShow + " best/worst");
            browseExtremeParticipations_button.Clicked += BrowseExtremeParticipations_button_Clicked;
            

            Button seeGoodRandomParticipation_button = new Button();
            ButtonLayout seeGoodRandomParticipation_layout = new ButtonLayout(seeGoodRandomParticipation_button, "See a random good one (better ones appear more often)");
            seeGoodRandomParticipation_button.Clicked += SeeGoodRandomParticipation_Clicked;

            Button seeRandomParticipations_button = new Button();
            ButtonLayout seeRandomParticipations_layout = new ButtonLayout(seeRandomParticipations_button, "See " + this.maxNumRandomActivitiesToShow + " chosen (uniformly) at random");
            seeRandomParticipations_button.Clicked += SeeRandomParticipations_button_Clicked;

            this.SubLayout = new Vertical_GridLayout_Builder()
                .AddLayout(helpLayout)
                .AddLayout(categoryBox)
                .AddLayout(sinceDate_box)
                .AddLayout(displayRatings_layout)
                .AddLayout(requireComments_layout)
                .AddLayout(requireSuccessful_layout)
                .AddLayout(
                    new Horizontal_GridLayout_Builder()
                    .Uniform()
                    .AddLayout(browseTopParticipations_layout)
                    .AddLayout(browseExtremeParticipations_layout)
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
            this.layoutStack.AddLayout(this.make_randomParticipations_layout(), "Some Times");
        }

        private void SeeGoodRandomParticipation_Clicked(object sender, EventArgs e)
        {
            this.layoutStack.AddLayout(this.make_goodRandomParticipation_layout(), "Good Times");
        }

        private void BrowseTopParticipations_Button_Clicked(object sender, EventArgs e)
        {
            this.layoutStack.AddLayout(this.make_topParticipations_layout(), "Best Times");
        }

        private void BrowseExtremeParticipations_button_Clicked(object sender, EventArgs e)
        {
            this.layoutStack.AddLayout(this.make_extremeParticipations_layout(), "Significant Times");
        }

        private LayoutChoice_Set Get_NoParticipations_Layout()
        {
            return new TextblockLayout("No matching participations exist for " + this.Category.Name + "! First record a participation.");
        }

        // returns the participations specified by the user
        private List<Participation> Participations
        {
            get
            {
                // choose activity to ask
                Activity activity = this.Category;
                List<Participation> participationsSinceDate;
                // filter by date
                if (this.sinceDate_box.IsDateValid())
                    participationsSinceDate = activity.getParticipationsSince(this.sinceDate_box.GetDate());
                else
                    participationsSinceDate = activity.Participations;
                // filter uncommented participations
                List<Participation> commentedParticipations;
                if (this.requireComments_box.Checked)
                {
                    commentedParticipations = new List<Participation>();
                    foreach (Participation participation in participationsSinceDate)
                    {
                        if (participation.Comment != null)
                            commentedParticipations.Add(participation);
                    }
                }
                else
                {
                    commentedParticipations = participationsSinceDate;
                }
                // filter participations based on whether they succeeded or failed
                List<Participation> correctSuccess_participations;
                if (this.requireSuccessful_box.SelectedIndex == 0)
                {
                    // no filter, include all
                    correctSuccess_participations = commentedParticipations;
                }
                else if (this.requireSuccessful_box.SelectedIndex == 1)
                {
                    // filter to participations that had no metric
                    correctSuccess_participations = new List<Participation>();
                    foreach (Participation participation in commentedParticipations)
                    {
                        if (participation.EffectivenessMeasurement == null)
                            correctSuccess_participations.Add(participation);
                    }
                }
                else
                {
                    // filter to participations with metrics and the appropriate success or failure status
                    correctSuccess_participations = new List<Participation>();
                    bool requirement = this.requireSuccessful_box.SelectedIndex == 2;
                    foreach (Participation participation in commentedParticipations)
                    {
                        if (participation.EffectivenessMeasurement != null)
                        {
                            if (participation.CompletedMetric == requirement)
                            {
                                correctSuccess_participations.Add(participation);
                            }
                        }
                    }
                }
                return correctSuccess_participations;
            }
        }

        private void SortByDecreasingScore(List<Participation> participations)
        {
            participations.Sort(new ParticipationScoreComparer());
            participations.Reverse();
        }


        private List<Participation> ParticipationsSortedByDecreasingScore
        {
            get
            {
                List<Participation> participations = this.Participations;
                this.SortByDecreasingScore(participations);
                return participations;
            }
        }

        private LayoutChoice_Set make_topParticipations_layout()
        {
            List<Participation> participations = this.ParticipationsSortedByDecreasingScore;
            int availableCount = participations.Count;
            if (participations.Count < 1)
                return this.Get_NoParticipations_Layout();

            if (participations.Count > this.maxNumTopParticipationsToShow)
                participations = participations.GetRange(0, this.maxNumTopParticipationsToShow);

            TitledControl mainView = new TitledControl("Top " + participations.Count + " (of " + availableCount + ") matching participations in " + this.Category.Name, 30);
            mainView.SetContent(new ListParticipations_Layout(participations, this.ShowRatings, this.randomGenerator));
            return mainView;
        }

        private LayoutChoice_Set make_extremeParticipations_layout()
        {
            List<Participation> participations = this.ParticipationsSortedByDecreasingScore;
            int availableCount = participations.Count;
            if (participations.Count < 1)
                return this.Get_NoParticipations_Layout();

            double averageScore = this.activityDatabase.RootActivity.Ratings.Mean;

            int lowIndex = 0;
            int highIndex = participations.Count - 1;
            List<Participation> chosenParticipations = new List<Participation>();
            while (chosenParticipations.Count < this.maxNumTopParticipationsToShow && lowIndex <= highIndex)
            {
                Participation low = participations[lowIndex];
                AbsoluteRating lowRating = low.GetAbsoluteRating();
                if (lowRating == null)
                {
                    lowIndex++;
                    continue;
                }
                double lowDifference = Math.Abs(lowRating.Score - averageScore);
                Participation high = participations[highIndex];
                AbsoluteRating highRating = high.GetAbsoluteRating();
                if (highRating == null)
                {
                    highIndex--;
                    continue;
                }
                double highDifference = Math.Abs(highRating.Score - averageScore);
                if (lowDifference > highDifference)
                {
                    chosenParticipations.Add(low);
                    lowIndex++;
                }
                else
                {
                    chosenParticipations.Add(high);
                    highIndex--;
                }
            }

            TitledControl mainView = new TitledControl("" + chosenParticipations.Count + " most extreme (of " + availableCount + ") matching participations in " + this.Category.Name, 30);
            mainView.SetContent(new ListParticipations_Layout(chosenParticipations, this.ShowRatings, this.randomGenerator));
            return mainView;
        }

        private LayoutChoice_Set make_goodRandomParticipation_layout()
        {
            List<Participation> participations = this.ParticipationsSortedByDecreasingScore;
            Participation participation = this.chooseInterestingParticipation(participations);
            if (participation == null)
                return this.Get_NoParticipations_Layout();

            TitledControl result = new TitledControl("Remember this?", 30);
            result.SetContent(new ListParticipations_Layout(new List<Participation>() { participation }, this.ShowRatings, this.randomGenerator));
            return result;
        }

        private LayoutChoice_Set make_randomParticipations_layout()
        {
            List<Participation> participations = this.Participations;

            int availableCount = participations.Count;
            if (participations.Count < 1)
                return this.Get_NoParticipations_Layout();

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

            TitledControl mainView = new TitledControl("" + participations.Count + " random participations (of " + availableCount + " matches) in " + this.Category.Name, 30);
            mainView.SetContent(new ListParticipations_Layout(participations, this.ShowRatings, this.randomGenerator));
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
        private VisiPlacement.CheckBox displayRatings_box;
        private VisiPlacement.CheckBox requireComments_box;
        private VisiPlacement.SingleSelect requireSuccessful_box;
        private DateEntryView sinceDate_box;
    }
}
