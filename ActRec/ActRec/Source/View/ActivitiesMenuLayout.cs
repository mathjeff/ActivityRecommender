using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;

namespace ActivityRecommendation.View
{
    class ActivitiesMenuLayout : ContainerLayout
    {
        public ActivitiesMenuLayout(
            LayoutChoice_Set browseInheritancesLayout,
            ActivityImportLayout importPremadeActivitiesLayout,
            InheritanceEditingLayout addOrEditActivitiesLayout,
            ProtoActivities_Layout protoactivitiesLayout,
            LayoutChoice_Set helpLayout,            
            LayoutStack layoutStack,
            ActivityDatabase activityDatabase)
        {
            this.browseInheritancesLayout = browseInheritancesLayout;
            this.importPremadeActivitiesLayout = importPremadeActivitiesLayout;
            this.addOrEditActivitiesLayout = addOrEditActivitiesLayout;
            this.protoactivitiesLayout = protoactivitiesLayout;
            this.helpLayout = helpLayout;
            this.layoutStack = layoutStack;
            this.activityDatabase = activityDatabase;
            this.setupLayouts();
        }

        public override SpecificLayout GetBestLayout(LayoutQuery query)
        {
            if (this.activityDatabase.ContainsCustomActivity())
                this.SubLayout = this.fullLayout;
            else
                this.SubLayout = this.noActivitiesLayout;
            return base.GetBestLayout(query);
        }

        private void setupLayouts()
        {
            StackEntry browseEntry = new StackEntry(this.browseInheritancesLayout, "Browse My Activities", null);
            StackEntry importEntry = new StackEntry(this.importPremadeActivitiesLayout, "Import Some Premade Activities", null);
            StackEntry addEntry = new StackEntry(this.addOrEditActivitiesLayout, "Add/Edit Activities", null);
            StackEntry protoactivitiesEntry = new StackEntry(this.protoactivitiesLayout, "Brainstorm Protoactivities", null);
            StackEntry helpEntry = new StackEntry(this.helpLayout, "Help", null);

            MenuLayoutBuilder noActivitiesBuilder = new MenuLayoutBuilder(this.layoutStack)
                .AddLayout(new AppFeatureCount_ButtonName_Provider("Import Some Premade Activities", this.importPremadeActivitiesLayout.GetFeatures()), importEntry)
                .AddLayout(new AppFeatureCount_ButtonName_Provider("Add/Edit Activities", this.addOrEditActivitiesLayout.GetFeatures()), addEntry)
                .AddLayout(new AppFeatureCount_ButtonName_Provider("Brainstorm Protoactivities", this.protoactivitiesLayout.GetFeatures()), protoactivitiesEntry)
                .AddLayout(helpEntry);
            this.noActivitiesLayout = noActivitiesBuilder.Build();

            MenuLayoutBuilder fullBuilder = new MenuLayoutBuilder(this.layoutStack)
                .AddLayout(browseEntry)
                .AddLayout(new AppFeatureCount_ButtonName_Provider("Import Some Premade Activities", this.importPremadeActivitiesLayout.GetFeatures()), importEntry)
                .AddLayout(new AppFeatureCount_ButtonName_Provider("Add/Edit Activities", this.addOrEditActivitiesLayout.GetFeatures()), addEntry)
                .AddLayout(new AppFeatureCount_ButtonName_Provider("Brainstorm Protoactivities", this.protoactivitiesLayout.GetFeatures()), protoactivitiesEntry)
                .AddLayout(helpEntry);
            this.fullLayout = fullBuilder.Build();
        }

        public List<AppFeature> GetFeatures()
        {
            List<AppFeature> features = new List<AppFeature>(this.importPremadeActivitiesLayout.GetFeatures());
            features.AddRange(this.addOrEditActivitiesLayout.GetFeatures());
            features.AddRange(this.protoactivitiesLayout.GetFeatures());
            return features;
        }

        LayoutStack layoutStack;
        LayoutChoice_Set browseInheritancesLayout;
        ActivityImportLayout importPremadeActivitiesLayout;
        InheritanceEditingLayout addOrEditActivitiesLayout;
        ProtoActivities_Layout protoactivitiesLayout;
        LayoutChoice_Set helpLayout;

        ActivityDatabase activityDatabase;

        LayoutChoice_Set noActivitiesLayout;
        LayoutChoice_Set fullLayout;

    }
}
