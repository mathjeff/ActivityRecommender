using System;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    // An OpenIssue_Layout displays an interface for the user to open an issue
    class OpenIssue_Layout
    {
        public static LayoutChoice_Set New()
        {
            Button button = new Button();
            button.Clicked += Button_Clicked;
            return new ButtonLayout(button, "Open an Issue");
        }

        private static void Button_Clicked(object sender, System.EventArgs e)
        {
            askUserToOpenIssue();
        }

        public static void askUserToOpenIssue()
        {
            Device.OpenUri(new Uri("https://github.com/mathjeff/ActivityRecommender/issues"));
        }
    }
}
