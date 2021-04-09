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
    class ExperimentationDifficultySelectionLayout : ContainerLayout
    {
        public event RequestedExperimentHandler Done;
        public delegate void RequestedExperimentHandler(List<SuggestedMetric> choices);

        public ExperimentationDifficultySelectionLayout(List<SuggestedMetric> suggestions, ActivityDatabase activityDatabase)
        {
            int requiredNumChoices = 3;
            if (suggestions.Count != requiredNumChoices)
                throw new ArgumentException("ExperimentationDifficultySelectionLayout unsupported number of choices: got " + suggestions.Count + ", expected " + requiredNumChoices);

            this.difficultyOptions = new List<ExperimentDifficultyEstimateLayout>();

            for (int i = 0; i < 3; i++)
            {
                this.difficultyOptions.Add(this.newItem(suggestions[i], activityDatabase));
            }

            string instructions = "Estimate the relative difficulty for each of these tasks. Also, you may change which metric you would like to use to measure your efficiency.";

            Button okButton = new Button();
            okButton.Clicked += OkButton_Clicked;
            this.okButtonLayout = new ButtonLayout(okButton, "Accept");
            this.invalidOrder_layout = new TextblockLayout("Illegal ordering! Each difficulty must be a positive number!");
            this.okButtonHolder = new ContainerLayout();


            GridLayout_Builder builder = new Vertical_GridLayout_Builder().Uniform();

            builder.AddLayout(new TextblockLayout(instructions));
            foreach (ExperimentDifficultyEstimateLayout option in this.difficultyOptions)
            {
                builder.AddLayout(option);
            }
            builder.AddLayout(this.okButtonHolder);

            this.updateValidity();

            this.SubLayout = builder.Build();
        }

        private ExperimentDifficultyEstimateLayout newItem(SuggestedMetric metric, ActivityDatabase activityDatabase)
        {
            Activity activity = activityDatabase.ResolveDescriptor(metric.ActivityDescriptor);
            ExperimentDifficultyEstimateLayout item = new ExperimentDifficultyEstimateLayout(metric, activity);
            item.DifficultyText_Changed += Item_DifficultyText_Changed;
            return item;
        }

        private void Item_DifficultyText_Changed()
        {
            this.updateValidity();
        }

        private void updateValidity()
        {
            if (this.isValidOrder(this.difficultyOptions))
            {
                this.okButtonHolder.SubLayout = this.okButtonLayout;
            }
            else
            {
                this.okButtonHolder.SubLayout = this.invalidOrder_layout;
            }

        }
        private bool isValidOrder(List<ExperimentDifficultyEstimateLayout> choices)
        {
            foreach (ExperimentDifficultyEstimateLayout choice in choices)
            {
                if (choice == null)
                    return false;
            }
            return true;
        }

        private void OkButton_Clicked(object sender, EventArgs e)
        {
            this.submit(this.difficultyOptions);
        }

        private void submit(List<ExperimentDifficultyEstimateLayout> reorderedItems)
        {
            if (!this.isValidOrder(reorderedItems))
                return;
            List<SuggestedMetric> results = new List<SuggestedMetric>();
            for (int i = 0; i < reorderedItems.Count; i++)
            {
                ExperimentDifficultyEstimateLayout item = reorderedItems[i];
                results.Add(item.SuggestedMetric);
            }
            int requiredNumChoices = 3;
            if (results.Count != requiredNumChoices)
                throw new ArgumentException("ExperimentationDifficultySelectionLayout unsupported number of choices: got " + results.Count + ", expected " + requiredNumChoices);
            this.Done.Invoke(results);
        }

        private List<ExperimentDifficultyEstimateLayout> difficultyOptions;
        private ButtonLayout okButtonLayout;
        private TextblockLayout invalidOrder_layout;
        private ContainerLayout okButtonHolder;

    }

    class ExperimentDifficultyEstimateLayout : ContainerLayout
    {
        public event DifficultyTextChanged_Handler DifficultyText_Changed;
        public delegate void DifficultyTextChanged_Handler();
        public ExperimentDifficultyEstimateLayout(SuggestedMetric suggestedMetric, Activity activity)
        {
            this.suggestedMetric = suggestedMetric;

            this.metricChooser = new ChooseMetric_View(false);
            this.metricChooser.SetActivity(activity);
            this.metricChooser.Choose(suggestedMetric.PlannedMetric.MetricName);

            this.difficultyBox = new Editor();
            this.difficultyBox.Keyboard = Keyboard.Numeric;
            this.difficultyBox.Placeholder = this.defaultDifficulty.ToString();
            this.difficultyLayout = new TitledControl("Difficulty:", new TextboxLayout(difficultyBox));
            this.difficultyBox.TextChanged += DifficultyBox_TextChanged;

            TextblockLayout nameLayout = new TextblockLayout(activity.Name);

            GridLayout evenGrid = GridLayout.New(new BoundProperty_List(1), BoundProperty_List.Uniform(3), LayoutScore.Zero);
            GridLayout unevenGrid = GridLayout.New(new BoundProperty_List(1), new BoundProperty_List(3), LayoutScore.Get_UnCentered_LayoutScore(1));

            evenGrid.AddLayout(this.metricChooser);
            unevenGrid.AddLayout(this.metricChooser);

            evenGrid.AddLayout(nameLayout);
            unevenGrid.AddLayout(nameLayout);

            evenGrid.AddLayout(this.difficultyLayout);
            unevenGrid.AddLayout(this.difficultyLayout);


            this.SubLayout = new LayoutUnion(evenGrid, unevenGrid);
        }
        private double defaultDifficulty = 1;

        private void DifficultyBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.DifficultyText_Changed != null)
                this.DifficultyText_Changed.Invoke();
        }

        public SuggestedMetric SuggestedMetric
        {
            get
            {
                double difficulty = this.Difficulty;
                if (difficulty < 0)
                {
                    // illegal difficulty
                    return null;
                }

                this.suggestedMetric.PlannedMetric.DifficultyEstimate.EstimatedRelativeSuccessRate_FromUser = 1.0 / difficulty;
                return this.suggestedMetric;
            }
        }

        private double Difficulty
        {
            get
            {
                // if the box is empty, use the placeholder
                if (this.difficultyBox.Text == null || this.difficultyBox.Text == "")
                    return this.defaultDifficulty;

                double difficulty;
                if (!double.TryParse(this.difficultyBox.Text, out difficulty))
                {
                    // invalid relative difficulty
                    return -1;
                }
                if (difficulty <= 0)
                {
                    // another invalid difficulty
                    return -1;
                }
                return difficulty;
            }
        }
        SuggestedMetric suggestedMetric;
        Editor difficultyBox;
        LayoutChoice_Set difficultyLayout;
        ChooseMetric_View metricChooser;
    }

}
