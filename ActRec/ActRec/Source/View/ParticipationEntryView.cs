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
        public event VisitActivitiesScreenHandler VisitActivitiesScreen;
        public delegate void VisitActivitiesScreenHandler();

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
            rowHeights.SetPropertyScale(2, 1.5);
            rowHeights.SetPropertyScale(3, 1);
            rowHeights.SetPropertyScale(4, 1.15);
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
            this.commentBox = new PopoutTextbox("Comment", layoutStack);
            this.commentBox.Placeholder("(Optional)");
            middleGrid.AddLayout(this.commentBox);

            contents.AddLayout(middleGrid);
            this.todoCompletionStatusHolder = new ContainerLayout();
            this.metricChooser = new ChooseMetric_View(true);
            this.metricChooser.ChoseNewMetric += TodoCompletionLabel_ChoseNewMetric;

            Horizontal_GridLayout_Builder todoInfo_builder = new Horizontal_GridLayout_Builder().Uniform();
            todoInfo_builder.AddLayout(new Vertical_GridLayout_Builder().Uniform()
                .AddLayout(this.metricChooser)
                .AddLayout(this.todoCompletionStatusHolder)
                .Build());
            this.helpStatusHolder = new ContainerLayout();
            todoInfo_builder.AddLayout(this.helpStatusHolder);

            contents.AddLayout(todoInfo_builder.Build());

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
                .AddMessage("2. You may enter a rating (this is strongly recommended). The rating is a measurement of how much happiness you received per unit time from "
                + "this participation divided by the amount of happiness you received per unit time for the previous. "
                + "(The ratio that you enter will be combined with ActivityRecommender's previous expectations of how much you would enjoy these two "
                + "participations, and will be used to create an appropriate absolute rating from 0 to 1 for this participation.)")
                .AddMessage("If this Activity is a ToDo, you will see a box asking you to specify whether you completed the ToDo. Press the box if you completed it.")
                .AddMessage("3. Enter a start date and an end date. If you use the \"End = Now\" button right when the activity completes, you don't even need to type the date in. If you " +
                "do have to type the date in, press the white box.")
                .AddMessage("4. Enter a comment if you like.")
                .AddMessage("5. Lastly, press OK.")
                .AddMessage("It's up to you how many participations you log, how often you rate them, and how accurate the start and end dates are. ActivityRecommender will be able to " +
                "provide more useful help to you if you provide more accurate data, but even just a few participations per day should still be enough for meaningful feedback.")
                .AddLayout(new CreditsButtonBuilder(layoutStack)
                    .AddContribution(ActRecContributor.AARON_SMITH, new DateTime(2019, 8, 17), "Pointed out out that it was hard to tell when the participation and suggestion screens are not yet relevant due to not having any activities")
                    .AddContribution(ActRecContributor.ANNI_ZHANG, new DateTime(2019, 11, 10), "Suggested disallowing entering participations having empty durations")
                    .AddContribution(ActRecContributor.ANNI_ZHANG, new DateTime(2019, 11, 28), "Mentioned that the keyboard was often in the way of text boxes on iOS")
                    .AddContribution(ActRecContributor.ANNI_ZHANG, new DateTime(2020, 1, 26), "Pointed out that feedback should be relative to average rather happiness than relative to the previous participation")
                    .AddContribution(ActRecContributor.ANNI_ZHANG, new DateTime(2020, 4, 19), "Discussed participation feedback messages")
                    .AddContribution(ActRecContributor.ANNI_ZHANG, new DateTime(2020, 7, 12), "Pointed out that the time required to log a participation can cause the end time of the next participation to be a couple minutes after the previous one")
                    .AddContribution(ActRecContributor.ANNI_ZHANG, new DateTime(2020, 8, 15), "Suggested that if the participation feedback recommends a different time, then it should specify which time")
                    .AddContribution(ActRecContributor.ANNI_ZHANG, new DateTime(2020, 8, 30), "Pointed out that participation feedback was missing more often than it should have been.")
                    .AddContribution(ActRecContributor.ANNI_ZHANG, new DateTime(2020, 8, 30), "Pointed out that the text in the starttime box had stopped fitting properly.")
                    .AddContribution(ActRecContributor.ANNI_ZHANG, new DateTime(2020, 10, 3), "Pointed out that it was possible record participations in the future.")
                    .Build()
                )
                .Build();

            GridLayout grid4 = GridLayout.New(BoundProperty_List.Uniform(1), BoundProperty_List.Uniform(4), LayoutScore.Zero);
            grid4.AddLayout(new ButtonLayout(this.setStartdateButton, "Start = now", 16));
            grid4.AddLayout(new ButtonLayout(this.okButton, "OK"));
            grid4.AddLayout(new HelpButtonLayout(helpWindow, this.layoutStack));
            grid4.AddLayout(new ButtonLayout(this.setEnddateButton, "End = now", 16));
            contents.AddLayout(grid4);

            this.mainLayout = contents;

            Vertical_GridLayout_Builder noActivities_help_builder = new Vertical_GridLayout_Builder();
            noActivities_help_builder.AddLayout(new TextblockLayout("This screen is where you will be able to record having participated in an activity.\n"));
            noActivities_help_builder.AddLayout(
                new HelpButtonLayout("Recording a participation is deceptively easy",
                    new HelpWindowBuilder()
                        .AddMessage("Autocomplete is everywhere in ActivityRecommender and is very fast. You will be impressed.")
                        .AddMessage("Autocomplete is one of the reasons that you must enter an Activity before you can record a participation, so " +
                        "ActivityRecommender can know which activity you're referring to, usually after you type only one or two letters.")
                        .Build()
                ,
                    layoutStack
                )
            );
            noActivities_help_builder.AddLayout(
                new HelpButtonLayout("You get feedback!",
                    new HelpWindowBuilder()
                        .AddMessage("Nearly every time you record a participation, ActivityRecommender will give you feedback on what you're doing. " +
                        "This feedback will eventually contain suggestions of other things you could be doing now, and alternate times for what you " +
                        "did do. This feedback gets increasingly specific and increasingly accurate as you record more data, eventually including " +
                        "current happiness, future happiness, and future efficiency. Wow!")
                        .Build()
                ,
                    layoutStack
                )
            );

            noActivities_help_builder.AddLayout(new TextblockLayout("Before you can record a participation, ActivityRecommender needs you to go back " +
                "and add some activities first. Here is a convenient button for jumping directly to the Activities screen:"));

            Button activitiesButton = new Button();
            activitiesButton.Text = "Activities";
            activitiesButton.Clicked += ActivitiesButton_Clicked;
            noActivities_help_builder.AddLayout(new ButtonLayout(activitiesButton));

            this.noActivities_explanationLayout = noActivities_help_builder.BuildAnyLayout();
        }

        public List<AppFeature> GetFeatures()
        {
            return new List<AppFeature>()
            {
                new UserLoggedAParticipation_Feature(this.activityDatabase),
                new UserEnteredAComment_Feature(this.activityDatabase),
                new UserEnteredARating_Feature(this.activityDatabase),
                new RecordExperimentParticipation_Feature(this.engine)
            };
        }
        private void ActivitiesButton_Clicked(object sender, EventArgs e)
        {
            if (this.VisitActivitiesScreen != null)
                this.VisitActivitiesScreen.Invoke();
        }

        private void FeedbackButton_Clicked(object sender, EventArgs e)
        {
            this.layoutStack.AddLayout(this.participationFeedback.Details, "Feedback");
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
            this.updateMetricSelectorVisibility();
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
        public IEnumerable<ActivitySuggestion> CurrentSuggestions = new List<ActivitySuggestion>();
        public DateTime StartDate
        {
            get
            {
                return this.startDateBox.GetDate();
            }
            set
            {
                this.startDateBox.SetDate(value);
            }
        }
        public bool Is_EndDate_Valid
        {
            get
            {
                return this.endDateBox.IsDateValid();
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
            this.endDateBox.SetDate(when);
            //this.setEnddateButton.SetDefaultBackground();
        }
        public void AddSetstartdateHandler(EventHandler h)
        {
            this.setStartdateButton.Clicked += h;
        }
        private Metric Metric
        {
            get
            {
                return this.metricChooser.Metric;
            }
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
            // If the user is trying to submit a participation with misordered dates, then point the out to them
            // Although we could've highlighted this to the user sooner, in most cases this would just distract them and they wouldn't want to think about it
            if (this.EndDate.CompareTo(this.StartDate) <= 0)
            {
                this.startDateBox.appearInvalid();
                this.endDateBox.appearInvalid();
                return null;
            }
            // If the user tries to record having done someting in the future, that's a mistake because it hasn't happened yet
            DateTime now = DateTime.Now;
            if (this.StartDate.CompareTo(now) > 0 || this.EndDate.CompareTo(now) > 0)
            {
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

            Metric metric = this.Metric;
            if (metric != null)
            {
                // Record the success/failure status of the participation
                string status = this.todoCompletionStatusPicker.SelectedItem.ToString();
                bool successful = (status == this.TaskCompleted_Text);
                bool closed = (status != this.TaskIncomplete_Text);
                double helpFraction = this.helpStatusPicker.GetHelpFraction(participation.Duration);
                participation.EffectivenessMeasurement = new CompletionEfficiencyMeasurement(metric, successful, helpFraction);
                participation.EffectivenessMeasurement.DismissedActivity = closed;
                // Notice that the Engine doesn't need us to pass in a Metric for computing the efficiency of this participation, because computing efficiency requires an experiment, and the
                // experiment already knows which metric we're using
                RelativeEfficiencyMeasurement measurement = engine.Make_CompletionEfficiencyMeasurement(participation);
                participation.EffectivenessMeasurement.Computation = measurement;
            }

            participation.Suggested = this.get_wasSuggested(participation.ActivityDescriptor);

            return participation;
        }

        private bool get_wasSuggested(ActivityDescriptor activity)
        {
            foreach (ActivitySuggestion suggestion in this.CurrentSuggestions)
            {
                if (activity.CanMatch(suggestion.ActivityDescriptor))
                    return true;
            }
            return false;

        }

        public void ActivityName_BecameValid(object sender, TextChangedEventArgs e)
        {
            this.Invalidate_FeedbackBlock_Text();
            this.updateMetricSelectorVisibility();
        }

        public void DemandNextParticipationBe(ActivityDescriptor activityDescriptor, Metric metric)
        {
            this.demanded_nextParticipationActivity = activityDescriptor;
            this.metricChooser.DemandMetric(metric);
            // If there was not previously a demanded participation and metric, then the metric chooser may have allowed the user to enter no metric
            // Now that we're demanding a specific metric, it's possible that a metric has appeared in the metric chooser, and we might need to
            // suddenly ask the user if they completed this metric
            this.updateMetricSelectorVisibility();
        }

        private void Invalidate_FeedbackBlock_Text()
        {
            this.feedbackIsUpToDate = false;
            this.AnnounceChange(true);
        }

        private void updateMetricSelectorVisibility()
        {
            this.metricChooser.SetActivity(this.nameBox.Activity);

            if (this.EnteringActivityWithMetric)
            {
                SingleSelect singleSelect;
                SingleSelect_Choice incomplete = new SingleSelect_Choice(this.TaskIncomplete_Text, Color.Yellow);
                SingleSelect_Choice complete = new SingleSelect_Choice(this.TaskCompleted_Text, Color.Green);
                SingleSelect_Choice obsolete = new SingleSelect_Choice(this.TaskObsolete_Text, Color.White);
                if (this.EnteringToDo)
                    singleSelect = new SingleSelect(new List<SingleSelect_Choice>() { incomplete, complete, obsolete });
                else
                    singleSelect = new SingleSelect(new List<SingleSelect_Choice>() { incomplete, complete });
                this.todoCompletionStatusPicker = singleSelect;
                this.helpStatusPicker = new HelpDurationInput_Layout(this.layoutStack);
            }
            this.updateCompletionStatusVisibility();
        }

        private void TodoCompletionLabel_ChoseNewMetric(ChooseMetric_View view)
        {
            this.updateCompletionStatusVisibility();
        }
        private void updateCompletionStatusVisibility()
        {
            Metric metric = this.Metric;
            if (metric != null)
            {
                this.helpStatusHolder.SubLayout = this.helpStatusPicker;
                if (this.todoCompletionStatusHolder.SubLayout == null)
                    this.todoCompletionStatusHolder.SubLayout = ButtonLayout.WithoutBevel(this.todoCompletionStatusPicker);
            }
            else
            {
                this.helpStatusHolder.SubLayout = null;
                this.todoCompletionStatusHolder.SubLayout = null;
            }
        }


        private bool EnteringActivityWithMetric
        {
            get
            {
                Activity activity = this.nameBox.Activity;
                if (activity != null)
                    return activity.HasAMetric;
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
                return new ParticipationFeedback(chosenActivity, summary, new ConstantValueProvider<LayoutChoice_Set>(new TextblockLayout(details)));
            }
            ParticipationFeedback standardFeedback = this.computeStandardFeedback(chosenActivity, startDate, endDate);
            if (standardFeedback != null)
                return standardFeedback;
            if (this.activityDatabase.RootActivity.NumParticipations < 20)
            {
                string summary = "You're off to a good start!";

                LayoutChoice_Set detailedLayout = new HelpWindowBuilder()
                    .AddMessage("Participation Feedback!")
                    .AddMessage("After you've entered enough data, the button on the previous screen will give you feedback about how you will feel about what you are doing.")
                    .AddMessage("(This may take a couple of days and requires that you give some ratings too, in the box on the left)")
                    .AddMessage("I have 128 different feedback messages that I know how to give!")
                    .AddMessage("When you press the feedback button, you will see a more detailed analysis on this screen.")
                    .AddMessage("Isn't that cool?")
                    .Build();

                return new ParticipationFeedback(chosenActivity, summary, new ConstantValueProvider<LayoutChoice_Set>(detailedLayout));
            }
            return null;
        }
        private ParticipationFeedback computeStandardFeedback(Activity chosenActivity, DateTime startDate, DateTime endDate)
        {
            DateTime comparisonDate = this.engine.chooseRandomBelievableParticipationStart(chosenActivity, startDate);
            if (comparisonDate.CompareTo(startDate) == 0)
            {
                // not enough data
                return null;
            }

            Distribution comparisonBonusInDays = this.compute_longtermValue_increase(chosenActivity, comparisonDate);
            if (comparisonBonusInDays.Mean == 0)
            {
                // not enough data
                return null;
            }
            Distribution comparisonEfficiencyBonusInHours = this.computeEfficiencyIncrease(chosenActivity, comparisonDate);
            Distribution comparisonValueRatio = this.compute_estimatedRating_ratio(chosenActivity, comparisonDate);

            Distribution longtermBonusInDays = this.compute_longtermValue_increase(chosenActivity, startDate);
            if (longtermBonusInDays.Mean == 0)
            {
                // no data
                return null;
            }
            Distribution efficiencyBonusInHours = this.computeEfficiencyIncrease(chosenActivity, startDate);
            Distribution shorttermValueRatio = this.compute_estimatedRating_ratio(chosenActivity, startDate);

            double roundedShorttermRatio = Math.Round(shorttermValueRatio.Mean, 3);
            double roundedShortTermStddev = Math.Round(shorttermValueRatio.StdDev, 3);
            double roundedComparisonBonus = Math.Round(comparisonValueRatio.Mean, 3);

            double roundedLongtermBonus = Math.Round(longtermBonusInDays.Mean, 3);
            double roundedLongtermStddev = Math.Round(longtermBonusInDays.StdDev, 3);
            double roundedComparisonLongtermBonus = Math.Round(comparisonBonusInDays.Mean, 3);

            double roundedEfficiencyBonus = Math.Round(efficiencyBonusInHours.Mean, 3);
            double roundedEfficiencyStddev = Math.Round(efficiencyBonusInHours.StdDev, 3);
            double roudnedComparisonEfficiencyLongtermBonus = Math.Round(comparisonEfficiencyBonusInHours.Mean, 3);

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
            bool funActivity = (shorttermValueRatio.Mean >= 1);
            bool funTime = (shorttermValueRatio.Mean >= comparisonValueRatio.Mean);
            bool soothingActivity = (longtermBonusInDays.Mean >= 0);
            bool soothingTime = (longtermBonusInDays.Mean > comparisonBonusInDays.Mean);
            bool efficientActivity = (efficiencyBonusInHours.Mean >= 0);
            bool efficientTime = (efficiencyBonusInHours.Mean >= comparisonEfficiencyBonusInHours.Mean);
            bool suggested = this.get_wasSuggested(chosenActivity.MakeDescriptor());

            string remark;

            if (funActivity)
            {
                if (funTime)
                {
                    if (soothingActivity)
                    {
                        if (soothingTime)
                        {
                            if (efficientActivity)
                            {
                                if (efficientTime)
                                {
                                    if (fast)
                                        remark = "Incredible!";
                                    else
                                        remark = "Spectacular!!!";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Fireworks!";
                                    else
                                        remark = "Solid work!";
                                }
                            }
                            else // !efficientActivity
                            {
                                if (efficientTime)
                                {
                                    if (fast)
                                        remark = "A good time for a party!";
                                    else
                                        remark = "So much fun! Don't forget to work too :p";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "A short, fun distraction";
                                    else
                                        remark = "Great! Don't forget to take a good break, though";
                                }
                            }
                        }
                        else // !soothingTime
                        {
                            if (efficientActivity)
                            {
                                if (efficientTime)
                                {
                                    if (fast)
                                        remark = "Good work!";
                                    else
                                        remark = "Great work!";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Good job! But could you choose a different time?";
                                    else
                                        remark = "Great job! But could you choose a different time?";
                                }
                            }
                            else // !efficientActivity
                            {
                                if (efficientTime)
                                {
                                    // This activity is soothing but this time is not
                                    // This time is efficient but this activity is not
                                    if (fast)
                                        remark = "What a crazy schedule";
                                    else
                                        remark = "Crazy schedule?";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Maybe choose another time with fewer interruptions?";
                                    else
                                        remark = "Any chance that you could choose a different time?";
                                }
                            }
                        }
                    }
                    else // !soothingActivity
                    {
                        if (soothingTime)
                        {
                            if (efficientActivity)
                            {
                                if (efficientTime)
                                {
                                    if (fast)
                                        remark = "Good job!";
                                    else
                                        remark = "Strong work!";
                                }
                                else // !efficientTime
                                {
                                    // a soothing time but not a soothing activity
                                    // an efficient activity but not an efficient time
                                    if (fast)
                                        remark = "A fun time for a little work";
                                    else
                                        remark = "A fun time for some work";
                                }
                            }
                            else // !efficientActivity
                            {
                                if (efficientTime)
                                {
                                    // it's not a soothing activity but it is a soothing time
                                    // it's not an efficient activity but it is an efficient time
                                    if (fast)
                                        remark = "A good idea, but something else might be more wholesome";
                                    else
                                        remark = "Not a bad time, but something else might be more wholesome";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Good job on stopping early";
                                    else
                                        remark = "That's a bit indulgent :p";
                                }
                            }
                        }
                        else // !soothingTime
                        {
                            if (efficientActivity)
                            {
                                if (efficientTime)
                                {
                                    if (fast)
                                        remark = "So fast!";
                                    else
                                        remark = "What a hard worker!";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Good work! You might burn out less if you choose another time though";
                                    else
                                        remark = "Good job! You might burn out less if you choose another time though";
                                }
                            }
                            else // !efficientActivity
                            {
                                if (efficientTime)
                                {
                                    // this activity isn't soothing and the time wasn't soothing
                                    // this activity isn't efficient but the time is efficient
                                    if (fast)
                                        remark = "Try doing something more wholesome next?";
                                    else
                                        remark = "Have you tried looking for a more wholesome activity?";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Good call on stopping early";
                                    else
                                        remark = "Are you sure you're not burning out?";
                                }
                            }
                        }
                    }
                }
                else // !funTime
                {
                    if (soothingActivity)
                    {
                        if (soothingTime)
                        {
                            if (efficientActivity)
                            {
                                if (efficientTime)
                                {
                                    if (fast)
                                        remark = "Nice!";
                                    else
                                        remark = "Well done!";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Pretty good! I think you'd be better off rescheduling though";
                                    else
                                        remark = "Not bad! I think you'd be better off rescheduling though";
                                }
                            }
                            else // !efficientActivity
                            {
                                if (efficientTime)
                                {
                                    // Fun activity but not a fun time
                                    // Soothing activity and also a soothing time
                                    // Not an efficient activity, but an efficient time
                                    if (fast)
                                        remark = "A turbocharged break!";
                                    else
                                        remark = "Not bad for a break";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "A short party!";
                                    else
                                        remark = "Nice party :p";
                                }
                            }
                        }
                        else // !soothingTime
                        {
                            if (efficientActivity)
                            {
                                if (efficientTime)
                                {
                                    // fun but not a fun time
                                    // soothing but not a soothing time
                                    // efficient and an efficient time
                                    if (fast)
                                        remark = "Solid work! Hope you this doesn't drain on you";
                                    else
                                        remark = "Solid work! Hope you this doesn't make you tired";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Good work, but you should choose a different time";
                                    else
                                        remark = "Good work, but please choose a different time";
                                }
                            }
                            else // !efficientActivity
                            {
                                if (efficientTime)
                                {
                                    // fun but not a fun time
                                    // soothing but not a soothing time
                                    // not an efficient activity, but an efficient time
                                    if (fast)
                                        remark = "Maybe do something else?";
                                    else
                                        remark = "Could you do something else?";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "I recommend rescheduling";
                                    else
                                        remark = "I strongly recommend rescheduling";
                                }
                            }
                        }
                    }
                    else // !soothingActivity
                    {
                        if (soothingTime)
                        {
                            if (efficientActivity)
                            {
                                if (efficientTime)
                                {
                                    // fun but not a fun time
                                    // not soothing, but a soothing time
                                    // efficent and an efficient time
                                    if (fast)
                                        remark = "Nice! But this might tire you out";
                                    else
                                        remark = "Nice! But watch out for whether this tires you out";
                                }
                                else // !efficientTime
                                {
                                    // fun but not a fun time
                                    // not soothing, but a soothing time
                                    // efficent but not efficient time
                                    if (fast)
                                        remark = "Isn't there a better time for this? :p";
                                    else
                                        remark = "Can you find a better time for this?";
                                }
                            }
                            else // !efficientActivity
                            {
                                if (efficientTime)
                                {
                                    // it's not a soothing activity but it is a soothing time
                                    // it's not an efficient activity but it is an efficient time
                                    if (fast)
                                        remark = "A decent idea. Is there something better you could do, though?";
                                    else
                                        remark = "This is a decent time, but is there something better you could be doing?";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Is there anything better you could do?";
                                    else
                                        remark = "There must be something better you could be doing";
                                }
                            }
                        }
                        else // !soothingTime
                        {
                            if (efficientActivity)
                            {
                                if (efficientTime)
                                {
                                    if (fast)
                                        remark = "Working hard!";
                                    else
                                        remark = "Really hard work! Are you doing ok?";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Are you sure that now is the right time?";
                                    else
                                        remark = "Are you sure it's worth doing this now?";
                                }
                            }
                            else // !efficientActivity
                            {
                                if (efficientTime)
                                {
                                    // this activity isn't soothing and the time wasn't soothing
                                    // this activity isn't efficient but the time is efficient
                                    if (fast)
                                        remark = "Really?";
                                    else
                                        remark = "Why?";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Thank goodness that you stopped";
                                    else
                                        remark = "That must be the biggest indulgence ever";
                                }
                            }
                        }
                    }
                }
            }
            else // !funActivity
            {
                if (funTime)
                {
                    if (soothingActivity)
                    {
                        if (soothingTime)
                        {
                            if (efficientActivity)
                            {
                                if (efficientTime)
                                {
                                    if (fast)
                                        remark = "Wow!";
                                    else
                                        remark = "Awesome!!!";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Impressive!";
                                    else
                                        remark = "Impressive!!!";
                                }
                            }
                            else // !efficientActivity
                            {
                                if (efficientTime)
                                {
                                    if (fast)
                                        remark = "If you had to do this, at least you chose a good time";
                                    else
                                        remark = "If this was necessary, at least you chose a good time";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "A short distraction";
                                    else
                                        remark = "You're on your way to happiness!";
                                }
                            }
                        }
                        else // !soothingTime
                        {
                            if (efficientActivity)
                            {
                                if (efficientTime)
                                {
                                    if (fast)
                                        remark = "You can do it!";
                                    else
                                        remark = "Keep it up!";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Good job! But I recommend another time if possible";
                                    else
                                        remark = "Great job! But I recommend another time if possible";
                                }
                            }
                            else // !efficientActivity
                            {
                                if (efficientTime)
                                {
                                    // This activity is soothing but this time is not
                                    // This time is efficient but this activity is not
                                    if (fast)
                                        remark = "A completely insane schedule";
                                    else
                                        remark = "Insane schedule?";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Maybe choose another time to avoid being interrupted?";
                                    else
                                        remark = "I think another time would be less chaotic";
                                }
                            }
                        }
                    }
                    else // !soothingActivity
                    {
                        if (soothingTime)
                        {
                            if (efficientActivity)
                            {
                                if (efficientTime)
                                {
                                    if (fast)
                                        remark = "Efficiency!";
                                    else
                                        remark = "Impressive! Do you think this is sustainable?";
                                }
                                else // !efficientTime
                                {
                                    // a soothing time but not a soothing activity
                                    // an efficient activity but not an efficient time
                                    if (fast)
                                        remark = "Not bad";
                                    else
                                        remark = "Not bad, but you might prefer something else";
                                }
                            }
                            else // !efficientActivity
                            {
                                if (efficientTime)
                                {
                                    // it's not a soothing activity but it is a soothing time
                                    // it's not an efficient activity but it is an efficient time
                                    if (fast)
                                        remark = "A good idea, but something else might be more enjoyable";
                                    else
                                        remark = "Not a bad time, but something else might be more enjoyable";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Probably don't want to do that for too long";
                                    else
                                        remark = "How'd you decide on that?";
                                }
                            }
                        }
                        else // !soothingTime
                        {
                            if (efficientActivity)
                            {
                                if (efficientTime)
                                {
                                    if (fast)
                                        remark = "Way to go!";
                                    else
                                        remark = "Such hard work!";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Good work! Something else might more fulfilling though";
                                    else
                                        remark = "Good job! Something else might more fulfilling though";
                                }
                            }
                            else // !efficientActivity
                            {
                                if (efficientTime)
                                {
                                    // this activity isn't soothing and the time wasn't soothing
                                    // this activity isn't efficient but the time is efficient
                                    if (fast)
                                        remark = "Glad that that's over";
                                    else
                                        remark = "Do you have to do that?";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Feel free not to do that";
                                    else
                                        remark = "Oops";
                                }
                            }
                        }
                    }
                }
                else // !funTime
                {
                    if (soothingActivity)
                    {
                        if (soothingTime)
                        {
                            if (efficientActivity)
                            {
                                if (efficientTime)
                                {
                                    if (fast)
                                        remark = "All right!";
                                    else
                                        remark = "I believe in you!";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Pretty good! But have you considered any other times?";
                                    else
                                        remark = "Not bad! But have you considered any other times?";
                                }
                            }
                            else // !efficientActivity
                            {
                                if (efficientTime)
                                {
                                    // not a fun activity, not a fun time
                                    // Soothing activity and also a soothing time
                                    // Not an efficient activity, but an efficient time
                                    if (fast)
                                        remark = "I think you'll mostly appreciate this later, at least";
                                    else
                                        remark = "I see some relaxation in your future";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Converting some future efficiency into happiness";
                                    else
                                        remark = "I think this will make you happier and less efficient";
                                }
                            }
                        }
                        else // !soothingTime
                        {
                            if (efficientActivity)
                            {
                                if (efficientTime)
                                {
                                    // not fun, not a fun time
                                    // soothing but not a soothing time
                                    // efficient and an efficient time
                                    if (fast)
                                        remark = "Solid work! Yeah, it's ok to take breaks if you need to";
                                    else
                                        remark = "Solid work! Remember to take breaks if you need them";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Good work, but you can choose a different time";
                                    else
                                        remark = "Good work, but you'd be better off choosing a different time";
                                }
                            }
                            else // !efficientActivity
                            {
                                if (efficientTime)
                                {
                                    // not fun, not a fun time
                                    // soothing but not a soothing time
                                    // not an efficient activity, but an efficient time
                                    if (fast)
                                        remark = "Not bad, but I don't think you had to do this now";
                                    else
                                        remark = "Not bad, but I don't think you have to do this now";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "I think you would prefer another time";
                                    else
                                        remark = "I definitely recommend rescheduling";
                                }
                            }
                        }
                    }
                    else // !soothingActivity
                    {
                        if (soothingTime)
                        {
                            if (efficientActivity)
                            {
                                if (efficientTime)
                                {
                                    // not fun, not a fun time
                                    // not soothing, but a soothing time
                                    // efficent and an efficient time
                                    if (fast)
                                        remark = "Great progress! Is this a project you care about?";
                                    else
                                        remark = "Solid progress. Is this a project you care about?";
                                }
                                else // !efficientTime
                                {
                                    // not fun, not a fun time
                                    // not soothing, but a soothing time
                                    // efficent but not efficient time
                                    if (fast)
                                        remark = "Hmm. Are you sure?";
                                    else
                                        remark = "Is this something you care about and is this the best time for it?";
                                }
                            }
                            else // !efficientActivity
                            {
                                if (efficientTime)
                                {
                                    // it's not a soothing activity but it is a soothing time
                                    // it's not an efficient activity but it is an efficient time
                                    if (fast)
                                        remark = "I don't think you have to do that, but that was a good time for it";
                                    else
                                        remark = "I don't think you have to do that, but this is a good time for it";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Come on!";
                                    else
                                        remark = "Seriously?";
                                }
                            }
                        }
                        else // !soothingTime
                        {
                            if (efficientActivity)
                            {
                                if (efficientTime)
                                {
                                    if (fast)
                                        remark = "Hard work!";
                                    else
                                        remark = "Very hard work! Are you ok?";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Maybe write this down and do it later?";
                                    else
                                        remark = "Can this wait?";
                                }
                            }
                            else // !efficientActivity
                            {
                                if (efficientTime)
                                {
                                    // this activity isn't soothing and the time wasn't soothing
                                    // this activity isn't efficient but the time is efficient
                                    if (fast)
                                        remark = "Yeah, don't do that";
                                    else
                                        remark = "What are you sacrificing for a little efficiency?";
                                }
                                else // !efficientTime
                                {
                                    if (fast)
                                        remark = "Oops!";
                                    else
                                        remark = "Oh no!";
                                }
                            }
                        }
                    }
                }
            }

            ParticipationNumericFeedback detailsProvider = new ParticipationNumericFeedback();
            detailsProvider.engine = this.engine;
            detailsProvider.ActivityDatabase = this.activityDatabase;
            detailsProvider.StartDate = startDate;
            detailsProvider.EndDate = endDate;
            detailsProvider.ComparisonDate = comparisonDate;
            detailsProvider.ParticipationDurationDividedByAverage = durationRatio;
            detailsProvider.ChosenActivity = chosenActivity;

            detailsProvider.ExpectedEfficiency = roundedEfficiencyBonus;
            detailsProvider.ComparisonExpectedEfficiency = roudnedComparisonEfficiencyLongtermBonus;
            detailsProvider.ExpectedEfficiencyStddev = roundedEfficiencyStddev;

            detailsProvider.ExpectedFutureFun = roundedLongtermBonus;
            detailsProvider.ComparisonExpectedFutureFun = roundedComparisonLongtermBonus;
            detailsProvider.ExpectedFutureFunStddev = roundedLongtermStddev;

            detailsProvider.PredictedValue = roundedShorttermRatio;
            detailsProvider.PredictedCurrentValueStddev = roundedShortTermStddev;
            detailsProvider.ComparisonPredictedValue = roundedComparisonBonus;

            detailsProvider.Suggested = suggested;

            return new ParticipationFeedback(chosenActivity, remark, detailsProvider);
        }

        private Distribution compute_estimatedRating_ratio(Activity chosenActivity, DateTime startDate)
        {
            Activity rootActivity = this.engine.ActivityDatabase.RootActivity;
            this.engine.EstimateSuggestionValue(chosenActivity, startDate);
            Prediction prediction = this.engine.EstimateRating(chosenActivity, startDate);
            return this.compute_estimatedRating_ratio(prediction.Distribution, rootActivity.Ratings);
        }
        private Distribution compute_estimatedRating_ratio(Activity chosenActivity)
        {
            Activity rootActivity = this.engine.ActivityDatabase.RootActivity;
            return this.compute_estimatedRating_ratio(chosenActivity.Ratings, rootActivity.Ratings);
        }
        private Distribution compute_estimatedRating_ratio(Distribution value, Distribution rootValue)
        {
            Distribution expectedShortermRating = value;
            double overallAverageRating = rootValue.Mean;
            Distribution shorttermRatio = expectedShortermRating.CopyAndStretchBy(1.0 / overallAverageRating);

            return shorttermRatio;
        }

        // given an activity and a DateTime for its Participation to start, estimates the change in longterm happiness (measured in days) caused by doing it
        private Distribution compute_longtermValue_increase(Activity chosenActivity, DateTime startDate)
        {
            Distribution endValue = this.engine.Get_OverallHappiness_ParticipationEstimate(chosenActivity, startDate).Distribution;
            this.currentPrediction.LongtermHappiness = endValue;
            return this.compute_longtermValue_increase(endValue);
        }
        private Distribution compute_longtermValue_increase(Activity chosenActivity)
        {
            Distribution endValue = this.engine.GetAverageLongtermValueWhenParticipated(chosenActivity);
            return this.compute_longtermValue_increase(endValue);
        }
        private Distribution compute_longtermValue_increase(Distribution endValue)
        {
            return this.engine.compute_longtermValue_increase_in_days(endValue);
        }

        // given an activity and a DateTime for its Participation to start, estimates the change in efficiency (measured in hours) in the near future caused by doing it
        private Distribution computeEfficiencyIncrease(Activity chosenActivity, DateTime startDate)
        {
            Distribution chosenEstimatedDistribution = this.engine.Get_OverallEfficiency_ParticipationEstimate(chosenActivity, startDate).Distribution;
            this.currentPrediction.LongtermEfficiency = chosenEstimatedDistribution;
            return this.computeEfficiencyIncrease(chosenEstimatedDistribution);
        }
        private Distribution computeEfficiencyIncrease(Activity chosenActivity)
        {
            Distribution endValue = this.engine.Get_AverageEfficiency_WhenParticipated(chosenActivity);
            return this.computeEfficiencyIncrease(endValue);
        }
        private Distribution computeEfficiencyIncrease(Distribution endValue)
        {
            if (endValue.Weight <= 0)
                return Distribution.Zero;
            Distribution baseValue = this.engine.Get_AverageEfficiency_WhenParticipated(this.activityDatabase.RootActivity);
            Distribution chosenValue = endValue.CopyAndReweightTo(1);

            Distribution bonusInHours = Distribution.Zero;
            // relWeight(x) = 2^(-x/halflife)
            // integral(relWeight) = -(log(e)/log(2))*halfLife*2^(-x/halflife)
            // totalWeight = (log(e)/log(2))*halflife
            // absWeight(x) = relWeight(x) / totalWeight
            // absWeight(x) = 2^(-x/halflife) / ((log(e)/log(2))*halflife)
            // absWeight(0) = log(2)/log(e)/halflife = log(2)/halflife
            double weightOfThisMoment = Math.Log(2) / UserPreferences.DefaultPreferences.EfficiencyHalflife.TotalHours;
            if (baseValue.Mean > 0)
            {
                Distribution combined = baseValue.Plus(chosenValue);
                double overallAverage = combined.Mean;

                double relativeImprovement = (chosenValue.Mean - baseValue.Mean) / overallAverage;
                double relativeVariance = chosenValue.Variance / (overallAverage * overallAverage);
                Distribution difference = Distribution.MakeDistribution(relativeImprovement, Math.Sqrt(relativeVariance), 1);

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
        ChooseMetric_View metricChooser;
        LayoutStack layoutStack;
        bool feedbackIsUpToDate;
        SingleSelect todoCompletionStatusPicker;
        ContainerLayout todoCompletionStatusHolder;
        HelpDurationInput_Layout helpStatusPicker;
        ContainerLayout helpStatusHolder;
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

    class UserLoggedAParticipation_Feature : AppFeature
    {
        public UserLoggedAParticipation_Feature(ActivityDatabase activityDatabase)
        {
            this.activityDatabase = activityDatabase;
        }
        public string GetDescription()
        {
            return "Record a participation";
        }
        public bool GetHasBeenUsed()
        {
            return this.activityDatabase.RootActivity.NumParticipations > 0;
        }
        public bool GetIsUsable()
        {
            return this.activityDatabase.ContainsCustomActivity();
        }
        private ActivityDatabase activityDatabase;
    }

    class UserEnteredARating_Feature : AppFeature
    {
        public UserEnteredARating_Feature(ActivityDatabase activityDatabase)
        {
            this.activityDatabase = activityDatabase;
        }
        public string GetDescription()
        {
            return "Enter a rating";
        }
        public bool GetHasBeenUsed()
        {
            foreach (Participation participation in this.activityDatabase.RootActivity.Participations)
            {
                if (participation.GetAbsoluteRating() != null)
                    return true;
            }
            return false;
        }
        public bool GetIsUsable()
        {
            return this.activityDatabase.ContainsCustomActivity();
        }
        private ActivityDatabase activityDatabase;
    }

    class UserEnteredAComment_Feature : AppFeature
    {
        public UserEnteredAComment_Feature(ActivityDatabase activityDatabase)
        {
            this.activityDatabase = activityDatabase;
        }
        public string GetDescription()
        {
            return "Enter a comment";
        }
        public bool GetHasBeenUsed()
        {
            foreach (Participation participation in this.activityDatabase.RootActivity.Participations)
            {
                if (participation.Comment != null)
                    return true;
            }
            return false;
        }
        public bool GetIsUsable()
        {
            return this.activityDatabase.ContainsCustomActivity();
        }
        private ActivityDatabase activityDatabase;
    }

    class RecordExperimentParticipation_Feature : AppFeature
    {
        public RecordExperimentParticipation_Feature(Engine engine)
        {
            this.engine = engine;
        }
        public string GetDescription()
        {
            return "Complete an experiment";
        }
        public bool GetHasBeenUsed()
        {
            return this.engine.NumCompletedExperiments > 0;
        }
        public bool GetIsUsable()
        {
            return this.engine.HasInitiatedExperiment;
        }
        private Engine engine;

    }
}