using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

// The TextConverter class will convert an object into a string or parse a string into a list of objects
// It only works on the specific types of objects that matter in the ActivityRecommender project
namespace ActivityRecommendation
{
    class TextConverter
    {
        #region Constructor

        public TextConverter(ActivityRecommender recommender)
        {
            this.recommenderToInform = recommender;
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
            if (data.LatestSuggestion != null)
                properties[this.SuggestionTag] = this.ConvertToStringBody(data.LatestSuggestion);

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
            properties[this.DateTag] = this.ConvertToStringBody(skip.Date);
            if (skip.SuggestionDate != null)
                properties[this.SuggestionDateTag] = this.ConvertToStringBody(skip.SuggestionDate);
            properties[this.RatingTag] = this.ConvertToStringBody(skip.RawRating);

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

        // opens the file, converts it into a sequence of objects, and sends them to the Engine
        // This function isn't done yet
        public void ReadFile(string fileName)
        {
            // If the file exists, then we want to read all of its data
            string text;
            try
            {
                text = System.IO.File.ReadAllText(fileName);
            }
            catch
            {
                // if the file doesn't exist, then there simply isn't anything to do
                return;
            }
            text = "<root>" + text + "</root>";
            XmlDocument document = new XmlDocument();
            document.LoadXml(text);
            XmlNode root = document.FirstChild;
            if (root != null)
            {
                foreach (XmlNode currentItem in root.ChildNodes)
                {
                    if (currentItem.Name == this.ParticipationTag)
                    {
                        this.ProcessParticipation(currentItem);
                        continue;
                    }
                    if (currentItem.Name == this.RatingTag)
                    {
                        this.ProcessRating(currentItem);
                        continue;
                    }
                    if (currentItem.Name == this.ActivityDescriptorTag)
                    {
                        this.ProcessActivityDescriptor(currentItem);
                        continue;
                    }
                    if (currentItem.Name == this.InheritanceTag)
                    {
                        this.ProcessInheritanceDescriptor(currentItem);
                        continue;
                    }
                    if (currentItem.Name == this.DateTag)
                    {
                        this.ProcessLatestDate(currentItem);
                        continue;
                    }
                    if (currentItem.Name == this.SkipTag)
                    {
                        this.ProcessSkip(currentItem);
                        continue;
                    }
                    if (currentItem.Name == this.ActivityRequestTag)
                    {
                        this.ProcessActivityRequest(currentItem);
                        continue;
                    }
                    if (currentItem.Name == this.RecentUserDataTag)
                    {
                        this.ProcessRecentUserData(currentItem);
                        continue;
                    }
                }
            }
        }

        // opens the file, converts it into a sequence of objects, updates them to the latest format, and writes it out to the new file
        public void ReformatFile(string currentFileName, string newFileName)
        {
            // If the file exists, then we want to read all of its data
            string inputText;
            try
            {
                inputText = System.IO.File.ReadAllText(currentFileName);
            }
            catch
            {
                // if the file doesn't exist, then there simply isn't anything to do
                return;
            }
            inputText = "<root>" + inputText + "</root>";
            XmlDocument document = new XmlDocument();
            document.LoadXml(inputText);
            XmlNode root = document.FirstChild;
            string outputText = "";
            Participation latestParticipation = null;
            if (root != null)
            {
                foreach (XmlNode currentItem in root.ChildNodes)
                {
                    if (currentItem.Name == this.ParticipationTag)
                    {
                        // write the previous participation
                        if (latestParticipation != null)
                            outputText += this.ConvertToString(latestParticipation) + Environment.NewLine;
                        // read the new participation
                        latestParticipation = this.ReadParticipation(currentItem);
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
                        // Maybe it's a skip
                        if (absolute != null && absolute.Score.ToString().Length > 6 || absolute.Score == 0)
                        {
                            // if it is an absolute rating with lots of digits, then it's a Skip
                            ActivitySkip skip = new ActivitySkip((DateTime)absolute.Date, absolute.ActivityDescriptor);
                            skip.RawRating = absolute;
                            // remove some redundant data
                            skip.RawRating.ActivityDescriptor = null;
                            skip.RawRating.Date = null;
                            outputText += this.ConvertToString(skip) + Environment.NewLine;
                            continue;
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
                        outputText += this.ConvertToString(skip) + Environment.NewLine;
                        continue;
                    }
                    if (currentItem.Name == this.ActivityRequestTag)
                    {
                        ActivityRequest request = this.ReadActivityRequest(currentItem);
                        outputText += this.ConvertToString(request) + Environment.NewLine;
                        continue;
                    }
                    Console.WriteLine("Warning: unrecognized symbol in TextConverter.ReformatFile");
                }
            }
            if (latestParticipation != null)
                outputText += this.ConvertToString(latestParticipation) + Environment.NewLine;

            StreamWriter writer = new StreamWriter(newFileName, false);
            writer.Write(outputText);
            writer.Close();
        }
        #endregion

        #region Private Member Functions

        private void ProcessLatestDate(XmlNode nodeRepresentation)
        {
            DateTime when = this.ReadDate(nodeRepresentation);
            this.recommenderToInform.SuspectLatestActionDate(when);
        }
        private void ProcessParticipation(XmlNode nodeRepresentation)
        {
            Participation currentParticipation = this.ReadParticipation(nodeRepresentation);
            if (currentParticipation.Duration.TotalSeconds >= 0)
            {
                this.recommenderToInform.PutParticipationInMemory(currentParticipation);
            }
            else
            {
                Console.WriteLine("Skipping invalid participation having startDate = " + currentParticipation.StartDate.ToString() + " and endDate = " + currentParticipation.EndDate);
            }
        }
        private void ProcessRating(XmlNode nodeRepresentation)
        {
            Rating currentRating = this.ReadRating(nodeRepresentation);
            this.recommenderToInform.PutRatingInMemory(currentRating);
        }
        private void ProcessActivityDescriptor(XmlNode nodeRepresentation)
        {
            ActivityDescriptor descriptor = this.ReadActivityDescriptor(nodeRepresentation);
            this.recommenderToInform.PutActivityDescriptorInMemory(descriptor);
        }
        private void ProcessInheritanceDescriptor(XmlNode nodeRepresentation)
        {
            Inheritance inheritance = this.ReadInheritance(nodeRepresentation);
            this.recommenderToInform.PutInheritanceInMemory(inheritance);
        }
        private void ProcessSkip(XmlNode nodeRepresentation)
        {
            ActivitySkip skip = this.ReadSkip(nodeRepresentation);
            this.recommenderToInform.PutSkipInMemory(skip);
        }
        private void ProcessActivityRequest(XmlNode nodeRepresentation)
        {
            ActivityRequest request = this.ReadActivityRequest(nodeRepresentation);
            this.recommenderToInform.PutActivityRequestInMemory(request);
        }
        private void ProcessRecentUserData(XmlNode nodeRepresentation)
        {
            RecentUserData data = this.ReadRecentUserData(nodeRepresentation);
            this.recommenderToInform.SetRecentUserData(data);
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
                rating.Source = RatingSource.FromParticipation(currentParticipation);
            currentParticipation.RawRating = rating;
            currentParticipation.Comment = comment;
            currentParticipation.Suggested = suggested;

            /* // Check whether the participation had an embedded rating
            if (rating != null)
            {
                // fill in necessary details
                //rating.FillInFromParticipation(currentParticipation);
                // send the rating to the engine
                //this.recommenderToInform.PutRatingInMemory(rating);
            }*/
            return currentParticipation;
        }

        // returns an object of type "Skip" that this XmlNode represents
        private ActivitySkip ReadSkip(XmlNode nodeRepresentation)
        {
            ActivitySkip skip = new ActivitySkip();
            foreach (XmlNode currentChild in nodeRepresentation.ChildNodes)
            {
                if (currentChild.Name == this.ActivityDescriptorTag)
                {
                    skip.ActivityDescriptor = this.ReadActivityDescriptor(currentChild);
                    continue;
                }
                if (currentChild.Name == this.DateTag)
                {
                    skip.Date = this.ReadDate(currentChild);
                    continue;
                }
                if (currentChild.Name == this.SuggestionDateTag)
                {
                    skip.SuggestionDate = this.ReadDate(currentChild);
                    continue;
                }
                if (currentChild.Name == this.RatingTag)
                {
                    skip.RawRating = this.ReadAbsoluteRating(currentChild);
                    continue;
                }
            }
            return skip;
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
                if (currentChild.Name == this.RatingSourceTag)
                {
                    rating.Source = this.ReadRatingSource(currentChild);
                    continue;
                }
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
        private RatingSource ReadRatingSource(XmlNode nodeRepresentation)
        {
            string text = this.ReadText(nodeRepresentation);
            RatingSource source = RatingSource.GetSourceWithDescription(text);
            return source;
        }
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
                if (currentChild.Name == this.SuggestionTag)
                {
                    data.LatestSuggestion = this.ReadSuggestion(currentChild);
                    continue;
                }
            }
            data.Synchronized = true;
            return data;
        }
        private ActivitySuggestion ReadSuggestion(XmlNode nodeRepresentation)
        {
            ActivityDescriptor descriptor = null;
            DateTime startDate = new DateTime();
            DateTime endDate = new DateTime();
            foreach (XmlNode currentChild in nodeRepresentation.ChildNodes)
            {
                if (currentChild.Name == this.ActivityDescriptorTag)
                {
                    descriptor = this.ReadActivityDescriptor(currentChild);
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
            suggestion.StartDate = startDate;
            suggestion.EndDate = endDate;
            return suggestion;
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

            return this.ConvertToStringBody(properties);
        }
        // converts the ActivitySuggestion into a string, and doesn't add the inital <Tag> or ending </Tag>
        private string ConvertToStringBody(ActivitySuggestion suggestion)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();
            properties[this.ActivityDescriptorTag] = this.ConvertToStringBody(suggestion.ActivityDescriptor);

            return this.ConvertToStringBody(properties);
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
        private string RecentUserDataTag
        {
            get
            {
                return "RecentData";
            }
        }
        #endregion

        #region Private Member Variables

        private ActivityRecommender recommenderToInform;

        #endregion
    }
}
