/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace ActivityRecommendation
{
    class ResizableTextBlock : ResizableWrapper
    {
        public ResizableTextBlock()
        {
            this.Initialize();
        }
        public ResizableTextBlock(string startingText)
        {
            this.Initialize();
            this.Text = startingText;
        }
        private void Initialize()
        {
            this.textBlock = new TextBlock();
            this.textBlock.TextWrapping = TextWrapping.Wrap;
            this.textBlock.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.SetContent(this.textBlock, new Resizability(1, 1));
        }

        public string Text
        {
            get
            {
                return this.textBlock.Text;
            }
            set
            {
                this.textBlock.Text = value;
            }
        }
        public TextAlignment TextAlignment
        {
            set
            {
                this.textBlock.TextAlignment = value;
            }
        }


        public TextBlock textBlock;
    }
}
*/