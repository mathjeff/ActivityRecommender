using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisiPlacement;
using Xamarin.Forms;

// A RequestSuggestion_Layout shows a user interface for the user to request a suggestion.
// It doesn't also show the suggestions themselves; that's done in a SuggestionsView or a SuggestionView
namespace ActivityRecommendation.View
{
    public class RequestSuggestion_Layout: ContainerLayout, OnBack_Listener
    {
        public event RequestSuggestion_Handler RequestSuggestion;
        public delegate void RequestSuggestion_Handler(ActivityRequest activityRequest);

        public RequestSuggestion_Layout(ActivityDatabase activityDatabase, bool allowRequestingActivitiesDirectly, bool allowMultipleSuggestionTypes, bool vertical,
            Engine engine, LayoutStack layoutStack)
        {
            Button suggestionButton = new Button();
            suggestionButton.Clicked += SuggestBestActivity_Clicked;
            ButtonLayout suggest_maxLongtermHappiness_button;
            if (allowMultipleSuggestionTypes)
                suggest_maxLongtermHappiness_button = new ButtonLayout(suggestionButton, "Suggest Best");
            else
                suggest_maxLongtermHappiness_button = new ButtonLayout(suggestionButton, "Suggest");
            LayoutChoice_Set suggestButton_layout;
            if (allowRequestingActivitiesDirectly)
            {

                Vertical_GridLayout_Builder builder = new Vertical_GridLayout_Builder().Uniform().AddLayout(suggest_maxLongtermHappiness_button);
                if (allowMultipleSuggestionTypes)
                {
                    Button suggestionButton2 = new Button();
                    suggestionButton2.Clicked += SuggestMostLikelyActivity_Clicked;
                    ButtonLayout suggest_mostLikely_button = new ButtonLayout(suggestionButton2, "Suggest Most Likely");
                    builder.AddLayout(suggest_mostLikely_button);

                    Button suggestionButton3 = new Button();
                    suggestionButton3.Clicked += SuggestMostEfficientActivity_Clicked;
                    ButtonLayout suggest_mostEfficient_button = new ButtonLayout(suggestionButton3, "Suggest For Most Future Efficiency");
                    builder.AddLayout(suggest_mostEfficient_button);
                }
                suggestButton_layout = builder.BuildAnyLayout();
            }
            else
            {
                suggestButton_layout = suggest_maxLongtermHappiness_button;
            }

            this.categoryBox = new ActivityNameEntryBox("Category:", activityDatabase, layoutStack);
            this.categoryBox.Placeholder("(Optional)");
            this.activityDatabase = activityDatabase;
            this.engine = engine;
            this.layoutStack = layoutStack;


            if (!allowRequestingActivitiesDirectly)
            {
                this.SubLayout = suggest_maxLongtermHappiness_button;
            }
            else
            {
                GridLayout configurationLayout = GridLayout.New(BoundProperty_List.Uniform(2), new BoundProperty_List(1), LayoutScore.Zero);
                configurationLayout.AddLayout(this.categoryBox);


                this.atLeastAsFunAs_button = new Button();
                atLeastAsFunAs_button.Clicked += RequestAsFunAs_Button_Clicked;
                configurationLayout.AddLayout(new TitledControl("As fun as:", new ButtonLayout(atLeastAsFunAs_button)));

                if (vertical)
                {
                    GridLayout verticalContentLayout = GridLayout.New(new BoundProperty_List(2), new BoundProperty_List(1), LayoutScore.Get_UnCentered_LayoutScore(2));
                    verticalContentLayout.AddLayout(configurationLayout);
                    verticalContentLayout.AddLayout(suggestButton_layout);
                    this.SubLayout = verticalContentLayout;
                }
                else
                {
                    GridLayout horizontalContentLayout = GridLayout.New(new BoundProperty_List(1), new BoundProperty_List(2), LayoutScore.Get_UnCentered_LayoutScore(1));
                    horizontalContentLayout.AddLayout(suggestButton_layout);
                    horizontalContentLayout.AddLayout(configurationLayout);

                    this.SubLayout = horizontalContentLayout;
                }
                this.specify_AtLeastAsFunAs_Layout = new Specify_AtLeastAsFunAs_Layout(this.activityDatabase, this.layoutStack);
                this.update_atLeastAsFunAs_activity();
            }

        }

        public Participation LatestParticipation
        {
            set
            {
                if (this.specify_AtLeastAsFunAs_Layout != null)
                    this.specify_AtLeastAsFunAs_Layout.LatestParticipation = value;
            }
        }

        private void RequestAsFunAs_Button_Clicked(object sender, EventArgs e)
        {
            this.layoutStack.AddLayout(new StackEntry(this.specify_AtLeastAsFunAs_Layout, ">Fun", this));
        }

        private void SuggestBestActivity_Clicked(object sender, EventArgs e)
        {
            this.Suggest(ActivityRequestOptimizationProperty.LONGTERM_HAPPINESS);
        }

