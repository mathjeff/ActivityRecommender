using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;
using System.Windows.Media;
using System.Windows.Controls;

// the ActivityRecommender class is the main class that connects the user-interface to the Engine
namespace ActivityRecommendation
{
    class ActivityRecommender
    {
        public ActivityRecommender(Window newMainWindow)
        {
            this.mainWindow = newMainWindow;
            this.displayManager = new DisplayManager(this.mainWindow);

            this.InitializeSettings();

            this.MakeEngine();

            this.ReadTempFile();

            this.SetupDrawing();

            if (this.engineTester != null)
            {
                this.engineTester.PrintResults();
                Console.WriteLine("");
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
            this.mainDisplay = new DisplayGrid(2, 1);

            String titleString = "ActivityRecommender By Jeff Gaston.";
            titleString += " Build Date: 2012-04-10T18:15";
            ResizableTextBlock titleBlock = new ResizableTextBlock();
            titleBlock.SetResizability(new Resizability(0, 1));
            titleBlock.Text = titleString;
            titleBlock.TextAlignment = TextAlignment.Center;
            //titleBlock.Background = new SolidColorBrush(Color.FromRgb(200, 200, 255));
            titleBlock.Background = Brushes.Cyan;

            this.mainDisplay.AddItem(titleBlock);

            this.mainDisplayGrid = new DisplayGrid(1, 4);
            //this.mainDisplayGrid.OverrideArrangement = true;
            this.mainDisplay.AddItem(this.mainDisplayGrid);

            this.inheritanceEditingView = new InheritanceEditingView();
            this.inheritanceEditingView.ActivityDatabase = this.engine.ActivityDatabase;
            this.inheritanceEditingView.AddClickHandler(new RoutedEventHandler(this.SubmitInheritance));
            this.inheritanceEditingView.Background = new SolidColorBrush(Color.FromRgb(230, 230, 230));
            this.mainDisplayGrid.AddItem(this.inheritanceEditingView);

            // this gets taken care of earlier so we don't get a null reference when we try to update it in response to the engine making changes
            //this.participationEntryView = new ParticipationEntryView();
            this.participationEntryView.ActivityDatabase = this.engine.ActivityDatabase;
            this.participationEntryView.AddOkClickHandler(new RoutedEventHandler(this.SubmitParticipation));
            this.participationEntryView.AddSetenddateHandler(new RoutedEventHandler(this.MakeEndNow));
            this.participationEntryView.AddSetstartdateHandler(new RoutedEventHandler(this.MakeStartNow));
            this.participationEntryView.Background = new SolidColorBrush(Color.FromRgb(220, 220, 220));
            this.participationEntryView.LatestParticipation = this.latestParticipation;
            this.mainDisplayGrid.AddItem(this.participationEntryView);
            this.UpdateDefaultParticipationData();

            this.suggestionView = new SuggestionView();
            this.suggestionView.AddSuggestionClickHandler(new RoutedEventHandler(this.MakeRecommendation));
            this.suggestionView.ActivityDatabase = this.engine.ActivityDatabase;
            this.suggestionView.Background = new SolidColorBrush(Color.FromRgb(210, 210, 210));
            this.mainDisplayGrid.AddItem(this.suggestionView);

            this.statisticsMenu = new MiniStatisticsMenu();
            this.statisticsMenu.ActivityDatabase = this.engine.ActivityDatabase;
            this.statisticsMenu.Background = new SolidColorBrush(Color.FromRgb(200, 200, 200));
            this.statisticsMenu.AddOkClickHandler(new RoutedEventHandler(this.VisualizeData));
            this.mainDisplayGrid.AddItem(this.statisticsMenu);
            this.mainWindow.KeyDown += new System.Windows.Input.KeyEventHandler(mainWindow_KeyDown);



            //this.mainWindow.Content = this.mainDisplay;
            this.displayManager.SetContent(this.mainDisplay);
            //this.mainWindow.Content = this.participationEntryView;

        }


        private void MakeEngine()
        {
            this.engine = new Engine();
            this.ReadEngineFiles();
            this.engine.FullUpdate();
            this.suppressDownoteOnRecommendation = true;        // the first time they get a recommendation, we don't treat it as a skip of the latest suggestion
        }
        public void PrepareEngine()
        {
            this.engine.MakeRecommendation(DateTime.Parse("2012-02-25T17:00:00"));
        }
        private void ReadEngineFiles()
        {
            this.textConverter.ReadFile(this.inheritancesFileName);
            this.textConverter.ReadFile(this.ratingsFileName);

            //this.textConverter.ReformatFile(this.ratingsFileName, "reformatted.txt");
        }
        private void ReadTempFile()
        {
            this.textConverter.ReadFile(this.tempFileName);
        }
        private void MakeRecommendation()
        {
            DateTime now = DateTime.Now;
            if ((this.LatestRecommendedActivity != null) && !this.suppressDownoteOnRecommendation)
            {
                // if they've asked for two recommendations in succession, it means they didn't like the previous one

                // Calculate the score to generate for this Activity as a result of that statement
                Distribution previousDistribution = this.LatestRecommendedActivity.PredictedScore.Distribution;
                double estimatedScore = previousDistribution.Mean - previousDistribution.StdDev;
                if (estimatedScore < 0)
                    estimatedScore = 0;
                // make a Skip object holding the needed data
                ActivitySkip skip = new ActivitySkip(now, this.LatestRecommendedActivity.MakeDescriptor());
                skip.SuggestionDate = this.LatestRecommendedActivity.PredictedScore.Date;
                AbsoluteRating rating = new AbsoluteRating();
                rating.Score = estimatedScore;
                skip.RawRating = rating;
                this.AddSkip(skip);

                //RatingSource source = RatingSource.Skip;
                //AbsoluteRating downvote = new AbsoluteRating(estimatedScore, now, this.latestRecommendedActivity.MakeDescriptor(), source);
                //this.AddRating(downvote);
            }
            this.suppressDownoteOnRecommendation = false;

            // now determine which category to predict from
            Activity bestActivity = null;
            string categoryText = this.suggestionView.CategoryText;
            if (categoryText != null && categoryText != "")
            {
                ActivityDescriptor categoryDescriptor = new ActivityDescriptor();
                categoryDescriptor.ActivityName = categoryText;
                categoryDescriptor.PreferHigherProbability = true;
                Activity category = this.engine.ActivityDatabase.ResolveDescriptor(categoryDescriptor);
                if (category != null)
                {
                    // if the user is requesting an idea from this category, we should upvote the category
                    //double goodScore = 1;
                    //RatingSource source = RatingSource.Request;
                    //AbsoluteRating upvote = new AbsoluteRating(goodScore, now, category.MakeDescriptor(), source);

                    ActivityRequest request = new ActivityRequest(category.MakeDescriptor(), now);
                    this.AddActivityRequest(request);
                    //this.AddRating(upvote);
                    // now we get a recommendation, from among all activities within this category
                    bestActivity = this.engine.MakeRecommendation(category, now);
                }
            }
            else
            {
                // now we get a recommendation
                bestActivity = this.engine.MakeRecommendation(now);
            }
            string recommendationText;
            string justificationText;
            if (bestActivity == null)
            {
                recommendationText = "There are no activities that can be chosen!";
                justificationText = recommendationText;
            }
            else
            {
                recommendationText = bestActivity.Name;
                justificationText = this.engine.JustifyRating(bestActivity);
                this.suggestionView.ExpectedScoreText = bestActivity.PredictedScore.Distribution.Mean.ToString();
                this.suggestionView.ScoreStdDevText = bestActivity.PredictedScore.Distribution.StdDev.ToString();
                this.suggestionView.ParticipationProbabilityText = bestActivity.PredictedParticipationProbability.Distribution.Mean.ToString();
            }
            this.suggestionView.SuggestionText = recommendationText;
            this.suggestionView.JustificationText = justificationText;
            this.LatestSuggestion = new ActivitySuggestion(bestActivity.MakeDescriptor());

            this.SuspectLatestActionDate(now);
            if (bestActivity != null)
            {
                this.participationEntryView.SetActivityName(bestActivity.Name);
            }
        }
        private void MakeRecommendation(object sender, EventArgs e)
        {
            this.MakeRecommendation();
        }
        private void WriteSuggestion(Activity suggestedActivity)
        {
            ActivityDescriptor descriptor = suggestedActivity.MakeDescriptor();
        }
        private void SubmitParticipation(object sender, EventArgs e)
        {
            this.SubmitParticipation();
        }
        private void SubmitParticipation()
        {
            // give the participation to the engine
            Participation participation = this.participationEntryView.GetParticipation(this.engine.ActivityDatabase, this.engine);

            if (this.LatestRecommendedActivity != null && this.ActivityDatabase.Matches(participation.ActivityDescriptor, this.LatestRecommendedActivity))
                participation.Suggested = true;
            else
                participation.Suggested = false;
            this.AddParticipation(participation);
            /* // if there is a rating, give it to the engine too
            AbsoluteRating rating = this.participationEntryView.Rating;
            if (rating != null)
            {
                this.AddRating(rating);
            }
            */
            // fill in some default data for the ParticipationEntryView
            //this.latestActionDate = new DateTime(0);
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
            /*if (newParticipation.Rating != null)
            {
                this.AddRating(newParticipation.Rating);
            }*/
        }
        private void WriteParticipation(Participation newParticipation)
        {
            string text = this.textConverter.ConvertToString(newParticipation) + Environment.NewLine;
            StreamWriter writer = new StreamWriter(this.ratingsFileName, true);
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
            StreamWriter writer = new StreamWriter(this.ratingsFileName, true);
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
            StreamWriter writer = new StreamWriter(this.ratingsFileName, true);
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

            this.inheritanceEditingView.ChildName = null;
            this.inheritanceEditingView.ParentName = null;

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
            string text = this.textConverter.ConvertToString(newInheritance) + Environment.NewLine;
            StreamWriter writer = new StreamWriter(this.inheritancesFileName, true);
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
            StreamWriter writer = new StreamWriter(this.ratingsFileName, true);
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
            StreamWriter writer = new StreamWriter(this.tempFileName, false);
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
                ActivityVisualizationView visualizationView = new ActivityVisualizationView(xAxisProgression, yAxisActivity, new TimeSpan());
                visualizationView.AddExitClickHandler(new RoutedEventHandler(this.ShowMainview));
                this.mainDisplay.PutItem(visualizationView, 1, 0);
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
            this.mainDisplay.PutItem(this.mainDisplayGrid, 1, 0);
            //this.displayManager.InvalidateMeasure();
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
            this.suppressDownoteOnRecommendation = true;
            this.suggestionView.ResetText();
        }
        // fills in some default data for the ParticipationEntryView
        private void UpdateDefaultParticipationData()
        {
            DateTime now = DateTime.Now;
            this.UpdateDefaultParticipationData(now);
        }
        private void UpdateDefaultParticipationData(DateTime when)
        {
            this.participationEntryView.Clear();

            this.participationEntryView.EndDate = when;
            this.participationEntryView.SetStartDate(this.LatestActionDate);
            //this.participationEntryView.ActivityName = "";
            //this.participationEntryView.RatingText = "";
            //this.participationEntryView.CommentText = "";
        }


        private Window mainWindow;
        private DisplayManager displayManager;
        private DisplayGrid mainDisplayGrid;
        private DisplayGrid mainDisplay;

        ParticipationEntryView participationEntryView;
        InheritanceEditingView inheritanceEditingView;
        SuggestionView suggestionView;
        MiniStatisticsMenu statisticsMenu;
        Engine engine;
        //DateTime latestRecommendationDate;
        Activity latestRecommendedActivity;
        bool suppressDownoteOnRecommendation;
        TextConverter textConverter;
        string ratingsFileName;         // the name of the file that stores ratings
        string inheritancesFileName;    // the name of the file that stores inheritances
        string tempFileName;
        //DateTime latestActionDate;
        Participation latestParticipation;
        EngineTester engineTester;
        RecentUserData recentUserData;

    }
}