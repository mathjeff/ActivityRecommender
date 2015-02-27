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

            GridLayout contents = GridLayout.New(BoundProperty_List.Uniform(4), BoundProperty_List.Uniform(2), LayoutScore.Zero);

            this.nameBox = new ActivityNameEntryBox("Activity Name");
            this.nameBox.AddTextChangedHandler(new TextChangedEventHandler(this.nameBox_TextChanged));
            this.nameBox.NameMatchedSuggestion += new NameMatchedSuggestionHandler(this.ActivityName_BecameValid);
            contents.AddLayout(this.nameBox);

            //this.ratingBox = new RatingEntryView("Rating (optional)");
            this.ratingBox = new RelativeRatingEntryView();
            //this.ratingBox.Background = System.Windows.Media.Brushes.Yellow;
            contents.AddLayout(this.ratingBox);



            this.startDateBox = new DateEntryView("StartDate");
            this.startDateBox.Add_TextChanged_Handler(new TextChangedEventHandler(this.DateText_Changed));
            //this.startDateBox.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            contents.AddLayout(this.startDateBox);

            this.endDateBox = new DateEntryView("EndDate");
            this.endDateBox.Add_TextChanged_Handler(new TextChangedEventHandler(this.DateText_Changed));
            //this.endDateBox.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            contents.AddLayout(this.endDateBox);


            TextBlock startDateButton_textBlock = new TextBlock();
            this.setStartdateButton = new ResizableButton();
            startDateButton_textBlock.Text = "Set start = now";
            this.setStartdateButton.VerticalAlignment = VerticalAlignment.Center;
            this.setStartdateButton.Click += new RoutedEventHandler(setStartdateButton_Click);
            contents.AddLayout(new ButtonLayout(this.setStartdateButton, new TextblockLayout(startDateButton_textBlock)));
            /*Image image = new Image();
            image.Source = ImageLoader.loadImage("icon.png");
            image.Stretch = Stretch.Fill;
            ImageLayout imageLayout = new ImageLayout(image, new LayoutScore(100, 100));
            TextBlock testBox = startDateButton_textBlock;
            testBox.Text = "a s d f\nz y x w";*/
            //ontents.AddLayout(new ButtonLayout(this.setStartdateButton, new TextblockLayout(testBox)));
            //contents.AddLayout(imageLayout);

            this.setEnddateButton = new ResizableButton();
            TextBlock endDateButton_textBlock = new TextBlock();
            endDateButton_textBlock.Text = "Set end = now";
            this.setEnddateButton.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            this.setEnddateButton.Click += new RoutedEventHandler(setEnddateButton_Click);
            contents.AddLayout(new ButtonLayout(this.setEnddateButton, new TextblockLayout(endDateButton_textBlock)));

            this.intendedActivity_box = new ActivityNameEntryBox("What you planned to do (optional)");
            //contents.AddLayout(this.intendedActivity_box);
            this.commentBox = new TitledTextbox("Comment (optional)");
            //contents.AddLayout(this.commentBox);
            

            this.okButton = new ResizableButton();
            TextBlock okButtonTextBlock = new TextBlock();
            okButtonTextBlock.Text = "OK";
            this.okButton.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            contents.AddLayout(new ButtonLayout(this.okButton, new TextblockLayout(okButtonTextBlock)));

            this.predictedRating_block = new TextBlock();
            contents.AddLayout(new TextblockLayout(this.predictedRating_block));



            this.helpWindow = (new HelpWindowBuilder()).AddMessage("Use this screen to record activities you have done.")
                .AddMessage("Type the name of the activity, and press Enter if you want to take the autocomplete suggestion")
                .AddMessage("Enter a rating if you like")
                .AddMessage("Enter a start date and an end date")
                .AddMessage("Lastly, click OK!")
                .Build();

            ResizableButton helpButton = new ResizableButton();
            helpButton.Click += helpButton_Click;

            contents.AddLayout(new ButtonLayout(helpButton, new TextblockLayout("Help")));

            this.SetContent(contents);

        }

        void helpButton_Click(object sender, RoutedEventArgs e)
        {
            this.layoutStack.AddLayout(this.helpWindow);
        }

        public void DateText_Changed(object sender, TextChangedEventArgs e)
        {
            try
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
            catch (FormatException)
            {
                // either the start date or end date is invalid
            }
        }
        void setEnddateButton_Click(object sender, RoutedEventArgs e)
        {
            this.setEnddateButton.UnHighlight();
        }
        void setStartdateButton_Click(object sender, RoutedEventArgs e)
        {
            this.setEnddateButton.Highlight();
            this.Update_ExpectedRating_Text();
        }
        void nameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.setEnddateButton.Highlight();
        }

        public void Clear()
        {
            this.ratingBox.Clear();
            this.nameBox.NameText = "";
            this.intendedActivity_box.NameText = "";
            this.CommentText = "";
            this.setEnddateButton.SetDefaultBackground();
            this.predictedRating_block.Text = "";
        }
        public ActivityDatabase ActivityDatabase
        {
            set
            {
                this.nameBox.Database = value;
                this.intendedActivity_box.Database = value;
                //this.ratingBox.ActivityDatabase = value;
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
            if (newName != "" && newName != null)
                this.setEnddateButton.Highlight();
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
            this.setEnddateButton.SetDefaultBackground();
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

            Participation participation = new Participation(this.StartDate, this.EndDate, descriptor);
            if (this.CommentText != "" && this.CommentText != null)
                participation.Comment = this.CommentText;


            try
            {
                Rating rating = this.ratingBox.GetRating(activities, engine, participation);
                participation.RawRating = rating;
            }
            catch (Exception)
            {
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
                    //this.predictedRating_block.Text = "Predicted Rating = " + activity.PredictedScore.Distribution.Mean.ToString() + "\nfor " + activity.Name + "\nat " + startDate.ToString();
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
        ResizableButton setStartdateButton;
        ResizableButton setEnddateButton;
        ResizableButton okButton;
        TextBlock predictedRating_block;
        Engine engine;
        LayoutStack layoutStack;
        LayoutChoice_Set helpWindow;
    }
}