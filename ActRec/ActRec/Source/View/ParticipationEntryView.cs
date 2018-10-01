using ActivityRecommendation.Effectiveness;
using System;
using VisiPlacement;
using Xamarin.Forms;

// the ParticipationEntryView provides a place for the user to describe what they've done recently
namespace ActivityRecommendation
{
    class ParticipationEntryView : TitledControl
    {
        public ParticipationEntryView(LayoutStack layoutStack) : base("Type What You've Been Doing")
        {
            this.layoutStack = layoutStack;

            BoundProperty_List rowHeights = new BoundProperty_List(6);
            rowHeights.BindIndices(0, 1);
            rowHeights.BindIndices(0, 2);
            rowHeights.BindIndices(0, 3);
            rowHeights.BindIndices(0, 4);
            rowHeights.BindIndices(0, 5);
            rowHeights.SetPropertyScale(0, 2);
            rowHeights.SetPropertyScale(1, 1);
            rowHeights.SetPropertyScale(2, 2);
            rowHeights.SetPropertyScale(3, 1);
            rowHeights.SetPropertyScale(4, 1.5);
            rowHeights.SetPropertyScale(5, 1);

            GridLayout contents = GridLayout.New(rowHeights, BoundProperty_List.Uniform(1), LayoutScore.Zero);

            this.nameBox = new ActivityNameEntryBox("Activity Name");
            this.nameBox.AutoAcceptAutocomplete = false;
            this.nameBox.AddTextChangedHandler(new EventHandler<TextChangedEventArgs>(this.nameBox_TextChanged));
            this.nameBox.NameMatchedSuggestion += new NameMatchedSuggestionHandler(this.ActivityName_BecameValid);
            contents.AddLayout(this.nameBox);

            this.predictedRating_block = new Label();
            contents.AddLayout(new TextblockLayout(this.predictedRating_block));
            
            GridLayout middleGrid = GridLayout.New(BoundProperty_List.Uniform(1), BoundProperty_List.Uniform(2), LayoutScore.Zero);
            this.ratingBox = new RelativeRatingEntryView();
            middleGrid.AddLayout(this.ratingBox);
            Editor commentBox = new Editor();
            this.commentBox = new TitledTextbox("Comment (optional)", commentBox);
            middleGrid.AddLayout(this.commentBox);
            
            contents.AddLayout(middleGrid);
            this.todoCompletionCheckbox = new CheckBox("Incomplete", "Complete!");
            this.todoCompletionCheckboxLayout = ButtonLayout.WithoutBevel(this.todoCompletionCheckbox);
            this.todoCompletionCheckboxHolder = new ContainerLayout();
            this.todoCompletionLabel = new Label();
            contents.AddLayout(new Horizontal_GridLayout_Builder().Uniform()
                .AddLayout(new TextblockLayout(this.todoCompletionLabel))
                .AddLayout(this.todoCompletionCheckboxHolder)
                .Build());

            GridLayout grid3 = GridLayout.New(BoundProperty_List.Uniform(1), BoundProperty_List.Uniform(2), LayoutScore.Zero);

            this.startDateBox = new DateEntryView("Start Time", this.layoutStack);
            this.startDateBox.Add_TextChanged_Handler(new EventHandler<TextChangedEventArgs>(this.DateText_Changed));
            this.startDateBox.Add_TextChanged_Handler(new EventHandler<TextChangedEventArgs>(this.StartDateText_Changed));
            grid3.AddLayout(this.startDateBox);
            this.endDateBox = new DateEntryView("End Time", this.layoutStack);
            this.endDateBox.Add_TextChanged_Handler(new EventHandler<TextChangedEventArgs>(this.DateText_Changed));
            grid3.AddLayout(this.endDateBox);
            contents.AddLayout(grid3);
            this.setStartdateButton = new Button();
            this.setEnddateButton = new Button();

            this.okButton = new Button();

            LayoutChoice_Set helpWindow = (new HelpWindowBuilder()).AddMessage("Use this screen to record participations.")
                .AddMessage("1. Type the name of the activity that you participated in, and press Enter if you want to take the autocomplete suggestion.")
                .AddMessage("You must have entered some activities in the activity name entry screen in order to enter them here.")
                .AddMessage("Notice that once you enter an activity name, ActivityRecommender will tell you how it estimates this will affect your longterm happiness.")
                .AddMessage("2. You may enter a rating (this is strongly encouraged). The rating is a measurement of how much happiness you received per unit time for doing " 
                + "this activity divided by the amount of happiness you received per unit time for doing the previous activity.")
                .AddMessage("If this Activity is a ToDo, you will see a box asking you to specify whether you completed the ToDo. Press the box if you completed it.")
                .AddMessage("3. Enter a start date and an end date. If you use the \"End = Now\" button right when the activity completes, you don't even need to type the date in. If you " +
                "do have to type the date in, press the white box.")
                .AddMessage("4. Enter a comment if you like. For the moment, the comments are just stored but don't do anything. That might change in the future.")
                .AddMessage("5. Lastly, press OK.")
                .AddMessage("It's up to you how many participations you log, how often you rate them, and how accurate the start and end dates are. ActivityRecommender will be able to " +
                "provide more useful help to you if you provide more accurate data, but even just a few participations per day should still be enough for meaningful feedback.")
                .Build();

            GridLayout grid4 = GridLayout.New(BoundProperty_List.Uniform(1), BoundProperty_List.Uniform(4), LayoutScore.Zero);
            grid4.AddLayout(new ButtonLayout(this.setStartdateButton, "Start = now"));
            grid4.AddLayout(new ButtonLayout(this.okButton, "OK"));
            grid4.AddLayout(new HelpButtonLayout(helpWindow, this.layoutStack));
            grid4.AddLayout(new ButtonLayout(this.setEnddateButton, "End = now"));
            contents.AddLayout(grid4);

            this.SetContent(contents);
        }

