using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows;

// the ParticipationEntryView provides a place for the user to describe what they've done recently
namespace ActivityRecommendation
{
    class ParticipationEntryView : TitledControl
    {
        public ParticipationEntryView()
        {
            this.SetTitle("Type What You've Been Doing");
            DisplayGrid contents = new DisplayGrid(4, 2);

            this.nameBox = new ActivityNameEntryBox("Name");
            contents.AddItem(this.nameBox);

            this.ratingBox = new RatingEntryView("Rating (optional)");
            //this.ratingBox.Background = System.Windows.Media.Brushes.Yellow;
            contents.AddItem(this.ratingBox);



            this.startDateBox = new DateEntryView("StartDate");
            //this.startDateBox.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            contents.AddItem(this.startDateBox);

            this.endDateBox = new DateEntryView("EndDate");
            //this.endDateBox.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            contents.AddItem(this.endDateBox);

            this.commentBox = new TitledTextbox("Comment (optional)");
            contents.AddItem(this.commentBox);

            this.autofillButton = new Button();
            this.autofillButton.Content = "Autofill";
            this.autofillButton.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            contents.AddItem(this.autofillButton);

            this.okButton = new Button();
            this.okButton.Content = "OK";
            this.okButton.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            contents.AddItem(this.okButton);

            this.SetContent(contents);
            //this.Background = System.Windows.Media.Brushes.Blue;
        }
        public void Clear()
        {
            this.ratingBox.Clear();
            this.ActivityName = "";
            this.CommentText = "";
        }
        public ActivityDatabase ActivityDatabase
        {
            set
            {
                this.nameBox.Database = value;
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
        public string ActivityName
        {
            get
            {
                return this.nameBox.NameText;
            }
            set
            {
                this.nameBox.NameText = value;
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
        public void AddOkClickHandler(RoutedEventHandler e)
        {
            this.okButton.Click += e;
        }
        public void AddAutofillClickHandler(RoutedEventHandler e)
        {
            this.autofillButton.Click += e;
        }
        public Participation GetParticipation(ActivityDatabase activities, Engine engine)
        {
            ActivityDescriptor descriptor = new ActivityDescriptor();
            descriptor.ActivityName = this.ActivityName;

            Participation participation = new Participation(this.StartDate, this.EndDate, descriptor);
            if (this.CommentText != "" && this.CommentText != null)
                participation.Comment = this.CommentText;


            try
            {
                Rating rating = this.ratingBox.GetRating(activities, engine, participation);
                //AbsoluteRating rating = new AbsoluteRating();
                //rating.Score = double.Parse(this.ratingBox.Text);
                participation.RawRating = rating;
            }
            catch (Exception)
            {
            }
            return participation;
        }
        

        // private member variables
        ActivityNameEntryBox nameBox;
        RatingEntryView ratingBox;
        TitledTextbox commentBox;
        DateEntryView startDateBox;
        DateEntryView endDateBox;
        Button okButton;
        Button autofillButton;
    }
}