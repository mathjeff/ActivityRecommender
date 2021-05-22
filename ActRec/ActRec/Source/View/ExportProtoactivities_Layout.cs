using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    class ExportProtoactivities_Layout : ContainerLayout
    {
        public ExportProtoactivities_Layout(ProtoActivity_Database protoActivity_database, PublicFileIo fileIo, TextConverter textConverter, LayoutStack layoutStack)
        {
            this.protoActivity_database = protoActivity_database;
            this.fileIo = fileIo;
            this.textConverter = textConverter;
            this.layoutStack = layoutStack;
        }

        public List<AppFeature> GetFeatures()
        {
            return new List<AppFeature>() { new ExportProtoactivities_Feature(this.protoActivity_database) };
        }
        private LayoutChoice_Set makeSublayout()
        {
            TextblockLayout title = new TextblockLayout("Export Protoactivities");
            TextblockLayout subtitle = new TextblockLayout("This feature exports a file containing all of your protoactivies. You can send them to your friends!");

            Button button = new Button();
            button.Clicked += Button_Clicked;
            ButtonLayout buttonLayout = new ButtonLayout(button, "Export");

            GridLayout_Builder builder = new Vertical_GridLayout_Builder()
                .AddLayout(title)
                .AddLayout(subtitle)
                .AddLayout(buttonLayout);

            return builder.BuildAnyLayout();
        }

        public override SpecificLayout GetBestLayout(LayoutQuery query)
        {
            if (this.SubLayout == null)
                this.SubLayout = this.makeSublayout();

            return base.GetBestLayout(query);
        }


        private void Button_Clicked(object sender, EventArgs e)
        {
            this.export();
        }

        private async void export()
        {
            string filename = "Protoactivities-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".txt";
            string text = this.textConverter.ConvertToString(this.protoActivity_database, true);
            await this.fileIo.Share(filename, text);
            this.layoutStack.AddLayout(new ExportSuccessLayout("file", this.fileIo), "Success");
        }

        private ProtoActivity_Database protoActivity_database;
        private PublicFileIo fileIo;
        private TextConverter textConverter;
        private LayoutStack layoutStack;
    }

    class ExportProtoactivities_Feature : AppFeature
    {
        public ExportProtoactivities_Feature(ProtoActivity_Database protoactivityDatabase)
        {
            this.protoactivityDatabase = protoactivityDatabase;
        }
        public string GetDescription()
        {
            return "Export ProtoActivities";
        }
        public bool GetHasBeenUsed()
        {
            // We don't actually keep track of whether this feature has been used.
            // We mostly just want it to be extra obvious that it doesn't make sense to use it when there are no protoactivities.
            // If this feature is usable then we assume it has been used.
            return this.GetIsUsable();
        }

        public bool GetIsUsable()
        {
            return this.protoactivityDatabase.Count >= 1;
        }

        ProtoActivity_Database protoactivityDatabase;
    }

}
