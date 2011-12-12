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
            this.titleBlock = new TextBlock();
            this.titleBlock.TextAlignment = System.Windows.TextAlignment.Center;
            this.titleBlock.Height = 20;
            this.AddItem(this.titleBlock);
            this.RowDefinitions[0].Height = new System.Windows.GridLength(30);
        }
        public void SetTitle(string newTitle)
        {
            this.titleBlock.Text = newTitle;
        }
        public void SetContent(UIElement newContent)
        {
            this.SetItem(newContent, 1, 0);
        }
        TextBlock titleBlock;
    }
}
