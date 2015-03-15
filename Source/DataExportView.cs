using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using VisiPlacement;

// Allows the user to export their data
namespace ActivityRecommendation.Source
{
    class DataExportView : TitledControl
    {
        public DataExportView()
        {
            this.SetTitle("Export Data");
            
            HelpWindowBuilder helper = new HelpWindowBuilder();
            helper.AddMessage("Warning: this txt file contains most of what you've entered into this application, and so it may become large");
            helper.AddMessage("After you export this file and it opens in another program, it is probably easiest for you to simply save the file, close that program, and transfer it to a more powerful device, rather than trying to read it inside that program.");
            LayoutChoice_Set help = helper.Build();

            this.exportButton = new Button();
            ButtonLayout buttonLayout = new ButtonLayout(this.exportButton, "Export");

            GridLayout grid = GridLayout.New(BoundProperty_List.Uniform(2), BoundProperty_List.Uniform(1), LayoutScore.Zero);
            grid.AddLayout(help);
            grid.AddLayout(buttonLayout);

            this.SetContent(grid);
        }

        public void Add_ClickHandler(RoutedEventHandler handler)
        {
            this.exportButton.Click += handler;
        }

        Button exportButton;
    }
}
