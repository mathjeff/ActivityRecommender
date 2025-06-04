using Xamarin.Forms.Platform.Android;
using Microsoft.Maui.Controls;


[assembly: ResolutionGroupName("VisiPlacement")]
[assembly: ExportEffect(typeof(ActRec.Droid.TextItemEffect), "TextItemEffect")]
namespace ActRec.Droid
{
    public class TextItemEffect : PlatformEffect
    {
        public TextItemEffect()
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