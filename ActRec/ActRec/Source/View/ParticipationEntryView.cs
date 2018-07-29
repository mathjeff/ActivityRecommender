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

            BoundProperty_List rowHeights = new BoundProperty_List(5);
            rowHeights.BindIndices(0, 1);
            rowHeights.BindIndices(0, 2);
            rowHeights.BindIndices(0, 3);
            rowHeights.BindIndices(0, 4);
            rowHeights.SetPropertyScale(0, 2);
            rowHeights.SetPropertyScale(1, 1);
            rowHeights.SetPropertyScale(2, 2);
            rowHeights.SetPropertyScale(3, 1.5);
            rowHeights.SetPropertyScale(4, 1);

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


            this.intendedActivity_box = new ActivityNameEntryBox("What you planned to do (optional)");

            this.okButton = new Button();

            LayoutChoice_Set helpWindow = (new HelpWindowBuilder()).AddMessage("Use this screen to record participations.")
                .AddMessage("Type the name of the activity that you participated in, and press Enter if you want to take the autocomplete suggestion.")
                .AddMessage("You must have entered some activities in the activity name entry screen in order to enter them here.")
                .AddMessage("Notice that once you enter an activity name, ActivityRecommender will tell you how it estimates this will affect your longterm happiness.")
                .AddMessage("If you like, then you can enter a rating. The rating is a measurement of how much happiness you received per unit time for doing this activity divided by " +
                "the amount of happiness you received per unit time for doing the previous activity.")
                .AddMessage("Enter a start date and an end date. If you use the \"End = Now\" button right when the activity completes, you don't even need to type the date in. If you " +
                "do have to type the date in, press the white box.")
                .AddMessage("Enter a comment if you like. For the moment, the comments are just stored but don't do anything. That might change in the future.")
                .AddMessage("Lastly, press OK.")
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
            this.intendedActivity_box.Clear();
            this.CommentText = "";
            //this.setEnddateButton.SetDefaultBackground();
            this.predictedRating_block.Text = "";
        }
        public ActivityDatabase ActivityDatabase
        {
            set
            {
                this.nameBox.Database = value;
                this.intendedActivity_box.Database = value;
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

            ActivityDescriptor considerationDescriptor = this.intendedActivity_box.ActivityDescriptor;
            if (considerationDescriptor != null)
            {
                Consideration consideration = new Consideration(considerationDescriptor);
                participation.Consideration = consideration;
            }
            
            return participation;
        }


        public void ActivityName_BecameValid(object sender, TextChangedEventArgs e)
        {
            this.Invalidate_FeedbackBlock_Text();
        }

        private void Invalidate_FeedbackBlock_Text()
        {
            this.feedbackIsUpToDate = false;
            this.AnnounceChange(true);
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
            //return this.compute_estimatedRating_feedback(chosenActivity, startDate);
            //return this.compute_longtermValue_feedback(chosenActivity, startDate);
            return compute_longtermAndShortterm_feedback(chosenActivity, startDate);
        }

        private string compute_longtermAndShortterm_feedback(Activity chosenActivity, DateTime startDate)
        {
            double longtermBonusInDays = this.compute_longtermValue_increase(chosenActivity, startDate);
            if (longtermBonusInDays == -1)
            {
                // no data
                return "";
            }
            double shorttermValueRatio = this.compute_estimatedRating_ratio(chosenActivity, startDate);

            double roundedLongtermBonus = Math.Round(longtermBonusInDays, 3);
            double roundedShorttermRatio = Math.Round(shorttermValueRatio, 3);

            bool fun = (shorttermValueRatio > 1);
            bool productive = (longtermBonusInDays > 0);


            string message;
            if (fun)
            {
                if (productive)
                    message = "Nice! I bet that was fun (" + roundedShorttermRatio + " x avg) and productive (+" + roundedLongtermBonus + " days)";
                else
                    message = "Indulgent: I estimate that was fun (" + roundedShorttermRatio + " x avg) but unproductive (" + roundedLongtermBonus + " days)";
            }
            else
            {
                if (productive)
                    message = "Good work! That must've been difficult (fun = " + roundedShorttermRatio + " x avg) but productive (+" + roundedLongtermBonus + " days)";
                else
                    message = "Oops! Probably not fun (" + roundedShorttermRatio + " x avg) or productive (" + roundedLongtermBonus + " days)";
            }
            return message;
        }

        private string compute_estimatedRating_feedback(Activity chosenActivity, DateTime startDate)
        {
            Activity rootActivity = this.engine.ActivityDatabase.RootActivity;
            this.engine.EstimateSuggestionValue(chosenActivity, startDate);

            double expectedShortermRating = chosenActivity.PredictedScore.Distribution.Mean;
            double overallAverageRating = rootActivity.Ratings.Mean;
            double shorttermRatio = expectedShortermRating / overallAverageRating;

            return "Predicted rating = " + expectedShortermRating.ToString() + " * average";
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

        private double compute_longtermValue_increase(Activity chosenActivity, DateTime startDate)
        {
            Activity rootActivity = this.engine.ActivityDatabase.RootActivity;

            Distribution chosenEstimatedDistribution = this.engine.Get_Overall_ParticipationEstimate(chosenActivity, startDate).Distribution;
            if (chosenEstimatedDistribution.Weight <= 0)
                return -1;
            double chosenValue = chosenEstimatedDistribution.Mean;

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
            //this.engine.RatingSummarizer.
            if (bonusInSeconds == 0)
            {
                return 0;
            }
            double bonusInDays = bonusInSeconds / 60 / 60 / 24;
            double baselineDays = usualValue / weightOfThisMoment / 60 / 60 / 24;

            return bonusInDays;
        }

        private string compute_longtermValue_feedback(Activity chosenActivity, DateTime startDate)
        {
            double bonusInDays = this.compute_longtermValue_increase(chosenActivity, startDate);
            if (bonusInDays == -1)
            {
                // no data
                return "";
            }

            double roundedBonus = Math.Round(bonusInDays, 3);

            string message;
            if (bonusInDays > 0)
                message = "Nice! I estimate this worth +" + roundedBonus + " days.";
            else
                message = "Hmmm. I estimate this worth -" + (-bonusInDays) + " days.";
            return message;

        }


        // private member variables
        ActivityNameEntryBox nameBox;
        ActivityNameEntryBox intendedActivity_box;
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
    }
}