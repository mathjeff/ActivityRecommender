using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisiPlacement;

namespace ActivityRecommendation.View
{
    class ProtoActivities_Layout : TitledControl
    {
        public ProtoActivities_Layout(ProtoActivity_Database protoActivity_database, ActivityDatabase activityDatabase, LayoutStack layoutStack, PublicFileIo publicFileIo, TextConverter textConverter)
        {
            this.SetTitle("Protoactivities");

            this.protoActivity_database = protoActivity_database;
            MenuLayoutBuilder menuBuilder = new MenuLayoutBuilder(layoutStack);
            this.newProtoactivityBuilder = new ProtoActivity_LayoutBuilder(protoActivity_database, activityDatabase, layoutStack);
            menuBuilder.AddLayout(
                new AppFeatureCount_ButtonName_Provider("New", this.newProtoactivityBuilder.GetFeatures()),
                this.newProtoactivityBuilder
            );

            this.browseBestProtoactivities_layout = new BrowseBest_ProtoActivities_Layout(protoActivity_database, activityDatabase, layoutStack);
            menuBuilder.AddLayout(
                new AppFeatureCount_ButtonName_Provider("Browse Best", this.browseBestProtoactivities_layout.GetFeatures()),
                new StackEntry(this.browseBestProtoactivities_layout, "Browse Best", null));


            this.listLayout = new BrowseAll_ProtoActivities_Layout(protoActivity_database, activityDatabase, layoutStack);

            menuBuilder.AddLayout(
                new AppFeatureCount_ButtonName_Provider("List All", listLayout.GetFeatures()),
                new StackEntry(listLayout, "List All", null));

            this.searchLayout = new SearchProtoActivities_Layout(protoActivity_database, activityDatabase, layoutStack);

            menuBuilder.AddLayout(
                new AppFeatureCount_ButtonName_Provider("Search", this.searchLayout.GetFeatures()),
                new StackEntry(searchLayout, "Search", null));

            this.exportProtoactivities_layout = new ExportProtoactivities_Layout(protoActivity_database, publicFileIo, textConverter, layoutStack);
            menuBuilder.AddLayout(
                new AppFeatureCount_ButtonName_Provider("Export", this.exportProtoactivities_layout.GetFeatures()),
                new StackEntry(this.exportProtoactivities_layout, "Export", null));
            menuBuilder.AddLayout("Help", new HelpWindowBuilder()
                .AddMessage("Here you can brainstorm things that you might want to do but that aren't yet formed well enough to be meaningful to suggest.")
                .AddMessage("Once an idea that you enter here does become worth suggesting, you can promote it from a ProtoActivity to an Activity")
                .Build());
            this.SetContent(menuBuilder.Build());
        }

        public List<AppFeature> GetFeatures()
        {
            List<AppFeature> features = new List<AppFeature>(this.newProtoactivityBuilder.GetFeatures());
            features.AddRange(this.browseBestProtoactivities_layout.GetFeatures());
            features.AddRange(this.listLayout.GetFeatures());
            features.AddRange(this.searchLayout.GetFeatures());
            features.AddRange(this.exportProtoactivities_layout.GetFeatures());
            return features;
        }

        ProtoActivity_Database protoActivity_database;
        ProtoActivity_LayoutBuilder newProtoactivityBuilder;
        BrowseBest_ProtoActivities_Layout browseBestProtoactivities_layout;
        ExportProtoactivities_Layout exportProtoactivities_layout;
        BrowseAll_ProtoActivities_Layout listLayout;
        SearchProtoActivities_Layout searchLayout;
    }


    class ProtoActivity_LayoutBuilder : ValueProvider<StackEntry>
    {
        public ProtoActivity_LayoutBuilder(ProtoActivity_Database protoActivity_Database, ActivityDatabase activityDatabase, LayoutStack layoutStack)
        {
            this.protoActivity_database = protoActivity_Database;
            this.activityDatabase = activityDatabase;
            this.layoutStack = layoutStack;
        }

        public StackEntry Get()
        {
            ProtoActivity protoActivity = new ProtoActivity(null, DateTime.Now, Distribution.Zero);
            ProtoActivity_Editing_Layout layout = new ProtoActivity_Editing_Layout(protoActivity, this.protoActivity_database, this.activityDatabase, this.layoutStack);
            return new StackEntry(layout, "Proto", layout);
        }

        public List<AppFeature> GetFeatures()
        {
            return new List<AppFeature>() { new CreateProtoactivity_Feature(this.protoActivity_database) };
        }

        private ProtoActivity_Database protoActivity_database;
        private ActivityDatabase activityDatabase;
        private LayoutStack layoutStack;
    }

    class CreateProtoactivity_Feature : AppFeature
    {
        public CreateProtoactivity_Feature(ProtoActivity_Database protoactivityDatabase)
        {
            this.protoactivityDatabase = protoactivityDatabase;
        }
        public string GetDescription()
        {
            return "Create a ProtoActivity";
        }
        public bool GetHasBeenUsed()
        {
            return this.protoactivityDatabase.Count > 0;
        }
        public bool GetIsUsable()
        {
            return true;
        }

        ProtoActivity_Database protoactivityDatabase;
    }

}
