using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;
using System.Windows.Media;

// the ActivityRecommender class is the main class that connects the user-interface to the Engine
namespace ActivityRecommendation
{
    class ActivityRecommender
    {
        public ActivityRecommender(Window newMainWindow)
        {
            this.mainWindow = newMainWindow;

            this.InitializeSettings();

            this.SetupEngine();

            this.SetupDrawing();
        }

        private void InitializeSettings()
        {
            this.ratingsFileName = "ActivityRatings.txt";
            this.inheritancesFileName = "ActivityInheritances.txt";
            this.tempFileName = "TemporaryData.txt";
            this.textConverter = new TextConverter(this);
        }

        private void SetupDrawing()
        {
            String titleString = "ActivityRecommender By Jeff Gaston.";
            titleString += " Build Date: 2011-12-13T17:38:00";
            this.mainDisplay = new TitledControl(titleString);
            this.mainDisplayGrid = new DisplayGrid(1, 4);
            this.mainDisplay.SetContent(this.mainDisplayGrid);

            this.inheritanceEditingView = new InheritanceEditingView();
            this.inheritanceEditingView.ActivityDatabase = this.engine.ActivityDatabase;
            this.inheritanceEditingView.AddClickHandler(new RoutedEventHandler(this.SubmitInheritance));
            this.inheritanceEditingView.Background = new SolidColorBrush(Color.FromRgb(230, 230, 230));
            this.mainDisplayGrid.AddItem(this.inheritanceEditingView);

            this.participationEntryView = new ParticipationEntryView();
            this.participationEntryView.ActivityDatabase = this.engine.ActivityDatabase;
            this.participationEntryView.AddOkClickHandler(new RoutedEventHandler(this.SubmitParticipation));
            this.participationEntryView.AddAutofillClickHandler(new RoutedEventHandler(this.AutoFillParticipation));
            this.participationEntryView.Background = new SolidColorBrush(Color.FromRgb(220, 220, 220));
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

            this.mainWindow.Content = this.mainDisplay;

        }


