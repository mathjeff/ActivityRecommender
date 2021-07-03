using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;

namespace ActivityRecommendation.View
{
    class ActivitiesMenuLayout : TitledControl
    {
        public ActivitiesMenuLayout(
            ActivitySearchView browseInheritancesLayout,
            ActivityImportLayout importPremadeActivitiesLayout,
            ActivityCreationLayout activityCreationLayout,
            ActivityEditingLayout activityEditingLayout,
            ProtoActivities_Layout protoactivitiesLayout,
            LayoutStack layoutStack,
            ActivityDatabase activityDatabase)
        {
            this.SetTitle("Activities");

            this.browseInheritancesLayout = browseInheritancesLayout;
            this.importPremadeActivitiesLayout = importPremadeActivitiesLayout;
            this.activityCreationLayout = activityCreationLayout;
            this.activityEditingLayout = activityEditingLayout;
            this.protoactivitiesLayout = protoactivitiesLayout;
            this.layoutStack = layoutStack;
            this.activityDatabase = activityDatabase;
            this.setupLayouts();
        }


        private void setupLayouts()
        {
            StackEntry browseEntry = new StackEntry(this.browseInheritancesLayout, "Browse", null);
            StackEntry importEntry = new StackEntry(this.importPremadeActivitiesLayout, "Quickstart / Premade", null);
            StackEntry addEntry = new StackEntry(this.activityCreationLayout, "New", null);
            StackEntry editEntry = new StackEntry(this.activityEditingLayout, "Edit", null);
            StackEntry protoactivitiesEntry = new StackEntry(this.protoactivitiesLayout, "Protoactivities: Brainstorm", null);

            MenuLayoutBuilder fullBuilder = new MenuLayoutBuilder(this.layoutStack)
                .AddLayout(new AppFeatureCount_ButtonName_Provider(new BrowseActivitiesMenu_Namer(this.activityDatabase), this.browseInheritancesLayout.GetFeatures()), browseEntry)
                .AddLayout(new AppFeatureCount_ButtonName_Provider("Quickstart / Premade", this.importPremadeActivitiesLayout.GetFeatures()), importEntry)
                .AddLayout(new AppFeatureCount_ButtonName_Provider("New (Category / ToDo / Problem)", this.activityCreationLayout.GetFeatures()), addEntry)
                .AddLayout(new AppFeatureCount_ButtonName_Provider("Edit", this.activityEditingLayout.GetFeatures()), editEntry)
                .AddLayout(new AppFeatureCount_ButtonName_Provider("Protoactivities: Brainstorm", this.protoactivitiesLayout.GetFeatures()), protoactivitiesEntry);
            this.SetContent(fullBuilder.Build());
        }

        public List<AppFeature> GetFeatures()
        {
            List<AppFeature> features = new List<AppFeature>(this.importPremadeActivitiesLayout.GetFeatures());
            features.AddRange(this.activityCreationLayout.GetFeatures());
            features.AddRange(this.activityEditingLayout.GetFeatures());
            features.AddRange(this.browseInheritancesLayout.GetFeatures());
            features.AddRange(this.protoactivitiesLayout.GetFeatures());
            return features;
        }

        LayoutStack layoutStack;
        ActivitySearchView browseInheritancesLayout;
        ActivityImportLayout importPremadeActivitiesLayout;
        ActivityCreationLayout activityCreationLayout;
        ActivityEditingLayout activityEditingLayout;
        ProtoActivities_Layout protoactivitiesLayout;

        ActivityDatabase activityDatabase;

    }

    class BrowseActivitiesMenu_Namer : ValueProvider<string>
    {
        public BrowseActivitiesMenu_Namer(ActivityDatabase activityDatabase)
        {
            this.activityDatabase = activityDatabase;
        }

        public string Get()
        {
            int count = this.activityDatabase.NumCustomActivities;
            string title = "Browse (" + count + ")";
            return title;
        }


        ActivityDatabase activityDatabase;
    }
}
