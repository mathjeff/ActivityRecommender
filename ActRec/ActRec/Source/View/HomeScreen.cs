using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;

namespace ActivityRecommendation.View
{
    class HomeScreen : ContainerLayout
    {
        public HomeScreen(
            LayoutChoice_Set activitiesLayout,
            LayoutChoice_Set participationsLayout,
            LayoutChoice_Set suggestionsLayout,
            LayoutChoice_Set statisticsLayout,
            LayoutChoice_Set importExportLayout,
            ActivityDatabase activityDatabase,
            LayoutStack layoutStack)
        {
            this.activitiesLayout = activitiesLayout;
            this.participationsLayout = participationsLayout;
            this.suggestionsLayout = suggestionsLayout;
            this.statisticsLayout = statisticsLayout;
            this.importExportLayout = importExportLayout;
            this.activityDatabase = activityDatabase;
            this.layoutStack = layoutStack;

            this.setupOptions();
        }

        public override SpecificLayout GetBestLayout(LayoutQuery query)
        {
            if (this.activityDatabase.ContainsCustomActivity())
            {
                if (this.activityDatabase.RootActivity.NumParticipations > 0)
                    this.SubLayout = this.fullLayout;
                else
                    this.SubLayout = this.noParticipations_layout;
            }
            else
            {
                this.SubLayout = this.noActivities_layout;
            }
            return base.GetBestLayout(query);
        }
        private void setupOptions()
        {
            StackEntry activitiesEntry = new StackEntry(this.activitiesLayout, "Activities", null);
            StackEntry participationsEntry = new StackEntry(this.participationsLayout, "Record Participations", null);
            StackEntry suggestionsEntry = new StackEntry(this.suggestionsLayout, "Get Suggestions", null);
            StackEntry statisticsEntry = new StackEntry(this.statisticsLayout, "View Statistics", null);
            StackEntry importExportEntry = new StackEntry(this.importExportLayout, "Import/Export", null);

            MenuLayoutBuilder noActivities_builder = new MenuLayoutBuilder(this.layoutStack);
            noActivities_builder.AddLayout(activitiesEntry);
            noActivities_builder.AddLayout(importExportEntry);
            this.noActivities_layout = noActivities_builder.Build();

            MenuLayoutBuilder noParticipations_builder = new MenuLayoutBuilder(this.layoutStack);
            noParticipations_builder.AddLayout(activitiesEntry);
            noParticipations_builder.AddLayout(participationsEntry);
            noParticipations_builder.AddLayout(suggestionsEntry);
            noParticipations_builder.AddLayout(importExportEntry);
            this.noParticipations_layout = noParticipations_builder.Build();

            MenuLayoutBuilder fullBuilder = new MenuLayoutBuilder(this.layoutStack);
            fullBuilder.AddLayout(activitiesEntry);
            fullBuilder.AddLayout(participationsEntry);
            fullBuilder.AddLayout(suggestionsEntry);
            fullBuilder.AddLayout(statisticsEntry);
            fullBuilder.AddLayout(importExportEntry);
            this.fullLayout = fullBuilder.Build();
        }
        private LayoutChoice_Set activitiesLayout;
        private LayoutChoice_Set participationsLayout;
        private LayoutChoice_Set suggestionsLayout;
        private LayoutChoice_Set statisticsLayout;
        private LayoutChoice_Set importExportLayout;

        private ActivityDatabase activityDatabase;
        private LayoutStack layoutStack;

        private LayoutChoice_Set noActivities_layout;
        private LayoutChoice_Set noParticipations_layout;
        private LayoutChoice_Set fullLayout;
    }
}
