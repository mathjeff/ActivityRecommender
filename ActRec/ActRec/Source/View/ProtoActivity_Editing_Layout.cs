using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    public class ProtoActivity_Editing_Layout : ContainerLayout, OnBack_Listener
    {
        public ProtoActivity_Editing_Layout(ProtoActivity protoActivity, ProtoActivity_Database database)
        {
            this.protoActivity = protoActivity;
            this.database = database;
            this.textBox = new Editor();
            this.textBox.Text = protoActivity.Text;

            Vertical_GridLayout_Builder gridBuilder = new Vertical_GridLayout_Builder();
            gridBuilder.AddLayout(new TextblockLayout("ProtoActivity #" + protoActivity.Id));

            Button deleteButton = new Button();
            deleteButton.Text = "Delete";
            deleteButton.Clicked += DeleteButton_Clicked;
            gridBuilder.AddLayout(new ButtonLayout(deleteButton));
            gridBuilder.AddLayout(ScrollLayout.New(new TextboxLayout(this.textBox)));

            this.SubLayout = gridBuilder.Build();
        }

        private void DeleteButton_Clicked(object sender, EventArgs e)
        {
            this.database.Remove(this.protoActivity);
            this.protoActivity.Ratings = Distribution.MakeDistribution(0, 0, 0);
            this.protoActivity.Text = null;
        }

        public void OnBack(LayoutChoice_Set layout)
        {
            this.save();
        }
        private void save()
        {
            this.protoActivity.Text = this.textBox.Text;
            this.protoActivity.LastInteractedWith = DateTime.Now;
        }
        private ProtoActivity protoActivity;
        private ProtoActivity_Database database;
        private Editor textBox;
    }
}
