using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ResolutionGroupName("VisiPlacement")]
[assembly: ExportEffect(typeof(ActRec.UWP.ButtonEffect), "ButtonEffect")]
namespace ActRec.UWP
{
    class ButtonEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            Button button = this.Element as Button;
            button.BorderWidth = 0;
            // We want padding of 0, but setting padding of 0 doesn't work (perhaps something overrides it later)
            // If we set a padding of 1, though, that seems to work better, at least
            button.Padding = 1;
            button.Margin = 0;
            //System.Diagnostics.Debug.WriteLine("Attaching " + this + " to element " + this.Element + " and control " + this.Control);
        }
        protected override void OnDetached()
        {

        }
    }
}
