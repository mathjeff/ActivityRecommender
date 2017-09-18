using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using VisiPlacement;
using System.Windows.Media.Imaging;
using System.Windows.Input;

// the ParticipationEntryView provides a place for the user to describe what they've done recently
namespace ActivityRecommendation
{
    class ParticipationEntryView : TitledControl
    {
        public ParticipationEntryView(LayoutStack layoutStack) : base("Type What You've Been Doing")
        {
            this.layoutStack = layoutStack;

            BoundProperty_List rowHeights = new BoundProperty_List(4);
            rowHeights.BindIndices(0, 1);
            rowHeights.BindIndices(0, 2);
            rowHeights.BindIndices(0, 3);
            rowHeights.SetPropertyScale(0, 4);
            rowHeights.SetPropertyScale(1, 8);
            rowHeights.SetPropertyScale(2, 2);
            rowHeights.SetPropertyScale(3, 1);

            GridLayout contents = GridLayout.New(rowHeights, BoundProperty_List.Uniform(1), LayoutScore.Zero);

            this.nameBox = new ActivityNameEntryBox("Activity Name");
            this.nameBox.AddTextChangedHandler(new TextChangedEventHandler(this.nameBox_TextChanged));
            this.nameBox.NameMatchedSuggestion += new NameMatchedSuggestionHandler(this.ActivityName_BecameValid);
            contents.AddLayout(this.nameBox);

            GridLayout ratingGrid = GridLayout.New(BoundProperty_List.Uniform(2), BoundProperty_List.Uniform(1), LayoutScore.Zero);
            this.ratingBox = new RelativeRatingEntryView();
            ratingGrid.AddLayout(this.ratingBox);

            this.predictedRating_block = new TextBlock();
            ratingGrid.AddLayout(new TextblockLayout(this.predictedRating_block));


            TextBox commentBox = new TextBox();
            InputScope inputScope = new InputScope();
            InputScopeName inputScopeName = new InputScopeName();
            inputScopeName.NameValue = InputScopeNameValue.Text;
            inputScope.Names.Add(inputScopeName);
            commentBox.InputScope = inputScope;
            this.commentBox = new TitledTextbox("Comment (optional)", commentBox);



            TextBox box = new TextBox();

            GridLayout middleGrid = GridLayout.New(BoundProperty_List.Uniform(1), BoundProperty_List.Uniform(2), LayoutScore.Zero);
            middleGrid.AddLayout(ratingGrid);
            middleGrid.AddLayout(this.commentBox);
            
            contents.AddLayout(middleGrid);

            GridLayout grid3 = GridLayout.New(BoundProperty_List.Uniform(1), BoundProperty_List.Uniform(2), LayoutScore.Zero);

            this.startDateBox = new DateEntryView("Start Time");
            this.startDateBox.Add_TextChanged_Handler(new TextChangedEventHandler(this.DateText_Changed));
            grid3.AddLayout(this.startDateBox);
            this.endDateBox = new DateEntryView("End Time");
            this.endDateBox.Add_TextChanged_Handler(new TextChangedEventHandler(this.DateText_Changed));
            grid3.AddLayout(this.endDateBox);
            contents.AddLayout(grid3);
            this.setStartdateButton = new Button();
            this.setEnddateButton = new Button();


            this.intendedActivity_box = new ActivityNameEntryBox("What you planned to do (optional)");

            this.okButton = new Button();

            this.helpWindow = (new HelpWindowBuilder()).AddMessage("Use this screen to record activities you have done.")
                .AddMessage("Type the name of the activity, and press Enter if you want to take the autocomplete suggestion")
                .AddMessage("Enter a rating if you like")
                .AddMessage("Enter a start date and an end date")
                .AddMessage("Lastly, click OK!")
                .Build();

            Button helpButton = new Button();
            helpButton.Click += helpButton_Click;

            GridLayout grid4 = GridLayout.New(BoundProperty_List.Uniform(1), BoundProperty_List.Uniform(4), LayoutScore.Zero);
            grid4.AddLayout(new ButtonLayout(this.setStartdateButton, "Start = now"));
            grid4.AddLayout(new ButtonLayout(this.okButton, "OK"));
            grid4.AddLayout(new ButtonLayout(helpButton, new TextblockLayout("Help")));
            grid4.AddLayout(new ButtonLayout(this.setEnddateButton, "End = now"));
            contents.AddLayout(grid4);

            this.SetContent(contents);
        }

        void helpButton_Click(object sender, RoutedEventArgs e)
        {
            this.layoutStack.AddLayout(this.helpWindow);
        }

