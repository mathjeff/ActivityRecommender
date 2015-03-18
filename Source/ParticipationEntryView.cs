using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using VisiPlacement;
using System.Windows.Media.Imaging;

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
            rowHeights.SetPropertyScale(0, 2);
            rowHeights.SetPropertyScale(1, 2);
            rowHeights.SetPropertyScale(2, 2);
            rowHeights.SetPropertyScale(3, 1);

            GridLayout contents = GridLayout.New(rowHeights, BoundProperty_List.Uniform(1), LayoutScore.Zero);

            this.nameBox = new ActivityNameEntryBox("Activity Name");
            this.nameBox.AddTextChangedHandler(new TextChangedEventHandler(this.nameBox_TextChanged));
            this.nameBox.NameMatchedSuggestion += new NameMatchedSuggestionHandler(this.ActivityName_BecameValid);
            contents.AddLayout(this.nameBox);

            GridLayout grid2 = GridLayout.New(BoundProperty_List.Uniform(1), BoundProperty_List.Uniform(2), LayoutScore.Zero);
            this.ratingBox = new RelativeRatingEntryView();
            grid2.AddLayout(this.ratingBox);

            this.helpWindow = (new HelpWindowBuilder()).AddMessage("Use this screen to record activities you have done.")
                .AddMessage("Type the name of the activity, and press Enter if you want to take the autocomplete suggestion")
                .AddMessage("Enter a rating if you like")
                .AddMessage("Enter a start date and an end date")
                .AddMessage("Lastly, click OK!")
                .Build();

            Button helpButton = new Button();
            helpButton.Click += helpButton_Click;
            grid2.AddLayout(new ButtonLayout(helpButton, new TextblockLayout("Help")));
            contents.AddLayout(grid2);

            GridLayout grid3 = GridLayout.New(BoundProperty_List.Uniform(2), BoundProperty_List.Uniform(2), LayoutScore.Zero);

            this.startDateBox = new DateEntryView("Start Time");
            this.startDateBox.Add_TextChanged_Handler(new TextChangedEventHandler(this.DateText_Changed));
            grid3.AddLayout(this.startDateBox);
            this.endDateBox = new DateEntryView("End Time");
            this.endDateBox.Add_TextChanged_Handler(new TextChangedEventHandler(this.DateText_Changed));
            grid3.AddLayout(this.endDateBox);
            this.setStartdateButton = new Button();
            grid3.AddLayout(new ButtonLayout(this.setStartdateButton, "Set start = now"));
            this.setEnddateButton = new Button();
            grid3.AddLayout(new ButtonLayout(this.setEnddateButton, "Set end = now"));
            contents.AddLayout(grid3);


            this.intendedActivity_box = new ActivityNameEntryBox("What you planned to do (optional)");
            this.commentBox = new TitledTextbox("Comment (optional)");

            GridLayout grid4 = GridLayout.New(BoundProperty_List.Uniform(1), BoundProperty_List.Uniform(2), LayoutScore.Zero);
            this.okButton = new Button();
            grid4.AddLayout(new ButtonLayout(this.okButton, "OK"));
            this.predictedRating_block = new TextBlock();
            grid4.AddLayout(new TextblockLayout(this.predictedRating_block));
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
            this.Update_ExpectedRating_Text();
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
            this.Update_ExpectedRating_Text();
        }


        private void Update_ExpectedRating_Text()
        {
            if (this.startDateBox.IsDateValid() && this.nameBox.ActivityDescriptor != null)
            {
                DateTime startDate = this.startDateBox.GetDate();
                Activity activity = this.engine.ActivityDatabase.ResolveDescriptor(this.nameBox.ActivityDescriptor);
                if (activity != null)
                {
                    this.engine.EstimateRating(activity, startDate);
                    this.predictedRating_block.Text = "Predicted Rating = " + activity.PredictedScore.Distribution.Mean.ToString() + " for " + activity.Name + " at " + startDate.ToString();
                }
            }
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