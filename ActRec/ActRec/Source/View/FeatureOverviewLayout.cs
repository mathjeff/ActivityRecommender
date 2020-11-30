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
            TitledControl mainLayout = new TitledControl("Highlights of ActivityRecommender:");
            MenuLayoutBuilder menuBuilder = new MenuLayoutBuilder(layoutStack);


            menuBuilder.AddLayout("Makes life more fun, right away",
                new TitledControl("ActivityRecommender will make life way more fun",
                    (new MenuLayoutBuilder(layoutStack))
                    .AddLayout("Provides suggestions", (new HelpWindowBuilder())
                        .AddMessage("The feature that ActivityRecommender is named for, of course, is that you can ask it for suggestions of what to do.")
                        .AddMessage("Each suggestion comes with an estimate of how likely ActivityRecommender thinks that it is that you will actually do the activity, along with how much fun " +
                        "it thinks you will have doing it.")
                        .AddMessage("As ActivityRecommender starts to know you better, it may become slightly more assertive with its suggestions. For example, suppose you really enjoy being " +
                        "creative but you often find it difficult to start. Even if you dismiss the first suggestion, ActivityRecommender may give the same suggestion a few times until either " +
                        "it's decided that you're truly unwilling to do it, or you realize that ActivityRecommender has a good point and you do the activity anyway.")
                        .AddMessage("ActivityRecommender doesn't make any hard promises regarding its suggestions; it doesn't guarantee that it will remind you to do your chores every day or to " +
                        "finish a certain task by a certain date. This keeps things exciting! In fact, in a few places it uses randomness if it doesn't have enough time to " +
                        "consider all options in a short time.")
                        .AddMessage("ActivityRecommender also will never interrupt you; it only gives you a suggestion when you're in the mood for one and ask for it.")
                        .Build()
                    )
                    .AddLayout("Fast interface with lots of autocomplete", (new HelpWindowBuilder())
                        .AddMessage("ActivityRecommender has autocomplete in many places.")
                        .AddMessage("When entering an activity name, press Enter to accept the autocomplete suggestion.")
                        .AddMessage("It's common to be able to record a participation by pressing the screen only six times (One press to select the name box, two presses to type the first two letters, " +
                        "one press of Enter to select the autocomplete suggestion, one press of \"End = Now\" to select and end time, and one press of OK to record it).")
                        .Build()
                    )
                    .AddLayout("Fast feedback", (new HelpWindowBuilder())
                        .AddMessage("Every time you record having done an activity, ActivityRecommender gives you feedback even before you press OK.")
                        .AddMessage("It builds a model of your longterm happiness and how it is expected to be affected by having done various activities.")
                        .AddMessage("As soon as you enter an activity name on the Record Participations screen, you will see an estimate of the difference between your expected longterm happiness " +
                        "with that participation vs without that participation.")
                        .AddMessage("For example, suppose that you exercised every week last year and recorded that you were twice as happy as the year before in which you did no exercising. " +
                        "If you then record another participation of exercise, ActivityRecommender may tell you that your participation in exercise is correlated with an increase in " +
                        "happiness of several hundred days, because any time you exercise, it indicates that you may be happier for a whole year.")
                        .Build()
                    )
                    .AddLayout("Graphs your life", (new HelpWindowBuilder())
                        .AddMessage("As soon as you record a participation or a rating, the time spent or rating assigned will immediately show up in the respective graph for that activity, " +
                        "acknowledging and visualizing the time that you spent or enjoyment you received.")
                        .AddMessage("You can inspect graphs to look for patterns.")
                        .Build()
                    )
                    .Build()
                )
            );
            menuBuilder.AddLayout("Helps make life more meaningful",
                new TitledControl("ActivityRecommender will make life more meaningful",
                    new MenuLayoutBuilder(layoutStack)
                    .AddLayout("Maximizes longterm happiness", (new HelpWindowBuilder())
                        .AddMessage("Several aspects of ActivityRecommender are designed to maximize your longterm happiness.")
                        .AddMessage("Among other things, this means that it should encourage you towards entertainment that recharges rather than frustrates you.")
                        .AddMessage("The way that this works is you gradually enter more and more data about things you've done and how happy they have made you.")
                        .AddMessage("In the meanwhile, ActivityRecommender builds an increasingly accurate model of your happiness.")
                        .AddMessage("At any given time, it uses this model to predict and attempt to maximize the net present value (an economics term) of how much happiness you'll experience " +
                        "in the future. This is approximately equivalent to maximizing the amount of happiness that you'll experience over the next " +
                        (UserPreferences.DefaultPreferences.HalfLife.TotalDays / 365) + " years.")
                        .AddMessage("So even if you don't like doing chores, if ActivityRecommender notices that doing chores from time to time enables you to have more time to do other fun " +
                        "things, it will recommend chores from time to time anyway.")
                        .Build()
                    )
                    .AddLayout("Personalized", (new HelpWindowBuilder())
                        .AddMessage("You decide which activities exist in your world and what their relationships are.")
                        .AddMessage("If your icecream store is far away and you want to declare that \"Buying Ice Cream\" is \"Exercise\", that's your prerogative.")
                        .Build()
                    )
                    .AddLayout("You define what to optimize", (new HelpWindowBuilder())
                        .AddMessage("Throughout ActivityRecommender, the word \"happiness\" is used to refer to the value being optimized. The expectation is that happiness is what you'll choose to " +
                        "optimize, but from ActivityRecommender's perspective, they're just numbers that you're reporting, so the exact meaning is up to you.")
                        .AddMessage("The important part is that ActivityRecommender interprets these as values per unit time.")
                        .AddMessage("For example, if you want to maximize your total distance traveled, then you should record that 1 mile in 8 minutes is just as good as 2 miles in 16 minutes. " +
                        "ActivityRecommender will know that the activity with longer duration had larger absolute impact.")
                        .AddMessage("ActivityRecommender doesn't support negative values at the moment.")
                        .AddMessage("Additionally, although the ratios between individual ratings can be unbounded, ActivityRecommender assumes that the ratings themselves (which you as a user " +
                        "don't directly enter) are numbers from 0 to 1.")
                        .Build()
                    )
                    .AddLayout("Meaningful relative ratings", (new HelpWindowBuilder())
                        .AddMessage("It can be hard to admit that a single activity was boring, a mistake, inefficient, or otherwise regrettable, let alone say it on a daily basis.")
                        .AddMessage("So, ActivityRecommender doesn't ever ask you to say that. ActivityRecommender never asks you to enter precisely how much fun you had doing a certain activity.")
                        .AddMessage("ActivityRecommender only ever asks you to compare two different participations.")
                        .AddMessage("Comparing two participations is pretty noncommital but still gives ActivityRecommender plenty of information to extract meaningful relationships.")
                        .AddMessage("For example, if you repeatedly report that listening to music is 1.5 times as enjoyable doing chores, then if you discover a new TV show and report that " +
                        "watching the first episode was 1.1 times as much fun as listening to music, then ActivityRecommender may estimate that watching that episode may have been 1.1*1.5=1.65 " +
                        "times as much fun as doing chores would have been.")
                        .Build()
                    )
                    .AddLayout("Measures your efficiency", (new HelpWindowBuilder())
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
                        "and it would be tempting to spend more time brushing your teeth and less time flying to Mars.")
                        // You could estimate difficulty ahead of time, but that's hard.
                        .AddMessage("If you were to estimate the difficulty of each individual task, then theoretically you could divide the expected time required by the actual time time spent and treat that as " +
                        "efficiency. However, do you know how much effort it requires to fly from Earth to Mars? 1 year? 10 years? 100 years? 1000 years? Do you feel confident in your estimate of the difficulty? " +
                        "If there's enough error or uncertainty in your estimates of the difficulty, then measuring how long it takes would provide less information about your efficiency on the task and would provide " +
                        "more information about what the proper difficulty estimate should have been to begin with.")
                        // You could have ActivityRecommender estimate difficulty ahead of time, but that still has the problem where when you do smaller tasks you get more points
                        .AddMessage("If you were to have ActivityRecommender estimate the difficulty of each individual task right before you did it, then that would prevent you from having to estimate difficulty " +
                        "and to subsequently doubt your estimation abilities. However, if you have a new task that you've never done before and whose difficulty is unknown to ActivityRecommender, then if it's an " +
                        "easier task, you complete it more quickly and your computed efficiency is higher. This could make it tempting to complete lots of small tasks!")
                        // ActivityRecommender measures efficiency by running experiments!
                        .AddMessage("How can ActivityRecommender compare tasks of unknown difficulties without asking the user to estimate their difficulties? ActivityRecommender compares tasks by experimenting! " +
                        "When you want to measure your efficiency, you ask ActivityRecommender to start an experiment. You and ActivityRecommender then decide together on the exact set of candidate tasks that might " +
                        "take part in the experiment. After you find a set of tasks that you approve of, you consent to the experiment, and ActivityRecommender randomly selects two tasks for you to do and randomly " +
                        "selects which one of the two you will do first. Because the first task to do is chosen randomly, there is a 50% chance that the easier task will be chosen to go first and a 50% chance that the " +
                        "harder task will be chosen to go first. In mathematical terms, we can now compute an unbiased estimate of your efficiency at these two times by imagining that the two tasks are of equal difficulty.")
                        // Now ActivityRecommender just needs some machine learning and a huge number of experiments in order to compute trends
                        .AddMessage("You may notice that these unbiased experiments can still have large variance, though: if you get lucky and are assigned Brushing your Teeth as a task, then that will presumably " +
                        "give you a much larger efficiency measurement than if you had tried to Fly to Mars. However, knowing the exact efficiency of an individual participation isn't what ActivityRecommender is " +
                        "trying to compute. Really what ActivityRecommender is looking for is overall patterns, like maybe you tend to be more efficient when you don't stay up late, regardless of whether you're " +
                        "brushing your teeth or preparing for a flight. As you run more and more experiments, ActivityRecommender will have more and more data to analyze for trends.")
                        // ActivityRecommender also lowers variance via some difficulty estimates
                        .AddMessage("In this example with Flying to Mars and Brushing your Teeth, the first time you run this experiment there is a huge uncertainty, and if everything in your life highly " +
                        "uncertain (for example if you write computer software), " +
                        "then ActivityRecommender will need a large number of experiments before it can notice meaningful trends. However, if you Fly to Mars often and Brush your Teeth often, then after a couple of times, " +
                        "ActivityRecommender will notice that one of these things is difficult and the other is easy, and will adjust its difficulty estimates accordingly. This won't introduce bias into the efficiency " +
                        "measurements of individual times because the question of which task is chosen is still random. However, it should lower the variance because ActivityRecommender stops being surprised when it " +
                        "turns out that Flying to Mars takes a long time.")
                        // You get feedback, too
                        .AddMessage("These efficiency measurements and trends that ActivityRecommender finds will appear to you in a few places in the application. When you record a participation, ActivityRecommender " +
                        "will evaluate whether it thinks this participation will help or hurt your future efficiency, and give you feedback accordingly. Additionally, you can view your efficiency in the " +
                        "Visualize one Activity graph screen.")
                        // Go!
                        .AddMessage("Are you excited to measure your efficiency!?")
                        .Build()
                    )
                    .AddLayout("Save ideas and organize them later", (new HelpWindowBuilder())
                        .AddMessage("Have you ever had an idea for something cool you wanted to think about, but you didn't know where to write it down and then you forgot?")
                        .AddMessage("Have you ever made a list of interesting ideas, and then found that this list became hard to read because there were so many less-interesting " +
                        "ideas before the ones that you wanted? Has this ever caused you to completely give up on the less-interesting ideas and make a new list?")
                        .AddMessage("ActivityRecommender can help you with this idea-sorting problem too!")
                        .AddMessage("If you have an idea that's interesting to think about and that might be interesting to act on in the future, you can create a ProtoActivity. " +
                        "Creating a ProtoActivity only requires typing a brief description of what the idea is.")
                        .AddMessage("Later, when you would like to be reminded about the interesting ideas that you've previously entered, " +
                        "you can browse them in several ways. The most interesting way to browse ProtoActivities is to look at the ones that you have declared as being most " +
                        "interesting in the past. You will see two ProtoActivities onscren  at one time, and have the opportunity to edit one, or to declare one of them as " +
                        "being more interesting than the other. When you compare them, both will be dismissed, but the one you liked more will return sooner and the one you " +
                        "liked less will return later. This allows you to enter lots of ideas whenever you think of them, and to later decide how interesting they are, " +
                        "based on comparing them to your other ideas and making edits to them over time.")
                        .AddMessage("After you've thought enough about a ProtoActivity and you're satisfied with it, you can either delete it if you don't need it anymore, " +
                        "or you can promote it into a ToDo if you plan to do it eventually. This ToDo can then be suggested by ActivityRecommender in the Suggestions " +
                        "screen, and it can also take part in experiments, too.")
                        .AddMessage("In summary, a ProtoActivity is great for a partially formed idea that you want to revisit later.")
                        .Build()
                    )
                    .Build()
                )
            );
            menuBuilder.AddLayout("All for free",
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
