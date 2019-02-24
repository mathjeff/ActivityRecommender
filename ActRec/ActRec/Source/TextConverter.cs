using ActivityRecommendation.Effectiveness;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

// The TextConverter class will convert an object into a string or parse a string into a list of objects
// It only works on the specific types of objects that matter in the ActivityRecommender project
namespace ActivityRecommendation
{
    public class TextConverter
    {
        #region Constructor

        public TextConverter(HistoryReplayer listener, ActivityDatabase activityDatabase)
        {
            this.listener = listener;
            this.activityDatabase = activityDatabase;
        }

        #endregion

        #region Public Member Functions

        // converts the rating into a string that is ready to write to disk
        public string ConvertToString(Rating rating)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();
            string objectName = this.RatingTag;
            string objectValue = this.ConvertToStringBody(rating);

            return this.ConvertToString(objectValue, objectName);
        }
        // converts the participation into a string that is ready to write to disk
        public string ConvertToString(Participation participation)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();
            string objectName = this.ParticipationTag;

            // the activity being described
            properties[this.ActivityDescriptorTag] = this.ConvertToStringBody(participation.ActivityDescriptor);
            // start/end dates
            properties[this.ParticipationStartDateTag] = this.ConvertToStringBody(participation.StartDate);
            properties[this.ParticipationEndDateTag] = this.ConvertToStringBody(participation.EndDate);
            // rating
            Rating rating = participation.RawRating;
            if (rating != null)
                properties[this.RatingTag] = this.ConvertToStringBody(rating);
            string comment = participation.Comment;
            if (participation.Suggested != null)
                properties[this.WasSuggestedTag] = this.ConvertToStringBody((bool)participation.Suggested);
            if (comment != null)
                properties[this.CommentTag] = this.XmlEscape(comment);
            if (participation.Consideration != null)
                properties[this.ConsiderationTag] = this.ConvertToStringBody(participation.Consideration);
            if (participation.CompletedMetric)
                properties[this.ParticipationSuccessful_Tag] = this.ConvertToStringBody(true);
            if (participation.RelativeEfficiencyMeasurement != null)
                properties[this.EfficiencyMeasurement_Tag] = this.ConvertToStringBody(participation.RelativeEfficiencyMeasurement);

