using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;
using Microsoft.Maui.Controls;

// a ProtoActivity_Splitting_Layout allows splitting a protoactivity
namespace ActivityRecommendation.View
{
    class ProtoActivity_Splitting_Layout : ContainerLayout, OnBack_Listener
    {
        public ProtoActivity_Splitting_Layout(ProtoActivity a, ProtoActivity b, ProtoActivity_Database database, LayoutStack layoutStack)
        {
            this.protoA = a;
            this.protoB = b;
            this.database = database;
            this.layoutStack = layoutStack;
            this.setupDrawing();
        }

        private void setupDrawing()
        {
            this.textA = new Editor();
            this.textA.Text = this.protoA.Text;
            this.textB = new Editor();
            this.textB.Text = this.protoB.Text;
            TextblockLayout description = new TextblockLayout("Split into two protoactivities, then go back");

            BoundProperty_List rowHeights = new BoundProperty_List(3);
            rowHeights.BindIndices(0, 2);
            GridLayout grid = GridLayout.New(rowHeights, new BoundProperty_List(1), LayoutScore.Zero);

            grid.AddLayout(ScrollLayout.New(new TextboxLayout(this.textA)));
            grid.AddLayout(description);
            grid.AddLayout(ScrollLayout.New(new TextboxLayout(this.textB)));
            this.SubLayout = grid;
        }

        public void OnBack(LayoutChoice_Set layout)
        {
            DateTime now = DateTime.Now;
            this.protoA.Text = this.textA.Text;
            this.protoA.LastInteractedWith = now;
            this.protoB.Text = this.textB.Text;
            this.protoB.LastInteractedWith = now;
            this.database.Put(this.protoA);
            this.database.Put(this.protoB);
        }

        private ProtoActivity protoA;
        private ProtoActivity protoB;
        private Editor textA;
        private Editor textB;
        private ProtoActivity_Database database;
        private LayoutStack layoutStack;

    }
}