        public override SpecificLayout GetBestLayout(LayoutQuery query)
        {
            if (!this.feedbackIsUpToDate)
                this.Update_FeedbackBlock_Text();
            return base.GetBestLayout(query);
        }

        public void DateText_Changed(object sender, TextChangedEventArgs e)
        {
            if (this.startDateBox.IsDateValid() && this.endDateBox.IsDateValid())
            {
                bool startValid = true;
                bool endValid = true;
                if (this.StartDate.CompareTo(this.EndDate) > 0)
                {
                    startValid = false;
                    endValid = false;
                }
                else
                {
                    DateTime now = DateTime.Now;
                    if (this.StartDate.CompareTo(now) > 0)
                        startValid = false;
                    if (this.EndDate.CompareTo(now) > 0)
                        endValid = false;

                }
                if (startValid)
                    this.startDateBox.appearValid();
                else
                    this.startDateBox.appearInvalid();
                if (endValid)
                    this.endDateBox.appearValid();
                else
                    this.endDateBox.appearInvalid();
            }
        }

        public void StartDateText_Changed(object sender, TextChangedEventArgs e)
        {
            this.Invalidate_FeedbackBlock_Text();
        }
        void nameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //this.setEnddateButton.Highlight();
        }

        public void Clear()
        {
            this.ratingBox.Clear();
            this.nameBox.Clear();
            this.CommentText = "";
            //this.setEnddateButton.SetDefaultBackground();
            this.predictedRating_block.Text = "";
            this.todoCompletionCheckbox.Checked = false;
            this.updateTodoCheckboxVisibility();
        }
        public ActivityDatabase ActivityDatabase
        {
            set
            {
                this.nameBox.Database = value;
            }
            get
            {
                return this.engine.ActivityDatabase;
            }
        }
        public Engine Engine
        {
            set
            {
                this.engine = value;
            }
        }
        public Participation LatestParticipation
        {
            set
            {
                this.ratingBox.LatestParticipation = value;
            }
        }
        public void SetStartDate(DateTime newDate)
        {
            this.startDateBox.SetDate(newDate);
        }
        public DateTime StartDate
        {
            get
            {
                return this.startDateBox.GetDate();
            }
        }
        public DateTime EndDate
        {
            get
            {
                return this.endDateBox.GetDate();
            }
            set
            {
                this.endDateBox.SetDate(value);
            }
        }
        public void SetActivityName(string newName)
        {
            if (newName != this.ActivityName)
            {
                this.nameBox.Set_NameText(newName);
                this.Invalidate_FeedbackBlock_Text();
                //if (newName != "" && newName != null)
                //    this.setEnddateButton.Highlight();
            }
        }
        public string ActivityName
        {
            get
            {
                Activity activity = this.nameBox.Activity;
                if (activity == null)
                    return null;
                return activity.Name;
            }
        }
        public string RatingText
        {
            set
            {
                //this.ratingBox.Text = value;
            }
        }
        public string CommentText
        {
            get
            {
                return this.commentBox.Text;
            }
            set
            {
                this.commentBox.Text = value;
            }
        }

