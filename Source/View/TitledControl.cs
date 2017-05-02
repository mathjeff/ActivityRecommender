using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using VisiPlacement;

// the TitledControl adds a title above another Control
namespace ActivityRecommendation
{
    public class TitledControl : LayoutCache
    {
        public TitledControl()
        {
            this.Initialize();
        }
        public TitledControl(string startingTitle)
        {
            this.Initialize();
            this.titleBlock.Text = startingTitle;
        }
        private void Initialize()
        {
            this.titleBlock = new TextBlock();
            this.titleBlock.TextAlignment = System.Windows.TextAlignment.Center;
            this.gridLayout = GridLayout.New(new BoundProperty_List(2), new BoundProperty_List(1), LayoutScore.Zero);
            //this.SetContent(
            //this.titleBlock.SetResizability(new Resizability(0, 0.25));    // the title doesn't need to be very big
            this.gridLayout.AddLayout(new TextblockLayout(this.titleBlock));
            base.LayoutToManage = gridLayout;
        }
        public void SetTitle(string newTitle)
        {
            this.titleBlock.Text = newTitle;
        }
        public void SetContent(LayoutChoice_Set layout)
        {
            this.gridLayout.PutLayout(layout, 0, 1);
        }
        protected TextBlock TitleBlock
        {
            get
            {
                return this.titleBlock;
            }
        }
        TextBlock titleBlock;
        GridLayout gridLayout;
    }
}
