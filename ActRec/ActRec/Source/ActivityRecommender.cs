using PCLStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using VisiPlacement;
using System.Threading.Tasks;
using ActivityRecommendation.View;
using Xamarin.Forms;

using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;

// the ActivityRecommender class is the main class that connects the user-interface to the Engine
namespace ActivityRecommendation
{
    class ActivityRecommender
    {
        public ActivityRecommender(ContentView newMainWindow)
        {
            this.mainWindow = newMainWindow;

            this.Initialize();
        }

        public void Initialize()
        {
            this.layoutStack = new LayoutStack();
            this.suggestionDatabase = new SuggestionDatabase();

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

        public void Reset()
        {
            this.ShutDown();
            this.Initialize();
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
            this.numCategoriesToConsiderAtOnce = 3;

            // allocate memory here so we don't have null references when we try to update it in response to the engine making changes
            this.participationEntryView = new ParticipationEntryView(this.layoutStack);
            this.recentUserData = new RecentUserData();
        }

        private void SetupDrawing()
        {

            InheritanceEditingView activityCreationView = new InheritanceEditingView(this.ActivityDatabase, this.layoutStack, true);
            activityCreationView.Submit += this.SubmitInheritance;
            InheritanceEditingView inheritanceCreationView = new InheritanceEditingView(this.ActivityDatabase, this.layoutStack, false);
            inheritanceCreationView.Submit += this.SubmitInheritance;

            LayoutChoice_Set inheritanceEditingView = new MenuLayoutBuilder(this.layoutStack)
                .AddLayout("Add New Activity", activityCreationView)
                .AddLayout("Assign Existing Activity as Child of A Parent Activity", inheritanceCreationView)
                .AddLayout("Undo/Change", (new HelpWindowBuilder()
                    .AddMessage("To undo, remove, or modify an entry, you have to edit the data file directly. Go back to the Export screen and export all of your data as a .txt file. " +
                    "Then make some changes, and go to the Import screen to load your changed file.")
                    .Build()))
                .Build();

            BoundProperty_List rowHeights = new BoundProperty_List(2);
            rowHeights.BindIndices(0, 1);
            rowHeights.SetPropertyScale(0, 2);
            rowHeights.SetPropertyScale(1, 3);

            GridLayout inheritanceInfoView = GridLayout.New(rowHeights, new BoundProperty_List(1), LayoutScore.Zero);
            inheritanceInfoView.AddLayout(new BrowseInheritancesView(this.ActivityDatabase, this.layoutStack));
            inheritanceInfoView.AddLayout(inheritanceEditingView);
               

            // this gets taken care of earlier so we don't get a null reference when we try to update it in response to the engine making changes
            this.participationEntryView.Engine = this.engine;
            this.participationEntryView.ActivityDatabase = this.engine.ActivityDatabase;
            this.participationEntryView.AddOkClickHandler(new EventHandler(this.SubmitParticipation));
            this.participationEntryView.AddSetenddateHandler(new EventHandler(this.MakeEndNow));
            this.participationEntryView.AddSetstartdateHandler(new EventHandler(this.MakeStartNow));
            this.participationEntryView.LatestParticipation = this.latestParticipation;
            this.UpdateDefaultParticipationData();

            this.suggestionsView = new SuggestionsView(this, this.layoutStack);
            this.suggestionsView.AddSuggestionClickHandler(new EventHandler(this.MakeRecommendation));
            this.suggestionsView.ActivityDatabase = this.engine.ActivityDatabase;

            MenuLayoutBuilder visualizationBuilder = new MenuLayoutBuilder(this.layoutStack);
            visualizationBuilder.AddLayout("Search for Cross-Activity Correlations", new ParticipationCorrelationMenu(this.layoutStack, this.ActivityDatabase, this.engine));

            
            this.statisticsMenu = new ActivityVisualizationMenu();
            this.statisticsMenu.ActivityDatabase = this.engine.ActivityDatabase;
            this.statisticsMenu.AddOkClickHandler(new EventHandler(this.VisualizeActivity));

            visualizationBuilder.AddLayout("Visualize one Activity", this.statisticsMenu);

            LayoutChoice_Set visualizationMenu = visualizationBuilder.Build();


            this.dataImportView = new DataImportView(this.layoutStack);
            this.dataImportView.RequestImport += this.ImportData;
            

            this.dataExportView = new DataExportView(this, this.layoutStack);

            LayoutChoice_Set importExportView = new MenuLayoutBuilder(this.layoutStack).AddLayout("Import", this.dataImportView).AddLayout("Export", this.dataExportView).Build();


            MenuLayoutBuilder usageMenu_builder = new MenuLayoutBuilder(this.layoutStack);
            usageMenu_builder.AddLayout("View/Edit Activities", inheritanceInfoView);
            usageMenu_builder.AddLayout("Record Participations", this.participationEntryView);
            usageMenu_builder.AddLayout("Get Suggestions", this.suggestionsView);
            usageMenu_builder.AddLayout("View Statistics", visualizationMenu);
            usageMenu_builder.AddLayout("Import/Export Data", importExportView);
            LayoutChoice_Set usageMenu = usageMenu_builder.Build();


            MenuLayoutBuilder helpMenu_builder = new MenuLayoutBuilder(this.layoutStack);
            helpMenu_builder.AddLayout("List the exciting features", FeatureOverviewLayout.New(this.layoutStack));
            helpMenu_builder.AddLayout("Explain the usage", InstructionsLayout.New(this.layoutStack));

            MenuLayoutBuilder introMenu_builder = new MenuLayoutBuilder(this.layoutStack);
            introMenu_builder.AddLayout("Intro", helpMenu_builder.Build());
            introMenu_builder.AddLayout("Start", usageMenu);
            LayoutChoice_Set helpOrStart_menu = introMenu_builder.Build();


            this.layoutStack.AddLayout(helpOrStart_menu);


            this.mainLayout = new LayoutCache(this.layoutStack);

            this.mainWindow.BackgroundColor = Color.Black;

            this.displayManager = new ViewManager(this.mainWindow, this.mainLayout);
            //this.displayManager = new ViewManager(this.mainWindow, TextDiagnosticLayout.New());            

        }

        public void ImportData(object sender, FileData fileData)
        {
            string content = System.Text.Encoding.UTF8.GetString(fileData.DataArray, 0, fileData.DataArray.Length);
            this.textConverter.Import(content, this.inheritancesFileName, this.ratingsFileName);
            this.Reset();
        }

        public string ExportData()
        {
            string content = "";
            content += this.textConverter.ReadAllText(this.inheritancesFileName);
            content += this.textConverter.ReadAllText(this.ratingsFileName);
            int maxNumLines = this.dataExportView.Get_NumLines();
            if (maxNumLines > 0)
            {
                int startIndex = content.Length - 1;
                for (int i = 0; i < maxNumLines; i++)
                {
                    startIndex = content.LastIndexOf('\n', startIndex - 1);
                    if (startIndex < 0)
                    {
                        startIndex = 0;
                        break;
                    }
                }
                content = content.Substring(startIndex);
            }

            DateTime now = DateTime.Now;
            string nowText = now.ToString("yyyy-MM-dd-HH-mm-ss");
            string fileName = "ActivityData-" + nowText + ".txt";

            // TODO make it possible for the user to control the file path
            bool success = this.textConverter.ExportFile(fileName, content);

            if (success)
                return "Saved " + fileName;
            else
                return "Failed to save " + fileName;
        }

        public bool GoBack()
        {
            return this.layoutStack.GoBack();
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
            this.ActivityDatabase.AssignDefaultParent();
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
        /*public void JustifySuggestion(ActivitySuggestion suggestion)
        {
            IActivitySuggestionJustification justification = this.engine.JustifySuggestion(suggestion);
            String text = justification.Summarize();
            TextblockLayout layout = new TextblockLayout(text);
            this.layoutStack.AddLayout(layout);
        }*/
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
            ParticipationsSummary participationSummary = bestActivity.SummarizeParticipationsBetween(new DateTime(), DateTime.Now);
            double typicalNumSeconds = Math.Exp(participationSummary.LogActiveTime.Mean);
            DateTime endDate = suggestionDate.Add(TimeSpan.FromSeconds(typicalNumSeconds));
            ActivitySuggestion suggestion = new ActivitySuggestion(bestActivity.MakeDescriptor());
            suggestion.CreatedDate = now;
            suggestion.StartDate = suggestionDate;
            suggestion.EndDate = endDate;
            suggestion.ParticipationProbability = bestActivity.PredictedParticipationProbability.Distribution.Mean;

            double average = this.ActivityDatabase.RootActivity.Ratings.Mean;
            if (average == 0)
                average = 1;
            suggestion.PredictedScoreDividedByAverage = bestActivity.PredictedScore.Distribution.Mean / average;


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
            this.textConverter.AppendText(text, this.ratingsFileName);
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
            this.textConverter.AppendText(text, this.ratingsFileName);
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
            this.textConverter.AppendText(text, this.ratingsFileName);
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
            this.textConverter.AppendText(text, this.ratingsFileName);
        }
        public void SubmitInheritance(object sender, Inheritance inheritance)
        {
            inheritance.DiscoveryDate = DateTime.Now;
            this.AddInheritance(inheritance);
        }
        private void AddInheritance(Inheritance newInheritance)
        {
            this.engine.ApplyInheritance(newInheritance);
            this.WriteInheritance(newInheritance);
        }
        public void WriteInheritance(Inheritance newInheritance)
        {
            string text = this.textConverter.ConvertToString(newInheritance) + Environment.NewLine;
            this.textConverter.AppendText(text, this.inheritancesFileName);
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
            this.textConverter.AppendText(text, this.ratingsFileName);
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
            this.textConverter.EraseFileAndWriteContent(this.tempFileName, text);
        }


        public void VisualizeActivity(object sender, EventArgs e)
        {
            this.VisualizeActivity();
        }
        public void VisualizeActivity()
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
                ActivityVisualizationView visualizationView = new ActivityVisualizationView(xAxisProgression, yAxisActivity, UserPreferences.DefaultPreferences.HalfLife, this.engine.RatingSummarizer, this.layoutStack);
                //visualizationView.AddExitClickHandler(new EventHandler(this.ShowMainview));
                this.layoutStack.AddLayout(visualizationView);
            }
        }

        /*void mainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                this.ShowMainview();
            }
        }*/

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
            DateTime startDate = this.LatestActionDate;
            DateTime endDate = when;
            if (startDate.Day != endDate.Day)
            {
                // it's more helpful for the default end-date to be on the same day
                endDate = new DateTime(startDate.Year, startDate.Month, startDate.Day, startDate.Hour, startDate.Minute, startDate.Second);
            }
            this.participationEntryView.EndDate = endDate;
            this.participationEntryView.SetStartDate(startDate);
        }


        ContentView mainWindow;
        ViewManager displayManager;
        LayoutChoice_Set mainLayout;

        ParticipationEntryView participationEntryView;
        SuggestionsView suggestionsView;
        DataExportView dataExportView;
        DataImportView dataImportView;
        ActivityVisualizationMenu statisticsMenu;
        Engine engine;
        TextConverter textConverter;
        string ratingsFileName;         // the name of the file that stores ratings
        string inheritancesFileName;    // the name of the file that stores inheritances
        string tempFileName;
        Participation latestParticipation;
        RatingReplayer ratingReplayer;
        RecentUserData recentUserData;
        int numCategoriesToConsiderAtOnce;
        LayoutStack layoutStack;
        SuggestionDatabase suggestionDatabase;

    }
}