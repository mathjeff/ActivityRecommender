﻿using System;
using System.Collections.Generic;
using System.Linq;
using VisiPlacement;
using ActivityRecommendation.View;
using Xamarin.Forms;

using Plugin.FilePicker.Abstractions;
using System.IO;
using ActivityRecommendation.Effectiveness;
using System.Threading.Tasks;
using System.Reflection;
using StatLists;

// the ActivityRecommender class is the main class that connects the user-interface to the Engine
namespace ActivityRecommendation
{
    public class ActivityRecommender
    {
        public ActivityRecommender(ContentView parentView, string version, ValueProvider<StreamReader> logReader)
        {
            this.parentView = parentView;
            this.version = version;
            this.LogReader = logReader;

            if (System.Diagnostics.Debugger.IsAttached)
                this.setupSynchronously();
            else
                this.setupAsync();
        }

        private void setupAsync()
        {
            this.loadPersona();
            this.setupLoadingScreen();
            Task.Run(() =>
            {
                this.Initialize();
                Device.BeginInvokeOnMainThread(() =>
                {
                    this.attachParentView();
                });
            });
        }
        private void setupSynchronously()
        {
            this.loadPersona();
            this.setupLoadingScreen();
            this.Initialize();
            this.attachParentView();
        }

        private void loadPersona()
        {
            EngineLoader loader = new EngineLoader();
            string personaText = this.internalFileIo.ReadAllText(this.personaFileName);
            loader.ReadText(personaText);
            this.persona = loader.Persona;
        }

        private VisualDefaults VisualDefaults
        {
            get
            {
                string themeName = this.persona.LayoutDefaults_Name;
                List<VisualDefaults> all = this.AllLayoutDefaults;
                foreach (VisualDefaults candidate in all)
                {
                    if (candidate.PersistedName == themeName)
                        return candidate;
                }

                System.Diagnostics.Debug.WriteLine("Theme '" + themeName + "' not found");
                return this.defaultVisualDefaults;
            }
        }

