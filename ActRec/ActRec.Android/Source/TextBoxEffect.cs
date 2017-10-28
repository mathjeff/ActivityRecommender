using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms;


[assembly: ResolutionGroupName("VisiPlacement")]
[assembly: ExportEffect(typeof(ActRec.Droid.TextBoxEffect), "TextBoxEffect")]
namespace ActRec.Droid
{
    public class TextBoxEffect : PlatformEffect
    {
        public TextBoxEffect()
        {
            this.Padding = new Thickness();
        }

        protected override void OnAttached()
        {
            this.Control.SetPadding((int)this.Padding.Left, (int)this.Padding.Top, (int)this.Padding.Right, (int)this.Padding.Bottom);
        }

        protected override void OnDetached()
        {

        }

        public Thickness Padding;
    }
}