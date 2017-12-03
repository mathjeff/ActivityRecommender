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

            this.lineCount_box = new Editor();

            TextblockLayout numLines_label = new TextblockLayout("Max num lines to include (default all)");

            LayoutChoice_Set entryLayout = new Horizontal_GridLayout_Builder().AddLayout(numLines_label).AddLayout(new TextboxLayout(this.lineCount_box)).Build();

            this.lineCount_box.Keyboard = Keyboard.Numeric;
            this.lineCount_box.TextChanged += lineCount_box_TextChanged;

            this.SetContent(new Vertical_GridLayout_Builder().AddLayout(help).AddLayout(entryLayout).AddLayout(buttonLayout).Build());

            this.activityRecommender = activityRecommender;
            this.layoutStack = layoutStack;
        }

        private void ExportButton_Clicked(object sender, EventArgs e)
        {
            string result = this.activityRecommender.ExportData();
            this.layoutStack.AddLayout(new TextblockLayout(result));
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
                backgroundColor = Color.LightGray;
            }
            else
            {
                backgroundColor = Color.Red;
            }
            this.lineCount_box.BackgroundColor = backgroundColor;
        }

        Button exportButton;
        Editor lineCount_box;
        ActivityRecommender activityRecommender;
        LayoutStack layoutStack;
    }
}
