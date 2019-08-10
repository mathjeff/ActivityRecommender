using System;
using System.Collections.Generic;
using System.Text;

namespace ActivityRecommendation
{
    class StringQueryMatcher
    {
        public int StringScore(string item, string query)
        {
            int totalScore = 0;
            if (item == query)
                totalScore++;
            if (item.ToLower() == query.ToLower())
                totalScore++;
            char separator = ' ';
            List<string> itemWords = new List<string>(item.Split(separator));
            string[] queryWords = query.Split(separator);

            foreach (string queryWord in queryWords)
            {
                if (queryWord.Length < 1)
                    continue;
                string queryWordLower = queryWord.ToLower();
                for (int i = 0; i < itemWords.Count; i++)
                {
                    string itemWord = itemWords[i];
                    string itemWordLower = itemWord.ToLower();

                    int matchScore = 0;
                    if (itemWordLower.StartsWith(queryWordLower))
                        matchScore++;
                    if (itemWord.StartsWith(queryWord))
                        matchScore++;
                    if (itemWordLower == queryWordLower)
                        matchScore++;
                    if (itemWord == queryWord)
                        matchScore++;
                    if (matchScore > 0)
                    {
                        if (i == 0)
                            matchScore += 3;
                        totalScore += matchScore * queryWord.Length;
                        itemWords.RemoveRange(0, i + 1);
                        break;
                    }
                }
            }

            return totalScore;
        }


    }
}
