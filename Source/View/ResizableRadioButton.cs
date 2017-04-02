/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace ActivityRecommendation
{
    class ResizableRadioButton : RadioButton
    {
        public ResizableRadioButton()
        {
        }
        public Size PreliminaryMeasure(Size constraint)
        {
            IResizable convertedContent = this.Content as IResizable;
            Size newConstraint = new Size(constraint.Width, double.PositiveInfinity);
            if (convertedContent != null)
            {
                return convertedContent.PreliminaryMeasure(newConstraint);
            }
            else
            {
                base.Measure(newConstraint);
            }
            return this.DesiredSize;
        }
        public Size FinalMeasure(Size constraint)
        {
            this.Measure(constraint);
            return constraint;
        }
        public Resizability GetHorizontalResizability()
        {
            IResizable convertedContent = this.Content as IResizable;
            if (convertedContent != null)
                return convertedContent.GetHorizontalResizability();
            return new Resizability(0, 1);
        }
        public Resizability GetVerticalResizability()
        {
            IResizable convertedContent = this.Content as IResizable;
            if (convertedContent != null)
                return convertedContent.GetVerticalResizability();
            return new Resizability(0, 1);
        }
    }
}

*/