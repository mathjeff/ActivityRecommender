using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;
using System.Windows.Media;
using System.Windows.Controls;
using VisiPlacement;
using Windows.Storage.Pickers;
using System.IO.IsolatedStorage;
using System.Reflection;
using Windows.Phone.UI.Input;
using System.ComponentModel;

// the ActivityRecommender class is the main class that connects the user-interface to the Engine
namespace ActivityRecommendation
{
    class ActivityRecommender
    {
        public ActivityRecommender(ContentControl newMainWindow)
        {
            this.mainWindow = newMainWindow;

            this.InitializeSettings();

            this.MakeEngine();

            //this.ReadTempFile();

            this.SetupDrawing();

            if (this.engineTester != null)
            {
                this.engineTester.Finish(); // do any cleanup calculations and print results
                System.Diagnostics.Debug.WriteLine("");
            }
        }

        // call this to do cleanup immediately before this object gets destroyed
        public void ShutDown()
        {
            //this.SuspectLatestActionDate(DateTime.Now);
            if (!this.recentUserData.Synchronized)
                this.WriteRecentUserData();
        }

        private void InitializeSettings()
        {
            this.ratingsFileName = "ActivityRatings.txt";
            this.inheritancesFileName = "ActivityInheritances.txt";
            this.tempFileName = "TemporaryData.txt";
            this.textConverter = new TextConverter(this);
            //this.engineTester = new EngineTester();

            // allocate memory here so we don't have null references when we try to update it in response to the engine making changes
            this.participationEntryView = new ParticipationEntryView();
            this.recentUserData = new RecentUserData();
        }