        public void DateText_Changed(object sender, TextChangedEventArgs e)
        {
            if (this.startDateBox.IsDateValid() && this.endDateBox.IsDateValid())
            {
                if (this.StartDate.CompareTo(this.EndDate) <= 0)
                {
                    // start date is before end date
                    this.startDateBox.appearValid();
                    this.endDateBox.appearValid();
                }
                else
                {
                    // start date is after end date
                    this.startDateBox.appearInvalid();
                    this.endDateBox.appearInvalid();
                }
            }
        }
        void setEnddateButton_Click(object sender, RoutedEventArgs e)
        {
            //this.setEnddateButton.UnHighlight();
        }
        void setStartdateButton_Click(object sender, RoutedEventArgs e)
        {
            //this.setEnddateButton.Highlight();
            this.Update_FeedbackBlock_Text();
        }
        void nameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //this.setEnddateButton.Highlight();
        }

        public void Clear()
        {
            this.ratingBox.Clear();
            this.nameBox.NameText = "";
            this.intendedActivity_box.NameText = "";
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
            this.nameBox.NameText = newName;
            this.Update_FeedbackBlock_Text();
            //if (newName != "" && newName != null)
            //    this.setEnddateButton.Highlight();
        }
        public string ActivityName
        {
            get
            {
                return this.nameBox.NameText;
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

        public void AddOkClickHandler(RoutedEventHandler h)
        {
            this.okButton.Click += h;
        }
        public void AddSetenddateHandler(RoutedEventHandler h)
        {
            this.setEnddateButton.Click += h;
        }
        public void SetEnddateNow(DateTime when)
        {
            DateTime now = DateTime.Now;
            this.endDateBox.SetDate(now);
            //this.setEnddateButton.SetDefaultBackground();
        }
        public void AddSetstartdateHandler(RoutedEventHandler h)
        {
            this.setStartdateButton.Click += h;
        }
        public Participation GetParticipation(ActivityDatabase activities, Engine engine)
        {
            // Fill in the necessary properties
            ActivityDescriptor descriptor = new ActivityDescriptor();
            descriptor.ActivityName = this.ActivityName;
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
            this.Update_FeedbackBlock_Text();
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
        }

        private string computeFeedback(Activity chosenActivity, DateTime startDate)
        {
            //return this.compute_estimatedRating_feedback(chosenActivity, startDate);
            return this.compute_longtermValue_feedback(chosenActivity, startDate);
        }

        private string compute_estimatedRating_feedback(Activity chosenActivity, DateTime startDate)
        {
            Activity rootActivity = this.engine.ActivityDatabase.RootActivity;
            this.engine.EstimateSuggestionValue(chosenActivity, startDate);

            double expectedShortermRating = chosenActivity.PredictedScore.Distribution.Mean;
            double overallAverageRating = rootActivity.Scores.Mean;
            double shorttermRatio = expectedShortermRating / overallAverageRating;

            return "Predicted rating = " + expectedShortermRating.ToString() + " * average";
        }

        private string compute_longtermValue_feedback(Activity chosenActivity, DateTime startDate)
        {
            Activity rootActivity = this.engine.ActivityDatabase.RootActivity;

            // estimate the activity's rating so that its parent activities will have their ratings estimated,
            // because the parent rating estimates are used as coordinates
            this.engine.EstimateRating(chosenActivity, startDate);
            //this.engine.EstimateSuggestionValue(chosenActivity, startDate);
            double chosenValue = chosenActivity.Predict_LongtermValue_If_Participated(startDate).Mean;

            //DateTime historyStart = new DateTime();
            //RatingSummary summary = new RatingSummary(historyStart);

            //summary.Update(this.engine.RatingSummarizer, historyStart, startDate);
            //double usualValue = summary.Item.Mean;
            double usualValue = rootActivity.Predict_LongtermValue_If_Participated(startDate).Mean;

            double bonusInSeconds = 0;
            if (usualValue != 0)
            {
                // relWeight(x) = 1 * 2^(-x/halfLife)
                // totalWeight = integral(relWeight) = (log(e)/log(2))*halfLife
                // absWeight(x) = 2^(-x/halfLife)/((log(e)/log(2))*halfLife)
                // absWeight(0) = 1/((log(e)/log(2))*halfLife)
                // absWeight(0) = log(2)/log(e)/halflife
                // absWeight(0) = log(2)/halflife

                double weightOfThisMoment = Math.Log(2) / UserPreferences.DefaultPreferences.HalfLife.TotalSeconds;

                double overallImprovment = (chosenValue / usualValue) - 1;

                bonusInSeconds = overallImprovment / weightOfThisMoment;
            }
            //this.engine.RatingSummarizer.
            if (bonusInSeconds == 0)
            {
                return "";
            }
            string message;
            double bonusInDays = bonusInSeconds / 60 / 60 / 24;
            if (bonusInDays > 0)
                message = "Nice! I estimate this worth +" + bonusInDays + " days.";
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
        TextBlock predictedRating_block;
        Engine engine;
        LayoutStack layoutStack;
        LayoutChoice_Set helpWindow;
    }
}