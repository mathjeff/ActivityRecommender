using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace ActivityRecommendation
{
    class ResizableTextBox : ResizableWrapper, IResizable
    {
        public ResizableTextBox()
        {
            this.textBox = new TextBox();
            this.textBox.TextWrapping = TextWrapping.Wrap;
            this.textBox.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.textBox.Background = null;
            this.Background = Brushes.White;
            this.SetContent(this.textBox, new Resizability(1, 1));
        }
        public void AddTextChangedHandler(TextChangedEventHandler h)
        {
            this.textBox.TextChanged += h;
        }
        public string Text
        {
            get
            {
                return this.textBox.Text;
            }
            set
            {
                this.textBox.Text = value;
            }
        }
        public TextAlignment TextAlignment
        {
            set
            {
                this.textBox.TextAlignment = value;
            }
        }
        public void SetResizability(Resizability newResizability)
        {
            base.SetContent(this.textBox, newResizability);
        }

        public override Size PreliminaryMeasure(Size constraint)
        {
            // ask the TextBox how big it wants to be
            this.textBox.Measure(new Size(constraint.Width, double.PositiveInfinity));
            Size rawSize = this.textBox.DesiredSize;
            return rawSize;

            /*
            // Now try again, leaving the margins slightly more equal in case other visuals need it elsewhere
            double fractionUsed = this.textBox.DesiredSize.Width * this.textBox.DesiredSize.Height / constraint.Width / constraint.Height;
            if (fractionUsed >= 1)
                return rawSize;
            Size adjustedSize = new Size(constraint.Width * Math.Sqrt(fractionUsed), double.PositiveInfinity);
            this.textBox.Measure(adjustedSize);

            return this.textBox.DesiredSize;
            */
        }
        public override Size FinalMeasure(Size constraint)
        {
            this.Measure(constraint);
            return constraint;
        }
        protected override System.Windows.Size MeasureOverride(System.Windows.Size constraint)
        {
            this.textBox.Measure(constraint);
            return this.textBox.DesiredSize;
        }
        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            Rect rect = new Rect(new Point(), arrangeBounds);
            this.textBox.Arrange(rect);
            return arrangeBounds;
        }

        private TextBox textBox;
    }
}
