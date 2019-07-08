using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    class PreferenceSummaryLayout : ContainerLayout
    {
        public PreferenceSummaryLayout(Engine engine, LayoutStack layoutStack, PublicFileIo fileIo)
        {
            this.engine = engine;
            this.layoutStack = layoutStack;
            this.fileIo = fileIo;

            Button okButton = new Button();
            okButton.Clicked += OkButton_Clicked;


            this.SubLayout = new Vertical_GridLayout_Builder()
                .Uniform()
                .AddLayout(new TextblockLayout("This screen analyzes your data and summarizes the results, for sharing with other people.\n" +
                "Currently, that means it sorts the activities you've done by their average rating, and lists them all.\n" +
                "This isn't intended for you to use to get a suggestion of what you should do now; for that, you should go to the Suggestions screen,\n" +
                "which also takes into account how long it has been since you've done various things and how they appear to affect your longterm happiness.\n" +
                "These results will be shown onscreen and saved in a file.\n" +
                "This may take some time."))
                .AddLayout(new ButtonLayout(okButton, "Summarize!"))
                .Build();
        }

        private void OkButton_Clicked(object sender, System.EventArgs e)
        {
            this.calculate();
        }

        private void calculate()
        {
            List<Activity> activities = this.engine.ActivitiesSortedByAverageRating;
            List<string> texts = new List<string>();

            Vertical_GridLayout_Builder gridBuilder = new Vertical_GridLayout_Builder().Uniform();
            foreach (Activity activity in activities)
            {
                string text = "" + activity.Ratings.Mean + " (" + activity.Ratings.Weight + ") : " + activity.Name;
                texts.Add(text);
            }
            string fileText = String.Join("\n", texts);
            DateTime now = DateTime.Now;
            string nowText = now.ToString("yyyy-MM-dd-HH-mm-ss");
            string fileName = "ActivityPrefSummary-" + nowText + ".txt";

            Task<bool> export = this.fileIo.ExportFile(fileName, fileText);

            export.ContinueWith(task =>
            {
                string title;
                if (task.Result)
                    title = "Exported " + fileName + " successfully";
                else
                    title = "Failed to export " + fileName;
                gridBuilder.AddLayout(new TextblockLayout("(" + title + ")"));
                gridBuilder.AddLayout(new TextblockLayout(" avg (count) : name"));
                foreach (string text in texts)
                {
                    gridBuilder.AddLayout(new TextblockLayout(text));
                }

                LayoutChoice_Set newLayout = ScrollLayout.New(gridBuilder.Build());
                this.layoutStack.AddLayout(newLayout);
            });

        }

        private Engine engine;
        private LayoutStack layoutStack;
        private PublicFileIo fileIo;
    }
}
