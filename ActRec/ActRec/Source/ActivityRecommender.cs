using System;
using System.Collections.Generic;
using System.Linq;
using VisiPlacement;
using ActivityRecommendation.View;
using Xamarin.Forms;

using Plugin.FilePicker.Abstractions;
using System.IO;
using ActivityRecommendation.Effectiveness;

// the ActivityRecommender class is the main class that connects the user-interface to the Engine
namespace ActivityRecommendation
{
    public class ActivityRecommender
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
        }

        // call this to do cleanup immediately before this object gets destroyed
        public void ShutDown()
        {
            if (!this.recentUserData.Synchronized)
                this.WriteRecentUserData();
            this.latestParticipation = null;
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
            this.recentUserData_fileName = "TemporaryData.txt";
            this.textConverter = new TextConverter(null, null);
            //this.historyReplayer = new EngineTester();
            //this.historyReplayer = new RatingRenormalizer(this.textConverter);
            //this.historyReplayer = new HistoryWriter(this.textConverter);

            // allocate memory here so we don't have null references when we try to update it in response to the engine making changes
            this.participationEntryView = new ParticipationEntryView(this.layoutStack);
            this.recentUserData = new RecentUserData();
        }

        private void SetupDrawing()
        {

            ActivityCreationLayout activityCreationView = new ActivityCreationLayout(this.ActivityDatabase, this.layoutStack);
            InheritanceEditingLayout inheritanceCreationView = new InheritanceEditingLayout(this.ActivityDatabase, this.layoutStack);

            LayoutChoice_Set inheritanceEditingView = new MenuLayoutBuilder(this.layoutStack)
                .AddLayout("Browse Activities", new BrowseInheritancesView(this.ActivityDatabase, this.layoutStack))
                .AddLayout("New Activity", activityCreationView)
                .AddLayout("New Relationship (Between Existing Activities)", inheritanceCreationView)
                .AddLayout("New Completion Metric", (new HelpWindowBuilder()
                    .AddMessage("Sorry, support to assign a completion metric to an Activity isn't done yet. Check back later!")
                    .Build()))
                .AddLayout("Help", (new HelpWindowBuilder()
                    .AddMessage("This page allows you to browse the types of activity that you have informed ActivityRecommender that you're interested in.")
                    .AddMessage("This page also allows you to add new types of activities.")
                    .AddMessage("Any recommendation that ActivityRecommdender makes will be one of these activities.")
                    .AddMessage("Additionally, if you plan to ask ActivityRecommender to measure how quickly (your Effectiveness) you complete various Activities, you have to enter a "+
                    "Metric for those activities, so ActivityRecommender can know that it makes sense to measure (for example, it wouldn't make sense to measure how quickly you sleep at "+
                    "once: it wouldn't count as twice effective to do two sleeps of half duration each).")
                    .AddMessage("To undo, remove, or modify an entry, you have to edit the data file directly. Go back to the Export screen and export all of your data as a .txt file. " +
                    "Then make some changes, and go to the Import screen to load your changed file.")
                    .Build()))
                .Build();

            // this gets taken care of earlier so we don't get a null reference when we try to update it in response to the engine making changes
            this.participationEntryView.Engine = this.engine;
            this.participationEntryView.ActivityDatabase = this.engine.ActivityDatabase;
            this.participationEntryView.AddOkClickHandler(new EventHandler(this.SubmitParticipation));
            this.participationEntryView.AddSetenddateHandler(new EventHandler(this.MakeEndNow));
            this.participationEntryView.AddSetstartdateHandler(new EventHandler(this.MakeStartNow));
            this.participationEntryView.LatestParticipation = this.latestParticipation;
            this.UpdateDefaultParticipationData();

            this.suggestionsView = new SuggestionsView(this, this.layoutStack);
            this.suggestionsView.AddSuggestionClickHandler(new EventHandler(this.SuggestionsView_MakeRecommendation));
            this.suggestionsView.ActivityDatabase = this.engine.ActivityDatabase;
            this.suggestionsView.AddSuggestions(this.recentUserData.Suggestions);
            this.suggestionsView.ExperimentRequested += SuggestionsView_ExperimentRequested;

            MenuLayoutBuilder visualizationBuilder = new MenuLayoutBuilder(this.layoutStack);
            visualizationBuilder.AddLayout("Search for Cross-Activity Correlations", new ParticipationCorrelationMenu(this.layoutStack, this.ActivityDatabase, this.engine));


            this.statisticsMenu = new ActivityVisualizationMenu();
            this.statisticsMenu.ActivityDatabase = this.engine.ActivityDatabase;
            this.statisticsMenu.AddOkClickHandler(new EventHandler(this.VisualizeActivity));

            visualizationBuilder.AddLayout("Visualize one Activity", this.statisticsMenu);

            visualizationBuilder.AddLayout("Compute ActivityRecommender's Accuracy (Very Slow)", new EngineTesterView(this, this.layoutStack));

            LayoutChoice_Set visualizationMenu = visualizationBuilder.Build();
            


            this.dataImportView = new DataImportView(this.layoutStack);
            this.dataImportView.RequestImport += this.ImportData;


            this.dataExportView = new DataExportView(this, this.layoutStack);

            LayoutChoice_Set importExportView = new MenuLayoutBuilder(this.layoutStack).AddLayout("Import", this.dataImportView).AddLayout("Export", this.dataExportView).Build();


            MenuLayoutBuilder usageMenu_builder = new MenuLayoutBuilder(this.layoutStack);
            usageMenu_builder.AddLayout("View/Edit Activities", inheritanceEditingView);
            usageMenu_builder.AddLayout("Record Participations", this.participationEntryView);
            usageMenu_builder.AddLayout("Get Suggestions", this.suggestionsView);
            usageMenu_builder.AddLayout("View Statistics", visualizationMenu);
            usageMenu_builder.AddLayout("Import/Export Data", importExportView);
            LayoutChoice_Set usageMenu = usageMenu_builder.Build();


            MenuLayoutBuilder helpMenu_builder = new MenuLayoutBuilder(this.layoutStack);
            helpMenu_builder.AddLayout("List exciting features", FeatureOverviewLayout.New(this.layoutStack));
            helpMenu_builder.AddLayout("Explain usage", InstructionsLayout.New(this.layoutStack));

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

        private void SuggestionsView_ExperimentRequested()
        {
            ExperimentInitializationLayout experimentationLayout = new ExperimentInitializationLayout(this.layoutStack, this);
            this.layoutStack.AddLayout(experimentationLayout);
            experimentationLayout.RequestedExperiment += ExperimentationLayout_RequestedExperiment;

        }

        private void ExperimentationLayout_RequestedExperiment(List<ExperimentSuggestion> choices)
        {
            PlannedExperiment experiment = this.engine.Experiment(choices);
            ActivitySuggestion suggestion = experiment.NextIncompleteSuggestion;
            // TODO: disallow ever dismissing this suggestion other than by working on it.
            // Should the UI convert a dismissal into a recording of an unsuccessful participation? That would be surprising, so probably not
            this.AddSuggestion_To_SuggestionsView(suggestion);
            this.layoutStack.RemoveLayout();
        }

        public void ImportData(object sender, FileData fileData)
        {
            string content = System.Text.Encoding.UTF8.GetString(fileData.DataArray, 0, fileData.DataArray.Length);
            try
            {
                this.textConverter.Import(content, this.inheritancesFileName, this.ratingsFileName, this.recentUserData_fileName);
            }
            catch (InvalidDataException e)
            {
                this.layoutStack.AddLayout(new TextblockLayout("Could not import " + fileData.FileName + " :\n" + e.ToString()));
                return;
            }
            this.Reset();
        }

        public string ExportData()
        {
            string content = "";
            content += this.internalFileIo.ReadAllText(this.recentUserData_fileName);
            content += this.internalFileIo.ReadAllText(this.inheritancesFileName);
            content += this.internalFileIo.ReadAllText(this.ratingsFileName);
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
            bool success = this.publicFileIo.ExportFile(fileName, content);

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
            this.engine.CreateNewActivities();
            // listen for subsequently created Activity or Inheritance objects
            this.engine.ActivityDatabase.ActivityAdded += ActivityDatabase_ActivityAdded;
            this.engine.ActivityDatabase.InheritanceAdded += ActivityDatabase_InheritanceAdded;

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
            EngineLoader loader = new EngineLoader();
            this.LoadFilesInto(loader);
            this.engine = loader.Finish();
            this.suggestionDatabase = loader.SuggestionDatabase;
            this.latestParticipation = loader.LatestParticipation;
            this.recentUserData = loader.RecentUserData;
            this.SuspectLatestActionDate(loader.LatestDate);

            this.ActivityDatabase.AssignDefaultParent();
            System.Diagnostics.Debug.WriteLine("Done parsing files");
        }

        public EngineTesterResults TestEngine()
        {
            EngineTester engineTester = new EngineTester();
            this.LoadFilesInto(engineTester);
            engineTester.Finish();
            return engineTester.Results;
        }

        private void LoadFilesInto(HistoryReplayer historyReplayer)
        {
            historyReplayer.LoadFile(this.inheritancesFileName);
            historyReplayer.LoadFile(this.ratingsFileName);
            historyReplayer.LoadFile(this.recentUserData_fileName);
        }

        public void DeclineSuggestion(ActivitySuggestion suggestion)
        {
            // Calculate the score to generate for this Activity as a result of that statement
            Activity activity = this.ActivityDatabase.ResolveDescriptor(suggestion.ActivityDescriptor);
            Distribution previousDistribution = activity.PredictedScore.Distribution;
            // make a Skip object holding the needed data
            DateTime considerationDate = this.LatestActionDate;
            DateTime suggestionCreationDate = suggestion.GuessCreationDate();
            ActivitySkip skip = new ActivitySkip(suggestion.ActivityDescriptor, suggestionCreationDate, considerationDate, DateTime.Now, suggestion.StartDate);

            this.AddSkip(skip);
        }
        /*public void JustifySuggestion(ActivitySuggestion suggestion)
        {
            IActivitySuggestionJustification justification = this.engine.JustifySuggestion(suggestion);
            String text = justification.Summarize();
            TextblockLayout layout = new TextblockLayout(text);
            this.layoutStack.AddLayout(layout);
        }*/

        // called when the SuggestionsView wants to make a recommendation
        private void SuggestionsView_MakeRecommendation()
        {
            DateTime now = DateTime.Now;
            Activity requestCategory = this.suggestionsView.Category;
            Activity activityToBeat = this.suggestionsView.DesiredActivity;
            IEnumerable<ActivitySuggestion> existingSuggestions = this.suggestionsView.GetSuggestions();
            ActivitySuggestion suggestion = this.MakeRecommendation(now, requestCategory, activityToBeat, existingSuggestions);
            if (suggestion == null)
            {
                this.suggestionsView.SetErrorMessage("No activities available! Go create some activities, and return here for suggestions.");
            }
            else
            {
                this.AddSuggestion_To_SuggestionsView(suggestion);
            }

        }

        private void AddSuggestion_To_SuggestionsView(ActivitySuggestion suggestion)
        {
            // add the suggestion to the list (note that this makes the startDate a couple seconds later if it took a couple seconds to compute the suggestion)
            this.suggestionsView.AddSuggestion(suggestion);

            // autofill the participationEntryView with a convenient value
            this.participationEntryView.SetActivityName(suggestion.ActivityDescriptor.ActivityName);

            // I'm not sure precisely when we want to update the list of current suggestions (which is used for determining whether a participation was prompted by being suggested)
            // Currently (2015-03-09) it's only modified when the user asks for another suggestion, at which point it's updated to match the suggestions that are displayed
            this.CurrentSuggestions = new LinkedList<ActivitySuggestion>(this.suggestionsView.GetSuggestions());
        }

        // called when making a recommendation, either for the SuggestionsView or the ExperimentationInitializationLayout
        private ActivitySuggestion MakeRecommendation(DateTime now, Activity requestCategory, Activity activityToBeat, IEnumerable<ActivitySuggestion> existingSuggestions)
        {
            this.SuspectLatestActionDate(now);
            
            if (requestCategory != null)
            {
                // record the user's request for a certain activity
                ActivityRequest request = new ActivityRequest(requestCategory.MakeDescriptor(), now);
                this.AddActivityRequest(request);
            }

            List<ActivitySuggestion> suggestions = new List<ActivitySuggestion>();

            DateTime suggestionDate;
            if (existingSuggestions.Count() > 0)
                suggestionDate = existingSuggestions.Last().EndDate.Value;
            else
                suggestionDate = now;

            // have the engine pretend that the user did everything we've suggested
            IEnumerable<Participation> hypotheticalParticipations = this.SupposeHypotheticalSuggestions(existingSuggestions);

            // now determine which category to predict from
            TimeSpan processingTime = this.suggestionProcessingDuration;
            
            // now we get a recommendation, from among all activities within this category
            ActivitySuggestion suggestion = this.engine.MakeRecommendation(requestCategory, activityToBeat, suggestionDate, processingTime);

            // if there are no matching activities, then give up
            if (suggestion != null)
            {
                // we have to separately tell the engine about its suggestion because sometimes we don't want to record the suggestion (like when we ask the engine for a suggestion at the beginning to prepare it, for speed)
                this.engine.PutSuggestionInMemory(suggestion);

                this.WriteSuggestion(suggestion);
            }

            this.UndoHypotheticalSuggestions(hypotheticalParticipations);
            return suggestion;
        }

        public ExperimentSuggestionOrError ChooseExperimentOption(List<ExperimentSuggestion> existingOptions)
        {
            DateTime now = DateTime.Now;
            ExperimentSuggestionOrError result = this.engine.ChooseExperimentOption(existingOptions, this.suggestionProcessingDuration, now);
            if (result.ExperimentSuggestion == null)
                return result;
            ActivitySuggestion suggestion = result.ExperimentSuggestion.ActivitySuggestion;
            this.engine.PutSuggestionInMemory(suggestion); // have to call this.engine.PutSuggestionInMemory so that ActivityRecommender can ask for a suggestion without recording it
            this.WriteSuggestion(suggestion);
            return result;
        }

        public string Test_ChooseExperimentOption()
        {
            return this.engine.Test_ChooseExperimentOption();
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
        private void SuggestionsView_MakeRecommendation(object sender, EventArgs e)
        {
            this.SuggestionsView_MakeRecommendation();
        }
        private void WriteSuggestion(ActivitySuggestion suggestion)
        {
            string text = this.textConverter.ConvertToString(suggestion) + Environment.NewLine;
            this.internalFileIo.AppendText(text, this.ratingsFileName);
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
        }
        private void AddParticipation(Participation newParticipation)
        {
            if (this.latestParticipation == null || newParticipation.EndDate.CompareTo(this.latestParticipation.EndDate) > 0)
            {
                this.latestParticipation = newParticipation;
                if (this.participationEntryView != null)
                    this.participationEntryView.LatestParticipation = this.latestParticipation;
            }
            this.engine.PutParticipationInMemory(newParticipation);

            this.SuspectLatestActionDate(newParticipation.EndDate);

            this.WriteParticipation(newParticipation);
        }
        private void WriteParticipation(Participation newParticipation)
        {
            string text = this.textConverter.ConvertToString(newParticipation) + Environment.NewLine;
            this.internalFileIo.AppendText(text, this.ratingsFileName);
        }
        // declares that the user didn't want to do something that was suggested
        private void AddSkip(ActivitySkip newSkip)
        {
            this.SuspectLatestActionDate(newSkip.CreationDate);
            this.engine.PutSkipInMemory(newSkip);
            this.WriteSkip(newSkip);
        }
        // writes this Skip to a data file
        private void WriteSkip(ActivitySkip newSkip)
        {
            string text = this.textConverter.ConvertToString(newSkip) + Environment.NewLine;
            this.internalFileIo.AppendText(text, this.ratingsFileName);
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
            this.internalFileIo.AppendText(text, this.ratingsFileName);
        }
        /*public void SubmitInheritance(object sender, Inheritance inheritance)
        {
            inheritance.DiscoveryDate = DateTime.Now;
            this.AddInheritance(inheritance);
        }
        private void AddInheritance(Inheritance newInheritance)
        {
            this.engine.ApplyInheritance(newInheritance);
            this.WriteInheritance(newInheritance);
        }
        */

        private void ActivityDatabase_ActivityAdded(object sender, Activity activity)
        {
            this.WriteActivity(activity);
        }
        private void WriteActivity(Activity activity)
        {
            string text = this.textConverter.ConvertToString(activity) + Environment.NewLine;
            this.internalFileIo.AppendText(text, this.inheritancesFileName);
        }

        private void ActivityDatabase_InheritanceAdded(object sender, Inheritance inheritance)
        {
            this.WriteInheritance(inheritance);
        }
        private void WriteInheritance(Inheritance newInheritance)
        {
            string text = this.textConverter.ConvertToString(newInheritance) + Environment.NewLine;
            this.internalFileIo.AppendText(text, this.inheritancesFileName);
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
            this.internalFileIo.AppendText(text, this.ratingsFileName);
        }

        // writes to a text file saying that the user was is this program now. It gets deleted soon

        // updates the ParticipationEntryView so that the start date is DateTime.Now
        public void MakeStartNow(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            this.SuspectLatestActionDate(now);
        }

        #region Functions to be called by the TextConverter
        // updates the ParticipationEntryView so that the start date is 'when'
        public void SuspectLatestActionDate(DateTime when)
        {
            if (when.CompareTo(DateTime.Now) <= 0 && when.CompareTo(this.LatestActionDate) > 0)
            {
                this.LatestActionDate = when;
                //this.WriteInteractionDate(when);
                this.UpdateDefaultParticipationData(when);
            }
        }
        #endregion

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

                this.writeRecentUserData_if_needed();
            }
        }
        
        private void writeRecentUserData_if_needed()
        {
            if (!this.recentUserData.Synchronized)
                this.WriteRecentUserData();
        }
        private void WriteRecentUserData()
        {
            this.recentUserData.Synchronized = true;
            string text = this.textConverter.ConvertToString(this.recentUserData) + Environment.NewLine;
            this.internalFileIo.EraseFileAndWriteContent(this.recentUserData_fileName, text);
        }


        public void VisualizeActivity(object sender, EventArgs e)
        {
            this.VisualizeActivity();
        }
        public void VisualizeActivity()
        {
            this.engine.EnsureRatingsAreAssociated();

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
                this.layoutStack.AddLayout(visualizationView);
            }
        }

        public void ShowMainview(object sender, EventArgs e)
        {
            this.ShowMainview();
        }
        public void ShowMainview()
        {
            this.displayManager.SetLayout(this.mainLayout);
        }

        public ActivityDatabase ActivityDatabase
        {
            get
            {
                return this.engine.ActivityDatabase;
            }
        }
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
            // updating the StartDate while a suggestion is onscreen takes a while (because of updating the feedback block text), so in those cases, skip it
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
        InternalFileIo internalFileIo = new InternalFileIo();
        PublicFileIo publicFileIo = new PublicFileIo();
        string ratingsFileName;         // the name of the file that stores ratings
        string inheritancesFileName;    // the name of the file that stores inheritances
        string recentUserData_fileName;
        Participation latestParticipation;
        RecentUserData recentUserData;
        LayoutStack layoutStack;
        SuggestionDatabase suggestionDatabase;
        // how long to spend making a suggestion
        TimeSpan suggestionProcessingDuration = TimeSpan.FromSeconds(2);

    }
}