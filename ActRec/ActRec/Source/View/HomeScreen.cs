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
            LayoutStack layoutStack)
        {
            this.activitiesLayout = activitiesLayout;
            this.participationsLayout = participationsLayout;
            this.suggestionsLayout = suggestionsLayout;
            this.statisticsLayout = statisticsLayout;
            this.importExportLayout = importExportLayout;
            this.layoutStack = layoutStack;

            this.setup();
        }

        private void setup()
        {
            StackEntry activitiesEntry = new StackEntry(this.activitiesLayout, "Activities", null);
            StackEntry participationsEntry = new StackEntry(this.participationsLayout, "Record Participations", null);
            StackEntry suggestionsEntry = new StackEntry(this.suggestionsLayout, "Get Suggestions", null);
            StackEntry statisticsEntry = new StackEntry(this.statisticsLayout, "View Statistics", null);
            StackEntry importExportEntry = new StackEntry(this.importExportLayout, "Import/Export", null);

            MenuLayoutBuilder fullBuilder = new MenuLayoutBuilder(this.layoutStack);
            fullBuilder.AddLayout(activitiesEntry);
            fullBuilder.AddLayout(participationsEntry);
            fullBuilder.AddLayout(suggestionsEntry);
            fullBuilder.AddLayout(statisticsEntry);
            fullBuilder.AddLayout(importExportEntry);
            this.SubLayout = fullBuilder.Build();
        }
        private LayoutChoice_Set activitiesLayout;
        private LayoutChoice_Set participationsLayout;
        private LayoutChoice_Set suggestionsLayout;
        private LayoutChoice_Set statisticsLayout;
        private LayoutChoice_Set importExportLayout;

        private LayoutStack layoutStack;

    }
}