        private List<VisualDefaults> AllLayoutDefaults
        {
            get
            {
                if (this.allLayoutDefaults == null)
                    this.allLayoutDefaults = this.build_AlllayoutDefaults();
                return this.allLayoutDefaults;
            }
        }
        private VisualDefaults defaultVisualDefaults
        {
            get
            {
                return new VisualDefaults_Builder()
                    .DisplayName("Night")
                    .UneditableText_Color(Color.LightGray)
                    .UneditableText_Background(Color.Black)
                    .ApplicationBackground(Color.Black)
                    .Build();
            }

        }
        private List<VisualDefaults> build_AlllayoutDefaults()
        {
            List<VisualDefaults> all = new List<VisualDefaults>();
            all.Add(new VisualDefaults_Builder()
                .DisplayName("Vampire")
                .UneditableText_Color(Color.DarkRed)
                .UneditableText_Background(Color.Black)
                .ApplicationBackground(Color.Gray)
                .ButtonInnerBevelColor(Color.FromRgb(180, 169, 169))
                .ButtonOuterBevelColor(Color.FromRgb(200, 200, 200))
                .FontName("BlackChancery.ttf#BlackChancery")
                .Build());
            all.Add(this.defaultVisualDefaults); // night

            all.Add(new VisualDefaults_Builder()
                .DisplayName("Scifi")
                .UneditableText_Color(Color.FromRgb(13, 125, 148))
                .UneditableText_Background(Color.FromRgb(1, 20, 38))
                .ApplicationBackground(Color.FromRgb(7, 72, 93))
                .ButtonInnerBevelColor(Color.FromRgb(180, 169, 169))
                .ButtonOuterBevelColor(Color.FromRgb(200, 200, 200))
                .FontName("SatellaRegular.ttf#Satella")
                .Build());
            all.Add(new VisualDefaults_Builder()
                .DisplayName("Terminal")
                .UneditableText_Color(Color.Green)
                .UneditableText_Background(Color.Black)
                .ApplicationBackground(Color.Black)
                .FontName("MinimalFont5x7.ttf#MinimalFont5x7")
                .FontSizeMultiplier(1.5)
                .Build());

            all.Add(new VisualDefaults_Builder()
                .DisplayName("Sweet")
                .UneditableText_Color(Color.FromRgb(219, 76, 119))
                .UneditableText_Background(Color.FromRgb(64, 0, 75))
                .ApplicationBackground(Color.FromRgb(64, 0, 75))
                .ButtonInnerBevelColor(Color.FromRgb(32, 96, 32))
                .ButtonOuterBevelColor(Color.FromRgb(64, 192, 64))
                .FontName("PruistineScript.ttf#Pruistine-Script")
                .Build());
            all.Add(new VisualDefaults_Builder()
                .DisplayName("Dreams")
                .UneditableText_Color(Color.FromRgb(158, 158, 176))
                .UneditableText_Background(Color.FromRgb(30, 0, 55))
                .ApplicationBackground(Color.FromRgb(51, 43, 142))
                .ButtonInnerBevelColor(Color.FromRgb(175, 164, 175))
                .ButtonOuterBevelColor(Color.FromRgb(185, 185, 195))
                .FontName("Beyond-Wonderland.ttf#Beyond-Wonderland")
                .Build());

            all.Add(new VisualDefaults_Builder()
                .DisplayName("Cocoa")
                .UneditableText_Color(Color.FromRgb(175, 99, 81))
                .UneditableText_Background(Color.FromRgb(50, 24, 27))
                .ApplicationBackground(Color.FromRgb(112, 61, 54))
                .ButtonInnerBevelColor(Color.FromRgb(30, 15, 15))
                .ButtonOuterBevelColor(Color.FromRgb(200, 190, 190))
                .FontName("TitanOne.ttf#TitanOne")
                .Build());
            all.Add(new VisualDefaults_Builder()
                .DisplayName("Butter")
                .UneditableText_Color(Color.FromRgb(255, 204, 0))
                .UneditableText_Background(Color.FromRgb(32, 0, 37))
                .ApplicationBackground(Color.FromRgb(199, 153, 9))
                .ButtonInnerBevelColor(Color.FromRgb(208, 208, 208))
                .ButtonOuterBevelColor(Color.FromRgb(169, 169, 169))
                .FontName("TitanOne.ttf#TitanOne")
                .Build());

            all.Add(new VisualDefaults_Builder()
                .DisplayName("Story")
                .UneditableText_Color(Color.FromRgb(150, 193, 235))
                .UneditableText_Background(Color.FromRgb(25, 62, 113))
                .ApplicationBackground(Color.FromRgb(87, 127, 174))
                .ButtonInnerBevelColor(Color.FromRgb(180, 169, 169))
                .ButtonOuterBevelColor(Color.FromRgb(210, 210, 210))
                .FontName("Beyond-Wonderland.ttf#Beyond-Wonderland")
                .Build());
            all.Add(new VisualDefaults_Builder()
                .DisplayName("Time")
                .UneditableText_Color(Color.FromRgb(246, 246, 245))
                .UneditableText_Background(Color.FromRgb(113, 75, 55))
                .ApplicationBackground(Color.FromRgb(96, 96, 96))
                .ButtonInnerBevelColor(Color.FromRgb(180, 169, 169))
                .ButtonOuterBevelColor(Color.FromRgb(215, 215, 215))
                .FontName("MinimalFont5x7.ttf#MinimalFont5x7")
                .FontSizeMultiplier(1.5)
                .Build());
                
            all.Add(new VisualDefaults_Builder()
                .DisplayName("Salt Water")
                .UneditableText_Color(Color.FromRgb(225, 225, 225))
                .UneditableText_Background(Color.FromRgb(62, 99, 150))
                .ApplicationBackground(Color.FromRgb(145, 163, 189))
                .FontName("PruistineScript.ttf#Pruistine-Script")
                .Build());
            all.Add(new VisualDefaults_Builder()
                .DisplayName("Taffy")
                .UneditableText_Color(Color.FromRgb(241, 80, 133))
                .UneditableText_Background(Color.FromRgb(29, 114, 171))
                .ApplicationBackground(Color.Gray)
                .ButtonInnerBevelColor(Color.FromRgb(3, 45, 120))
                .ButtonOuterBevelColor(Color.FromRgb(4, 60, 150))
                .FontName("Qdbettercomicsans.ttf#QDBetterComicSans")
                .Build());

            all.Add(new VisualDefaults_Builder()
                .DisplayName("Garden")
                .UneditableText_Color(Color.FromRgb(240, 245, 95))
                .UneditableText_Background(Color.FromRgb(21, 60, 8))
                .ApplicationBackground(Color.Black)
                .ButtonInnerBevelColor(Color.FromRgb(160, 160, 160))
                .ButtonOuterBevelColor(Color.FromRgb(200, 200, 200))
                .FontName("Beyond-Wonderland.ttf#Beyond-Wonderland")
                .Build());
            all.Add(new VisualDefaults_Builder()
                .DisplayName("Salad")
                .UneditableText_Color(Color.FromRgb(135, 40, 30))
                .UneditableText_Background(Color.FromRgb(119, 165, 53))
                .ApplicationBackground(Color.FromRgb(227, 205, 148))
                .ButtonInnerBevelColor(Color.FromRgb(60, 60, 60))
                .ButtonOuterBevelColor(Color.FromRgb(80, 80, 80))
                .FontName("Qdbettercomicsans.ttf#QDBetterComicSans")
                .Build());

            all.Add(new VisualDefaults_Builder()
                .DisplayName("Fire")
                .UneditableText_Color(Color.FromRgb(243, 241, 37))
                .UneditableText_Background(Color.FromRgb(177, 46, 14))
                .ApplicationBackground(Color.FromRgb(209, 143, 25))
                .ButtonInnerBevelColor(Color.FromRgb(100, 100, 100))
                .ButtonOuterBevelColor(Color.FromRgb(130, 130, 130))
                .FontName("BlackChancery.ttf#BlackChancery")
                .Build());
            all.Add(new VisualDefaults_Builder()
                .DisplayName("Truck")
                .UneditableText_Color(Color.FromRgb(96, 96, 96))
                .UneditableText_Background(Color.FromRgb(236, 236, 236))
                .ApplicationBackground(Color.FromRgb(166, 166, 166))
                .ButtonInnerBevelColor(Color.FromRgb(10, 10, 10))
                .ButtonOuterBevelColor(Color.FromRgb(200, 200, 200))
                .FontName("SatellaRegular.ttf#Satella")
                .Build());

            return all;
        }

