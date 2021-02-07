using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;

namespace ActivityRecommendation.View
{
    class HomeScreen : ContainerLayout
    {
        public HomeScreen(
            ActivitiesMenuLayout activitiesLayout,
            ParticipationEntryView participationsLayout,
            SuggestionsView suggestionsLayout,
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
            StackEntry suggestionsEntry = new StackEntry(this.suggestionsLayout, "Suggest/Experiment", this.suggestionsLayout);
            StackEntry statisticsEntry = new StackEntry(this.statisticsLayout, "Analyze", null);
            StackEntry importExportEntry = new StackEntry(this.importExportLayout, "Import/Export", null);

            MenuLayoutBuilder fullBuilder = new MenuLayoutBuilder(this.layoutStack);
            fullBuilder.AddLayout(new AppFeatureCount_ButtonName_Provider("Activities", this.activitiesLayout.GetFeatures()), activitiesEntry);
            fullBuilder.AddLayout(new AppFeatureCount_ButtonName_Provider("Record Participations", this.participationsLayout.GetFeatures()), participationsEntry);
            fullBuilder.AddLayout(new AppFeatureCount_ButtonName_Provider("Suggest/Experiment", this.suggestionsLayout.GetFeatures()), suggestionsEntry);
            fullBuilder.AddLayout(statisticsEntry);
            fullBuilder.AddLayout(importExportEntry);
            this.SubLayout = fullBuilder.Build();
        }
        public List<AppFeature> GetFeatures()
        {
            List<AppFeature> features = new List<AppFeature>(this.activitiesLayout.GetFeatures());
            features.AddRange(this.participationsLayout.GetFeatures());
            features.AddRange(this.suggestionsLayout.GetFeatures());
            return features;
        }
        private ActivitiesMenuLayout activitiesLayout;
        private ParticipationEntryView participationsLayout;
        private SuggestionsView suggestionsLayout;
        private LayoutChoice_Set statisticsLayout;
        private LayoutChoice_Set importExportLayout;

        private LayoutStack layoutStack;

    }
}
