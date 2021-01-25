using StatLists;
using System;
using System.Collections.Generic;

namespace ActivityRecommendation.TextSummary
{

    // a LifeSummarizer creates a text description of how the user's life has been going during a certain time
    class LifeSummarizer
    {
        public LifeSummarizer(Engine engine, Persona persona, Random generator)
        {
            this.engine = engine;
            this.persona = persona;
            this.generator = generator;
        }

        public string Summarize(DateTime startDate, DateTime endDate)
        {
            // add an overall summary
            List<LifeSummaryItem> components = new List<LifeSummaryItem>();
            List<Participation> containedParticipations = this.engine.ActivityDatabase.RootActivity.getParticipationsSince(startDate);

            // summarize some individual participations
            List<Participation> sampleParticipations = this.chooseRandomParticipations(containedParticipations, 80);
            foreach (Participation participation in sampleParticipations)
            {
                components.Add(this.summarizeParticipation(participation));
            }
            // find the most interesting summaries and remove the rest
            components.Sort(new LifeSummaryItem_InterestComparer());
            components.Reverse();
            int maxCount = 20;
            if (components.Count > maxCount)
                components = components.GetRange(0, maxCount);
            // reorder the summaries based on the order we want to display them
            components.Sort(new LifeSummaryItem_DateComparer());

            // compute very brief summary
            List<string> texts = new List<string>();
            texts.Add(this.EvaluateQualityOfLife(engine, startDate, endDate).text);
            // join all the summaries with newlines
            LifeSummaryItem previousItem = null;
            foreach (LifeSummaryItem item in components)
            {
                texts.Add(this.summaryWithConnector(item, previousItem));
                previousItem = item;
            }
            texts.Add("Sincerely, " + persona.Name);
            return string.Join("\n\n", texts);
        }

        private LifeSummaryItem EvaluateQualityOfLife(Engine engine, DateTime startDate, DateTime endDate)
        {
            Distribution thisQuality = engine.RatingSummarizer.GetValueDistributionForDates(startDate, endDate, true, true);

            string during = TimeFormatter.summarizeTimespan(startDate, endDate);
            string subject = this.randomString(new List<string>() { "life has been", "life was", "things have been", "things were" });
            string obj = this.summarizeQuality(thisQuality.Mean, this.ActivityDatabase.RootActivity.Ratings);

            string text = during + " " + subject + " " + obj;
            return new LifeSummaryItem(text, thisQuality.Mean, double.PositiveInfinity, startDate, endDate);
        }

        private string summarizeQuality(double participationScore, Distribution usualActivityScore)
        {
            string qualitySummary = this.summarizeQuality(participationScore);
            if (usualActivityScore.Weight < 1)
            {
                // We don't have much data about how this activity usually goes, so we just describe how it went
                return qualitySummary + ".";
            }
            // We do have information about how this activity usually goes, so we can include that in the description too
            double difference = participationScore - usualActivityScore.Mean;
            string surpriseQualifier = "";
            // If this activity is usually extreme but this time was less extreme, we might a qualifier like "just" to make something like "just good"
            if ((difference >= 0) == (participationScore < 0.5))
            {
                // the surprise is that this participation was closer to average than normal for this activity
                surpriseQualifier = this.randomString(new List<string>() { "just ", "simply ", "merely " });
            }
            if (Math.Abs(difference) < 0.1)
            {
                if (usualActivityScore.StdDev < 0.1)
                    return qualitySummary + ", like always.";
                if (usualActivityScore.StdDev < 0.2)
                    return qualitySummary + this.randomString(new List<string>() { ", like usual.", "." });
                return "unsurprisingly " + qualitySummary + ".";
            }
            if (Math.Abs(difference) < 0.2)
            {
                if (usualActivityScore.StdDev < 0.2)
                    return "unexpectedly " + surpriseQualifier + qualitySummary + ".";
                return "unusually " + surpriseQualifier + qualitySummary + ".";
            }
            if (Math.Abs(difference) < 0.3)
            {
                if (usualActivityScore.StdDev < 0.3)
                    return "surprisingly " + surpriseQualifier + qualitySummary + ".";
                return this.randomString(new List<string>() { "quite ", "really " }) + qualitySummary + ".";
            }
            if (Math.Abs(difference) < usualActivityScore.StdDev)
            {
                // a large but not unusual difference
                return qualitySummary + "!";
            }
            else
            {
                // a large, unusual difference
                return "astonishingly " + surpriseQualifier + qualitySummary + "!";
            }
        }

