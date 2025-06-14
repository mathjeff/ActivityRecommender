﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VisiPlacement;
using Microsoft.Maui.Controls;

namespace ActivityRecommendation.View
{
    class ParticipationComparisonMenu : TitledControl
    {
        public ParticipationComparisonMenu(LayoutStack layoutStack, ActivityDatabase activityDatabase, Engine engine)
        {
            this.layoutStack = layoutStack;
            this.activityDatabase = activityDatabase;
            this.engine = engine;

            this.SetTitle("Finding which activities often precede another");

            Vertical_GridLayout_Builder builder = new Vertical_GridLayout_Builder();

            this.activityToPredict_box = new ActivityNameEntryBox("Activity:", activityDatabase, layoutStack);
            builder.AddLayout(this.activityToPredict_box);

            builder.AddLayout(new TextblockLayout("Window duration:"));
            this.durationBox = new DurationEntryView();
            builder.AddLayout(this.durationBox);

            this.activityToPredictFrom_box = new ActivityNameEntryBox("Predictor Activity (default = all categories):", activityDatabase, layoutStack);
            builder.AddLayout(this.activityToPredictFrom_box);

            builder.AddLayout(new TextblockLayout("Comparison Type:"));
            this.typebox = new VisiPlacement.CheckBox("Linear Regression", "Bin Comparison");
            builder.AddLayout(this.typebox);

            this.okButton = new Button();
            this.okButton.Clicked += OkButton_Click;
            builder.AddLayout(new ButtonLayout(this.okButton, "Ok"));

            this.SetContent(builder.Build());
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            this.Enter();
        }
        private void Enter()
        {
            if (this.durationBox.IsDurationValid() && this.activityToPredict_box.Activity != null)
            {
                if (this.typebox.Checked)
                    this.layoutStack.AddLayout(new Participation_BinComparison_View(this.engine, this.ActivitiesToPredictFrom, this.activityToPredict_box.Activity, this.durationBox.GetDuration()), "Bin Comparison");
                else
                    this.layoutStack.AddLayout(new ParticipationCorrelationView(this.engine, this.ActivitiesToPredictFrom, this.activityToPredict_box.Activity, this.durationBox.GetDuration()), "Correlation");
            }
        }

        private IEnumerable<Activity> ActivitiesToPredictFrom
        {
            get
            {
                if (this.activityToPredictFrom_box.Activity != null)
                {
                    List<Activity> predictors = new List<Activity>();
                    predictors.Add(this.activityToPredictFrom_box.Activity);
                    return predictors;
                }
                return this.activityDatabase.AllCategories;
            }
        }

        private ActivityNameEntryBox activityToPredict_box;
        private ActivityNameEntryBox activityToPredictFrom_box;
        private DurationEntryView durationBox;
        private Button okButton;
        private LayoutStack layoutStack;
        private Engine engine;
        private ActivityDatabase activityDatabase;
        private VisiPlacement.CheckBox typebox;


    }
}
