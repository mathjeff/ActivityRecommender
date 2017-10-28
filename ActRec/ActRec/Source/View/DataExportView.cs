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
        public DataExportView()
        {
            this.SetTitle("Export Data");
            
            HelpWindowBuilder helper = new HelpWindowBuilder();
            helper.AddMessage("Warning: this txt file contains most of what you've entered into this application, and so it may become large.");
            helper.AddMessage("After you export this file and it opens in another program, it is probably easiest for you to simply save the file, close that program, and transfer it to a more powerful device, rather than trying to read it inside that program.");
            LayoutChoice_Set help = helper.Build();

            this.exportButton = new Button();
            ButtonLayout buttonLayout = new ButtonLayout(this.exportButton, "Export");

            this.lineCount_box = new Editor();

            TextblockLayout numLines_label = new TextblockLayout("Max num lines to include (default all)");
            LayoutChoice_Set entryLayout = new Horizontal_GridLayout_Builder().AddLayout(numLines_label).AddLayout(new TextboxLayout(this.lineCount_box)).Build();

            //this.lineCount_box.InputScope = InputScopeUtils.Numeric;
            this.lineCount_box.TextChanged += lineCount_box_TextChanged;

            this.SetContent(new Vertical_GridLayout_Builder().AddLayout(help).AddLayout(entryLayout).AddLayout(buttonLayout).Build());
        }

        public void Add_ClickHandler(EventHandler handler)
        {
            this.exportButton.Clicked += handler;
        }

        public int Get_NumLines()
        {
            string text = this.lineCount_box.Text;
            if (text == "")
            {
                return 0;
            }
            long numLines = -1;
            if (!Int64.TryParse(text, out numLines)) {
                numLines = -1;
            }
            return (int)numLines;
        }

        private void lineCount_box_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.update_linecountBox_appearance();
        }

        private void update_linecountBox_appearance()
        {
            Color backgroundColor;
            if (this.Get_NumLines() >= 0)
            {
                backgroundColor = Color.White;
            }
            else
            {
                backgroundColor = Color.Red;
            }
            this.lineCount_box.BackgroundColor = backgroundColor;
        }

        Button exportButton;
        Editor lineCount_box;
    }
}
