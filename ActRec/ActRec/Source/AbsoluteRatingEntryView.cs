
namespace ActivityRecommendation
{
    class AbsoluteRatingEntryView : TitledTextbox
    {
        public AbsoluteRatingEntryView()
            : base("Scale: 0-1")
        {
        }
        public Rating GetRating()
        {
            AbsoluteRating rating = new AbsoluteRating();
            rating.Score = double.Parse(this.Text);
            return rating;
        }
    }
}
