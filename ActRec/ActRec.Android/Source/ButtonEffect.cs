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
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;

[assembly: ExportEffect(typeof(ActRec.Droid.ButtonEffect), "ButtonEffect")]
namespace ActRec.Droid
{
    class ButtonEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            Android.Views.View view = this.Control;
            view.SetPadding(0, 0, 0, 0);
            view.SetBackgroundColor(new Android.Graphics.Color(0, 0, 0, 0));
            //System.Diagnostics.Debug.WriteLine("Attaching " + this + " to element " + this.Element + " and control " + this.Control);
        }
        protected override void OnDetached()
        {

        }
    }
}