        private void SetupEngine()
        {
            this.latestActionDate = new DateTime(0);
            this.engine = new Engine();
            this.ReadFiles();
            this.engine.FullUpdate();
            this.engine.MakeRecommendation();
        }
        private void ReadFiles()
        {
            this.textConverter.ReadFile(this.inheritancesFileName);
            this.textConverter.ReadFile(this.ratingsFileName);
            this.textConverter.ReadFile(this.tempFileName);
        }
        private void MakeRecommendation()
        {
            DateTime now = DateTime.Now;
            if ((this.latestRecommendedActivity != null) && !this.suppressDownoteOnRecommendation)
            {
                // if they've asked for two recommendations in succession, it means they didn't like the previous one
                Distribution previousDistribution = this.latestRecommendedActivity.LatestEstimatedRating;
                double estimatedScore = previousDistribution.Mean - previousDistribution.StdDev;
                if (estimatedScore < 0)
                {
                    estimatedScore = 0;
                }
                AbsoluteRating downvote = new AbsoluteRating(estimatedScore, now, this.latestRecommendedActivity.MakeDescriptor(), null);
                this.AddRating(downvote);
            }
            this.suppressDownoteOnRecommendation = false;

            // now determine which category to predict from
            Activity bestActivity = null;
            string categoryText = this.suggestionView.CategoryText;
            if (categoryText != null && categoryText != "")
            {
                ActivityDescriptor categoryDescriptor = new ActivityDescriptor();
                categoryDescriptor.NamePrefix = categoryText;
                categoryDescriptor.PreferBetterRatings = true;
                Activity category = this.engine.ActivityDatabase.ResolveDescriptor(categoryDescriptor);
                if (category != null)
                {
                    // if the user is requesting an idea from this category, we should upvote the category
                    double goodScore = 1;
                    AbsoluteRating upvote = new AbsoluteRating(goodScore, now, category.MakeDescriptor(), null);
                    this.AddRating(upvote);
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
                recommendationText = "There are activities that can be chosen!";
                justificationText = recommendationText;
            }
            else
            {
                recommendationText = bestActivity.Name;
                justificationText = this.engine.JustifyRating(bestActivity);
                this.suggestionView.ExpectedScoreText = bestActivity.LatestEstimatedRating.Mean.ToString();
                this.suggestionView.ScoreStdDevText = bestActivity.LatestEstimatedRating.StdDev.ToString();
            }
            this.suggestionView.SuggestionText = recommendationText;
            this.suggestionView.JustificationText = justificationText;
            this.latestRecommendedActivity = bestActivity;

            this.SuspectLatestActionDate(now);
        }
        private void MakeRecommendation(object sender, EventArgs e)
        {
            this.MakeRecommendation();
        }

        private void SubmitParticipation(object sender, EventArgs e)
        {
            this.SubmitParticipation();
        }
        private void SubmitParticipation()
        {
            // give the participation to the engine
            Participation participation = this.participationEntryView.Participation;
            this.AddParticipation(participation);
            // if there is a rating, give it to the engine too
            AbsoluteRating rating = this.participationEntryView.Rating;
            if (rating != null)
            {
                this.AddRating(rating);
            }
            // fill in some default data for the ParticipationEntryView
            this.latestActionDate = new DateTime(0);
            this.UpdateDefaultParticipationData();
            this.SuppressDownvoteOnRecommendation();
        }
        private void AutoFillParticipation(object sender, EventArgs e)
        {
            this.AutoFillParticipation();
        }
        private void AutoFillParticipation()
        {
            // first update the dates
            this.UpdateDefaultParticipationData();
            // now fill-in the latest activity name
            string latestName = "";
            if (this.latestRecommendedActivity != null)
            {
                latestName = this.latestRecommendedActivity.Name;
            }
            this.participationEntryView.ActivityName = latestName;
        }
        private void AddParticipation(Participation newParticipation)
        {
            this.engine.PutParticipationInMemory(newParticipation);
            this.WriteParticipation(newParticipation);
        }
        private void WriteParticipation(Participation newParticipation)
        {
            string text = this.textConverter.ConvertToString(newParticipation) + Environment.NewLine;
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
        private void AddRating(AbsoluteRating newRating)
        {
            this.engine.PutRatingInMemory(newRating);
            this.WriteRating(newRating);
        }
        private void WriteRating(AbsoluteRating newRating)
        {
            string text = this.textConverter.ConvertToString(newRating) + Environment.NewLine;
            StreamWriter writer = new StreamWriter(this.ratingsFileName, true);
            writer.Write(text);
            writer.Close();
        }

        // writes to a text file saying that the user was is this program now. It gets deleted soon
        private void WriteInteractionDate()
        {
            DateTime when = DateTime.Now;
        }
        private void WriteInteractionDate(DateTime when)
        {
            string text = this.textConverter.ConvertToString(when) + Environment.NewLine;
            StreamWriter writer = new StreamWriter(this.tempFileName, false);
            writer.Write(text);
            writer.Close();
        }
        #region Functions to be called by the TextConverter

        public void PutParticipationInMemory(Participation newParticipation)
        {
            this.engine.PutParticipationInMemory(newParticipation);
        }
        public void PutRatingInMemory(AbsoluteRating newRating)
        {
            this.engine.PutRatingInMemory(newRating);
        }
        public void PutActivityDescriptorInMemory(ActivityDescriptor newDescriptor)
        {
            this.engine.PutActivityDescriptorInMemory(newDescriptor);
        }
        public void PutInheritanceInMemory(Inheritance newInheritance)
        {
            this.engine.PutInheritanceInMemory(newInheritance);
        }
        public void SuspectLatestActionDate(DateTime when)
        {
            this.latestActionDate = when;
            this.WriteInteractionDate(when);
        }
        public void VisualizeData(object sender, EventArgs e)
        {
            this.VisualizeData();
        }
        public void VisualizeData()
        {
            string name = this.statisticsMenu.ActivityName;

            ActivityDescriptor descriptor = this.statisticsMenu.ActivityDescriptor;
            Activity activity = null;
            if (descriptor != null)
            {
                activity = this.engine.ActivityDatabase.ResolveDescriptor(descriptor);
                if (activity != null)
                {
                    ActivityVisualizationView visualizationView = new ActivityVisualizationView(activity);
                    visualizationView.AddExitClickHandler(new RoutedEventHandler(this.ShowMainview));
                    this.mainDisplay.SetContent(visualizationView);
                }
            }
        }

        void mainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            this.ShowMainview();
        }

        public void ShowMainview(object sender, EventArgs e)
        {
            this.ShowMainview();
        }
        public void ShowMainview()
        {
            this.mainDisplay.SetContent(this.mainDisplayGrid);
        }

        #endregion
        // declares that the user did something that means if they ask for another recommendation then we should not downvote the latest one
        private void SuppressDownvoteOnRecommendation()
        {
            //this.latestRecommendationDate = new DateTime(0);
            this.suppressDownoteOnRecommendation = true;
            this.suggestionView.SuggestionText = "<Click \"Suggest\" for a suggestion>";
            this.suggestionView.JustificationText = "<Here will be a short justification>";
            this.suggestionView.ExpectedScoreText = "<Here will be the expected score>";
            this.suggestionView.ScoreStdDevText = "<Here will be a measure of the uncertainty of the score>";
        }
        // fills in some default data for the ParticipationEntryView
        private void UpdateDefaultParticipationData()
        {
            this.participationEntryView.EndDate = DateTime.Now;
            this.participationEntryView.StartDate = this.LatestInteractionDate;
            this.participationEntryView.ActivityName = "";
            this.participationEntryView.RatingText = "";
        }
        private DateTime LatestInteractionDate
        {
            get
            {
                DateTime date1 = this.latestActionDate;
                DateTime date2 = this.engine.LatestInteractionDate;
                if (date1.CompareTo(date2) > 0)
                {
                    return date1;
                }
                else
                {
                    return date2;
                }
            }
        }
        

        private Window mainWindow;
        private DisplayGrid mainDisplayGrid;
        private TitledControl mainDisplay;

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
        DateTime latestActionDate;

    }
}