        private void SetupDrawing()
        {
            LayoutStack layoutStack = new LayoutStack();

            //GridLayout unevenDisplayGrid = GridLayout.New(new BoundProperty_List(1), new BoundProperty_List(4), LayoutScore.Get_UnCentered_LayoutScore(4));
            //GridLayout evenDisplayGrid = GridLayout.New(new BoundProperty_List(1), BoundProperty_List.Uniform(4), LayoutScore.Zero);
            //GridLayout mainGrid = GridLayout.New(new BoundProperty_List(1), new BoundProperty_List(3), LayoutScore.Zero);
            //GridLayout leftGrid = GridLayout.New(new BoundProperty_List(2), new BoundProperty_List(1), LayoutScore.Zero);
            //mainGrid.AddLayout(leftGrid);

            this.inheritanceEditingView = new InheritanceEditingView();
            this.inheritanceEditingView.ActivityDatabase = this.engine.ActivityDatabase;
            this.inheritanceEditingView.AddClickHandler(new RoutedEventHandler(this.SubmitInheritance));
            //mainGrid.AddLayout(this.inheritanceEditingView);
            //leftGrid.AddLayout(this.inheritanceEditingView);

            // this gets taken care of earlier so we don't get a null reference when we try to update it in response to the engine making changes
            //this.participationEntryView = new ParticipationEntryView();
            this.participationEntryView.Engine = this.engine;
            this.participationEntryView.ActivityDatabase = this.engine.ActivityDatabase;
            this.participationEntryView.AddOkClickHandler(new RoutedEventHandler(this.SubmitParticipation));
            this.participationEntryView.AddSetenddateHandler(new RoutedEventHandler(this.MakeEndNow));
            this.participationEntryView.AddSetstartdateHandler(new RoutedEventHandler(this.MakeStartNow));
            //this.participationEntryView.Background = new SolidColorBrush(Color.FromRgb(220, 220, 220));
            this.participationEntryView.LatestParticipation = this.latestParticipation;
            //evenDisplayGrid.AddLayout(this.participationEntryView);
            //unevenDisplayGrid.AddLayout(this.participationEntryView);
            //mainGrid.AddLayout(this.participationEntryView);
            this.UpdateDefaultParticipationData();

            this.suggestionsView = new SuggestionsView();
            this.suggestionsView.AddSuggestionClickHandler(new RoutedEventHandler(this.MakeRecommendation));
            this.suggestionsView.ActivityDatabase = this.engine.ActivityDatabase;
            //this.suggestionsView.Background = new SolidColorBrush(Color.FromRgb(210, 210, 210));
            //evenDisplayGrid.AddLayout(this.suggestionsView);
            //unevenDisplayGrid.AddLayout(this.suggestionsView);
            //mainGrid.AddLayout(this.suggestionsView);

            /*
            this.statisticsMenu = new MiniStatisticsMenu();
            this.statisticsMenu.ActivityDatabase = this.engine.ActivityDatabase;
            this.statisticsMenu.AddOkClickHandler(new RoutedEventHandler(this.VisualizeData));
            leftGrid.AddLayout(this.statisticsMenu);
            */


            this.mainWindow.KeyDown += new System.Windows.Input.KeyEventHandler(mainWindow_KeyDown);


            MenuLayoutBuilder usageMenu_builder = new MenuLayoutBuilder(layoutStack);
            usageMenu_builder.AddLayout("Add New Activities", this.inheritanceEditingView);
            usageMenu_builder.AddLayout("Record Participations", this.participationEntryView);
            usageMenu_builder.AddLayout("Get Suggestions", this.suggestionsView);
            LayoutChoice_Set usageMenu = usageMenu_builder.Build();


            MenuLayoutBuilder introMenu_builder = new MenuLayoutBuilder(layoutStack);
            string newline = Environment.NewLine;
            string[] helpTexts = {"Press your phone's Back button when finished.",
                                     "This ActivityRecommender can give you suggestions for what to do now, based on time-stamped data from you about what you've done recently and how much you liked it",
                                     "First you must enter some activities to choose from. Go to Add New Activities, type the name of the activity, and consider making it be a subcategory of another " +
                                     "activity. For example, you might enter 'Computer Programming' as the child activity and enter 'Useful' as the parent activity",
                                     "If you're typing an activity into a box and you want to use the suggested value below it, press the Enter button. If you intentionally or accidentally type an " +
                                     "activity that isn't known to this program yet, then it will be created automatically for you.",
                                     "Then you can ask for suggestions! Go to Get Suggestions for some ideas if you're in a hurry.",
                                     "Also be sure take a look at the Record Participations screen, and give feedback about how much you did or didn't like certain things that you did.",
                                     "This version of this application does not use the internet and does not report your entries to anyone.",
                                     "Visit https://github.com/mathjeff/ActivityRecommender-WPhone for more information and to contribute. Thanks! - Jeffry Gaston"};

            LinkedList<TextblockLayout> helpBoxes = new LinkedList<TextblockLayout>();
            GridLayout helpLayout = GridLayout.New(BoundProperty_List.Uniform(helpTexts.Length), BoundProperty_List.Uniform(1), LayoutScore.Zero);
            foreach (string message in helpTexts)
            {
                helpLayout.AddLayout(new TextblockLayout(message));
            }


            introMenu_builder.AddLayout("Help", helpLayout);
            introMenu_builder.AddLayout("Start", usageMenu);
            LayoutChoice_Set helpOrStart_menu = introMenu_builder.Build();


            layoutStack.AddLayout(helpOrStart_menu);


            this.mainLayout = new LayoutCache(layoutStack);
            this.layoutStack = layoutStack;
                        
            this.displayManager = new ViewManager(this.mainWindow, mainLayout);

        }

        public void GoBack(object sender, CancelEventArgs e)
        {
            if (this.layoutStack.GoBack())
                e.Cancel = true;
        }