        public void AddOkClickHandler(EventHandler h)
        {
            this.okButton.Clicked += h;
        }
        public void AddSetenddateHandler(EventHandler h)
        {
            this.setEnddateButton.Clicked += h;
        }
        public void SetEnddateNow(DateTime when)
        {
            DateTime now = DateTime.Now;
            this.endDateBox.SetDate(now);
            //this.setEnddateButton.SetDefaultBackground();
        }
        public void AddSetstartdateHandler(EventHandler h)
        {
            this.setStartdateButton.Clicked += h;
        }
        public Participation GetParticipation(ActivityDatabase activities, Engine engine)
        {
            Activity activity = this.nameBox.Activity;
            if (activity == null)
                return null;
            ActivityDescriptor descriptor = activity.MakeDescriptor();
            if (descriptor.ActivityName == null || descriptor.ActivityName.Length == 0)
                return null;

            Participation participation;
            try
            {
                participation = new Participation(this.StartDate, this.EndDate, descriptor);
            }
            catch (FormatException)
            {
                // if the dates are invalid, then give up
                return null;
            }
            if (this.CommentText != "" && this.CommentText != null)
                participation.Comment = this.CommentText;

            try
            {
                Rating rating = this.ratingBox.GetRating(activities, engine, participation);
                participation.RawRating = rating;
            }
            catch (Exception)
            {
                // If the rating is invalid, then we can ignore that
            }

            if (this.EnteringActivityWithMetric)
            {
                participation.EffectivenessMeasurement = new CompletionEfficiencyMeasurement(this.todoCompletionCheckbox.Checked);
                RelativeEfficiencyMeasurement measurement = engine.Make_CompletionEfficiencyMeasurement(participation);
                participation.EffectivenessMeasurement.Computation = measurement;
            }

            return participation;
        }


        public void ActivityName_BecameValid(object sender, TextChangedEventArgs e)
        {
            this.Invalidate_FeedbackBlock_Text();
            this.updateTodoCheckboxVisibility();
        }

        private void Invalidate_FeedbackBlock_Text()
        {
            this.feedbackIsUpToDate = false;
            this.AnnounceChange(true);
        }

        private void updateTodoCheckboxVisibility()
        {
            if (this.EnteringActivityWithMetric)
            {
                this.todoCompletionCheckboxHolder.SubLayout = this.todoCompletionCheckboxLayout;
                // TODO: if there are multiple metrics; figure out how to determine which one to show
                this.todoCompletionLabel.Text = this.nameBox.Activity.Metrics[0].Name + "?";
            }
            else
            {
                this.todoCompletionCheckboxHolder.SubLayout = null;
                this.todoCompletionLabel.Text = "";
            }
        }

        private bool EnteringActivityWithMetric
        {
            get
            {
                Activity activity = this.nameBox.Activity;
                if (activity != null)
                    return (activity.Metrics.Count > 0);
                return false;
            }
        }

        private void Update_FeedbackBlock_Text()
        {
            if (this.startDateBox.IsDateValid() && this.nameBox.ActivityDescriptor != null)
            {
                DateTime startDate = this.startDateBox.GetDate();
                Activity activity = this.engine.ActivityDatabase.ResolveDescriptor(this.nameBox.ActivityDescriptor);
                if (activity != null)
                {
                    string text = this.computeFeedback(activity, startDate);
                    this.predictedRating_block.Text = text;
                }
            }
            this.feedbackIsUpToDate = true;
        }

