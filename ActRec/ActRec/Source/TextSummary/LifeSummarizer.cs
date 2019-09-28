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
            List<Participation> sampleParticipations = this.chooseRandomParticipations(containedParticipations, 40);
            foreach (Participation participation in sampleParticipations)
            {
                components.Add(this.summarizeParticipation(participation));
            }
            // find the most interesting summaries and remove the rest
            components.Sort(new LifeSummaryItem_InterestComparer());
            components.Reverse();
            int maxCount = 6;
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

            DateTime baselineStartDate = this.computeBaselineStartDate(engine.ActivityDatabase.RootActivity.DiscoveryDate, startDate, endDate);

            Distribution baselineQuality = engine.RatingSummarizer.GetValueDistributionForDates(baselineStartDate, endDate, true, true);

            double qualityRatio = thisQuality.Mean / baselineQuality.Mean;
            double randomizedQualityRatio = qualityRatio + this.generator.NextDouble() * 0.05 - 0.025;

            string during = TimeFormatter.summarizeTimespan(startDate, endDate);
            string subject = this.randomString(new List<string>() { "life has been", "life was", "things have been", "things were" });
            string obj = this.summarizeQuality(randomizedQualityRatio);

            string text = during + " " + subject + " " + obj;
            return new LifeSummaryItem(text, double.PositiveInfinity, startDate);
        }

        private string summarizeQuality(double qualityDividedByAverage)
        {
            double quality = qualityDividedByAverage;
            if (quality < 0.1)
                return "unbelievable.";
            if (quality < 0.2)
                return "disastrous.";
            if (quality < 0.3)
                return "horrendous.";
            if (quality < 0.3)
                return "miserable.";
            if (quality < 0.5)
                return "terrible.";
            if (quality < 0.6)
                return "awful.";
            if (quality < 0.7)
                return "bad.";
            if (quality < 0.8)
                return "sad.";
            if (quality < 0.9)
                return "disappointing.";
            if (quality < 1)
                return "so-so.";
            if (quality < 1.1)
                return "nice.";
            if (quality < 1.2)
                return "good.";
            if (quality < 1.3)
                return "great.";
            if (quality < 1.4)
                return "awesome.";
            if (quality < 1.4)
                return "spectacular!";
            return "phenomenal!";
        }


        private LifeSummaryItem summarizeParticipation(Participation participation)
        {
            string timespan = TimeFormatter.summarizeTimespan(participation.StartDate, participation.EndDate);
            string activityName = participation.ActivityDescriptor.ActivityName;
            double score;
            AbsoluteRating rating = participation.GetAbsoluteRating();
            if (rating != null)
                score = rating.Score;
            else
                score = 0.5;
            string quality = this.summarizeQuality(score * 2);
            string summary;
            if (this.generator.Next(2) == 0)
                summary = timespan + ", " + activityName;
            else
                summary = activityName + " " + timespan.ToLower();
            summary += " was " + quality;
            double interest = Math.Abs(score - 0.5);
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
