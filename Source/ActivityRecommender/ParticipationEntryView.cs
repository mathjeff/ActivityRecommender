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
            DisplayGrid contents = new DisplayGrid(3, 2);

            this.nameBox = new ActivityNameEntryBox("Name");
            contents.AddItem(this.nameBox);

            this.ratingBox = new TitledTextbox("Rating (0-1) (optional)");
            contents.AddItem(this.ratingBox);

            this.startDateBox = new DateEntryView("StartDate");
            this.startDateBox.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            contents.AddItem(this.startDateBox);

            this.endDateBox = new DateEntryView("EndDate");
            this.endDateBox.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            contents.AddItem(this.endDateBox);

            this.autofillButton = new Button();
            this.autofillButton.Content = "Autofill";
            this.autofillButton.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            contents.AddItem(this.autofillButton);

            this.okButton = new Button();
            this.okButton.Content = "OK";
            this.okButton.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            contents.AddItem(this.okButton);

            this.SetContent(contents);
            //this.Background = System.Windows.Media.Brushes.Blue;
        }
        public ActivityDatabase ActivityDatabase
        {
            set
            {
                this.nameBox.Database = value;
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
            get
            {
                return this.ratingBox.Text;
            }
            set
            {
                this.ratingBox.Text = value;
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
        public Participation Participation
        {
            get
            {
                ActivityDescriptor descriptor = new ActivityDescriptor();
                descriptor.ActivityName = this.ActivityName;

                Participation result = new Participation(this.StartDate, this.EndDate, descriptor);
                return result;
            }
        }
        public AbsoluteRating Rating
        {
            get
            {
                string text = this.ratingBox.Text;
                try
                {
                    double score = double.Parse(text);
                    Participation participation = this.Participation;
                    AbsoluteRating result = new AbsoluteRating(score, participation.StartDate,participation.ActivityDescriptor, null);
                    return result;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        // private member variables
        ActivityNameEntryBox nameBox;
        TitledTextbox ratingBox;
        DateEntryView startDateBox;
        DateEntryView endDateBox;
        Button okButton;
        Button autofillButton;
    }
}