using ActivityRecommendation.Effectiveness;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

// The TextConverter class will convert an object into a string or parse a string into a list of objects
// It only works on the specific types of objects that matter in the ActivityRecommender project
namespace ActivityRecommendation
{
    public class TextConverter
    {
        #region Constructor

        private Dictionary<string, ActivityDescriptor> activityDescriptors = new Dictionary<string, ActivityDescriptor>();

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
            Rating rating = participation.CompressedRating;
            if (rating != null)
                properties[this.RatingTag] = this.ConvertToStringBody(rating);
            string comment = participation.Comment;
            if (participation.Suggested)
                properties[this.WasSuggestedTag] = this.ConvertToStringBody(participation.Suggested);
            if (comment != null)
                properties[this.ParticipationEmbeddedComment_Tag] = this.XmlEscape(comment);
            if (participation.Consideration != null)
                properties[this.ConsiderationTag] = this.ConvertToStringBody(participation.Consideration);
            
            // success/failure
            Activity activity = this.activityDatabase.ResolveDescriptor(participation.ActivityDescriptor);
            if (participation.EffectivenessMeasurement != null)
            {
                bool successful = participation.CompletedMetric; // whether this participation succeeded
                bool recordedMetric = false; // whether we recorded a metric for this participation
                bool dismissed = participation.DismissedActivity; // whether this participation dismissed its activity
                if (activity.DefaultMetric == null)
                {
                    recordedMetric = true;
                    // If this activity doesn't have a clear default metric, then we have to record which metric was used
                    properties[this.Participation_MetricName_Tag] = participation.EffectivenessMeasurement.Metric.Name;
                }
                if (successful)
                {
                    // if this participation was successful, record that
                    properties[this.ParticipationSuccessful_Tag] = this.ConvertToStringBody(true);
                    if (participation.HelpFraction > 0)
                        properties[this.HelpFraction_Tag] = this.ConvertToStringBody(participation.HelpFraction);
                }
                else
                {
                    if (dismissed)
                    {
                        // if this participation dismissed its activity, record that
                        properties[this.DismissedActivity_Tag] = this.ConvertToStringBody(true);
                    }
                    else
                    {
                        // if this participation failed without dismissing the activity, then decide how to record that
                        if (recordedMetric)
                        {
                            // If we recorded a metric, then it's already clear that the user tried to succeed
                            // So, we don't need to explicitly record failure
                        }
                        else
                        {
                            if (this.shouldNullMetricAndNullSuccessBeInterpretedAsFailure(activity))
                            {
                                // We didn't record a metric, but we don't need to record a success either because it will still be counted as failure
                            }
                            else
                            {
                                // We didn't record a metric, so we have to record a failure or else it would be treated as having made no attempt
                                properties[this.ParticipationSuccessful_Tag] = this.ConvertToStringBody(false);
                            }
                        }
                    }
                }
                if (participation.RelativeEfficiencyMeasurement != null)
                    properties[this.EfficiencyMeasurement_Tag] = this.ConvertToStringBody(participation.RelativeEfficiencyMeasurement);
            }
            else
            {
                // This Participation didn't attempt to complete a Metric
                if (this.shouldNullMetricAndNullSuccessBeInterpretedAsFailure(activity))
                {
                    // If we don't record a success or failure status for this Participation, then that would be interpreted as a failure
                    // So, we explicitly record that it was unattempted
                    properties[this.Participation_MetricName_Tag] = "";
                }

            }

            return this.ConvertToString(properties, objectName);
        }
        public string ConvertToString(ParticipationComment comment)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();
            properties[this.TopLevelComment_Text_Tag] = this.XmlEscape(comment.Text);
            properties[this.ActivityDescriptorTag] = this.ConvertToStringBody(comment.ActivityDescriptor);
            properties[this.TopLevelComment_Applicable_Tag] = this.ConvertToStringBody(comment.ApplicableDate);
            properties[this.TopLevelComment_CreatedDate_Tag] = this.ConvertToStringBody(comment.CreatedDate);