        private void MakeEngine()
        {
            this.engine = new Engine();
            this.ReadEngineFiles();
            //this.engine.FullUpdate(); // this causes this engine to categorize a bunch of data but it takes a while and we don't want to do it right away
            this.engine.CreateActivities();
            this.suppressDownvoteOnRecommendation = true;        // the first time they get a recommendation, we don't treat it as a skip of the latest suggestion
        }
        public void PrepareEngine()
        {
            //this.engine.MakeRecommendation(DateTime.Parse("2012-02-25T17:00:00"));
            this.engine.MakeRecommendation();
        }
        private void ReadEngineFiles()
        {
            //this.CleanDataIfNecessary();
            this.WriteDataIfMissing();

            System.Diagnostics.Debug.WriteLine("Starting to read files");
            this.textConverter.ReadFile(this.inheritancesFileName);
            this.textConverter.ReadFile(this.ratingsFileName);
            this.textConverter.ReadFile(this.tempFileName);
            System.Diagnostics.Debug.WriteLine("Done parsing files");
            //this.textConverter.ReformatFile(this.ratingsFileName, "reformatted.txt");
        }

        // writes pre-loaded data to disk
        private void WriteDataIfMissing()
        {
            //this.WriteInheritancesIfMissing();
            //this.WriteParticipationsIfMissing();
        }

