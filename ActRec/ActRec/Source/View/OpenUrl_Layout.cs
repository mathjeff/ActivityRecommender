using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using System;
using VisiPlacement;

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

        public async void visitUrl()
        {
            try
            {
                Uri uri = new Uri(this.url);
                await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                // An unexpected error occurred. No browser may be installed on the device.
            }
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
