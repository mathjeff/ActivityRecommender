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
    // An ExperimentationDifficultySelectionLayout asks the user for some information about the relative difficulties of several activities
    // This is supposed to happen after the user has identified several plausible activities to include in an experiment but before one specific activity has been chosen to do now
    class ExperimentationDifficultySelectionLayout : ContainerLayout, LayoutProvider<ExperimentDifficulty_ListItem>
    {
        public event RequestedExperimentHandler Done;
        public delegate void RequestedExperimentHandler(List<SuggestedMetric> choices);

        public ExperimentationDifficultySelectionLayout(List<SuggestedMetric> suggestions)
        {
            int requiredNumChoices = 3;
            if (suggestions.Count != requiredNumChoices)
                throw new ArgumentException("ExperimentationDifficultySelectionLayout unsupported number of choices: got " + suggestions.Count + ", expected " + requiredNumChoices);

            List<ExperimentDifficulty_ListItem> difficultyOptions = new List<ExperimentDifficulty_ListItem>();
            this.aPlusB_listItem = new ExperimentDifficulty_TextItem("Two times the difficulty of the easiest activity listed above");

            difficultyOptions.Add(new ExperimentDifficulty_SuggestedMetric(suggestions[0]));
            difficultyOptions.Add(new ExperimentDifficulty_SuggestedMetric(suggestions[1]));
            difficultyOptions.Add(this.aPlusB_listItem);
            difficultyOptions.Add(new ExperimentDifficulty_SuggestedMetric(suggestions[2]));

            string instructions = "Rearrange these tasks so they appear in order by increasing difficulty.";

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
                ExperimentDifficulty_SuggestedMetric suggestion = item as ExperimentDifficulty_SuggestedMetric;
                if (suggestion != null)
                {
                    SuggestedMetric suggestedMetric = suggestion.SuggestedMetric;
                    suggestedMetric.PlannedMetric.DifficultyEstimate.NumEasiers = i;
                    suggestedMetric.PlannedMetric.DifficultyEstimate.NumHarders = reorderedItems.Count - 1 - i;
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
            LayoutChoice_Set itemAsLayout = item as LayoutChoice_Set;
            if (itemAsLayout != null)
                return itemAsLayout;
            return SuggestedMetric_Renderer.Instance.GetLayout((item as ExperimentDifficulty_SuggestedMetric).SuggestedMetric);
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
        public ExperimentDifficulty_SuggestedMetric(SuggestedMetric suggestedMetric) { this.SuggestedMetric = suggestedMetric; }
        public SuggestedMetric SuggestedMetric { get; set; }
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
