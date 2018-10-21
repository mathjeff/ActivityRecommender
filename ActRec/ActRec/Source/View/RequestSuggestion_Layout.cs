using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    public class RequestSuggestion_Layout: ContainerLayout
    {
        public event RequestSuggestion_Handler RequestSuggestion;
        public delegate void RequestSuggestion_Handler(ActivityRequest activityRequest);

        public RequestSuggestion_Layout(ActivityDatabase activityDatabase, bool allowRequestingActivitiesDirectly)
        {
            Button suggestionButton = new Button();
            suggestionButton.Clicked += SuggestionButton_Clicked;
            ButtonLayout buttonLayout = new ButtonLayout(suggestionButton, "Suggest");

            this.categoryBox = new ActivityNameEntryBox("From category (optional):");
            this.categoryBox.Database = activityDatabase;

            this.desiredActivity_box = new ActivityNameEntryBox("At least as fun as this activity (optional):");
            this.desiredActivity_box.Database = activityDatabase;

            if (!allowRequestingActivitiesDirectly)
            {
                this.SubLayout = buttonLayout;
            }
            else
            {
                GridLayout horizontalContentLayout = GridLayout.New(new BoundProperty_List(1), new BoundProperty_List(2), LayoutScore.Get_UnCentered_LayoutScore(1));
                horizontalContentLayout.AddLayout(buttonLayout);

                GridLayout configurationLayout = GridLayout.New(BoundProperty_List.Uniform(2), new BoundProperty_List(1), LayoutScore.Zero);
                configurationLayout.AddLayout(this.categoryBox);
                configurationLayout.AddLayout(this.desiredActivity_box);

                horizontalContentLayout.AddLayout(configurationLayout);

                GridLayout verticalContentLayout = GridLayout.New(new BoundProperty_List(2), new BoundProperty_List(1), LayoutScore.Get_UnCentered_LayoutScore(2));
                verticalContentLayout.AddLayout(configurationLayout);
                verticalContentLayout.AddLayout(buttonLayout);

                this.SubLayout = new LayoutUnion(horizontalContentLayout, verticalContentLayout);
            }
        }

        private void SuggestionButton_Clicked(object sender, EventArgs e)
        {
            ActivityRequest activityRequest = new ActivityRequest(this.categoryBox.Activity, this.desiredActivity_box.Activity, DateTime.Now);
            this.RequestSuggestion.Invoke(activityRequest);
        }

        private ActivityNameEntryBox categoryBox;
        private ActivityNameEntryBox desiredActivity_box;
    }

}
