using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    class ResetVersionNumberLayout : ContainerLayout
    {
        public event ChangeVersion_Handler RequestChangeVersion;
        public delegate void ChangeVersion_Handler(string version);
        public ResetVersionNumberLayout()
        {
            this.label = new Label();
            TextblockLayout instructions = new TextblockLayout(this.label, 16, false, false);

            Button clearVersionButton = new Button();
            clearVersionButton.Clicked += ClearVersionButton_Clicked;

            Button resetVersionButton = new Button();
            resetVersionButton.Clicked += ResetVersionButton_Clicked;

            this.SubLayout = new Vertical_GridLayout_Builder().Uniform()
                .AddLayout(instructions)
                .AddLayout(new ButtonLayout(clearVersionButton, "Clear Saved Version"))
                .AddLayout(new ButtonLayout(resetVersionButton, "Reset Saved Version to '1.0.0'"))
                .Build();
        }

        private void ClearVersionButton_Clicked(object sender, EventArgs e)
        {
            this.update("");
        }
        private void ResetVersionButton_Clicked(object sender, EventArgs e)
        {
            this.update("1.0.0");
        }
        private void update(string text)
        {
            if (this.RequestChangeVersion != null)
            {
                this.RequestChangeVersion.Invoke(text);
                this.label.Text = "Updated version to '" + text + "'; restart ActivityRecommender to see the effect";
            }
        }
        Label label;
    }
}
