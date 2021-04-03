using ActivityRecommendation.View;
using VisiPlacement;

// This file serves two pruposes:
// #1. It generates the features overview screen in the application, primarily for the purpose of getting users excited
// #2. It can describe the advantages of using ActivityRecommender to people that don't have the application installed

namespace ActivityRecommendation.View
{
    public class FeatureOverviewLayout
    {
        public static LayoutChoice_Set New(LayoutStack layoutStack)
        {
            TitledControl mainLayout = new TitledControl("I want...");
            MenuLayoutBuilder menuBuilder = new MenuLayoutBuilder(layoutStack);

            menuBuilder.AddLayout("To be happy now",
                new TitledControl("Happiness at any time!",
                    (new MenuLayoutBuilder(layoutStack))
                    .AddLayout("So many activity ideas", (new HelpWindowBuilder())
                        .AddMessage("To help you effortlessly get started, there are hundreds of wonderful, categorized activity ideas under Start -> Activities -> Import Some Premade Activities.")
                        .AddMessage("These ideas can supplement your own ideas that you can add under Start -> Activities -> New Activity.")
                        .Build()
                    )
                    .AddLayout("Awesome suggestions", (new HelpWindowBuilder())
                        .AddMessage("Go to Start -> Suggestions/Experiment for endless, personalized ideas!")
                        .AddMessage("If you like an idea, press the 'Doing it!' button which will take you straight to the Record Participations screen.")
                        .AddMessage("ActivityRecommender learns based on how you rate what you do, so be sure to enter a rating often.")
                        .AddMessage("If you press the X button on an idea, it will go away and will appear less often. If it's a very good idea, though, it might be back quite soon!")
                        .Build()
                    )
                    .AddLayout("Advanced: Efficiency Experiment", (new HelpWindowBuilder())
                        .AddMessage("Imagine you have some tasks you need to do, and you want to estimate how efficient you are at various times of day.")
                        .AddMessage("If you are willing to work on these tasks in a random order, then you can measure your efficiency! Doing them in a random order allows us to correct for the fact that some tasks " +
                        "may be harder than expected.")
                        .AddMessage("ActivityRecommender offers this feature under Start -> Suggest/Experiment -> Efficiency Experiment.")
                        .AddMessage("These efficiency measurements will show up in various places in the application, including in graphs!")
                        .Build()
                    )
                    .Build()
                )
            );
            menuBuilder.AddLayout("To be happy later",
                new TitledControl("Live a happy life!",
                    new MenuLayoutBuilder(layoutStack)
                    .AddLayout("Fulfilling suggestions", (new HelpWindowBuilder())
                        .AddMessage("ActivityRecommender's suggestions are optimized to make you happy for long periods of time. If something you do tends to make you slightly happy now and very unhappy later, such " +
                        "as staying up late, then as ActivityRecommender gets to know you better, it will suggest this less and less. Of course, if something makes you very happy now and slightly unhappy later, then " +
                        "you can expect ActivityRecommender to keep suggesting it.")
                        .AddMessage("You don't have to do anything special to enable this! Just keep recording ratings for the things that you do.")
                        .Build()
                    )
                    .AddLayout("Validating feedback: way to go!", (new HelpWindowBuilder())
                        .AddMessage("Even something as simple as recording your time here is really fun. ActivityRecommender can understand what makes you happy and when, and chooses accordingly from hundreds " +
                        "of possible responses!")
                        .AddMessage("Go to Start -> Record Participations to record what you're up to, and be sure to enter lots of ratings.")
                        .Build()
                    )
                    .AddLayout("Analysis: what makes you least happy?", (new HelpWindowBuilder())
                        .AddMessage("To see what has had the largest postive or negative impact on your happiness recently, go to Start -> Analyze -> Significant Activities.")
                        .AddMessage("This analysis will be weighted by the amount of time you spent on various things, so even a small problem that lasts for a long time can be considered significant!")
                        .Build()
                    )
                    .AddLayout("Analysis: reminisice", (new HelpWindowBuilder())
                        .AddMessage("If you want to browse happy things you have done in the past, you can go to Start -> Analyze -> Search Participations and request a 'random, probably good one'.")
                        .AddMessage("Of course, this will be more interesting after you've recorded more comments under Start -> Record Participations.")
                        .Build()
                    )
                    .AddLayout("Analysis: graphs", (new HelpWindowBuilder())
                        .AddMessage("Obviously, ActivityRecommender also provides interesting graphs of what you've been doing.")
                        .AddMessage("Go to Start -> Analyze -> Visualize one Activity to see!")
                        .Build()
                    )
                    .AddLayout("Summaries to share with friends", (new HelpWindowBuilder())
                        .AddMessage("ActivityRecommender even offers multiple types of summaries you can share with friends.")
                        .AddMessage("For a summary of what you've been up to lately, to share with old friends, see Start -> Analyze -> Life Story.")
                        .AddMessage("For a summary of what kinds of things you like to do, to share with new friends, see Start -> Analyze Favorite Activities.")
                        .Build()
                    )
                    .Build()
                )
            );
            menuBuilder.AddLayout("To be motivated and impactful",
                new TitledControl("Be powerful",
                    (new MenuLayoutBuilder(layoutStack))
                    .AddLayout("Great suggestions", (new HelpWindowBuilder())
                        .AddMessage("Go to Start -> Suggestions/Experiment for endless suggestions!")
                        .AddMessage("If a suggestion sounds like a difficult thing to do at first, try thinking about it for a few minutes. You might find that you would like it anyway!")
                        .Build()
                    )
                    .AddLayout("Measure and optimize your efficiency", (new HelpWindowBuilder())
                        .AddMessage("Imagine you have some tasks you need to do, and you want to estimate how efficient you are at various times of day.")
                        .AddMessage("If you are willing to work on these tasks in a random order, then you can measure your efficiency! Doing them in a random order allows us to correct for the fact that some tasks " +
                        "may be harder than expected.")
                        .AddMessage("ActivityRecommender offers this feature under Start -> Suggest/Experiment -> Efficiency Experiment.")
                        .AddMessage("These efficiency measurements will show up in various places in the application, including in suggestions and feedback.")
                        .Build()
                    )
                    .AddLayout("Save partial ideas to improve later", (new HelpWindowBuilder())
                        .AddMessage("Have you ever had an idea that you thought was really cool but not quite complete? Can you immediately recall all of the ideas you've ever had?")
                        .AddMessage("Have you ever tried writing your ideas down so you can remember them later? Have you ever found that your list quickly became too long and disorganized?")
                        .AddMessage("ActivityRecommender solves this problem by allowing you to save your ideas, compare them, edit them, browse your top ideas, promote them into Activities, and delete them.")
                        .AddMessage("These ideas are called ProtoActivities (because they turn into Activities) and can be found at Start -> Activities -> Brainstorm Protoactivities")
                        .AddMessage("If you ever have an idea you want to remember later, just make a ProtoActivity. If you ever need inspiration, just browse your best protoactivities.")
                        .Build()
                    )
                    .Build()
                )
            );

            menuBuilder.AddLayout("Something free and convenient",
                new TitledControl("Free!",
                    (new MenuLayoutBuilder(layoutStack))
                    .AddLayout("No fees, no ads", (new HelpWindowBuilder())
                        .AddMessage("There are no advertisements in ActivityRecommender, and there is no fee to install it either.")
                        .AddMessage("Why?")
                        .AddMessage("1. I created ActivityRecommender in 2011 primarily to help motivate myself. I really really really hope you like it too, but the success of the project isn't dependent " +
                        "on getting anyone to pay for it.")
                        .AddMessage("2. I think it would be extremely cool if you like and use ActivityRecommender. That means I've improved your life, right? I don't want to risk interfering with that by " +
                        "charging money for it or including any distractions in it.")
                        .AddMessage("3. ActivityRecommender doesn't require any money to keep a server running, because all the data and processing happen on your device.")
                        .Build()
                    )
                    .AddLayout("Fast interface, lots of autocomplete", (new HelpWindowBuilder())
                        .AddMessage("ActivityRecommender has autocomplete in many places.")
                        .AddMessage("When entering an activity name, press Enter to accept the autocomplete suggestion.")
                        .AddMessage("It's common to be able to record a participation by pressing the screen only six times (One press to select the name box, two presses to type the first two letters, " +
                        "one press of Enter to select the autocomplete suggestion, one press of \"End = Now\" to select and end time, and one press of OK to record it).")
                        .Build()
                    )
                    .AddLayout("Not trying to make you use it more", (new HelpWindowBuilder())
                        .AddMessage("ActivityRecommender has a deep understanding of how happy you are with your life, and that's what it optimizes.")
                        .AddMessage("ActivityRecommender doesn't assume that using it more is a sign of being happier.")
                        .AddMessage("In fact, any time you spend using ActivityRecommender is time that you don't spend doing something else.")
                        .AddMessage("So, on the Suggestions screen, if you ever push the X button, then ActivityRecommender assumes that while that " +
                        "activity was onscreen, you were spending all of your brainpower contemplating whether to do that activity, and that that " +
                        "time was worth 0 happiness to you.")
                        .AddMessage("ActivityRecommender will only attempt to use more of your time when it estimates that your resultant increase in " +
                        "happiness will be enough to compensate (for example, when you ask for a suggestion, it may suggest that you do your chores even " +
                        "if it thinks there is only a small chance that you will take the suggestion).")
                        .Build()
                    )
                    .AddLayout("All runs on the device", (new HelpWindowBuilder())
                        .AddMessage("No internet is required. Really!")
                        .AddMessage("On-device processing is pretty fast.")
                        .AddMessage("ActivityRecommender itself does not send your data to other devices. Of course, if your device automatically makes backup of application data, then those " +
                        "backups may contain copies of your ActivityRecommender data. You can still use the Export functionality within ActivityRecommender to save a data snapshot to the " +
                        "device, which you can then separately save.")
                        .Build()
                    )
                    .AddLayout("Open source", new Vertical_GridLayout_Builder()
                        .AddLayout(new TextblockLayout("Visit https://github.com/mathjeff/ActivityRecommender for more information. If you're thinking about contributing, start by opening an issue. Thanks!"))
                        .AddLayout(OpenIssue_Layout.New())
                        .Build()
                    )
                    .AddLayout("But feel free to thank us", (new HelpWindowBuilder())
                        .AddMessage("If you tell us that we've done something that you like, that will make us very happy :)")
                        .AddMessage("If you describe something that you would like changed, then that will help us to make improvements, and if you tell us that you like those improvements, that will make " +
                        "us very happy :) . We'll probably even add your name inside the application as an acknowledgement!")
                        .AddMessage("Getting feedback makes us more productive! Believe us, we measured it: about 30-60 minutes for the first instance each day.")
                        .AddMessage("If you would like to give us feedback, good options include leaving a review on the application that you downloaded from the store, or opening a bug.")
                        .AddMessage("Thanks!")
                        .Build()
                    )
                    .Build()
                )
            );

            mainLayout.SetContent(menuBuilder.Build());
            return mainLayout;
        }
    }
}