        private void setupLoadingScreen()
        {
            VisualDefaults defaults = this.VisualDefaults;
            this.parentView.BackgroundColor = defaults.ViewDefaults.ApplicationBackground;
            TextblockLayout layout = new TextblockLayout("I'm loading your data! Sincerely, " + this.persona.Name);
            layout.AlignHorizontally(TextAlignment.Center);
            layout.AlignVertically(TextAlignment.Center);
            ViewManager viewManager = new ViewManager(this.parentView, layout, this.VisualDefaults);
        }

        public void Initialize()
        {
            DateTime start = DateTime.Now;
            this.CheckIfIsNewVersion();

            this.layoutStack = new LayoutStack();
            this.protoActivities_database = new ProtoActivity_Database();

            this.InitializeSettings();

            this.SetupEngine();

            this.textConverter = new TextConverter(null, this.ActivityDatabase);

            this.SetupDrawing();
            DateTime end = DateTime.Now;
            System.Diagnostics.Debug.WriteLine("ActivityRecommender.Initialize completed in " + (end - start));
        }

        private void attachParentView()
        {
            this.viewManager.SetParent(this.parentView);
        }

        public void Reload()
        {
            this.latestParticipation = null;
            this.loadPersona();
            this.Initialize();
            this.attachParentView();
        }

        private void CheckIfIsNewVersion()
        {
            // parse an ApplicationExecution describing the previous version
            ApplicationExecution oldExecution = new ApplicationExecution();
            oldExecution.version = this.internalFileIo.ReadAllText(this.versionFilename);
            string debuggingMarker = "Debug-";
            if (oldExecution.version.StartsWith(debuggingMarker))
            {
                oldExecution.version = oldExecution.version.Substring(debuggingMarker.Length);
                oldExecution.debuggerAttached = true;
            }

            // make an ApplicationExecution for this current run
            ApplicationExecution newExecution = new ApplicationExecution();
            newExecution.version = this.version;
            newExecution.debuggerAttached = System.Diagnostics.Debugger.IsAttached;

            // Checkcheck for things to do when the version changes
            this.OnDeterminedVersion(oldExecution, newExecution);

            // save new info if out-of-date
            if (!oldExecution.version.Equals(newExecution.version) || oldExecution.debuggerAttached != newExecution.debuggerAttached)
            {
                string text = newExecution.version;
                if (newExecution.debuggerAttached)
                    text = debuggingMarker + text;
                this.internalFileIo.EraseFileAndWriteContent(this.versionFilename, text);
            }
        }

        // gets called after determining the version
        private void OnDeterminedVersion(ApplicationExecution oldExecution, ApplicationExecution newExecution)
        {
            // Whenever ActivityRecommender is updated to a version that's unfamiliar to its user, we'd like to make a backup of the user's data, in case the new version does
            // something wrong. This is especially true because the entity that updated ActivityRecommender might not have explicitly asked the user before updating it.

            // So, if either this version or the last version didn't run in the debugger, then we should make a backup of the data.

            // However, if both this version and the last version were running in the debugger, then we already have a recent backup (the one from when the debugger was first enabled)
            // and the developer is probably doing testing that they don't care to back up.
            if (!oldExecution.version.Equals(newExecution.version))
            {
                if (oldExecution.version == "")
                {
                    // This is the first execution, so we give the user a brief description of what ActivityRecommender does
                    this.welcomeMessage = "You should try to improve your life a little bit every day.\n" +
                        "Soon your life will be awesome!\n" +
                        "If this sounds inconvenient, don't worry: I am ActivityRecommender and I make improvement super easy by helping you keep track of and analyze everything.";
                }
                else
                {
                    if (this.persona.Name == "ActivityRecommender")
                        this.welcomeMessage = "Welcome to ActivityRecommender version " + newExecution.version;
                    else
                        this.welcomeMessage += "Hi! I'm now version " + newExecution.version + "! Sincerely, " + this.persona.Name + ".";
                }
                if (!oldExecution.debuggerAttached || !newExecution.debuggerAttached)
                {
                    //string status = this.ExportData();
                }
            }
        }

        private void InitializeSettings()
        {
            this.recentUserData = new RecentUserData();
        }

