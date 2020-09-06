using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    // a SearchProtoActivities_Layout lets the user search for protoactivities via a text query
    class SearchProtoActivities_Layout : ContainerLayout
    {
        public SearchProtoActivities_Layout(ProtoActivity_Database protoactivityDatabase, ActivityDatabase activityDatabase, LayoutStack layoutStack)
        {
            this.protoactivityDatabase = protoactivityDatabase;
            this.activityDatabase = activityDatabase;
            this.layoutStack = layoutStack;

            this.queryBox = new Editor();
            this.queryBox.TextChanged += QueryText_Changed;
            this.okButton = new Button();
            this.okButton.Clicked += OkButton_Clicked;

            LayoutChoice_Set sublayout = new Vertical_GridLayout_Builder()
                .AddLayout(new ButtonLayout(this.okButton))
                .AddLayout(new TextboxLayout(this.queryBox))
                .BuildAnyLayout();
            this.SubLayout = sublayout;
        }

        private void QueryText_Changed(object sender, TextChangedEventArgs e)
        {
            this.updateButtonText();
        }

        private void OkButton_Clicked(object sender, EventArgs e)
        {
            ProtoActivity protoActivity = this.ProtoActivity;
            if (protoActivity != null)
            {
                ProtoActivity_Editing_Layout layout = new ProtoActivity_Editing_Layout(protoActivity, this.protoactivityDatabase, this.activityDatabase, this.layoutStack);
                this.layoutStack.AddLayout(layout, "Proto", layout);
            }
        }

        private void updateButtonText()
        {
            ProtoActivity protoActivity = this.ProtoActivity;
            string summary = "";
            if (protoActivity != null)
                summary = protoActivity.Summarize();
            this.okButton.Text = summary;
        }

        private string QueryText
        {
            get
            {
                return this.queryBox.Text;
            }
        }
        private ProtoActivity ProtoActivity
        {
            get
            {
                return this.protoactivityDatabase.TextSearch(this.QueryText);
            }
        }

        private LayoutStack layoutStack;
        private ProtoActivity_Database protoactivityDatabase;
        private ActivityDatabase activityDatabase;
        private Editor queryBox;
        private Button okButton;
    }
}