            return this.ConvertToString(properties, this.ParticipationEmbeddedComment_Tag);
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
            Problem problem = activity as Problem;
            if (problem != null)
                return this.ConvertToString(problem);
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
        public string ConvertToString(Problem problem)
        {
            string body = this.ConvertToStringBody(problem.MakeDescriptor());
            return this.ConvertToString(body, this.ProblemTag);
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
            if (request.UserPredictedRating != null)
                properties[this.UserPredictedRating_Tag] = this.ConvertToStringBody(request.UserPredictedRating);
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
                properties[this.RecentUserData_MultipleSuggestions_Tag] = this.ConvertToStringBody(data.Suggestions);
            if (data.NumRecent_UserChosen_ExperimentSuggestions != 0)
                properties[this.NumRecent_UserChosen_ExperimentSuggestions_Tag] = this.ConvertToStringBody(data.NumRecent_UserChosen_ExperimentSuggestions);
            if (data.DemandedMetricName != "")
                properties[this.MetricName_Tag] = data.DemandedMetricName;

            return this.ConvertToString(properties, objectName);
        }
        public string ConvertToString(ProtoActivity_Database database, bool forSharing)
        {
            List<String> components = new List<string>();
            IEnumerable<ProtoActivity> protoactivities;
            if (forSharing)
                protoactivities = database.SortedByDecreasingInterest();
            else
                protoactivities = database.ProtoActivities;
            foreach (ProtoActivity protoActivity in protoactivities)
            {
                if (protoActivity.Text != null && protoActivity.Text != "")
                {
                    components.Add(this.ConvertToString(protoActivity));
                    if (forSharing)
                        components.Add("\n\n\n");
                }
            }
            return string.Join(Environment.NewLine, components);
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

            properties[this.ActivityDescriptorsTag] = this.ConvertToStringBody(skip.ActivityDescriptors);
            properties[this.DateTag] = this.ConvertToStringBody(skip.CreationDate);
            properties[this.SuggestionCreationDate] = this.ConvertToStringBody(skip.SuggestionCreationDate);
            if (skip.SuggestionStartDate != skip.SuggestionCreationDate)
                properties[this.SuggestionStartDateTag] = this.ConvertToStringBody(skip.SuggestionStartDate);
            if (skip.ConsideredSinceDate != skip.SuggestionCreationDate)
                properties[this.SkipConsideredSinceDate] = this.ConvertToStringBody(skip.ConsideredSinceDate);

            return this.ConvertToString(properties, objectName);
        }
        public string ConvertToString(ActivitiesSuggestion activitySuggestion)
        {
            string body = this.ConvertToStringBody(activitySuggestion);
            return this.ConvertToString(body, this.ParallelSuggestions_Tag);
        }
        public string ConvertToStringBody(ActivitiesSuggestion activitySuggestion)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();

            //properties[this.ActivityDescriptorsTag] = this.ConvertToStringBody(activitySuggestion.ActivityDescriptors);

            string startDate_text = this.ConvertToStringBody(activitySuggestion.StartDate);
            if (activitySuggestion.CreatedDate != null)
            {
                // If the suggestion's creation DateTime matches its StartDate, then we don't need to record its creation DateTime
                // In case the two different DateTime's differ by a negligible amount, we compare the serialized text instead of the dates
                string createdDate_text = this.ConvertToStringBody(activitySuggestion.CreatedDate);
                if (createdDate_text != startDate_text)
                    properties[this.SuggestionCreationDate] = createdDate_text;
            }
            properties[this.SuggestionStartDateTag] = startDate_text;

            List<string> childComponents = new List<string>();
            foreach (ActivitySuggestion child in activitySuggestion.Children)
            {
                Dictionary<string, string> childProperties = new Dictionary<string, string>();
                childProperties[this.ActivityDescriptorTag] = this.ConvertToStringBody(child.ActivityDescriptor);
                if (child.EndDate != null)
                    childProperties[this.SuggestionEndDateTag] = this.ConvertToStringBody(child.EndDate);
                childComponents.Add(this.ConvertToString(childProperties, this.ChildSuggestion_Tag));
            }
            properties[this.ChildSuggestions_Tag] = string.Join("", childComponents);

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
            if (activity.DefaultMetric == null || experiment.MetricName != activity.DefaultMetric.Name)
                properties[this.MetricTag] = this.XmlEscape(experiment.MetricName);

            // difficulty estimate from user in the form of <NumEasiers> and <NumHarders>
            if (experiment.DifficultyEstimate.NumEasiers > 0)
                properties[this.NumEasierParticipationsTag] = this.ConvertToStringBody(experiment.DifficultyEstimate.NumEasiers);
            if (experiment.DifficultyEstimate.NumHarders > 0)
                properties[this.NumHarderParticipationsTag] = this.ConvertToStringBody(experiment.DifficultyEstimate.NumHarders);
            // difficulty estimate from the user as a ratio
            properties[this.UserEstimated_SuccessRate_Tag] = this.ConvertToStringBody(experiment.DifficultyEstimate.EstimatedRelativeSuccessRate_FromUser);
            // calculated difficulty estimate
            properties[this.SuccessRateTag] = this.ConvertToStringBody(experiment.DifficultyEstimate.EstimatedSuccessesPerSecond);

