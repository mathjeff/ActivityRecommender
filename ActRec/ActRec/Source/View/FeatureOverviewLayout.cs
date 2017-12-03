using VisiPlacement;

// This file serves two pruposes:
// #1. It generates the features overview screen in the application
// #2. It can describe the features to people that don't have the application installed

namespace ActivityRecommendation
{
    public class FeatureOverviewLayout
    {
        public static LayoutChoice_Set New(LayoutStack layoutStack)
        {
            TitledControl mainLayout = new TitledControl("Highlights of ActivityRecommender:");

            MenuLayoutBuilder menuBuilder = new MenuLayoutBuilder(layoutStack);

            menuBuilder.AddLayout("Personalized", (new HelpWindowBuilder())
                .AddMessage("You decide which activities exist in your world and what their relationships are.")
                .AddMessage("If your icecream store is far away and you want to declare that \"Buying Ice Cream\" is \"Exercise\", that's your prerogative.")
                .Build());

            menuBuilder.AddLayout("Maximizes longterm happiness", (new HelpWindowBuilder())
                .AddMessage("Several aspects of ActivityRecommender are designed to maximize your longterm happiness.")
                .AddMessage("Among other things, this means that it should encourage you towards entertainment that recharges rather than frustrates you.")
                .AddMessage("The way that this works is you gradually enter more and more data about things you've done and how happy they have made you.")
                .AddMessage("In the meanwhile, ActivityRecommender builds an increasingly accurate model of your happiness.")

                .AddMessage("At any given time, it uses this model to predict and attempt to maximize the net present value (in the economic sense) of how much happiness you'll experience " +
                "in the future. This is approximately equivalent to maximizing the amount of happiness that you'll experience over the next " +
                (UserPreferences.DefaultPreferences.HalfLife.TotalDays / 365) + " years.")

                .AddMessage("So even if you don't like doing chores, if ActivityRecommender notices that doing chores from time to time enables you to have more time to do other fun " +
                "things, it will recommend chores from time to time anyway.")
                .Build());

            menuBuilder.AddLayout("Fast feedback", (new HelpWindowBuilder())
                .AddMessage("Every time you record having done an activity, ActivityRecommender gives you feedback even before you press OK.")
                .AddMessage("It builds a model of your longterm happiness and how it is expected to be affected by having done various activities.")
                .AddMessage("As soon as you enter an activity name on the Record Participations screen, you will see an estimate of the difference between your expected longterm happiness " +
                "with that participation vs without that participation.")
                .AddMessage("For example, suppose that you exercised every week last year and recorded that you were twice as happy as the year before in which you did no exercising. " +
                "If you then record another participation of exercise, ActivityRecommender may tell you that your participation in exercise is correlated with an increase in " +
                "happiness measured in hundreds of days.")
                .Build());

            menuBuilder.AddLayout("Meaningful relative ratings", (new HelpWindowBuilder())
                .AddMessage("It's hard to admit that an activity was boring, a mistake, inefficient, or otherwise regrettable, let alone say it on a daily basis.")
                .AddMessage("So, ActivityRecommender doesn't ever ask you to say that. ActivityRecommender never asks you to enter precisely how much fun you had doing a certain activity.")
                .AddMessage("ActivityRecommender only ever asks you to compare two different participations.")
                .AddMessage("Comparing two participations is pretty noncommital but still gives ActivityRecommender plenty of information to extract meaningful relationships.")
                .AddMessage("For example, if you repeatedly report that listening to music is 1.5 times as enjoyable doing chores, then if you discover a new TV show and report that " +
                "watching the first episode was 1.1 times as much fun as listening to music, then ActivityRecommender may estimate that watching that episode may have been 1.1*1.5=1.65 " +
                "times as much fun as doing chores would have been.")
                .Build());

            menuBuilder.AddLayout("Fast UI with lots of autocomplete", (new HelpWindowBuilder())
                .AddMessage("ActivityRecommender has autocomplete in many places.")
                .AddMessage("When entering an activity name, press Enter to accept the autocomplete suggestion.")
                .AddMessage("It's common to be able to record a participation by pressing the screen only six times (One press to select the name box, two presses to type the first two letters, one press of Enter to select the autocomplete suggestion, one press of " +
                "\"End = Now\" to select and end time, and one press of OK to record it).")
                .Build());

            menuBuilder.AddLayout("Provides suggestions", (new HelpWindowBuilder())
                .AddMessage("The feature that ActivityRecommender is named for, of course, is that you can ask it for suggestions of what to do.")
                .AddMessage("ActivityRecommender doesn't make any hard promises regarding its suggestions. Actually in a few places it uses randomness if it doesn't have enough time to " +
                "consider all options in a short time. It also doesn't promise to remind you about some deadline that's coming up.")
                .AddMessage("ActivityRecommender also will never interrupt you; it only gives you a suggestion when you're in the mood for one and ask for it.")
                .AddMessage("Each suggestion comes with an estimate of how likely ActivityRecommender thinks that it is that you will actually do the activity, along with how much fun " +
                "it thinks you will have doing it.")
                .AddMessage("As ActivityRecommender starts to know you better, it will become slightly more assertive with its suggestions. For example, suppose you really enjoy being " +
                "creative but you often find it difficult to start. Even if you dismiss the first suggestion, ActivityRecommender may give the same suggestion a few times until either " +
                "it's decided that you're truly unwilling to do it, or you realize that ActivityRecommender has a good point and you do the activity anyway.")
                .Build());

            menuBuilder.AddLayout("Graphs", (new HelpWindowBuilder())
                .AddMessage("As soon as you record a participation or a rating, the time spent or rating assigned will immediately show up in the respective graph for that activity, " +
                "acknowledging and visualizing the time that you spent or enjoyment you received.")
                .AddMessage("You can inspect graphs to look for patterns.")
                .Build());

            menuBuilder.AddLayout("Doesn't try to maximize its own usage", (new HelpWindowBuilder())
                .AddMessage("Because you explicitly tell ActivityRecommender how happy you are to have done various activities, that's what it optimizes. Its goal is not to maximize the " +
                "amount of time you spend using ActivityRecommender.")
                .AddMessage("In fact, any time you spend using ActivityRecommender is time that you don't spend doing something else.")
                .AddMessage("So, on the Suggestions screen, if you ever push the X button, then ActivityRecommender assumes that while that activity was onscreen, that you were spending all" +
                "of your brainpower contemplating whethre to do that activity, and that that time was worth 0 happiness to you.")
                .AddMessage("ActivityRecommender will only attempt to use more of your time when it estimates that your resultant increase in happiness will be enough to compensate.")
                .Build());

            menuBuilder.AddLayout("You define what to optimize", (new HelpWindowBuilder())
                .AddMessage("Throughout ActivityRecommender, the word \"happiness\" is used to refer to the value being optimized. The expectation is that happiness is what you'll choose to " +
                "optimize, but from ActivityRecommender's perspective, they're just numbers that you're reporting, so the exact meaning is up to you.")
                .AddMessage("The important part is that ActivityRecommender interprets these as values per unit time.")
                .AddMessage("For example, if you want to maximize your total distance traveled, then you should record that 1 mile in 8 minutes is just as good as 2 miles in 16 minutes. " +
                "ActivityRecommender will know that the activity with longer duration had larger absolute impact.")
                .AddMessage("ActivityRecommender doesn't support negative values at the moment.")
                .AddMessage("Additionally, although the ratios between individual ratings can be unbounded, ActivityRecommender assumes that the ratings themselves (which you as a user " +
                "don't directly enter) are numbers from 0 to 1.")
                .Build());

            menuBuilder.AddLayout("All runs on the device", (new HelpWindowBuilder())
                .AddMessage("No internet is required.")
                .AddMessage("On-device processing is pretty fast.")
                .AddMessage("ActivityRecommender itself does not send your data to other devices. Of course, if your device automatically makes backup of application data, then those " +
                "backups may contain copies of your ActivityRecommender data. You can still use the Export functionality within ActivityRecommender to make save a data snapshot to the " +
                "device, which you can then separately save.")
                .Build());

            menuBuilder.AddLayout("No fees or ads", (new HelpWindowBuilder())
                .AddMessage("There are no advertisements in ActivityRecommender, and there is no fee to install it either.")
                .AddMessage("Why?")
                .AddMessage("1. I'm more interested in the sense of accomplishment from helping people than in monetizing ActivityRecommender. I expect adoption to be higher if it is free.")
                .AddMessage("2. I created ActivityRecommender for myself. The fact that other people can use it is a bonus.")
                .AddMessage("3. ActivityRecommender doesn't require any money to keep a server running, because all the data and processing happen on your device.")
                .Build());

            menuBuilder.AddLayout("Open source", (new HelpWindowBuilder())
                .AddMessage("Visit https://github.com/mathjeff/ActivityRecommender for more information. If you're thinking about contributing, start by opening an issue. Thanks!")
                .Build());

            mainLayout.SetContent(menuBuilder.Build());
            return mainLayout;
        }
    }
}
