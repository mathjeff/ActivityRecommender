using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation
{
    class New_ParticipationComment_Layout : ContainerLayout
    {
        public event AddParticipationComment_Handler AddParticipationComment;
        public delegate void AddParticipationComment_Handler(ParticipationComment comment);

        public New_ParticipationComment_Layout(Participation participation, LayoutStack layoutStack)
        {
            this.participation = participation;
            this.textBox = new Editor();
            this.layoutStack = layoutStack;
            Button saveButton = new Button();
            saveButton.Clicked += SaveButton_Clicked;
            GridLayout_Builder builder = new Vertical_GridLayout_Builder()
                .AddLayout(new TextblockLayout("Comment"))
                .AddLayout(new TextboxLayout(textBox))
                .AddLayout(new ButtonLayout(saveButton, "Save"));
            this.SubLayout = builder.BuildAnyLayout();
        }

        private void SaveButton_Clicked(object sender, EventArgs e)
        {
            if (this.AddParticipationComment != null)
            {
                ParticipationComment comment = new ParticipationComment(this.textBox.Text, this.participation.StartDate, DateTime.Now, this.participation.ActivityDescriptor);
                this.AddParticipationComment.Invoke(comment);
            }
            this.layoutStack.GoBack();
        }

        Participation participation;
        Editor textBox;
        LayoutStack layoutStack;
    }
}