        private void SetupDrawing()
        {
            this.mainLayout = ContainerLayout.SameSize_Scroller(new ScrollView(), this.layoutStack);
            this.viewManager = new ViewManager(null, this.mainLayout, this.VisualDefaults);

            ActivityImportLayout activityImportLayout = new ActivityImportLayout(this.ActivityDatabase, this.layoutStack);
            ProtoActivities_Layout protoActivitiesLayout = new ProtoActivities_Layout(this.protoActivities_database, this.ActivityDatabase, this.layoutStack);

            this.activitiesMenuLayout = new ActivitiesMenuLayout(
                new BrowseInheritancesView(this.ActivityDatabase, this.protoActivities_database, this.layoutStack),
                activityImportLayout,
                new InheritanceEditingLayout(this.ActivityDatabase, this.layoutStack),
                protoActivitiesLayout,
                (new HelpWindowBuilder()
                    .AddMessage("This screen allows you to browse the types of activity that you have informed ActivityRecommender that you're interested in.")
                    .AddMessage("This screen also allows you to add new types of activities.")
                    .AddMessage("When you ask ActivityRecommender for a recommendation later, it will only suggest activities that you have entered here.")
                    .AddMessage("Additionally, if you plan to ask ActivityRecommender to measure how quickly (your Effectiveness) you complete various Activities, you have to enter a " +
                    "Metric for those activities, so ActivityRecommender can know that it makes sense to measure (for example, it wouldn't make sense to measure how quickly you sleep at " +
                    "once: it wouldn't count as twice effective to do two sleeps of half duration each).")
                    .AddMessage("To undo, remove, or modify an entry, you have to edit the data file directly. Go back to the Export screen and export all of your data as a .txt file. " +
                    "Then make some changes, and go to the Import screen to load your changed file.")
                    .AddLayout(new CreditsButtonBuilder(layoutStack)
                        .AddContribution(ActRecContributor.CORY_JALBERT, new DateTime(2017, 12, 14), "Suggested having pre-chosen activities available for easy import")
                        .AddContribution(ActRecContributor.ANNI_ZHANG, new DateTime(2020, 3, 8), "Pointed out that linebreaks in buttons didn't work correctly on iOS")
                        .Build()
                    )
                    .Build()
                ),
                this.layoutStack,
                this.ActivityDatabase);

            this.participationEntryView = new ParticipationEntryView(this.engine.ActivityDatabase, this.layoutStack);
            this.participationEntryView.Engine = this.engine;
            this.participationEntryView.AddOkClickHandler(new EventHandler(this.SubmitParticipation));
            this.participationEntryView.AddSetenddateHandler(new EventHandler(this.MakeEndNow));
            this.participationEntryView.AddSetstartdateHandler(new EventHandler(this.MakeStartNow));
            this.participationEntryView.LatestParticipation = this.latestParticipation;
            if (this.recentUserData.Suggestions.Count() > 0)
            {
                this.participationEntryView.SetActivityName(this.recentUserData.Suggestions.First().ActivityDescriptor.ActivityName);
            }
            this.participationEntryView.VisitActivitiesScreen += ParticipationEntryView_VisitActivitiesScreen;
            this.UpdateDefaultParticipationData();

            this.suggestionsView = new SuggestionsView(this, this.layoutStack, this.ActivityDatabase, this.engine);
            this.suggestionsView.AddSuggestions(this.recentUserData.Suggestions);
            this.suggestionsView.RequestSuggestion += SuggestionsView_RequestSuggestion;
            this.suggestionsView.ExperimentRequested += SuggestionsView_ExperimentRequested;
            this.suggestionsView.JustifySuggestion += SuggestionsView_JustifySuggestion;
            this.suggestionsView.VisitParticipationScreen += SuggestionsView_VisitParticipationScreen;
            this.suggestionsView.VisitActivitiesScreen += SuggestionsView_VisitActivitiesScreen;
            this.suggestionsView.LatestParticipation = this.latestParticipation;
            this.updateExperimentParticipationDemands();

            StatisticsMenu visualizationMenu = new StatisticsMenu(this.engine, this.layoutStack);
            visualizationMenu.VisitActivitiesScreen += VisualizationMenu_VisitActivitiesScreen;
            visualizationMenu.VisitParticipationsScreen += VisualizationMenu_VisitParticipationsScreen;

            this.dataImportView = new DataImportView(this.layoutStack);
            this.dataImportView.RequestImport += this.ImportData;


            this.dataExportView = new DataExportView(this, this.persona, this.layoutStack);

            LayoutChoice_Set importExportView = new MenuLayoutBuilder(this.layoutStack)
                .AddLayout("Import", this.dataImportView)
                .AddLayout("Export", this.dataExportView)
                .AddLayout("Summarize",
                    new MenuLayoutBuilder(this.layoutStack)
                    .AddLayout("Summarize Preferences", new PreferenceSummaryLayout(engine, layoutStack, publicFileIo))
                    .AddLayout("Summarize Participations", new ParticipationSummarizerLayout(engine, persona, layoutStack))
                    .Build())
                .Build();


            HomeScreen usageMenu = new HomeScreen(
                this.activitiesMenuLayout,
                this.participationEntryView,
                this.suggestionsView,
                visualizationMenu,
                importExportView,
                this.layoutStack);

            LayoutChoice_Set helpMenu = InstructionsLayout.New(this.layoutStack);

            MenuLayoutBuilder debuggingBuilder = new MenuLayoutBuilder(this.layoutStack);
            debuggingBuilder.AddLayout("View Logs", new MenuLayoutBuilder(this.layoutStack).AddLayout("View Logs", new LogViewer(this.LogReader)).Build());
            debuggingBuilder.AddLayout("Enable/Disable Layout Debugging", new EnableDebugging_Layout(this.viewManager));
            debuggingBuilder.AddLayout("Change Screen Size", new Change_ViewSize_Layout(this.viewManager));
            debuggingBuilder.AddLayout("Compute ActivityRecommender's Accuracy (Very Slow)", new EngineTesterView(this, this.layoutStack));
            debuggingBuilder.AddLayout("View Memory Usage", new ViewMemoryUsageLayout());
            ResetVersionNumberLayout resetVersionNumberLayout = new ResetVersionNumberLayout();
            resetVersionNumberLayout.RequestChangeVersion += ResetVersionNumberLayout_RequestChangeVersion;
            debuggingBuilder.AddLayout("Reset Version Number", resetVersionNumberLayout);
            debuggingBuilder.AddLayout("View Demo", new DemoLayout(this.viewManager, this.ActivityDatabase));

            PersonaNameCustomizationView personaCustomizationView = new PersonaNameCustomizationView(this.persona);
            Choose_LayoutDefaults_Layout themeCustomizationView = new Choose_LayoutDefaults_Layout(this.AllLayoutDefaults);

            themeCustomizationView.Chose_VisualDefaults += ThemeCustomizationView_Chose_LayoutDefaults;

            MenuLayoutBuilder introMenu_builder = new MenuLayoutBuilder(this.layoutStack);
            introMenu_builder.AddLayout("Intro/Info", helpMenu);
            CustomizeLayout customizeLayout = new CustomizeLayout(personaCustomizationView, themeCustomizationView, this.persona, this.layoutStack);
            introMenu_builder.AddLayout(
                new AppFeatureCount_ButtonName_Provider("Appearance", customizeLayout.GetFeatures()),
                new StackEntry(customizeLayout, "Appearance", null)
            );
            introMenu_builder.AddLayout(new AppFeatureCount_ButtonName_Provider("Start!", usageMenu.GetFeatures()), new StackEntry(usageMenu, "Home", null));

            introMenu_builder.AddLayout("Debugging", debuggingBuilder.Build());
            introMenu_builder.AddLayout("Credits (your name could be here!)",
                (new HelpWindowBuilder()
                    .AddMessage("If you would like to appear in ActivityRecommender's credits, you can start by suggesting an improvement to ActivityRecommender here!")
                    .AddLayout(OpenIssue_Layout.New())
                    .AddMessage("There are many credits screens throughout ActivityRecommender, each one describing contributions to a specific screen. How many can you find?")
                    .AddLayout(
                        new HelpButtonLayout("See credits",
                           (new CreditsWindowBuilder(layoutStack)
                                .AddContribution(ActRecContributor.JEFFRY_GASTON, new DateTime(2011, 10, 16), "Designed and created ActivityRecommendor") // a misspelling subsequently pointed out by Tony
                                .AddContribution(ActRecContributor.TONY_FISCHETTI, new DateTime(2011, 12, 13), "Mentioned that \"ActivityRecommendor\" was a misspelling and that the project should be \"ActivityRecommender\"")
                                .AddContribution(ActRecContributor.DAD, new DateTime(2015, 5, 22), "Suggested adding a bevel to the buttons")
                                .AddContribution(ActRecContributor.ANNI_ZHANG, new DateTime(2019, 7, 20), "Suggested giving ActivityRecommender a personalizable name")
                                .AddContribution(ActRecContributor.AARON_SMITH, new DateTime(2019, 8, 17), "Pointed out that users might not know where they are and might not think to go back")
                                .AddContribution(ActRecContributor.ANNI_ZHANG, new DateTime(2019, 9, 21), "Tested ActivityRecommender on iOS")
                                .AddContribution(ActRecContributor.DAD, new DateTime(2020, 2, 14), "Offered some ideas about application icons, with examples")
                                .AddContribution(ActRecContributor.ANNI_ZHANG, new DateTime(2020, 2, 16), "Created the application icon that Android and iOS users see now")
                                .Build()
                            ),
                            layoutStack
                        )
                    )
               ).Build()
            );

            LayoutChoice_Set helpOrStart_menu = introMenu_builder.Build();

            List<LayoutChoice_Set> startLayouts = new List<LayoutChoice_Set>(1);
            startLayouts.Add(helpOrStart_menu);

            if (this.error != "")
            {
                TextblockLayout textLayout = new TextblockLayout(this.error, true, true);
                startLayouts.Insert(0, textLayout);
                startLayouts.Add(OpenIssue_Layout.New());
            }

            if (this.welcomeMessage != "")
            {
                List<LayoutChoice_Set> welcomeOptions = new List<LayoutChoice_Set>();
                // It's extra important for the welcome screen to use all of the space
                // Also, there aren't many things on it, so it's not hard to check lots of sizes
                // So, we check lots of possible sizes for the welcome message
                for (int fontSize = 12; fontSize <= 40; fontSize += 2)
                {
                    welcomeOptions.Add(new TextblockLayout(this.welcomeMessage, fontSize));
                }

                startLayouts.Insert(0, new LayoutUnion(welcomeOptions));
                this.welcomeMessage = "";
            }

            helpOrStart_menu = new Vertical_GridLayout_Builder().Uniform().AddLayouts(startLayouts).BuildAnyLayout();

            this.layoutStack.AddLayout(helpOrStart_menu, "Welcome", 0);
        }

