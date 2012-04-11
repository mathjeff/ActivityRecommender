using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

// The user is able to view statistics of their data
// This menu is used to allow the user to open that view
namespace ActivityRecommendation
{
    class MiniStatisticsMenu : TitledControl
    {
        public MiniStatisticsMenu()
        {
            this.SetTitle("View Statistics");
            this.displayGrid = new DisplayGrid(3, 1);

            this.yAxisNameBox = new ActivityNameEntryBox("Y-Axis Activity (required)");
            this.displayGrid.AddItem(this.yAxisNameBox);

            this.okButton = new ResizableButton();
            this.okButton.Content = "Visualize";
            this.okButton.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            this.displayGrid.AddItem(this.okButton);

            this.xAxisNameBox = new ActivityNameEntryBox("X-Axis Activity (optional)");
            this.xAxisProgressionSelector = new ProgressionSelectionView("X-Axis");
            this.displayGrid.AddItem(this.xAxisProgressionSelector);


            this.SetContent(this.displayGrid);
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
                this.xAxisProgressionSelector.ActivityDatabase = value;
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
                return this.xAxisProgressionSelector.Progression;
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
        ResizableButton okButton;
        DisplayGrid displayGrid;
        ActivityNameEntryBox yAxisNameBox;
        ActivityNameEntryBox xAxisNameBox;
        ProgressionSelectionView xAxisProgressionSelector;
    }
}
