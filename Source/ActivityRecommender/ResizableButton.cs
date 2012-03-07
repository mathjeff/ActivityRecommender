using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace ActivityRecommendation
{
    class ResizableButton : Button, IResizable
    {
        public ResizableButton()
        {
        }
        public Size PreliminaryMeasure(Size size)
        {
            base.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            return base.DesiredSize;
        }
        public Size FinalMeasure(Size size)
        {
            this.Measure(size);
            return size;
        }
        public Resizability GetHorizontalResizability()
        {
            return new Resizability(1, 1);
        }
        public Resizability GetVerticalResizability()
        {
            return new Resizability(1, 1);
        }
        public void SetDefaultBackground()
        {
            this.Background = this.GetDefaultBackground();
        }
        public Brush GetDefaultBackground()
        {
            if (this.defaultBackground == null)
                this.defaultBackground = this.Background;
            return this.defaultBackground;
        }
        public void Highlight()
        {
            this.Background = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
        }
        public void UnHighlight()
        {
            this.SetDefaultBackground();
        }
        private Brush defaultBackground;
    }
}
