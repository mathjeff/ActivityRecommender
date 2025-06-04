using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;
using Microsoft.Maui.Controls;

namespace ActivityRecommendation.View
{
    class ExportSuccessLayout : ContainerLayout
    {
        public ExportSuccessLayout(string filePath, PublicFileIo fileIo)
        {
            this.fileIo = fileIo;

            GridLayout grid = GridLayout.New(new BoundProperty_List(2), new BoundProperty_List(1), LayoutScore.Zero);
            TextblockLayout title = new TextblockLayout("Shared " + filePath, 16, false, true);
            grid.AddLayout(title);

            this.SubLayout = grid;
        }

        private PublicFileIo fileIo;
    }
}
