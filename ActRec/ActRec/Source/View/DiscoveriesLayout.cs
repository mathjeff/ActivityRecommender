using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;

// This file serves two purposes:
// #1. It generates the instructions screen in the application
// #2. It can describe the usage instructions to people that don't have the application installed
namespace ActivityRecommendation.View
{
    class DiscoveriesLayout
    {
        public static LayoutChoice_Set New(LayoutStack layoutStack)
        {
            TitledControl mainLayout = new TitledControl("Discoveries made by using ActivityRecommender:\nHow many of these apply to you?");

            MenuLayoutBuilder menuBuilder = new MenuLayoutBuilder(layoutStack);

            menuBuilder.AddLayout("Large impact of environment on happiness",
                new HelpWindowBuilder()
                    .AddMessage("Every time I've changed cities or jobs, it's been correlated with a 1 - 3% change in net present happiness.")
                    .AddMessage("If a change of 1 - 3 % sounds small, consider that it equals 15 - 45 minutes per day.")
                    .AddMessage("How do you feel about your city and your job?")
                    .Build()
            );

            menuBuilder.AddLayout("Willpower is consistent over long periods of time",
                new HelpWindowBuilder()
                    .AddMessage("Imagine that you relaxed all day and feel terrible at the end of the day for not having gotten any work done. Presumably, " +
                    "you are imagining that if you had gotten more work done during the day then that would have been free and would not affect your " +
                    "motivation to get work done on a later day. Right?")
                    .AddMessage("By recording via ActivityRecommender my time spent working, I've made two observations. The first is that graph of the " +
                    "cumulative amount of time I've spent working, over long periods of time tends to be fairly straight. If each day's productivity were random, then this graph would be " +
                    "more varied. In other words, there's a limit to the average amount of work I can do per day using sheer willpower, and expending an " +
                    "above-average amount of willpower on one day generally results in less willpower available for later days.")
                    .AddMessage("The second observation is a measurement of how many extra hours of work I can do before I run out of willpower. For me, " +
                    "the size of my pool of willpower is 120 hours. This means, for example, that if I feel there is an urgent problem that I must solve as soon as possible, " +
                    "then I can devote my average amount of work to it, plus an additional 40 hours per week above my average, for 3 weeks (40*3=120), " +
                    "before I'm no longer willing to do an above average amount of work. Alternatively, if I instead devote only 1 hour per day beyond " +
                    "my average amount of work, then I can keep that pace up for 120 days before running out of interest and willpower.")
                    .AddMessage("The size of this reserve of extra worktime may be different for different users, but still may be larger than users may " +
                    "expect, if it can take ~3 months to deplete.")
                    .Build()
            );

            menuBuilder.AddLayout("Feedback empowers more work",
                new HelpWindowBuilder()
                    .AddMessage("On 2017-09-20, I added a feature into ActivityRecommender that causes it to give users feedback when they record " +
                    "participations. This feedback could look like 'Nice! +55 days!' and refers to the difference between the user's predicted net present " + 
                    "happiness after doing this activity compared to what it would have been without this participation.")
                    .AddMessage("This was really exciting for me as a user because suddenly it felt like I could count on some entity to give me " + 
                    "some thoughts about the things I was doing and to at least offer some independent confirmation about whether the things I was " + 
                    "doing were reasonable, even if it was only doing math on the data I input and confirming that because I seemed to be enjoying " + 
                    "these things, that it didn't notice anything to be wrong.")
                    .AddMessage("During the following year from 2017-09-20 to 2018-09-20, I spent 108 additional hours (an average increase of 18 minutes " +
                    "per day) working on technology projects (primarily ActivityRecommender) beyond my historical average .")
                    .AddMessage("Note that because this 108 hour increase is less than the size of my previously measured pool of willpower of 120 " +
                    "hours, I can't be quite sure that this feedback was the reason for this extra motivation, but it seems likely because after one year was " +
                    "over, I did not stop working as hard; instead, I added a new feature into ActivityRecommender that motivated me more (and " +
                    "therefore made it hard to collect data for this feature specifically).")
                    .Build()
            );

            menuBuilder.AddLayout("Measuring efficiency empowers even more work",
                new HelpWindowBuilder()
                    .AddMessage("On 2018-09-20 I added a feature into ActivityRecommender that allows users to run experiments to measure their " +
                    "efficiency.")
                    .AddMessage("This was really exciting for me because it suddenly meant that there was an extremely clear meaning in whether I " +
                    "accomplished various tasks, and there was also a uniform way to compare very different tasks.")
                    .AddMessage("In the two years after implementing this feature, I spent an additional 202 additional hours working on technology projects " +
                    "than my previous average from 2017-09-20 to 2018-09-20, (an increase of about 17 minutes per day).")
                    .Build()
            );

            menuBuilder.AddLayout("Large impact of efficiency measurement on happiness",
                new HelpWindowBuilder()
                    .AddMessage("On 2018-09-20 I implemented the experimentation feature in ActivityRecommender, which allows users to " +
                    "run experiments to measure their efficiency. It was very exciting and meaningful to feel that an entity was keeping score of the " +
                    "things I accomplished, large and small.")
                    .AddMessage("The experimentation feature was correlated with a 3% improvement to my net present happiness. That's " +
                    "equivalent to having an additional 45 minutes of free time every day!")
                    .Build()
            );

            menuBuilder.AddLayout("More efficient in the morning",
                new HelpWindowBuilder()
                    .AddMessage("Using the efficiency data I collected via experiments, I computed the linear regression of my efficiency as a function " +
                    "time of day (number of seconds since midnight).")
                    .AddMessage("My calculations indicate that I am 1.317 times as efficient in the morning than in the evening.")
                    .AddMessage("This suggests that in the morning I am better suited to tasks that involve effort, and that the evening is the most " +
                    "appropriate time for tasks that involve more waiting.")
                    .Build()
            );

            menuBuilder.AddLayout("Making plans is useful",
                new HelpWindowBuilder()
                    .AddMessage("Shortly after implementing the experimentation feature in ActivityRecommender, I began to find myself having difficulty " + 
                    "thinking of enough measurable tasks to work on. Without enough tasks, I wouldn't have anything to experiment on, and wouldn't be " +
                    "able to gain the happiness and efficiency benefits from running an experiment and learning the results.")
                    .AddMessage("When this happened and I was low on ToDos, the value in entering a new ToDo became more clear because it was easily " + 
                    "recognizable as a prerequisite for being motivated to do the work that it described. Suddenly, entering a new ToDo became exciting " + 
                    "because it was a prerequisite for running an exciting experiment.")
                    .AddMessage("Regardless of your motivation for completing your ToDos, if thinking of a ToDo is a prerequisite for doing it, you " +
                    "may find that thinking of new ToDos is valuable too.")
                    .Build()
            );

            mainLayout.SetContent(menuBuilder.Build());
            return mainLayout;
        }
    }
}
