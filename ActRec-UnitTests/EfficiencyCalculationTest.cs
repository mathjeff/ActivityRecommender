using System;
using Xunit;
using ActivityRecommendation;
using System.Collections.Generic;
using StatLists;
using ActivityRecommendation.Effectiveness;
using ActivityRecommendation.View;

namespace ActRec_UnitTests
{
    public class EfficiencyCalculationTest
    {
        [Fact]
        public void Test1()
        {
            Engine engine = this.MakeEngine(400);
            StatList<DateTime, double> efficiencies = this.GetEfficiencies();
            this.simulate(engine, efficiencies);
        }

        private void simulate(Engine engine, StatList<DateTime, double> efficiencies)
        {
            for (int i = 0; i < efficiencies.NumItems; i++)
            {
                DateTime when = efficiencies.GetValueAtIndex(i).Key;
                if (engine.Test_ChooseExperimentOption().HasError)
                    break; // not enough data to run more experiments
                // make a list of experiment options
                List<SuggestedMetric> experimentOptions = engine.ChooseExperimentOptions(when);
                // Skip setting difficulties for now.
                // Task difficulties could be set via something like:
                //   experimentOptions[0].PlannedMetric.DifficultyEstimate.NumEasiers++;
                // Make an experiment
                ExperimentSuggestion suggestion = engine.Experiment(experimentOptions, when);
                if (!suggestion.Experiment.Started)
                    engine.PutExperimentInMemory(suggestion.Experiment);
                // Do the suggestion
                double efficiency = efficiencies.FindPreviousItem(when, false).Value;
                double duration = 1.0 / efficiency;
                ActivityDescriptor activityDescriptor = suggestion.ActivitySuggestion.ActivityDescriptor;
                Activity activity = engine.ActivityDatabase.ResolveDescriptor(activityDescriptor);
                Metric metric = activity.DefaultMetric;
                Participation participation = new Participation(when, when.AddDays(duration), activityDescriptor);

                participation.EffectivenessMeasurement = new CompletionEfficiencyMeasurement(metric, true, 0);
                participation.EffectivenessMeasurement.DismissedActivity = true;
                RelativeEfficiencyMeasurement measurement = engine.Make_CompletionEfficiencyMeasurement(participation);
                participation.EffectivenessMeasurement.Computation = measurement;

                engine.PutParticipationInMemory(participation);
            }
            engine.FullUpdate();
            DateTime lastDay = efficiencies.GetLastValue().Key;
            for (int i = 1; i < efficiencies.NumItems; i++)
            {
                ListItemStats<DateTime, double> item = efficiencies.GetValueAtIndex(i - 1);
                ListItemStats<DateTime, double> nextItem = efficiencies.GetValueAtIndex(i);
                Distribution estimatedEfficiency = engine.EfficiencySummarizer.GetValueDistributionForDates(item.Key, nextItem.Key, true, false);
                System.Diagnostics.Debug.WriteLine("True efficiency at " + item.Key + " = " + item.Value + ", estimated efficiency = " + estimatedEfficiency.Mean);
            }
            System.Diagnostics.Debug.WriteLine("Test done");
        }

        private StatList<DateTime, double> GetEfficiencies()
        {
            // Suppose efficiency[day[i]] = i
            StatList<DateTime, double> efficiencies = new StatList<DateTime, double>(new DateComparer(), new FloatAdder());
            for (int i = 1; i < 365; i++)
            {
                efficiencies.Add(new DateTime(2000, 1, 1).AddDays(i), Math.Pow(1.01, i));
            }
            return efficiencies;
        }

        private Engine MakeEngine(int numActivities)
        {
            Engine engine = new Engine();
            engine.Randomness = new Random(0);
            ActivityDatabase activities = engine.ActivityDatabase;
            for (int i = 1; i < numActivities; i++)
            {
                ActivityDescriptor activityDescriptor = new ActivityDescriptor("a" + i);
                Inheritance inheritance = new Inheritance(activities.RootActivity.MakeDescriptor(), activityDescriptor);
                activities.CreateToDo(inheritance);
            }
            return engine;
        }
    }
}