        private void WriteInheritancesIfMissing()
        {
            throw new NotImplementedException();
            /*
            if (this.textConverter.FileExists(this.inheritancesFileName))
                return;
            string ratings = ActivityRecommendation.Resources.AppResources.Default_ActivityInheritances;
            StreamWriter writer = this.textConverter.OpenFileForAppending(this.inheritancesFileName);
            writer.Write(ratings);
            writer.Close();
             * */
        }
        private void WriteParticipationsIfMissing()
        {
            throw new NotImplementedException();
            /*
            if (this.textConverter.FileExists(this.ratingsFileName))
                return;
            string inheritances = ActivityRecommendation.Resources.AppResources.Default_ActivityRatings;
            StreamWriter writer = this.textConverter.OpenFileForAppending(this.ratingsFileName);
            writer.Write(inheritances);
            writer.Close();
            */
        }
        private void MakeRecommendation()
        {
#if TEST_UI
            DateTime now = DateTime.Parse("3000-01-01T00:00:00");
#else
            DateTime now = DateTime.Now;
#endif
            this.SuspectLatestActionDate(now);
            //now = DateTime.Parse("2012-07-30T00:00:00").AddSeconds(this.counter);

            
            //now = this.engine.LatestInteractionDate.AddSeconds(1);
            if ((this.LatestRecommendedActivity != null) && !this.suppressDownvoteOnRecommendation)
            {
                //now = DateTime.Parse("2012-07-30T00:00:00").AddSeconds(this.counter + 1);
                // if they've asked for two recommendations in succession, it means they didn't like the previous one

                // Calculate the score to generate for this Activity as a result of that statement
                Distribution previousDistribution = this.LatestRecommendedActivity.PredictedScore.Distribution;
                double estimatedScore = previousDistribution.Mean - previousDistribution.StdDev;
                if (estimatedScore < 0)
                    estimatedScore = 0;
                // make a Skip object holding the needed data
                ActivitySkip skip = new ActivitySkip(now, this.LatestRecommendedActivity.MakeDescriptor());
                //skip.SuggestionDate = this.recentUserData.LatestSuggestion.StartDate;
                skip.SuggestionDate = this.LatestSuggestion.StartDate;
                //skip.SuggestionDate = this.LatestRecommendedActivity.PredictedScore.ApplicableDate;
                AbsoluteRating rating = new AbsoluteRating();
                rating.Score = estimatedScore;
                skip.RawRating = rating;
                this.AddSkip(skip);
            }
            this.suppressDownvoteOnRecommendation = false;
            
            // if they requested that the first suggestion be from a certain category, find that category
            Activity requestCategory = null;
            string categoryText = this.suggestionsView.CategoryText;
            if (categoryText != null && categoryText != "")
            {
                ActivityDescriptor categoryDescriptor = new ActivityDescriptor();
                categoryDescriptor.ActivityName = categoryText;
                categoryDescriptor.PreferHigherProbability = true;
                requestCategory = this.engine.ActivityDatabase.ResolveDescriptor(categoryDescriptor);

                ActivityRequest request = new ActivityRequest(requestCategory.MakeDescriptor(), now);
                this.AddActivityRequest(request);
            }

            // try a few different values of greediness, so we can tell the user what's best overall and what's best now
            //Composite_ActivitySuggestion completeSuggestion = new Composite_ActivitySuggestion(null, new List<ActivitySuggestion>());
            List<ActivitySuggestion> suggestions = new List<ActivitySuggestion>();
            for (double greediness = 0; greediness < 1; greediness += 1)
            {
                int i;
                DateTime suggestionDate = now;
                List<Participation> fakeParticipations = new List<Participation>();
                // because the engine takes some time to become fast, we keep track of how many suggestions we've asked for, and we ask for suggestions increasingly more frequently
                int desiredNumUniqueSuggestions = this.numSuggestionsPerRequest;
                int numCategoriesToConsider = desiredNumUniqueSuggestions + 2;
                if (this.numSuggestionsPerRequest < 4)
                    this.numSuggestionsPerRequest++;
                int maxNumIterations = desiredNumUniqueSuggestions;
                //Composite_ActivitySuggestion parentSuggestion = completeSuggestion;
                for (i = 0; i < maxNumIterations; i++)
                {
                    // now determine which category to predict from
                    Activity bestActivity = null;
                    if (requestCategory != null && i == 0)
                    {
                        // now we get a recommendation, from among all activities within this category
                        //bestActivity = this.engine.MakeRecommendation(requestCategory, suggestionDate, greediness);
                        bestActivity = this.engine.MakeFastRecommendation(requestCategory, suggestionDate, numCategoriesToConsider);
                    }
                    else
                    {
                        // now we get a recommendation
                        //bestActivity = this.engine.MakeRecommendation(suggestionDate, greediness);
                        bestActivity = this.engine.MakeFastRecommendation();
                    }
                    // if there are no matching activities, then give up
                    if (bestActivity == null)
                        break;
                    // after making a recommendation, get the rest of the details of the suggestion
                    // (Note that eventually the suggested duration will be calculated in a more intelligent manner than simply taking the average duration)
                    Participation participationSummary = bestActivity.SummarizeParticipationsBetween(new DateTime(), DateTime.Now);
                    double typicalNumSeconds = Math.Exp(participationSummary.LogActiveTime.Mean);
                    DateTime endDate = suggestionDate.Add(TimeSpan.FromSeconds(typicalNumSeconds));
                    Composite_ActivitySuggestion suggestion = new Composite_ActivitySuggestion(bestActivity.MakeDescriptor(), new List<ActivitySuggestion>());
                    suggestion.CreatedDate = now;
                    suggestion.StartDate = suggestionDate;
                    suggestion.EndDate = endDate;
                    suggestion.ParticipationProbability = bestActivity.PredictedParticipationProbability.Distribution.Mean;
                    suggestion.PredictedScore = bestActivity.PredictedScore.Distribution;

                    //parentSuggestion.AddChild(suggestion);
                    //parentSuggestion = suggestion;
                    if (suggestions.Count() != 0 && suggestions.Last().ActivityDescriptor.ActivityName.Equals(suggestion.ActivityDescriptor.ActivityName) && (i != maxNumIterations - 1 || suggestions.Count() > 1))
                    {
                        // if two consecutive suggestions are for the same activity, then just extend the end date of the previous to make it faster to read
                        suggestions.Last().EndDate = endDate;
                    }
                    else
                    {
                        suggestions.Add(suggestion);
                    }

                    if (i == 0 && greediness == 0)
                    {
                        if (bestActivity != null)
                        {
                            // autofill the participationEntryView with a convenient value
                            this.participationEntryView.SetActivityName(bestActivity.Name);

                            // keep track of the latest suggestion that applies to the same date at which it was created
                            this.LatestSuggestion = suggestion;

                            this.WriteSuggestion(suggestion);
                        }
                    }

                    if (suggestions.Count == desiredNumUniqueSuggestions)
                    {
                        break;
                    }
                    //suggestions.Add(suggestion);

                    if (i != maxNumIterations - 1)
                    {
                        // pretend that the user took our suggestion and tell that to the engine
                        Participation fakeParticipation = new Participation(suggestion.StartDate, suggestion.EndDate.Value, bestActivity.MakeDescriptor());
                        fakeParticipation.Hypothetical = true;
                        this.engine.PutParticipationInMemory(fakeParticipation);
                        fakeParticipations.Add(fakeParticipation);
                    }

                    // only the first suggestion is based on the requested category
                    requestCategory = null;

                    // update the next suggestion for the date at which the user will need another activity to do
                    suggestionDate = endDate;

                }

                // Reset the engine to the state it was in originally by removing the pretend participations
                foreach (Participation fakeParticipation in fakeParticipations)
                {
                    this.engine.RemoveParticipation(fakeParticipation);
                }

            }

            //Composite_ActivitySuggestion completeSuggestion = new Composite_ActivitySuggestion(null, null);
            this.suggestionsView.Suggestions = suggestions;
            if (this.LatestSuggestion != null)
            {
                // we have to manually tell the engine about its suggestion because sometimes we don't want to record the suggestion (like when preparing the engine, for speed)
                this.engine.PutSuggestionInMemory(this.LatestSuggestion);
                //this.suggestionsView.Suggestion = completeSuggestion;
            }

        }
        private void MakeRecommendation(object sender, EventArgs e)
        {
            this.MakeRecommendation();
        }
        private void WriteSuggestion(ActivitySuggestion suggestion)
        {
            string text = this.textConverter.ConvertToString(suggestion) + Environment.NewLine;
            StreamWriter writer = this.textConverter.OpenFileForAppending(this.ratingsFileName);
            writer.Write(text);
            writer.Close();
        }
        private void SubmitParticipation(object sender, EventArgs e)
        {
            this.SubmitParticipation();
        }
        private void SubmitParticipation()
        {
            // give the participation to the engine
            Participation participation = this.participationEntryView.GetParticipation(this.engine.ActivityDatabase, this.engine);
            if (participation == null)
                return;

            if (this.LatestRecommendedActivity != null && this.ActivityDatabase.Matches(participation.ActivityDescriptor, this.LatestRecommendedActivity))
                participation.Suggested = true;
            else
                participation.Suggested = false;
            this.AddParticipation(participation);
            // fill in some default data for the ParticipationEntryView
            //this.latestActionDate = new DateTime(0);
            this.participationEntryView.Clear();
            this.UpdateDefaultParticipationData();
            this.SuppressDownvoteOnRecommendation();

            // give the information to the appropriate activities
            this.engine.ApplyParticipationsAndRatings();
        }
        private void MakeEndNow(object sender, EventArgs e)
        {
            this.MakeEndNow();
        }
        private void MakeEndNow()
        {
            DateTime now = DateTime.Now;
            this.LatestActionDate = now;
            this.participationEntryView.SetEnddateNow(now);
            /*
            // first update the dates
            this.UpdateDefaultParticipationData();
            // now fill-in the latest activity name
            string latestName = "";
            if (this.latestRecommendedActivity != null)
            {
                latestName = this.latestRecommendedActivity.Name;
            }
            this.participationEntryView.ActivityName = latestName;
            */
        }
        private void AddParticipation(Participation newParticipation)
        {
            this.PutParticipationInMemory(newParticipation);
            this.WriteParticipation(newParticipation);
        }
        private void WriteParticipation(Participation newParticipation)
        {
            string text = this.textConverter.ConvertToString(newParticipation) + Environment.NewLine;
            StreamWriter writer = this.textConverter.OpenFileForAppending(this.ratingsFileName);
            writer.Write(text);
            writer.Close();
        }
        // declares that the user didn't want to do something that was suggested
        private void AddSkip(ActivitySkip newSkip)
        {
            this.PutSkipInMemory(newSkip);
            this.WriteSkip(newSkip);
        }
        // writes this Skip to a data file
        private void WriteSkip(ActivitySkip newSkip)
        {
            string text = this.textConverter.ConvertToString(newSkip) + Environment.NewLine;
            StreamWriter writer = this.textConverter.OpenFileForAppending(this.ratingsFileName);
            writer.Write(text);
            writer.Close();
        }
        // writes this ActivityRequest to a data file
        private void AddActivityRequest(ActivityRequest newRequest)
        {
            this.engine.PutActivityRequestInMemory(newRequest);
            this.WriteActivityRequest(newRequest);
        }
        private void WriteActivityRequest(ActivityRequest newRequest)
        {
            string text = this.textConverter.ConvertToString(newRequest) + Environment.NewLine;
            StreamWriter writer = this.textConverter.OpenFileForAppending(this.ratingsFileName);
            writer.Write(text);
            writer.Close();
        }
        private void SubmitInheritance(object sender, EventArgs e)
        {
            this.SubmitInheritance();
        }
        private void SubmitInheritance()
        {
            Inheritance inheritance = new Inheritance();
            inheritance.DiscoveryDate = DateTime.Now;

            ActivityDescriptor childDescriptor = new ActivityDescriptor();
            childDescriptor.ActivityName = this.inheritanceEditingView.ChildName;
            inheritance.ChildDescriptor = childDescriptor;

            ActivityDescriptor parentDescriptor = new ActivityDescriptor();
            parentDescriptor.ActivityName = this.inheritanceEditingView.ParentName;
            inheritance.ParentDescriptor = parentDescriptor;

            //this.engine.AddInheritance(inheritance);
            this.AddInheritance(inheritance);

            this.inheritanceEditingView.ChildName = "";
            this.inheritanceEditingView.ParentName = "";

            this.SuppressDownvoteOnRecommendation();
        }
        private void AddInheritance(Inheritance newInheritance)
        {
            //this.engine.PutInheritanceInMemory(newInheritance);
            this.engine.ApplyInheritance(newInheritance);
            this.WriteInheritance(newInheritance);
        }
        public void WriteInheritance(Inheritance newInheritance)
        {
            StreamWriter writer =  this.textConverter.OpenFileForAppending(this.inheritancesFileName);
            string text = this.textConverter.ConvertToString(newInheritance) + Environment.NewLine;
            writer.Write(text);
            writer.Close();
        }
        private void AddRating(Rating newRating)
        {
            AbsoluteRating absoluteRating = (AbsoluteRating)newRating;
            if (absoluteRating != null)
            {
                this.engine.PutRatingInMemory(absoluteRating);
                this.WriteRating(absoluteRating);
            }
        }
        private void WriteRating(AbsoluteRating newRating)
        {
            string text = this.textConverter.ConvertToString(newRating) + Environment.NewLine;
            StreamWriter writer = this.textConverter.OpenFileForAppending(this.ratingsFileName);
            writer.Write(text);
            writer.Close();
        }

