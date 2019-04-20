using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    // Allows creating or updating a ProtoActivity
    public class ProtoActivity_Editing_Layout : ContainerLayout, OnBack_Listener
    {
        public ProtoActivity_Editing_Layout(ProtoActivity protoActivity, ProtoActivity_Database database)
        {
            this.protoActivity = protoActivity;
            this.database = database;
            this.textBox = new Editor();
            this.textBox.Text = protoActivity.Text;

            Vertical_GridLayout_Builder gridBuilder = new Vertical_GridLayout_Builder();
            TextblockLayout title;
            if (protoActivity.Id >= 0)
                title = new TextblockLayout("Protoactivity #" + protoActivity.Id);
            else
                title = new TextblockLayout("New Protoactivity");
            gridBuilder.AddLayout(title);
            gridBuilder.AddLayout(ScrollLayout.New(new TextboxLayout(this.textBox)));

            this.SubLayout = gridBuilder.Build();
        }

        private void DeleteButton_Clicked(object sender, EventArgs e)
        {
            this.delete();
        }
        private void delete()
        {
            this.textBox.Text = null;
        }

        public void OnBack(LayoutChoice_Set layout)
        {
            this.persist();
        }
        private void persist()
        {
            if (this.protoActivity.Text != this.textBox.Text)
            {
                this.protoActivity.Text = this.textBox.Text;
                this.protoActivity.LastInteractedWith = DateTime.Now;
                if (this.protoActivity.Text != null && this.protoActivity.Text != "")
                    this.database.Put(this.protoActivity);
                else
                    this.database.Remove(this.protoActivity);
            }
        }
        private ProtoActivity protoActivity;
        private ProtoActivity_Database database;
        private Editor textBox;
    }
}
