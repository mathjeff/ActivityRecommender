using StatLists;
using System;
using System.Collections.Generic;
using System.Text;

// A FeedbackReplayer recomputes the feedback that would be given to the user for each of their participations
namespace ActivityRecommendation
{
    class FeedbackReplayer : HistoryReplayer
    {
        public override void PreviewParticipation(Participation newParticipation)
        {
            logCounter++;
            // resolve some parameters
            Activity activity = this.activityDatabase.ResolveDescriptor(newParticipation.ActivityDescriptor);
            DateTime when = newParticipation.StartDate;
            if (logCounter % 1000 == 0)
                this.log("Processing participation at " + when + " for " + activity.Name);
            ParticipationFeedback feedbackObject = this.engine.computeStandardParticipationFeedback(activity, when, newParticipation.EndDate);
            if (feedbackObject != null)
            {
                // remove the number
                string fullFeedback = feedbackObject.Summary;
                string feedback = this.stripLeadingNumbers(fullFeedback);

                // save into this.feedbacks
                string prefix = "Feedback at " + when + " for " + activity + ":";
                //this.log(this.padColumn(prefix) + " " + fullFeedback);
                if (!this.feedbacks.ContainsKey(feedback))
                    this.feedbacks[feedback] = new Dictionary<Activity, int>();
                Dictionary<Activity, int> counts = feedbacks[feedback];
                if (!counts.ContainsKey(activity))
                    counts[activity] = 0;
                counts[activity]++;
            }
            // call parent class
            base.PreviewParticipation(newParticipation);
        }

        public override Engine Finish()
        {
            this.log("Grouping participations by category");
            foreach (KeyValuePair<String, Dictionary<Activity, int>> entry in this.feedbacks)
            {
                this.log("");
                this.log("Instances of feedback " + entry.Key + ":");
                // sort
                StatList<int, Activity> sorted = new StatList<int, Activity>(new IntComparerer(), null);
                foreach (KeyValuePair<Activity, int> e2 in entry.Value)
                {
                    sorted.Add(e2.Value, e2.Key);
                }
                // Find the activities that were done often enough to be noteworthy
                int max = sorted.GetLastValue().Key;
                int cumulative = 0;
                int minInterestingKey = 0;
                List<ListItemStats<int, Activity>> interesting = new List<ListItemStats<int, Activity>>();
                foreach (ListItemStats<int, Activity> stats in sorted.AllItems)
                {
                    cumulative += stats.Key;
                    // Some activities were only done a very small number of times. We skip displaying those because they might be distracting
                    if (cumulative >= max)
                    {
                        interesting.Add(stats);
                    }
                }
                // sort the more commonly done activities to the top
                interesting.Reverse();
                foreach (ListItemStats<int, Activity> stats in interesting)
                {
                    this.log(stats.Value + " : " + stats.Key + " times");
                }
            }
            return base.Finish();
        }
        private string stripLeadingNumbers(string message)
        {
            for (int i = 0; i < message.Length; i++)
            {
                char c = message[i];
                if (c >= '0' && c <= '9')
                {
                    continue;
                }
                if (c == '+' || c == '-' || c == ':' || c == '!' || c == ' ')
                {
                    continue;
                }
                // Found a non-special character, return the rest
                return message.Substring(i);
            }
            return message;
        }
        private void log(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        // feedbacks[text][activity] = the number of times that activity got that feedback
        Dictionary<string, Dictionary<Activity, int>> feedbacks = new Dictionary<string, Dictionary<Activity, int>>();
        int logCounter;
    }
}
