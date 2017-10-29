using System;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation
{
    public class DataImportView : TitledControl
    {
        public DataImportView()
        {
            this.SetTitle("Import Data");

            this.importButton = new Button();
            ButtonLayout buttonLayout = new ButtonLayout(this.importButton, "Import and Overwrite");

            this.SetContent(buttonLayout);
        }


        public void Add_ClickHandler(EventHandler handler)
        {
            this.importButton.Clicked += handler;
        }


        private Button importButton;
    }
}