        private string computeFeedback(Activity chosenActivity, DateTime startDate)
        {
            double longtermBonusInDays = this.compute_longtermValue_increase(chosenActivity, startDate);
            if (longtermBonusInDays == 0)
            {
                // no data
                return "";
            }
            double shorttermValueRatio = this.compute_estimatedRating_ratio(chosenActivity, startDate);
            double efficiencyBonusInHours = this.computeEfficiencyIncrease(chosenActivity, startDate);

            double roundedLongtermBonus = Math.Round(longtermBonusInDays, 3);
            double roundedShorttermRatio = Math.Round(shorttermValueRatio, 3);
            double roundedEfficiencyBonus = Math.Round(efficiencyBonusInHours, 3);

            string rounded_longtermHappiness_window = "the next " + Math.Round(UserPreferences.DefaultPreferences.HalfLife.TotalDays / Math.Log(2), 0) + " days";
            string roundedEfficiencyWindow = "the next " + Math.Round(UserPreferences.DefaultPreferences.EfficiencyHalflife.TotalDays / Math.Log(2), 0) + " days";

            bool fun = (shorttermValueRatio > 1);
            bool soothing = (longtermBonusInDays >= 0);
            bool efficient = (efficiencyBonusInHours >= 0);

            string message;
            if (fun)
            {
                if (soothing)
                {
                    if (efficient)
                        message = "Nice! I bet that was fun (" + roundedShorttermRatio + " x avg), " +
                            "will improve future happiness (+" + roundedLongtermBonus + " days over " + rounded_longtermHappiness_window + "), " +
                            "and improve future efficiency (+" + roundedEfficiencyBonus + " hours over " + roundedEfficiencyWindow + ")";
                    else
                        message = "Pleasant: I bet that was nice (" + roundedShorttermRatio + " x avg) " +
                            "and will continue to make you happier (+" + roundedLongtermBonus + " days over " + rounded_longtermHappiness_window + "), " +
                            "at the cost of future efficiency (" + roundedEfficiencyBonus + " hours over " + roundedEfficiencyWindow + ")";
                }
                else
                {
                    if (efficient)
                        message = "A good break? I bet that was fun (" + roundedShorttermRatio + " x avg), " +
                            "but will sacrifice future happiness (" + roundedLongtermBonus + " days over " + rounded_longtermHappiness_window + "), " +
                            "for efficiency (+" + roundedEfficiencyBonus + " hours over " + roundedEfficiencyWindow + ")";
                    else
                        message = "Indulgent: I bet that was fun (" + roundedShorttermRatio + " x avg), " +
                            "but will lower future happiness (-" + roundedLongtermBonus + " days over " + rounded_longtermHappiness_window + "), " +
                            "and efficiency (-" + roundedEfficiencyBonus + " hours over " + roundedEfficiencyWindow + ")";
                }
            }
            else
            {
                if (soothing)
                {
                    if (efficient)
                        message = "Awesome work: I bet that was hard (" + roundedShorttermRatio + " x avg), " +
                            "but will make you happier (+" + roundedLongtermBonus + " days over " + rounded_longtermHappiness_window + "), " +
                            "and more efficient (+" + roundedEfficiencyBonus + " hours over " + roundedEfficiencyWindow + ")";
                    else
                        message = "Lazy: I bet that wasn't fun (" + roundedShorttermRatio + " x avg), " +
                            "though you'll appreciate it (+" + roundedLongtermBonus + " days over " + rounded_longtermHappiness_window + "), " +
                            "and lose efficiency (" + roundedEfficiencyBonus + " hours over " + roundedEfficiencyWindow + ")";
                }
                else
                {
                    if (efficient)
                        message = "Power break: I bet that was difficult (" + roundedShorttermRatio + " x avg), " +
                            "and will trade upcoming happiness (" + roundedLongtermBonus + " days over " + rounded_longtermHappiness_window + "), " +
                            "for efficiency (+" + roundedEfficiencyBonus + " hours over " + roundedEfficiencyWindow + ")";
                    else
                        message = "Oops! I bet that wasn't fun (" + roundedShorttermRatio + " x avg) " +
                            "and won't improve your happiness (+" + roundedLongtermBonus + " days over " + rounded_longtermHappiness_window + "), " +
                            "or future efficiency (" + roundedEfficiencyBonus + " hours over " + roundedEfficiencyWindow + ")!";
                }
            }
            return message;
        }

