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
        public BrowseParticipations_Layout(ActivityDatabase activityDatabase, Engine engine, ScoreSummarizer scoreSummarizer, LayoutStack layoutStack)
        {
            this.activityDatabase = activityDatabase;
            this.engine = engine;
            this.layoutStack = layoutStack;
            this.scoreSummarizer = scoreSummarizer;

            TextblockLayout helpLayout = new TextblockLayout("Browse participations");

            ActivityNameEntryBox categoryBox = new ActivityNameEntryBox("Category", activityDatabase, layoutStack, false, false);
            categoryBox.Placeholder("(Optional)");
            this.categoryBox = categoryBox;

            this.sinceDate_box = new DateEntryView("Since", layoutStack, false);
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
            this.sortBy_box = new VisiPlacement.SingleSelect(new List<string>() { this.sortByFun_text, this.sortBy_netPresentHappiness_text, this.sortByEfficiency_text });
            LayoutChoice_Set sortBy_layout = new Horizontal_GridLayout_Builder()
                .Uniform()
                .AddLayout(new TextblockLayout("Sort by"))
                .AddLayout(new ButtonLayout(this.sortBy_box))
                .BuildAnyLayout();
            

            Button browseTopParticipations_button = new Button();
            ButtonLayout browseTopParticipations_layout = new ButtonLayout(browseTopParticipations_button, "Top " + this.maxNumTopParticipationsToShow);
            browseTopParticipations_button.Clicked += BrowseTopParticipations_Button_Clicked;

            Button browseExtremeParticipations_button = new Button();
            ButtonLayout browseExtremeParticipations_layout = new ButtonLayout(browseExtremeParticipations_button, "" + this.maxNumTopParticipationsToShow + " best/worst");
            browseExtremeParticipations_button.Clicked += BrowseExtremeParticipations_button_Clicked;
            

            Button seeGoodRandomParticipation_button = new Button();
            ButtonLayout seeGoodRandomParticipation_layout = new ButtonLayout(seeGoodRandomParticipation_button, "A random, probably good one");
            seeGoodRandomParticipation_button.Clicked += SeeGoodRandomParticipation_Clicked;

            Button seeRandomParticipations_button = new Button();
            ButtonLayout seeRandomParticipations_layout = new ButtonLayout(seeRandomParticipations_button, "" + this.maxNumRandomActivitiesToShow + " (uniformly) random");
            seeRandomParticipations_button.Clicked += SeeRandomParticipations_button_Clicked;

            this.SubLayout = new Vertical_GridLayout_Builder()
                .AddLayout(helpLayout)
                .AddLayout(categoryBox)
                .AddLayout(sinceDate_box)
                .AddLayout(displayRatings_layout)
                .AddLayout(requireComments_layout)
                .AddLayout(requireSuccessful_layout)
                .AddLayout(sortBy_layout)
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
                // filter based on whether the participations have the property we're interested in
                List<Participation> hasProperty_participations = new List<Participation>();
                if (this.sortsByFun)
                {
                    foreach (Participation participation in correctSuccess_participations)
                    {
                        if (participation.GetAbsoluteRating() != null)
                            hasProperty_participations.Add(participation);
                    }
                }
                else
                {
                    if (this.sortsByEfficiency)
                    {
                        foreach (Participation participation in correctSuccess_participations)
                        {
                            if (participation.RelativeEfficiencyMeasurement != null)
                            {
                                if (participation.DismissedActivity && !participation.CompletedMetric)
                                {
                                    // If the participation dismissed the activity without completing it, then this participation really just recorded that this activity was obsolete,
                                    // which doesn't really count as low efficiency
                                }
                                else
                                {
                                    hasProperty_participations.Add(participation);
                                }
                            }
                        }
                    }
                    else
                    {
                        // sorts by net present happiness

                        // When sorting by net present happiness, the only requirement is that we have a happiness estimate.
                        // The existence of the participation is enough information for a happiness estimate
                        hasProperty_participations = correctSuccess_participations;
                    }
                }

                return hasProperty_participations;
            }
        }

        private void SortByDecreasingScore(List<Participation> participations)
        {
            participations.Sort(new ParticipationScoreComparer());
            participations.Reverse();
        }

        private void SortByDecreasingEfficiency(List<Participation> participations)
        {
            participations.Sort(new ParticipationEfficiencyComparer());
            participations.Reverse();
        }

        private void SortByDecreasing_NetPresentHappiness(List<Participation> participations)
        {
            participations.Sort(new Participation_NetPresentHappiness_Comparer(this.scoreSummarizer));
            participations.Reverse();
        }

        private void Sort(List<Participation> participations)
        {
            if (this.sortsByFun)
            {
                this.SortByDecreasingScore(participations);
                return;
            }
            if (this.sortsByEfficiency)
            {
                this.SortByDecreasingEfficiency(participations);
                return;
            }
            this.SortByDecreasing_NetPresentHappiness(participations);

        }


        private List<Participation> SortedParticipations
        {
            get
            {
                List<Participation> participations = this.Participations;
                this.Sort(participations);
                return participations;
            }
        }

        private LayoutChoice_Set make_topParticipations_layout()
        {
            List<Participation> participations = this.SortedParticipations;
            int availableCount = participations.Count;
            if (participations.Count < 1)
                return this.Get_NoParticipations_Layout();

            if (participations.Count > this.maxNumTopParticipationsToShow)
                participations = participations.GetRange(0, this.maxNumTopParticipationsToShow);

            TitledControl mainView = new TitledControl("" + participations.Count + " matching participations with highest " + this.sortBy_box.SelectedItem + " (of " + availableCount + ") in " + this.Category.Name, 30);
            mainView.SetContent(new ListParticipations_Layout(participations, this.ShowRatings, this.engine, this.scoreSummarizer, this.layoutStack, this.randomGenerator));
            return mainView;
        }

        private LayoutChoice_Set make_extremeParticipations_layout()
        {
            // Find the participations, filter them, and sort them
            List<Participation> participations = this.SortedParticipations;
            int availableCount = participations.Count;
            if (participations.Count < 1)
                return this.Get_NoParticipations_Layout();

            // determine how many best and worst participations to show
            int maxCount = Math.Min(participations.Count, this.maxNumTopParticipationsToShow);
            int lowCount = maxCount / 2;
            int highCount = maxCount - lowCount;

            // get the best and worst participations
            // The participations are already in the right order (decreasing score), we just want to take the beginning and end of the list
            List<Participation> highParticipations = participations.GetRange(0, lowCount);
            List<Participation> lowParticipations = participations.GetRange(participations.Count - highCount, highCount);
            List<Participation> chosenParticipations = highParticipations;
            chosenParticipations.AddRange(lowParticipations);

            // build layout
            TitledControl mainView = new TitledControl("" + chosenParticipations.Count + " matching participations with most extreme " + this.sortBy_box.SelectedItem.ToLower() + " (of " + availableCount + ") in " + this.Category.Name, 30);
            mainView.SetContent(new ListParticipations_Layout(chosenParticipations, this.ShowRatings, this.engine, this.scoreSummarizer, this.layoutStack, this.randomGenerator));
            return mainView;
        }

        private LayoutChoice_Set make_goodRandomParticipation_layout()
        {
            List<Participation> participations = this.SortedParticipations;
            Participation participation = this.chooseInterestingParticipation(participations);
            if (participation == null)
                return this.Get_NoParticipations_Layout();

            TitledControl result = new TitledControl("Remember this?", 30);
            result.SetContent(new ListParticipations_Layout(new List<Participation>() { participation }, this.ShowRatings, this.engine, this.scoreSummarizer, this.layoutStack, this.randomGenerator));
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
            mainView.SetContent(new ListParticipations_Layout(participations, this.ShowRatings, this.engine, this.scoreSummarizer, this.layoutStack, this.randomGenerator));
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

        private string sortByFun_text = "Fun";
        private string sortByEfficiency_text = "Efficiency";
        private string sortBy_netPresentHappiness_text = "Future Happiness";
        private bool sortsByFun
        {
            get
            {
                return this.sortBy_box.SelectedItem == this.sortByFun_text;
            }
        }
        private bool sortsByEfficiency
        {
            get
            {
                return this.sortBy_box.SelectedItem == this.sortByEfficiency_text;
            }
        }
        private bool sortsBy_netPresentHappiness
        {
            get
            {
                return this.sortBy_box.SelectedItem == this.sortBy_netPresentHappiness_text;
            }
        }

        private ActivityDatabase activityDatabase;
        private Engine engine;
        private LayoutStack layoutStack;
        private ActivityNameEntryBox categoryBox;
        private Random randomGenerator;
        private ScoreSummarizer scoreSummarizer;
        private int maxNumTopParticipationsToShow = 10;
        private int maxNumRandomActivitiesToShow = 2;
        private VisiPlacement.CheckBox displayRatings_box;
        private VisiPlacement.CheckBox requireComments_box;
        private VisiPlacement.SingleSelect requireSuccessful_box;
        private VisiPlacement.SingleSelect sortBy_box;
        private DateEntryView sinceDate_box;
    }
}