        // writes to a text file saying that the user was is this program now. It gets deleted soon
        #region Functions to be called by the TextConverter

        public void PutParticipationInMemory(Participation newParticipation)
        {
            if (this.latestParticipation == null || newParticipation.EndDate.CompareTo(this.latestParticipation.EndDate) > 0)
            {
                this.latestParticipation = newParticipation;
                if (this.participationEntryView != null)
                    this.participationEntryView.LatestParticipation = this.latestParticipation;
            }
            this.engine.PutParticipationInMemory(newParticipation);
            if (this.engineTester != null)
                this.engineTester.AddParticipation(newParticipation);
        }
        public void PutRatingInMemory(Rating newRating)
        {
            this.engine.PutRatingInMemory(newRating);
            if (this.engineTester != null)
                this.engineTester.AddRating(newRating);
        }
        public void PutSkipInMemory(ActivitySkip newSkip)
        {
            this.counter++;
            this.engine.PutSkipInMemory(newSkip);
            if (this.engineTester != null)
                this.engineTester.AddSkip(newSkip);
        }
        public void PutActivityRequestInMemory(ActivityRequest newRequest)
        {
            this.engine.PutActivityRequestInMemory(newRequest);
            if (this.engineTester != null)
                this.engineTester.AddRequest(newRequest);
        }
        public void PutActivityDescriptorInMemory(ActivityDescriptor newDescriptor)
        {
            this.engine.PutActivityDescriptorInMemory(newDescriptor);
        }
        public void PutInheritanceInMemory(Inheritance newInheritance)
        {
            this.engine.PutInheritanceInMemory(newInheritance);
            if (this.engineTester != null)
                this.engineTester.AddInheritance(newInheritance);
        }
        // updates the ParticipationEntryView so that the start date is DateTime.Now
        public void MakeStartNow(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            this.SuspectLatestActionDate(now);
        }
        // updates the ParticipationEntryView so that the start date is 'when'
        public void SuspectLatestActionDate(DateTime when)
        {
            this.LatestActionDate = when;
            //this.WriteInteractionDate(when);
            this.UpdateDefaultParticipationData(when);
        }
        // sets the given RecentUserData
        public void SetRecentUserData(RecentUserData data)
        {
            this.recentUserData = data;
            if (this.LatestSuggestion != null)
                this.latestRecommendedActivity = this.ActivityDatabase.ResolveDescriptor(this.LatestSuggestion.ActivityDescriptor);
        }
        public void PutSuggestionInMemory(ActivitySuggestion suggestion)
        {
            this.engine.PutSuggestionInMemory(suggestion);
            if (this.engineTester != null)
                this.engineTester.AddSuggestion(suggestion);
        }
        public DateTime LatestActionDate
        {
            get
            {
                // The latest date will usually be from this.recentUserData, but in case it gets deleted, we also compare against this.engine.LatestInteractionDate
                DateTime date1;
                if (this.recentUserData.LatestActionDate == null)
                    date1 = new DateTime(0);
                else
                    date1 = (DateTime)this.recentUserData.LatestActionDate;

                DateTime date2 = this.engine.LatestInteractionDate;
                if (date1.CompareTo(date2) > 0)
                    return date1;
                else
                    return date2;
            }
            set
            {
                this.recentUserData.LatestActionDate = value;
                //this.WriteRecentUserData();
            }
        }
        public Activity LatestRecommendedActivity
        {
            get
            {
                return this.latestRecommendedActivity;
            }
        }
        public ActivitySuggestion LatestSuggestion
        {
            get
            {
                return this.recentUserData.LatestSuggestion;
            }
            set
            {
                this.recentUserData.LatestSuggestion = value;
                this.latestRecommendedActivity = this.ActivityDatabase.ResolveDescriptor(value.ActivityDescriptor);
                //this.WriteRecentUserData();
            }
        }
        public void WriteRecentUserData()
        {
            this.recentUserData.Synchronized = true;
            string text = this.textConverter.ConvertToString(this.recentUserData) + Environment.NewLine;
            StreamWriter writer = this.textConverter.OpenFileForAppending(this.tempFileName);
            writer.Write(text);
            writer.Close();
        }


