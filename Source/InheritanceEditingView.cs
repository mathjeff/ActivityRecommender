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
        public InheritanceEditingView(LayoutStack layoutStack)
        {
            this.layoutStack = layoutStack;

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

            ResizableButton helpButton = new ResizableButton();
            TextBlock helpTextBlock = new TextBlock();
            helpTextBlock.Text = "Help";
            content.AddLayout(new LayoutCache(new ButtonLayout(helpButton, new TextblockLayout(helpTextBlock))));
            helpButton.Click += helpButton_Click;

            this.helpMenu = (new HelpWindowBuilder()).AddMessage("This page is for you to enter activities, to use as future suggestions")
                .AddMessage("The text box on the left is where you type the activity name.")
                .AddMessage("The text box on the right is where you type another activity that you want to make a supercategory of the activity.")
                .AddMessage("For example, you might specify that Exercise is a subcategory of Useful")
                .AddMessage("Each box will offer autocomplete suggestions in case you want to type an existing activity. Press Enter to fill in the autocomplete suggestion")
                .Build();

            this.SetContent(new LayoutCache(content));
        }

        void helpButton_Click(object sender, RoutedEventArgs e)
        {
            this.layoutStack.AddLayout(this.helpMenu);
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
        private LayoutStack layoutStack;
        private LayoutChoice_Set helpMenu;
    }
}
