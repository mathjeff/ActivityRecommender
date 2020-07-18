using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ResolutionGroupName("VisiPlacement")]
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
            button.LineBreakMode = UILineBreakMode.WordWrap;
            button.TitleLabel.TextAlignment = UITextAlignment.Center;

            // TODO: figure out how to disable the button tint, and then make this image bright green like Android's button background
            button.SetBackgroundImage(UIImage.FromBundle("button_background.png"), UIControlState.Highlighted);
        }

        protected override void OnDetached()
        {
        }

    }
}