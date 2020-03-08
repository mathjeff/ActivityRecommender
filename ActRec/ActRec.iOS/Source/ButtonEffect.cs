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
        }

        protected override void OnDetached()
        {
        }

    }
}