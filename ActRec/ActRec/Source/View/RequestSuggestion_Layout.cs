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
            this.suggestButton = new Button();
            this.suggestButton.Clicked += SuggestBestActivity_Clicked;
            LayoutChoice_Set suggestButton_layout;

            suggestButton_layout = new ButtonLayout(this.suggestButton);

            this.activityDatabase = activityDatabase;
            this.engine = engine;
            this.layoutStack = layoutStack;


            if (!allowRequestingActivitiesDirectly)
            {
                this.SubLayout = suggestButton_layout;
            }
            else
            {
                this.filterDetails_layout = new RequestSuggestion_Filter_Layout(activityDatabase, layoutStack);
                this.filterButton = new Button();
                this.filterButton.Clicked += FilterButton_Clicked;
                ButtonLayout filterButton_layout = new ButtonLayout(this.filterButton, "Customize");

                if (vertical)
                {
                    GridLayout verticalContentLayout = GridLayout.New(new BoundProperty_List(2), new BoundProperty_List(1), LayoutScore.Get_UnCentered_LayoutScore(1));
                    verticalContentLayout.AddLayout(suggestButton_layout);
                    verticalContentLayout.AddLayout(filterButton_layout);
                    this.SubLayout = verticalContentLayout;
                }
                else
                {
                    GridLayout evenGrid = GridLayout.New(new BoundProperty_List(1), BoundProperty_List.Uniform(2), LayoutScore.Zero);
                    GridLayout unevenGrid = GridLayout.New(new BoundProperty_List(1), new BoundProperty_List(2), LayoutScore.Get_UnCentered_LayoutScore(1));
                    
                    evenGrid.AddLayout(suggestButton_layout);
                    unevenGrid.AddLayout(suggestButton_layout);
                    
                    evenGrid.AddLayout(filterButton_layout);
                    unevenGrid.AddLayout(filterButton_layout);
                    
                    this.SubLayout = new LayoutUnion(evenGrid, unevenGrid);
                }
            }
            this.update_suggestButton_text();
        }

        private void FilterButton_Clicked(object sender, EventArgs e)
        {
            this.layoutStack.AddLayout(this.filterDetails_layout, "Details", this);
        }

        public Participation LatestParticipation
        {
            set
            {
                if (this.filterDetails_layout != null)
                    this.filterDetails_layout.LatestParticipation = value;
            }
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
            ActivityRequest activityRequest = new ActivityRequest(this.category, this.activityToBeat, DateTime.Now, optimizationProperty);
            if (activityToBeat != null)
            {
                DateTime startDate = activityRequest.Date;
                DateTime hypotheticalEndDate = this.engine.GuessParticipationEndDate(activityToBeat, startDate);
                Participation hypotheticalParticipation = new Participation(startDate, hypotheticalEndDate, activityRequest.ActivityToBeat);
                hypotheticalParticipation.Hypothetical = true;

                Rating userPredictedRating = this.filterDetails_layout.EstimatedRating_Box.GetRating(this.engine, hypotheticalParticipation);
                activityRequest.UserPredictedRating = userPredictedRating;
            }
            this.RequestSuggestion.Invoke(activityRequest);
        }

        public void OnBack(LayoutChoice_Set layout)
        {
            this.category = this.filterDetails_layout.Category;
            this.activityToBeat = this.filterDetails_layout.ActivityToBeat;
            this.optimizationProperty = this.filterDetails_layout.OptimizationProperty;
            this.update_suggestButton_text();
        }

        private void update_suggestButton_text()
        {
            string text = "";
            switch(this.optimizationProperty)
            {
                case ActivityRequestOptimizationProperty.LONGTERM_HAPPINESS:
                    text = "Suggest Best";
                    break;
                case ActivityRequestOptimizationProperty.PARTICIPATION_PROBABILITY:
                    text = "Suggest Most Likely";
                    break;
                case ActivityRequestOptimizationProperty.LONGTERM_EFFICIENCY:
                    text = "Suggest Max Efficiency";
                    break;
            }
            if (this.category != null)
            {
                text += " " + this.category.Name;
            }
            if (this.activityToBeat != null)
            {
                text += ", as fun as " + this.activityToBeat.Name;
                double? ratio = this.filterDetails_layout.Ratio;
                if (ratio != null)
                {
                    text += " (" + ratio + "x prev)";
                }
            }
            this.suggestButton.Text = text;
        }

        private LayoutStack layoutStack;
        private ActivityDatabase activityDatabase;
        private Engine engine;
        private RequestSuggestion_Filter_Layout filterDetails_layout;
        private Button filterButton;
        private Button suggestButton;
        private Activity category;
        private Activity activityToBeat;
        private ActivityRequestOptimizationProperty optimizationProperty = ActivityRequestOptimizationProperty.LONGTERM_HAPPINESS;
    }

    class RequestSuggestion_Filter_Layout : ContainerLayout
    {
        public RequestSuggestion_Filter_Layout(ActivityDatabase activityDatabase, LayoutStack layoutStack)
        {
            LayoutChoice_Set title = new TextblockLayout("I want:");

            this.categoryBox = new ActivityNameEntryBox("from this Category:", activityDatabase, layoutStack);
            this.categoryBox.Placeholder("(Optional)");

            this.desiredActivity_box = new ActivityNameEntryBox("and I want it to be at least as fun as this one:", activityDatabase, layoutStack);
            this.desiredActivity_box.Placeholder("(Optional)");
            this.desiredActivity_box.PreferSuggestibleActivities = true;

            this.estimatedRating_box = new RelativeRatingEntryView();
            this.estimatedRating_box.SetTitle("which I think will be this much fun:");

            LayoutChoice_Set helpWindow = new HelpWindowBuilder()
                .AddMessage("This screen allows you to specify more information about the activity you want suggested.")
                .AddMessage("You may specify that the suggested activity must inherit from a certain category.")
                .AddMessage("You may also specify that the suggested activity must be at least as fun as another activity.")
                .AddMessage("The reason you might do this is if you have an idea of what you might end up doing but you are hoping for ActivityRecommender to provide a better idea.")
                .AddMessage("If you enter an activity name into the box on this screen, then the suggestion that ActivityRecommender makes on the next screen will be " +
                    "one such that ActivityRecommender thinks you will have at least as much fun doing the given suggestion as you would have had doing the activity you " +
                    "specified on this screen.")
                 .AddMessage("You can also specify how much fun you think you would have doing the given activity.")
                 .AddMessage("This doesn't affect the suggestion at all, this is just for your own information.")
                 .AddMessage("The reason you might specify how much fun you think you would have doing the given activity is that if you do end up doing the activity " +
                    "that you were thinking about, then you can reflect on how much fun you actually had and whether it matches how much fun you expected")
                .Build();

            this.optimizationProperty_selector = new SingleSelect(new List<string>() { "the activity to maximize future happiness", "the most likely activity", "the activity to maximize future efficiency" });

            this.SubLayout = new Vertical_GridLayout_Builder().Uniform()
                .AddLayout(title)
                .AddLayout(new ButtonLayout(this.optimizationProperty_selector))
                .AddLayout(this.categoryBox)
                .AddLayout(this.desiredActivity_box)
                .AddLayout(this.estimatedRating_box)
                .AddLayout(new HelpButtonLayout(helpWindow, layoutStack))
                .Build();
        }

        public Activity Category
        {
            get
            {
                return this.categoryBox.Activity;
            }
        }
        public Activity ActivityToBeat
        {
            get
            {
                return this.desiredActivity_box.Activity;
            }
        }
        public ActivityRequestOptimizationProperty OptimizationProperty
        {
            get
            {
                switch (this.optimizationProperty_selector.SelectedIndex)
                {
                    case 0:
                        return ActivityRequestOptimizationProperty.LONGTERM_HAPPINESS;
                    case 1:
                        return ActivityRequestOptimizationProperty.PARTICIPATION_PROBABILITY;
                    case 2:
                        return ActivityRequestOptimizationProperty.LONGTERM_EFFICIENCY;
                    default:
                        throw new Exception("Invalid index " + this.optimizationProperty_selector.SelectedIndex + " for optimization property");
                }

            }
        }
        public Participation LatestParticipation
        {
            set
            {
                this.estimatedRating_box.LatestParticipation = value;
            }
        }
        public double? Ratio
        {
            get
            {
                return this.estimatedRating_box.GetRatio();
            }
        }

        public RelativeRatingEntryView EstimatedRating_Box
        {
            get
            {
                return this.estimatedRating_box;
            }
        }

        ActivityNameEntryBox categoryBox;
        ActivityNameEntryBox desiredActivity_box;
        RelativeRatingEntryView estimatedRating_box;
        SingleSelect optimizationProperty_selector;
    }
}
