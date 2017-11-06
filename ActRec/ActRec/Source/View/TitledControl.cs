using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using VisiPlacement;
using Xamarin.Forms;

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
            this.titleBlock = new Label();
            this.titleBlock.HorizontalTextAlignment = TextAlignment.Center;
            this.gridLayout = GridLayout.New(new BoundProperty_List(2), new BoundProperty_List(1), LayoutScore.Zero);
            this.gridLayout.AddLayout(new TextblockLayout(this.titleBlock));
            base.LayoutToManage = gridLayout;
        }
        public void SetTitle(string newTitle)
        {
            this.titleBlock.Text = newTitle;
        }
        public string GetTitle()
        {
            return this.titleBlock.Text;
        }
        public void SetContent(LayoutChoice_Set layout)
        {
            this.gridLayout.PutLayout(layout, 0, 1);
        }
        public LayoutChoice_Set GetContent()
        {
            return this.gridLayout.GetLayout(0, 1);
        }
        protected Label TitleBlock
        {
            get
            {
                return this.titleBlock;
            }
        }
        Label titleBlock;
        GridLayout gridLayout;
    }
}
