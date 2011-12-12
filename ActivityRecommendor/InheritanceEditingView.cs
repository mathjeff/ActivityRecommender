using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace ActivityRecommendation
{
    class InheritanceEditingView : TitledControl
    {
        public InheritanceEditingView()
        {
            this.SetTitle("Enter Activities to Choose From");

            DisplayGrid content = new DisplayGrid(2, 2);

            this.childNameBox = new TitledTextbox("Activity Name");
            content.AddItem(this.childNameBox);

            this.parentNameBox = new ActivityNameEntryBox("Parent Name");
            content.AddItem(this.parentNameBox);

            this.okButton = new Button();
            this.okButton.Content = "OK";
            this.okButton.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            content.AddItem(okButton);

            this.SetContent(content);
        }
        public string ChildName
        {
            get
            {
                return this.childNameBox.Text;
            }
            set
            {
                this.childNameBox.Text = value;
            }
        }
        public string ParentName
        {
            get
            {
                return this.parentNameBox.NameText;
            }
            set
            {
                this.parentNameBox.NameText = value;
            }
        }
        public ActivityDatabase ActivityDatabase
        {
            set
            {
                this.parentNameBox.Database = value;
            }
        }
        public void AddClickHandler(RoutedEventHandler h)
        {
            this.okButton.Click += h;
        }
        private TitledTextbox childNameBox;
        private ActivityNameEntryBox parentNameBox;
        private Button okButton;
    }
}
