using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace ActivityRecommendation
{
    class AbsoluteRatingEntryView : TitledTextbox, IResizable
    {
        public AbsoluteRatingEntryView()
            : base("Scale: 0-1")
        {
        }
        /*public Resizability GetHorizontalResizability()
        {
            return new Resizability(1, 1);
        }
        public Resizability GetVerticalResizability()
        {
            return new Resizability(1, 1);
        }
        public Size PreliminaryMeasure(Size constraint)
        {
            return this.FinalMeasure(constraint);
        }
        public Size FinalMeasure(Size constraint)
        {
            return base.MeasureOverride(constraint);
        }
        protected override Size MeasureOverride(Size constraint)
        {
            return this.FinalMeasure(constraint);
        }*/
        public Rating GetRating()
        {
            AbsoluteRating rating = new AbsoluteRating();
            rating.Score = double.Parse(this.Text);
            return rating;
        }
    }
}
