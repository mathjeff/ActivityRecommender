using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;

namespace ActivityRecommendation.View
{
    class ActivitiesMenuLayout : TitledControl
    {
        public ActivitiesMenuLayout(
            LayoutChoice_Set browseInheritancesLayout,
            ActivityImportLayout importPremadeActivitiesLayout,
            ActivityCreationLayout activityCreationLayout,
            ActivityEditingLayout activityEditingLayout,
            ProtoActivities_Layout protoactivitiesLayout,
            LayoutStack layoutStack,
            ActivityDatabase activityDatabase)
        {
            this.SetTitle("Activities: Add/Browse");

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
            StackEntry browseEntry = new StackEntry(this.browseInheritancesLayout, "Browse My Activities", null);
            StackEntry importEntry = new StackEntry(this.importPremadeActivitiesLayout, "Import Some Premade Activities", null);
            StackEntry addEntry = new StackEntry(this.activityCreationLayout, "Add/Edit Activities", null);
            StackEntry editEntry = new StackEntry(this.activityEditingLayout, "Edit Activities", null);
            StackEntry protoactivitiesEntry = new StackEntry(this.protoactivitiesLayout, "Brainstorm Protoactivities", null);

            MenuLayoutBuilder fullBuilder = new MenuLayoutBuilder(this.layoutStack)
                .AddLayout(new BrowseActivitiesMenu_Namer(this.activityDatabase), browseEntry)
                .AddLayout(new AppFeatureCount_ButtonName_Provider("Import Some Premade Activities", this.importPremadeActivitiesLayout.GetFeatures()), importEntry)
                .AddLayout(new AppFeatureCount_ButtonName_Provider("New Activity (Category/ToDo/Problem/Solution)", this.activityCreationLayout.GetFeatures()), addEntry)
                .AddLayout(new AppFeatureCount_ButtonName_Provider("Edit Activities", this.activityEditingLayout.GetFeatures()), editEntry)
                .AddLayout(new AppFeatureCount_ButtonName_Provider("Brainstorm Protoactivities", this.protoactivitiesLayout.GetFeatures()), protoactivitiesEntry);
            this.SetContent(fullBuilder.Build());
        }

        public List<AppFeature> GetFeatures()
        {
            List<AppFeature> features = new List<AppFeature>(this.importPremadeActivitiesLayout.GetFeatures());
            features.AddRange(this.activityCreationLayout.GetFeatures());
            features.AddRange(this.protoactivitiesLayout.GetFeatures());
            return features;
        }

        LayoutStack layoutStack;
        LayoutChoice_Set browseInheritancesLayout;
        ActivityImportLayout importPremadeActivitiesLayout;
        ActivityCreationLayout activityCreationLayout;
        ActivityEditingLayout activityEditingLayout;
        ProtoActivities_Layout protoactivitiesLayout;

        ActivityDatabase activityDatabase;

    }

    class BrowseActivitiesMenu_Namer : ValueProvider<MenuItem>
    {
        public BrowseActivitiesMenu_Namer(ActivityDatabase activityDatabase)
        {
            this.activityDatabase = activityDatabase;
        }

        public MenuItem Get()
        {
            int count = this.activityDatabase.NumCustomActivities;
            string title = "Browse My Activities (" + count + ")";
            return new MenuItem(title, null);
        }


        ActivityDatabase activityDatabase;
    }
}
