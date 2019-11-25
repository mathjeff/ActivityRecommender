using VisiPlacement;

// This file serves two pruposes:
// #1. It generates the instructions screen in the application
// #2. It can describe the usage instructions to people that don't have the application installed

namespace ActivityRecommendation.View
{
    public class InstructionsLayout
    {
        public static LayoutChoice_Set New(LayoutStack layoutStack)
        {
            TitledControl mainLayout = new TitledControl("ActivityRecommender Intro:");

            MenuLayoutBuilder menuBuilder = new MenuLayoutBuilder(layoutStack);

            menuBuilder.AddLayout("List exciting features", FeatureOverviewLayout.New(layoutStack));

            menuBuilder.AddLayout("Navigation", (new HelpWindowBuilder())
                .AddMessage("Your device should have Back button. Press it from any screen to go back.")
                .AddMessage("")
                .AddMessage("Screens in ActivityRecommender are arranged hierarchically by topic.")
                .AddMessage("This means that each screen in ActivityRecommender provides less detail but a larger breadth of information than any " +
                "of the screens inside it. Consequently, if you are looking for a particular piece of information on one screen but nothing on " +
                "that screen seems related, then you should press Back to return to a more general page and continue looking.")
                .Build());

            menuBuilder.AddLayout("Usage Overview", (new HelpWindowBuilder())
                .AddMessage("Step 1: Get excited. To see a list of key features of ActivityRecommender so you can decide whether they interest you, go back and choose \"Get Excited\". Everything " +
                "else mentioned here is under the \"Start\" menu option.")
                .AddMessage("Step 2: Add some activities. Everything that ActivityRecommender does is based on the activities that you tell it about. Think of some activities that you like to do, " +
                "and enter their names. The only activity that exists initially is the built-in activity named Activity. Each other must have a parent activity. For example, if you " +
                "create a new activity named Exercise whose parent is Activity, you can then create a new activity named Frisbee whose parent is Exercise. The advantage to categorizing " +
                "activities like this is it allows ActivityRecommender to know that they are related and enables ActivityRecommender to give better suggestions.")
                .AddMessage("Step 3: Record some participations. Whereas an activity is something like \"Frisbee\", a participation is something like \"I played Frisbee today from 14:00 to 15:00 " +
                "and it was 1.1 times as much fun as having watched television earlier today.\" For ActivityRecommender to know what you like to do, it needs some information about what " +
                "you have done in the past, along with how much you liked it. Before you can report having played Frisbee, you must have recorded that Frisbee is an activity.")
                .AddMessage("Step 4a: Ask for suggestions. Once you've entered at least one activity, ActivityRecommender will be able to give you activity recommendations. As you enter more data, " +
                "ActivityRecommender will be able to give you better and better suggestions.")
                .AddMessage("Step 4b: Export a backup of your data if it's important to you.")
                .AddMessage("Step 4c: Explore! ActivityRecommender has more features than are mentioned here :)")
                .Build());

            mainLayout.SetContent(menuBuilder.Build());

            return mainLayout;
        }
    }
}
