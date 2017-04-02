using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using VisiPlacement;

// a SuggestionView displays one suggested Activity and some details of the suggestion
namespace ActivityRecommendation
{
    class SuggestionView : SingleItem_Layout
    {
        public SuggestionView(ActivitySuggestion suggestion, SuggestionsView container)
        {
            this.container = container;
            this.suggestion = suggestion;

            // have the X button use a certain amount of space on the right
            BoundProperty_List widths = new BoundProperty_List(2);
            int titleWidthWeight = 6;
            widths.SetPropertyScale(0, titleWidthWeight + 1);
            widths.SetPropertyScale(1, 1);
            widths.BindIndices(0, 1);
            this.mainGrid = GridLayout.New(new BoundProperty_List(1), widths, LayoutScore.Zero);
            this.contentGrid = GridLayout.New(BoundProperty_List.Uniform(4), new BoundProperty_List(1), LayoutScore.Zero);

            // Attempt to center the activity name, but allow it to be off-center if necessary
            TextblockLayout titleLayout = new TextblockLayout(suggestion.ActivityDescriptor.ActivityName, TextAlignment.Center);
            BoundProperty_List titleComponentWidths = new BoundProperty_List(2);
            titleComponentWidths.BindIndices(0, 1);
            titleComponentWidths.SetPropertyScale(0, 1);
            titleComponentWidths.SetPropertyScale(1, titleWidthWeight);
            GridLayout centeredTitle = GridLayout.New(BoundProperty_List.Uniform(1), titleComponentWidths, LayoutScore.Zero);
            centeredTitle.PutLayout(titleLayout, 1, 0);
            GridLayout offsetTitle = GridLayout.New(BoundProperty_List.Uniform(1), BoundProperty_List.Uniform(1), LayoutScore.Get_UnCentered_LayoutScore(1));
            offsetTitle.PutLayout(titleLayout, 0, 0);
            this.contentGrid.AddLayout(new LayoutUnion(centeredTitle, offsetTitle));

            // Add the remaining fields
            this.contentGrid.AddLayout(this.make_displayField("When:", suggestion.StartDate.ToString("hh:mm:ss")));
            this.contentGrid.AddLayout(this.make_displayField("Probability:", Math.Round(suggestion.ParticipationProbability, 3).ToString()));
            this.contentGrid.AddLayout(this.make_displayField("Rating:", Math.Round(suggestion.PredictedScore.Mean, 3).ToString()));

            // Add buttons
            this.mainGrid.AddLayout(this.contentGrid);
            this.cancelButton = new Button();
            this.cancelButton.Click += cancelButton_Click;
            this.justifyButton = new Button();
            this.justifyButton.Click += justifyButton_Click;
            GridLayout buttonsLayout = GridLayout.New(BoundProperty_List.Uniform(2), BoundProperty_List.Uniform(1), LayoutScore.Zero);
            buttonsLayout.AddLayout(new ButtonLayout(this.justifyButton, "?"));
            buttonsLayout.AddLayout(new ButtonLayout(this.cancelButton, "X"));
            this.mainGrid.AddLayout(buttonsLayout);


            this.SubLayout = this.mainGrid;
        }

        void justifyButton_Click(object sender, RoutedEventArgs e)
        {
            this.container.JustifySuggestion(this.suggestion);
        }

        void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.container.DeclineSuggestion(this.suggestion);
        }

        private LayoutChoice_Set make_displayField(string propertyName, string propertyValue)
        {
            GridLayout centeredGrid = GridLayout.New(new BoundProperty_List(1), BoundProperty_List.Uniform(2), LayoutScore.Zero);
            GridLayout uncenteredGrid = GridLayout.New(new BoundProperty_List(1), new BoundProperty_List(2), LayoutScore.Get_UnCentered_LayoutScore(1));

            TextBlock nameBlock = new TextBlock();
            nameBlock.Text = propertyName;
            nameBlock.TextAlignment = System.Windows.TextAlignment.Left;
            nameBlock.VerticalAlignment = VerticalAlignment.Center;
            TextblockLayout nameLayout = new TextblockLayout(nameBlock);
            centeredGrid.AddLayout(nameLayout);
            uncenteredGrid.AddLayout(nameLayout);

            TextBlock valueBlock = new TextBlock();
            valueBlock.Text = propertyValue;
            valueBlock.TextAlignment = System.Windows.TextAlignment.Center;
            valueBlock.VerticalAlignment = VerticalAlignment.Center;
            TextblockLayout valueLayout = new TextblockLayout(valueBlock);
            centeredGrid.AddLayout(valueLayout);
            uncenteredGrid.AddLayout(valueLayout);

            LayoutUnion row = new LayoutUnion(centeredGrid, uncenteredGrid);

            return row;
            
        }
        
        GridLayout contentGrid;
        GridLayout mainGrid;
        Button cancelButton;
        Button justifyButton;
        SuggestionsView container;
        ActivitySuggestion suggestion;
        
    }
}
