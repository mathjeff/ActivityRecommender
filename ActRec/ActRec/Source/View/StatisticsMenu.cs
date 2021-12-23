using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    class StatisticsMenu : ContainerLayout
    {

        public event VisitActivitiesScreenHandler VisitActivitiesScreen;
        public delegate void VisitActivitiesScreenHandler();

        public event VisitParticipationScreenHandler VisitParticipationsScreen;
        public delegate void VisitParticipationScreenHandler();

        public event AddParticipationComment_Handler AddParticipationComment;
        public delegate void AddParticipationComment_Handler(ParticipationComment comment);

        public StatisticsMenu(Engine engine, LayoutStack layoutStack, PublicFileIo publicFileIo, UserSettings userSettings)
        {
            this.engine = engine;
            this.layoutStack = layoutStack;
            this.publicFileIo = publicFileIo;
            this.userSettings = userSettings;
        }

        public override SpecificLayout GetBestLayout(LayoutQuery query)
        {
            if (this.ActivityDatabase.ContainsCustomActivity())
            {
                if (this.ActivityDatabase.RootActivity.NumParticipations > 0)
                    this.SubLayout = this.NormalLayout;
                else
                    this.SubLayout = this.NoParticipations_Layout;
            }
            else
            {
                this.SubLayout = this.NoActivities_Layout;
            }
            return base.GetBestLayout(query);
        }

        public List<AppFeature> GetFeatures()
        {
            return new List<AppFeature>() { new AnalysisFeature(this.engine.ActivityDatabase) };
        }

        private LayoutChoice_Set NormalLayout
        {
            get
            {
                if (this.normalContent == null)
                {
                    // These screens are supposed to be ordered from more simple and/or more interesting on top to more complicated and/or less interesting on the bottom
                    MenuLayoutBuilder visualizationBuilder = new MenuLayoutBuilder(this.layoutStack);

                    visualizationBuilder.AddLayout("Significant Activities", new Browse_RecentSignificantActivities_Layout(this.engine, this.engine.RatingSummarizer, this.layoutStack));
                    visualizationBuilder.AddLayout("Favorite Activities", new PreferenceSummaryLayout(this.engine, this.layoutStack, this.publicFileIo));
                    visualizationBuilder.AddLayout("Life Story", new ParticipationSummarizerLayout(this.engine, this.userSettings, this.layoutStack));
                    visualizationBuilder.AddLayout("Efficiency Growth", new EfficiencyTrendLayout(this.engine.EfficiencyCorrelator));

                    visualizationBuilder.AddLayout("Visualize one Activity", new ActivityVisualizationMenu(this.engine, layoutStack));

                    BrowseParticipations_Layout browseLayout = new BrowseParticipations_Layout(this.ActivityDatabase, this.engine, this.engine.RatingSummarizer, this.userSettings, this.layoutStack);
                    browseLayout.AddParticipationComment += BrowseLayout_AddParticipationComment;
                    visualizationBuilder.AddLayout("Search Participations", browseLayout);

                    visualizationBuilder.AddLayout("Cross-Activity Correlations", new ParticipationComparisonMenu(this.layoutStack, this.ActivityDatabase, this.engine));

                    visualizationBuilder.AddLayout("Number of Activities over Time", new NumberOfActivities_Graph(this.ActivityDatabase));

                    this.normalContent = visualizationBuilder.Build();
                }
                return this.normalContent;
            }
        }

        private void BrowseLayout_AddParticipationComment(ParticipationComment comment)
        {
            if (AddParticipationComment != null)
                AddParticipationComment.Invoke(comment);
        }

        private LayoutChoice_Set NoActivities_Layout
        {
            get
            {
                if (this.noActivities_layout == null)
                    this.noActivities_layout = this.makeHelpLayout(false);
                return this.noActivities_layout;
            }
        }
        private LayoutChoice_Set NoParticipations_Layout
        {
            get
            {
                if (this.noParticipations_layout == null)
                    this.noParticipations_layout = this.makeHelpLayout(true);
                return this.noParticipations_layout;
            }
        }
        private LayoutChoice_Set makeHelpLayout(bool hasActivities)
        {
            Vertical_GridLayout_Builder builder = new Vertical_GridLayout_Builder();
            builder.AddLayout(new TextblockLayout("This screen is where you will be able to view statistics about things you've done."));
            builder.AddLayout(
                new HelpButtonLayout("There will be cool graphs!",
                    new HelpWindowBuilder()
                        .AddMessage("After you've entered some data, this screen will allow you analyze your data in lots of cool ways.")
                        .AddMessage("You will be able to view a graph of your data, including how much time you spent, how much you liked it, and " +
                        "how efficient you were.")
                        .AddMessage("You will be able to reminisce about your most favorite events and read any nice comment you entered for them.")
                        .AddMessage("You will be able to search for the most significant events to have happened to you and contemplate how to make the " +
                        "happy events occur more often and the sad events occur less often.")
                    .Build(),
                    this.layoutStack
                )
            );
            builder.AddLayout(new TextblockLayout("Before you can browse things that you've done, you need to go back and do these things first:"));
            if (!hasActivities)
            {
                Button visitActivitiesButton = new Button();
                visitActivitiesButton.Clicked += VisitActivitiesButton_Clicked;
                builder.AddLayout(new ButtonLayout(visitActivitiesButton, "Enter an activity that you like to do"));
            }
            Button recordParticipationsButton = new Button();
            recordParticipationsButton.Clicked += RecordParticipationsButton_Clicked;
            builder.AddLayout(new ButtonLayout(recordParticipationsButton, "Record having participated in an activity"));
            return builder.Build();
        }

        private void VisitActivitiesButton_Clicked(object sender, EventArgs e)
        {
            if (this.VisitActivitiesScreen != null)
                this.VisitActivitiesScreen.Invoke();
        }

        private void RecordParticipationsButton_Clicked(object sender, EventArgs e)
        {
            if (this.VisitParticipationsScreen != null)
                this.VisitParticipationsScreen.Invoke();
        }

        private ActivityDatabase ActivityDatabase
        {
            get
            {
                return this.engine.ActivityDatabase;
            }
        }
        private Engine engine;
        private LayoutStack layoutStack;
        private LayoutChoice_Set normalContent;
        private LayoutChoice_Set noParticipations_layout;
        private LayoutChoice_Set noActivities_layout;
        private PublicFileIo publicFileIo;
        private UserSettings userSettings;
    }

    public class AnalysisFeature : AppFeature
    {
        public AnalysisFeature(ActivityDatabase activityDatabase)
        {
            this.activityDatabase = activityDatabase;
        }
        public string GetDescription()
        {
            // When we start recording whether this feature has been used, we should also split it into a more granular set of features
            return "Analyze your data";
        }

        public bool GetIsUsable()
        {
            if (!this.activityDatabase.ContainsCustomActivity())
                return false;

            if (this.activityDatabase.RootActivity.NumParticipations < 1)
                return false;

            return true;
        }

        public bool GetHasBeenUsed()
        {
            // Not currently recording whether this has been used
            // For the moment we assume that if it can be used then it was used
            return this.GetIsUsable();
        }

        private ActivityDatabase activityDatabase;
    }
}