        private double compute_estimatedRating_ratio(Activity chosenActivity, DateTime startDate)
        {
            Activity rootActivity = this.engine.ActivityDatabase.RootActivity;
            this.engine.EstimateSuggestionValue(chosenActivity, startDate);

            double expectedShortermRating = chosenActivity.PredictedScore.Distribution.Mean;
            double overallAverageRating = rootActivity.Ratings.Mean;
            double shorttermRatio = expectedShortermRating / overallAverageRating;

            return shorttermRatio;
        }

        // given an activity and a DateTime for its Participation to start, estimates the change in longterm happiness (measured in days) caused by doing it
        private double compute_longtermValue_increase(Activity chosenActivity, DateTime startDate)
        {
            Distribution chosenEstimatedDistribution = this.engine.Get_OverallHappiness_ParticipationEstimate(chosenActivity, startDate).Distribution;
            if (chosenEstimatedDistribution.Weight <= 0)
                return 0;
            double chosenValue = chosenEstimatedDistribution.Mean;


            Activity rootActivity = this.engine.ActivityDatabase.RootActivity;
            double usualValue = rootActivity.GetAverageLongtermValueWhenParticipated().Mean;

            double bonusInSeconds = 0;
            double weightOfThisMoment = Math.Log(2) / UserPreferences.DefaultPreferences.HalfLife.TotalSeconds;
            if (usualValue != 0)
            {
                // relWeight(x) = 1 * 2^(-x/halfLife)
                // totalWeight = integral(relWeight) = (log(e)/log(2))*halfLife
                // absWeight(x) = 2^(-x/halfLife)/((log(e)/log(2))*halfLife)
                // absWeight(0) = 1/((log(e)/log(2))*halfLife)
                // absWeight(0) = log(2)/log(e)/halflife
                // absWeight(0) = log(2)/halflife

                double overallImprovment = (chosenValue / usualValue) - 1;

                bonusInSeconds = overallImprovment / weightOfThisMoment;
            }
            if (bonusInSeconds == 0)
                return 0;
            double bonusInDays = bonusInSeconds / 60 / 60 / 24;
            return bonusInDays;
        }

        // given an activity and a DateTime for its Participation to start, estimates the change in efficiency (measured in hours) in the near future caused by doing it
        private double computeEfficiencyIncrease(Activity chosenActivity, DateTime startDate)
        {
            Activity rootActivity = this.ActivityDatabase.RootActivity;
            Distribution usual = rootActivity.GetAverageEfficiencyWhenParticipated();
            if (usual.Weight <= 0)
                return 0;
            double usualValue = usual.Mean;

            Distribution chosenEstimatedDistribution = this.engine.Get_Efficiency_ParticipationEstimate(chosenActivity, startDate);
            if (chosenEstimatedDistribution.Weight <= 0)
                return 0;
            double chosenValue = chosenEstimatedDistribution.Mean;

            double bonusInSeconds = 0;
            double weightOfThisMoment = Math.Log(2) / UserPreferences.DefaultPreferences.EfficiencyHalflife.TotalSeconds;
            if (usualValue != 0)
            {
                // relWeight(x) = 1 * 2^(-x/halfLife)
                // totalWeight = integral(relWeight) = (log(e)/log(2))*halfLife
                // absWeight(x) = 2^(-x/halfLife)/((log(e)/log(2))*halfLife)
                // absWeight(0) = 1/((log(e)/log(2))*halfLife)
                // absWeight(0) = log(2)/log(e)/halflife
                // absWeight(0) = log(2)/halflife

                double overallImprovment = (chosenValue / usualValue) - 1;

                bonusInSeconds = overallImprovment / weightOfThisMoment;
            }
            if (bonusInSeconds == 0)
                return 0;
            double bonusInHours = bonusInSeconds / 60 / 60;
            return bonusInHours;
        }


        // private member variables
        ActivityNameEntryBox nameBox;
        RelativeRatingEntryView ratingBox;
        TitledTextbox commentBox;
        DateEntryView startDateBox;
        DateEntryView endDateBox;
        Button setStartdateButton;
        Button setEnddateButton;
        Button okButton;
        Label predictedRating_block;
        Engine engine;
        LayoutStack layoutStack;
        bool feedbackIsUpToDate;
        CheckBox todoCompletionCheckbox;
        Label todoCompletionLabel;
        LayoutChoice_Set todoCompletionCheckboxLayout;
        ContainerLayout todoCompletionCheckboxHolder;
    }
}