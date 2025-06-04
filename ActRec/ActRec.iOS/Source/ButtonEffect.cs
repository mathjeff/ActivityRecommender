using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Platform;

[assembly: ResolutionGroupName("ActRec")]
[assembly: ExportEffect(typeof(ActRec.iOS.ButtonEffect), "ButtonEffect")]
namespace ActRec.iOS
{
    class ButtonEffect : PlatformEffect
    {
        public ButtonEffect()
        {
        }

        protected override void OnAttached()
        {
            UIView view = this.Control;
            UIButton button = view as UIButton;
            button.TitleLabel.TextAlignment = UITextAlignment.Center;
            button.TitleLabel.LineBreakMode = UILineBreakMode.WordWrap;
            //button.HorizontalAlignment = UIControlContentHorizontalAlignment.Center;
            //button.VerticalAlignment = UIControlContentVerticalAlignment.Fill;

            // TODO: figure out how to disable the button tint, and then make this image bright green like Android's button background
            button.SetBackgroundImage(UIImage.FromBundle("button_background.png"), UIControlState.Highlighted);
        }

        protected override void OnDetached()
        {
        }

    }
}