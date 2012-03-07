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
            this.displayGrid = new DisplayGrid(2, 1);

            this.nameEntryBox = new ActivityNameEntryBox("Name of Activity to View");
            this.displayGrid.AddItem(this.nameEntryBox);

            this.okButton = new ResizableButton();
            this.okButton.Content = "Visualize";
            this.okButton.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            this.displayGrid.AddItem(this.okButton);

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
                this.nameEntryBox.Database = value;
            }
        }
        public string ActivityName
        {
            get
            {
                return this.nameEntryBox.NameText;
            }
            set
            {
                this.nameEntryBox.NameText = value;
            }
        }
        public ActivityDescriptor ActivityDescriptor
        {
            get
            {
                return this.nameEntryBox.ActivityDescriptor;
            }
        }
        ResizableButton okButton;
        DisplayGrid displayGrid;
        ActivityNameEntryBox nameEntryBox;
    }
}
