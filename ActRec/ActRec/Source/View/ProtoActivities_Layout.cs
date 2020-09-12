using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisiPlacement;

namespace ActivityRecommendation.View
{
    public class ProtoActivities_Layout : ContainerLayout
    {
        public ProtoActivities_Layout(ProtoActivity_Database protoActivity_database, ActivityDatabase activityDatabase, LayoutStack layoutStack)
        {
            MenuLayoutBuilder menuBuilder = new MenuLayoutBuilder(layoutStack);
            menuBuilder.AddLayout("Enter new ProtoActivity", new ProtoActivity_LayoutBuilder(protoActivity_database, activityDatabase, layoutStack));
            menuBuilder.AddLayout("Browse Best ProtoActivities", new BrowseBest_ProtoActivities_Layout(protoActivity_database, activityDatabase, layoutStack));
            menuBuilder.AddLayout("View All ProtoActivities", new BrowseAll_ProtoActivities_Layout(protoActivity_database, activityDatabase, layoutStack));
            menuBuilder.AddLayout("Search ProtoActivities", new SearchProtoActivities_Layout(protoActivity_database, activityDatabase, layoutStack));
            menuBuilder.AddLayout("Help", new HelpWindowBuilder()
                .AddMessage("Here you can brainstorm things that you might want to do but that aren't yet formed well enough to be meaningful to suggest.")
                .AddMessage("Once an idea that you enter here does become worth suggesting, you can promote it from a ProtoActivity to an Activity")
                .Build());
            this.SubLayout = menuBuilder.Build();
        }
    }


    public class ProtoActivity_LayoutBuilder : ValueProvider<StackEntry>
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

        private ProtoActivity_Database protoActivity_database;
        private ActivityDatabase activityDatabase;
        private LayoutStack layoutStack;
    }

}
