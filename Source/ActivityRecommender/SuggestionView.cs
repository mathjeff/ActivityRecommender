using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

// a SuggestionView displays one suggested Activity and some details of the suggestion
namespace ActivityRecommendation
{
    class SuggestionView : Border, IResizable
    {
        public SuggestionView(ActivitySuggestion suggestion)
        {
            this.displayGrid = new DisplayGrid(3, 2);
            
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


            //this.AddItem(this.make_displayField("Duration:", suggestion.Duration.ToString()));


            this.add_displayField("Participation Probability:", suggestion.ParticipationProbability.ToString());
            //ResizableWrapper wrapper = new ResizableWrapper();

            // the proper way to display a border for items found in a displayGrid (since I don't want two borders for adjacent items) would be to have the
            // displayGrid do it. For now, we just put it on this SuggestionView because it's convenient
            this.BorderBrush = System.Windows.Media.Brushes.Yellow;
            this.BorderThickness = new System.Windows.Thickness(2, 1, 1, 2);


            this.Child = this.displayGrid;
        }
        private void add_displayField(string propertyName, string propertyValue)
        {
            /*TitledTextblock result = new TitledTextblock(propertyName);
            result.Text = propertyValue;
            return result;
            */

            //DisplayGrid grid = new DisplayGrid(1, 2);
            
            ResizableTextBlock nameBlock = new ResizableTextBlock(propertyName);
            nameBlock.SetResizability(new Resizability(0, 1));
            nameBlock.TextAlignment = System.Windows.TextAlignment.Left;
            nameBlock.VerticalAlignment = VerticalAlignment.Center;
            this.displayGrid.AddItem(nameBlock);

            ResizableTextBlock valueBlock = new ResizableTextBlock(propertyValue);
            valueBlock.TextAlignment = System.Windows.TextAlignment.Center;
            valueBlock.VerticalAlignment = VerticalAlignment.Center;
            this.displayGrid.AddItem(valueBlock);
            //return grid;
            
        }
        
        public Resizability GetHorizontalResizability()
        {
            return this.displayGrid.GetHorizontalResizability();
        }
        public Resizability GetVerticalResizability()
        {
            return this.displayGrid.GetVerticalResizability();
        }
        public Size PreliminaryMeasure(Size constraint)
        {
            Size availableGridsize = new Size(constraint.Width - this.BorderThickness.Left - this.BorderThickness.Right, constraint.Height - this.BorderThickness.Top - this.BorderThickness.Bottom);
            Size desiredGridSize = this.displayGrid.PreliminaryMeasure(availableGridsize);
            Size desiredSize = new Size(desiredGridSize.Width + this.BorderThickness.Left + this.BorderThickness.Right, desiredGridSize.Height + this.BorderThickness.Top + this.BorderThickness.Bottom);
            //this.Measure(constraint);
            //return this.DesiredSize;
            return desiredSize;
        }
        public Size FinalMeasure(Size finalSize)
        {
            this.Measure(finalSize);
            return this.DesiredSize;
        }
        
        private DisplayGrid displayGrid;
        
    }
}
