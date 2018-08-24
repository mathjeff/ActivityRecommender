using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

// The TextConverter class will convert an object into a string or parse a string into a list of objects
// It only works on the specific types of objects that matter in the ActivityRecommender project
namespace ActivityRecommendation
{
    class TextConverter
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
                properties[this.CommentTag] = comment;
            if (participation.Consideration != null)
                properties[this.ConsiderationTag] = this.ConvertToStringBody(participation.Consideration);


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

            properties[this.ActivityDescriptorTag] = this.ConvertToStringBody(request.ActivityDescriptor);
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
            properties[this.SuggestionDateTag] = this.ConvertToStringBody(skip.SuggestionCreationDate);
            if (skip.SuggestionStartDate != skip.SuggestionCreationDate)
                properties[this.SuggestionStartDateTag] = this.ConvertToStringBody(skip.SuggestionStartDate);
            if (skip.ConsideredSinceDate != skip.SuggestionCreationDate)
                properties[this.SkipConsideredSinceDate] = this.ConvertToStringBody(skip.ConsideredSinceDate);

            return this.ConvertToString(properties, objectName);
        }
        public string ConvertToString(ActivitySuggestion activitySuggestion)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();
            string objectName = this.SuggestionTag;

            properties[this.ActivityDescriptorTag] = this.ConvertToStringBody(activitySuggestion.ActivityDescriptor);
            if (activitySuggestion.CreatedDate != null)
                properties[this.SuggestionDateTag] = this.ConvertToStringBody(activitySuggestion.CreatedDate);
            properties[this.SuggestionStartDateTag] = this.ConvertToStringBody(activitySuggestion.StartDate);
            if (activitySuggestion.EndDate != null)
                properties[this.SuggestionEndDateTag] = this.ConvertToStringBody(activitySuggestion.EndDate);

            return this.ConvertToString(properties, objectName);
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

        private IEnumerable<XmlNode> ParseText(string text)
        {
            if (text.Length <= 0)
                return null;
            text = "<root>" + text + "</root>";
            XmlDocument document = new XmlDocument();
            document.LoadXml(text);
            XmlNode root = document.FirstChild;
            if (root == null)
                return null;
            return root.ChildNodes;
        }

        // opens the file, converts it into a sequence of objects, and sends them to the Engine
        public void ReadFile(string fileName)
        {
            StreamReader reader = this.internalFileIo.OpenFileForReading(fileName);
            String text = "";
            if (reader.BaseStream.Length > 0)
                text = reader.ReadToEnd();
            reader.Dispose();
            IEnumerable<XmlNode> nodes = this.ParseText(text);
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
                    this.ProcessInheritanceDescriptor(node);
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
                throw new Exception("Unrecognized node: <" + node.Name + ">");
            }
        }

        // opens the file, converts it into a sequence of objects, updates them to the latest format, and writes it out to the new file
        public void ReformatFile(string currentFileName, string newFileName)
        {
            // If the file exists, then we want to read all of its data
            XmlDocument document = new XmlDocument();
            document.LoadXml(this.internalFileIo.ReadAllText(currentFileName));
            XmlNode root = document.FirstChild;
            string outputText = "";
            Participation latestParticipation = null;
            ActivitySuggestion latestSuggestion = null;
            if (root != null)
            {
                foreach (XmlNode currentItem in root.ChildNodes)
                {
                    if (currentItem.Name == this.ParticipationTag)
                    {
                        // read the next particpation
                        Participation newParticipation = this.ReadParticipation(currentItem);
                        // write the previous participation
                        if (latestParticipation != null)
                            outputText += this.ConvertToString(latestParticipation) + Environment.NewLine;

                        if (newParticipation.Suggested.HasValue && newParticipation.Suggested.Value == true)
                        {
                            // This new participation was suggested
                            if (latestSuggestion != null && newParticipation.ActivityDescriptor.CanMatch(latestSuggestion.ActivityDescriptor) && newParticipation.StartDate.Equals(latestSuggestion.StartDate))
                            {
                                // This new participation was suggested but that suggestion was already saved
                            }
                            else
                            {
                                // This new participation was suggested, and its suggestion wasn't saved yet
                                // update latestSuggestion and save it before saving the participation
                                latestSuggestion = new ActivitySuggestion(newParticipation.ActivityDescriptor);
                                latestSuggestion.StartDate = newParticipation.StartDate;
                                outputText += this.ConvertToString(latestSuggestion) + Environment.NewLine;
                            }
                        }

                        // save the latest participation
                        latestParticipation = newParticipation;
                        continue;
                    }
                    if (currentItem.Name == this.RatingTag)
                    {
                        Rating rating = this.ReadRating(currentItem);
                        AbsoluteRating absolute = rating as AbsoluteRating;
                        // check that the rating applies to the previous participation
                        if (absolute != null && latestParticipation != null && absolute.Date.Equals(latestParticipation.StartDate))
                        {
                            // clear some redundant data
                            absolute.Date = null;
                            absolute.ActivityDescriptor = null;
                            // assign it to the latest participation
                            latestParticipation.RawRating = rating;
                            // write the previous participation
                            outputText += this.ConvertToString(latestParticipation) + Environment.NewLine;
                            latestParticipation = null;
                            continue;
                        }
                        if (latestParticipation != null)
                        {
                            outputText += this.ConvertToString(latestParticipation) + Environment.NewLine;
                            latestParticipation = null;
                        }
                        // maybe it's an ActivityRequest
                        if (absolute != null && absolute.Score == 1)
                        {
                            ActivityRequest request = new ActivityRequest(absolute.ActivityDescriptor, (DateTime)absolute.Date);
                            outputText += this.ConvertToString(request) + Environment.NewLine;
                            continue;
                        }
                    }
                    if (latestParticipation != null)
                    {
                        outputText += this.ConvertToString(latestParticipation) + Environment.NewLine;
                        latestParticipation = null;
                    }
                    if (currentItem.Name == this.ActivityDescriptorTag)
                    {
                        continue;
                    }
                    if (currentItem.Name == this.InheritanceTag)
                    {
                        continue;
                    }
                    if (currentItem.Name == this.DateTag)
                    {
                        continue;
                    }
                    if (currentItem.Name == this.SkipTag)
                    {
                        ActivitySkip skip = this.ReadSkip(currentItem);

                        if (latestSuggestion != null && latestSuggestion.ActivityDescriptor.CanMatch(skip.ActivityDescriptor) && latestSuggestion.StartDate.Equals(skip.CreationDate))
                        {
                            // This skip applied to an existing suggestion already
                        }
                        else
                        {
                            // This activity was suggested, and that suggestion wasn't saved yet
                            // update latestSuggestion and save it before saving the skip
                            latestSuggestion = new ActivitySuggestion(skip.ActivityDescriptor);
                            latestSuggestion.StartDate = skip.CreationDate;
                            latestSuggestion.CreatedDate = skip.SuggestionCreationDate;


                            outputText += this.ConvertToString(latestSuggestion) + Environment.NewLine;
                        }


                        outputText += this.ConvertToString(skip) + Environment.NewLine;
                        continue;
                    }
                    if (currentItem.Name == this.ActivityRequestTag)
                    {
                        ActivityRequest request = this.ReadActivityRequest(currentItem);
                        outputText += this.ConvertToString(request) + Environment.NewLine;
                        continue;
                    }
                    if (currentItem.Name == this.SuggestionTag)
                    {
                        if (latestParticipation != null)
                        {
                            // If we're reading a suggestion, then first flush any participation data since we won't update that participation any more
                            outputText += this.ConvertToString(latestParticipation) + Environment.NewLine;
                            latestParticipation = null;
                        }
                        latestSuggestion = this.ReadSuggestion(currentItem);
                        outputText += this.ConvertToString(latestSuggestion) + Environment.NewLine;
                        continue;
                    }
                    System.Diagnostics.Debug.WriteLine("Warning: unrecognized symbol in TextConverter.ReformatFile");
                }
            }
            if (latestParticipation != null)
                outputText += this.ConvertToString(latestParticipation) + Environment.NewLine;

            this.internalFileIo.EraseFileAndWriteContent(newFileName, outputText);
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
            this.activityDatabase.CreateCategory(activityDescriptor);
        }
        private void ProcessTodo(XmlNode nodeRepresentation)
        {
            // the Todo just puts all of the fields of the ActivityDescriptor at the top level
            ActivityDescriptor activityDescriptor = this.ReadActivityDescriptor(nodeRepresentation);
            this.activityDatabase.CreateToDo(activityDescriptor);
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
        private void ProcessInheritanceDescriptor(XmlNode nodeRepresentation)
        {
            Inheritance inheritance = this.ReadInheritance(nodeRepresentation);
            this.listener.AddInheritance(inheritance);
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
            this.listener.SetRecentUserData(data);
        }
        private void ProcessSuggestion(XmlNode nodeRepresentation)
        {
            ActivitySuggestion suggestion = this.ReadSuggestion(nodeRepresentation);
            this.setPendingSkip(null, suggestion.GuessCreationDate());
            this.listener.AddSuggestion(suggestion);
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

            this.latestParticipationRead = currentParticipation;
            return currentParticipation;
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
                if (currentChild.Name == this.SuggestionDateTag)
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
                    request.ActivityDescriptor = this.ReadActivityDescriptor(currentChild);
                    continue;
                }
                if (currentChild.Name == this.DateTag)
                {
                    request.Date = this.ReadDate(currentChild);
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
            data.Synchronized = true;
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
            foreach (XmlNode currentChild in nodeRepresentation.ChildNodes)
            {
                if (currentChild.Name == this.ActivityDescriptorTag)
                {
                    descriptor = this.ReadActivityDescriptor(currentChild);
                    continue;
                }
                if (currentChild.Name == this.SuggestionDateTag)
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
            }
            ActivitySuggestion suggestion = new ActivitySuggestion(descriptor);
            suggestion.CreatedDate = createdDate;
            suggestion.StartDate = startDate;
            suggestion.EndDate = endDate;
            return suggestion;
        }

        public void Import(string contents, string inheritancesFilePath, string historyFilePath, string recentUserDataPath)
        {
            ActivityDatabase activityDatabase = new ActivityDatabase(null);
            IEnumerable<XmlNode> nodes = this.ParseText(contents);

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
                    Inheritance inheritance = this.ReadInheritance(node);
                    inheritanceTexts.Add(this.ConvertToString(inheritance));
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
                if (node.Name == this.CategoryTag)
                {
                    ActivityDescriptor activityDescriptor = this.ReadActivityDescriptor(node);
                    Activity activity = activityDatabase.GetOrCreateCategory(activityDescriptor);
                    historyTexts.Add(this.ConvertToString(activity));
                    continue;
                }
                if (node.Name == this.TodoTag)
                {
                    ActivityDescriptor activityDescriptor = this.ReadActivityDescriptor(node);
                    Activity activity = activityDatabase.GetOrCreateTodo(activityDescriptor);
                    historyTexts.Add(this.ConvertToString(activity));
                    continue;
                }
                throw new Exception("Unrecognized node: <" + node.Name + ">");
            }

            string inheritancesText = String.Join("\n", inheritanceTexts);
            this.internalFileIo.EraseFileAndWriteContent(inheritancesFilePath, inheritancesText);
            string historyText = string.Join("\n", historyTexts);
            this.internalFileIo.EraseFileAndWriteContent(historyFilePath, historyText);
            this.internalFileIo.EraseFileAndWriteContent(recentUserDataPath, recentUserDataText);
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
                properties[this.ActivityNameTag] = activityName;
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
        private string SuggestionDateTag
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


}
