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
        }

        public override SpecificLayout GetBestLayout(LayoutQuery query)
        {
            if (this.activityDatabase.ContainsCustomActivity() != this.containedCustomActivity)
            {
                this.update();
            }
            else
            {
                if (this.SubLayout == null)
                    this.update();
            }
            return base.GetBestLayout(query);
        }
        private void update()
        {
            bool containsCustomActivity = this.activityDatabase.ContainsCustomActivity();
            MenuLayoutBuilder usageMenu_builder = new MenuLayoutBuilder(this.layoutStack);
            usageMenu_builder.AddLayout("Activities I Like", this.activitiesLayout);
            if (containsCustomActivity)
            {
                usageMenu_builder.AddLayout("Record Participations", this.participationsLayout);
                usageMenu_builder.AddLayout("What Do I Do Now?", this.suggestionsLayout);
                usageMenu_builder.AddLayout("View Statistics", this.statisticsLayout);
            }
            usageMenu_builder.AddLayout("Import/Export", this.importExportLayout);

            LayoutChoice_Set usageMenu = usageMenu_builder.Build();
            this.SubLayout = usageMenu;

            this.containedCustomActivity = containsCustomActivity;
        }
        private LayoutChoice_Set activitiesLayout;
        private LayoutChoice_Set participationsLayout;
        private LayoutChoice_Set suggestionsLayout;
        private LayoutChoice_Set statisticsLayout;
        private LayoutChoice_Set importExportLayout;
        private ActivityDatabase activityDatabase;
        private LayoutStack layoutStack;
        private bool containedCustomActivity = false;
    }
}