        private void ResetVersionNumberLayout_RequestChangeVersion(string version)
        {
            this.internalFileIo.EraseFileAndWriteContent(this.versionFilename, version);
        }

        private void ThemeCustomizationView_Chose_LayoutDefaults(VisualDefaults defaults)
        {
            this.persona.LayoutDefaults_Name = defaults.PersistedName;
            this.parentView.BackgroundColor = defaults.ViewDefaults.ApplicationBackground;
            this.savePersona();
            this.viewManager.VisualDefaults = this.VisualDefaults;
        }

        private void ParticipationEntryView_VisitActivitiesScreen()
        {
            this.layoutStack.GoBack();
            this.layoutStack.AddLayout(this.activitiesMenuLayout, "Activities");
        }

        private void SuggestionsView_VisitActivitiesScreen()
        {
            this.ParticipationEntryView_VisitActivitiesScreen();
        }

        private void SuggestionsView_VisitParticipationScreen()
        {
            this.layoutStack.GoBack();
            this.layoutStack.AddLayout(this.participationEntryView, "Record Participations");
        }

        private void VisualizationMenu_VisitActivitiesScreen()
        {
            this.SuggestionsView_VisitActivitiesScreen();
        }

        private void VisualizationMenu_VisitParticipationsScreen()
        {
            this.SuggestionsView_VisitParticipationScreen();
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
            ExperimentInitializationLayout experimentationLayout = new ExperimentInitializationLayout(this.layoutStack, this, this.ActivityDatabase, this.engine, 3 - this.recentUserData.NumRecent_UserChosen_ExperimentSuggestions);
            experimentationLayout.LatestParticipation = this.LatestParticipation;
            this.layoutStack.AddLayout(experimentationLayout, "Experiment");
            experimentationLayout.RequestedExperiment += ExperimentationInitializationLayout_RequestedExperiment;
        }

