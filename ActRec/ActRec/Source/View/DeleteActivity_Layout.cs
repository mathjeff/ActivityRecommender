using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    public class Confirm_BackupBeforeDeleteActivity_Layout : ContainerLayout
    {
        public event Confirmed_BackupBeforeDelete_Activity_Handler Confirmed_BackupBeforeDelete_Activity;
        public delegate void Confirmed_BackupBeforeDelete_Activity_Handler(Activity activity);

        public Confirm_BackupBeforeDeleteActivity_Layout(Activity activity, LayoutStack layoutStack)
        {
            this.activity = activity;
            TextblockLayout warning = new TextblockLayout("You have requested to delete " + activity.Name + ". We will first make a backup of all of your data, and then reload from that backup and exclude " +
                "this activity. Would you like to continue?");
            Button okButton = new Button();
            okButton.Clicked += ConfirmDeletion;

            LayoutChoice_Set credits = new CreditsButtonBuilder(layoutStack)
                .AddContribution(ActRecContributor.TOBY_HUANG, new DateTime(2021, 02, 23), "Suggested supporting deletion of created activities that have never been used, in case of mistakes in creating them")
                .Build();

            GridLayout_Builder builder = new Vertical_GridLayout_Builder()
                .AddLayout(warning)
                .AddLayout(new ButtonLayout(okButton, "Back up your data"))
                .AddLayout(credits);
            this.SubLayout = builder.BuildAnyLayout();
        }

        private void ConfirmDeletion(object sender, EventArgs e)
        {
            if (this.Confirmed_BackupBeforeDelete_Activity != null)
                this.Confirmed_BackupBeforeDelete_Activity.Invoke(this.activity);
        }
        private Activity activity;
    }

    public class DeleteActivity_Button : ContainerLayout
    {
        public event RequestDeletion_Handler RequestDeletion;
        public delegate void RequestDeletion_Handler(Activity activity, string BackupFilepath);
        public DeleteActivity_Button(Activity activity)
        {
            Button button = new Button();
            button.Clicked += Button_Clicked;
            this.activity = activity;
            this.SubLayout = new ButtonLayout(button, "Delete " + activity.Name + " and restart");
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            if (this.RequestDeletion != null)
                this.RequestDeletion.Invoke(this.activity, this.BackupContent);
        }

        public string BackupContent;

        private Activity activity;
    }
}
