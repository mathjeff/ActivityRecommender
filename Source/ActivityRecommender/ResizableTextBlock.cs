using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace ActivityRecommendation
{
    class ResizableTextBlock : ResizableWrapper, IResizable
    {
        public ResizableTextBlock()
        {
            this.textBlock = new TextBlock();
            this.textBlock.TextWrapping = TextWrapping.Wrap;
            this.textBlock.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.SetContent(this.textBlock, new Resizability(0, 1));
        }

        public string Text
        {
            get
            {
                return this.textBlock.Text;
            }
            set
            {
                this.textBlock.Text = value;
            }
        }
        public TextAlignment TextAlignment
        {
            set
            {
                this.textBlock.TextAlignment = value;
            }
        }
        public void SetResizability(Resizability newResizability)
        {
            base.SetContent(this.textBlock, newResizability);
        }

        public override Size PreliminaryMeasure(Size constraint)
        {
            // ask the TextBlock how big it wants to be
            this.textBlock.Measure(new Size(constraint.Width, double.PositiveInfinity));
            Size rawSize = this.textBlock.DesiredSize;
            return rawSize;

            /*
            // Now try again, leaving the margins slightly more equal in case other visuals need it elsewhere
            double fractionUsed = this.textBlock.DesiredSize.Width * this.textBlock.DesiredSize.Height / constraint.Width / constraint.Height;
            if (fractionUsed >= 1)
                return rawSize;
            Size adjustedSize = new Size(constraint.Width * Math.Sqrt(fractionUsed), double.PositiveInfinity);
            this.textBlock.Measure(adjustedSize);

            return this.textBlock.DesiredSize;
            */
        }
        public override Size FinalMeasure(Size constraint)
        {
            this.Measure(constraint);
            return constraint;
        }
        protected override System.Windows.Size MeasureOverride(System.Windows.Size constraint)
        {
            this.textBlock.Measure(constraint);
            return this.textBlock.DesiredSize;
        }
        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            Rect rect = new Rect(new Point(), arrangeBounds);
            this.textBlock.Arrange(rect);
            return arrangeBounds;
        }

        public TextBlock textBlock;
    }
}
