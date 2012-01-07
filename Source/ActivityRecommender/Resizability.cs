using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace ActivityRecommendation
{
    interface IResizable
    {
        Resizability GetHorizontalResizability();
        Resizability GetVerticalResizability();
        Size PreliminaryMeasure(Size constraint);    // This replaces all Measure() calls except the final one
        Size FinalMeasure(Size finalSize);         // This replaces the final Measure() call
    }

    // a Resizability describes a bunch of UIElements
    // It tells how well the UIElements can actually use any blank space, and also how many pieces the blank space will be split into
    class Resizability
    {
        public Resizability(int priority, double weight)
        {
            this.Priority = priority;
            this.Weight = weight;
        }

        // any Resizability of a higher priority will get all of the extra space
        public int Priority { get; set; }
        // any Resizability's of the same priority will be weighted by their weights
        public double Weight { get; set; }

        // adds two Resizability objects
        public Resizability Plus(Resizability other)
        {
            // if the priorities are the same, then add the weights
            if (this.Priority == other.Priority)
                return new Resizability(this.Priority, this.Weight + other.Weight);
            // if the priorities are different, then simply use one of higher priority
            return this.Max(other);
        }
        // returns the larger of two Resizability objects
        public Resizability Max(Resizability other)
        {
            if (this.Priority > other.Priority)
                return this;
            if (this.Priority < other.Priority)
                return other;
            if (this.Weight > other.Weight)
                return this;
            return other;
        }
        // division
        public double DividedBy(Resizability other)
        {
            if (this.Priority < other.Priority)
                return 0;
            if (other.Weight != 0)
                return this.Weight / other.Weight;
            return 0;
        }
    }

    class ResizableWrapper : Grid, IResizable
    {
        public ResizableWrapper()
        {
            this.Initialize();
        }
        public ResizableWrapper(FrameworkElement content, Resizability resizability)
        {
            this.Initialize();
            this.SetContent(content, resizability);
        }
        private void Initialize()
        {
            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.VerticalAlignment = VerticalAlignment.Stretch;
            //this.Background = System.Windows.Media.Brushes.Green;
            this.ColumnDefinitions.Add(new ColumnDefinition());
            this.RowDefinitions.Add(new RowDefinition());
        }

        public void SetContent(FrameworkElement content, Resizability resizability)
        {
            this.Children.Clear();
            this.convertedContents = content;
            if (this.convertedContents != null)
                this.Children.Add(this.convertedContents);
            this.horizontalResizability = this.verticalResizability = resizability;
        }
        protected override Size MeasureOverride(Size constraint)
        {
            this.convertedContents.Measure(constraint);
            return this.convertedContents.DesiredSize;
        }
        
        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            Rect rect = new Rect(new Point(), arrangeBounds);
            this.convertedContents.Arrange(rect);
            return arrangeBounds;
            //return arrangeBounds;
        }

        public virtual Size PreliminaryMeasure(Size constraint)
        {
            this.convertedContents.Measure(constraint);
            return this.convertedContents.DesiredSize;
        }
        public virtual Size FinalMeasure(Size finalSize)
        {
            this.Measure(finalSize);
            return this.DesiredSize;
        }

        public Resizability GetHorizontalResizability()
        {
            return this.horizontalResizability;
        }
        public Resizability GetVerticalResizability()
        {
            return this.verticalResizability;
        }
        private Resizability horizontalResizability;
        private Resizability verticalResizability;
        private UIElement convertedContents;
    }
}
