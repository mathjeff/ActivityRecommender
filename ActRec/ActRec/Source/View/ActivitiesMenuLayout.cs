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
            LayoutChoice_Set importPremadeActivitiesLayout,
            LayoutChoice_Set addOrEditActivitiesLayout,
            LayoutChoice_Set protoactivitiesLayout,
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
                .AddLayout(importEntry)
                .AddLayout(addEntry)
                .AddLayout(protoactivitiesEntry)
                .AddLayout(helpEntry);
            this.noActivitiesLayout = noActivitiesBuilder.Build();

            MenuLayoutBuilder fullBuilder = new MenuLayoutBuilder(this.layoutStack)
                .AddLayout(browseEntry)
                .AddLayout(importEntry)
                .AddLayout(addEntry)
                .AddLayout(protoactivitiesEntry)
                .AddLayout(helpEntry);
            this.fullLayout = fullBuilder.Build();
        }

        LayoutStack layoutStack;
        LayoutChoice_Set browseInheritancesLayout;
        LayoutChoice_Set importPremadeActivitiesLayout;
        LayoutChoice_Set addOrEditActivitiesLayout;
        LayoutChoice_Set protoactivitiesLayout;
        LayoutChoice_Set helpLayout;

        ActivityDatabase activityDatabase;

        LayoutChoice_Set noActivitiesLayout;
        LayoutChoice_Set fullLayout;

    }
}
