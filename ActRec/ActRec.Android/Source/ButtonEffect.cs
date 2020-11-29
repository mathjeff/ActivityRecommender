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
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportEffect(typeof(ActRec.Droid.ButtonEffect), "ButtonEffect")]
namespace ActRec.Droid
{
    class ButtonEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            System.Diagnostics.Debug.WriteLine("Attaching " + this + " to element " + this.Element + " and control " + this.Control);
        }
        protected override void OnDetached()
        {

        }
    }
}