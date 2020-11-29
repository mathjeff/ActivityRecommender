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
            System.Diagnostics.Debug.WriteLine("Attaching " + this + " to element " + this.Element + " and control " + this.Control);
        }
        protected override void OnDetached()
        {

        }
    }
}
