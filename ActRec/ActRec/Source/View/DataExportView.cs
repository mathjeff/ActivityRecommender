using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using VisiPlacement;
using Xamarin.Forms;


// Allows the user to export their data
namespace ActivityRecommendation
{
    class DataExportView : TitledControl
    {
        public DataExportView(ActivityRecommender activityRecommender, LayoutStack layoutStack)
        {
            this.SetTitle("Export Data");
            
            HelpWindowBuilder helper = new HelpWindowBuilder();
            helper.AddMessage("This this txt file contains most of what you've entered into this application, and so it may become large.");
            LayoutChoice_Set help = helper.Build();

            this.exportButton = new Button();
            this.exportButton.Clicked += ExportButton_Clicked;
            ButtonLayout buttonLayout = new ButtonLayout(this.exportButton, "Export");

            this.SetContent(new Vertical_GridLayout_Builder().AddLayout(help).AddLayout(buttonLayout).Build());

            this.activityRecommender = activityRecommender;
            this.layoutStack = layoutStack;
        }

        private async void ExportButton_Clicked(object sender, EventArgs e)
        {
            await this.activityRecommender.ExportData();
        }

        Button exportButton;
        ActivityRecommender activityRecommender;
        LayoutStack layoutStack;
    }
}