        public void VisualizeData(object sender, EventArgs e)
        {
            this.VisualizeData();
        }
        public void VisualizeData()
        {
            //string name = this.statisticsMenu.ActivityName;

            //ActivityDescriptor xAxisDescriptor = this.statisticsMenu.XAxisActivityDescriptor;
            IProgression xAxisProgression = this.statisticsMenu.XAxisProgression;
            ActivityDescriptor yAxisDescriptor = this.statisticsMenu.YAxisActivityDescriptor;
            //Activity xAxisActivity = null;
            Activity yAxisActivity = null;
            /*
            if (xAxisDescriptor != null)
            {
                xAxisActivity = this.engine.ActivityDatabase.ResolveDescriptor(xAxisDescriptor);
            }
            */
            if (yAxisDescriptor != null)
            {
                yAxisActivity = this.engine.ActivityDatabase.ResolveDescriptor(yAxisDescriptor);
            }
            if (yAxisActivity != null)
            {
                // TODO: fix this
                ActivityVisualizationView visualizationView = new ActivityVisualizationView(xAxisProgression, yAxisActivity, UserPreferences.DefaultPreferences.HalfLife, this.engine.RatingSummarizer);
                visualizationView.AddExitClickHandler(new RoutedEventHandler(this.ShowMainview));
                //this.displayManager
                this.displayManager.SetLayout(visualizationView);
            }
        }