        private void ExperimentationInitializationLayout_RequestedExperiment(List<SuggestedMetric> choices)
        {
            this.Request_ExperimentOption_Difficulties(choices);
        }

        public void Request_ExperimentOption_Difficulties(List<SuggestedMetric> choices)
        {
            ExperimentationDifficultySelectionLayout layout = new ExperimentationDifficultySelectionLayout(choices, this.ActivityDatabase);
            layout.Done += this.ExperimentDifficultySelectionLayout_Done;
            this.layoutStack.AddLayout(layout, "Difficulty");
        }

        // We limit the number of experiment suggestions that the user is allowed to request at once
        // (This is because we don't want the user to empty the pool of post-tasks too quickly, lowering the accuracy of our longterm efficiency estimates)
        // This function updates the user's quota of how many experiment suggestions they're allowed to control now
        private void update_numRecent_userChosenExperimentSuggestions(List<SuggestedMetric> choices)
        {
            int numUserChosen_suggestions = 0;
            foreach (SuggestedMetric metric in choices)
            {
                if (metric.ChosenByUser)
                    numUserChosen_suggestions++;
            }
            // The user is only allowed to choose a certain number of suggestions per experiment, on average
            // This is the deviation from that average
            int deviationFromMaxAllowedAverage = numUserChosen_suggestions - 1;
            int newNum = this.recentUserData.NumRecent_UserChosen_ExperimentSuggestions + deviationFromMaxAllowedAverage;
            if (newNum < 0)
                newNum = 0;
            if (newNum > 3)
                newNum = 3;
            this.recentUserData.NumRecent_UserChosen_ExperimentSuggestions = newNum;
            this.writeRecentUserData_if_needed();
        }
        private void ExperimentDifficultySelectionLayout_Done(List<SuggestedMetric> choices)
        {
            DateTime when = DateTime.Now;
            this.SuspectLatestActionDate(when);
            this.update_numRecent_userChosenExperimentSuggestions(choices);
            ExperimentSuggestion experimentSuggestion = this.engine.Experiment(choices, when);
            ActivitySuggestion activitySuggestion = experimentSuggestion.ActivitySuggestion;
            this.recentUserData.DemandedMetricName = experimentSuggestion.MetricName;
            this.AddSuggestion_To_SuggestionsView(activitySuggestion);

            PlannedExperiment experiment = experimentSuggestion.Experiment;

            if (!experiment.InProgress)
            {
                this.engine.PutExperimentInMemory(experiment);
                this.WriteExperiment(experiment);
            }

            this.layoutStack.RemoveLayout();
            this.layoutStack.RemoveLayout();
        }

