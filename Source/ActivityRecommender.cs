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

            this.layoutStack = new LayoutStack();

            this.InitializeSettings();

            this.MakeEngine();

            //this.ReadTempFile();

            this.SetupDrawing();

            if (this.ratingReplayer != null)
            {
                this.ratingReplayer.Finish(); // do any cleanup calculations and print results
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
            //this.ratingReplayer = new EngineTester();
            //this.ratingReplayer = new RatingRenormalizer(this.textConverter);
            //this.ratingReplayer = new HistoryWriter(this.textConverter);

            // allocate memory here so we don't have null references when we try to update it in response to the engine making changes
            this.participationEntryView = new ParticipationEntryView(this.layoutStack);
            this.recentUserData = new RecentUserData();
        }

        private void SetupDrawing()
        {

            //GridLayout unevenDisplayGrid = GridLayout.New(new BoundProperty_List(1), new BoundProperty_List(4), LayoutScore.Get_UnCentered_LayoutScore(4));
            //GridLayout evenDisplayGrid = GridLayout.New(new BoundProperty_List(1), BoundProperty_List.Uniform(4), LayoutScore.Zero);
            //GridLayout mainGrid = GridLayout.New(new BoundProperty_List(1), new BoundProperty_List(3), LayoutScore.Zero);
            //GridLayout leftGrid = GridLayout.New(new BoundProperty_List(2), new BoundProperty_List(1), LayoutScore.Zero);
            //mainGrid.AddLayout(leftGrid);

            this.inheritanceEditingView = new InheritanceEditingView(this.layoutStack);
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

            this.suggestionsView = new SuggestionsView(this, this.layoutStack);
            this.suggestionsView.AddSuggestionClickHandler(new RoutedEventHandler(this.MakeRecommendation));
            this.suggestionsView.ActivityDatabase = this.engine.ActivityDatabase;
            //this.suggestionsView.Background = new SolidColorBrush(Color.FromRgb(210, 210, 210));
            //evenDisplayGrid.AddLayout(this.suggestionsView);
            //unevenDisplayGrid.AddLayout(this.suggestionsView);
            //mainGrid.AddLayout(this.suggestionsView);

            
            this.statisticsMenu = new MiniStatisticsMenu();
            this.statisticsMenu.ActivityDatabase = this.engine.ActivityDatabase;
            this.statisticsMenu.AddOkClickHandler(new RoutedEventHandler(this.VisualizeData));
            //leftGrid.AddLayout(this.statisticsMenu);
            

            this.dataExportView = new DataExportView();
            this.dataExportView.Add_ClickHandler(new RoutedEventHandler(this.ExportData));


            this.mainWindow.KeyDown += new System.Windows.Input.KeyEventHandler(mainWindow_KeyDown);


            MenuLayoutBuilder usageMenu_builder = new MenuLayoutBuilder(this.layoutStack);
            usageMenu_builder.AddLayout("Add New Activities", this.inheritanceEditingView);
            usageMenu_builder.AddLayout("Record Participations", this.participationEntryView);
            usageMenu_builder.AddLayout("Get Suggestions", this.suggestionsView);
            usageMenu_builder.AddLayout("View Statistics", this.statisticsMenu);
            usageMenu_builder.AddLayout("Export Data", this.dataExportView);
            LayoutChoice_Set usageMenu = usageMenu_builder.Build();


            MenuLayoutBuilder introMenu_builder = new MenuLayoutBuilder(this.layoutStack);
            LayoutChoice_Set helpLayout = (new HelpWindowBuilder()).AddMessage("Press your phone's Back button when finished.")
                .AddMessage("This ActivityRecommender can give you suggestions for what to do now, based on time-stamped data from you about what you've done recently and how much you liked it.")
                .AddMessage("Create some activities, log some participations, and ask for suggestions!")
                .AddMessage("This version of this application does not use the internet and does not report your entries to anyone.")
                .AddMessage("Visit https://github.com/mathjeff/ActivityRecommender-WPhone for more information and to contribute. Thanks!")
                .Build();


            introMenu_builder.AddLayout("Help", helpLayout);
            introMenu_builder.AddLayout("Start", usageMenu);
            LayoutChoice_Set helpOrStart_menu = introMenu_builder.Build();


            this.layoutStack.AddLayout(helpOrStart_menu);


            this.mainLayout = new LayoutCache(this.layoutStack);
                        
            this.displayManager = new ViewManager(this.mainWindow, this.mainLayout);

        }

        public void ExportData(object sender, EventArgs e)
        {
            string content = "";
            content += this.textConverter.ReadAllText(this.inheritancesFileName);
            content += this.textConverter.ReadAllText(this.ratingsFileName);
            this.textConverter.ExportFile("ActivityData.txt", content);
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
            this.engine.CreateNewActivities();

            this.PrepareEngine();
        }
        // Asks the engine to do some processing so that the next recommendation will be faster
        public void PrepareEngine()
        {
            this.engine.FullUpdate();
        }
        private void ReadEngineFiles()
        {
            System.Diagnostics.Debug.WriteLine("Starting to read files");
            this.textConverter.ReadFile(this.inheritancesFileName);
            this.textConverter.ReadFile(this.ratingsFileName);
            this.textConverter.ReadFile(this.tempFileName);
            System.Diagnostics.Debug.WriteLine("Done parsing files");
        }

        public void DeclineSuggestion(ActivitySuggestion suggestion)
        {
            // Calculate the score to generate for this Activity as a result of that statement
            Activity activity = this.ActivityDatabase.ResolveDescriptor(suggestion.ActivityDescriptor);
            Distribution previousDistribution = activity.PredictedScore.Distribution;
            double estimatedScore = previousDistribution.Mean - previousDistribution.StdDev;
            if (estimatedScore < 0)
                estimatedScore = 0;
            // make a Skip object holding the needed data
            ActivitySkip skip = new ActivitySkip();
            skip.ApplicableDate = suggestion.StartDate;
            skip.CreationDate = DateTime.Now;
            skip.SuggestionCreationDate = suggestion.GuessCreationDate();
            skip.ActivityDescriptor = suggestion.ActivityDescriptor;

            AbsoluteRating rating = new AbsoluteRating();
            rating.Score = estimatedScore;
            skip.RawRating = rating;
            this.AddSkip(skip);
        }
        public void JustifySuggestion(ActivitySuggestion suggestion)
        {
            IActivitySuggestionJustification justification = this.engine.JustifySuggestion(suggestion);
            String text = justification.Summarize();
            TextblockLayout layout = new TextblockLayout(text);
            this.layoutStack.AddLayout(layout);
        }
        private void MakeRecommendation()
        {
            DateTime now = DateTime.Now;
            this.SuspectLatestActionDate(now);

            
            // If the user requested that the first suggestion be from a certain category, find that category
            Activity requestCategory = null;
            string categoryText = this.suggestionsView.CategoryText;
            if (categoryText != null && categoryText != "")
            {
                ActivityDescriptor categoryDescriptor = new ActivityDescriptor();
                categoryDescriptor.ActivityName = categoryText;
                categoryDescriptor.PreferHigherProbability = true;
                requestCategory = this.engine.ActivityDatabase.ResolveDescriptor(categoryDescriptor);

                if (requestCategory != null)
                {
                    ActivityRequest request = new ActivityRequest(requestCategory.MakeDescriptor(), now);
                    this.AddActivityRequest(request);
                }
            }

            // If the user requested that the suggestion be at least as good as a certain activity, then find that activity
            Activity desiredActivity = null;
            string desiredActivity_text = this.suggestionsView.DesiredActivity_Text;
            if (desiredActivity_text != null && desiredActivity_text != "")
            {
                ActivityDescriptor categoryDescriptor = new ActivityDescriptor();
                categoryDescriptor.ActivityName = desiredActivity_text;
                categoryDescriptor.PreferHigherProbability = true;
                desiredActivity = this.engine.ActivityDatabase.ResolveDescriptor(categoryDescriptor);
            }

            IEnumerable<ActivitySuggestion> existingSuggestions = this.suggestionsView.GetSuggestions();
            List<ActivitySuggestion> suggestions = new List<ActivitySuggestion>();

            DateTime suggestionDate;
            if (existingSuggestions.Count() > 0)
                suggestionDate = existingSuggestions.Last().EndDate.Value;
            else
                suggestionDate = now;

            // have the engine pretend that the user did everything we've suggested
            IEnumerable<Participation> hypotheticalParticipations = this.SupposeHypotheticalSuggestions(existingSuggestions);

            // because the engine takes some time to become fast, we keep track of how many suggestions we've asked for, and we ask for suggestions increasingly more frequently
            this.numCategoriesToConsiderAtOnce++;
            int numCategoriesToConsider = this.numCategoriesToConsiderAtOnce;


            // now determine which category to predict from
            Activity bestActivity = null;
            TimeSpan processingTime = TimeSpan.FromSeconds(2);
            if (requestCategory != null)
            {
                // now we get a recommendation, from among all activities within this category
                bestActivity = this.engine.MakeRecommendation(requestCategory, desiredActivity, suggestionDate, processingTime);
            }
            else
            {
                // now we get a recommendation
                bestActivity = this.engine.MakeRecommendation(suggestionDate, processingTime);
            }
            // if there are no matching activities, then give up
            if (bestActivity == null)
            {
                this.suggestionsView.SetErrorMessage("No activities available! Go create some activities, and return here for suggestions.");
                return;
            }
            // after making a recommendation, get the rest of the details of the suggestion
            // (Note that eventually the suggested duration will be calculated in a more intelligent manner than simply taking the average duration)
            Participation participationSummary = bestActivity.SummarizeParticipationsBetween(new DateTime(), DateTime.Now);
            double typicalNumSeconds = Math.Exp(participationSummary.LogActiveTime.Mean);
            DateTime endDate = suggestionDate.Add(TimeSpan.FromSeconds(typicalNumSeconds));
            ActivitySuggestion suggestion = new ActivitySuggestion(bestActivity.MakeDescriptor());
            suggestion.CreatedDate = now;
            suggestion.StartDate = suggestionDate;
            suggestion.EndDate = endDate;
            suggestion.ParticipationProbability = bestActivity.PredictedParticipationProbability.Distribution.Mean;
            suggestion.PredictedScore = bestActivity.PredictedScore.Distribution;


            // autofill the participationEntryView with a convenient value
            if (existingSuggestions.Count() == 0)
                this.participationEntryView.SetActivityName(bestActivity.Name);

            // add the suggestion to the list (note that this makes the startDate a couple seconds later if it took a couple seconds to compute the suggestion)
            this.suggestionsView.AddSuggestion(suggestion);

            this.WriteSuggestion(suggestion);

            this.UndoHypotheticalSuggestions(hypotheticalParticipations);

            // we have to separately tell the engine about its suggestion because sometimes we don't want to record the suggestion (like when we ask the engine for a suggestion at the beginning to prepare it, for speed)
            this.engine.PutSuggestionInMemory(suggestion);

            // I'm not sure precisely when we want to update the list of current suggestions (which is used for determining whether a participation was prompted by being suggested)
            // Currently (2015-03-09) it's only modified when the user asks for another suggestion, at which point it's updated to match the suggestions that are displayed
            this.CurrentSuggestions = new LinkedList<ActivitySuggestion>(this.suggestionsView.GetSuggestions());

        }

        private IEnumerable<Participation> SupposeHypotheticalSuggestions(IEnumerable<ActivitySuggestion> suggestions)
        {
            LinkedList<Participation> fakeParticipations = new LinkedList<Participation>();
            foreach (ActivitySuggestion suggestion in suggestions)
            {
                // pretend that the user took our suggestion and tell that to the engine
                Participation fakeParticipation = new Participation(suggestion.StartDate, suggestion.EndDate.Value, suggestion.ActivityDescriptor);
                fakeParticipation.Hypothetical = true;
                this.engine.PutParticipationInMemory(fakeParticipation);
                fakeParticipations.AddLast(fakeParticipation);
            }
            return fakeParticipations;
        }
        private void UndoHypotheticalSuggestions(IEnumerable<Participation> participations)
        {
            foreach (Participation participation in participations)
            {
                this.engine.RemoveParticipation(participation);
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

            participation.Suggested = false;
            foreach (ActivitySuggestion suggestion in this.CurrentSuggestions)
            {
                if (participation.ActivityDescriptor.CanMatch(suggestion.ActivityDescriptor))
                    participation.Suggested = true;
            }
            this.AddParticipation(participation);
            // fill in some default data for the ParticipationEntryView
            this.participationEntryView.Clear();

            IEnumerable<ActivitySuggestion> existingSuggestions = this.suggestionsView.GetSuggestions();
            if (existingSuggestions.Count() > 0 && existingSuggestions.First().ActivityDescriptor.CanMatch(participation.ActivityDescriptor))
                this.suggestionsView.RemoveSuggestion(existingSuggestions.First());

            this.UpdateDefaultParticipationData();

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
            this.ActivityDatabase.CreateActivityIfMissing(newParticipation.ActivityDescriptor);
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
            if (this.ratingReplayer != null)
                this.ratingReplayer.AddParticipation(newParticipation);
        }
        public void PutRatingInMemory(Rating newRating)
        {
            this.engine.PutRatingInMemory(newRating);
            if (this.ratingReplayer != null)
                this.ratingReplayer.AddRating(newRating);
        }
        public void PutSkipInMemory(ActivitySkip newSkip)
        {
            // link the skip to its suggestion
            DateTime? suggestionCreationDate = newSkip.SuggestionCreationDate;
            if (suggestionCreationDate != null)
            {
                ActivitySuggestion suggestion = this.suggestionDatabase.GetSuggestion(newSkip.ActivityDescriptor, suggestionCreationDate.Value);
                if (suggestion != null)
                    suggestion.Skip = newSkip;
            }
            // save the skip
            this.engine.PutSkipInMemory(newSkip);
            if (this.ratingReplayer != null)
                this.ratingReplayer.AddSkip(newSkip);
        }
        public void PutActivityRequestInMemory(ActivityRequest newRequest)
        {
            this.engine.PutActivityRequestInMemory(newRequest);
            if (this.ratingReplayer != null)
                this.ratingReplayer.AddRequest(newRequest);
        }
        public void PutActivityDescriptorInMemory(ActivityDescriptor newDescriptor)
        {
            this.engine.PutActivityDescriptorInMemory(newDescriptor);
        }
        public void PutInheritanceInMemory(Inheritance newInheritance)
        {
            this.engine.PutInheritanceInMemory(newInheritance);
            if (this.ratingReplayer != null)
                this.ratingReplayer.AddInheritance(newInheritance);
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
        }
        public void PutSuggestionInMemory(ActivitySuggestion suggestion)
        {
            this.suggestionDatabase.AddSuggestion(suggestion);
            this.engine.PutSuggestionInMemory(suggestion);
            if (this.ratingReplayer != null)
                this.ratingReplayer.AddSuggestion(suggestion);
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
        public Activity CurrentRecommendedActivity
        {
            get
            {
                ActivitySuggestion suggestion = this.CurrentSuggestions.FirstOrDefault(null);
                if (suggestion != null)
                    return this.ActivityDatabase.ResolveDescriptor(suggestion.ActivityDescriptor);
                return null;
            }
        }
        public LinkedList<ActivitySuggestion> CurrentSuggestions
        {
            get
            {
                return this.recentUserData.Suggestions;
            }
            set
            {
                this.recentUserData.Suggestions = value;

                this.WriteRecentUserData();
            }
        }
        public void WriteRecentUserData()
        {
            this.recentUserData.Synchronized = true;
            string text = this.textConverter.ConvertToString(this.recentUserData) + Environment.NewLine;
            StreamWriter writer = this.textConverter.EraseFileAndOpenForWriting(this.tempFileName);
            writer.Write(text);
            writer.Close();
        }


        public void VisualizeData(object sender, EventArgs e)
        {
            this.VisualizeData();
        }
        public void VisualizeData()
        {
            this.engine.EnsureRatingsAreAssociated();
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
                yAxisActivity.ApplyPendingData();
                ActivityVisualizationView visualizationView = new ActivityVisualizationView(xAxisProgression, yAxisActivity, UserPreferences.DefaultPreferences.HalfLife, this.engine.RatingSummarizer);
                //visualizationView.AddExitClickHandler(new RoutedEventHandler(this.ShowMainview));
                this.layoutStack.AddLayout(visualizationView);
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
        DataExportView dataExportView;
        MiniStatisticsMenu statisticsMenu;
        Engine engine;
        //DateTime latestRecommendationDate;
        //Activity currentRecommendedActivity;
        TextConverter textConverter;
        string ratingsFileName;         // the name of the file that stores ratings
        string inheritancesFileName;    // the name of the file that stores inheritances
        string tempFileName;
        //DateTime latestActionDate;
        Participation latestParticipation;
        RatingReplayer ratingReplayer;
        RecentUserData recentUserData;
        // ActivityDatabase primedActivities; // activities that have already been considered and therefore are fast to consider again
        int numCategoriesToConsiderAtOnce = 3;
        LayoutStack layoutStack;
        SuggestionDatabase suggestionDatabase = new SuggestionDatabase();

    }
}