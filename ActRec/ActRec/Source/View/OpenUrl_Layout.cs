using System;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    class OpenUrl_Layout : ContainerLayout
    {
        public OpenUrl_Layout(string url, string text = null)
        {
            if (text == null)
                text = url;
            this.url = url;
            Button button = new Button();
            button.Clicked += Button_Clicked;
            this.SubLayout = new ButtonLayout(button, text);
        }

        private void Button_Clicked(object sender, System.EventArgs e)
        {
            visitUrl();
        }

        public void visitUrl()
        {
            Device.OpenUri(new Uri(this.url));
        }
        private string url;
    }

    // An OpenIssue_Layout displays an interface for the user to open an issue
    class OpenIssue_Layout
    {
        public static LayoutChoice_Set New(string buttonText = "Open an Issue")
        {
            return new OpenUrl_Layout("https://github.com/mathjeff/ActivityRecommender/issues", buttonText);
        }
    }
}