            return this.ConvertToStringBody(properties);
        }
        public string ConvertToString(Metric metric)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();
            properties[this.MetricName_Tag] = this.XmlEscape(metric.Name);
            properties[this.ActivityDescriptorTag] = this.ConvertToStringBody(metric.ActivityDescriptor);
            if (metric.DiscoveryDate != null)
                properties[this.DiscoveryDateTag] = this.ConvertToStringBody(metric.DiscoveryDate);

            return this.ConvertToString(properties, this.MetricTag);
        }
        public string ConvertToString(ProtoActivity protoActivity)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();
            properties[this.ProtoActivity_Text_Tag] = this.XmlEscape(protoActivity.Text);
            properties[this.ProtoActivity_Ratings_Tag] = this.ConvertToStringBody(protoActivity.Ratings);
            properties[this.ProtoActivity_LastInteracted_Tag] = this.ConvertToStringBody(protoActivity.LastInteractedWith);

            return this.ConvertToString(properties, this.ProtoActivity_Tag);
        }
        public string ConvertToString(Persona persona)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();
            properties[this.PersonaName_Tag] = persona.Name;
            properties[this.PersonaAppearance_Tag] = persona.LayoutDefaults_Name;

            return this.ConvertToString(properties, this.PersonaTag);
        }


        public string ConvertToStringBody(Distribution distribution)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();
            properties[this.DistributionMean_Tag] = this.ConvertToStringBody(distribution.Mean);
            properties[this.DistributionStdDev_Tag] = this.ConvertToStringBody(distribution.StdDev);
            properties[this.DistributionWeight_Tag] = this.ConvertToStringBody(distribution.Weight);

            return this.ConvertToStringBody(properties);
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

        private XmlDocument ParseToXmlNodes(TextReader text)
        {
            //text = "<root>" + text + "</root>";
            XmlDocument document = new XmlDocument(text);
            return document;
        }

        // converts the given text into a sequence of objects and sends them to the Engine
        public void ProcessText(TextReader text)
        {
            XmlDocument nodes = this.ParseToXmlNodes(text);
            while (true)
            {
                XmlNode node = nodes.Next();
                if (node == null)
                    break;

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
                if (node.Name == this.ProblemTag)
                {
                    this.ProcessProblem(node);
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
                if (node.Name == this.ParallelSuggestions_Tag)
                {
                    this.ProcessSuggestions(node);
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
                if (node.Name == this.ProtoActivity_Tag)
                {
                    this.Process_ProtoActivity(node);
                    continue;
                }
                if (node.Name == this.PersonaTag)
                {
                    this.ProcessPersona(node);
                    continue;
                }
                if (node.Name == this.TopLevelComment_Tag)
                {
                    this.ProcessComment(node);
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
        private void ProcessProblem(XmlNode nodeRepresentation)
        {
            // the Todo just puts all of the fields of the ActivityDescriptor at the top level
            ActivityDescriptor activityDescriptor = this.ReadActivityDescriptor(nodeRepresentation);
            Problem todo = this.activityDatabase.CreateProblem(activityDescriptor);
            this.listener.PostProblem(todo);
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
        private void ProcessComment(XmlNode nodeRepresentation)
        {
            ParticipationComment comment = this.ReadTopLevelComment(nodeRepresentation);
            this.listener.AddComment(comment);
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
            this.listener.AddSuggestion(new ActivitiesSuggestion(suggestion));
        }
        private void ProcessSuggestions(XmlNode nodeRepresentation)
        {
            ActivitiesSuggestion suggestion = this.ReadParallelSuggestions(nodeRepresentation);
            if (suggestion.Children.Count > 0)
            {
                this.setPendingSkip(null, suggestion.CreatedDate);
                this.listener.AddSuggestion(suggestion);
            }
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
            activity.AddIntrinsicMetric(metric);
            if (this.listener != null)
                this.listener.AddMetric(metric);

            return metric;
        }
        private void Process_ProtoActivity(XmlNode nodeRepresentation)
        {
            ProtoActivity protoActivity = this.Read_ProtoActivity(nodeRepresentation);
            if (this.listener != null)
                this.listener.Add_ProtoActivity(protoActivity);
        }
        private void ProcessPersona(XmlNode nodeRepresentation)
        {
            Persona persona = this.ReadPersona(nodeRepresentation);
            if (this.listener != null)
                this.listener.SetPersona(persona);
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
            ActivityDescriptor activityDescriptor = null;
            DateTime startDate = DateTime.Now;
            DateTime endDate = startDate;
            string comment = null;
            bool suggested = false;
            bool? successful = null;
            bool dismissedActivity = false;
            string metricName = null;
            RelativeEfficiencyMeasurement efficiencyMeasurement = null;
            double helpFraction = 0;
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
                if (currentChild.Name == this.ParticipationEmbeddedComment_Tag)
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
                if (currentChild.Name == HelpFraction_Tag)
                {
                    helpFraction = this.ReadDouble(currentChild);
                    continue;
                }
                if (currentChild.Name == this.DismissedActivity_Tag)
                {
                    dismissedActivity = this.ReadBool(currentChild);
                    continue;
                }
                if (currentChild.Name == this.EfficiencyMeasurement_Tag)
                {
                    efficiencyMeasurement = this.ReadEfficiencyMeasurement(currentChild);
                    continue;
                }
                if (currentChild.Name == this.Participation_MetricName_Tag)
                {
                    metricName = this.ReadText(currentChild);
                    continue;
                }
            }
            if (activityDescriptor == null)
            {
                throw new InvalidDataException("No activity descriptor specified!");
            }
            Participation currentParticipation = new Participation(startDate, endDate, activityDescriptor);
            currentParticipation.Rating = rating;
            if (rating != null)
            {
                // In case it was a relative rating, give the rating a chance to keep a pointer to the previous participation
                RelativeRating convertedRating = rating as RelativeRating;
                if (convertedRating != null)
                {
                    convertedRating.AttemptToMatch(this.latestParticipationRead);
                }
            }
            currentParticipation.Comment = comment;
            currentParticipation.Suggested = suggested;

            // Fill the success/failure status for this participation
            Activity activity = this.activityDatabase.ResolveDescriptor(activityDescriptor);

            if (metricName == null && successful == null)
            {
                // in some cases we treat null metric and null success status as failure
                if (this.shouldNullMetricAndNullSuccessBeInterpretedAsFailure(activity))
                {
                    successful = false;
                }
            }
            if (dismissedActivity && successful == null)
            {
                // if this participation dismissed the activity and didn't specify a success/failure status, then it failed
                successful = false;
            }
            if (metricName != null && metricName != "" && successful == null)
            {
                // if a metric was specified and no success status was specified, that's failure
                successful = false;
            }

            if (successful == true)
            {
                // a successful participation dismisses its ToDo
                dismissedActivity = true;
            }

            // if we determined a success/failure status, then make a note of it
            if (successful != null)
            {
                Metric metric;
                if (metricName == null)
                    metric = activity.DefaultMetric;
                else
                    metric = activity.MetricForName(metricName);
                if (metric == null)
                {
                    System.Diagnostics.Debug.WriteLine("Invalid metric '" + metricName + "' specified for participation '" + activityDescriptor.ActivityName + "' at " + startDate);
                }
                else
                {
                    CompletionEfficiencyMeasurement effectivenessMeasurement = new CompletionEfficiencyMeasurement(metric, successful.Value, helpFraction);
                    effectivenessMeasurement.DismissedActivity = dismissedActivity;
                    currentParticipation.EffectivenessMeasurement = effectivenessMeasurement;
                    if (efficiencyMeasurement != null)
                    {
                        efficiencyMeasurement.FillInFromParticipation(currentParticipation);
                    }
                    currentParticipation.EffectivenessMeasurement.Computation = efficiencyMeasurement;
                }
            }

            this.latestParticipationRead = currentParticipation;
            return currentParticipation;
        }

        private ParticipationComment ReadTopLevelComment(XmlNode nodeRepresentation)
        {
            string text = null;
            DateTime applicableDate = new DateTime();
            DateTime createdDate = new DateTime();
            ActivityDescriptor activity = null;
            foreach (XmlNode child in nodeRepresentation.ChildNodes)
            {
                if (child.Name == this.TopLevelComment_Text_Tag)
                {
                    text = this.ReadText(child);
                    continue;
                }
                if (child.Name == this.ActivityDescriptorTag)
                {
                    activity = this.ReadActivityDescriptor(child);
                    continue;
                }
                if (child.Name == this.TopLevelComment_Applicable_Tag)
                {
                    applicableDate = this.ReadDate(child);
                    continue;
                }
                if (child.Name == this.TopLevelComment_CreatedDate_Tag)
                {
                    createdDate = this.ReadDate(child);
                    continue;
                }
            }
            return new ParticipationComment(text, applicableDate, createdDate, activity);
        }

        // Before ActivityRecommender supported multiple metrics per activity, we did not record which metric was being completed.
        // Additionally, when a Participation failed, we recorded no success/failure status either, and left it implied that the participation failed.
        // This function tells whether a participation in <Activity> that specifies metricName == "" and success = null to be a failed participation
        private bool shouldNullMetricAndNullSuccessBeInterpretedAsFailure(Activity activity)
        {
            if (activity.DefaultMetric == null)
            {
                // If this activity doesn't have a metric that can be recognized as the default metric, then
                // if a participation specifies no metric, we cannot treat that participation as failed because
                // we won't know which metric it failed.
                return false;
            }

            if (activity.DefaultMetric.DiscoveryDate != null)
            {
                // If this activity's default metric knows its discovery date, then it's possible that:
                // 1. this participation was created before support for multiple metrics per activity was added, and
                // 2. this metric was assigned after support for multiple metrics per activity was added.
                // In this case, the participation wouldn't have known about this metric, and would not have considered null data to be failure
                return false;
            }
            // If we get here, then the activity has a default metric which was created before activities could have multiple metrics
            // There are 3 cases:
            // A. `activity` is a ToDo (a ToDo's metric doesn't record a separate DiscoveryDate)
            //    In this case, we must treat (null,null) as failure because it could be an old ToDo
            // B. `activity` is a Problem (a Problem's metric doesn't record a separate DiscoveryDate)
            //    In this case, it doesn't matter whether we treat (null,null) as failure or not as long as we're consistent
            // C. `activity`'s default metric was created before support was added for multiple metrics per activity
            //    In this case, we must treat (null,null) as failure because it could be an old Participation
            return true;
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
            List<ActivityDescriptor> activityDescriptors = new List<ActivityDescriptor>();
            DateTime? suggestionCreationDate = null;
            DateTime? consideredSinceDate = null;
            DateTime? suggestionStartDate = null;
            DateTime? skipCreationDate = null;
            AbsoluteRating rawRating = null;
            foreach (XmlNode currentChild in nodeRepresentation.ChildNodes)
            {
                if (currentChild.Name == this.ActivityDescriptorTag)
                {
                    activityDescriptors = new List<ActivityDescriptor>() { this.ReadActivityDescriptor(currentChild) };
                    continue;
                }
                if (currentChild.Name == this.ActivityDescriptorsTag)
                {
                    activityDescriptors = this.ReadActivityDescriptors(currentChild);
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
            return new ActivitySkip(activityDescriptors, suggestionCreationDate.Value, consideredSinceDate.Value, skipCreationDate.Value, suggestionStartDate.Value);
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
            string name = null;
            foreach (XmlNode currentChild in nodeRepresentation.ChildNodes)
            {
                if (currentChild.Name == this.ActivityNameTag)
                {
                    name = this.ReadText(currentChild);
                    continue;
                }
            }
            if (!this.activityDescriptors.ContainsKey(name))
            {
                ActivityDescriptor result = new ActivityDescriptor(name);
                this.activityDescriptors[name] = result;
            }
            return this.activityDescriptors[name];
        }
        private List<ActivityDescriptor> ReadActivityDescriptors(XmlNode nodeRepresentation)
        {
            List<ActivityDescriptor> children = new List<ActivityDescriptor>();
            foreach (XmlNode currentChild in nodeRepresentation.ChildNodes)
            {
                if (currentChild.Name == this.ActivityDescriptorTag)
                {
                    children.Add(this.ReadActivityDescriptor(currentChild));
                    continue;
                }
            }
            return children;
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
                if (currentChild.Name == this.RecentUserData_MultipleSuggestions_Tag)
                {
                    data.Suggestions = this.ReadRecentSuggestions(currentChild);
                    continue;
                }
                if (currentChild.Name == this.RecentUserData_Suggestions_Legacy_Tag)
                {
                    data.Suggestions = this.ReadRecentSuggestions_Legacy(currentChild);
                    continue;
                }
                if (currentChild.Name == this.NumRecent_UserChosen_ExperimentSuggestions_Tag)
                {
                    data.NumRecent_UserChosen_ExperimentSuggestions = this.ReadInt(currentChild);
                    continue;
                }
                if (currentChild.Name == this.MetricName_Tag)
                {
                    data.DemandedMetricName = this.ReadText(currentChild);
                    continue;
                }
            }
            return data;
        }
        private List<ActivitiesSuggestion> ReadRecentSuggestions(XmlNode nodeRepresentation)
        {
            List<ActivitiesSuggestion> suggestions = new List<ActivitiesSuggestion>(nodeRepresentation.ChildNodes.Count);
            foreach (XmlNode currentChild in nodeRepresentation.ChildNodes)
            {
                suggestions.Add(this.ReadParallelSuggestions(currentChild));
            }
            return suggestions;
        }
        private List<ActivitiesSuggestion> ReadRecentSuggestions_Legacy(XmlNode nodeRepresentation)
        {
            List<ActivitiesSuggestion> suggestions = new List<ActivitiesSuggestion>(nodeRepresentation.ChildNodes.Count);
            foreach (XmlNode currentChild in nodeRepresentation.ChildNodes)
            {
                suggestions.Add(new ActivitiesSuggestion(this.ReadSuggestion(currentChild)));
            }
            return suggestions;
        }
        private ActivitiesSuggestion ReadParallelSuggestions(XmlNode nodeRepresentation)
        {
            DateTime startDate = new DateTime();
            DateTime? createdDate = null;
            bool? skippable = null;
            List<ActivitySuggestion> children = new List<ActivitySuggestion>();
            foreach (XmlNode currentChild in nodeRepresentation.ChildNodes)
            {
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
                if (currentChild.Name == this.SuggestionTag)
                {
                    children.Add(this.ReadSuggestion(currentChild));
                    continue;
                }
                if (currentChild.Name == this.SkippableTag)
                {
                    skippable = this.ReadBool(currentChild);
                    continue;
                }
                if (currentChild.Name == this.ChildSuggestions_Tag)
                {
                    foreach (XmlNode grandChild in currentChild.ChildNodes)
                    {
                        children.Add(this.ReadSuggestion(grandChild));
                    }
                    continue;
                }
            }

            if (createdDate == null)
                createdDate = startDate;

            foreach (ActivitySuggestion child in children)
            {
                child.CreatedDate = createdDate.Value;
                child.StartDate = startDate;
                if (skippable != null)
                {
                    child.Skippable = skippable.Value;
                }
            }
            return new ActivitiesSuggestion(children);
        }
        private ActivitySuggestion ReadSuggestion(XmlNode nodeRepresentation)
        {
            ActivityDescriptor descriptor = null;
            DateTime? startDate = new DateTime();
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
            if (createdDate != null)
                suggestion.CreatedDate = createdDate.Value;
            if (startDate != null)
                suggestion.StartDate = startDate.Value;
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
                if (currentChild.Name == this.UserEstimated_SuccessRate_Tag)
                {
                    metric.DifficultyEstimate.EstimatedRelativeSuccessRate_FromUser = this.ReadDouble(currentChild);
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
            if (metric.MetricName == "")
            {
                Activity activity = this.activityDatabase.ResolveDescriptor(metric.ActivityDescriptor);
                metric.MetricName = activity.DefaultMetric.Name;
            }

            return metric;
        }

        public Metric ReadMetric(XmlNode nodeRepresentation)
        {
            string metricName = null;
            ActivityDescriptor activityDescriptor = null;
            DateTime? discoveryDate = null;
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
                if (currentChild.Name == this.DiscoveryDateTag)
                {
                    discoveryDate = this.ReadDate(currentChild);
                    continue;
                }
            }
            Activity activity = this.activityDatabase.ResolveDescriptor(activityDescriptor);
            Metric metric = new CompletionMetric(metricName, activity);
            metric.DiscoveryDate = discoveryDate;
            return metric;
        }

        public ProtoActivity Read_ProtoActivity(XmlNode nodeRepresentation)
        {
            string text = null;
            DateTime lastInteracted = DateTime.Now;
            Distribution ratings = null;
            foreach (XmlNode currentChild in nodeRepresentation.ChildNodes)
            {
                if (currentChild.Name == this.ProtoActivity_LastInteracted_Tag)
                {
                    lastInteracted = this.ReadDate(currentChild);
                    continue;
                }
                if (currentChild.Name == this.ProtoActivity_Ratings_Tag)
                {
                    ratings = this.ReadDistribution(currentChild);
                    continue;
                }
                if (currentChild.Name == this.ProtoActivity_Text_Tag)
                {
                    text = this.ReadText(currentChild);
                    continue;
                }
            }
            return new ProtoActivity(text, lastInteracted, ratings);
        }

        public Persona ReadPersona(XmlNode nodeRepresentation)
        {
            Persona persona = new Persona();
            foreach (XmlNode currentChild in nodeRepresentation.ChildNodes)
            {
                if (currentChild.Name == this.PersonaName_Tag)
                {
                    persona.Name = this.ReadText(currentChild);
                    continue;
                }
                if (currentChild.Name == this.PersonaAppearance_Tag)
                {
                    persona.LayoutDefaults_Name = this.ReadText(currentChild);
                    continue;
                }
            }
            return persona;
        }

        public Distribution ReadDistribution(XmlNode nodeRepresentation)
        {
            double mean = 0;
            double stddev = 0;
            double weight = 0;
            foreach (XmlNode currentChild in nodeRepresentation.ChildNodes)
            {
                if (currentChild.Name == this.DistributionMean_Tag)
                {
                    mean = this.ReadDouble(currentChild);
                    continue;
                }
                if (currentChild.Name == this.DistributionStdDev_Tag)
                {
                    stddev = this.ReadDouble(currentChild);
                    continue;
                }
                if (currentChild.Name == this.DistributionWeight_Tag)
                {
                    weight = this.ReadDouble(currentChild);
                    continue;
                }
            }
            return Distribution.MakeDistribution(mean, stddev, weight);
        }

        public PersistentUserData ParseForImport(TextReader contents)
        {
            string personaText = "";
            List<string> inheritanceTexts = new List<string>();
            List<string> historyTexts = new List<string>();
            string recentUserDataText = "";
            List<string> protoActivity_texts = new List<string>();

            XmlDocument nodes = this.ParseToXmlNodes(contents);
            while (true)
            {
                XmlNode node = nodes.Next();
                if (node == null)
                    break;

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
                if (node.Name == this.ProblemTag)
                {
                    ActivityDescriptor activityDescriptor = this.ReadActivityDescriptor(node);
                    Problem problem = this.activityDatabase.GetOrCreateProblem(activityDescriptor);
                    inheritanceTexts.Add(this.ConvertToString(problem));
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
                if (node.Name == this.TopLevelComment_Tag)
                {
                    ParticipationComment comment = this.ReadTopLevelComment(node);
                    historyTexts.Add(this.ConvertToString(comment));
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
                    ActivitiesSuggestion parent = new ActivitiesSuggestion(new List<ActivitySuggestion>(){ suggestion });
                    historyTexts.Add(this.ConvertToString(parent));
                    continue;
                }
                if (node.Name == this.ParallelSuggestions_Tag)
                {
                    ActivitiesSuggestion suggestion = this.ReadParallelSuggestions(node);
                    historyTexts.Add(this.ConvertToString(suggestion));
                    continue;
                }
                if (node.Name == this.ExperimentTag)
                {
                    PlannedExperiment experiment = this.ReadExperiment(node);
                    historyTexts.Add(this.ConvertToString(experiment));
                    continue;
                }
                if (node.Name == this.ProtoActivity_Tag)
                {
                    ProtoActivity protoActivity = this.Read_ProtoActivity(node);
                    protoActivity_texts.Add(this.ConvertToString(protoActivity));
                    continue;
                }
                if (node.Name == this.PersonaTag)
                {
                    Persona persona = this.ReadPersona(node);
                    personaText = this.ConvertToString(persona);
                    continue;
                }
                throw new InvalidDataException("Unrecognized node: <" + node.Name + ">");
            }

            PersistentUserData result = new PersistentUserData();
            result.PersonaReader = new StringReader(personaText);
            result.InheritancesReader = new StringReader(string.Join("\n", inheritanceTexts));
            result.HistoryReader = new StringReader(string.Join("\n", historyTexts));
            result.RecentUserDataReader = new StringReader(recentUserDataText);
            result.ProtoActivityReader = new StringReader(string.Join("\n", protoActivity_texts));

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
        private string ConvertToString(ActivityDescriptor descriptor)
        {
            return this.ConvertToString(this.ConvertToStringBody(descriptor), this.ActivityDescriptorTag);
        }
        private string ConvertToStringBody(List<ActivityDescriptor> activityDescriptors)
        {
            List<string> components = new List<string>();
            foreach (ActivityDescriptor descriptor in activityDescriptors)
            {
                components.Add(this.ConvertToString(descriptor));
            }
            return string.Join("", components);
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
        private string ConvertToStringBody(IEnumerable<ActivitiesSuggestion> suggestions)
        {
            string result = "";
            foreach (ActivitiesSuggestion suggestion in suggestions)
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
        public string ActivityDescriptorsTag
        {
            get
            {
                return "Activities";
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
        public string ProblemTag
        {
            get
            {
                return "Problem";
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
        private string HelpFraction_Tag
        {
            get
            {
                return "HelpFraction";
            }
        }
        private string DismissedActivity_Tag
        {
            get
            {
                return "ClosedActivity";
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
        private string UserPredictedRating_Tag
        {
            get
            {
                return "RequestPredictedRating";
            }
        }

        private string ParticipationEmbeddedComment_Tag
        {
            get
            {
                return "Comment";
            }
        }

        private string TopLevelComment_Tag
        {
            get
            {
                return "Comment";
            }
        }
        private string TopLevelComment_CreatedDate_Tag
        {
            get
            {
                return "Created";
            }
        }
        private string TopLevelComment_Applicable_Tag
        {
            get
            {
                return "Date";
            }
        }
        private string TopLevelComment_Text_Tag
        {
            get
            {
                return "Text";
            }
        }

        private string SuggestionTag
        {
            get
            {
                return "Suggestion";
            }
        }
        private string ParallelSuggestions_Tag
        {
            get
            {
                return "Suggestions";
            }
        }
        private string SkippableTag
        {
            get
            {
                return "Skippable";
            }
        }
        private string ChildSuggestions_Tag
        {
            get
            {
                return "Options";
            }
        }
        private string ChildSuggestion_Tag
        {
            get
            {
                return "Option";
            }
        }
        private string RecentUserData_MultipleSuggestions_Tag
        {
            get
            {
                return "PendingSuggestions";
            }
        }
        private string RecentUserData_Suggestions_Legacy_Tag
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
        private string Participation_MetricName_Tag
        {
            get
            {
                return "Metric";
            }
        }
        private string SuccessRateTag
        {
            get
            {
                return "SuccessRate";
            }
        }
        private string UserEstimated_SuccessRate_Tag
        {
            get
            {
                return "UserSuccessRate";
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
        private string ProtoActivity_Tag
        {
            get
            {
                return "ProtoActivity";
            }
        }
        private string ProtoActivity_Text_Tag
        {
            get
            {
                return this.ParticipationEmbeddedComment_Tag;
            }
        }
        private string ProtoActivity_LastInteracted_Tag
        {
            get
            {
                return "LastChecked";
            }
        }
        private string ProtoActivity_Ratings_Tag
        {
            get
            {
                return this.RatingTag;
            }
        }

        private string PersonaTag
        {
            get
            {
                return "Persona";
            }
        }
        private string PersonaName_Tag
        {
            get
            {
                return "Name";
            }
        }

        private string PersonaAppearance_Tag
        {
            get
            {
                return "Theme";
            }
        }

        private string DistributionMean_Tag
        {
            get
            {
                return "Mean";
            }
        }
        private string DistributionStdDev_Tag
        {
            get
            {
                return "StdDev";
            }
        }
        private string DistributionWeight_Tag
        {
            get
            {
                return "Weight";
            }
        }

        private string NumRecent_UserChosen_ExperimentSuggestions_Tag
        {
            get
            {
                return "RecentGuidedExperiments";
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
        public static List<Inheritance> Parse(TextReader text)
        {
            return new InheritancesParser().parse(text);
        }

        private void ActivityDatabase_InheritanceAdded(Inheritance inheritance)
        {
            this.inheritances.Add(inheritance);
        }

        private List<Inheritance> parse(TextReader text)
        {
            ActivityDatabase activityDatabase = new ActivityDatabase(null, null);
            activityDatabase.InheritanceAdded += ActivityDatabase_InheritanceAdded;
            TextConverter impl = new TextConverter(null, activityDatabase);
            impl.ProcessText(text);

            return this.inheritances;
        }

        private List<Inheritance> inheritances = new List<Inheritance>();
    }

    public class PersistentUserData
    {
        public TextReader InheritancesReader;
        public TextReader HistoryReader;
        public TextReader RecentUserDataReader;
        public TextReader ProtoActivityReader;
        public TextReader PersonaReader;

        private List<TextReader> readers
        {
            get
            {
                return new List<TextReader>() { this.PersonaReader, this.RecentUserDataReader, this.ProtoActivityReader, this.InheritancesReader, this.HistoryReader };
            }
        }
        public string serialize()
        {
            StringBuilder builder = new StringBuilder();
            foreach (TextReader reader in this.readers)
            {
                builder.Append(reader.ReadToEnd());
                reader.Close();
                reader.Dispose();
            }
            this.InheritancesReader = null;
            this.HistoryReader = null;
            this.RecentUserDataReader = null;
            this.ProtoActivityReader = null;
            this.PersonaReader = null;
            return builder.ToString();
        }
    }
}
