﻿using ActivityRecommendation.View;
using System;
using VisiPlacement;
using Xamarin.Forms;

// The user is able to view statistics of their data
// This menu is used to allow the user to open that view
namespace ActivityRecommendation
{
    class ActivityVisualizationMenu : TitledControl
    {
        public ActivityVisualizationMenu(Engine engine, LayoutStack layoutStack)
        {
            this.SetTitle("View Statistics");
            this.engine = engine;
            this.layoutStack = layoutStack;

            Vertical_GridLayout_Builder gridBuilder = new Vertical_GridLayout_Builder().Uniform();

            this.yAxisNameBox = new ActivityNameEntryBox("Activity", engine.ActivityDatabase, layoutStack);
            gridBuilder.AddLayout(this.yAxisNameBox);

            this.okButton = new Button();
            this.okButton.Clicked += OkButton_Clicked;

            gridBuilder.AddLayout(new ButtonLayout(this.okButton, "Visualize"));

            this.SetContent(gridBuilder.Build());
        }

        private void OkButton_Clicked(object sender, EventArgs e)
        {
            this.engine.EnsureRatingsAreAssociated();

            IProgression xAxisProgression = this.XAxisProgression;
            ActivityDescriptor yAxisDescriptor = this.YAxisActivityDescriptor;
            Activity yAxisActivity = null;
            if (yAxisDescriptor != null)
            {
                yAxisActivity = this.engine.ActivityDatabase.ResolveDescriptor(yAxisDescriptor);
            }
            if (yAxisActivity != null)
            {
                yAxisActivity.ApplyPendingData();
                ActivityVisualizationView visualizationView = new ActivityVisualizationView(xAxisProgression, yAxisActivity, this.engine.RatingSummarizer, this.engine.EfficiencySummarizer, this.layoutStack);
                this.layoutStack.AddLayout(visualizationView, "Graph");
            }
        }

        public IProgression XAxisProgression
        {
            get
            {
                return TimeProgression.AbsoluteTime;
            }
        }
        public ActivityDescriptor YAxisActivityDescriptor
        {
            get
            {
                return this.yAxisNameBox.ActivityDescriptor;
            }
        }
        Button okButton;
        ActivityNameEntryBox yAxisNameBox;
        Engine engine;
        LayoutStack layoutStack;
    }
}
