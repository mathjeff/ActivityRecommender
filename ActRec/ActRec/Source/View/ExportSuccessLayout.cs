using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
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