        private void SuggestMostLikelyActivity_Clicked(object sender, EventArgs e)
        {
            this.Suggest(ActivityRequestOptimizationProperty.PARTICIPATION_PROBABILITY);
        }

        private void SuggestMostEfficientActivity_Clicked(object sender, EventArgs e)
        {
            this.Suggest(ActivityRequestOptimizationProperty.LONGTERM_EFFICIENCY);
        }

        private void Suggest(ActivityRequestOptimizationProperty optimizationProperty)
        {
            ActivityRequest activityRequest = new ActivityRequest(this.categoryBox.Activity, this.atLeastAsFunAs_activity, DateTime.Now, optimizationProperty);
            if (this.atLeastAsFunAs_activity != null)
            {
                DateTime startDate = activityRequest.Date;
                DateTime hypotheticalEndDate = this.engine.GuessParticipationEndDate(this.atLeastAsFunAs_activity, startDate);
                Participation hypotheticalParticipation = new Participation(startDate, hypotheticalEndDate, activityRequest.ActivityToBeat);
                hypotheticalParticipation.Hypothetical = true;

                Rating userPredictedRating = this.specify_AtLeastAsFunAs_Layout.EstimatedRating_Box.GetRating(this.engine, hypotheticalParticipation);
                activityRequest.UserPredictedRating = userPredictedRating;
            }
            this.RequestSuggestion.Invoke(activityRequest);
        }

        public void OnBack(LayoutChoice_Set layout)
        {
            this.update_atLeastAsFunAs_activity();
        }

        private void update_atLeastAsFunAs_activity()
        {
            this.atLeastAsFunAs_activity = this.specify_AtLeastAsFunAs_Layout.Activity;
            if (this.atLeastAsFunAs_activity == null)
            {
                this.atLeastAsFunAs_button.Text = "(Optional)";
            }
            else
            {
                string text = this.atLeastAsFunAs_activity.Name;
                double? ratio = this.specify_AtLeastAsFunAs_Layout.Ratio;
                if (ratio != null)
                {
                    text += " (" + ratio + "x prev)";
                }
                this.atLeastAsFunAs_button.Text = text;
            }
        }

        private ActivityNameEntryBox categoryBox;
        private Specify_AtLeastAsFunAs_Layout specify_AtLeastAsFunAs_Layout;
        private LayoutStack layoutStack;
        private Button atLeastAsFunAs_button;
        private Activity atLeastAsFunAs_activity;
        private ActivityDatabase activityDatabase;
        private Engine engine;
    }

    class Specify_AtLeastAsFunAs_Layout : ContainerLayout
    {
        public Specify_AtLeastAsFunAs_Layout(ActivityDatabase activityDatabase, LayoutStack layoutStack)
        {
            LayoutChoice_Set helpWindow = new HelpWindowBuilder()
                .AddMessage("As part of making an activity request, you may specify that you want the suggested activity to be at least as fun as another activity.")
                .AddMessage("The reason you might do this is if you have an idea of what you might end up doing but you are hoping for ActivityRecommender to provide a better idea.")
                .AddMessage("If you enter an activity name into the box on this screen, then the suggestion that ActivityRecommender makes on the next screen will be " +
                    "one such that ActivityRecommender thinks you will have at least as much fun doing the given suggestion as you would have had doing the activity you " +
                    "specified on this screen.")
                 .AddMessage("You can also specify how much fun you think you would have doing the given activity.")
                 .AddMessage("This doesn't affect the suggestion at all, this is just for your own information.")
                 .AddMessage("The reason you might specify how much fun you think you would have doing the given activity is that if you do end up doing the activity " +
                    "that you were thinking about, then you can reflect on how much fun you actually had and whether it matches how much fun you expected")
                .Build();
            LayoutChoice_Set helpLayout = new HelpButtonLayout(helpWindow, layoutStack);

            this.desiredActivity_box = new ActivityNameEntryBox("I want an activity at least as fun as this one:", activityDatabase, layoutStack);
            this.desiredActivity_box.Placeholder("(Optional)");
            this.desiredActivity_box.PreferSuggestibleActivities = true;

            this.estimatedRating_box = new RelativeRatingEntryView();
            this.estimatedRating_box.SetTitle("which I think will be this much fun:");

            this.SubLayout = new Vertical_GridLayout_Builder().AddLayout(helpLayout).AddLayout(this.desiredActivity_box).AddLayout(this.EstimatedRating_Box).BuildAnyLayout();
        }

        public Activity Activity
        {
            get
            {
                return this.desiredActivity_box.Activity;
            }
        }

        public RelativeRatingEntryView EstimatedRating_Box
        {
            get
            {
                return this.estimatedRating_box;
            }
        }

        public double? Ratio
        {
            get
            {
                if (this.desiredActivity_box.Activity != null)
                {
                    return this.estimatedRating_box.GetRatio();
                }
                return null;
            }
        }

        public Participation LatestParticipation
        {
            set
            {
                this.estimatedRating_box.LatestParticipation = value;
            }
        }

        private ActivityNameEntryBox desiredActivity_box;
        private RelativeRatingEntryView estimatedRating_box;
    }

}
