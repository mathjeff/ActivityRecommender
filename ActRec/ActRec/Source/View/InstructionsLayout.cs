using System.Dynamic;
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

            menuBuilder.AddLayout("What's awesome about ActivityRecommender?", FeatureOverviewLayout.New(layoutStack));

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

            menuBuilder.AddLayout("Credits", new CreditsWindowBuilder(layoutStack)
                .AddContribution(ActRecContributor.ANNI_ZHANG, new System.DateTime(2019, 8, 18), "Discussed the organization of the introduction and get-excited screens")
                .Build());

            mainLayout.SetContent(menuBuilder.Build());

            return mainLayout;
        }

        private static LayoutChoice_Set makeNavigationHelp_layout(LayoutStack layoutStack)
        {
            LayoutChoice_Set backButtonExplanationLayout = new HelpWindowBuilder()
                .AddMessage("If nothing onscreen looks interesting to you, then go back! Have you seen the handy buttons at the bottom of the screen?")
                .AddLayout(
                    new HelpButtonLayout("I see the back buttons!",
                        (new HelpWindowBuilder()
                            .AddMessage("Great! Press one of the buttons at the bottom of the screen to go back to an earlier screen.")
                            .AddMessage("The button will tell you which screen it returns you to.")
                            .AddMessage("We try to fit a lot of these back buttons on your screen at once. However, sometimes not all of the buttons fit, " +
                            "so we might not be able to show you a button for every screen that you can go back to. " +
                            "If you'd like to go back further, just press one of the back buttons, and then on the new screen look for new back buttons.")
                            .Build()
                        ),
                        layoutStack
                    )
                )
                .AddLayout(
                    new HelpButtonLayout("What buttons?",
                        (
                            new HelpWindowBuilder()
                            .AddMessage("There! At the bottom! The buttons changed! Do you see those buttons?")
                            .AddLayout(
                                new HelpButtonLayout("Oh, those buttons",
                                    new TextblockLayout("Yes! Now press a button to go back to a previous screen. The words on the button will " +
                                    "tell you which screen they go to."),
                                    layoutStack
                                )
                            )
                            .AddLayout(
                                new HelpButtonLayout("Nope, I'm still confused.",
                                    (new HelpWindowBuilder()
                                        .AddMessage("Ok, you're very funny.")
                                        .AddMessage("We've now removed all the other buttons. You have to push one of the back buttons at the bottom of the screen.")
                                        .Build()
                                    ),
                                    layoutStack
                                )
                            )
                            .Build()
                        ),
                        layoutStack
                    )
                )
                .Build();

            LayoutChoice_Set forwardButtonExplanationLayout = new HelpWindowBuilder()
                .AddMessage("If the information on your screen seems related to what you want but is too general, then " +
                "look at the buttons onscreen. Do any of them look promising? ")
                .AddMessage("Press a button and see what happens!")
                .AddMessage("For example, trying pressing this button:")
                .AddLayout(
                    new HelpButtonLayout("Compliment Me!",
                        new TextblockLayout("Wow! You're doing a great job reading all of the instructions. I think you will do good things!"),
                        layoutStack
                    )
                )
                .Build();

            LayoutChoice_Set helpButtonExplanationLayout =
                new HelpWindowBuilder()
                .AddMessage("If you are looking at a screen in ActivityRecommender and you don't know what it means, you can:")
                .AddMessage("A) Search for the button labelled 'Help' and push it. Most screens in ActivityRecommender offer help!.")
                .AddMessage("B) Ask for help! Tell us about that something is confusing, by pushing this button:")
                .AddLayout(OpenIssue_Layout.New())
                .Build();

            LayoutChoice_Set howActivityRecommenderWorksLayout = new TextblockLayout("If you're not sure what you want ActivityRecommender to do, then " +
                "you should first look at the overview of what ActivityRecommender can do. Go back and select 'How do I use ActivityRecommender?'");

            LayoutChoice_Set result = new TitledControl("How to navigate ActivityRecommender!\nNavigating ActivityRecommender depends on what you want and which screen you are on.",
                (new MenuLayoutBuilder(layoutStack)
                    .AddLayout("When the screen doesn't contain what you want, go back!", backButtonExplanationLayout)
                    .AddLayout("When the screen is too general, go forward!", forwardButtonExplanationLayout)
                    .AddLayout("When you're not sure whether the screen contains what you want, check for help!", helpButtonExplanationLayout)
                    .AddLayout("When you're unsure what you want, read the feature overview!", howActivityRecommenderWorksLayout)
                    .Build()
                )
            );

            return result;
        }
    }
}
