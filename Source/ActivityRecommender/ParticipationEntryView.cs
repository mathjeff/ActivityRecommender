using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

// the ParticipationEntryView provides a place for the user to describe what they've done recently
namespace ActivityRecommendation
{
    class ParticipationEntryView : TitledControl
    {
        public ParticipationEntryView()
        {
            this.SetTitle("Type What You've Been Doing");
            DisplayGrid contents = new DisplayGrid(5, 2);

            this.nameBox = new ActivityNameEntryBox("Activity Name");
            this.nameBox.AddTextChangedHandler(new TextChangedEventHandler(this.nameBox_TextChanged));
            contents.AddItem(this.nameBox);

            this.ratingBox = new RatingEntryView("Rating (optional)");
            //this.ratingBox.Background = System.Windows.Media.Brushes.Yellow;
            contents.AddItem(this.ratingBox);



            this.startDateBox = new DateEntryView("StartDate");
            this.startDateBox.Add_TextChanged_Handler(new TextChangedEventHandler(this.DateText_Changed));
            //this.startDateBox.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            contents.AddItem(this.startDateBox);

            this.endDateBox = new DateEntryView("EndDate");
            this.endDateBox.Add_TextChanged_Handler(new TextChangedEventHandler(this.DateText_Changed));
            //this.endDateBox.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            contents.AddItem(this.endDateBox);


            this.setStartdateButton = new ResizableButton();
            this.setStartdateButton.Content = "Set start = now";
            this.setStartdateButton.VerticalAlignment = VerticalAlignment.Center;
            this.setStartdateButton.Click += new RoutedEventHandler(setStartdateButton_Click);
            contents.AddItem(this.setStartdateButton);

            this.setEnddateButton = new ResizableButton();
            this.setEnddateButton.Content = "Set end = now";
            this.setEnddateButton.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            this.setEnddateButton.Click += new RoutedEventHandler(setEnddateButton_Click);
            contents.AddItem(this.setEnddateButton);

            this.intendedActivity_box = new ActivityNameEntryBox("What you planned to do (optional)");
            contents.AddItem(this.intendedActivity_box);

            this.commentBox = new TitledTextbox("Comment (optional)");
            contents.AddItem(this.commentBox);

            this.okButton = new ResizableButton();
            this.okButton.Content = "OK";
            this.okButton.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            contents.AddItem(this.okButton);

            this.SetContent(contents);
            //this.Background = System.Windows.Media.Brushes.Blue;
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
        

        // private member variables
        ActivityNameEntryBox nameBox;
        ActivityNameEntryBox intendedActivity_box;
        RatingEntryView ratingBox;
        TitledTextbox commentBox;
        DateEntryView startDateBox;
        DateEntryView endDateBox;
        ResizableButton setStartdateButton;
        ResizableButton setEnddateButton;
        ResizableButton okButton;
    }
}