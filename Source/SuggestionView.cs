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
        public SuggestionView(ActivitySuggestion suggestion)
        {
            //this.displayGrid = GridLayout.New(BoundProperty_List.Uniform(4), new BoundProperty_List(2), LayoutScore.Zero);
            this.displayGrid = GridLayout.New(BoundProperty_List.Uniform(4), new BoundProperty_List(2), LayoutScore.Zero);
            
            /*
            //DisplayGrid grid = this.displayGrid = this;

            // setup to display the desired data
            ResizableTextBlock name_nameBlock = new ResizableTextBlock("Name:");
            name_nameBlock.SetResizability(new Resizability(0, 1));
            grid.AddItem(name_nameBlock);



            grid.AddItem(new ResizableTextBlock(suggestion.ActivityDescriptor.ActivityName));


            ResizableTextBlock name_nameBlock = new ResizableTextBlock("StartDate:");
            name_nameBlock.SetResizability(new Resizability(0, 1));
            grid.AddItem(name_nameBlock);



            ResizableTextBlock name_nameBlock = new ResizableTextBlock("Participation Probability:");
            name_nameBlock.SetResizability(new Resizability(0, 1));
            grid.AddItem(name_nameBlock);

            */
            this.add_displayField("Name:", suggestion.ActivityDescriptor.ActivityName);

            this.add_displayField("StartDate:", suggestion.StartDate.ToString());

            this.add_displayField("Participation Probability:", suggestion.ParticipationProbability.ToString());

            this.add_displayField("Rating:", suggestion.PredictedScore.Mean.ToString());

            //ResizableWrapper wrapper = new ResizableWrapper();

            // the proper way to display a border for items found in a displayGrid (since I don't want two borders for adjacent items) would be to have the
            // displayGrid do it. For now, we just put it on this SuggestionView because it's convenient
            //this.BorderBrush = System.Windows.Media.Brushes.Yellow;
            //this.BorderThickness = new System.Windows.Thickness(2, 1, 2, 1);


            //this.Child = this.displayGrid;
            this.SubLayout = this.displayGrid;
        }
        private void add_displayField(string propertyName, string propertyValue)
        {
            /*TitledTextblock result = new TitledTextblock(propertyName);
            result.Text = propertyValue;
            return result;
            */

            //DisplayGrid grid = new DisplayGrid(1, 2);
            
            TextBlock nameBlock = new TextBlock();
            nameBlock.Text = propertyName;
            //nameBlock.SetResizability(new Resizability(0, 1));
            nameBlock.TextAlignment = System.Windows.TextAlignment.Left;
            nameBlock.VerticalAlignment = VerticalAlignment.Center;
            this.displayGrid.AddLayout(new TextblockLayout(nameBlock));

            TextBlock valueBlock = new TextBlock();
            valueBlock.Text = propertyValue;
            valueBlock.TextAlignment = System.Windows.TextAlignment.Center;
            valueBlock.VerticalAlignment = VerticalAlignment.Center;
            this.displayGrid.AddLayout(new TextblockLayout(valueBlock));
            //return grid;
            
        }
        
        private GridLayout displayGrid;
        
    }
}