        private string summarizeQuality(double quality)
        {
            if (quality < 0.05)
                return "unbelievable";
            if (quality < 0.1)
                return "disastrous";
            if (quality < 0.15)
                return "horrendous";
            if (quality < 0.2)
                return "miserable";
            if (quality < 0.25)
                return "terrible";
            if (quality < 0.3)
                return "awful";
            if (quality < 0.35)
                return "bad";
            if (quality < 0.4)
                return "sad";
            if (quality < 0.45)
                return "disappointing";
            if (quality < 0.5)
                return "so-so";
            if (quality < 0.55)
                return "ok";
            if (quality < 0.6)
                return "nice";
            if (quality < 0.65)
                return "good";
            if (quality < 0.7)
                return "great";
            if (quality < 0.75)
                return "awesome";
            if (quality < 0.8)
                return "wonderful";
            if (quality < 0.85)
                return "amazing";
            if (quality < 0.9)
                return "incredible";
            if (quality < 0.95)
                return "spectacular";
            return "phenomenal";
        }


        private LifeSummaryItem summarizeParticipation(Participation participation)
        {
            string timespan = TimeFormatter.summarizeTimespan(participation.StartDate, participation.EndDate);
            string activityName = participation.ActivityDescriptor.ActivityName;
            double participationScore;
            AbsoluteRating rating = participation.GetAbsoluteRating();
            if (rating != null)
                participationScore = rating.Score;
            else
                participationScore = 0.5;
            Activity activity = this.ActivityDatabase.ResolveDescriptor(participation.ActivityDescriptor);
            Distribution usualScore = activity.Ratings;
            string quality = this.summarizeQuality(participationScore, usualScore);
            string summary;
            if (this.generator.Next(2) == 0)
                summary = timespan + ", " + activityName;
            else
                summary = activityName + " " + timespan.ToLower();
            summary += " was " + quality;
            double interest = Math.Abs(participationScore - 0.5);
            if (participation.Comment != null)
            {
                summary += " " + participation.Comment;
                interest *= 2;
            }
            return new LifeSummaryItem(summary, participationScore, interest, participation.StartDate, participation.EndDate);
        }

        private string randomString(List<string> choices)
        {
            return choices[this.generator.Next(choices.Count)];
        }

        private IEnumerable<int> nChooseK(int maxExclusive, int count)
        {
            StatList<int, bool> choices = new StatList<int, bool>(new IntComparerer(), new NoopCombiner<bool>());

            if (count > maxExclusive)
                count = maxExclusive;
            for (int i = 0; i < count; i++)
            {
                int randInt = this.generator.Next(maxExclusive - i);
                int index = randInt;
                while (true)
                {
                    int smallerCount = choices.CountBeforeKey(index, true);
                    if (index != randInt + smallerCount)
                        index = randInt + smallerCount;
                    else
                        break;
                }
                choices.Add(index, true);
            }
            return choices.Keys;
        }

        private List<Participation> chooseRandomParticipations(List<Participation> choices, int count)
        {
            IEnumerable<int> indices = this.nChooseK(choices.Count, count);
            List<Participation> participations = new List<Participation>();
            foreach (int i in indices)
            {
                participations.Add(choices[i]);
            }
            return participations;
        }

