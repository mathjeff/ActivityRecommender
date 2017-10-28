using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using Xamarin.Forms;

namespace ActivityRecommendation
{
    class ResizableButton : Button
    {
        public ResizableButton()
        {
            this.Margin = new Thickness();
            //this.Padding = new Thickness();
        }
        public void SetDefaultBackground()
        {
            this.BackgroundColor = this.GetDefaultBackground();
        }
        public Color GetDefaultBackground()
        {
            if (this.defaultBackground == null)
                this.defaultBackground = this.BackgroundColor;
            return this.defaultBackground;
        }
        public void Highlight()
        {
            // In case this is the first time the background has changed, keep track of what the default background is
            this.defaultBackground = this.GetDefaultBackground();

            // now actually change the current background
            this.BackgroundColor = Color.FromRgb(255, 0, 255);
        }
        public void UnHighlight()
        {
            this.SetDefaultBackground();
        }
        private Color defaultBackground;
    }
}
