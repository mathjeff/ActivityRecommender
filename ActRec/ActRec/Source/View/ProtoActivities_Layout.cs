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
        public ProtoActivities_Layout(ProtoActivity_Database database, LayoutStack layoutStack)
        {
            MenuLayoutBuilder menuBuilder = new MenuLayoutBuilder(layoutStack);
            menuBuilder.AddLayout("Enter new ProtoActivity", new ProtoActivity_LayoutBuilder(database));
            menuBuilder.AddLayout("Browse ProtoActivities", new Browse_ProtoActivities_Layout(database, layoutStack));
            menuBuilder.AddLayout("Help", new HelpWindowBuilder()
                .AddMessage("Here you can brainstorm things that you might want to do but that aren't yet formed well enough to be meaningful to suggest.")
                .AddMessage("Once an idea that you enter here does become worth suggesting, you can promote it from a ProtoActivity to an Activity")
                .Build());
            this.SubLayout = menuBuilder.Build();
        }
    }


    public class ProtoActivity_LayoutBuilder : ValueProvider<StackEntry>
    {
        public ProtoActivity_LayoutBuilder(ProtoActivity_Database activityDatabase)
        {
            this.protoActivity_database = activityDatabase;
        }

        public StackEntry Get()
        {
            ProtoActivity protoActivity = new ProtoActivity(null, DateTime.Now, new Distribution());
            this.protoActivity_database.Put(protoActivity);
            ProtoActivity_Editing_Layout layout = new ProtoActivity_Editing_Layout(protoActivity, this.protoActivity_database);
            return new StackEntry(layout, layout);
        }

        private ProtoActivity_Database protoActivity_database;
    }

}
