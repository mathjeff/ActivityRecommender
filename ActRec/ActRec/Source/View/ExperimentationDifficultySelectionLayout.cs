using ActivityRecommendation.Effectiveness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    // An ExperimentationDifficultySelectionLayout asks the user for some information about the relative difficulties of several activities.
    // This is supposed to happen after the user has identified several plausible activities to include in an experiment but before one specific activity has been chosen to do now.
    // The user may also specify which specific metrics they want to use to measure their participations.
    class ExperimentationDifficultySelectionLayout : ContainerLayout, LayoutProvider<ExperimentDifficulty_ListItem>
    {
        public event RequestedExperimentHandler Done;
        public delegate void RequestedExperimentHandler(List<SuggestedMetric> choices);

        public ExperimentationDifficultySelectionLayout(List<SuggestedMetric> suggestions, ActivityDatabase activityDatabase)
        {
            int requiredNumChoices = 3;
            if (suggestions.Count != requiredNumChoices)
                throw new ArgumentException("ExperimentationDifficultySelectionLayout unsupported number of choices: got " + suggestions.Count + ", expected " + requiredNumChoices);

            List<ExperimentDifficulty_ListItem> difficultyOptions = new List<ExperimentDifficulty_ListItem>();
            this.aPlusB_listItem = new ExperimentDifficulty_TextItem("Two times the difficulty of the easiest activity listed above");

            difficultyOptions.Add(this.newItem(suggestions[0], activityDatabase));
            difficultyOptions.Add(this.newItem(suggestions[1], activityDatabase));
            difficultyOptions.Add(this.aPlusB_listItem);
            difficultyOptions.Add(this.newItem(suggestions[2], activityDatabase));

            string instructions = "Rearrange these tasks so they appear in order by increasing difficulty. Also, you may change which metric you would like to use to measure your efficiency.";

            Button okButton = new Button();
            okButton.Clicked += OkButton_Clicked;
            this.okButtonLayout = new ButtonLayout(okButton, "Accept");
            this.invalidOrder_layout = new TextblockLayout("Illegal ordering! Two times the difficulty of the easiest activity must be more than its difficulty!");
            this.okButtonHolder = new ContainerLayout();

            this.choicesLayout = new ReorderableList<ExperimentDifficulty_ListItem>(difficultyOptions, this);
            this.choicesLayout.Reordered += ChoicesLayout_Reordered;


            BoundProperty_List rowHeights = new BoundProperty_List(5);
            rowHeights.BindIndices(1, 3); // Make the size of the Easiest text block match the size of the Hardest text block
            GridLayout gridLayout = GridLayout.New(rowHeights, new BoundProperty_List(1), LayoutScore.Zero);

            gridLayout.AddLayout(new TextblockLayout(instructions));
            gridLayout.AddLayout(new TextblockLayout("Easiest", 16).AlignHorizontally(TextAlignment.Center));
            gridLayout.AddLayout(this.choicesLayout);
            gridLayout.AddLayout(new TextblockLayout("Hardest", 16).AlignHorizontally(TextAlignment.Center));
            gridLayout.AddLayout(this.okButtonHolder);

            this.updateValidity(this.choicesLayout.Items);

            this.SubLayout = gridLayout;
        }

        private ExperimentDifficulty_SuggestedMetric newItem(SuggestedMetric metric, ActivityDatabase activityDatabase)
        {
            Activity activity = activityDatabase.ResolveDescriptor(metric.ActivityDescriptor);
            return new ExperimentDifficulty_SuggestedMetric(metric, activity);
        }

        private void ChoicesLayout_Reordered(List<ExperimentDifficulty_ListItem> choices)
        {
            this.updateValidity(choices);
        }
        private void updateValidity(List<ExperimentDifficulty_ListItem> choices)
        {
            if (this.isValidOrder(choices))
            {
                this.aPlusB_listItem.TextblockLayout.setBackgroundColor(Color.Black);
                this.okButtonHolder.SubLayout = this.okButtonLayout;
            }
            else
            {
                this.aPlusB_listItem.TextblockLayout.setBackgroundColor(Color.Red);
                this.okButtonHolder.SubLayout = this.invalidOrder_layout;
            }

        }
        private bool isValidOrder(List<ExperimentDifficulty_ListItem> choices)
        {
            if (choices[0] == this.aPlusB_listItem)
                return false;
            return true;
        }

        private void OkButton_Clicked(object sender, EventArgs e)
        {
            this.submit(this.choicesLayout.Items);
        }

        private void submit(List<ExperimentDifficulty_ListItem> reorderedItems)
        {
            if (!this.isValidOrder(reorderedItems))
                return;
            List<SuggestedMetric> results = new List<SuggestedMetric>();
            for (int i = 0; i < reorderedItems.Count; i++)
            {
                ExperimentDifficulty_ListItem item = reorderedItems[i];
                ExperimentDifficulty_SuggestedMetric suggestionItem = item as ExperimentDifficulty_SuggestedMetric;
                if (suggestionItem != null)
                {
                    SuggestedMetric suggestedMetric = suggestionItem.SuggestedMetric;
                    suggestedMetric.PlannedMetric.DifficultyEstimate.NumEasiers = i;
                    suggestedMetric.PlannedMetric.DifficultyEstimate.NumHarders = reorderedItems.Count - 1 - i;
                    suggestedMetric.PlannedMetric.MetricName = suggestionItem.MetricChooser.Metric.Name;

                    results.Add(suggestedMetric);
                }
            }
            int requiredNumChoices = 3;
            if (results.Count != requiredNumChoices)
                throw new ArgumentException("ExperimentationDifficultySelectionLayout unsupported number of choices: got " + results.Count + ", expected " + requiredNumChoices);
            this.Done.Invoke(results);
        }


        public LayoutChoice_Set GetLayout(ExperimentDifficulty_ListItem item)
        {
            ExperimentDifficulty_TextItem textItem = item as ExperimentDifficulty_TextItem;
            if (textItem != null)
                return textItem;

            ExperimentDifficulty_SuggestedMetric metricItem = item as ExperimentDifficulty_SuggestedMetric;

            BoundProperty_List columnWidths = new BoundProperty_List(2);
            columnWidths.BindIndices(0, 1);
            columnWidths.SetPropertyScale(0, 5);
            columnWidths.SetPropertyScale(1, 2);
            GridLayout gridLayout = GridLayout.New(new BoundProperty_List(1), columnWidths, LayoutScore.Zero);
            gridLayout.AddLayout(new TextblockLayout(metricItem.SuggestedMetric.ActivityDescriptor.ActivityName));
            ChooseMetric_View chooser = metricItem.MetricChooser;

            gridLayout.AddLayout(chooser);
            return gridLayout;

        }

        private ReorderableList<ExperimentDifficulty_ListItem> choicesLayout;
        private ExperimentDifficulty_TextItem aPlusB_listItem;
        private ButtonLayout okButtonLayout;
        private TextblockLayout invalidOrder_layout;
        private ContainerLayout okButtonHolder;

    }

    interface ExperimentDifficulty_ListItem
    {
    }

    class ExperimentDifficulty_SuggestedMetric : ExperimentDifficulty_ListItem
    {
        public ExperimentDifficulty_SuggestedMetric(SuggestedMetric suggestedMetric, Activity activity)
        {
            this.SuggestedMetric = suggestedMetric;
            this.MetricChooser = new ChooseMetric_View();
            this.MetricChooser.SetActivity(activity);
            this.MetricChooser.Choose(suggestedMetric.PlannedMetric.MetricName);
        }
        public SuggestedMetric SuggestedMetric { get; set; }
        public ChooseMetric_View MetricChooser { get; set; }
    }

    class ExperimentDifficulty_TextItem : ContainerLayout, ExperimentDifficulty_ListItem
    {
        public ExperimentDifficulty_TextItem(string text)
        {
            this.TextblockLayout = new TextblockLayout(text);
            this.SubLayout = this.TextblockLayout;
        }
        public TextblockLayout TextblockLayout;
    }

}
