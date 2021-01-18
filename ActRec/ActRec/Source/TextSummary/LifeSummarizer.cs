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
            components.Add(this.EvaluateQualityOfLife(engine, startDate, endDate));
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

            // join with newlines
            List<string> texts = new List<string>();
            foreach (LifeSummaryItem item in components)
            {
                texts.Add(item.text);
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
            return new LifeSummaryItem(text, double.PositiveInfinity, startDate);
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
            return new LifeSummaryItem(summary, interest, participation.StartDate);
        }

        // chooses a DateTime to use for comparison
        private DateTime computeBaselineStartDate(DateTime dataStartDate, DateTime summaryStartDate, DateTime summaryEndDate)
        {
            DateTime firstExistentDate = dataStartDate;
            double existenceDurationDays = summaryEndDate.Subtract(firstExistentDate).TotalDays;
            double includedDurationDays = summaryEndDate.Subtract(summaryEndDate).TotalDays;

            double includedFraction = includedDurationDays / existenceDurationDays;
            double relevanceFraction = Math.Sqrt(includedFraction);

            double comparisonDurationDays = existenceDurationDays * relevanceFraction;
            TimeSpan comparisonDuration = TimeSpan.FromDays(comparisonDurationDays);
            DateTime firstRelevantDate = summaryStartDate.Subtract(comparisonDuration);
            return firstRelevantDate;

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
        public LifeSummaryItem(string text, double interest, DateTime applicableDate)
        {
            this.text = text;
            this.interest = interest;
            this.applicableDate = applicableDate;
        }
        // how interesting it is to include this item
        public double interest;
        // where in the summary we want this item to go
        public DateTime applicableDate;
        public string text;
    }

    class LifeSummaryItem_DateComparer : IComparer<LifeSummaryItem>
    {
        public int Compare(LifeSummaryItem a, LifeSummaryItem b)
        {
            return a.applicableDate.CompareTo(b.applicableDate);
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
