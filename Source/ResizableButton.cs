using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace ActivityRecommendation
{
    class ResizableButton : Button
    {
        public ResizableButton()
        {
            this.Margin = new Thickness();
            this.Padding = new Thickness();
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
            // In case this is the first time the background has changed, keep track of what the default background is
            this.defaultBackground = this.GetDefaultBackground();

            // now actually change the current background
            this.Background = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
        }
        public void UnHighlight()
        {
            this.SetDefaultBackground();
        }
        private Brush defaultBackground;
    }
}
