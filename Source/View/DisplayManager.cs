/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

// The Measure/Arrange system in WPF is acceptable but not as maintainable as it should be
// The DisplayManager allows easier placement and resizing of controls
namespace ActivityRecommendation
{
    class DisplayManager : Canvas
    {
        public DisplayManager(Window window)
        {
            this.mainWindow = window;
            window.Content = this;
        }
        public void SetContent(UIElement newContent)
        {
            if (this.content != null)
                this.Children.Remove(this.content);
            this.content = newContent;
            if (this.content != null)
                this.Children.Add(newContent);
        }
        protected override Size MeasureOverride(Size constraint)
        {
            IResizable converted = this.content as IResizable;
            //converted.PreliminaryMeasure(constraint);
            return converted.FinalMeasure(constraint);
        }
        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            //Size newBounds = this.DesiredSize;
            this.content.Arrange(new Rect(new Point(0, 0), arrangeBounds));
            return arrangeBounds;
        }
        private Window mainWindow;
        private UIElement content;
    }
}
*/