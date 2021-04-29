using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation
{
    public class DataImportView : TitledControl
    {
        public event RequestDataImport RequestImport;
        public delegate void RequestDataImport(object sender, OpenedFile fileData);

        public DataImportView(LayoutStack layoutStack)
        {
            this.SetTitle("Import Data and Overwrite");
            this.layoutStack = layoutStack;
        }

        public override SpecificLayout GetBestLayout(LayoutQuery query)
        {
            if (this.GetContent() == null)
            {
                this.SetContent(this.makeContent());
            }
            return base.GetBestLayout(query);
        }
        private LayoutChoice_Set makeContent()
        {
            this.importLatest_button = new Button();
            this.importLatest_button.Clicked += ImportLatest_button_Clicked;
            ButtonLayout latestLayout = new ButtonLayout(this.importLatest_button, "Import latest");

            this.chooseFile_button = new Button();
            this.chooseFile_button.Clicked += ChooseFile;
            ButtonLayout chooseLayout = new ButtonLayout(this.chooseFile_button, "Select file");

            GridLayout_Builder builder = new Vertical_GridLayout_Builder()
                .Uniform()
                .AddLayout(new TextblockLayout("The file to import should be of the form created by the Export feature " +
                "(and the name of file should start with \"ActivityData\")"));
            builder.AddLayout(latestLayout);
            builder.AddLayout(chooseLayout);

            return builder.Build();
        }

        private async void ImportLatest_button_Clicked(object sender, EventArgs e)
        {
            string latestFilename = await this.getLatestFilename();
            if (latestFilename != null)
            {
                OpenedFile fileData = await this.publicFileIo.Open(latestFilename);
                this.selected(fileData);
            }
            else
            {
                this.layoutStack.AddLayout(new TextblockLayout("No data file found!"), "Import Error");
            }
        }

        private async void ChooseFile(object sender, EventArgs e)
        {
            OpenedFile fileData = await this.publicFileIo.PromptUserForFile();
            if (fileData == null)
                return; // cancelled
            this.selected(fileData);
        }

        private void selected(OpenedFile fileData)
        {
            ImportConfirmationView confirmationView = new ImportConfirmationView(fileData);
            this.layoutStack.AddLayout(confirmationView, "Confirm Import");
            confirmationView.RequestImport += ConfirmationView_RequestImport;
        }

        private async Task<string> getLatestFilename()
        {
            List<string> filenames = await this.publicFileIo.ListDir();
            string prefix = "ActivityData-";
            string suffix = ".txt";

            string latestFileName = null;
            string latestMiddle = null;
            foreach (string candidate in filenames)
            {
                string fileName = Path.GetFileName(candidate);
                if (fileName.StartsWith(prefix))
                {
                    string start = candidate.Remove(0, prefix.Length);
                    if (start.EndsWith(suffix))
                    {
                        string middle = start.Remove(start.Length - suffix.Length);
                        if (latestMiddle == null || middle.CompareTo(latestMiddle) > 0)
                        {
                            latestFileName = candidate;
                            latestMiddle = middle;
                        }
                    }
                }
            }
            return latestFileName;
        }

        private void ConfirmationView_RequestImport(object sender, OpenedFile fileData)
        {
            this.RequestImport.Invoke(sender, fileData);
        }

        private Button importLatest_button;
        private Button chooseFile_button;
        private LayoutStack layoutStack;
        private PublicFileIo publicFileIo = new PublicFileIo();

    }

    class ImportConfirmationView : TitledControl
    {

        public event RequestDataImport RequestImport;
        public delegate void RequestDataImport(object sender, OpenedFile fileData);

        public ImportConfirmationView(OpenedFile fileData)
        {
            this.fileData = fileData;

            Button button = new Button();
            string fileName = Path.GetFileName(fileData.Path);
            button.Text = "Import " + fileName + " (" + fileData.Content.Length + " bytes)  and overwrite ALL existing data.";
            button.Clicked += Button_Clicked;

            this.SetTitle("Confirm Import");
            this.SetContent(new ButtonLayout(button));
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            this.RequestImport.Invoke(this, this.fileData);
        }

        private OpenedFile fileData;
    }
}