        private string summaryWithConnector(LifeSummaryItem item, LifeSummaryItem prev)
        {
            if (prev == null)
                return item.text;

            List<String> happinessConnectors = new List<string>();

            double baseline = 0.5;
            bool prevGood = prev.happiness >= baseline;
            bool currentGood = item.happiness >= baseline;

            // If the user did something sad followed by something happy, we can say "X was mediocre. However, Y was great!"
            if (prevGood != currentGood)
                happinessConnectors.Add("However");

            List<String> temporalConnectors = new List<string>();
            TimeSpan timeBetween = item.startDate.Subtract(prev.endDate);
            // both events occurred on the same day
            bool overlapping = prev.endDate.CompareTo(item.startDate) > 0;
            int numDaysLater = (int)(item.startDate.Date.Subtract(prev.endDate.Date).TotalDays + 0.5);
            if (overlapping)
            {
                temporalConnectors.Add("Meanwhile");
                temporalConnectors.Add("At the same time");
            }
            else
            {
                if (timeBetween.CompareTo(TimeSpan.FromMinutes(5)) < 0)
                {
                    temporalConnectors.Add("Then");
                    temporalConnectors.Add("Next");
                    temporalConnectors.Add("After that");
                }
                else
                {
                    if (timeBetween.CompareTo(TimeSpan.FromHours(1)) < 0)
                    {
                        temporalConnectors.Add("A little while later");
                        temporalConnectors.Add("Not long after that");
                        if (numDaysLater == 0)
                            temporalConnectors.Add("Later that day");
                        else
                            temporalConnectors.Add("Then, a little bit after midnight");
                    }
                    else
                    {
                        if (numDaysLater == 0)
                            temporalConnectors.Add("Later that day");
                        else if (numDaysLater == 1)
                            temporalConnectors.Add("The next day");
                        else if (numDaysLater == 2)
                            temporalConnectors.Add("A couple days later");
                        else if (numDaysLater < 5)
                            temporalConnectors.Add("A few days later");
                        else if (numDaysLater < 10)
                            temporalConnectors.Add("About a week later");
                        else if (numDaysLater < 18)
                            temporalConnectors.Add("A couple weeks later");
                        else if (numDaysLater < 40)
                            temporalConnectors.Add("About a month later");
                        else if (numDaysLater < 80)
                            temporalConnectors.Add("A couple months later");
                        else if (numDaysLater < 120)
                            temporalConnectors.Add("A few months later");
                    }
                }
            }

            string connector = "";
            if (happinessConnectors.Count > 0 && this.generator.NextDouble() < 0.33)
            {
                connector = this.randomString(happinessConnectors) + ", ";
            }
            else
            {
                if (temporalConnectors.Count > 0 && this.generator.NextDouble() < 0.33)
                    connector += this.randomString(temporalConnectors) + ", ";
            }

            string summary = connector + item.text;
            return summary;
        }



        private ActivityDatabase ActivityDatabase
        {
            get
            {
                return this.engine.ActivityDatabase;
            }
        }

        private Engine engine;
        private Persona persona;
        private Random generator;
    }

    class LifeSummaryItem
    {
        public LifeSummaryItem(string text, double happiness, double interest, DateTime startDate, DateTime endDate)
        {
            this.text = text;
            this.happiness = happiness;
            this.interest = interest;
            this.startDate = startDate;
            this.endDate = endDate;
        }
        public double happiness;
        // how interesting it is to include this item
        public double interest;
        // where in the summary we want this item to go
        public DateTime startDate;
        public DateTime endDate;
        public string text;
    }

    class LifeSummaryItem_DateComparer : IComparer<LifeSummaryItem>
    {
        public int Compare(LifeSummaryItem a, LifeSummaryItem b)
        {
            return a.startDate.CompareTo(b.startDate);
        }

    }
    class LifeSummaryItem_InterestComparer : IComparer<LifeSummaryItem>
    {
        public int Compare(LifeSummaryItem a, LifeSummaryItem b)
        {
            return a.interest.CompareTo(b.interest);
        }

    }

}
