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
            this.protoActivities_database = new ProtoActivity_Database();

            this.InitializeSettings();

            this.SetupEngine();

            this.textConverter = new TextConverter(null, this.ActivityDatabase);

            //this.ReadTempFile();

            this.SetupDrawing();
        }

        public void Reload()
        {
            this.latestParticipation = null;
            this.Initialize();
        }

        private void InitializeSettings()
        {
            // allocate memory here so we don't have null references when we try to update it in response to the engine making changes
            this.participationEntryView = new ParticipationEntryView(this.layoutStack);
            this.recentUserData = new RecentUserData();
        }

        private void SetupDrawing()
        {

            ActivityCreationLayout activityCreationView = new ActivityCreationLayout(this.ActivityDatabase, this.layoutStack);
            ActivityImportLayout activityImportLayout = new ActivityImportLayout(this.ActivityDatabase, this.layoutStack);
            InheritanceEditingLayout inheritanceCreationView = new InheritanceEditingLayout(this.ActivityDatabase, this.layoutStack);
            ProtoActivities_Layout protoActivitiesLayout = new ProtoActivities_Layout(this.protoActivities_database, this.layoutStack);


            LayoutChoice_Set inheritanceEditingView = new MenuLayoutBuilder(this.layoutStack)
                .AddLayout("Browse Activities", new BrowseInheritancesView(this.ActivityDatabase, this.layoutStack))
                .AddLayout("Import Some Common Activities", activityImportLayout)
                .AddLayout("Brainstorm New Activities", protoActivitiesLayout)
                .AddLayout("Enter New Activity", activityCreationView)
                .AddLayout("New Relationship (Between Existing Activities)", inheritanceCreationView)
                .AddLayout("New Completion Metric", new MetricEditingLayout(this.ActivityDatabase, this.layoutStack))
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

            this.suggestionsView = new SuggestionsView(this, this.layoutStack, this.ActivityDatabase);
            this.suggestionsView.AddSuggestions(this.recentUserData.Suggestions);
            this.suggestionsView.RequestSuggestion += SuggestionsView_RequestSuggestion;
            this.suggestionsView.ExperimentRequested += SuggestionsView_ExperimentRequested;
            this.suggestionsView.JustifySuggestion += SuggestionsView_JustifySuggestion;
            this.updateExperimentParticipationDemands();

            MenuLayoutBuilder visualizationBuilder = new MenuLayoutBuilder(this.layoutStack);
            visualizationBuilder.AddLayout("Search for Cross-Activity Correlations", new ParticipationComparisonMenu(this.layoutStack, this.ActivityDatabase, this.engine));

            this.statisticsMenu = new ActivityVisualizationMenu();
            this.statisticsMenu.ActivityDatabase = this.engine.ActivityDatabase;
            this.statisticsMenu.AddOkClickHandler(new EventHandler(this.VisualizeActivity));

            visualizationBuilder.AddLayout("Visualize one Activity", this.statisticsMenu);
            visualizationBuilder.AddLayout("Browse Favorite Commented Participations", new ParticipationsCommentView(this.ActivityDatabase, this.layoutStack));
            visualizationBuilder.AddLayout("Compute ActivityRecommender's Accuracy (Very Slow)", new EngineTesterView(this, this.layoutStack));

            LayoutChoice_Set visualizationMenu = visualizationBuilder.Build();
            


            this.dataImportView = new DataImportView(this.layoutStack);
            this.dataImportView.RequestImport += this.ImportData;


            this.dataExportView = new DataExportView(this, this.layoutStack);

            LayoutChoice_Set importExportView = new MenuLayoutBuilder(this.layoutStack)
                .AddLayout("Import", this.dataImportView)
                .AddLayout("Export", this.dataExportView)
                .AddLayout("Summarize", new PreferenceSummaryLayout(engine, layoutStack, publicFileIo))
                .Build();


            MenuLayoutBuilder usageMenu_builder = new MenuLayoutBuilder(this.layoutStack);
            usageMenu_builder.AddLayout("Activities", inheritanceEditingView);
            usageMenu_builder.AddLayout("Record Participations", this.participationEntryView);
            usageMenu_builder.AddLayout("Get Suggestions", this.suggestionsView);
            usageMenu_builder.AddLayout("View Statistics", visualizationMenu);
            usageMenu_builder.AddLayout("Import/Export", importExportView);
            LayoutChoice_Set usageMenu = usageMenu_builder.Build();


            MenuLayoutBuilder helpMenu_builder = new MenuLayoutBuilder(this.layoutStack);
            helpMenu_builder.AddLayout("List exciting features", FeatureOverviewLayout.New(this.layoutStack));
            helpMenu_builder.AddLayout("Explain usage", InstructionsLayout.New(this.layoutStack));

            MenuLayoutBuilder introMenu_builder = new MenuLayoutBuilder(this.layoutStack);
            introMenu_builder.AddLayout("Intro", helpMenu_builder.Build());
            introMenu_builder.AddLayout("Start", usageMenu);
            LayoutChoice_Set helpOrStart_menu = introMenu_builder.Build();

            if (this.error != "")
            {
                TextblockLayout textLayout = new TextblockLayout(this.error);
                textLayout.ScoreIfCropped = true;
                
                helpOrStart_menu = new Vertical_GridLayout_Builder().Uniform().AddLayout(textLayout).AddLayout(helpOrStart_menu)
                    .AddLayout(OpenIssue_Layout.New()).Build();
            }


            this.layoutStack.AddLayout(helpOrStart_menu);

            GridLayout gridLayout = GridLayout.New(new BoundProperty_List(2), new BoundProperty_List(1), LayoutScore.Zero);
            gridLayout.PutLayout(new LayoutCache(this.layoutStack), 0, 1);

#if true
            this.mainLayout = gridLayout;
#else
            this.mainLayout = this.layoutStack;
#endif

            this.mainWindow.BackgroundColor = Color.Black;

            this.displayManager = new ViewManager(this.mainWindow, this.mainLayout);
            gridLayout.PutLayout(new LayoutDuration_Layout(this.displayManager), 0, 0);

        }

        private void SuggestionsView_RequestSuggestion(ActivityRequest request)
        {
            IEnumerable<ActivitySuggestion> existingSuggestions = this.suggestionsView.GetSuggestions();
            ActivitySuggestion suggestion = this.MakeRecommendation(request, existingSuggestions);
            if (suggestion == null)
            {
                this.suggestionsView.SetErrorMessage("No activities available! Go create some activities, and return here for suggestions.");
            }
            else
            {
                this.AddSuggestion_To_SuggestionsView(suggestion);
            }
        }

        private void SuggestionsView_JustifySuggestion(ActivitySuggestion suggestion)
        {
            this.JustifySuggestion(suggestion);
        }

        private void SuggestionsView_ExperimentRequested()
        {
            ExperimentInitializationLayout experimentationLayout = new ExperimentInitializationLayout(this.layoutStack, this, this.ActivityDatabase);
            this.layoutStack.AddLayout(experimentationLayout);
            experimentationLayout.RequestedExperiment += ExperimentationInitializationLayout_RequestedExperiment;
        }

        private void ExperimentationInitializationLayout_RequestedExperiment(List<SuggestedMetric> choices)
        {
            this.Request_ExperimentOption_Difficulties(choices);
        }

        public void Request_ExperimentOption_Difficulties(List<SuggestedMetric> choices)
        {
            ExperimentationDifficultySelectionLayout layout = new ExperimentationDifficultySelectionLayout(choices);
            layout.Done += this.ExperimentDifficultySelectionLayout_Done;
            this.layoutStack.AddLayout(layout);
        }

        private void ExperimentDifficultySelectionLayout_Done(List<SuggestedMetric> choices)
        {
            DateTime when = DateTime.Now;
            ExperimentSuggestion experimentSuggestion = this.engine.Experiment(choices, when);
            ActivitySuggestion activitySuggestion = experimentSuggestion.ActivitySuggestion;
            this.AddSuggestion_To_SuggestionsView(activitySuggestion);

            PlannedExperiment experiment = experimentSuggestion.Experiment;

            if (!experiment.InProgress)
            {
                this.engine.PutExperimentInMemory(experiment);
                this.WriteExperiment(experiment);
            }
            this.SuspectLatestActionDate(when);

            this.layoutStack.RemoveLayout();
            this.layoutStack.RemoveLayout();
        }

        public void ImportData(object sender, FileData fileData)
        {
            string content = System.Text.Encoding.UTF8.GetString(fileData.DataArray, 0, fileData.DataArray.Length);
            try
            {
                TextConverter importer = new TextConverter(null, new ActivityDatabase(null, null));
                PersistentUserData userData = importer.ParseForImport(content);
                this.internalFileIo.EraseFileAndWriteContent(this.inheritancesFileName, userData.InheritancesText);
                this.internalFileIo.EraseFileAndWriteContent(this.ratingsFileName, userData.HistoryText);
                this.internalFileIo.EraseFileAndWriteContent(this.recentUserData_fileName, userData.RecentUserDataText);
                this.internalFileIo.EraseFileAndWriteContent(this.protoActivities_filename, userData.ProtoActivityText);
            }
            catch (Exception e)
            {
                TextblockLayout textLayout = new TextblockLayout("Could not import " + fileData.FileName + " :\n" + e.ToString());
                textLayout.ScoreIfCropped = true;
                this.layoutStack.AddLayout(textLayout);
                return;
            }
            this.Reload();
        }

        private PersistentUserData readPersistentUserData()
        {
            PersistentUserData data = new PersistentUserData();
            data.InheritancesText = this.internalFileIo.ReadAllText(this.inheritancesFileName);
            data.HistoryText = this.internalFileIo.ReadAllText(this.ratingsFileName);
            data.RecentUserDataText = this.internalFileIo.ReadAllText(this.recentUserData_fileName);
            data.ProtoActivityText = this.internalFileIo.ReadAllText(this.protoActivities_filename);
            return data;
        }

        public string ExportData()
        {
            string content = this.readPersistentUserData().serialize();
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


        private void SetupEngine()
        {
            System.Diagnostics.Debug.WriteLine("Starting to read files");

            this.error = "";
            EngineLoader loader = new EngineLoader();
            Engine engine;
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.LoadFilesInto(loader);
            }
            else
            {
                try
                {
                    this.LoadFilesInto(loader);
                }
                catch (Exception e)
                {
                    this.error = "Failed to load files: " + e;
                }
            }
#if true
            // If we get here then we just set an Engine up as normal
            // This is the typical case
            engine = loader.Finish();
#else
            // If we get here then we run a special HistoryReplayer before startup
            // This is extra slow and extra confusing so to enable it you have to change the source code
            HistoryReplayer historyReplayer = new RatingRenormalizer(false, true);
            this.LoadFilesInto(historyReplayer);
            engine = historyReplayer.Finish();
#endif
            this.engine = engine;
            this.protoActivities_database = loader.ProtoActivity_Database;
            this.suggestionDatabase = loader.SuggestionDatabase;
            this.latestParticipation = loader.LatestParticipation;
            this.recentUserData = loader.RecentUserData;
            this.SuspectLatestActionDate(loader.LatestDate);

            this.ActivityDatabase.AssignDefaultParent();
            System.Diagnostics.Debug.WriteLine("Done parsing files");

            // listen for subsequently created Activity or Inheritance objects
            engine.ActivityDatabase.ActivityAdded += ActivityDatabase_ActivityAdded;
            engine.ActivityDatabase.InheritanceAdded += ActivityDatabase_InheritanceAdded;
            engine.ActivityDatabase.MetricAdded += ActivityDatabase_MetricAdded;
            // listen for subsequently modified ProtoActivity objects
            this.protoActivities_database.TextChanged += ProtoActivities_database_TextChanged;
            this.protoActivities_database.RatingsChanged += ProtoActivities_database_RatingsChanged;


            engine.FullUpdate();
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
            PersistentUserData data = this.readPersistentUserData();
            historyReplayer.ReadText(data.ProtoActivityText);
            historyReplayer.ReadText(data.InheritancesText);
            historyReplayer.ReadText(data.HistoryText);
            historyReplayer.ReadText(data.RecentUserDataText);
        }

        public ActivitySkip DeclineSuggestion(ActivitySuggestion suggestion)
        {
            // make a Skip object holding the needed data
            DateTime considerationDate = this.LatestActionDate;
            DateTime suggestionCreationDate = suggestion.CreatedDate;
            ActivitySkip skip = new ActivitySkip(suggestion.ActivityDescriptor, suggestionCreationDate, considerationDate, DateTime.Now, suggestion.StartDate);

            this.AddSkip(skip);
            this.PersistSuggestions();

            return skip;
        }
        public void JustifySuggestion(ActivitySuggestion suggestion)
        {
            ActivitySuggestion_Justification justification = this.engine.JustifySuggestion(suggestion);
            this.layoutStack.AddLayout(new ActivitySuggestion_Justification_Layout(justification));
        }

        private void AddSuggestion_To_SuggestionsView(ActivitySuggestion suggestion)
        {
            // add the suggestion to the list (note that this makes the startDate a couple seconds later if it took a couple seconds to compute the suggestion)
            this.suggestionsView.AddSuggestion(suggestion);

            if (this.suggestionsView.GetSuggestions().Count() == 1)
            {
                // autofill the participationEntryView with a convenient value
                this.participationEntryView.SetActivityName(suggestion.ActivityDescriptor.ActivityName);
                this.updateExperimentParticipationDemands();
            }

            this.PersistSuggestions();
        }

        private void updateExperimentParticipationDemands()
        {
            ActivityDescriptor demand = null;
            if (this.suggestionsView.GetSuggestions().Count() > 0)
            {
                ActivitySuggestion suggestion = this.suggestionsView.GetSuggestions().First();
                if (!suggestion.Skippable)
                {
                    demand = suggestion.ActivityDescriptor;
                }
            }
            this.participationEntryView.DemandNextParticipationBe(demand);
        }

        private void PersistSuggestions()
        {
            this.CurrentSuggestions = this.suggestionsView.GetSuggestions();
        }

        // called when making a recommendation, either for the SuggestionsView or the ExperimentationInitializationLayout
        private ActivitySuggestion MakeRecommendation(ActivityRequest request, IEnumerable<ActivitySuggestion> existingSuggestions)
        {
            DateTime now = request.Date;
            this.SuspectLatestActionDate(now);
            
            if (request.FromCategory != null || request.ActivityToBeat != null)
            {
                // record the user's request for a certain activity
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

            // now we get a recommendation, from among all activities within this category
            request.RequestedProcessingTime = this.suggestionProcessingDuration;
            ActivitySuggestion suggestion = this.engine.MakeRecommendation(request);

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

        public SuggestedMetric_Metadata ChooseExperimentOption(ActivityRequest activityRequest, List<SuggestedMetric> existingOptions)
        {
            DateTime now = DateTime.Now;

            SuggestedMetric_Metadata result = this.engine.ChooseExperimentOption(activityRequest, existingOptions, this.suggestionProcessingDuration, now);
            if (result.Error != "")
                return result;
            ActivitySuggestion suggestion = result.ActivitySuggestion;
            this.engine.PutSuggestionInMemory(suggestion); // have to call this.engine.PutSuggestionInMemory so that ActivityRecommender can ask for a suggestion without recording it
            this.WriteSuggestion(suggestion);
            return result;
        }

        public SuggestedMetric_Metadata Test_ChooseExperimentOption()
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
        private void WriteSuggestion(ActivitySuggestion suggestion)
        {
            string text = this.textConverter.ConvertToString(suggestion) + Environment.NewLine;
            this.internalFileIo.AppendText(text, this.ratingsFileName);
        }
        private void WriteExperiment(PlannedExperiment experiment)
        {
            string text = this.textConverter.ConvertToString(experiment) + Environment.NewLine;
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
            this.PersistSuggestions();

            this.UpdateDefaultParticipationData();

            // give the information to the appropriate activities
            this.engine.ApplyParticipationsAndRatings();

            this.updateExperimentParticipationDemands();
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

        private void ActivityDatabase_ActivityAdded(Activity activity)
        {
            this.WriteActivity(activity);
        }
        private void WriteActivity(Activity activity)
        {
            string text = this.textConverter.ConvertToString(activity) + Environment.NewLine;
            this.internalFileIo.AppendText(text, this.inheritancesFileName);
        }

        private void ActivityDatabase_InheritanceAdded(Inheritance inheritance)
        {
            this.WriteInheritance(inheritance);
        }
        private void WriteInheritance(Inheritance newInheritance)
        {
            string text = this.textConverter.ConvertToString(newInheritance) + Environment.NewLine;
            this.internalFileIo.AppendText(text, this.inheritancesFileName);
        }

        private void ActivityDatabase_MetricAdded(Metric metric, Activity activity)
        {
            this.WriteMetric(metric);
        }
        private void WriteMetric(Metric metric)
        {
            string text = this.textConverter.ConvertToString(metric);
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

        private void ProtoActivities_database_RatingsChanged()
        {
            this.write_protoActivities_database();
        }

        private void ProtoActivities_database_TextChanged()
        {
            this.write_protoActivities_database();
        }

        private void write_protoActivities_database()
        {
            string text = this.textConverter.ConvertToString(this.protoActivities_database) + Environment.NewLine;
            this.internalFileIo.EraseFileAndWriteContent(this.protoActivities_filename, text);
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
        public IEnumerable<ActivitySuggestion> CurrentSuggestions
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
                List<ScoreSummarizer> ratingSummarizers = new List<ScoreSummarizer>();
                ActivityVisualizationView visualizationView = new ActivityVisualizationView(xAxisProgression, yAxisActivity, this.engine.RatingSummarizer, this.engine.EfficiencySummarizer, this.layoutStack);
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
        string ratingsFileName = "ActivityRatings.txt";         // the name of the file that stores ratings
        string inheritancesFileName = "ActivityInheritances.txt";    // the name of the file that stores inheritances
        string recentUserData_fileName = "TemporaryData.txt";
        string protoActivities_filename = "ProtoActivities.txt";
        Participation latestParticipation;
        RecentUserData recentUserData;
        LayoutStack layoutStack;
        SuggestionDatabase suggestionDatabase;
        // how long to spend making a suggestion
        TimeSpan suggestionProcessingDuration = TimeSpan.FromSeconds(2);
        string error = "";
        ProtoActivity_Database protoActivities_database;
    }
}