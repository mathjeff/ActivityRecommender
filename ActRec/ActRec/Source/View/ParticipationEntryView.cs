using ActivityRecommendation.Effectiveness;
using ActivityRecommendation.TextSummary;
using ActivityRecommendation.View;
using System;
using System.Collections.Generic;
using VisiPlacement;
using Xamarin.Forms;

// the ParticipationEntryView provides a place for the user to describe what they've done recently
namespace ActivityRecommendation
{
    class ParticipationEntryView : TitledControl
    {
        public ParticipationEntryView(ActivityDatabase activityDatabase, LayoutStack layoutStack) : base("Type What You've Been Doing")
        {
            this.activityDatabase = activityDatabase;
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

            this.nameBox = new ActivityNameEntryBox("Activity Name", activityDatabase, layoutStack);
            this.nameBox.AutoAcceptAutocomplete = false;
            this.nameBox.PreferSuggestibleActivities = true;
            this.nameBox.AddTextChangedHandler(new EventHandler<TextChangedEventArgs>(this.nameBox_TextChanged));
            this.nameBox.NameMatchedSuggestion += new NameMatchedSuggestionHandler(this.ActivityName_BecameValid);
            contents.AddLayout(this.nameBox);

            this.feedbackButton = new Button();
            this.feedbackButton.Clicked += FeedbackButton_Clicked;
            contents.AddLayout(ButtonLayout.HideIfEmpty(new ButtonLayout(this.feedbackButton)));
            
            GridLayout middleGrid = GridLayout.New(BoundProperty_List.Uniform(1), BoundProperty_List.Uniform(2), LayoutScore.Zero);
            this.ratingBox = new RelativeRatingEntryView();
            middleGrid.AddLayout(this.ratingBox);
            this.commentBox = new PopoutTextbox("Comment (optional)", layoutStack);
            middleGrid.AddLayout(this.commentBox);

            contents.AddLayout(middleGrid);
            this.todoCompletionStatusHolder = new ContainerLayout();
            this.todoCompletionLabel = new Label();
            contents.AddLayout(new Horizontal_GridLayout_Builder().Uniform()
                .AddLayout(new TextblockLayout(this.todoCompletionLabel))
                .AddLayout(this.todoCompletionStatusHolder)
                .Build());

            GridLayout grid3 = GridLayout.New(BoundProperty_List.Uniform(1), BoundProperty_List.Uniform(2), LayoutScore.Zero);

            this.startDateBox = new DateEntryView("Start Time", this.layoutStack);
            this.startDateBox.Add_TextChanged_Handler(new EventHandler<TextChangedEventArgs>(this.DateText_Changed));
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
                .AddMessage("4. Enter a comment if you like.")
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

            this.mainLayout = contents;

            this.noActivities_explanationLayout = new TextblockLayout(
                "This screen is where you will be able to record having participated in an activity.\n" +
                "Before you can record a participation in an activity, ActivityRecommender needs to know what activities are relevant to you.\n" +
                "You should go back and create at least one activity first (press the button that says \"Activities\" and proceed from there)."
                );
        }

        private void FeedbackButton_Clicked(object sender, EventArgs e)
        {
            string detailsText = this.participationFeedback.Details;
            this.layoutStack.AddLayout(new TextblockLayout(detailsText));
        }

        public override SpecificLayout GetBestLayout(LayoutQuery query)
        {
            if (this.hasActivities)
                this.SetContent(this.mainLayout);
            else
                this.SetContent(this.noActivities_explanationLayout);

            if (!this.feedbackIsUpToDate)
                this.Update_FeedbackBlock_Text();
            return base.GetBestLayout(query);
        }

        private bool hasActivities
        {
            get
            {
                return this.activityDatabase.ContainsCustomActivity();
            }
        }