        public void ImportData(object sender, FileData fileData)
        {
            string content = System.Text.Encoding.UTF8.GetString(fileData.DataArray, 0, fileData.DataArray.Length);
            this.ImportData(fileData.FileName, content);
        }
        public void ImportData(string filename, string content)
        {
            try
            {
                TextConverter importer = new TextConverter(null, new ActivityDatabase(null, null));
                PersistentUserData userData = importer.ParseForImport(content);
                this.internalFileIo.EraseFileAndWriteContent(this.personaFileName, userData.PersonaText);
                this.internalFileIo.EraseFileAndWriteContent(this.inheritancesFileName, userData.InheritancesText);
                this.internalFileIo.EraseFileAndWriteContent(this.ratingsFileName, userData.HistoryText);
                this.internalFileIo.EraseFileAndWriteContent(this.recentUserData_fileName, userData.RecentUserDataText);
                this.internalFileIo.EraseFileAndWriteContent(this.protoActivities_filename, userData.ProtoActivityText);
            }
            catch (Exception e)
            {
                TextblockLayout textLayout = new TextblockLayout("Could not import " + filename + " :\n" + e.ToString(), true, true);
                this.layoutStack.AddLayout(textLayout, "Import Error");
                return;
            }
            this.Reload();
        }

        private PersistentUserData getPersistentUserData()
        {
            PersistentUserData data = new PersistentUserData();
            data.PersonaText = this.internalFileIo.ReadAllText(this.personaFileName);
            data.InheritancesText = this.internalFileIo.ReadAllText(this.inheritancesFileName);
            data.HistoryText = this.internalFileIo.ReadAllText(this.ratingsFileName);
            data.RecentUserDataText = this.internalFileIo.ReadAllText(this.recentUserData_fileName);
            data.ProtoActivityText = this.internalFileIo.ReadAllText(this.protoActivities_filename);
            return data;
        }

        public async Task ExportData()
        {
            string content = this.getPersistentUserData().serialize();

            DateTime now = DateTime.Now;
            string nowText = now.ToString("yyyy-MM-dd-HH-mm-ss");
            string fileName = "ActivityData-" + nowText + ".txt";

            // TODO make it possible for the user to control the file path
            bool successful = await this.publicFileIo.ExportFile(fileName, content);

            if (successful)
            {
                this.layoutStack.AddLayout(new ExportSuccessLayout(fileName, this.publicFileIo), "Success");
            }
            else
            {
                this.layoutStack.AddLayout(new TextblockLayout("Failed to save " + fileName), "Error");
            }
        }

        public bool GoBack()
        {
            if (this.layoutStack != null)
                return this.layoutStack.GoBack();
            return false;
        }