        void mainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                this.ShowMainview();
            }
        }

        public void ShowMainview(object sender, EventArgs e)
        {
            this.ShowMainview();
        }
        public void ShowMainview()
        {
            //this.mainDisplay.PutItem(this.mainDisplayGrid, 1, 0);
            this.displayManager.SetLayout(this.mainLayout);
            //this.displayManager.InvalidateMeasure();
            //this.displayManager.SetContent(this.mainDisplayGrid);
        }

        public ActivityDatabase ActivityDatabase
        {
            get
            {
                return this.engine.ActivityDatabase;
            }
        }
        #endregion
        // declares that the user did something that means if they ask for another recommendation then we should not downvote the latest one
        private void SuppressDownvoteOnRecommendation()
        {
            //this.latestRecommendationDate = new DateTime(0);
            this.suppressDownvoteOnRecommendation = true;
            this.suggestionsView.ResetText();
        }
        // fills in some default data for the ParticipationEntryView
        private void UpdateDefaultParticipationData()
        {
            DateTime now = DateTime.Now;
            this.UpdateDefaultParticipationData(now);
        }
        private void UpdateDefaultParticipationData(DateTime when)
        {
            //this.participationEntryView.Clear();
            DateTime startDate = this.LatestActionDate;
            DateTime endDate = when;
            if (startDate.Day != endDate.Day)
            {
                // it's more helpful for the default end-date to be on the same day
                endDate = new DateTime(startDate.Year, startDate.Month, startDate.Day, startDate.Hour, startDate.Minute, startDate.Second);
            }
            this.participationEntryView.EndDate = endDate;
            this.participationEntryView.SetStartDate(startDate);
            //this.participationEntryView.ActivityName = "";
            //this.participationEntryView.RatingText = "";
            //this.participationEntryView.CommentText = "";
        }


        ContentControl mainWindow;
        ViewManager displayManager;
        //GridLayout mainDisplayGrid;
        LayoutChoice_Set mainLayout;
        //GridLayou mainDisplay;

        ParticipationEntryView participationEntryView;
        InheritanceEditingView inheritanceEditingView;
        SuggestionsView suggestionsView;
        MiniStatisticsMenu statisticsMenu;
        Engine engine;
        //DateTime latestRecommendationDate;
        Activity latestRecommendedActivity;
        bool suppressDownvoteOnRecommendation;
        TextConverter textConverter;
        string ratingsFileName;         // the name of the file that stores ratings
        string inheritancesFileName;    // the name of the file that stores inheritances
        string tempFileName;
        //DateTime latestActionDate;
        Participation latestParticipation;
        EngineTester engineTester;
        RecentUserData recentUserData;
        int counter = 0;
        // ActivityDatabase primedActivities; // activities that have already been considered and therefore are fast to consider again
        int numSuggestionsPerRequest = 1;
        LayoutStack layoutStack;

    }
}