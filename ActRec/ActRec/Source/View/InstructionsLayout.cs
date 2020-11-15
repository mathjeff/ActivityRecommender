using System.Dynamic;
using VisiPlacement;

// This file serves two purposes:
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

            menuBuilder.AddLayout("What's awesome?", FeatureOverviewLayout.New(layoutStack));

            menuBuilder.AddLayout("How do I navigate?", makeNavigationHelp_layout(layoutStack));

            menuBuilder.AddLayout("How do I use ActivityRecommender?", (new HelpWindowBuilder())
                .AddMessage("Step 1: Get excited. To see a list of key features of ActivityRecommender so you can decide whether they interest you, go back and choose \"Get Excited\". Everything " +
                "else mentioned here is under the \"Start\" menu option.")
                .AddMessage("Step 2: Add some activities. Everything that ActivityRecommender does is based on the activities that you tell it about. Think of some activities that you like to do, " +
                "and enter their names. The only activity that exists initially is the built-in activity named Activity. Each other must have a parent activity. For example, if you " +
                "create a new activity named Exercise whose parent is Activity, you can then create a new activity named Frisbee whose parent is Exercise. The main advantage to categorizing " +
                "activities like this is it allows ActivityRecommender to know that they are related and enables ActivityRecommender to give better suggestions. Additionally, this also allows you " +
                "to later view a graph of your total time spent in Exercise, which would then include Frisbee.")
                .AddMessage("Step 3: Record some participations. Whereas an activity is something like \"Frisbee\", a participation is something like \"I played Frisbee today from 14:00 to 15:00 " +
                "and it was 1.1 times as much fun as having watched television earlier today.\" For ActivityRecommender to know what you like to do, it needs some information about what " +
                "you have done in the past, along with how much you liked it. Before you can report having played Frisbee, you must have recorded that Frisbee is an activity.")
                .AddMessage("Step 4a: Ask for suggestions. Once you've entered at least one activity, ActivityRecommender will be able to give you activity recommendations. As you enter more data, " +
                "ActivityRecommender will be able to give you better and better suggestions.")
                .AddMessage("Step 4b: Export a backup of your data if it's important to you.")
                .AddMessage("Step 4c: Explore! ActivityRecommender has more features than are mentioned here :)")
                .Build());

            menuBuilder.AddLayout("Discoveries made with ActivityRecommender", DiscoveriesLayout.New(layoutStack));

            menuBuilder.AddLayout("Credits", new CreditsWindowBuilder(layoutStack)
                .AddContribution(ActRecContributor.ANNI_ZHANG, new System.DateTime(2019, 8, 18), "Discussed the organization of the introduction and get-excited screens")
                .Build());


            mainLayout.SetContent(menuBuilder.Build());

            return mainLayout;
        }

        private static LayoutChoice_Set makeNavigationHelp_layout(LayoutStack layoutStack)
        {
            LayoutChoice_Set result = new TitledControl("Navigating ActivityRecommender",
                new HelpWindowBuilder()
                    .AddMessage("To find your way around in ActivityRecommender, do this:")
                    .AddMessage("1. Decide what you want to do with ActivityRecommender. If you don't know what ActivityRecommender can do, then " +
                    "go back and visit the Get Excited or How to Use screens.")
                    .AddMessage("2. Look at the current screen and try to determine whether it seems capable of doing what you want.")
                    .AddMessage("2a. If you're not sure, look for a button labelled Help and press it.")
                    .AddLayout(
                        new HelpButtonLayout("Help",
                            new TextblockLayout("The screen you were reading explains how to find things in ActivityRecommender, and you successfully found the Help button. Congratulations!"),
                            layoutStack
                        )
                    )
                    .AddMessage("2b. If you're still not sure, you can ask for help here:")
                    .AddLayout(OpenIssue_Layout.New())
                    .AddMessage("3. Whenever the current screen looks promising, press a button and see what happens!")
                    .AddMessage("4. Whenever the current screen doesn't seem like what you want, push one of the back buttons at the bottom of the screen")
                    .AddLayout(
                        new HelpButtonLayout("Tell me more about the back buttons",
                            (new HelpWindowBuilder()
                                .AddLayout(
                                    new HelpWindowBuilder()
                                    .AddMessage("As you visit more screens, you will see more buttons at the bottom of the screen. Each button will jump you back to the screen " +
                                        "that it represents. Press one!")
                                    .AddMessage("You may notice that the very first screen in ActivityRecommender doesn't usually get a back button. That's because you will almost " +
                                        "never want to return to it, so we only show you its back button when it's the only screen left to go back to.")
                                    .Build()
                                )
                                .AddLayout(
                                    new HelpButtonLayout("How many back buttons can there be?",
                                        new TextblockLayout("The maximum number of back buttons you can see onscreen at once depends on the size of your screen and the length of " +
                                        "the words in them. How many do you see now?"),
                                        layoutStack
                                    )
                                )
                                .Build()
                            ),
                            layoutStack
                        )
                    )
                .Build()
            );


            return result;
        }
    }
}