        private void SetupEngine()
        {
            System.Diagnostics.Debug.WriteLine("Starting to read files");

            this.error = "";

            EngineLoader loader = new EngineLoader();
            Engine engine;
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.loadDataFilesInto(loader);
            }
            else
            {
                try
                {
                    this.loadDataFilesInto(loader);
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
            this.persona.NameChanged += Persona_NameChanged;
            this.protoActivities_database = loader.ProtoActivity_Database;
            this.latestParticipation = loader.LatestParticipation;
            this.recentUserData = loader.RecentUserData;
            this.SuspectLatestActionDate(loader.LatestDate);
            if (!this.engine.ActivityDatabase.ContainsCustomActivity())
            {
                // If the user hasn't entered any data yet, then starting ActivityRecommender counts as taking an action
                // We wouldn't want to start the participation entry view date at year 0
                this.LatestActionDate = DateTime.Now;
            }

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

        private void Persona_NameChanged(string newName)
        {
            this.savePersona();
        }

        private void savePersona()
        {
            this.internalFileIo.EraseFileAndWriteContent(this.personaFileName, this.textConverter.ConvertToString(this.persona));
        }

        public EngineTesterResults TestEngine()
        {
            EngineTester engineTester = new EngineTester();
            this.loadDataFilesInto(engineTester);
            engineTester.Finish();
            return engineTester.Results;
        }

        private void loadDataFilesInto(HistoryReplayer historyReplayer)
        {
            PersistentUserData data = this.getPersistentUserData();
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
            ActivitySuggestion_Explanation justification = this.engine.ExplainSuggestion(suggestion);
            this.layoutStack.AddLayout(new ActivitySuggestion_Explanation_Layout(justification), "Why");
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
            ActivityDescriptor demandedActivity = null;
            Metric demandedMetric = null;
            if (this.suggestionsView.GetSuggestions().Count() > 0)
            {
                ActivitySuggestion suggestion = this.suggestionsView.GetSuggestions().First();
                if (!suggestion.Skippable)
                {
                    demandedActivity = suggestion.ActivityDescriptor;
                    Activity activity = this.ActivityDatabase.ResolveDescriptor(demandedActivity);
                    string metricName = this.recentUserData.DemandedMetricName;
                    if (metricName != "")
                        demandedMetric = activity.MetricForName(metricName);
                    else
                        demandedMetric = activity.DefaultMetric;
                }
            }
            this.participationEntryView.DemandNextParticipationBe(demandedActivity, demandedMetric);
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

            // have the engine pretend that the user did everything we've suggested
            IEnumerable<Participation> hypotheticalParticipations = this.SupposeHypotheticalSuggestions(existingSuggestions);

            // now we get a recommendation, from among all activities within this category
            request.RequestedProcessingTime = this.suggestionProcessingDuration;
            ActivitySuggestion suggestion = this.engine.MakeRecommendation(request);

            // if there are no matching activities, then give up
            if (suggestion != null)
            {
                // we have to separately tell the engine about its suggestion because sometimes we don't want to record the suggestion (like when we ask the engine for a suggestion at the beginning to prepare it, for speed)
                this.engine.ApplySuggestion(suggestion);

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
            List<Participation> fakeParticipations = new List<Participation>(suggestions.Count());
            foreach (ActivitySuggestion suggestion in suggestions)
            {
                // pretend that the user took our suggestion and tell that to the engine
                Participation fakeParticipation = new Participation(suggestion.StartDate, suggestion.EndDate.Value, suggestion.ActivityDescriptor);
                fakeParticipation.Hypothetical = true;
                this.engine.PutParticipationInMemory(fakeParticipation);
                fakeParticipations.Add(fakeParticipation);
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
            this.participationEntryView.SetEnddateNow(DateTime.Now);
            this.LatestActionDate = this.participationEntryView.EndDate;
        }
        private void AddParticipation(Participation newParticipation)
        {
            if (this.latestParticipation == null || newParticipation.EndDate.CompareTo(this.latestParticipation.EndDate) > 0)
            {
                this.LatestParticipation = newParticipation;
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
            this.engine.ApplySkip(newSkip);
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
        public IEnumerable<ActivitySuggestion> CurrentSuggestions
        {
            get
            {
                return this.recentUserData.Suggestions;
            }
            set
            {
                this.recentUserData.Suggestions = value;
                this.participationEntryView.CurrentSuggestions = this.recentUserData.Suggestions;

                this.writeRecentUserData_if_needed();
            }
        }

        private Participation LatestParticipation
        {
            get
            {
                return this.latestParticipation;
            }
            set
            {
                this.latestParticipation = value;
                if (this.participationEntryView != null)
                    this.participationEntryView.LatestParticipation = this.latestParticipation;
                if (this.suggestionsView != null)
                    this.suggestionsView.LatestParticipation = this.latestParticipation;
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


        public ActivityDatabase ActivityDatabase
        {
            get
            {
                return this.engine.ActivityDatabase;
            }
        }

        public ValueProvider<StreamReader> LogReader { get; set; }
        
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
                // We don't want the default start and end dates to be on different days; it's more likely that each participation lasted < 1 day
                endDate = startDate;
            }
            if (this.participationEntryView.Is_EndDate_Valid && this.LatestActionDate.Equals(this.participationEntryView.EndDate))
            {
                // If the user hasn't done anything since recording the previous participation,
                // then most likely their next participation hasn't started yet, so we leave the end date invalid (equal to the start date)
                // so the user cannot forget to explicitly set it
                endDate = startDate;
            }
            this.participationEntryView.EndDate = endDate;
            this.participationEntryView.StartDate = startDate;
        }


        string version;
        ContentView parentView;
        ViewManager viewManager;
        LayoutChoice_Set mainLayout;

        ActivitiesMenuLayout activitiesMenuLayout;
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
        string versionFilename = "version.txt";
        string personaFileName = "persona.txt";
        Participation latestParticipation;
        RecentUserData recentUserData;
        LayoutStack layoutStack;
        // how long to spend making a suggestion
        TimeSpan suggestionProcessingDuration = TimeSpan.FromSeconds(2);
        string error = "";
        string welcomeMessage = "";
        ProtoActivity_Database protoActivities_database;
        Persona persona;
        private List<VisualDefaults> allLayoutDefaults;

    }

    class ApplicationExecution
    {
        public string version;
        public bool debuggerAttached;
    }

}
