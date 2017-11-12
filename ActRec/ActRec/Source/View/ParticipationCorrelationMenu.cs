using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    class ParticipationCorrelationMenu : TitledControl
    {
        public ParticipationCorrelationMenu(LayoutStack layoutStack, ActivityDatabase activityDatabase, Engine engine)
        {
            this.layoutStack = layoutStack;
            this.activityDatabase = activityDatabase;
            this.engine = engine;


            this.SetTitle("Finding which activities often precede another");

            Vertical_GridLayout_Builder builder = new Vertical_GridLayout_Builder();

            this.activityName_box = new ActivityNameEntryBox("Activity:");
            this.activityName_box.Database = activityDatabase;
            builder.AddLayout(this.activityName_box);

            builder.AddLayout(new TextblockLayout("Window duration:"));
            this.durationBox = new DurationEntryView();
            builder.AddLayout(this.durationBox);

            this.okButton = new Button();
            this.okButton.Clicked += OkButton_Click;
            builder.AddLayout(new ButtonLayout(this.okButton, "Ok"));

            this.SetContent(builder.Build());


        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            this.Enter();
        }
        private void Enter()
        {
            if (this.durationBox.IsDurationValid())
                this.layoutStack.AddLayout(new ParticipationCorrelationView(this.engine, this.activityDatabase, this.activityName_box.ActivityDescriptor, this.durationBox.GetDuration()));
        }

        private ActivityNameEntryBox activityName_box;
        private DurationEntryView durationBox;
        private Button okButton;
        private LayoutStack layoutStack;
        private Engine engine;
        private ActivityDatabase activityDatabase;


    }
}
