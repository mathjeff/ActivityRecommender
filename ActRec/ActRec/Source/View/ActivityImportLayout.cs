using System;
using System.Collections.Generic;
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
<Inheritance><Parent><Name>Activity</Name></Parent><Child><Name>Buying Something</Name></Child></Inheritance>
<Inheritance><Parent><Name>Activity</Name></Parent><Child><Name>Fun</Name></Child></Inheritance>
<Inheritance><Parent><Name>Activity</Name></Parent><Child><Name>Interesting</Name></Child></Inheritance>
<Inheritance><Parent><Name>Activity</Name></Parent><Child><Name>Reading</Name></Child></Inheritance>
<Inheritance><Parent><Name>Activity</Name></Parent><Child><Name>Useful</Name></Child></Inheritance>
<Inheritance><Parent><Name>Chores</Name></Parent><Child><Name>Cleaning</Name></Child></Inheritance>
<Inheritance><Parent><Name>Chores</Name></Parent><Child><Name>Buying Food</Name></Child></Inheritance>
<Inheritance><Parent><Name>Chores</Name></Parent><Child><Name>Buying Sneakers</Name></Child></Inheritance>
<Inheritance><Parent><Name>Chores</Name></Parent><Child><Name>Getting my Hair Cut</Name></Child></Inheritance>
<Inheritance><Parent><Name>Cleaning</Name></Parent><Child><Name>Cleaning my Room</Name></Child></Inheritance>
<Inheritance><Parent><Name>Cleaning</Name></Parent><Child><Name>Cleaning the Bathroom Sink</Name></Child></Inheritance>
<Inheritance><Parent><Name>Cleaning</Name></Parent><Child><Name>Emptying the Dishwasher</Name></Child></Inheritance>
<Inheritance><Parent><Name>Cleaning</Name></Parent><Child><Name>Mopping the Floor</Name></Child></Inheritance>
<Inheritance><Parent><Name>Exercise</Name></Parent><Child><Name>Biking</Name></Child></Inheritance>
<Inheritance><Parent><Name>Exercise</Name></Parent><Child><Name>Playing in the Snow</Name></Child></Inheritance>
<Inheritance><Parent><Name>Exercise</Name></Parent><Child><Name>Frisbee</Name></Child></Inheritance>
<Inheritance><Parent><Name>Exercise</Name></Parent><Child><Name>Miniature Golf</Name></Child></Inheritance>
<Inheritance><Parent><Name>Exercise</Name></Parent><Child><Name>Ice Skating</Name></Child></Inheritance>
<Inheritance><Parent><Name>Exercise</Name></Parent><Child><Name>Jogging</Name></Child></Inheritance>
<Inheritance><Parent><Name>Exercise</Name></Parent><Child><Name>Ping Pong</Name></Child></Inheritance>
<Inheritance><Parent><Name>Exercise</Name></Parent><Child><Name>Pushups</Name></Child></Inheritance>
<Inheritance><Parent><Name>Exercise</Name></Parent><Child><Name>Rock Climbing</Name></Child></Inheritance>
<Inheritance><Parent><Name>Exercise</Name></Parent><Child><Name>Soccer</Name></Child></Inheritance>
<Inheritance><Parent><Name>Exercise</Name></Parent><Child><Name>Situps</Name></Child></Inheritance>
<Inheritance><Parent><Name>Exercise</Name></Parent><Child><Name>Swimming</Name></Child></Inheritance>
<Inheritance><Parent><Name>Exercise</Name></Parent><Child><Name>Volleyball</Name></Child></Inheritance>
<Inheritance><Parent><Name>Exercise</Name></Parent><Child><Name>Yardwork</Name></Child></Inheritance>
<Inheritance><Parent><Name>Exercise</Name></Parent><Child><Name>Walking</Name></Child></Inheritance>
<Inheritance><Parent><Name>Fun</Name></Parent><Child><Name>Appreciating my Accomplishments</Name></Child></Inheritance>
<Inheritance><Parent><Name>Fun</Name></Parent><Child><Name>Game</Name></Child></Inheritance>
<Inheritance><Parent><Name>Fun</Name></Parent><Child><Name>Making a Costume</Name></Child></Inheritance>
<Inheritance><Parent><Name>Fun</Name></Parent><Child><Name>Music</Name></Child></Inheritance>
<Inheritance><Parent><Name>Fun</Name></Parent><Child><Name>Playing in the Snow</Name></Child></Inheritance>
<Inheritance><Parent><Name>Fun</Name></Parent><Child><Name>Pleasure Reading</Name></Child></Inheritance>
<Inheritance><Parent><Name>Fun</Name></Parent><Child><Name>Watching Television</Name></Child></Inheritance>
<Inheritance><Parent><Name>Game</Name></Parent><Child><Name>Board/Card Game</Name></Child></Inheritance>
<Inheritance><Parent><Name>Game</Name></Parent><Child><Name>Cryptograms</Name></Child></Inheritance>
<Inheritance><Parent><Name>Game</Name></Parent><Child><Name>Computer Game</Name></Child></Inheritance>
<Inheritance><Parent><Name>Game</Name></Parent><Child><Name>Video Game</Name></Child></Inheritance>
<Inheritance><Parent><Name>Interesting</Name></Parent><Child><Name>Analyzing/Optimizing my Life</Name></Child></Inheritance>
<Inheritance><Parent><Name>Interesting</Name></Parent><Child><Name>Fixing Something</Name></Child></Inheritance>
<Inheritance><Parent><Name>Interesting</Name></Parent><Child><Name>Learning a Language</Name></Child></Inheritance>
<Inheritance><Parent><Name>Interesting</Name></Parent><Child><Name>Listening to a Podcast</Name></Child></Inheritance>
<Inheritance><Parent><Name>Interesting</Name></Parent><Child><Name>Investing</Name></Child></Inheritance>
<Inheritance><Parent><Name>Interesting</Name></Parent><Child><Name>Social</Name></Child></Inheritance>
<Inheritance><Parent><Name>Interesting</Name></Parent><Child><Name>Technology</Name></Child></Inheritance>
<Inheritance><Parent><Name>Interesting</Name></Parent><Child><Name>Trying Something New</Name></Child></Inheritance>
<Inheritance><Parent><Name>Interesting</Name></Parent><Child><Name>Watching a Presentation</Name></Child></Inheritance>
<Inheritance><Parent><Name>Interesting</Name></Parent><Child><Name>Working on a Math Problem</Name></Child></Inheritance>
<Inheritance><Parent><Name>Interesting</Name></Parent><Child><Name>Writing a Story</Name></Child></Inheritance>
<Inheritance><Parent><Name>Music</Name></Parent><Child><Name>Listening to Music</Name></Child></Inheritance>
<Inheritance><Parent><Name>Physical Activity</Name></Parent><Child><Name>Exercise</Name></Child></Inheritance>
<Inheritance><Parent><Name>Physical Activity</Name></Parent><Child><Name>Stretching</Name></Child></Inheritance>
<Inheritance><Parent><Name>Playing in the Snow</Name></Parent><Child><Name>Sledding</Name></Child></Inheritance>
<Inheritance><Parent><Name>Reading</Name></Parent><Child><Name>Pleasure Reading</Name></Child></Inheritance>
<Inheritance><Parent><Name>Reading</Name></Parent><Child><Name>Reading a Nonfiction Book</Name></Child></Inheritance>
<Inheritance><Parent><Name>Pleasure Reading</Name></Parent><Child><Name>Reading a Webcomic</Name></Child></Inheritance>
<Inheritance><Parent><Name>Pleasure Reading</Name></Parent><Child><Name>Reading a Fiction Book</Name></Child></Inheritance>
<Inheritance><Parent><Name>Reading</Name></Parent><Child><Name>Reading the News</Name></Child></Inheritance>
<Inheritance><Parent><Name>Music</Name></Parent><Child><Name>Watching a Music Video</Name></Child></Inheritance>
<Inheritance><Parent><Name>Music</Name></Parent><Child><Name>Singing</Name></Child></Inheritance>
<Inheritance><Parent><Name>Social</Name></Parent><Child><Name>Hanging Out with Friends</Name></Child></Inheritance>
<Inheritance><Parent><Name>Social</Name></Parent><Child><Name>Browsing Social Media</Name></Child></Inheritance>
<Inheritance><Parent><Name>Technology</Name></Parent><Child><Name>Computer Programming</Name></Child></Inheritance>
<Inheritance><Parent><Name>Technology</Name></Parent><Child><Name>Working on a Math Problem</Name></Child></Inheritance>
<Inheritance><Parent><Name>Technology</Name></Parent><Child><Name>Working on ActivityRecommender</Name></Child></Inheritance>
<Inheritance><Parent><Name>Useful</Name></Parent><Child><Name>Cooking</Name></Child></Inheritance>
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
<Inheritance><Parent><Name>Work</Name></Parent><Child><Name>Employment</Name></Child></Inheritance>
<Inheritance><Parent><Name>Work</Name></Parent><Child><Name>Homework</Name></Child></Inheritance>
<Inheritance><Parent><Name>Work</Name></Parent><Child><Name>Job Searching</Name></Child></Inheritance>
<Inheritance><Parent><Name>Work</Name></Parent><Child><Name>Laundry</Name></Child></Inheritance>
<Inheritance><Parent><Name>Work</Name></Parent><Child><Name>Paperwork</Name></Child></Inheritance>
<Inheritance><Parent><Name>Work</Name></Parent><Child><Name>Working on a Gift</Name></Child></Inheritance>
<Inheritance><Parent><Name>Work</Name></Parent><Child><Name>Yardwork</Name></Child></Inheritance>
";

        private List<Inheritance> DefaultInheritances
        {
            get
            {
                if (defaultInheritances == null)
                {
                    defaultInheritances = InheritancesParser.Parse(inheritancesText);
                }
                return defaultInheritances;
            }
        }

        public ActivityImportLayout(ActivityDatabase activityDatabase, LayoutStack layoutStack)
        {
            this.activityDatabase = activityDatabase;
            this.layoutStack = layoutStack;
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
            this.entries = new Dictionary<Button, Inheritance>();
            if (immediatelyUsableInheritances.Count < 1)
            {
                this.SubLayout = new TextblockLayout("You have already accepted all of the built-in activity inheritances. There are none left to add!");
            }
            else
            {
                Vertical_GridLayout_Builder gridBuilder = new Vertical_GridLayout_Builder().Uniform();
                LayoutChoice_Set helpLayout = new HelpWindowBuilder()
                    .AddMessage("Before ActivityRecommender can give you any suggestions or can record your progress, it needs to know what kinds of things you like to do.")
                    .AddMessage("This screen helps you to get started in entering new activities you may want to do.")
                    .AddMessage("If you don't want to think of and type every activity and instead want to start with a bunch of pre-made activities, you can accept the ideas on this screen.")
                    .AddMessage("When you see an entry on this screen of the form 'child (parent)', then pressing that entry is the same as " +
                    "declaring that you believe that the parent activity encompasses the child activity and that the child activity is relevant to you.")
                    .AddMessage("For example, pressing an entry that says 'Board/Card Game (Game)' means that you think that board and card games are games, and it also means " +
                    "that you think board and card games are relevant to you.")
                    .Build();

                gridBuilder.AddLayout(new TextblockLayout("Press everything you like to do! Then go back.").AlignHorizontally(TextAlignment.Center));
                gridBuilder.AddLayout(new HelpButtonLayout(helpLayout, this.layoutStack));
                gridBuilder.AddLayout(new TextblockLayout("" + unchosenInheritances.Count + " remaining, built-in activity ideas:"));

                foreach (Inheritance inheritance in immediatelyUsableInheritances)
                {
                    Button button = new Button();
                    this.entries[button] = inheritance;
                    button.Clicked += Button_Clicked;
                    string text = inheritance.ChildDescriptor.ActivityName + " (" + inheritance.ParentDescriptor.ActivityName + ")";
                    ButtonLayout bigLayout = new ButtonLayout(button, text, 30);
                    ButtonLayout smallLayout = new ButtonLayout(button, text, 24);
                    gridBuilder.AddLayout(new LayoutUnion(bigLayout, smallLayout));
                }
                this.SubLayout = ScrollLayout.New(gridBuilder.Build());
            }
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            Button button = sender as Button;
            Inheritance inheritance = this.entries[button];
            inheritance.DiscoveryDate = DateTime.Now;
            this.activityDatabase.AddInheritance(inheritance);
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
                if (activityDatabase.HasActivity(inheritance.ParentDescriptor))
                {
                    result.Add(inheritance);
                }
            }
            return result;
        }

        private ActivityDatabase activityDatabase;
        private LayoutStack layoutStack;
        private Dictionary<Button, Inheritance> entries;
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
