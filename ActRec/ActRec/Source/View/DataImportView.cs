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
            FileData fileData = await this.publicFileIo.PromptUserForFile();
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
        private PublicFileIo publicFileIo = new PublicFileIo();

    }

    class ImportConfirmationView : TitledControl
    {

        public event RequestDataImport RequestImport;
        public delegate void RequestDataImport(object sender, FileData fileData);

        public ImportConfirmationView(FileData fileData)
        {
            this.fileData = fileData;

            Button button = new Button();
            button.Text = "Import " + fileData.FileName + " (" + fileData.DataArray.Length + " bytes)  and overwrite ALL existing data.";
            button.Clicked += Button_Clicked;

            this.SetTitle("Confirm Import");
            this.SetContent(new ButtonLayout(button));
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            this.RequestImport.Invoke(this, this.fileData);
        }

        private FileData fileData;
    }
}
