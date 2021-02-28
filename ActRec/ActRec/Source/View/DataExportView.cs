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
            this.SetTitle("Export Your Data");

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
            LayoutChoice_Set instructions = new TextblockLayout("This this txt file contains most of what you've provided to " + this.persona.Name + ", and so it may become large.");
            ButtonLayout buttonLayout = new ButtonLayout(this.exportButton, "Export");
            LayoutChoice_Set credits = new CreditsButtonBuilder(this.layoutStack)
                .AddContribution(ActRecContributor.ANNI_ZHANG, new DateTime(2020, 04, 05), "Pointed out that exported data files could not be seen by users on iOS")
                .Build();

            this.SetContent(new Vertical_GridLayout_Builder().Uniform()
                .AddLayout(instructions)
                .AddLayout(buttonLayout)
                .AddLayout(credits)
                .Build());
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

    class ExportSuccessLayout : ContainerLayout
    {
        public ExportSuccessLayout(string filePath, PublicFileIo fileIo)
        {
            this.fileIo = fileIo;

            GridLayout grid = GridLayout.New(new BoundProperty_List(2), new BoundProperty_List(1), LayoutScore.Zero);
            TextblockLayout title = new TextblockLayout("Saved " + filePath, 16, false, true);
            grid.AddLayout(title);

            Button viewButton = new Button();
            ButtonLayout viewButtonLayout = new ButtonLayout(viewButton, "View files");
            grid.AddLayout(viewButtonLayout);
            viewButton.Clicked += ViewButton_Clicked;

            this.SubLayout = grid;
        }

        private async void ViewButton_Clicked(object sender, EventArgs e)
        {
            await this.fileIo.PromptUserForFile();
        }

        private PublicFileIo fileIo;
    }
}
