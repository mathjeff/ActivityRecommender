﻿using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;
using Microsoft.Maui.Controls;

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
            this.okButtonLayout = new ButtonLayout(this.okButton);

            LayoutChoice_Set sublayout = new Vertical_GridLayout_Builder()
                .AddLayout(this.okButtonLayout)
                .AddLayout(new TextboxLayout(this.queryBox))
                .BuildAnyLayout();
            this.SubLayout = sublayout;
        }

        public List<AppFeature> GetFeatures()
        {
            return new List<AppFeature>() { new SearchProtoActivities_Feature(this.protoactivityDatabase) };
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
            this.okButtonLayout.setText(summary);
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
        private ButtonLayout okButtonLayout;
    }

    class SearchProtoActivities_Feature : AppFeature
    {
        public SearchProtoActivities_Feature(ProtoActivity_Database protoActivity_database)
        {
            this.protoActivity_database = protoActivity_database;
        }

        public string GetDescription()
        {
            return "Search your protoactivities";
        }

        public bool GetIsUsable()
        {
            return this.protoActivity_database.NonEmpty;
        }

        public bool GetHasBeenUsed()
        {
            // We don't track whether this has been used so we assume if it's usable then it has been used
            return this.GetIsUsable();
        }
        private ProtoActivity_Database protoActivity_database;
    }
}