            return this.ConvertToString(properties, objectName);
        }
        // converts the Inheritance into a string that is ready to write to disk
        public string ConvertToString(Inheritance inheritance)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();
            string objectName = this.InheritanceTag;

            properties[this.InheritanceChildTag] = this.ConvertToStringBody(inheritance.ChildDescriptor);
            properties[this.InheritanceParentTag] = this.ConvertToStringBody(inheritance.ParentDescriptor);
            if (inheritance.DiscoveryDate != null)
                properties[this.DiscoveryDateTag] = this.ConvertToStringBody((DateTime)inheritance.DiscoveryDate);

            return this.ConvertToString(properties, objectName);

        }
        public string ConvertToString(Activity activity)
        {
            ToDo toDo = activity as ToDo;
            if (toDo != null)
                return this.ConvertToString(toDo);
            Category category = activity as Category;
            if (category != null)
                return this.ConvertToString(category);
            throw new Exception("Unsupported Activity type: " + activity);
        }
        public string ConvertToString(ToDo toDo)
        {
            string body = this.ConvertToStringBody(toDo.MakeDescriptor());
            return this.ConvertToString(body, this.TodoTag);
        }
        public string ConvertToString(Category category)
        {
            string body = this.ConvertToStringBody(category.MakeDescriptor());
            return this.ConvertToString(body, this.CategoryTag);
        }

        // converts the ActivityRequest into a string that is ready to write to disk
        public string ConvertToString(ActivityRequest request)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();
            string objectName = this.ActivityRequestTag;

            if (request.FromCategory != null)
                properties[this.ActivityDescriptorTag] = this.ConvertToStringBody(request.FromCategory);
            if (request.ActivityToBeat != null)
                properties[this.ActivityToBeat_Tag] = this.ConvertToStringBody(request.ActivityToBeat);
            properties[this.DateTag] = this.ConvertToStringBody(request.Date);

            return this.ConvertToString(properties, objectName);
        }
        // converts the RecentUserData into a string that is ready to write to disk
        public string ConvertToString(RecentUserData data)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();
            string objectName = this.RecentUserDataTag;

            if (data.LatestActionDate != null)
                properties[this.DateTag] = this.ConvertToStringBody(data.LatestActionDate);
            if (data.Suggestions != null)
                properties[this.SuggestionsTag] = this.ConvertToStringBody(data.Suggestions);

            return this.ConvertToString(properties, objectName);
        }


        // converts the DateTime into a string that is ready to write to disk
        public string ConvertToString(DateTime when)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();
            string objectName = this.DateTag;

            string contents = this.ConvertToStringBody(when);

            return this.ConvertToString(contents, objectName);
        }
        // converts the Skip into a string that is ready to write to disk
        public string ConvertToString(ActivitySkip skip)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();
            string objectName = this.SkipTag;

            properties[this.ActivityDescriptorTag] = this.ConvertToStringBody(skip.ActivityDescriptor);
            properties[this.DateTag] = this.ConvertToStringBody(skip.CreationDate);
            properties[this.SuggestionCreationDate] = this.ConvertToStringBody(skip.SuggestionCreationDate);
            if (skip.SuggestionStartDate != skip.SuggestionCreationDate)
                properties[this.SuggestionStartDateTag] = this.ConvertToStringBody(skip.SuggestionStartDate);
            if (skip.ConsideredSinceDate != skip.SuggestionCreationDate)
                properties[this.SkipConsideredSinceDate] = this.ConvertToStringBody(skip.ConsideredSinceDate);

            return this.ConvertToString(properties, objectName);
        }
        public string ConvertToString(ActivitySuggestion activitySuggestion)
        {
            string body = this.ConvertToStringBody(activitySuggestion);
            return this.ConvertToString(body, this.SuggestionTag);
        }
        public string ConvertToStringBody(ActivitySuggestion activitySuggestion)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();

            properties[this.ActivityDescriptorTag] = this.ConvertToStringBody(activitySuggestion.ActivityDescriptor);
            if (activitySuggestion.CreatedDate != null && !activitySuggestion.CreatedDate.Equals(activitySuggestion.StartDate))
                properties[this.SuggestionCreationDate] = this.ConvertToStringBody(activitySuggestion.CreatedDate);
            properties[this.SuggestionStartDateTag] = this.ConvertToStringBody(activitySuggestion.StartDate);
            if (activitySuggestion.EndDate != null)
                properties[this.SuggestionEndDateTag] = this.ConvertToStringBody(activitySuggestion.EndDate);
            if (!activitySuggestion.Skippable)
                properties[this.SkippableTag] = this.ConvertToStringBody(activitySuggestion.Skippable);

            return this.ConvertToStringBody(properties);
        }
        public string ConvertToString(PlannedExperiment experiment)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();

            properties[this.EarlierSuggestionInExperimentTag] = this.ConvertToStringBody(experiment.Earlier);
            properties[this.LaterSuggestionInExperimentTag] = this.ConvertToStringBody(experiment.Later);

            return this.ConvertToString(properties, this.ExperimentTag);
            
        }
        public string ConvertToStringBody(PlannedMetric experiment)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();

            properties[this.ActivityDescriptorTag] = this.ConvertToStringBody(experiment.ActivityDescriptor);

            // record the metric if it's not the first metric (in most (initially all at the time of writing) cases it will be the first metric)
            Activity activity = this.activityDatabase.ResolveDescriptor(experiment.ActivityDescriptor);
            if (activity.Metrics.Count < 1)
                throw new ArgumentException("Internal error: an ExperimentSuggestion's Activity cannot have 0 metrics");            
            if (experiment.MetricName != activity.Metrics[0].Name && experiment.MetricName != null)
                properties[this.MetricTag] = this.XmlEscape(experiment.MetricName);

            properties[this.SuccessRateTag] = this.ConvertToStringBody(experiment.DifficultyEstimate.EstimatedSuccessesPerSecond);
            properties[this.NumEasierParticipationsTag] = this.ConvertToStringBody(experiment.DifficultyEstimate.NumEasiers);
            properties[this.NumHarderParticipationsTag] = this.ConvertToStringBody(experiment.DifficultyEstimate.NumHarders);

            return this.ConvertToStringBody(properties);
        }
        public string ConvertToString(Metric metric)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();
            properties[this.MetricName_Tag] = this.XmlEscape(metric.Name);
            properties[this.ActivityDescriptorTag] = this.ConvertToStringBody(metric.ActivityDescriptor);

            return this.ConvertToString(properties, this.MetricTag);
        }

        // converts the dictionary into a string that is ready to be written to disk
        private string ConvertToString(Dictionary<string, string> properties, string objectName)
        {
            string value = this.ConvertToStringBody(properties);
            return this.ConvertToString(value, objectName);
        }
        // wraps the given body with the appropriate open/close tags
        public string ConvertToString(string stringBody, string objectName)
        {
            return "<" + objectName + ">" + stringBody + "</" + objectName + ">";
        }

        private IEnumerable<XmlNode> ParseToXmlNodes(string text)
        {
            if (text.Length <= 0)
                return null;
            text = "<root>" + text + "</root>";
            XmlDocument document = new XmlDocument();
            try
            {
                document.LoadXml(text);
            }
            catch (XmlException e)
            {
                int lineNumber = e.LineNumber - 1;
                string[] lines = text.Split('\n');
                if (lineNumber >= 0 && lineNumber < lines.Length)
                {
                    string line = lines[lineNumber];
                    throw new XmlException("Failed to parse '" + lines[lineNumber] + "'", e);
                }
                throw e;
            }
            XmlNode root = document.FirstChild;
            if (root == null)
                return null;
            return root.ChildNodes;
        }

        // converts the given text into a sequence of objects and sends them to the Engine
        public void ReadText(string text)
        {
            IEnumerable<XmlNode> nodes = this.ParseToXmlNodes(text);
            if (nodes == null)
                return;
            foreach (XmlNode node in nodes)
            {
                if (node.Name == this.CategoryTag)
                {
                    this.ProcessCategory(node);
                    continue;
                }
                if (node.Name == this.TodoTag)
                {
                    this.ProcessTodo(node);
                    continue;
                }
                if (node.Name == this.ParticipationTag)
                {
                    this.ProcessParticipation(node);
                    continue;
                }
                if (node.Name == this.RatingTag)
                {
                    this.ProcessRating(node);
                    continue;
                }
                if (node.Name == this.ActivityDescriptorTag)
                {
                    this.ProcessActivityDescriptor(node);
                    continue;
                }
                if (node.Name == this.InheritanceTag)
                {
                    this.ProcessInheritance(node);
                    continue;
                }
                if (node.Name == this.DateTag)
                {
                    this.ProcessLatestDate(node);
                    continue;
                }
                if (node.Name == this.SkipTag)
                {
                    this.ProcessSkip(node);
                    continue;
                }
                if (node.Name == this.ActivityRequestTag)
                {
                    this.ProcessActivityRequest(node);
                    continue;
                }
                if (node.Name == this.RecentUserDataTag)
                {
                    this.ProcessRecentUserData(node);
                    continue;
                }
                if (node.Name == this.SuggestionTag)
                {
                    this.ProcessSuggestion(node);
                    continue;
                }
                if (node.Name == this.ExperimentTag)
                {
                    this.ProcessExperiment(node);
                    continue;
                }
                if (node.Name == this.MetricTag)
                {
                    this.ProcessMetric(node);
                    continue;
                }
                throw new Exception("Unrecognized node: <" + node.Name + ">");
            }
        }

        private void ProcessLatestDate(XmlNode nodeRepresentation)
        {
            DateTime when = this.ReadDate(nodeRepresentation);
            this.listener.SetLatestDate(when);
        }
        private void ProcessCategory(XmlNode nodeRepresentation)
        {
            // the Category just puts all of the fields of the ActivityDescriptor at the top level
            ActivityDescriptor activityDescriptor = this.ReadActivityDescriptor(nodeRepresentation);
            Category category = this.activityDatabase.CreateCategory(activityDescriptor);
            this.listener.PostCategory(category);
        }
        private void ProcessTodo(XmlNode nodeRepresentation)
        {
            // the Todo just puts all of the fields of the ActivityDescriptor at the top level
            ActivityDescriptor activityDescriptor = this.ReadActivityDescriptor(nodeRepresentation);
            ToDo todo = this.activityDatabase.CreateToDo(activityDescriptor);
            this.listener.PostToDo(todo);
        }
        private void ProcessParticipation(XmlNode nodeRepresentation)
        {
            Participation currentParticipation = this.ReadParticipation(nodeRepresentation);
            // we found something that happened after the latest pending skip, so we update the date of any pending skip to the start date
            this.setPendingSkip(null, currentParticipation.StartDate);
            if (currentParticipation.Duration.TotalSeconds >= 0)
                this.listener.AddParticipation(currentParticipation);
            else
                System.Diagnostics.Debug.WriteLine("Skipping invalid participation having startDate = " + currentParticipation.StartDate.ToString() + " and endDate = " + currentParticipation.EndDate);
        }
        private void ProcessRating(XmlNode nodeRepresentation)
        {
            Rating currentRating = this.ReadRating(nodeRepresentation);
            this.listener.AddRating(currentRating);
        }
        private void ProcessActivityDescriptor(XmlNode nodeRepresentation)
        {
            ActivityDescriptor descriptor = this.ReadActivityDescriptor(nodeRepresentation);
            this.listener.PreviewActivityDescriptor(descriptor);
        }
        private Inheritance ProcessInheritance(XmlNode nodeRepresentation)
        {
            Inheritance inheritance = this.ReadInheritance(nodeRepresentation);
            this.activityDatabase.AddInheritance(inheritance);
            if (this.listener != null)
                this.listener.AddInheritance(inheritance);
            return inheritance;
        }
        private void ProcessSkip(XmlNode nodeRepresentation)
        {
            ActivitySkip skip = this.ReadSkip(nodeRepresentation);
            if (skip != null)
                this.listener.AddSkip(skip);
        }
        private void ProcessActivityRequest(XmlNode nodeRepresentation)
        {
            ActivityRequest request = this.ReadActivityRequest(nodeRepresentation);
            this.listener.AddRequest(request);
        }
        private void ProcessRecentUserData(XmlNode nodeRepresentation)
        {
            RecentUserData data = this.ReadRecentUserData(nodeRepresentation);
            data.Synchronized = true;
            this.listener.SetRecentUserData(data);
        }
        private void ProcessSuggestion(XmlNode nodeRepresentation)
        {
            ActivitySuggestion suggestion = this.ReadSuggestion(nodeRepresentation);
            this.setPendingSkip(null, suggestion.CreatedDate);
            this.listener.AddSuggestion(suggestion);
        }
        private void ProcessExperiment(XmlNode nodeRepresentation)
        {
            PlannedExperiment experiment = this.ReadExperiment(nodeRepresentation);
            this.listener.AddExperiment(experiment);
        }
        private Metric ProcessMetric(XmlNode nodeRepresentation)
        {
            Metric metric = this.ReadMetric(nodeRepresentation);
            Activity activity = this.activityDatabase.ResolveDescriptor(metric.ActivityDescriptor);
            activity.AddMetric(metric);
            if (this.listener != null)
                this.listener.AddMetric(metric);

            return metric;
        }
        // reads the Inheritance represented by nodeRepresentation
        private Inheritance ReadInheritance(XmlNode nodeRepresentation)
        {
            Inheritance inheritance = new Inheritance();
            foreach (XmlNode currentChild in nodeRepresentation.ChildNodes)
            {
                if (currentChild.Name == this.InheritanceChildTag)
                {
                    inheritance.ChildDescriptor = this.ReadActivityDescriptor(currentChild);
                    continue;
                }
                if (currentChild.Name == this.InheritanceParentTag)
                {
                    inheritance.ParentDescriptor = this.ReadActivityDescriptor(currentChild);
                    continue;
                }
                if (currentChild.Name == this.DiscoveryDateTag)
                {
                    inheritance.DiscoveryDate = this.ReadDate(currentChild);
                    continue;
                }
            }
            return inheritance;
        }

        // reads the Participation represented by nodeRepresentation
        private Participation ReadParticipation(XmlNode nodeRepresentation)
        {
            // the participation being read
            // The participation may have an embedded rating
            Rating rating = null;
            ActivityDescriptor activityDescriptor = new ActivityDescriptor();
            DateTime startDate = DateTime.Now;
            DateTime endDate = DateTime.Now;
            string comment = null;
            bool? suggested = null;
            bool successful = false;
            RelativeEfficiencyMeasurement efficiencyMeasurement = null;
            foreach (XmlNode currentChild in nodeRepresentation.ChildNodes)
            {
                if (currentChild.Name == this.ActivityDescriptorTag)
                {
                    activityDescriptor = this.ReadActivityDescriptor(currentChild);
                    continue;
                }
                if (currentChild.Name == this.ParticipationStartDateTag)
                {
                    startDate = this.ReadDate(currentChild);
                    continue;
                }
                if (currentChild.Name == this.ParticipationEndDateTag)
                {
                    endDate = this.ReadDate(currentChild);
                    continue;
                }
                if (currentChild.Name == this.RatingTag)
                {
                    rating = this.ReadRating(currentChild);
                    // because the RelativeRatings are more accurate (avoiding grade-inflation), we currently ignore each AbsoluteRating provided by the user
                    AbsoluteRating absolute = rating as AbsoluteRating;
                    if (absolute != null)
                    {
                        if (absolute.FromUser)
                            rating = null;
                    }
                    continue;
                }
                if (currentChild.Name == this.CommentTag)
                {
                    comment = this.ReadText(currentChild);
                    continue;
                }
                if (currentChild.Name == this.WasSuggestedTag)
                {
                    suggested = this.ReadBool(currentChild);
                    continue;
                }
                if (currentChild.Name == this.ParticipationSuccessful_Tag)
                {
                    successful = this.ReadBool(currentChild);
                    continue;
                }
                if (currentChild.Name == this.EfficiencyMeasurement_Tag)
                {
                    efficiencyMeasurement = this.ReadEfficiencyMeasurement(currentChild);
                    continue;
                }
            }
            Participation currentParticipation = new Participation(startDate, endDate, activityDescriptor);
            if (rating != null)
            {
                // inform the rating of the participation that generated it
                rating.Source = RatingSource.FromParticipation(currentParticipation);
                // In case it was a relative rating, give the rating a chance to keep a pointer to the previous participation
                RelativeRating convertedRating = rating as RelativeRating;
                if (convertedRating != null)
                {
                    convertedRating.AttemptToMatch(this.latestParticipationRead);
                }
            }
            currentParticipation.RawRating = rating;
            currentParticipation.Comment = comment;
            currentParticipation.Suggested = suggested;
            if (successful || efficiencyMeasurement != null)
            {
                currentParticipation.EffectivenessMeasurement = new CompletionEfficiencyMeasurement(successful);
                if (efficiencyMeasurement != null)
                {
                    efficiencyMeasurement.FillInFromParticipation(currentParticipation);
                }
                currentParticipation.EffectivenessMeasurement.Computation = efficiencyMeasurement;
            }

            this.latestParticipationRead = currentParticipation;
            return currentParticipation;
        }

        private RelativeEfficiencyMeasurement ReadEfficiencyMeasurement(XmlNode nodeRepresentation)
        {
            RelativeEfficiencyMeasurement measurement = new RelativeEfficiencyMeasurement();
            double weight = 1;
            double mean = 1;
            foreach (XmlNode child in nodeRepresentation.ChildNodes)
            {
                if (child.Name == this.ActivityDescriptorTag)
                {
                    measurement.ActivityDescriptor = this.ReadActivityDescriptor(child);
                    continue;
                }
                if (child.Name == this.ParticipationStartDateTag)
                {
                    measurement.StartDate = this.ReadDate(child);
                    continue;
                }
                if (child.Name == this.ParticipationEndDateTag)
                {
                    measurement.EndDate = this.ReadDate(child);
                    continue;
                }
                if (child.Name == this.EfficiencyValue_Tag)
                {
                    mean = this.ReadDouble(child);
                    continue;
                }
                if (child.Name == this.EfficiencyWeight_Tag)
                {
                    weight = this.ReadDouble(child);
                    continue;
                }
                if (child.Name == this.EarlierEfficency_Tag)
                {
                    measurement.Earlier = this.ReadEfficiencyMeasurement(child);
                    continue;
                }
            }
            measurement.RecomputedEfficiency = Distribution.MakeDistribution(mean, 0, weight);
            return measurement;
        }

        // returns an object of type "Skip" that this XmlNode represents
        private ActivitySkip ReadSkip(XmlNode nodeRepresentation)
        {
            ActivityDescriptor activityDescriptor = null;
            DateTime? suggestionCreationDate = null;
            DateTime? consideredSinceDate = null;
            DateTime? suggestionStartDate = null;
            DateTime? skipCreationDate = null;
            AbsoluteRating rawRating = null;
            foreach (XmlNode currentChild in nodeRepresentation.ChildNodes)
            {
                if (currentChild.Name == this.ActivityDescriptorTag)
                {
                    activityDescriptor = this.ReadActivityDescriptor(currentChild);
                    continue;
                }
                if (currentChild.Name == this.SuggestionCreationDate)
                {
                    suggestionCreationDate = this.ReadDate(currentChild);
                    continue;
                }
                if (currentChild.Name == this.SkipConsideredSinceDate)
                {
                    consideredSinceDate = this.ReadDate(currentChild);
                    continue;
                }
                if (currentChild.Name == this.DateTag)
                {
                    skipCreationDate = this.ReadDate(currentChild);
                    continue;
                }
                if (currentChild.Name == this.SuggestionStartDateTag)
                {
                    suggestionStartDate = this.ReadDate(currentChild);
                    continue;
                }
                if (currentChild.Name == this.RatingTag)
                {
                    rawRating = this.ReadAbsoluteRating(currentChild);
                    continue;
                }
            }
            if (suggestionStartDate == null)
            {
                // Fill in default value for suggestionStartDate
                suggestionStartDate = suggestionCreationDate;
            }
            if (consideredSinceDate == null)
            {
                // fill in default value for suggestionCreationDate
                consideredSinceDate = suggestionCreationDate;
            }
            return new ActivitySkip(activityDescriptor, suggestionCreationDate.Value, consideredSinceDate.Value, skipCreationDate.Value, suggestionStartDate.Value);
        }

        // sets the pending skip at the given time (and submits the previous pending skip if it exists)
        private void setPendingSkip(ActivitySkip skip, DateTime when)
        {
            if (this.pendingSkip != null)
            {
                if (this.pendingSkip.CreationDate.CompareTo(when) < 0)
                    this.pendingSkip.CreationDate = when;
                this.listener.AddSkip(this.pendingSkip);
            }
            this.pendingSkip = skip;
        }

        private ActivityRequest ReadActivityRequest(XmlNode nodeRepresentation)
        {
            ActivityRequest request = new ActivityRequest();
            foreach (XmlNode currentChild in nodeRepresentation.ChildNodes)
            {
                if (currentChild.Name == this.ActivityDescriptorTag)
                {
                    request.FromCategory = this.ReadActivityDescriptor(currentChild);
                    continue;
                }
                if (currentChild.Name == this.DateTag)
                {
                    request.Date = this.ReadDate(currentChild);
                    continue;
                }
                if (currentChild.Name == this.ActivityToBeat_Tag)
                {
                    request.ActivityToBeat = this.ReadActivityDescriptor(currentChild);
                    continue;
                }
            }
            return request;
        }

        private DateTime ReadDate(XmlNode nodeRepresentation)
        {
            string text = this.ReadText(nodeRepresentation);
            return this.ReadDate(text);
        }
        private DateTime ReadDate(string text)
        {
            return DateTime.Parse(text);
        }
        private bool ReadBool(XmlNode nodeRepresentation)
        {
            return bool.Parse(this.ReadText(nodeRepresentation));
        }
        private string ReadText(XmlNode nodeRepresentation)
        {
            XmlNode firstChild = nodeRepresentation.FirstChild;
            if (firstChild != null)
            {
                return firstChild.Value;
            }
            return "";
        }
        private Rating ReadRating(XmlNode nodeRepresentation)
        {
            // figure out what type whether it is a relative rating
            foreach (XmlNode currentChild in nodeRepresentation.ChildNodes)
            {
                if (currentChild.Name == this.BetterRatingTag || currentChild.Name == this.WorseRatingTag)
                {
                    return this.ReadRelativeRating(nodeRepresentation);
                }
            }
            return this.ReadAbsoluteRating(nodeRepresentation);
        }
        private AbsoluteRating ReadAbsoluteRating(XmlNode nodeRepresentation)
        {
            AbsoluteRating rating = new AbsoluteRating();
            foreach (XmlNode currentChild in nodeRepresentation.ChildNodes)
            {
                if (currentChild.Name == this.RatingDateTag)
                {
                    rating.Date = this.ReadDate(currentChild);
                    continue;
                }
                if (currentChild.Name == this.ActivityDescriptorTag)
                {
                    rating.ActivityDescriptor = this.ReadActivityDescriptor(currentChild);
                    continue;
                }
                if (currentChild.Name == this.RatingScoreTag)
                {
                    rating.Score = this.ReadDouble(currentChild);
                    continue;
                }
                if (currentChild.Name == this.RatingFromUserTag)
                {
                    rating.FromUser = this.ReadBool(currentChild);
                    continue;
                }
                /*if (currentChild.Name == this.RatingSourceTag)
                {
                    rating.Source = this.ReadRatingSource(currentChild);
                    continue;
                }*/
            }
            return rating;
        }
        private RelativeRating ReadRelativeRating(XmlNode nodeRepresentation)
        {
            RelativeRating rating = new RelativeRating();
            foreach (XmlNode currentChild in nodeRepresentation.ChildNodes)
            {
                if (currentChild.Name == this.BetterRatingTag)
                {
                    rating.BetterRating = this.ReadAbsoluteRating(currentChild);
                    continue;
                }
                if (currentChild.Name == this.WorseRatingTag)
                {
                    rating.WorseRating = this.ReadAbsoluteRating(currentChild);
                    continue;
                }
                if (currentChild.Name == this.RelativeRatingScaleTag)
                {
                    rating.RawScoreScale = this.ReadDouble(currentChild);
                    continue;
                }
            }
            return rating;
        }
        private ActivityDescriptor ReadActivityDescriptor(XmlNode nodeRepresentation)
        {
            ActivityDescriptor descriptor = new ActivityDescriptor();
            foreach (XmlNode currentChild in nodeRepresentation.ChildNodes)
            {
                if (currentChild.Name == this.ActivityNameTag)
                {
                    descriptor.ActivityName = this.ReadText(currentChild);
                    continue;
                }
                if (currentChild.Name == this.ChoosableTag)
                {
                    descriptor.Choosable = this.ReadBool(currentChild);
                    continue;
                }
            }
            return descriptor;
        }
        /*private RatingSource ReadRatingSource(XmlNode nodeRepresentation)
        {
            string text = this.ReadText(nodeRepresentation);
            RatingSource source = RatingSource.GetSourceWithDescription(text);
            return source;
        }*/
        private double ReadDouble(XmlNode nodeRepresentation)
        {
            string text = this.ReadText(nodeRepresentation);
            return Double.Parse(text);
        }
        private int ReadInt(XmlNode nodeRepresentation)
        {
            return (int)this.ReadDouble(nodeRepresentation);
        }
        private RecentUserData ReadRecentUserData(XmlNode nodeRepresentation)
        {
            RecentUserData data = new RecentUserData();
            foreach (XmlNode currentChild in nodeRepresentation.ChildNodes)
            {
                if (currentChild.Name == this.DateTag)
                {
                    data.LatestActionDate = this.ReadDate(currentChild);
                    continue;
                }
                if (currentChild.Name == this.SuggestionsTag)
                {
                    data.Suggestions = this.ReadSuggestions(currentChild);
                    continue;
                }
            }
            return data;
        }
        private LinkedList<ActivitySuggestion> ReadSuggestions(XmlNode nodeRepresentation)
        {
            LinkedList<ActivitySuggestion> suggestions = new LinkedList<ActivitySuggestion>();
            foreach (XmlNode currentChild in nodeRepresentation.ChildNodes)
            {
                ActivitySuggestion suggestion = this.ReadSuggestion(currentChild);
                suggestions.AddLast(suggestion);
            }
            return suggestions;
        }
        private ActivitySuggestion ReadSuggestion(XmlNode nodeRepresentation)
        {
            ActivityDescriptor descriptor = null;
            DateTime startDate = new DateTime();
            DateTime? endDate = null;
            DateTime? createdDate = null;
            bool skippable = true;
            foreach (XmlNode currentChild in nodeRepresentation.ChildNodes)
            {
                if (currentChild.Name == this.ActivityDescriptorTag)
                {
                    descriptor = this.ReadActivityDescriptor(currentChild);
                    continue;
                }
                if (currentChild.Name == this.SuggestionCreationDate)
                {
                    createdDate = this.ReadDate(currentChild);
                    continue;
                }
                if (currentChild.Name == this.SuggestionStartDateTag)
                {
                    startDate = this.ReadDate(currentChild);
                    continue;
                }
                if (currentChild.Name == this.SuggestionEndDateTag)
                {
                    endDate = this.ReadDate(currentChild);
                    continue;
                }
                if (currentChild.Name == this.SkippableTag)
                {
                    skippable = this.ReadBool(currentChild);
                    continue;
                }
            }
            ActivitySuggestion suggestion = new ActivitySuggestion(descriptor);
            if (createdDate == null)
                createdDate = startDate;
            suggestion.CreatedDate = createdDate.Value;
            suggestion.StartDate = startDate;
            suggestion.EndDate = endDate;
            suggestion.Skippable = skippable;
            return suggestion;
        }

        public PlannedExperiment ReadExperiment(XmlNode nodeRepresentation)
        {
            PlannedExperiment experiment = new PlannedExperiment();
            foreach (XmlNode currentChild in nodeRepresentation.ChildNodes)
            {
                if (currentChild.Name == this.EarlierSuggestionInExperimentTag)
                {
                    experiment.Earlier = this.ReadExperimentSuggestion(currentChild);
                    continue;
                }
                if (currentChild.Name == this.LaterSuggestionInExperimentTag)
                {
                    experiment.Later = this.ReadExperimentSuggestion(currentChild);
                    continue;
                }
            }
            return experiment;
        }

        public PlannedMetric ReadExperimentSuggestion(XmlNode nodeRepresentation)
        {
            PlannedMetric metric = new PlannedMetric();
            foreach (XmlNode currentChild in nodeRepresentation.ChildNodes)
            {
                if (currentChild.Name == this.ActivityDescriptorTag)
                {
                    metric.ActivityDescriptor = this.ReadActivityDescriptor(currentChild);
                    continue;
                }
                if (currentChild.Name == this.MetricTag)
                {
                    metric.MetricName = currentChild.Value;
                    continue;
                }
                if (currentChild.Name == this.SuccessRateTag)
                {
                    metric.DifficultyEstimate.EstimatedSuccessesPerSecond = this.ReadDouble(currentChild);
                    continue;
                }
                if (currentChild.Name == this.NumEasierParticipationsTag)
                {
                    metric.DifficultyEstimate.NumEasiers = this.ReadInt(currentChild);
                    continue;
                }
                if (currentChild.Name == this.NumHarderParticipationsTag)
                {
                    metric.DifficultyEstimate.NumHarders = this.ReadInt(currentChild);
                    continue;
                }
            }
            return metric;
        }

        public Metric ReadMetric(XmlNode nodeRepresentation)
        {
            string metricName = null;
            ActivityDescriptor activityDescriptor = null;
            foreach (XmlNode currentChild in nodeRepresentation.ChildNodes)
            {
                if (currentChild.Name == this.MetricName_Tag)
                {
                    metricName = this.ReadText(currentChild);
                    continue;
                }
                if (currentChild.Name == this.ActivityDescriptorTag)
                {
                    activityDescriptor = this.ReadActivityDescriptor(currentChild);
                    continue;
                }
            }
            Activity activity = this.activityDatabase.ResolveDescriptor(activityDescriptor);
            Metric metric = new CompletionMetric(metricName, activity);
            return metric;
        }

        public PersistentUserData ParseForImport(string contents)
        {
            IEnumerable<XmlNode> nodes = this.ParseToXmlNodes(contents);

            List<string> inheritanceTexts = new List<string>();
            List<string> historyTexts = new List<string>();
            string recentUserDataText = "";


            foreach (XmlNode node in nodes)
            {
                if (node.Name == this.RecentUserDataTag)
                {
                    RecentUserData recentUserData = this.ReadRecentUserData(node);
                    recentUserDataText = this.ConvertToString(recentUserData);
                    continue;
                }

                if (node.Name == this.InheritanceTag)
                {
                    Inheritance inheritance = this.ProcessInheritance(node);
                    inheritanceTexts.Add(this.ConvertToString(inheritance));
                    continue;
                }
                if (node.Name == this.CategoryTag)
                {
                    ActivityDescriptor activityDescriptor = this.ReadActivityDescriptor(node);
                    Activity activity = this.activityDatabase.GetActivityOrCreateCategory(activityDescriptor);
                    inheritanceTexts.Add(this.ConvertToString(activity));
                    continue;
                }
                if (node.Name == this.TodoTag)
                {
                    ActivityDescriptor activityDescriptor = this.ReadActivityDescriptor(node);
                    Activity activity = this.activityDatabase.GetOrCreateTodo(activityDescriptor);
                    inheritanceTexts.Add(this.ConvertToString(activity));
                    continue;
                }
                if (node.Name == this.MetricTag)
                {
                    Metric metric = this.ProcessMetric(node);
                    inheritanceTexts.Add(this.ConvertToString(metric));
                    continue;
                }

                if (node.Name == this.ParticipationTag)
                {
                    Participation participation = this.ReadParticipation(node);
                    historyTexts.Add(this.ConvertToString(participation));
                    continue;
                }
                if (node.Name == this.SkipTag)
                {
                    ActivitySkip skip = this.ReadSkip(node);
                    historyTexts.Add(this.ConvertToString(skip));
                    continue;
                }
                if (node.Name == this.ActivityRequestTag)
                {
                    ActivityRequest request = this.ReadActivityRequest(node);
                    historyTexts.Add(this.ConvertToString(request));
                    continue;
                }
                if (node.Name == this.SuggestionTag)
                {
                    ActivitySuggestion suggestion = this.ReadSuggestion(node);
                    historyTexts.Add(this.ConvertToString(suggestion));
                    continue;
                }
                if (node.Name == this.ExperimentTag)
                {
                    PlannedExperiment experiment = this.ReadExperiment(node);
                    historyTexts.Add(this.ConvertToString(experiment));
                    continue;
                }
                throw new InvalidDataException("Unrecognized node: <" + node.Name + ">");
            }

            PersistentUserData result = new PersistentUserData();

            result.InheritancesText = String.Join("\n", inheritanceTexts);
            result.HistoryText = string.Join("\n", historyTexts);
            result.RecentUserDataText = recentUserDataText;

            return result;
        }

        // converts the dictionary into a string, without adding the initial <Tag> or final </Tag>
        private string ConvertToStringBody(Dictionary<string, string> properties)
        {
            string result = "";
            foreach (string key in properties.Keys)
            {
                string value = properties[key];
                result += this.ConvertToString(value, key);
            }
            return result;
        }
        // creates a string that represents the Activity Descriptor, without adding the initial <Tag> or final </Tag>
        private string ConvertToStringBody(ActivityDescriptor descriptor)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();
            string activityName = descriptor.ActivityName;
            if (activityName != null)
            {
                properties[this.ActivityNameTag] = this.XmlEscape(activityName);
            }
            return this.ConvertToStringBody(properties);
        }
        // converts the DateTime into a string, and doesn't add the initial <Tag> or ending </Tag>
        private string ConvertToStringBody(DateTime? when)
        {
            if (when == null)
                return null;
            string result = this.ConvertToStringBody((DateTime)when);
            return result;
        }
        // converts the DateTime into a string, and doesn't add the initial <Tag> or ending </Tag>
        private string ConvertToStringBody(DateTime when)
        {
            string result = when.ToString("yyyy-MM-ddTHH:mm:ss");
            return result;
        }
        // converts the double into a string, and doesn't add the initial <Tag> or ending </Tag>
        private string ConvertToStringBody(double value)
        {
            return value.ToString();
        }
        // converts the bool into a string, and doesn't add the initial <Tag> or ending </Tag>
        private string ConvertToStringBody(bool value)
        {
            return value.ToString();
        }
        // converts the rating to a string based on its type, and doesn't add the initial <Tag> or ending </Tag>
        private string ConvertToStringBody(Rating rating)
        {
            // figure out what type it is and convert it accordingly
            if (rating is AbsoluteRating)
                return this.ConvertToStringBody((AbsoluteRating)rating);
            if (rating is RelativeRating)
                return this.ConvertToStringBody((RelativeRating)rating);
            return null;
        }
        // converts the RelativeRating to a string, and doesn't add the initial <Tag> or ending </Tag>
        private string ConvertToStringBody(RelativeRating rating)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();

            properties[this.BetterRatingTag] = this.ConvertToStringBody(rating.BetterRating);
            properties[this.WorseRatingTag] = this.ConvertToStringBody(rating.WorseRating);
            if (rating.RawScoreScale != null)
                properties[this.RelativeRatingScaleTag] = this.ConvertToStringBody((double)rating.RawScoreScale);

            return this.ConvertToStringBody(properties);
        }
        // converts the AbsoluteRating into a string, and doesn't add the inital <Tag> or ending </Tag>
        private string ConvertToStringBody(AbsoluteRating rating)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();
            if (rating.ActivityDescriptor != null)
                properties[this.ActivityDescriptorTag] = this.ConvertToStringBody(rating.ActivityDescriptor);
            if (rating.Date != null)
                properties[this.RatingDateTag] = this.ConvertToStringBody(rating.Date);
            properties[this.RatingScoreTag] = this.ConvertToStringBody(rating.Score);
            if (!rating.FromUser)
                properties[this.RatingFromUserTag] = this.ConvertToStringBody(rating.FromUser);

            return this.ConvertToStringBody(properties);
        }
        // converts the ActivitySuggestion into a string, and doesn't add the inital <Tag> or ending </Tag>
        private string ConvertToStringBody(Consideration consideration)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();
            properties[this.ActivityDescriptorTag] = this.ConvertToStringBody(consideration.ActivityDescriptor);

            return this.ConvertToStringBody(properties);
        }
        // converts the list of suggestions into a string without the initial <Tag> or ending </Tag>
        private string ConvertToStringBody(IEnumerable<ActivitySuggestion> suggestions)
        {
            string result = "";
            foreach (ActivitySuggestion suggestion in suggestions)
            {
                result += this.ConvertToString(suggestion);
            }
            return result;
        }

        private string ConvertToStringBody(RelativeEfficiencyMeasurement measurement)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();

            bool includeParticipationDescription = (measurement.Earlier == null);
            if (includeParticipationDescription)
            {
                properties[this.ActivityDescriptorTag] = this.ConvertToStringBody(measurement.ActivityDescriptor);
                properties[this.ParticipationStartDateTag] = this.ConvertToStringBody(measurement.StartDate);
                properties[this.ParticipationEndDateTag] = this.ConvertToStringBody(measurement.EndDate);
            }
            properties[this.EfficiencyValue_Tag] = this.ConvertToStringBody(measurement.RecomputedEfficiency.Mean);
            properties[this.EfficiencyWeight_Tag] = this.ConvertToStringBody(measurement.RecomputedEfficiency.Weight);
            if (measurement.Earlier != null)
                properties[this.EarlierEfficency_Tag] = this.ConvertToStringBody(measurement.Earlier);

            return this.ConvertToStringBody(properties);
        }

        private string XmlEscape(String input)
        {
            return input.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }

        #endregion

        #region Special String Identifiers
        public string ActivityDescriptorTag
        {
            get
            {
                return "Activity";
            }
        }
        public string ActivityNameTag
        {
            get
            {
                return "Name";
            }
        }
        public string CategoryTag
        {
            get
            {
                return "Category";
            }
        }
        public string TodoTag
        {
            get
            {
                return "ToDo";
            }
        }
        public string ChoosableTag
        {
            get
            {
                return "Choose";
            }
        }
        public string DiscoveryDateTag
        {
            get
            {
                return "DiscoveryDate";
            }
        }
        private string DateTag
        {
            get
            {
                return "Date";
            }
        }

        public string RatingTag
        {
            get
            {
                return "Rating";
            }
        }
        public string RatingDateTag
        {
            get
            {
                return "Date";
            }
        }
        public string RatingScoreTag
        {
            get
            {
                return "Score";
            }
        }
        public string RatingFromUserTag
        {
            get
            {
                // tells whether the user entered this rating (rather than it being an estimate created by the engine)
                return "FromUser";
            }
        }
        public string RatingSourceTag
        {
            get
            {
                return "Source";
            }
        }

        public string BetterRatingTag
        {
            get
            {
                return "BetterRating";
            }
        }
        public string WorseRatingTag
        {
            get
            {
                return "WorseRating";
            }
        }
        public string RelativeRatingScaleTag
        {
            get
            {
                return "scale";
            }
        }


        private string ParticipationTag
        {
            get
            {
                return "Participation";
            }
        }
        private string ParticipationStartDateTag
        {
            get
            {
                return "StartDate";
            }
        }
        private string ParticipationEndDateTag
        {
            get
            {
                return "EndDate";
            }
        }
        private string WasSuggestedTag
        {
            get
            {
                return "Suggested";
            }
        }
        private string ConsiderationTag
        {
            get
            {
                return "Consideration";
            }
        }
        private string ParticipationSuccessful_Tag
        {
            get
            {
                return "Successful";
            }
        }
        private string EfficiencyMeasurement_Tag
        {
            get
            {
                return "Efficiency";
            }
        }
        public string EfficiencyValue_Tag
        {
            get
            {
                return "Value";
            }
        }
        public string EfficiencyWeight_Tag
        {
            get
            {
                return "Weight";
            }
        }

        public string EarlierEfficency_Tag
        {
            get
            {
                return "Earlier";
            }
        }

        private string InheritanceTag
        {
            get
            {
                return "Inheritance";
            }
        }
        private string InheritanceParentTag
        {
            get
            {
                return "Parent";
            }
        }
        private string InheritanceChildTag
        {
            get
            {
                return "Child";
            }
        }

        private string SkipTag
        {
            get
            {
                return "Skip";
            }
        }
        private string SkipConsideredSinceDate
        {
            get
            {
                return "Since";
            }
        }
        private string SuggestionCreationDate
        {
            get
            {
                return "SuggestionDate";
            }
        }
        private string SuggestionStartDateTag
        {
            get
            {
                return "StartDate";
            }
        }
        private string SuggestionEndDateTag
        {
            get
            {
                return "EndDate";
            }
        }

        private string ActivityRequestTag
        {
            get
            {
                return "Request";
            }
        }

        private string ActivityToBeat_Tag
        {
            get
            {
                return "Beat";
            }
        }

        private string CommentTag
        {
            get
            {
                return "Comment";
            }
        }

        private string SuggestionTag
        {
            get
            {
                return "Suggestion";
            }
        }
        private string SkippableTag
        {
            get
            {
                return "Skippable";
            }
        }
        private string SuggestionsTag
        {
            get
            {
                return "Suggestions";
            }
        }
        private string RecentUserDataTag
        {
            get
            {
                return "RecentData";
            }
        }
        private string MetricTag
        {
            get
            {
                return "Metric";
            }
        }
        private string MetricName_Tag
        {
            get
            {
                return "Name";
            }
        }
        private string SuccessRateTag
        {
            get
            {
                return "SuccessRate";
            }
        }
        private string NumEasierParticipationsTag
        {
            get
            {
                return "Easiers";
            }
        }
        private string NumHarderParticipationsTag
        {
            get
            {
                return "Harders";
            }
        }
        private string ExperimentTag
        {
            get
            {
                return "Experiment";
            }
        }
        private string EarlierSuggestionInExperimentTag
        {
            get
            {
                return "Earlier";
            }
        }
        private string LaterSuggestionInExperimentTag
        {
            get
            {
                return "Later";
            }
        }
        #endregion

        #region Private Member Variables

        private HistoryReplayer listener;
        private Participation latestParticipationRead;
        private ActivitySkip pendingSkip;
        private InternalFileIo internalFileIo = new InternalFileIo();
        private PublicFileIo publicFileIo = new PublicFileIo();
        private ActivityDatabase activityDatabase;

        #endregion
    }

    class InheritancesParser
    {
        public static List<Inheritance> Parse(string text)
        {
            return new InheritancesParser().parse(text);
        }

        private void ActivityDatabase_InheritanceAdded(Inheritance inheritance)
        {
            this.inheritances.Add(inheritance);
        }

        private List<Inheritance> parse(string text)
        {
            ActivityDatabase activityDatabase = new ActivityDatabase(null, null);
            activityDatabase.InheritanceAdded += ActivityDatabase_InheritanceAdded;
            TextConverter impl = new TextConverter(null, activityDatabase);
            impl.ReadText(text);

            return this.inheritances;
        }

        private List<Inheritance> inheritances = new List<Inheritance>();
    }

    public class PersistentUserData
    {
        public String InheritancesText;
        public String HistoryText;
        public String RecentUserDataText;

        public String serialize()
        {
            return this.RecentUserDataText + "\n" + this.InheritancesText + "\n" + this.HistoryText;
        }
    }
}
