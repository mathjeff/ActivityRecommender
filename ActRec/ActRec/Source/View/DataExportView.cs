﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using VisiPlacement;
using Microsoft.Maui.Controls;


// Allows the user to export their data
namespace ActivityRecommendation
{
    class DataExportView : TitledControl
    {
        public DataExportView(ActivityRecommender activityRecommender, UserSettings persona, LayoutStack layoutStack)
        {
            this.SetTitle("Export Your Data");

            this.persona = persona;
            

            this.exportButton = new Button();
            this.exportButton.Clicked += ExportButton_Clicked;

            this.activityRecommender = activityRecommender;
            this.layoutStack = layoutStack;

            this.SetupView();
            this.persona.Changed += PersonaChanged;
        }

        private void PersonaChanged()
        {
            this.SetupView();
        }

        private void SetupView()
        {
            LayoutChoice_Set instructions = new TextblockLayout("This this txt file contains most of what you've provided to " + this.persona.PersonaName + ", and so it may become large.");
            ButtonLayout buttonLayout = new ButtonLayout(this.exportButton, "Export");
            LayoutChoice_Set credits = new CreditsButtonBuilder(this.layoutStack)
                .AddContribution(ActRecContributor.ANNI_ZHANG, new DateTime(2020, 04, 05), "Pointed out that exported data files could not be seen by users on iOS")
                .AddContribution(ActRecContributor.TOBY_HUANG, new DateTime(2021, 02, 16), "Pointed out that it was hard to find the exported files on ChromeOS")
                .Build();

            this.SetContent(new Vertical_GridLayout_Builder().Uniform()
                .AddLayout(instructions)
                .AddLayout(buttonLayout)
                .AddLayout(credits)
                .Build());
        }

        private async void ExportButton_Clicked(object sender, EventArgs e)
        {
            await this.activityRecommender.ExportData();
        }

        Button exportButton;
        ActivityRecommender activityRecommender;
        UserSettings persona;
        LayoutStack layoutStack;
    }
}
