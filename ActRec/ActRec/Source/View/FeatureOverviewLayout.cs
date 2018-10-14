using VisiPlacement;

// This file serves two pruposes:
// #1. It generates the features overview screen in the application, primarily for the purpose of getting users excited
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

            menuBuilder.AddLayout("Measures your efficiency", (new HelpWindowBuilder())
                // Defining what to measure is hard,
                .AddMessage("Imagine that you wanted to measure your efficiency without ActivityRecommender. How would you define the efficiency of your life? " +
                "It's easy to measure your efficiency on one particular task, for example, by measuring how long it takes you to mow the lawn. " +
                "However, it's hard to compare that measurement against any other measurement of efficiency. " +
                "Is mowing your lawn in 30 minutes better or worse than responding to your mail in 25 minutes?")
                // but if you enter measurable tasks,
                .AddMessage("ActivityRecommender starts its efficiency measurement process by requiring you to enter some activities you want to do. " +
                "Each one can either be a ToDo (which gets done once), or can be a Category (which can be done several times) that you've added a Metric to " +
                "(a Metric signifies that you know how to measure participations in the associated Category).")
                // then we may be able to measure your efficiency on each individual task and take the average.
                .AddMessage("ActivityRecommender doesn't expect you to specify everything in your life that you want to get done, because that would take too long. " +
                "As a result, notice that what ActivityRecommender expects to measure is your efficiency on the tasks you've said you'd like to do, not your " +
                "productivity over all things you might want to do (because there may also be lots of little things you want to get done that are too much effort " +
                "to tell to ActivityRecommender).")
                // We can't just directly compare tasks completed per unit time, though.
                .AddMessage("This still leaves the question of how to compare tasks having vastly different difficulties. Suppose there are two things you'd like to do: " +
                "1: Fly from Earth to Mars, and 2: Brush your teeth. One of these things is much more difficult than the other. If we were to directly count the number " +
                "of tasks you completed every day and count that as your efficiency, then you would get just as many points for flying to the Mars as for brushing your teeth, " +
                "and it would be tempting to spend more time brushing your teeth and less time flying to the Mars.")
                // You could estimate difficulty ahead of time, but that's hard.
                .AddMessage("If you were to estimate the difficulty of each individual task, then theoretically you could divide the expected time required by the actual time time spent and treat that as " +
                "efficiency. However, do you know how much effort it requires to fly from Earth to Mars? 1 year? 10 years? 100 years? 1000 years? Do you feel confident in your estimate of the difficulty? " +
                "If there's enough error or uncertainty in your estimates of the difficulty, then measuring how long it takes would provide less information about your efficiency on the task and would provide " +
                "more information about what the proper difficulty estimate should have been to begin with.")
                // You could have ActivityRecommender estimate difficulty ahead of time, but that still has the problem where when you do smaller tasks you get more points
                .AddMessage("If you were to have ActivityRecommender estimate the difficulty of each individual task right before you did it, then that would prevent you from having to estimate difficulty " +
                "and to subsequently doubt your estimation abilities. However, because ActivityRecommender won't know how long a task will take, this would mean that when you work " +
                "on an easier task, you complete it more quickly and your computed efficiency is higher, which could make it tempting to complete lots of small tasks.")
                // ActivityRecommender measures efficiency by running experiments!
                .AddMessage("How can ActivityRecommender compare tasks of unknown difficulties without asking the user to estimate their difficulties? ActivityRecommender compares tasks by experimenting! " +
                "When you want to measure your efficiency, you ask ActivityRecommender to start an experiment. You and ActivityRecommender then decide together on the exact set of candidate tasks that might " +
                "take part in the experiment. After you find a set of tasks that you approve of, you consent to the experiment, and ActivityRecommender randomly selects two tasks for you to do and randomly " +
                "selects which one of the two you will do first. Because the first task to do is chosen randomly, there is a 50% chance that the easier task will be chosen to go first and a 50% chance that the " +
                "harder task will be chosen to go first. In mathematical terms, we can now compute an unbiased estimate of your efficiency by assuming that the two tasks are of equal difficulty")
                // Now ActivityRecommender just needs some machine learning and a huge number of experiments in order to compute trends
                .AddMessage("Then, with your help, ActivityRecommender can just run a large number of experiments to estimate your efficiency over time and to notice trends. " +
                "If the tasks you enter vary significantly in difficulty, then this will increase the variance of the measurements but shouldn't introduce any bias. ActivityRecommender also does attempt to look " +
                "for patterns and to slightly estimate the difficulties of tasks in an effort to decrease variance a little bit more.")
                // You get feedback, too
                .AddMessage("These measurements of your efficiency allow ActivityRecommender to give you feedback on your participations, and to look for trends about which activities tend to cause you to be more " +
                "efficient in the near future. For example, it may be that if you go to bed at a consistent and slightly early time every night then you may observe higher efficiency on tasks in the near future.")
                // Go!
                .AddMessage("Are you excited to measure your efficiency!?")
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
                "backups may contain copies of your ActivityRecommender data. You can still use the Export functionality within ActivityRecommender to save a data snapshot to the " +
                "device, which you can then separately save.")
                .Build());

            menuBuilder.AddLayout("No fees or ads", (new HelpWindowBuilder())
                .AddMessage("There are no advertisements in ActivityRecommender, and there is no fee to install it either.")
                .AddMessage("Why?")
                .AddMessage("1. I created ActivityRecommender for myself. The fact that other people can use it is a bonus.")
                .AddMessage("2. I'm more interested in the sense of accomplishment from helping people than in monetizing ActivityRecommender. I expect adoption to be higher if it is free.")
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
