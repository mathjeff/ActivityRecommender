using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    // an ActivityImportLayout allows the user to browse activities that are likely to be interesting to the user
    // and to accept them more quickly than having to enter them via the ActivityCreationLayout
    class ActivityImportLayout : ContainerLayout
    {
        private static List<Inheritance> defaultInheritances = null;
        private static string inheritancesText = @"
<Inheritance><Parent><Name>Activity</Name></Parent><Child><Name>Sleeping</Name></Child></Inheritance>
<Inheritance><Parent><Name>Activity</Name></Parent><Child><Name>Shopping</Name></Child></Inheritance>
<Inheritance><Parent><Name>Activity</Name></Parent><Child><Name>Fun</Name></Child></Inheritance>
<Inheritance><Parent><Name>Activity</Name></Parent><Child><Name>Interesting</Name></Child></Inheritance>
<Inheritance><Parent><Name>Activity</Name></Parent><Child><Name>Reading</Name></Child></Inheritance>
<Inheritance><Parent><Name>Activity</Name></Parent><Child><Name>Useful</Name></Child></Inheritance>
<Inheritance><Parent><Name>Chores</Name></Parent><Child><Name>Cleaning</Name></Child></Inheritance>
<Inheritance><Parent><Name>Shopping</Name></Parent><Child><Name>Buying Food</Name></Child></Inheritance>
<Inheritance><Parent><Name>Shopping</Name></Parent><Child><Name>Visiting a Restaurant</Name></Child></Inheritance>
<Inheritance><Parent><Name>Visiting a Restaurant</Name></Parent><Child><Name>Visiting a Greek Restaurant</Name></Child></Inheritance>
<Inheritance><Parent><Name>Visiting a Restaurant</Name></Parent><Child><Name>Visiting a Italian Restaurant</Name></Child></Inheritance>
<Inheritance><Parent><Name>Visiting a Restaurant</Name></Parent><Child><Name>Visiting a Chinese Restaurant</Name></Child></Inheritance>
<Inheritance><Parent><Name>Visiting a Restaurant</Name></Parent><Child><Name>Visiting a Mexican Restaurant</Name></Child></Inheritance>
<Inheritance><Parent><Name>Visiting a Restaurant</Name></Parent><Child><Name>Visiting a Seafood Restaurant</Name></Child></Inheritance>
<Inheritance><Parent><Name>Visiting a Restaurant</Name></Parent><Child><Name>Visiting a Korean Restaurant</Name></Child></Inheritance>
<Inheritance><Parent><Name>Visiting a Restaurant</Name></Parent><Child><Name>Visiting a Vegetarian Restaurant</Name></Child></Inheritance>
<Inheritance><Parent><Name>Visiting a Restaurant</Name></Parent><Child><Name>Visiting a Thai Restaurant</Name></Child></Inheritance>
<Inheritance><Parent><Name>Visiting a Restaurant</Name></Parent><Child><Name>Visiting a Italian Restaurant</Name></Child></Inheritance>
<Inheritance><Parent><Name>Visiting a Restaurant</Name></Parent><Child><Name>Visiting a Breakfast Restaurant</Name></Child></Inheritance>
<Inheritance><Parent><Name>Visiting a Restaurant</Name></Parent><Child><Name>Visiting a Greek Restaurant</Name></Child></Inheritance>
<Inheritance><Parent><Name>Visiting a Restaurant</Name></Parent><Child><Name>Visiting a Burger Restaurant</Name></Child></Inheritance>
<Inheritance><Parent><Name>Visiting a Restaurant</Name></Parent><Child><Name>Visiting a Pizza Restaurant</Name></Child></Inheritance>
<Inheritance><Parent><Name>Shopping</Name></Parent><Child><Name>Buying Sneakers</Name></Child></Inheritance>
<Inheritance><Parent><Name>Chores</Name></Parent><Child><Name>Getting my Hair Cut</Name></Child></Inheritance>
<Inheritance><Parent><Name>Cleaning</Name></Parent><Child><Name>Hygiene</Name></Child></Inheritance>
<Inheritance><Parent><Name>Cleaning</Name></Parent><Child><Name>Cleaning my Room</Name></Child></Inheritance>
<Inheritance><Parent><Name>Cleaning</Name></Parent><Child><Name>Cleaning the Bathroom Sink</Name></Child></Inheritance>
<Inheritance><Parent><Name>Cleaning</Name></Parent><Child><Name>Emptying the Dishwasher</Name></Child></Inheritance>
<Inheritance><Parent><Name>Cleaning</Name></Parent><Child><Name>Doing the Dishes</Name></Child></Inheritance>
<Inheritance><Parent><Name>Cleaning</Name></Parent><Child><Name>Mopping the Floor</Name></Child></Inheritance>
<Inheritance><Parent><Name>Exercise</Name></Parent><Child><Name>Biking</Name></Child></Inheritance>
<Inheritance><Parent><Name>Exercise</Name></Parent><Child><Name>Hiking</Name></Child></Inheritance>
<Inheritance><Parent><Name>Exercise</Name></Parent><Child><Name>Camping</Name></Child></Inheritance>
<Inheritance><Parent><Name>Exercise</Name></Parent><Child><Name>Playing in the Snow</Name></Child></Inheritance>
<Inheritance><Parent><Name>Exercise</Name></Parent><Child><Name>Sledding</Name></Child></Inheritance>
<Inheritance><Parent><Name>Exercise</Name></Parent><Child><Name>Frisbee</Name></Child></Inheritance>
<Inheritance><Parent><Name>Exercise</Name></Parent><Child><Name>Miniature Golf</Name></Child></Inheritance>
<Inheritance><Parent><Name>Exercise</Name></Parent><Child><Name>Ice Skating</Name></Child></Inheritance>
<Inheritance><Parent><Name>Exercise</Name></Parent><Child><Name>Jogging</Name></Child></Inheritance>
<Inheritance><Parent><Name>Exercise</Name></Parent><Child><Name>Running</Name></Child></Inheritance>
<Inheritance><Parent><Name>Exercise</Name></Parent><Child><Name>Ping Pong</Name></Child></Inheritance>
<Inheritance><Parent><Name>Exercise</Name></Parent><Child><Name>Pushups</Name></Child></Inheritance>
<Inheritance><Parent><Name>Exercise</Name></Parent><Child><Name>Soccer</Name></Child></Inheritance>
<Inheritance><Parent><Name>Exercise</Name></Parent><Child><Name>Situps</Name></Child></Inheritance>
<Inheritance><Parent><Name>Exercise</Name></Parent><Child><Name>Curl Ups</Name></Child></Inheritance>
<Inheritance><Parent><Name>Exercise</Name></Parent><Child><Name>Swimming</Name></Child></Inheritance>
<Inheritance><Parent><Name>Exercise</Name></Parent><Child><Name>Volleyball</Name></Child></Inheritance>
<Inheritance><Parent><Name>Exercise</Name></Parent><Child><Name>Tennis</Name></Child></Inheritance>
<Inheritance><Parent><Name>Exercise</Name></Parent><Child><Name>Yardwork</Name></Child></Inheritance>
<Inheritance><Parent><Name>Exercise</Name></Parent><Child><Name>Walking</Name></Child></Inheritance>
<Inheritance><Parent><Name>Exercise</Name></Parent><Child><Name>Piggy Back Ride</Name></Child></Inheritance>
<Inheritance><Parent><Name>Exercise</Name></Parent><Child><Name>Laser Tag</Name></Child></Inheritance>
<Inheritance><Parent><Name>Exercise</Name></Parent><Child><Name>Yoga</Name></Child></Inheritance>
<Inheritance><Parent><Name>Exercise</Name></Parent><Child><Name>Dancing</Name></Child></Inheritance>
<Inheritance><Parent><Name>Exercise</Name></Parent><Child><Name>Climbing</Name></Child></Inheritance>
<Inheritance><Parent><Name>Fun</Name></Parent><Child><Name>Appreciating my Accomplishments</Name></Child></Inheritance>
<Inheritance><Parent><Name>Fun</Name></Parent><Child><Name>Game</Name></Child></Inheritance>
<Inheritance><Parent><Name>Fun</Name></Parent><Child><Name>Making a Costume</Name></Child></Inheritance>
<Inheritance><Parent><Name>Fun</Name></Parent><Child><Name>Music</Name></Child></Inheritance>
<Inheritance><Parent><Name>Fun</Name></Parent><Child><Name>Playing in the Snow</Name></Child></Inheritance>
<Inheritance><Parent><Name>Fun</Name></Parent><Child><Name>Pleasure Reading</Name></Child></Inheritance>
<Inheritance><Parent><Name>Fun</Name></Parent><Child><Name>Watching Television</Name></Child></Inheritance>
<Inheritance><Parent><Name>Fun</Name></Parent><Child><Name>Thinking of a Joke</Name></Child></Inheritance>
<Inheritance><Parent><Name>Fun</Name></Parent><Child><Name>Paper Airplanes</Name></Child></Inheritance>
<Inheritance><Parent><Name>Fun</Name></Parent><Child><Name>Visiting an Amusement Park</Name></Child></Inheritance>
<Inheritance><Parent><Name>Watching Television</Name></Parent><Child><Name>Watching an Action Show</Name></Child></Inheritance>
<Inheritance><Parent><Name>Watching Television</Name></Parent><Child><Name>Watching a Drama Show</Name></Child></Inheritance>
<Inheritance><Parent><Name>Watching Television</Name></Parent><Child><Name>Watching Cartoons</Name></Child></Inheritance>
<Inheritance><Parent><Name>Watching Television</Name></Parent><Child><Name>Watching Comedy</Name></Child></Inheritance>
<Inheritance><Parent><Name>Watching Television</Name></Parent><Child><Name>Watching a Romance Show</Name></Child></Inheritance>
<Inheritance><Parent><Name>Watching Television</Name></Parent><Child><Name>Watching a Horrow Show</Name></Child></Inheritance>
<Inheritance><Parent><Name>Watching Television</Name></Parent><Child><Name>Watching a Documentary</Name></Child></Inheritance>
<Inheritance><Parent><Name>Watching Television</Name></Parent><Child><Name>Watching a Musical</Name></Child></Inheritance>
<Inheritance><Parent><Name>Watching Television</Name></Parent><Child><Name>Watching a Sitcom</Name></Child></Inheritance>
<Inheritance><Parent><Name>Watching Television</Name></Parent><Child><Name>Watching a Game Show</Name></Child></Inheritance>
<Inheritance><Parent><Name>Watching Television</Name></Parent><Child><Name>Watching Opera</Name></Child></Inheritance>
<Inheritance><Parent><Name>Fun</Name></Parent><Child><Name>Relaxing</Name></Child></Inheritance>
<Inheritance><Parent><Name>Game</Name></Parent><Child><Name>Board/Card Game</Name></Child></Inheritance>
<Inheritance><Parent><Name>Board/Card Game</Name></Parent><Child><Name>Strategy Board Game</Name></Child></Inheritance>
<Inheritance><Parent><Name>Board/Card Game</Name></Parent><Child><Name>Cooperative Board Game</Name></Child></Inheritance>
<Inheritance><Parent><Name>Board/Card Game</Name></Parent><Child><Name>Party Game</Name></Child></Inheritance>
<Inheritance><Parent><Name>Party Game</Name></Parent><Child><Name>Word Game</Name></Child></Inheritance>
<Inheritance><Parent><Name>Game</Name></Parent><Child><Name>Cryptograms</Name></Child></Inheritance>
<Inheritance><Parent><Name>Game</Name></Parent><Child><Name>Computer Game</Name></Child></Inheritance>
<Inheritance><Parent><Name>Computer Game</Name></Parent><Child><Name>Strategy Computer Game</Name></Child></Inheritance>
<Inheritance><Parent><Name>Strategy Computer Game</Name></Parent><Child><Name>4X Game</Name></Child></Inheritance>
<Inheritance><Parent><Name>Computer Game</Name></Parent><Child><Name>Tower Defense Game</Name></Child></Inheritance>
<Inheritance><Parent><Name>Computer Game</Name></Parent><Child><Name>Action Computer Game</Name></Child></Inheritance>
<Inheritance><Parent><Name>Computer Game</Name></Parent><Child><Name>Racing Game</Name></Child></Inheritance>
<Inheritance><Parent><Name>Computer Game</Name></Parent><Child><Name>Music Game</Name></Child></Inheritance>
<Inheritance><Parent><Name>Music</Name></Parent><Child><Name>Music Game</Name></Child></Inheritance>
<Inheritance><Parent><Name>Computer Game</Name></Parent><Child><Name>Story Computer Game</Name></Child></Inheritance>
<Inheritance><Parent><Name>Computer Game</Name></Parent><Child><Name>Role-Playing Game</Name></Child></Inheritance>
<Inheritance><Parent><Name>Game</Name></Parent><Child><Name>Video Game</Name></Child></Inheritance>
<Inheritance><Parent><Name>Video Game</Name></Parent><Child><Name>Sidescroller</Name></Child></Inheritance>
<Inheritance><Parent><Name>Interesting</Name></Parent><Child><Name>Analyzing/Optimizing my Life</Name></Child></Inheritance>
<Inheritance><Parent><Name>Interesting</Name></Parent><Child><Name>Fixing Something</Name></Child></Inheritance>
<Inheritance><Parent><Name>Interesting</Name></Parent><Child><Name>Learning Something</Name></Child></Inheritance>
<Inheritance><Parent><Name>Interesting</Name></Parent><Child><Name>Learning a Language</Name></Child></Inheritance>
<Inheritance><Parent><Name>Interesting</Name></Parent><Child><Name>Taking a Class</Name></Child></Inheritance>
<Inheritance><Parent><Name>Interesting</Name></Parent><Child><Name>Listening to a Podcast</Name></Child></Inheritance>
<Inheritance><Parent><Name>Interesting</Name></Parent><Child><Name>Listening to an Audiobook</Name></Child></Inheritance>
<Inheritance><Parent><Name>Interesting</Name></Parent><Child><Name>Investing</Name></Child></Inheritance>
<Inheritance><Parent><Name>Interesting</Name></Parent><Child><Name>Social</Name></Child></Inheritance>
<Inheritance><Parent><Name>Interesting</Name></Parent><Child><Name>Technology</Name></Child></Inheritance>
<Inheritance><Parent><Name>Interesting</Name></Parent><Child><Name>Trying Something New</Name></Child></Inheritance>
<Inheritance><Parent><Name>Interesting</Name></Parent><Child><Name>Watching a Presentation</Name></Child></Inheritance>
<Inheritance><Parent><Name>Interesting</Name></Parent><Child><Name>Puzzles</Name></Child></Inheritance>
<Inheritance><Parent><Name>Interesting</Name></Parent><Child><Name>Working on a Math Problem</Name></Child></Inheritance>
<Inheritance><Parent><Name>Interesting</Name></Parent><Child><Name>Exploring</Name></Child></Inheritance>
<Inheritance><Parent><Name>Interesting</Name></Parent><Child><Name>Visiting a Science Museum</Name></Child></Inheritance>
<Inheritance><Parent><Name>Interesting</Name></Parent><Child><Name>Stargazing</Name></Child></Inheritance>
<Inheritance><Parent><Name>Interesting</Name></Parent><Child><Name>Art</Name></Child></Inheritance>
<Inheritance><Parent><Name>Art</Name></Parent><Child><Name>Drawing</Name></Child></Inheritance>
<Inheritance><Parent><Name>Art</Name></Parent><Child><Name>Painting</Name></Child></Inheritance>
<Inheritance><Parent><Name>Art</Name></Parent><Child><Name>Writing a Story</Name></Child></Inheritance>
<Inheritance><Parent><Name>Art</Name></Parent><Child><Name>Visiting an Art Museum</Name></Child></Inheritance>
<Inheritance><Parent><Name>Music</Name></Parent><Child><Name>Listening to Music</Name></Child></Inheritance>
<Inheritance><Parent><Name>Music</Name></Parent><Child><Name>Watching a Music Video</Name></Child></Inheritance>
<Inheritance><Parent><Name>Music</Name></Parent><Child><Name>Singing</Name></Child></Inheritance>
<Inheritance><Parent><Name>Music</Name></Parent><Child><Name>Playing an Instrument</Name></Child></Inheritance>
<Inheritance><Parent><Name>Listening to Music</Name></Parent><Child><Name>Attending a Concert</Name></Child></Inheritance>
<Inheritance><Parent><Name>Physical Activity</Name></Parent><Child><Name>Exercise</Name></Child></Inheritance>
<Inheritance><Parent><Name>Physical Activity</Name></Parent><Child><Name>Stretching</Name></Child></Inheritance>
<Inheritance><Parent><Name>Physical Activity</Name></Parent><Child><Name>Visiting an Amusement Park</Name></Child></Inheritance>
<Inheritance><Parent><Name>Playing in the Snow</Name></Parent><Child><Name>Sledding</Name></Child></Inheritance>
<Inheritance><Parent><Name>Reading</Name></Parent><Child><Name>Pleasure Reading</Name></Child></Inheritance>
<Inheritance><Parent><Name>Reading</Name></Parent><Child><Name>Reading a Nonfiction Book</Name></Child></Inheritance>
<Inheritance><Parent><Name>Reading a Nonfiction Book</Name></Parent><Child><Name>Reading a Self-Improvement Book</Name></Child></Inheritance>
<Inheritance><Parent><Name>Reading a Nonfiction Book</Name></Parent><Child><Name>Reading a History Book</Name></Child></Inheritance>
<Inheritance><Parent><Name>Reading a Nonfiction Book</Name></Parent><Child><Name>Reading a Science Book</Name></Child></Inheritance>
<Inheritance><Parent><Name>Reading a Nonfiction Book</Name></Parent><Child><Name>Reading a Math Book</Name></Child></Inheritance>
<Inheritance><Parent><Name>Pleasure Reading</Name></Parent><Child><Name>Reading a Webcomic</Name></Child></Inheritance>
<Inheritance><Parent><Name>Pleasure Reading</Name></Parent><Child><Name>Reading a Fiction Book</Name></Child></Inheritance>
<Inheritance><Parent><Name>Reading a Fiction Book</Name></Parent><Child><Name>Reading a SciFi Book</Name></Child></Inheritance>
<Inheritance><Parent><Name>Reading a Fiction Book</Name></Parent><Child><Name>Reading a Fantasy Book</Name></Child></Inheritance>
<Inheritance><Parent><Name>Reading a Fiction Book</Name></Parent><Child><Name>Reading a Fairy Tale</Name></Child></Inheritance>
<Inheritance><Parent><Name>Reading a Fiction Book</Name></Parent><Child><Name>Reading a Romance Book</Name></Child></Inheritance>
<Inheritance><Parent><Name>Reading a Fiction Book</Name></Parent><Child><Name>Reading a Mystery</Name></Child></Inheritance>
<Inheritance><Parent><Name>Reading a Fiction Book</Name></Parent><Child><Name>Reading a Horror Book</Name></Child></Inheritance>
<Inheritance><Parent><Name>Reading a Fiction Book</Name></Parent><Child><Name>Reading a Short Story</Name></Child></Inheritance>
<Inheritance><Parent><Name>Reading a Fiction Book</Name></Parent><Child><Name>Reading a Comic Book</Name></Child></Inheritance>
<Inheritance><Parent><Name>Reading a Fiction Book</Name></Parent><Child><Name>Reading Realistic Fiction</Name></Child></Inheritance>
<Inheritance><Parent><Name>Pleasure Reading</Name></Parent><Child><Name>Reading a Joke Book</Name></Child></Inheritance>
<Inheritance><Parent><Name>Reading</Name></Parent><Child><Name>Reading the News</Name></Child></Inheritance>
<Inheritance><Parent><Name>Social</Name></Parent><Child><Name>Hanging Out with Friends</Name></Child></Inheritance>
<Inheritance><Parent><Name>Hanging Out with Friends</Name></Parent><Child><Name>Spending Time with Family</Name></Child></Inheritance>
<Inheritance><Parent><Name>Hanging Out with Friends</Name></Parent><Child><Name>Hanging Out with Coworkers</Name></Child></Inheritance>
<Inheritance><Parent><Name>Social</Name></Parent><Child><Name>Browsing Social Media</Name></Child></Inheritance>
<Inheritance><Parent><Name>Technology</Name></Parent><Child><Name>Computer Programming</Name></Child></Inheritance>
<Inheritance><Parent><Name>Computer Programming</Name></Parent><Child><Name>Working on a Game</Name></Child></Inheritance>
<Inheritance><Parent><Name>Computer Programming</Name></Parent><Child><Name>Useful Software Project</Name></Child></Inheritance>
<Inheritance><Parent><Name>Computer Programming</Name></Parent><Child><Name>Social Software Project</Name></Child></Inheritance>
<Inheritance><Parent><Name>Technology</Name></Parent><Child><Name>Working on a Math Problem</Name></Child></Inheritance>
<Inheritance><Parent><Name>Technology</Name></Parent><Child><Name>Working on ActivityRecommender</Name></Child></Inheritance>
<Inheritance><Parent><Name>Useful</Name></Parent><Child><Name>Cooking</Name></Child></Inheritance>
<Inheritance><Parent><Name>Cooking</Name></Parent><Child><Name>Making Smoothie</Name></Child></Inheritance>
<Inheritance><Parent><Name>Cooking</Name></Parent><Child><Name>Making Sandwiches</Name></Child></Inheritance>
<Inheritance><Parent><Name>Useful</Name></Parent><Child><Name>Fixing Something</Name></Child></Inheritance>
<Inheritance><Parent><Name>Useful</Name></Parent><Child><Name>Investing</Name></Child></Inheritance>
<Inheritance><Parent><Name>Useful</Name></Parent><Child><Name>Physical Activity</Name></Child></Inheritance>
<Inheritance><Parent><Name>Useful</Name></Parent><Child><Name>Reading the News</Name></Child></Inheritance>
<Inheritance><Parent><Name>Useful</Name></Parent><Child><Name>Seeing a Doctor</Name></Child></Inheritance>
<Inheritance><Parent><Name>Useful</Name></Parent><Child><Name>Seeing the Dentist</Name></Child></Inheritance>
<Inheritance><Parent><Name>Useful</Name></Parent><Child><Name>Technology</Name></Child></Inheritance>
<Inheritance><Parent><Name>Useful</Name></Parent><Child><Name>Helping Someone</Name></Child></Inheritance>
<Inheritance><Parent><Name>Useful</Name></Parent><Child><Name>Work</Name></Child></Inheritance>
<Inheritance><Parent><Name>Watching Television</Name></Parent><Child><Name>Watching a Movie</Name></Child></Inheritance>
<Inheritance><Parent><Name>Work</Name></Parent><Child><Name>Checking Email</Name></Child></Inheritance>
<Inheritance><Parent><Name>Work</Name></Parent><Child><Name>Chores</Name></Child></Inheritance>
<Inheritance><Parent><Name>Chores</Name></Parent><Child><Name>Decorating</Name></Child></Inheritance>
<Inheritance><Parent><Name>Chores</Name></Parent><Child><Name>Mowing the Lawn</Name></Child></Inheritance>
<Inheritance><Parent><Name>Work</Name></Parent><Child><Name>Employment</Name></Child></Inheritance>
<Inheritance><Parent><Name>Work</Name></Parent><Child><Name>Homework</Name></Child></Inheritance>
<Inheritance><Parent><Name>Work</Name></Parent><Child><Name>Job Searching</Name></Child></Inheritance>
<Inheritance><Parent><Name>Work</Name></Parent><Child><Name>Laundry</Name></Child></Inheritance>
<Inheritance><Parent><Name>Work</Name></Parent><Child><Name>Paperwork</Name></Child></Inheritance>
<Inheritance><Parent><Name>Work</Name></Parent><Child><Name>Working on a Gift</Name></Child></Inheritance>
<Inheritance><Parent><Name>Work</Name></Parent><Child><Name>Yardwork</Name></Child></Inheritance>
<Inheritance><Parent><Name>Work</Name></Parent><Child><Name>Helping Someone Move</Name></Child></Inheritance>
<Inheritance><Parent><Name>Work</Name></Parent><Child><Name>Gardening</Name></Child></Inheritance>
";

        private List<Inheritance> DefaultInheritances
        {
            get
            {
                if (defaultInheritances == null)
                {
                    defaultInheritances = InheritancesParser.Parse(new StringReader(inheritancesText));
                }
                return defaultInheritances;
            }
        }

        private Dictionary<string, List<Inheritance>> InheritancesByParent
        {
            get
            {
                if (this.inheritancesByParent == null)
                {
                    Dictionary<string, List<Inheritance>> inheritancesByParent = new Dictionary<string, List<Inheritance>>();
                    foreach (Inheritance inheritance in this.DefaultInheritances)
                    {
                        string parent = inheritance.ParentDescriptor.ActivityName;
                        if (!inheritancesByParent.ContainsKey(parent))
                        {
                            inheritancesByParent.Add(parent, new List<Inheritance>());
                        }
                        inheritancesByParent[parent].Add(inheritance);
                    }
                    this.inheritancesByParent = inheritancesByParent;
                }
                return this.inheritancesByParent;
            }
        }

        public ActivityImportLayout(ActivityDatabase activityDatabase, LayoutStack layoutStack)
        {
            this.activityDatabase = activityDatabase;
            this.layoutStack = layoutStack;
            this.dismissedInheritances = new HashSet<Inheritance>();
            this.regenerateEntries();
        }

        public List<AppFeature> GetFeatures()
        {
            return new List<AppFeature>() { new ImportPremadeActivity_Feature(this.activityDatabase) };
        }
        private void regenerateEntries()
        {
            List<Inheritance> unchosenInheritances = this.findUnchosenInheritances();
            List<Inheritance> immediatelyUsableInheritances = this.selectUsableInheritances(unchosenInheritances);
            if (immediatelyUsableInheritances.Count < 1)
            {
                if (this.dismissedInheritances.Count > 0)
                    this.SubLayout = new TextblockLayout("You have already accepted or dismissed all of the built-in activity inheritances. Go back to the Add Activities screen to enter your own!");
                else
                    this.SubLayout = new TextblockLayout("You have already accepted all of the built-in activity inheritances. Go back to the Add Activities screen to enter your own!");
            }
            else
            {
                GridLayout_Builder gridBuilder = new Vertical_GridLayout_Builder().Uniform();
                LayoutChoice_Set helpLayout = new HelpWindowBuilder()
                    .AddMessage("ActivityRecommender needs to know what you like to do, for providing suggestions, autocomplete, and more.")
                    .AddMessage("Do you like any of the activites here?")
                    .AddMessage("Selecting 'I like this' on an item of the form 'child (parent)' indicates two things: A: that you believe that the child is encompassed by the parent, and B: that the child is relevant to you.")
                    .AddMessage("Pushing the 'I like all kinds' button is equivalent to pushing the 'I like this' button on the given activity and all of its descendants.")
                    .AddMessage("Pushing the 'Not Interested' button will simply hide the given activity in this screen")
                    .Build();

                gridBuilder.AddLayout(new TextblockLayout("Press everything you like to do! Then go back.\n" +
                    "" + unchosenInheritances.Count + " remaining, built-in activity ideas:", 30).AlignHorizontally(TextAlignment.Center));

                int count = 0;
                foreach (Inheritance inheritance in immediatelyUsableInheritances)
                {
                    int numDescendants = this.FindDescendants(inheritance).Count;
                    Import_SpecificActivities_Layout specific = new Import_SpecificActivities_Layout(inheritance, numDescendants);
                    specific.Dismissed += DismissedInheritance;
                    specific.AcceptedSingle += AcceptedSingleInheritance;
                    specific.AcceptedAll += AcceptedInheritanceRecursive;
                    gridBuilder.AddLayout(specific);
                    // Don't display too many results at once to avoid taking to long to update the screen.
                    // The user should dismiss the ones they're not interested in, anyway
                    count++;
                    if (count >= 10)
                    {
                        gridBuilder.AddLayout(new TextblockLayout("Dismiss or accept an idea to see more!"));
                        break;
                    }
                }
                gridBuilder.AddLayout(new HelpButtonLayout(helpLayout, this.layoutStack, 30));

                this.SubLayout = ScrollLayout.New(gridBuilder.Build());
            }
        }

        // returns a list containing this inheritance plus all other inheritances having this one as an ancestor
        private List<Inheritance> FindDescendants(Inheritance parent)
        {
            List<Inheritance> results = new List<Inheritance>();
            HashSet<Inheritance> resultSet = new HashSet<Inheritance>();
            results.Add(parent);
            resultSet.Add(parent);
            for (int i = 0; i < results.Count; i++)
            {
                Inheritance child = results[i];
                string parentName = child.ChildDescriptor.ActivityName;
                if (this.InheritancesByParent.ContainsKey(parentName))
                {
                    foreach (Inheritance descendant in this.InheritancesByParent[parentName])
                    {
                        if (!resultSet.Contains(descendant))
                        {
                            resultSet.Add(descendant);
                            results.Add(descendant);
                        }
                    }
                }
            }
            return results;
        }
        private void AcceptedInheritanceRecursive(Inheritance inheritance)
        {
            this.accept(this.FindDescendants(inheritance));
        }

        private void AcceptedSingleInheritance(Inheritance inheritance)
        {
            this.accept(new List<Inheritance>() { inheritance });
        }
        private void DismissedInheritance(Inheritance inheritance)
        {
            this.dismissedInheritances.Add(inheritance);
            this.regenerateEntries();
        }

        private void accept(List<Inheritance> inheritances)
        {
            DateTime now = DateTime.Now;
            foreach (Inheritance descendant in inheritances)
            {
                descendant.DiscoveryDate = now;
                this.activityDatabase.AddInheritance(descendant);
            }
            this.regenerateEntries();
        }

        private List<Inheritance> findUnchosenInheritances()
        {
            List<Inheritance> result = new List<Inheritance>();
            foreach (Inheritance inheritance in DefaultInheritances)
            {
                if (!activityDatabase.HasActivity(inheritance.ChildDescriptor))
                {
                    result.Add(inheritance);
                }
            }
            return result;
        }
        private List<Inheritance> selectUsableInheritances(List<Inheritance> candidates)
        {
            List<Inheritance> result = new List<Inheritance>();
            foreach (Inheritance inheritance in candidates)
            {
                if (!activityDatabase.HasActivity(inheritance.ParentDescriptor))
                {
                    // parent must exist before child can be added
                    continue;
                }
                if (this.dismissedInheritances.Contains(inheritance))
                {
                    // skip displaying any that the user has asked to hide
                    continue;
                }
                result.Add(inheritance);
            }
            return result;
        }

        private ActivityDatabase activityDatabase;
        private LayoutStack layoutStack;
        private HashSet<Inheritance> dismissedInheritances;
        private Dictionary<string, List<Inheritance>> inheritancesByParent;
    }

    class Import_SpecificActivities_Layout : ContainerLayout
    {
        public event Request_DismissInheritance Dismissed;
        public delegate void Request_DismissInheritance(Inheritance inheritance);

        public event Request_AcceptSingleInheritance AcceptedSingle;
        public delegate void Request_AcceptSingleInheritance(Inheritance inheritance);

        public event Request_AcceptAllInheritances AcceptedAll;
        public delegate void Request_AcceptAllInheritances(Inheritance inheritance);
        public Import_SpecificActivities_Layout(Inheritance inheritance, int numDescendants)
        {
            this.inheritance = inheritance;

            TextblockLayout title = new TextblockLayout(inheritance.ChildDescriptor.ActivityName + " (" + inheritance.ParentDescriptor.ActivityName + ")", 30);
            title.AlignVertically(TextAlignment.Center);

            Button selectAll_button = new Button();
            selectAll_button.Clicked += SelectAll_button_Clicked;
            Button customizeButton = new Button();
            customizeButton.Clicked += CustomizeButton_Clicked;
            Button dismissButton = new Button();
            dismissButton.Clicked += DismissButton_Clicked;
            GridLayout_Builder buttonsBuilder = new Horizontal_GridLayout_Builder().Uniform();
            if (numDescendants > 1)
            {
                buttonsBuilder.AddLayout(new ButtonLayout(selectAll_button, "I like all kinds! (" + numDescendants + " ideas)", 16));
                buttonsBuilder.AddLayout(new ButtonLayout(customizeButton, "I like this. Show me more!", 16));
            }
            else
            {
                buttonsBuilder.AddLayout(new ButtonLayout(customizeButton, "I like this!", 16));
            }

            buttonsBuilder.AddLayout(new ButtonLayout(dismissButton, "Not interested", 16));

            this.SubLayout = new Vertical_GridLayout_Builder()
                .AddLayout(title)
                .AddLayout(buttonsBuilder.Build())
                .Build();
        }

        private void DismissButton_Clicked(object sender, EventArgs e)
        {
            if (this.Dismissed != null)
                this.Dismissed.Invoke(this.inheritance);
        }

        private void CustomizeButton_Clicked(object sender, EventArgs e)
        {
            if (this.AcceptedSingle != null)
                this.AcceptedSingle.Invoke(this.inheritance);
        }

        private void SelectAll_button_Clicked(object sender, EventArgs e)
        {
            if (this.AcceptedAll != null)
                this.AcceptedAll.Invoke(this.inheritance);
        }

        Inheritance inheritance;
    }

    class ImportPremadeActivity_Feature : AppFeature
    {
        public ImportPremadeActivity_Feature(ActivityDatabase activityDatabase)
        {
            this.activityDatabase = activityDatabase;
        }

        public string GetDescription()
        {
            return "Import a premade activity";
        }

        public bool GetHasBeenUsed()
        {
            foreach (string name in new List<string>() { "Sleeping", "Buying Something", "Reading", "Fun", "Useful", "Interesting" })
            {
                if (this.activityDatabase.ResolveDescriptor(new ActivityDescriptor(name)) != null)
                    return true;
            }
            return false;
        }

        public bool GetIsUsable()
        {
            return true;
        }

        ActivityDatabase activityDatabase;
    }
}
