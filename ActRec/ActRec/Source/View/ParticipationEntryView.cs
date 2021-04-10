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
    class ParticipationEntryView : ContainerLayout
    {
        public event VisitActivitiesScreenHandler VisitActivitiesScreen;
        public delegate void VisitActivitiesScreenHandler();

        public event VisitActivitiesScreenHandler VisitSuggestionsScreen;
        public delegate void VisitSuggestionsScreenHandler();

        public event VisitStatisticsScreenHandler VisitStatisticsScreen;
        public delegate void VisitStatisticsScreenHandler();

        public ParticipationEntryView(ActivityDatabase activityDatabase, LayoutStack layoutStack)
        {
            this.activityDatabase = activityDatabase;
            this.layoutStack = layoutStack;

            BoundProperty_List rowHeights = new BoundProperty_List(6);
            rowHeights.BindIndices(0, 1);
            rowHeights.BindIndices(0, 2);
            rowHeights.BindIndices(0, 3);
            rowHeights.SetPropertyScale(0, 5); // activity name and feedback
            rowHeights.SetPropertyScale(1, 5); // rating, comments, and metrics
            rowHeights.SetPropertyScale(2, 2.3); // start and end times
            rowHeights.SetPropertyScale(3, 2); // buttons

            // activity name and feedback
            Vertical_GridLayout_Builder nameAndFeedback_builder = new Vertical_GridLayout_Builder();

            GridLayout contents = GridLayout.New(rowHeights, BoundProperty_List.Uniform(1), LayoutScore.Zero);

            this.nameBox = new ActivityNameEntryBox("What Have You Been Doing?", activityDatabase, layoutStack);
            this.nameBox.AutoAcceptAutocomplete = false;
            this.nameBox.PreferSuggestibleActivities = true;
            this.nameBox.NameTextChanged += this.ActivityNameText_Changed;
            nameAndFeedback_builder.AddLayout(this.nameBox);

            this.promptHolder = new ContainerLayout();
            nameAndFeedback_builder.AddLayout(this.promptHolder);

            Button responseButton = new Button();
            responseButton.Clicked += ResponseButton_Clicked;
            this.participationFeedbackButtonLayout = new ButtonLayout(responseButton);
            contents.AddLayout(nameAndFeedback_builder.BuildAnyLayout());

            this.navigationLayout = GridLayout.New(new BoundProperty_List(1), BoundProperty_List.Uniform(2), LayoutScore.Zero);
            Button suggestionsButton = new Button();
            suggestionsButton.Clicked += SuggestionsButton_Clicked;
            this.navigationLayout.AddLayout(new ButtonLayout(suggestionsButton, "What's next?"));
            Button analyzeButton = new Button();
            analyzeButton.Clicked += AnalyzeButton_Clicked;
            this.navigationLayout.AddLayout(new ButtonLayout(analyzeButton, "Analyze!"));

            Button experimentFeedbackButton = new Button();
            experimentFeedbackButton.Clicked += ExperimentFeedbackButton_Clicked;
            this.experimentFeedbackLayout = new ButtonLayout(experimentFeedbackButton, "Experiment Complete!");

            Vertical_GridLayout_Builder detailsBuilder = new Vertical_GridLayout_Builder();

            GridLayout commentAndRating_grid = GridLayout.New(BoundProperty_List.Uniform(1), BoundProperty_List.Uniform(2), LayoutScore.Zero);
            this.ratingBox = new RelativeRatingEntryView();
            commentAndRating_grid.AddLayout(this.ratingBox);
            this.commentBox = new PopoutTextbox("Comment", layoutStack);
            this.commentBox.Placeholder("(Optional)");
            commentAndRating_grid.AddLayout(this.commentBox);

            detailsBuilder.AddLayout(commentAndRating_grid);
            this.todoCompletionStatusHolder = new ContainerLayout();
            this.metricChooser = new ChooseMetric_View(true);
            this.metricChooser.ChoseNewMetric += TodoCompletionLabel_ChoseNewMetric;

            LayoutChoice_Set metricLayout = new Vertical_GridLayout_Builder().Uniform()
                    .AddLayout(this.metricChooser)
                    .AddLayout(this.todoCompletionStatusHolder)
                    .Build();
            this.helpStatusHolder = new ContainerLayout();
            GridLayout_Builder centered_todoInfo_builder = new Horizontal_GridLayout_Builder().Uniform();
            centered_todoInfo_builder.AddLayout(metricLayout);
            centered_todoInfo_builder.AddLayout(this.helpStatusHolder);
            GridLayout_Builder offset_todoInfo_builder = new Horizontal_GridLayout_Builder();
            offset_todoInfo_builder.AddLayout(metricLayout);
            offset_todoInfo_builder.AddLayout(this.helpStatusHolder);

            LayoutChoice_Set metricStatusLayout = new LayoutUnion(
                    centered_todoInfo_builder.Build(),
                    new ScoreShifted_Layout(
                        offset_todoInfo_builder.Build(),
                        LayoutScore.Get_UnCentered_LayoutScore(1)
                    )
                );
            this.helpStatusPicker = new HelpDurationInput_Layout(this.layoutStack);
            detailsBuilder.AddLayout(metricStatusLayout);
            contents.AddLayout(detailsBuilder.BuildAnyLayout());

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
                    .AddContribution(ActRecContributor.ANNI_ZHANG, new DateTime(2021, 3, 9), "Suggested making different metrics appear more distinct.")
                    .AddContribution(ActRecContributor.ANNI_ZHANG, new DateTime(2021, 3, 21), "Pointed out that the participation feedback had stopped finding a better activity")
                    .Build()
                )
                .Build();

            GridLayout grid4 = GridLayout.New(BoundProperty_List.Uniform(1), BoundProperty_List.Uniform(4), LayoutScore.Zero);
            grid4.AddLayout(new ButtonLayout(this.setStartdateButton, "Start = now", 16));
            grid4.AddLayout(new ButtonLayout(this.okButton, "OK"));
            grid4.AddLayout(new HelpButtonLayout(helpWindow, this.layoutStack));
            grid4.AddLayout(new ButtonLayout(this.setEnddateButton, "End = now", 16));
            contents.AddLayout(grid4);

            this.mainLayout = LayoutCache.For(contents);

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

        private void ExperimentFeedbackButton_Clicked(object sender, EventArgs e)
        {
            this.showExperimentFeedback();
        }

        private void showExperimentFeedback()
        {
            this.layoutStack.AddLayout(new ExperimentResultsView(this.LatestParticipation), "Experiment Results");
        }

        private void AnalyzeButton_Clicked(object sender, EventArgs e)
        {
            if (this.VisitStatisticsScreen != null)
                this.VisitStatisticsScreen.Invoke();
        }

        private void SuggestionsButton_Clicked(object sender, EventArgs e)
        {
            // The response button suggested requesting another suggestion
            if (this.VisitSuggestionsScreen != null)
                this.VisitSuggestionsScreen.Invoke();
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

        private void ResponseButton_Clicked(object sender, EventArgs e)
        {
            // The response button had feedback about the most recent participation
            this.layoutStack.AddLayout(this.participationFeedback.GetDetails(this.layoutStack), "Feedback");
        }

        public override SpecificLayout GetBestLayout(LayoutQuery query)
        {
            if (this.hasActivities)
                this.SubLayout = this.mainLayout;
            else
                this.SubLayout = this.noActivities_explanationLayout;

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

        public void Clear()
        {
            this.ratingBox.Clear();
            this.nameBox.Clear();
            this.CommentText = "";
            this.updateMetricSelectorVisibility();
            this.Update_FeedbackBlock_Text();
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
                bool successful = (status == this.TaskCompleted_Text || status == this.ProblemComplete_Text);
                bool closed = (status != this.TaskIncomplete_Text && status != this.ProblemIncomplete_Text);
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

        public void ActivityNameText_Changed()
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
                SingleSelect singleSelect;
                if (!(metric is ProblemMetric))
                {
                    SingleSelect_Choice complete = new SingleSelect_Choice(this.TaskCompleted_Text, Color.Green);
                    SingleSelect_Choice incomplete = new SingleSelect_Choice(this.TaskIncomplete_Text, Color.Yellow);
                    SingleSelect_Choice obsolete = new SingleSelect_Choice(this.TaskObsolete_Text, Color.White);
                    if (this.EnteringToDo)
                        singleSelect = new SingleSelect(new List<SingleSelect_Choice>() { complete, incomplete, obsolete });
                    else
                        singleSelect = new SingleSelect(new List<SingleSelect_Choice>() { complete, incomplete });
                }
                else
                {
                    SingleSelect_Choice complete = new SingleSelect_Choice(this.ProblemComplete_Text, Color.LightBlue);
                    SingleSelect_Choice incomplete = new SingleSelect_Choice(this.ProblemIncomplete_Text, Color.Orange);
                    singleSelect = new SingleSelect(new List<SingleSelect_Choice>() { complete, incomplete });
                }
                this.todoCompletionStatusPicker = singleSelect;

                this.helpStatusHolder.SubLayout = this.helpStatusPicker;
                this.todoCompletionStatusHolder.SubLayout = ButtonLayout.WithoutBevel(this.todoCompletionStatusPicker);
            }
            else
            {
                this.helpStatusHolder.SubLayout = null;
                this.todoCompletionStatusPicker = null;
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
            ParticipationFeedback participationFeedback = null;
            Activity activity = this.nameBox.Activity;
            if (activity != null && this.startDateBox.IsDateValid() && this.endDateBox.IsDateValid())
            {
                // We have a valid activity and valid dates, so give feedback about the participation
                DateTime startDate = this.startDateBox.GetDate();
                DateTime endDate = this.endDateBox.GetDate();
                participationFeedback = this.computeFeedback(activity, startDate, endDate);
                if (participationFeedback != null)
                {
                    this.participationFeedbackButtonLayout.setText(participationFeedback.Summary);
                    bool? happySummary = participationFeedback.happySummary;
                    if (happySummary != null)
                    {
                        if (happySummary.Value == true)
                        {
                            this.participationFeedbackButtonLayout.setTextColor(Color.Green);
                        }
                        else
                        {
                            this.participationFeedbackButtonLayout.setTextColor(Color.Red);
                        }
                    }
                    else
                    {
                        this.participationFeedbackButtonLayout.resetTextColor();
                    }
                    this.promptHolder.SubLayout = this.participationFeedbackButtonLayout;
                }
                else
                {
                    this.promptHolder.SubLayout = null;
                }
            }
            else
            {
                if (this.nameBox.NameText == null || this.nameBox.NameText == "")
                {
                    // If we have no text in the activity name box, then we do have space for a response and there isn't a current activity to give feedback on
                    Participation latestParticipation = this.LatestParticipation;
                    if (latestParticipation != null && latestParticipation.RelativeEfficiencyMeasurement != null)
                    {
                        this.promptHolder.SubLayout = this.experimentFeedbackLayout;
                    }
                    else
                    {
                        // We can remind the user to get another suggestion,
                        // and we can give them a convenient button for going there
                        this.promptHolder.SubLayout = this.navigationLayout;
                    }
                }
                else
                {
                    // If we have no valid activity name but we do have some text in the name box, then we don't need to say anything
                    // The name entry box will offer autocomplete suggestions and/or help
                    this.promptHolder.SubLayout = null;
                }
            }
            // Note that this is the only method that modifies either participationFeedback or responseButtonLayout.text,
            // so we can ensure that they match
            this.participationFeedback = participationFeedback;
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
                return new ParticipationFeedback(chosenActivity, summary, false, new TextblockLayout(details));
            }
            ParticipationFeedback standardFeedback = this.engine.computeStandardParticipationFeedback(chosenActivity, startDate, endDate);
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

                return new ParticipationFeedback(chosenActivity, summary, null, detailedLayout);
            }
            return null;
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
        private string ProblemComplete_Text
        {
            get
            {
                return "Fixed!";
            }
        }
        private string ProblemIncomplete_Text
        {
            get
            {
                return "Not fixed";
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
        ButtonLayout participationFeedbackButtonLayout;
        ContainerLayout promptHolder;
        GridLayout navigationLayout;
        LayoutChoice_Set experimentFeedbackLayout;
        string feedback;
        Engine engine;
        ChooseMetric_View metricChooser;
        LayoutStack layoutStack;
        bool feedbackIsUpToDate;
        SingleSelect todoCompletionStatusPicker;
        ContainerLayout todoCompletionStatusHolder;
        HelpDurationInput_Layout helpStatusPicker;
        ContainerLayout helpStatusHolder;
        ActivityDescriptor demanded_nextParticipationActivity;
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