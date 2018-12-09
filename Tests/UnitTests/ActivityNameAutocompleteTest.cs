using ActivityRecommendation;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace UnitTests
{
    // tests of the autocomplete used by ActivityNameEntryBox 
    public class ActivityNameAutocompleteTest
    {
        private ActivityDatabase newDatabase()
        {
            return new ActivityDatabase(null, null);
        }

        [Fact]
        public void Test_WordOrdering_MoreImportantThan_WordCasing()
        {
            // setup
            ActivityDatabase activityDatabase = newDatabase();
            Category a = activityDatabase.CreateCategory(new ActivityDescriptor("Doing Computer Programming"));
            activityDatabase.CreateCategory(new ActivityDescriptor("Something about programming computers"));
            // test
            ActivityDescriptor query = new ActivityDescriptor("c p");
            query.RequiresPerfectMatch = false;
            Activity result = activityDatabase.ResolveDescriptor(query);
            if (result != a)
            {
                throw new Exception("ResolveDescriptor did not prioritize word ordering over word casing");
            }
        }

        [Fact]
        public void Test_WordPosition_MoreImportantThan_WordCasing()
        {
            // setup
            ActivityDatabase activityDatabase = newDatabase();
            activityDatabase.CreateCategory(new ActivityDescriptor("Doing work on things"));
            Category b = activityDatabase.CreateCategory(new ActivityDescriptor("Work on stuff"));
            // test
            ActivityDescriptor query = new ActivityDescriptor("work");
            query.RequiresPerfectMatch = false;
            Activity result = activityDatabase.ResolveDescriptor(query);
            if (result != b)
            {
                throw new Exception("ResolveDescriptor did not prioritize word position over word casing");
            }
        }

        [Fact]
        public void Test_ResultMustShareLetters()
        {
            // setup
            ActivityDatabase activityDatabase = newDatabase();
            activityDatabase.CreateCategory(new ActivityDescriptor("Sample Activity One"));
            activityDatabase.CreateCategory(new ActivityDescriptor("Another thing to do"));
            // test
            ActivityDescriptor query = new ActivityDescriptor("Nonexistent");
            query.RequiresPerfectMatch = false;
            Activity result = activityDatabase.ResolveDescriptor(query);
            if (result != null)
            {
                throw new Exception("ResolveDescriptor returned a non-null result (" + result + ") with no overlapping words");
            }
        }
    }
}