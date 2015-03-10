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

            // have the X button use the rightmost 10% of the view
            BoundProperty_List widths = new BoundProperty_List(2);
            widths.SetPropertyScale(0, 9);
            widths.SetPropertyScale(1, 1);
            widths.BindIndices(0, 1);
            this.mainGrid = GridLayout.New(new BoundProperty_List(1), widths, LayoutScore.Zero);

            this.contentGrid = GridLayout.New(BoundProperty_List.Uniform(4), new BoundProperty_List(1), LayoutScore.Zero);
            
            this.contentGrid.AddLayout(this.make_displayField("Name:", suggestion.ActivityDescriptor.ActivityName));
            this.contentGrid.AddLayout(this.make_displayField("When:", suggestion.StartDate.ToString("hh:mm:ss")));
            this.contentGrid.AddLayout(this.make_displayField("Probability:", Math.Round(suggestion.ParticipationProbability, 3).ToString()));
            this.contentGrid.AddLayout(this.make_displayField("Rating:", Math.Round(suggestion.PredictedScore.Mean, 3).ToString()));

            this.mainGrid.AddLayout(this.contentGrid);
            this.cancelButton = new Button();
            this.cancelButton.Click += cancelButton_Click;
            this.mainGrid.AddLayout(new ButtonLayout(this.cancelButton, "X"));

            this.SubLayout = this.mainGrid;
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
        SuggestionsView container;
        ActivitySuggestion suggestion;
        
    }
}
