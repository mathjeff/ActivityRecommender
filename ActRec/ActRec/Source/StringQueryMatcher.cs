using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Maui.Controls;

namespace ActivityRecommendation
{
    class StringQueryMatcher
    {
        // returns a number indicating how well these two strings match
        public int StringScore(string item, string query)
        {
            int numSimilarities = 0;

            if (item.ToLower() == query.ToLower())
                numSimilarities++;
            char separator = ' ';
            List<string> itemWords = new List<string>(item.Split(separator));
            string[] queryWords = query.Split(separator);

            int numCaseMatches = 0;
            int numUnmatchedWords = 0;
            foreach (string queryWord in queryWords)
            {
                if (queryWord.Length < 1)
                    continue;
                string queryWordLower = queryWord.ToLower();
                bool thisQueryWordMatches = false;
                for (int i = 0; i < itemWords.Count; i++)
                {
                    string itemWord = itemWords[i];
                    string itemWordLower = itemWord.ToLower();

                    int matchScore = 0;
                    // 1 point for a case-insensitive prefix match
                    if (itemWordLower.StartsWith(queryWordLower))
                        matchScore++;
                    // 1 point for a case-sensitive prefix match
                    if (itemWord.StartsWith(queryWord))
                        numCaseMatches++;
                    // 1 point for a case-insensitive word match
                    if (itemWordLower == queryWordLower)
                        matchScore++;
                    // 1 point for a full word match
                    if (itemWord == queryWord)
                        numCaseMatches++;
                    if (matchScore > 0)
                    {
                        // more points for matching earlier available words
                        if (i < 20)
                            matchScore += 1;
                        if (i < 10)
                            matchScore += 1;
                        if (i == 0)
                            matchScore += 1;
                        // more points for longer queries
                        numSimilarities += matchScore + queryWord.Length;
                        itemWords.RemoveRange(0, i + 1);
                        thisQueryWordMatches = true;
                        break;
                    }
                    numUnmatchedWords++;
                }
                // a query was unmatched
                if (!thisQueryWordMatches)
                    return 0;
            }

            if (query.EndsWith(" ") && numSimilarities > 0)
            {
                if (itemWords.Count > 0)
                {
                    // If the query ends with " ", then prefer to match items that have more words remaining
                    numSimilarities++;
                    itemWords.RemoveAt(0);
                }
                else
                {
                    numUnmatchedWords++;
                }
            }

            numUnmatchedWords += itemWords.Count;

            // Usually, case doesn't matter, but it is theoretically possible for a user to have two activities with the same name except casing
            // However, it may be likely for a user to have two different activities containing the same word with different casings,
            // and in that case we don't want to consider the casing difference to be significant unless the user has typed a long query
            // So, if all words were matched, then we also give points for correct casing
            if (numUnmatchedWords == 0)
                numSimilarities += numCaseMatches;

            return numSimilarities;
        }
    }
    
}
