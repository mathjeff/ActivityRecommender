using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace ActivityRecommendation
{
    class ResizableRadioButton : RadioButton, IResizable
    {
        public ResizableRadioButton()
        {
        }
        public Size PreliminaryMeasure(Size constraint)
        {
            base.Measure(new Size(constraint.Width, double.PositiveInfinity));
            return this.DesiredSize;
        }
        public Size FinalMeasure(Size constraint)
        {
            this.Measure(constraint);
            return constraint;
        }
        public Resizability GetHorizontalResizability()
        {
            return new Resizability(0, 1);
        }
        public Resizability GetVerticalResizability()
        {
            return new Resizability(0, 1);
        }
    }
}
