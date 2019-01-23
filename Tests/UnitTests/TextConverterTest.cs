using ActivityRecommendation;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace UnitTests
{
    public class TextConverterTest
    {
        private TextConverter newTextConverter()
        {
            return new TextConverter(null, new ActivityDatabase(null, null));
        }

        [Fact]
        public void doTestImportExportConsistency()
        {
            this.testImportExportConsistency(
@"<RecentData><Date>2019-01-19T13:16:27</Date><Suggestions><Suggestion><Activity><Name>Activity1</Name></Activity><SuggestionDate>2019-01-19T13:42:52</SuggestionDate><StartDate>2019-01-19T13:47:23</StartDate><EndDate>2019-01-19T13:47:24</EndDate><Skippable>False</Skippable></Suggestion></Suggestions></RecentData>
<ToDo><Name>ToDo1</Name></ToDo>
<Category><Name>Activity1</Name></Category>
<Inheritance><Child><Name>Activity2</Name></Child><Parent><Name>Activity1</Name></Parent><DiscoveryDate>2018-10-22T22:08:50</DiscoveryDate></Inheritance>
<Metric><Name>Metric1</Name><Activity><Name>Activity2</Name></Activity></Metric>
<Participation><Activity><Name>Activity2</Name></Activity><StartDate>2018-10-16T19:00:00</StartDate><EndDate>2018-10-16T20:00:00</EndDate></Participation>
<Suggestion><Activity><Name>Activity2</Name></Activity><StartDate>2018-10-19T15:06:25</StartDate></Suggestion>
<Skip><Activity><Name>Activity2</Name></Activity><Date>2018-10-19T15:07:23</Date><SuggestionDate>2018-10-19T15:06:25</SuggestionDate></Skip>
<Request><Activity><Name>Activity1</Name></Activity><Date>2019-01-07T12:36:19</Date></Request>
<Experiment><Earlier><Activity><Name>Activity2</Name></Activity><Metric></Metric><SuccessRate>1</SuccessRate><Easiers>0</Easiers><Harders>0</Harders></Earlier><Later><Activity><Name>ToDo1</Name></Activity><Metric></Metric><SuccessRate>1</SuccessRate><Easiers>0</Easiers><Harders>0</Harders></Later></Experiment>
");
        }

        // Checks that an import of <text> followed by an export results in the same string
        private void testImportExportConsistency(string text)
        {
            text = text.Replace("\r", "").Trim();
            TextConverter textConverter = this.newTextConverter();
            PersistentUserData data = textConverter.ParseForImport(text);
            string regeneratedOutput = data.serialize();

            string diff = this.diffStrings(text, "input", regeneratedOutput, "regeneratedOutput");
            if (diff != "")
            {
                string message = "Loaded an input string, exported the result, and the two did not match.\n" + diff;
                throw new Exception(message);
            }

        }

        // Checks whether a == b, and if not, then returns a string describing the difference
        private string diffStrings(string a, string aName, string b, string bName)
        {
            if (String.Equals(a, b))
                return "";
            int firstDiff = -1;
            int maxIndex = Math.Min(a.Length, b.Length);
            for (int i = 0; i < maxIndex; i++)
            {
                if (a[i] != b[i])
                {
                    firstDiff = i;
                    break;
                }
            }
            if (firstDiff < 0)
            {
                if (a.Length < b.Length)
                {
                    string missingText = b.Substring(maxIndex, b.Length - maxIndex);
                    return aName + " is shorter than " + bName + " by these " + missingText.Length + " chars: '" + missingText + "'";
                }
                if (b.Length < a.Length)
                {
                    string missingText = a.Substring(maxIndex, a.Length - maxIndex);
                    return bName + " is shorter than " + aName + " by these " + missingText.Length + " chars: '" + missingText + "'";
                }
                return aName + " and " + bName + " are different";
            }
            int windowStart = Math.Max(firstDiff - 10, 0);
            int windowEnd = Math.Min(firstDiff + 20, maxIndex);
            int windowSize = windowEnd - windowStart;
            string substringA = a.Substring(windowStart, windowSize);
            string substringB = b.Substring(windowStart, windowSize);
            return aName + "[" + windowStart + ":" + windowEnd + "] ='" + substringA + "' whereas " +
                bName + "[" + windowStart + ":" + windowEnd + "] = '" + substringB + "'";
        }

    }
}
