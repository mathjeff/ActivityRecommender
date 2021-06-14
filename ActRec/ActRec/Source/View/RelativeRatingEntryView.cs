using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using VisiPlacement;

using System.Windows.Input;
using Xamarin.Forms;

namespace ActivityRecommendation
{
    class RelativeRatingEntryView : TitledControl
    {
        public event RatingRatioChanged_Handler RatingRatioChanged;
        public delegate void RatingRatioChanged_Handler();
        public RelativeRatingEntryView() : base("")
        {
            this.TitleLayout.AlignVertically(TextAlignment.Center);
            this.mainDisplayGrid = GridLayout.New(new BoundProperty_List(2), new BoundProperty_List(1), LayoutScore.Zero);

            this.scaleBox = new Editor();
            this.scaleBox.Keyboard = Keyboard.Numeric;
            this.scaleBox.TextChanged += this.ScaleBlock_TextChanged;
            this.scaleBoxLayout = new TextboxLayout(this.scaleBox);
            ContainerLayout scaleBoxHolder = new ContainerLayout(null, this.scaleBoxLayout, false);
            // The fact that the rating is relative to the previous participation is really important, so we put this text into its own text block.
            // Additionally, the timesBlock might be able to fit into the same line as the text box into which the user types the rating ratio.
            TextblockLayout timesBlock = new TextblockLayout("times").AlignVertically(TextAlignment.Center).AlignHorizontally(TextAlignment.Center);

            LayoutChoice_Set horizontalBox = new Horizontal_GridLayout_Builder()
                .AddLayout(scaleBoxHolder)
                .AddLayout(timesBlock)
                .BuildAnyLayout();

            LayoutChoice_Set verticalBox = new Vertical_GridLayout_Builder()
                .AddLayout(scaleBoxHolder)
                .AddLayout(timesBlock)
                .BuildAnyLayout();

            this.Clear();
            this.mainDisplayGrid.AddLayout(new LayoutUnion(horizontalBox, verticalBox));

            // We try to use large font for the name layout and we also try to use as many clarifying words as possible
            // If not all of the words fit onscreen at large font, we will shrink the font and also potentially remove some words
            this.fullNameLayout = new TextblockLayout();
            this.shortenedNameLayout = new TextblockLayout("", 10);
            this.mainDisplayGrid.AddLayout(
                new LayoutUnion(
                    new List<LayoutChoice_Set>()
                    {
                        this.fullNameLayout,
                        // We don't want to remove the clarifying words (so we give it a score penalty), but it is better than cropping
                        new ScoreShifted_Layout(this.shortenedNameLayout, LayoutScore.Get_ReducedContent_Score(1))
                    }
                )
            );

            this.Placeholder("(Optional)");
        }

        private void ScaleBlock_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.RatingRatioChanged != null)
                this.RatingRatioChanged.Invoke();
            this.UpdateColor();
        }
        private bool IsRatioValid()
        {
            return (this.GetRatio() != null);
        }
        public double? GetRatio()
        {
            string text = this.scaleBox.Text;
            try
            {
                double value = double.Parse(this.scaleBox.Text);
                if (value < 0)
                    return null;
                return value;
            }
            catch (FormatException)
            {
                return null;
            }
        }
        public void Placeholder(string text)
        {
            this.scaleBox.Placeholder = text;
        }
        void UpdateColor()
        {
            if (this.IsRatioValid() || this.scaleBox.Text == "")
                this.AppearValid();
            else
                this.AppearInvalid();
        }

        // alters the appearance to indicate that the given date is not valid
        void AppearValid()
        {
            this.scaleBoxLayout.SetBackgroundColor(Color.FromRgba(0, 0, 0, 0));
        }

        // alters the appearance to indicate that the given date is not valid
        void AppearInvalid()
        {
            this.scaleBoxLayout.SetBackgroundColor(Color.Red);
        }

        public Participation LatestParticipation
        {
            set
            {
                this.latestParticipation = value;

                if (this.latestParticipation != null)
                {
                    DateTime now = DateTime.Now;
                    string dateFormatString;
                    // Show the day if it happened more than 24 hours ago
                    if (now.Subtract(value.StartDate).CompareTo(new TimeSpan(24, 0, 0)) > 0)
                        dateFormatString = "yyyy-MM-ddTHH:mm:ss";
                    else
                        dateFormatString = "HH:mm:ss";
                    string prevDescription = "" + value.ActivityDescriptor.ActivityName + " from " + value.StartDate.ToString(dateFormatString) + " to " + value.EndDate.ToString(dateFormatString);
                    this.fullNameLayout.setText("the score of your latest participation in " + prevDescription);
                    this.shortenedNameLayout.setText(prevDescription);
                    this.SetContent(this.mainDisplayGrid);
                }
                else
                {
                    this.SetContent(null);
                }
            }
            get
            {
                return this.latestParticipation;
            }
        }
        // creates the rating to assign to the given Participation
        public Rating GetRelativeRating(Engine engine, Participation participation)
        {
            // abort if null input
            double? maybeScale = this.GetRatio();
            if (maybeScale == null)
                return engine.MakeEstimatedRating(participation);
            double scale = maybeScale.Value;
            if (this.latestParticipation == null)
                return null;
            RelativeRating rating = engine.MakeRelativeRating(participation.ActivityDescriptor, participation.StartDate, scale, this.latestParticipation);
            return rating;
        }
        // provides what the actual rating was compared to what a typical rating would be
        // 0 means within normal expectations (1 stddev)
        // 1 means more than expected
        // -1 means less than expected
        public void SetRatingComparison(int ratingComparison)
        {
            this.ratingComparison = ratingComparison;
            this.UpdateReactionText();
        }

        private void UpdateReactionText()
        {
            if (this.ratingComparison > 0)
            {
                this.SetTitle("Wow! Score:");
            }
            else
            {
                if (this.ratingComparison < 0)
                {
                    this.SetTitle("Aww. Score:");
                }
                else
                {
                    this.SetTitle("Score:");
                }
            }
        }

        public void Clear()
        {
            this.scaleBox.Text = "";
            this.ratingComparison = 0;
            this.UpdateReactionText();
        }

        private GridLayout mainDisplayGrid;
        private Participation latestParticipation;
        private TextblockLayout fullNameLayout;
        private TextblockLayout shortenedNameLayout;
        private Editor scaleBox;
        private TextboxLayout scaleBoxLayout;
        private int ratingComparison;
    }
}
