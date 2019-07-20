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
        public DataExportView(ActivityRecommender activityRecommender, Persona persona, LayoutStack layoutStack)
        {
            this.SetTitle("Export Data");

            this.persona = persona;
            

            this.exportButton = new Button();
            this.exportButton.Clicked += ExportButton_Clicked;

            this.activityRecommender = activityRecommender;
            this.layoutStack = layoutStack;

            this.SetupView();
            this.persona.NameChanged += Persona_NameChanged;
        }

        private void Persona_NameChanged(string newName)
        {
            this.SetupView();
        }

        private void SetupView()
        {

            HelpWindowBuilder helper = new HelpWindowBuilder();
            helper.AddMessage("This this txt file contains most of what you've provided to " + this.persona.Name + ", and so it may become large.");
            LayoutChoice_Set help = helper.Build();
            ButtonLayout buttonLayout = new ButtonLayout(this.exportButton, "Export");

            this.SetContent(new Vertical_GridLayout_Builder().AddLayout(help).AddLayout(buttonLayout).Build());
        }

        private async void ExportButton_Clicked(object sender, EventArgs e)
        {
            await this.activityRecommender.ExportData();
        }

        Button exportButton;
        ActivityRecommender activityRecommender;
        Persona persona;
        LayoutStack layoutStack;
    }
}
