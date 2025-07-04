﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VisiPlacement;
using Microsoft.Maui.Controls;

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
                .AddLayout(new TextblockLayout("Your activities sorted by how much you have liked them.\n" +
                "This may take some time."))
                .AddLayout(new ButtonLayout(okButton, "Sort!"))
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

            GridLayout_Builder gridBuilder = new Vertical_GridLayout_Builder().Uniform();
            foreach (Activity activity in activities)
            {
                string text = "" + activity.Ratings.Mean + " (" + activity.Ratings.Weight + ") : " + activity.Name;
                texts.Add(text);
            }
            string fileText = String.Join("\n", texts);
            DateTime now = DateTime.Now;
            string nowText = now.ToString("yyyy-MM-dd-HH-mm-ss");
            string fileName = "ActivityPrefSummary-" + nowText + ".txt";

            Task t = this.fileIo.Share(fileName, fileText);

            t.ContinueWith(task =>
            {
                string title = "Exported " + fileName;
                gridBuilder.AddLayout(new TextblockLayout("(" + title + ")"));
                gridBuilder.AddLayout(new TextblockLayout(" avg (count) : name"));
                foreach (string text in texts)
                {
                    gridBuilder.AddLayout(new TextblockLayout(text));
                }

                LayoutChoice_Set newLayout = ScrollLayout.New(gridBuilder.Build());
                this.layoutStack.AddLayout(newLayout, "Preferences");
            });

        }

        private Engine engine;
        private LayoutStack layoutStack;
        private PublicFileIo fileIo;
    }
}
