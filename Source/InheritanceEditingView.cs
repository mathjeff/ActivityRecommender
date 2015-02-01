using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using VisiPlacement;

namespace ActivityRecommendation
{
    class InheritanceEditingView : TitledControl
    {
        public InheritanceEditingView()
        {
            this.SetTitle("Enter Activities to Choose From");

            GridLayout content = GridLayout.New(BoundProperty_List.Uniform(2), BoundProperty_List.Uniform(2), LayoutScore.Zero);

            this.childNameBox = new ActivityNameEntryBox("Activity Name");
            content.AddLayout(this.childNameBox);

            this.parentNameBox = new ActivityNameEntryBox("Parent Name");
            content.AddLayout(this.parentNameBox);

            TextBlock buttonTextBlock = new TextBlock();
            buttonTextBlock.Text = "OK";
            this.okButton = new ResizableButton();
            this.okButton.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            content.AddLayout(new LayoutCache(new ButtonLayout(this.okButton, new TextblockLayout(buttonTextBlock))));

            this.SetContent(new LayoutCache(content));
        }
        public string ChildName
        {
            get
            {
                return this.childNameBox.NameText;
            }
            set
            {
                this.childNameBox.NameText = value;
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
                this.childNameBox.Database = value;
            }
        }
        public void AddClickHandler(RoutedEventHandler h)
        {
            this.okButton.Click += h;
        }
        private ActivityNameEntryBox childNameBox;
        private ActivityNameEntryBox parentNameBox;
        private ResizableButton okButton;
    }
}
