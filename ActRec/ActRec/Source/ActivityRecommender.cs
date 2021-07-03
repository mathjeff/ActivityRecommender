using System;
using System.Collections.Generic;
using System.Linq;
using VisiPlacement;
using ActivityRecommendation.View;
using Xamarin.Forms;

using System.IO;
using ActivityRecommendation.Effectiveness;
using System.Threading.Tasks;
using System.Reflection;
using StatLists;
using Xamarin.Essentials;

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
            loader.ReadText(this.internalFileIo.OpenFileForReading(this.personaFileName));
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
                    .DisplayName("Castle")
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
                .DisplayName("Dracula")
                .UneditableText_Color(Color.DarkRed)
                .UneditableText_Background(Color.Black)
                .ApplicationBackground(Color.Gray)
                .ButtonInnerBevelColor(Color.FromRgb(180, 169, 169))
                .ButtonOuterBevelColor(Color.FromRgb(200, 200, 200))
                .FontName("BlackChancery.ttf#BlackChancery")
                .Build());
            all.Add(this.defaultVisualDefaults);

            all.Add(new VisualDefaults_Builder()
                .DisplayName("Lightning")
                .UneditableText_Color(Color.FromRgb(252, 239, 37))
                .UneditableText_Background(Color.FromRgb(0, 0, 0))
                .ApplicationBackground(Color.FromRgb(166, 166, 166))
                .ButtonInnerBevelColor(Color.FromRgb(190, 234, 235))
                .ButtonOuterBevelColor(Color.FromRgb(190, 234, 235))
                .FontName("SatellaRegular.ttf#Satella")
                .Build());
            all.Add(new VisualDefaults_Builder()
                .DisplayName("Bug")
                .UneditableText_Color(Color.FromRgb(134, 224, 215))
                .UneditableText_Background(Color.FromRgb(26, 25, 18))
                .ApplicationBackground(Color.FromRgb(165, 69, 73))
                .FontName("Qdbettercomicsans.ttf#QDBetterComicSans")
                .Build());

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
                .DisplayName("Programming")
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
                .DisplayName("Brownie")
                .UneditableText_Color(Color.FromRgb(175, 99, 81))
                .UneditableText_Background(Color.FromRgb(50, 24, 27))
                .ApplicationBackground(Color.FromRgb(112, 61, 54))
                .ButtonInnerBevelColor(Color.FromRgb(30, 15, 15))
                .ButtonOuterBevelColor(Color.FromRgb(200, 190, 190))
                .FontName("TitanOne.ttf#TitanOne")
                .Build());
            all.Add(new VisualDefaults_Builder()
                .DisplayName("Ice Cream")
                .UneditableText_Color(Color.FromRgb(253, 241, 56))
                .UneditableText_Background(Color.FromRgb(29, 114, 171))
                .ApplicationBackground(Color.Gray)
                .ButtonInnerBevelColor(Color.FromRgb(172, 68, 72))
                .ButtonOuterBevelColor(Color.FromRgb(218, 4, 16))
                .FontName("Qdbettercomicsans.ttf#QDBetterComicSans")
                .Build());

            all.Add(new VisualDefaults_Builder()
                .DisplayName("Fantasy")
                .UneditableText_Color(Color.FromRgb(117, 195, 118))
                .UneditableText_Background(Color.FromRgb(33, 52, 104))
                .ApplicationBackground(Color.FromRgb(87, 127, 174))
                .ButtonInnerBevelColor(Color.FromRgb(180, 169, 169))
                .ButtonOuterBevelColor(Color.FromRgb(210, 210, 210))
                .FontName("Beyond-Wonderland.ttf#Beyond-Wonderland")
                .Build());
            all.Add(new VisualDefaults_Builder()
                .DisplayName("Game")
                .UneditableText_Color(Color.FromRgb(190, 190, 190))
                .UneditableText_Background(Color.FromRgb(17, 24, 90))
                .ApplicationBackground(Color.FromRgb(60, 60, 65))
                .ButtonInnerBevelColor(Color.FromRgb(33, 159, 164))
                .ButtonOuterBevelColor(Color.FromRgb(1, 252, 255))
                .FontName("MinimalFont5x7.ttf#MinimalFont5x7")
                .FontSizeMultiplier(1.5)
                .Build());

            all.Add(new VisualDefaults_Builder()
                .DisplayName("Tuna")
                .UneditableText_Color(Color.FromRgb(225, 225, 225))
                .UneditableText_Background(Color.FromRgb(25, 62, 113))
                .ApplicationBackground(Color.FromRgb(145, 163, 189))
                .FontName("SatellaRegular.ttf#Satella")
                .Build());
            all.Add(new VisualDefaults_Builder()
                .DisplayName("Salad")
                .UneditableText_Color(Color.FromRgb(46, 96, 0))
                .UneditableText_Background(Color.FromRgb(219, 225, 138))
                .ApplicationBackground(Color.FromRgb(133, 162, 168))
                .ButtonInnerBevelColor(Color.FromRgb(99, 137, 39))
                .ButtonOuterBevelColor(Color.FromRgb(70, 120, 3))
                .FontName("Qdbettercomicsans.ttf#QDBetterComicSans")
                .Build());

            all.Add(new VisualDefaults_Builder()
                .DisplayName("Forest")
                .UneditableText_Color(Color.FromRgb(240, 245, 95))
                .UneditableText_Background(Color.FromRgb(21, 60, 8))
                .ApplicationBackground(Color.Black)
                .ButtonInnerBevelColor(Color.FromRgb(160, 160, 160))
                .ButtonOuterBevelColor(Color.FromRgb(200, 200, 200))
                .FontName("Beyond-Wonderland.ttf#Beyond-Wonderland")
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

            return all;
        }

        private void setupLoadingScreen()
        {
            VisualDefaults defaults = this.VisualDefaults;
            this.parentView.BackgroundColor = defaults.ViewDefaults.ApplicationBackground;
            TextblockLayout welcome = new TextblockLayout("I'm loading your data! Sincerely, " + this.persona.Name);
            welcome.AlignHorizontally(TextAlignment.Center);
            welcome.AlignVertically(TextAlignment.Center);
            TextblockLayout tip = new TextblockLayout(this.choose_usageTip());
            tip.AlignHorizontally(TextAlignment.Center);
            tip.AlignVertically(TextAlignment.Center);

            Vertical_GridLayout_Builder gridBuilder = new Vertical_GridLayout_Builder();
            gridBuilder.AddLayout(tip);
            gridBuilder.AddLayout(welcome);
            ViewManager viewManager = new ViewManager(this.parentView, gridBuilder.Build(), this.VisualDefaults);
        }

        // returns a tip/suggestion to show to the user
        private string choose_usageTip()
        {
            List<String> choices = new List<string>()
            {
                // miscellaneous facts
                "ActivityRecommender becomes more knowledgeable and more helpful as you record more data.",
                "Try making a backup of your data in the Export screen and opening the file to see what it looks like.",
                "All of the help screens in ActivityRecommender are useful and organized. If you like these tips, you should check them out!",
                "ActivityRecommender is open source. That means you can check out how it works and even make changes.",
                "Feedback is welcome! If you find a bug or want a new feature, please say so. Your name may even appear in the credits!",
                "ActivityRecommender was created in 2011.",
                "ActivityRecommender runs on Android, iOS, and Windows.",
                // mapping specific use cases to features
                "If you have lots of interesting ideas that you want to save for later, you can make them into ProtoActivities and prioritize them as you need to.",
                "If you want to measure your efficiency, you can start an experiment.",
                "If you want to update a friend on how your life is going, you can go to Analyze -> Life Summary to jog your memory.",
                "If you want to appreciate things you've done in the past, you can go to Analyze -> Search Participations and look for random good ones.",
                "If you want to find something about your life to change, you can go to Analyze -> Significant Activities and see what has affected your happiness the most recently.",
                "If you want to tell a friend what kinds of things you like to do, you can go to Analyze -> Favorite Activities and share its sorted list.",
                "If you want to see a graph of the time you spent on a certain activity over time, you can go to Analyze -> Visualize one Activity",
                // Some descriptions of how the algorithms work
                "Assigning the same parent activity to two child activities allows ActivityRecommender to know that they are similar and to use that in its predictions.",
                "ActivityRecommender models both your current happiness and your future happiness and attempts to maximize your future happiness.",
                "If you are happy now then it increases the probability that you will be happy in the future.",
                "ActivityRecommender separately keeps track of what you like to do and what kinds of suggestions you like to hear. They are probably the same but not necessarily!",
                "The concept of Net Present Value is a way to compare value tomorrow with value today. It suggests that value today is worth as much as slightly more value tomorrow.",
                "'Machine Learning' just refers to a computer algorithm that incorporates data to make predictions. Even fitting a line to a 2d scatter plot is a machine learning algorithm!",
                "ActivityRecommender saves some calculations in memory. Whenever it is restarted, some actions might take longer the next time." // StatList and AdaptiveInterpolator
            };
            return choices[this.randomGenerator.Next(choices.Count)];
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
            StreamReader reader = this.internalFileIo.OpenFileForReading(this.versionFilename);
            oldExecution.version = reader.ReadToEnd();
            reader.Close();
            reader.Dispose();
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
            ProtoActivities_Layout protoActivitiesLayout = new ProtoActivities_Layout(this.protoActivities_database, this.ActivityDatabase, this.layoutStack, this.publicFileIo, this.textConverter);

            ActivitySearchView searchView = new ActivitySearchView(this.ActivityDatabase, this.protoActivities_database, this.layoutStack, true);
            searchView.RequestDeletion += SearchView_RequestStartDeletion;

            this.activitiesMenuLayout = new ActivitiesMenuLayout(
                searchView,
                activityImportLayout,
                new ActivityCreationLayout(this.ActivityDatabase, this.layoutStack),
                new ActivityEditingLayout(this.ActivityDatabase, this.layoutStack),
                protoActivitiesLayout,
                this.layoutStack,
                this.ActivityDatabase);

            this.participationEntryView = new ParticipationEntryView(this.engine.ActivityDatabase, this.layoutStack);
            this.participationEntryView.Engine = this.engine;
            this.participationEntryView.AddOkClickHandler(new EventHandler(this.SubmitParticipation));
            this.participationEntryView.AddSetenddateHandler(new EventHandler(this.MakeEndNow));
            this.participationEntryView.AddSetstartdateHandler(new EventHandler(this.MakeStartNow));
            this.participationEntryView.LatestParticipation = this.latestParticipation;
            this.participationEntryView.VisitActivitiesScreen += ParticipationEntryView_VisitActivitiesScreen;
            this.participationEntryView.VisitSuggestionsScreen += ParticipationEntryView_VisitSuggestionsScreen;
            this.UpdateDefaultParticipationData();

            this.suggestionsView = new SuggestionsView(this, this.layoutStack, this.ActivityDatabase, this.engine);
            this.suggestionsView.AddSuggestions(this.recentUserData.Suggestions);
            this.suggestionsView.RequestSuggestion += SuggestionsView_RequestSuggestion;
            this.suggestionsView.ExperimentRequested += SuggestionsView_ExperimentRequested;
            this.suggestionsView.JustifySuggestion += SuggestionsView_JustifySuggestion;
            this.suggestionsView.AcceptedSuggestion += SuggestionsView_AcceptedSuggestion;
            this.suggestionsView.VisitActivitiesScreen += SuggestionsView_VisitActivitiesScreen;
            this.suggestionsView.LatestParticipation = this.latestParticipation;
            this.updateExperimentParticipationDemands();

            this.statisticsMenu = new StatisticsMenu(this.engine, this.layoutStack, this.publicFileIo, this.persona);
            this.statisticsMenu.AddParticipationComment += StatisticsMenu_AddParticipationComment;
            this.statisticsMenu.VisitActivitiesScreen += VisualizationMenu_VisitActivitiesScreen;
            this.statisticsMenu.VisitParticipationsScreen += VisualizationMenu_VisitParticipationsScreen;

            this.dataImportView = new DataImportView(this.layoutStack);
            this.dataImportView.RequestImport += this.ImportData;


            this.dataExportView = new DataExportView(this, this.persona, this.layoutStack);

            LayoutChoice_Set importExportView = new MenuLayoutBuilder(this.layoutStack)
                .AddLayout("Import", this.dataImportView)
                .AddLayout("Export", this.dataExportView)
                .Build();


            HomeScreen usageMenu = new HomeScreen(
                this.activitiesMenuLayout,
                this.participationEntryView,
                this.suggestionsView,
                this.statisticsMenu,
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
            Confirm_BackupBeforeRecalculateEfficiency_Layout recalculateEfficiency_layout = new Confirm_BackupBeforeRecalculateEfficiency_Layout();
            recalculateEfficiency_layout.Confirmed_BackupBefore_RecalculateEfficiency += RecalculateEfficiency_layout_Confirmed_BackupBefore_RecalculateEfficiency;
            debuggingBuilder.AddLayout("Recalculate Efficiency", recalculateEfficiency_layout);
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

            introMenu_builder.AddLayout("Feedback + Credits",
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
                                .AddContribution(ActRecContributor.MOM, new DateTime(2021, 5, 30), "Mentioned that the back buttons were hard to recognize as being back buttons")
                                .AddContribution(ActRecContributor.MOM, new DateTime(2021, 5, 30), "Mentioned that cursoring over a button in Windows made its text no longer visible")
                                .Build()
                            ),
                            layoutStack
                        )
                    )
               ).Build()
            );
            introMenu_builder.AddLayout("Debugging", debuggingBuilder.Build());

            LayoutChoice_Set helpOrStart_menu = introMenu_builder.Build();

            List<LayoutChoice_Set> startLayouts = new List<LayoutChoice_Set>();

            if (this.error != "")
            {
                TextblockLayout textLayout = new TextblockLayout(this.error, true, true);
                startLayouts.Add(textLayout);
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

                startLayouts.Add(new LayoutUnion(welcomeOptions));
                this.welcomeMessage = "";
            }

            if (startLayouts.Count == 0)
            {
                if (this.startupFeedback != "")
                {
                    TextblockLayout feedback = new TextblockLayout(this.startupFeedback);
                    feedback.AlignHorizontally(TextAlignment.Center);
                    feedback.AlignVertically(TextAlignment.Center);
                    startLayouts.Add(feedback);
                }
            }

            startLayouts.Add(helpOrStart_menu);

            helpOrStart_menu = new Vertical_GridLayout_Builder().AddLayouts(startLayouts).BuildAnyLayout();

            this.layoutStack.AddLayout(helpOrStart_menu, "Welcome", 0);
        }

        private void StatisticsMenu_AddParticipationComment(ParticipationComment comment)
        {
            this.addComment(comment);
        }
        private void addComment(ParticipationComment comment)
        {
            this.engine.PutCommentInMemory(comment);
            this.writeComment(comment);
        }
        private void writeComment(ParticipationComment comment)
        {
            string text = this.textConverter.ConvertToString(comment);
            this.internalFileIo.AppendText(text, this.ratingsFileName);
        }

        private void RecalculateEfficiency_layout_Confirmed_BackupBefore_RecalculateEfficiency()
        {
            this.ExportBeforeEfficiencyRecalculation();
        }

        private void SearchView_RequestStartDeletion(Activity activity)
        {
            Confirm_BackupBeforeDeleteActivity_Layout confirmLayout = new Confirm_BackupBeforeDeleteActivity_Layout(activity, this.layoutStack);
            confirmLayout.Confirmed_BackupBeforeDelete_Activity += ConfirmLayout_Confirmed_ExportBeforeDelete;
            this.layoutStack.AddLayout(confirmLayout, "Confirm");
        }

        private void ConfirmLayout_Confirmed_ExportBeforeDelete(Activity activity)
        {
            this.ExportBeforeDeletion(activity);
        }

        public async void ExportBeforeDeletion(Activity activity)
        {
            DeleteActivity_Button deleter = new DeleteActivity_Button(activity);
            deleter.RequestDeletion += DeleteButton_RequestDeleteActivityNow;
            FileShareResult exportResult = await this.ExportData(deleter);
            deleter.BackupContent = exportResult.Content;
        }

        private void DeleteButton_RequestDeleteActivityNow(Activity activity, string backupContent)
        {
            this.DeleteActivity(activity, backupContent);
        }

        private void DeleteActivity(Activity activity, string backupContent)
        {
            ActivityDeleter historyReplayer = new ActivityDeleter(activity);
            this.runHistoryRewriter(backupContent, historyReplayer);
        }

        public async void ExportBeforeEfficiencyRecalculation()
        {
            RenormalizeEfficiency_Button renormalizer = new RenormalizeEfficiency_Button();
            renormalizer.RequestRecalculation += RenormalizeEfficiencyButton_Clicked;
            FileShareResult exportResult = await this.ExportData(renormalizer);
            renormalizer.BackupContent = exportResult.Content;
        }

        private void RenormalizeEfficiencyButton_Clicked(string backupContent)
        {
            HistoryWriter rewriter = new RatingRenormalizer(false, true);
            this.runHistoryRewriter(backupContent, rewriter);
        }

        // runs the given HistoryRewriter against the given data and imports the result
        private void runHistoryRewriter(string data, HistoryWriter rewriter)
        {
            TextConverter importer = new TextConverter(null, new ActivityDatabase(null, null));
            PersistentUserData userData = importer.ParseForImport(new StringReader(data));

            this.loadDataFilesInto(userData, rewriter);
            string serialized = rewriter.Serialize();
            this.ImportData("modified data", new StringReader(serialized));
        }

        private void ParticipationEntryView_VisitStatisticsScreen()
        {
            this.layoutStack.GoBack();
            this.layoutStack.AddLayout(this.statisticsMenu, "Analyze");
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
            this.layoutStack.AddLayout(this.activitiesMenuLayout, "Organize Activities");
        }

        private void SuggestionsView_VisitActivitiesScreen()
        {
            this.ParticipationEntryView_VisitActivitiesScreen();
        }

        private void SuggestionsView_AcceptedSuggestion(ActivitySuggestion suggestion)
        {
            this.participationEntryView.SetActivityName(suggestion.ActivityDescriptor.ActivityName);
            this.SuggestionsView_VisitParticipationScreen();
        }
        private void SuggestionsView_VisitParticipationScreen()
        {
            this.layoutStack.GoBack();
            this.layoutStack.AddLayout(this.participationEntryView, "Record Participations");
        }

        private void ParticipationEntryView_VisitSuggestionsScreen()
        {
            this.layoutStack.GoBack();
            this.layoutStack.AddLayout(this.suggestionsView, "Suggest/Experiment");
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
            IEnumerable<ActivitiesSuggestion> existingSuggestions = this.suggestionsView.GetSuggestions();
            ActivitiesSuggestion suggestion = this.MakeRecommendation(request, existingSuggestions);
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
            ExperimentInitializationLayout experimentationLayout = new ExperimentInitializationLayout(this.layoutStack, this, this.ActivityDatabase, this.protoActivities_database, this.engine, 3 - this.recentUserData.NumRecent_UserChosen_ExperimentSuggestions);
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
            this.SuspectLatestActionDate(when, true);
            this.update_numRecent_userChosenExperimentSuggestions(choices);
            ExperimentSuggestion experimentSuggestion = this.engine.Experiment(choices, when);
            ActivitiesSuggestion activitySuggestion = new ActivitiesSuggestion(new List<ActivitySuggestion>() { experimentSuggestion.ActivitySuggestion });
            this.recentUserData.DemandedMetricName = experimentSuggestion.MetricName;
            this.AddSuggestion_To_SuggestionsView(activitySuggestion);

            PlannedExperiment experiment = experimentSuggestion.Experiment;

            if (!experiment.Started)
            {
                this.engine.PutExperimentInMemory(experiment);
                this.WriteExperiment(experiment);
            }

            this.layoutStack.RemoveLayout();
            this.layoutStack.RemoveLayout();
        }

        public void ImportData(object sender, OpenedFile fileData)
        {
            TextReader reader = new StreamReader(fileData.Content);
            this.ImportData(fileData.Path, reader);
        }
        private void doImport(TextReader content)
        {
            TextConverter importer = new TextConverter(null, new ActivityDatabase(null, null));
            PersistentUserData userData = importer.ParseForImport(content);
            this.internalFileIo.EraseFileAndWriteContent(this.personaFileName, userData.PersonaReader);
            this.internalFileIo.EraseFileAndWriteContent(this.inheritancesFileName, userData.InheritancesReader);
            this.internalFileIo.EraseFileAndWriteContent(this.ratingsFileName, userData.HistoryReader);
            this.internalFileIo.EraseFileAndWriteContent(this.recentUserData_fileName, userData.RecentUserDataReader);
            this.internalFileIo.EraseFileAndWriteContent(this.protoActivities_filename, userData.ProtoActivityReader);
        }
        public void ImportData(string filename, TextReader content)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.doImport(content);
            }
            else
            {
                try
                {
                    this.doImport(content);
                }
                catch (Exception e)
                {
                    TextblockLayout textLayout = new TextblockLayout("Could not import " + filename + " :\n" + e.ToString(), true, true);
                    this.layoutStack.AddLayout(textLayout, "Import Error");
                    return;
                }
            }
            this.Reload();
        }

        private PersistentUserData getPersistentUserData()
        {
            PersistentUserData data = new PersistentUserData();
            data.PersonaReader = this.internalFileIo.OpenFileForReading(this.personaFileName);
            data.InheritancesReader = this.internalFileIo.OpenFileForReading(this.inheritancesFileName);
            data.HistoryReader = this.internalFileIo.OpenFileForReading(this.ratingsFileName);
            data.RecentUserDataReader = this.internalFileIo.OpenFileForReading(this.recentUserData_fileName);
            data.ProtoActivityReader = this.internalFileIo.OpenFileForReading(this.protoActivities_filename);
            return data;
        }

        public async Task<FileShareResult> ExportData(LayoutChoice_Set successFooter = null)
        {
            string content = this.getPersistentUserData().serialize();

            DateTime now = DateTime.Now;
            string nowText = now.ToString("yyyy-MM-dd-HH-mm-ss");
            string fileName = "ActivityData-" + nowText + ".txt";

            // TODO make it possible for the user to control the file path
            await this.publicFileIo.Share(fileName, content);

            GridLayout_Builder builder = new Vertical_GridLayout_Builder()
                .AddLayout(new ExportSuccessLayout("file", this.publicFileIo));
            if (successFooter != null)
                builder.AddLayout(successFooter);
            this.layoutStack.AddLayout(builder.BuildAnyLayout(), "Exported");

            return new FileShareResult(content);
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
            engine = loader.GetEngine();
#else
            // If we get here then we run a special HistoryReplayer before startup
            // This is extra slow and extra confusing so to enable it you have to change the source code
            //HistoryReplayer historyReplayer = new RatingRenormalizer(false, true);
            HistoryReplayer historyReplayer = new FeedbackReplayer();
            this.loadDataFilesInto(historyReplayer);
            engine = historyReplayer.GetEngine();
            throw new NotImplementedException();
#endif
            this.engine = engine;
            this.persona.NameChanged += Persona_NameChanged;
            this.protoActivities_database = loader.ProtoActivity_Database;
            this.latestParticipation = loader.LatestParticipation;
            this.recentUserData = loader.RecentUserData;
            this.SuspectLatestActionDate(loader.LatestDate, false);
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
            this.computeStartupFeedback();
        }

        private void computeStartupFeedback()
        {
            this.startupFeedback = this.engine.ComputeBriefFeedback(DateTime.Now);
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
            this.loadDataFilesInto(data, historyReplayer);
        }
        private void loadDataFilesInto(PersistentUserData data, HistoryReplayer historyReplayer)
        {
            historyReplayer.ReadText(data.ProtoActivityReader);
            historyReplayer.ReadText(data.InheritancesReader);
            historyReplayer.ReadText(data.HistoryReader);
            historyReplayer.ReadText(data.RecentUserDataReader);
        }
        public ActivitySkip DeclineSuggestion(ActivitiesSuggestion suggestion)
        {
            // make a Skip object holding the needed data
            DateTime considerationDate = this.LatestActionDate;
            DateTime suggestionCreationDate = suggestion.Children[0].CreatedDate;
            ActivitySkip skip = new ActivitySkip(suggestion.ActivityDescriptors, suggestionCreationDate, considerationDate, DateTime.Now, suggestion.StartDate);

            this.AddSkip(skip);
            this.PersistSuggestions();

            return skip;
        }
        public void JustifySuggestion(ActivitySuggestion suggestion)
        {
            ActivitySuggestion_Explanation justification = this.engine.JustifySuggestion(suggestion);
            this.layoutStack.AddLayout(new ActivitySuggestion_Explanation_Layout(justification), "Why");
        }

        private void AddSuggestion_To_SuggestionsView(ActivitiesSuggestion suggestion)
        {
            // add the suggestion to the list (note that this makes the startDate a couple seconds later if it took a couple seconds to compute the suggestion)
            this.suggestionsView.AddSuggestion(suggestion);

            if (this.suggestionsView.GetSuggestions().Count() == 1)
            {
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
                ActivitiesSuggestion suggestion = this.suggestionsView.GetSuggestions().First();
                if (!suggestion.Skippable)
                {
                    demandedActivity = suggestion.Children[0].ActivityDescriptor;
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
        private ActivitiesSuggestion MakeRecommendation(ActivityRequest request, IEnumerable<ActivitiesSuggestion> existingSuggestions)
        {
            DateTime now = request.Date;
            this.SuspectLatestActionDate(now, true);
            
            if (request.FromCategory != null || request.ActivityToBeat != null)
            {
                // record the user's request for a certain activity
                this.AddActivityRequest(request);
            }

            // have the engine pretend that the user did everything we've suggested
            IEnumerable<Participation> hypotheticalParticipations = this.SupposeHypotheticalSuggestions(existingSuggestions);

            // now we get a recommendation, from among all activities within this category
            ActivitiesSuggestion suggestion = this.engine.MakeRecommendation(request);

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
            this.WriteSuggestion(new ActivitiesSuggestion(suggestion));
            return result;
        }

        public SuggestedMetric_Metadata Test_ChooseExperimentOption()
        {
            return this.engine.Test_ChooseExperimentOption();
        }

        private IEnumerable<Participation> SupposeHypotheticalSuggestions(IEnumerable<ActivitiesSuggestion> suggestions)
        {
            List<Participation> fakeParticipations = new List<Participation>(suggestions.Count());
            foreach (ActivitiesSuggestion suggestion in suggestions)
            {
                // pretend that the user took our suggestion and tell that to the engine
                Participation fakeParticipation = new Participation(suggestion.StartDate, suggestion.Children.First().EndDate.Value, suggestion.Children.First().ActivityDescriptor);
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
        private void WriteSuggestion(ActivitiesSuggestion suggestion)
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
            Participation participation = this.participationEntryView.GetParticipation(this.engine);
            if (participation == null)
                return;

            participation.Suggested = false;
            this.AddParticipation(participation);
            // fill in some default data for the ParticipationEntryView
            this.participationEntryView.Clear();

            IEnumerable<ActivitiesSuggestion> existingSuggestions = this.suggestionsView.GetSuggestions();
            if (existingSuggestions.Count() > 0 && existingSuggestions.First().CanMatch(participation.ActivityDescriptor))
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

            this.SuspectLatestActionDate(newParticipation.EndDate, false);

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
            this.SuspectLatestActionDate(newSkip.CreationDate, false);
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
            string text = this.textConverter.ConvertToString(this.protoActivities_database, false) + Environment.NewLine;
            this.internalFileIo.EraseFileAndWriteContent(this.protoActivities_filename, text);
        }




        // writes to a text file saying that the user was is this program now. It gets deleted soon

        // updates the ParticipationEntryView so that the start date is DateTime.Now
        public void MakeStartNow(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            this.SuspectLatestActionDate(now, true);
        }

#region Functions to be called by the TextConverter
        // updates the ParticipationEntryView so that the start date is 'when'
        public void SuspectLatestActionDate(DateTime when, bool save)
        {
            if (when.CompareTo(DateTime.Now) <= 0 && when.CompareTo(this.LatestActionDate) > 0)
            {
                this.LatestActionDate = when;
                //this.WriteInteractionDate(when);
                this.UpdateDefaultParticipationData(when);
                if (save)
                    this.writeRecentUserData_if_needed();
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
        public IEnumerable<ActivitiesSuggestion> CurrentSuggestions
        {
            get
            {
                return this.recentUserData.Suggestions;
            }
            set
            {
                this.recentUserData.Suggestions = value;
                this.participationEntryView.ExternalSuggestions = this.recentUserData.Suggestions;

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
        StatisticsMenu statisticsMenu;
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
        string startupFeedback = "";
        ProtoActivity_Database protoActivities_database;
        Persona persona;
        List<VisualDefaults> allLayoutDefaults;
        Random randomGenerator = new Random();

    }

    class ApplicationExecution
    {
        public string version;
        public bool debuggerAttached;
    }

}
