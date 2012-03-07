using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

// the TitledControl adds a title above another Control
namespace ActivityRecommendation
{
    class TitledControl : DisplayGrid
    {
        public TitledControl()
            : base(2, 1)
        {
            this.Initialize();
        }
        public TitledControl(string startingTitle)
            : base(2, 1)
        {
            this.Initialize();
            this.titleBlock.Text = startingTitle;
        }
        private void Initialize()
        {
            this.titleBlock = new ResizableTextBlock();
            this.titleBlock.TextAlignment = System.Windows.TextAlignment.Center;
            this.titleBlock.SetResizability(new Resizability(0, 0.25));    // the title doesn't need to be very big
            //this.titleBlock.TextWrapping = TextWrapping.Wrap;
            //this.titleBlock.VerticalAlignment = VerticalAlignment.Bottom;
            //this.titleBlock.HorizontalAlignment = HorizontalAlignment.Center;
            this.AddItem(this.titleBlock);
        }
        public void SetTitle(string newTitle)
        {
            this.titleBlock.Text = newTitle;
        }
        public void SetContent(UIElement newContent)
        {
            this.PutItem(newContent, 1, 0);
        }
        protected ResizableTextBlock TitleBlock
        {
            get
            {
                return this.titleBlock;
            }
        }
        ResizableTextBlock titleBlock;
    }
}
