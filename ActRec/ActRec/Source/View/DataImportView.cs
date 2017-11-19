using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using System;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation
{
    public class DataImportView : TitledControl
    {
        public event RequestDataImport RequestImport;
        public delegate void RequestDataImport(object sender, FileData fileData);

        public DataImportView(LayoutStack layoutStack)
        {
            this.SetTitle("Import Data");
            this.layoutStack = layoutStack;

            this.importButton = new Button();
            this.importButton.Clicked += ChooseFile;
            ButtonLayout buttonLayout = new ButtonLayout(this.importButton, "Import and Overwrite");
            

            this.SetContent(buttonLayout);
        }

        private async void ChooseFile(object sender, EventArgs e)
        {
            IFilePicker filePicker = CrossFilePicker.Current;
            FileData fileData = await filePicker.PickFile();
            if (fileData == null)
                return; // cancelled

            ImportConfirmationView confirmationView = new ImportConfirmationView(fileData);
            this.layoutStack.AddLayout(confirmationView);
            confirmationView.RequestImport += ConfirmationView_RequestImport;
        }

        private void ConfirmationView_RequestImport(object sender, FileData fileData)
        {
            this.RequestImport.Invoke(sender, fileData);
        }

        private Button importButton;
        private LayoutStack layoutStack;
    }

    class ImportConfirmationView : TitledControl
    {

        public event RequestDataImport RequestImport;
        public delegate void RequestDataImport(object sender, FileData fileData);

        public ImportConfirmationView(FileData fileData)
        {
            Button button = new Button();
            button.Clicked += Button_Clicked1; ;

            button.Text = "Import " + fileData.FileName + " and overwrite ALL existing data.";

            this.fileData = fileData;

            this.SetTitle("Confirm Import");
            this.SetContent(new ButtonLayout(button));
        }

        private void Button_Clicked1(object sender, EventArgs e)
        {
            this.RequestImport.Invoke(this, this.fileData);
        }

        private FileData fileData;

    }
}