        public void DateText_Changed(object sender, TextChangedEventArgs e)
        {
            this.Invalidate_FeedbackBlock_Text();
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
                    this.startDateBox.appear_defaultValid();
                else
                    this.startDateBox.appearInvalid();
                if (endValid)
                {
                    if (this.EndDate.Subtract(this.StartDate).CompareTo(TimeSpan.FromDays(1)) >= 0)
                        this.endDateBox.appearConcerned();
                    else
                        this.endDateBox.appear_defaultValid();
                }
                else
                {
                    this.endDateBox.appearInvalid();
                }
            }
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
            this.feedbackButton.Text = "";
            this.updateTodoCheckboxVisibility();
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
                this.previousPrediction = this.currentPrediction;
                this.currentPrediction = new LongtermPrediction();
            }
            get
            {
                return this.ratingBox.LatestParticipation;
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

            if (!this.startDateBox.IsDateValid())
                return null;
            if (!this.endDateBox.IsDateValid())
                return null;
            if (this.EndDate.CompareTo(this.StartDate) <= 0)
            {
                // If the user is trying to submit a participation with misordered dates, then point the out to them
                // Although we could've highlighted this to the user sooner, in most cases this would just distract them and they wouldn't want to think about it
                this.startDateBox.appearInvalid();
                this.endDateBox.appearInvalid();
                return null;
            }
            Participation participation = new Participation(this.StartDate, this.EndDate, descriptor);
            if (this.CommentText != "" && this.CommentText != null)
                participation.Comment = this.CommentText;

            try
            {
                Rating rating = this.ratingBox.GetRating(engine, participation);
                participation.RawRating = rating;
            }
            catch (Exception)
            {
                // If the rating is invalid, then we can ignore that
            }

            if (this.EnteringActivityWithMetric)
            {
                string status = this.todoCompletionStatusPicker.SelectedItem.ToString();
                bool successful = (status == this.TaskCompleted_Text);
                bool closed = (status != TaskIncomplete_Text);
                participation.EffectivenessMeasurement = new CompletionEfficiencyMeasurement(successful);
                participation.EffectivenessMeasurement.DismissedActivity = closed;
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

        public void DemandNextParticipationBe(ActivityDescriptor activityDescriptor)
        {
            this.demanded_nextParticipationActivity = activityDescriptor;
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
                SingleSelect singleSelect;
                Picker picker = new Picker();
                if (this.EnteringToDo)
                {
                    singleSelect = new SingleSelect(new List<String>() { this.TaskIncomplete_Text, this.TaskCompleted_Text, this.TaskObsolete_Text });
                }
                else
                {
                    singleSelect = new SingleSelect(new List<String>() { this.TaskIncomplete_Text, this.TaskCompleted_Text });
                }
                this.todoCompletionStatusPicker = singleSelect;
                this.todoCompletionStatusHolder.SubLayout = ButtonLayout.WithoutBevel(this.todoCompletionStatusPicker);
                // TODO: if there are multiple metrics; figure out how to determine which one to show
                this.todoCompletionLabel.Text = this.nameBox.Activity.Metrics[0].Name + "?";
            }
            else
            {
                this.todoCompletionStatusHolder.SubLayout = null;
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

        private bool EnteringToDo
        {
            get
            {
                Activity activity = this.nameBox.Activity;
                if (activity != null)
                    return (activity is ToDo);
                return false;
            }
        }

        private void Update_FeedbackBlock_Text()
        {
            if (this.startDateBox.IsDateValid() && this.endDateBox.IsDateValid() && this.nameBox.ActivityDescriptor != null)
            {
                DateTime startDate = this.startDateBox.GetDate();
                DateTime endDate = this.endDateBox.GetDate();
                Activity activity = this.engine.ActivityDatabase.ResolveDescriptor(this.nameBox.ActivityDescriptor);
                if (activity != null)
                {
                    ParticipationFeedback participationFeedback = this.computeFeedback(activity, startDate, endDate);
                    if (participationFeedback != null)
                    {
                        this.feedbackButton.Text = participationFeedback.Summary;
                        this.participationFeedback = participationFeedback;
                    }
                    else
                    {
                        this.feedbackButton.Text = "";
                    }
                }
            }
            this.feedbackIsUpToDate = true;
        }

        private ParticipationFeedback computeFeedback(Activity chosenActivity, DateTime startDate, DateTime endDate)
        {
            if (this.demanded_nextParticipationActivity != null && !this.demanded_nextParticipationActivity.Matches(chosenActivity))
            {
                string summary = "THE IRE OF THE EXPERIMENT GODS RAINS ON YOU AND YOUR BROKEN PROMISES";
                string details = "You previously initiated an experiment where you promised that you would be willing to do " +
                    this.demanded_nextParticipationActivity.ActivityName + ". Instead you did " + chosenActivity.Name + ". If you " +
                    "don't follow through on your promises, then your data might be skewed in strange ways. For example, it's possible that " +
                    "in the evening that you may choose to skip doing difficult tasks and save them for the morning. This could cause you to " +
                    "take more time working on any individual task in the morning than in the evening, which could incorrectly suggest that " +
                    "your efficiency is lower in the morning than in the evening.";
                return new ParticipationFeedback(chosenActivity, summary, details);
            }
            else
            {
                return this.computeStandardFeedback(chosenActivity, startDate, endDate);
            }
        }
        private ParticipationFeedback computeStandardFeedback(Activity chosenActivity, DateTime startDate, DateTime endDate)
        {
            Distribution longtermBonusInDays = this.compute_longtermValue_increase(chosenActivity, startDate);
            if (longtermBonusInDays.Mean == 0)
            {
                // no data
                return null;
            }
            Distribution shorttermValueRatio = this.compute_estimatedRating_ratio(chosenActivity, startDate);
            Distribution efficiencyBonusInHours = this.computeEfficiencyIncrease(chosenActivity, startDate);

            double roundedLongtermBonus = Math.Round(longtermBonusInDays.Mean, 3);
            double roundedLongtermStddev = Math.Round(longtermBonusInDays.StdDev, 3);

            double roundedShorttermRatio = Math.Round(shorttermValueRatio.Mean, 3);
            double roundedShortTermStddev = Math.Round(shorttermValueRatio.StdDev, 3);

            double roundedEfficiencyBonus = Math.Round(efficiencyBonusInHours.Mean, 3);
            double roundedEfficiencyStddev = Math.Round(efficiencyBonusInHours.StdDev, 3);

            // compute how long the user spent doing this and how long they usually spend doing it
            // TODO: do we want to change this calculation to use Math.Exp(LogActiveTime) like Engine.GuessParticipationEndDate does?
            double typicalNumSeconds = chosenActivity.MeanParticipationDuration;
            double actualNumSeconds = endDate.Subtract(startDate).TotalSeconds;
            double durationRatio;
            if (typicalNumSeconds != 0)
                durationRatio = Math.Round(actualNumSeconds / typicalNumSeconds, 3);
            else
                durationRatio = 0;

            bool fast = (actualNumSeconds <= typicalNumSeconds);
            bool fun = (shorttermValueRatio.Mean > 1);
            bool soothing = (longtermBonusInDays.Mean >= 0);
            bool efficient = (efficiencyBonusInHours.Mean >= 0);

            string remark;

            if (fast)
            {
                if (fun)
                {
                    if (soothing)
                    {
                        if (efficient)
                            remark = "Great!";
                        else
                            remark = "A brief respite.";
                    }
                    else
                    {
                        if (efficient)
                            remark = "You can do it!";
                        else
                            remark = "Thanks for stopping early.";
                    }
                }
                else
                {
                    if (soothing)
                    {
                        if (efficient)
                            remark = "So fast!";
                        else
                            remark = "Find happiness?";
                    }
                    else
                    {
                        if (efficient)
                            remark = "Power break.";
                        else
                            remark = "Oops!";
                    }
                }
            }
            else
            {
                if (fun)
                {
                    if (soothing)
                    {
                        if (efficient)
                            remark = "Phenomenal!";
                        else
                            remark = "Pleasant.";
                    }
                    else
                    {
                        if (efficient)
                            remark = "A good break?";
                        else
                            remark = "That's pretty indulgent.";
                    }
                }
                else
                {
                    if (soothing)
                    {
                        if (efficient)
                            remark = "Good work.";
                        else
                            remark = "Lazy.";
                    }
                    else
                    {
                        if (efficient)
                            remark = "You'll get some rest eventually.";
                        else
                            remark = "Oh dear. Don't do that.";
                    }
                }

            }

            string detailsMessage = chosenActivity.Name + "\n";
            detailsMessage += TimeFormatter.summarizeTimespan(startDate, endDate) + "\n";
            detailsMessage += "You spent " + durationRatio + " as long as average.\n\n";
            detailsMessage  += "I predict: \n\n";
            detailsMessage += roundedShorttermRatio + " * avg fun while doing it (+/- " + roundedShortTermStddev + ")\n\n";
            if (roundedLongtermBonus > 0)
                detailsMessage += "+";
            detailsMessage += roundedLongtermBonus + " days fun (+/- " + roundedLongtermStddev + ") over next " + Math.Round(UserPreferences.DefaultPreferences.HalfLife.TotalDays / Math.Log(2), 0) + " days\n\n";
            if (roundedEfficiencyBonus > 0)
                detailsMessage += "+";
            detailsMessage += roundedEfficiencyBonus + " hours effectiveness (+/- " + roundedEfficiencyStddev + ") over next " + Math.Round(UserPreferences.DefaultPreferences.EfficiencyHalflife.TotalDays / Math.Log(2), 0) + " days";

            return new ParticipationFeedback(chosenActivity, remark, detailsMessage);
        }

        private Distribution compute_estimatedRating_ratio(Activity chosenActivity, DateTime startDate)
        {
            Activity rootActivity = this.engine.ActivityDatabase.RootActivity;
            this.engine.EstimateSuggestionValue(chosenActivity, startDate);
            Prediction prediction = this.engine.EstimateRating(chosenActivity, startDate);

            Distribution expectedShortermRating = prediction.Distribution;
            double overallAverageRating = rootActivity.Ratings.Mean;
            Distribution shorttermRatio = expectedShortermRating.CopyAndStretchBy(1.0 / overallAverageRating);

            return shorttermRatio;
        }

        // given an activity and a DateTime for its Participation to start, estimates the change in longterm happiness (measured in days) caused by doing it
        private Distribution compute_longtermValue_increase(Activity chosenActivity, DateTime startDate)
        {
            Distribution chosenEstimatedDistribution = this.engine.Get_OverallHappiness_ParticipationEstimate(chosenActivity, startDate).Distribution;
            this.currentPrediction.LongtermHappiness = chosenEstimatedDistribution;
            if (chosenEstimatedDistribution.Weight <= 0)
                return new Distribution();
            Distribution previousValue = this.previousPrediction.LongtermHappiness.CopyAndReweightTo(1);
            Distribution chosenValue = chosenEstimatedDistribution.CopyAndReweightTo(1);

            Distribution bonusInDays = new Distribution();
            // relWeight(x) = 2^(-x/halflife)
            // integral(relWeight) = -(log(e)/log(2))*halfLife*2^(-x/halflife)
            // totalWeight = (log(e)/log(2))*halflife
            // absWeight(x) = relWeight(x) / totalWeight
            // absWeight(x) = 2^(-x/halflife) / ((log(e)/log(2))*halflife)
            // absWeight(0) = log(2)/log(e)/halflife = log(2)/halflife
            double weightOfThisMoment = Math.Log(2) / UserPreferences.DefaultPreferences.HalfLife.TotalDays;
            if (previousValue.Mean > 0)
            {
                Distribution combined = previousValue.Plus(chosenValue);
                double overallAverage = combined.Mean;

                double overallImprovement = (chosenValue.Mean - previousValue.Mean) / overallAverage;
                double overallVariance = (chosenValue.Variance + previousValue.Variance) / (overallAverage * overallAverage);
                Distribution difference = Distribution.MakeDistribution(overallImprovement, Math.Sqrt(overallVariance), 1);

                bonusInDays = difference.CopyAndStretchBy(1.0 / weightOfThisMoment);
            }
            return bonusInDays;
        }

        // given an activity and a DateTime for its Participation to start, estimates the change in efficiency (measured in hours) in the near future caused by doing it
        private Distribution computeEfficiencyIncrease(Activity chosenActivity, DateTime startDate)
        {
            Distribution chosenEstimatedDistribution = this.engine.Get_OverallEfficiency_ParticipationEstimate(chosenActivity, startDate).Distribution;
            this.currentPrediction.LongtermEfficiency = chosenEstimatedDistribution;
            if (chosenEstimatedDistribution.Weight <= 0)
                return new Distribution();
            Distribution previousValue = this.previousPrediction.LongtermEfficiency.CopyAndReweightTo(1);
            Distribution chosenValue = chosenEstimatedDistribution.CopyAndReweightTo(1);

            Distribution bonusInHours = new Distribution();
            // relWeight(x) = 2^(-x/halflife)
            // integral(relWeight) = -(log(e)/log(2))*halfLife*2^(-x/halflife)
            // totalWeight = (log(e)/log(2))*halflife
            // absWeight(x) = relWeight(x) / totalWeight
            // absWeight(x) = 2^(-x/halflife) / ((log(e)/log(2))*halflife)
            // absWeight(0) = log(2)/log(e)/halflife = log(2)/halflife
            double weightOfThisMoment = Math.Log(2) / UserPreferences.DefaultPreferences.EfficiencyHalflife.TotalHours;
            if (previousValue.Mean > 0)
            {
                Distribution combined = previousValue.Plus(chosenValue);
                double overallAverage = combined.Mean;

                double overallImprovement = (chosenValue.Mean - previousValue.Mean) / overallAverage;
                double overallVariance = (chosenValue.Variance + previousValue.Variance) / (overallAverage * overallAverage);
                Distribution difference = Distribution.MakeDistribution(overallImprovement, Math.Sqrt(overallVariance), 1);

                bonusInHours = difference.CopyAndStretchBy(1.0 / weightOfThisMoment);
            }
            return bonusInHours;
        }


        private string TaskCompleted_Text
        {
            get
            {
                return "Complete!";
            }
        }
        private string TaskIncomplete_Text
        {
            get
            {
                return "Incomplete";
            }
        }
        private string TaskObsolete_Text
        {
            get
            {
                return "Obsolete";
            }
        }


        // private member variables
        LayoutChoice_Set mainLayout;
        LayoutChoice_Set noActivities_explanationLayout;
        ActivityDatabase activityDatabase;
        ActivityNameEntryBox nameBox;
        RelativeRatingEntryView ratingBox;
        PopoutTextbox commentBox;
        DateEntryView startDateBox;
        DateEntryView endDateBox;
        Button setStartdateButton;
        Button setEnddateButton;
        Button okButton;
        Button feedbackButton;
        Engine engine;
        LayoutStack layoutStack;
        bool feedbackIsUpToDate;
        Label todoCompletionLabel;
        SingleSelect todoCompletionStatusPicker;
        ContainerLayout todoCompletionStatusHolder;
        ActivityDescriptor demanded_nextParticipationActivity;
        LongtermPrediction previousPrediction = new LongtermPrediction();
        LongtermPrediction currentPrediction = new LongtermPrediction();
        ParticipationFeedback participationFeedback;
    }

    class LongtermPrediction
    {
        public LongtermPrediction()
        {
            this.LongtermHappiness = Distribution.MakeDistribution(0.5, 0, 1);
            this.LongtermEfficiency = Distribution.MakeDistribution(1, 0, 1);
        }
        public Distribution LongtermHappiness;
        public Distribution LongtermEfficiency;
    }
}