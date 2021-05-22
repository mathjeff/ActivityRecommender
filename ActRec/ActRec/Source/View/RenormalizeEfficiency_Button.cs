using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    public class Confirm_BackupBeforeRecalculateEfficiency_Layout : ContainerLayout
    {
        public event Confirmed_BackupBeforeRecalculateEfficiency_Handler Confirmed_BackupBefore_RecalculateEfficiency;
        public delegate void Confirmed_BackupBeforeRecalculateEfficiency_Handler();

        public Confirm_BackupBeforeRecalculateEfficiency_Layout()
        {
            TextblockLayout warning = new TextblockLayout("You have requested to recalculate your efficiency using ActivityRecommender's latest algorithm. We will first " +
                "make a backup of your data, and then reload from that backup and recalculate efficiency. Would you like to continue?");
            Button okButton = new Button();
            okButton.Clicked += Confirm;
            GridLayout_Builder builder = new Vertical_GridLayout_Builder()
                .AddLayout(warning)
                .AddLayout(new ButtonLayout(okButton, "Back up your data"));
            this.SubLayout = builder.BuildAnyLayout();
        }

        private void Confirm(object sender, EventArgs e)
        {
            if (this.Confirmed_BackupBefore_RecalculateEfficiency != null)
                this.Confirmed_BackupBefore_RecalculateEfficiency.Invoke();
        }
    }


    public class RenormalizeEfficiency_Button : ContainerLayout
    {
        public event RequestEfficiencyRecalculation_Handler RequestRecalculation;
        public delegate void RequestEfficiencyRecalculation_Handler(string BackupContent);
        public RenormalizeEfficiency_Button()
        {
            Button button = new Button();
            button.Clicked += Button_Clicked;
            this.SubLayout = new ButtonLayout(button, "Recalculate efficiencies and restart");
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            if (this.RequestRecalculation != null)
                this.RequestRecalculation.Invoke(this.BackupContent);
        }

        public string BackupContent;
    }

}
