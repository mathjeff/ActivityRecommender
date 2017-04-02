using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using VisiPlacement;

// The user is able to view statistics of their data
// This menu is used to allow the user to open that view
namespace ActivityRecommendation
{
    class ActivityVisualizationMenu : TitledControl
    {
        public ActivityVisualizationMenu()
        {
            this.SetTitle("View Statistics");
            this.displayGrid = GridLayout.New(BoundProperty_List.Uniform(3), new BoundProperty_List(1), LayoutScore.Zero);

            this.yAxisNameBox = new ActivityNameEntryBox("Y-Axis Activity (required)");
            this.displayGrid.AddLayout(this.yAxisNameBox);

            this.okButton = new Button();

            this.displayGrid.AddLayout(new LayoutCache(new ButtonLayout(this.okButton, "Visualize")));

            //this.xAxisNameBox = new ActivityNameEntryBox("X-Axis Activity (optional)");
            //this.xAxisProgressionSelector = new ProgressionSelectionView("X-Axis");
            //this.displayGrid.AddItem(this.xAxisProgressionSelector);


            this.SetContent(new LayoutCache(this.displayGrid));
        }

        public void AddOkClickHandler(RoutedEventHandler e)
        {
            this.okButton.Click += e;
        }
        public ActivityDatabase ActivityDatabase
        {
            set
            {
                //this.xAxisNameBox.Database = value;
                //this.xAxisProgressionSelector.ActivityDatabase = value;
                this.yAxisNameBox.Database = value;
            }
        }
        /*
        public string XAxisActivityName
        {
            get
            {
                return this.xAxisNameBox.NameText;
            }
            set
            {
                this.xAxisNameBox.NameText = value;
            }
        }
        */
        public IProgression XAxisProgression
        {
            get
            {
                return TimeProgression.AbsoluteTime; //TODO: fix this
                //return this.xAxisProgressionSelector.Progression;
            }
        }
        public string YAxisActivtyName
        {
            get
            {
                return this.yAxisNameBox.NameText;
            }
            set
            {
                this.yAxisNameBox.NameText = value;
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
        GridLayout displayGrid;
        ActivityNameEntryBox yAxisNameBox;
        ActivityNameEntryBox xAxisNameBox;
        //ProgressionSelectionView xAxisProgressionSelector; // TODO: make this work
    }
}
