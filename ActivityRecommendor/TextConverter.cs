using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

// The TextConverter class will convert an object into a string or parse a string into a list of objects
// It only works on the specific types of objects that matter in the ActivityRecommendor project
namespace ActivityRecommendation
{
    class TextConverter
    {
        #region Constructor

        public TextConverter(ActivityRecommendor recommendor)
        {
            this.recommendorToInform = recommendor;
        }
        
        #endregion

        #region Public Member Functions

        // converts the rating into a string that is ready to write to disk
        public string ConvertToString(AbsoluteRating rating)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();
            string objectName = this.RatingTag;

            properties[this.ActivityDescriptorTag] = this.ConvertToStringBody(rating.ActivityDescriptor);
            properties[this.RatingDateTag] = this.ConvertToStringBody(rating.Date);
            properties[this.RatingScoreTag] = this.ConvertToStringBody(rating.Score);

            return this.ConvertToString(properties, objectName);
        }
        // converts the participation into a string that is ready to write to disk
        public string ConvertToString(Participation participation)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();
            string objectName = this.ParticipationTag;

            properties[this.ActivityDescriptorTag] = this.ConvertToStringBody(participation.ActivityDescriptor);
            properties[this.ParticipationStartDateTag] = this.ConvertToStringBody(participation.StartDate);
            properties[this.ParticipationEndDateTag] = this.ConvertToStringBody(participation.EndDate);

            return this.ConvertToString(properties, objectName);
        }
        // converts the Inheritance into a string that is ready to write to disk
        public string ConvertToString(Inheritance inheritance)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();
            string objectName = this.InheritanceTag;

            properties[this.InheritanceChildTag] = this.ConvertToStringBody(inheritance.ChildDescriptor);
            properties[this.InheritanceParentTag] = this.ConvertToStringBody(inheritance.ParentDescriptor);
            properties[this.DiscoveryDateTag] = this.ConvertToStringBody(inheritance.DiscoveryDate);

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
            string text = System.IO.File.ReadAllText(fileName);
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
                    }
                    if (currentItem.Name == this.InheritanceTag)
                    {
                        this.ProcessInheritanceDescriptor(currentItem);
                    }
                    if (currentItem.Name == this.DateTag)
                    {
                        this.ProcessLatestDate(currentItem);
                    }
                }
            }
        }

        #endregion

        #region Private Member Functions

        private void ProcessLatestDate(XmlNode nodeRepresentation)
        {
            DateTime when = this.ReadDate(nodeRepresentation);
            this.recommendorToInform.SuspectLatestActionDate(when);
        }
        private void ProcessParticipation(XmlNode nodeRepresentation)
        {
            Participation currentParticipation = this.ReadParticipation(nodeRepresentation);
            this.recommendorToInform.PutParticipationInMemory(currentParticipation);
        }
        private void ProcessRating(XmlNode nodeRepresentation)
        {
            AbsoluteRating currentRating = this.ReadRating(nodeRepresentation);
            this.recommendorToInform.PutRatingInMemory(currentRating);
        }
        private void ProcessActivityDescriptor(XmlNode nodeRepresentation)
        {
            ActivityDescriptor descriptor = this.ReadActivityDescriptor(nodeRepresentation);
            this.recommendorToInform.PutActivityDescriptorInMemory(descriptor);
        }
        private void ProcessInheritanceDescriptor(XmlNode nodeRepresentation)
        {
            Inheritance inheritance = this.ReadInheritance(nodeRepresentation);
            this.recommendorToInform.PutInheritanceInMemory(inheritance);
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
            //Participation currentParticipation = new Participation();
            // The participation may have an embedded rating
            AbsoluteRating absoluteRating1 = null;
            ActivityDescriptor activityDescriptor = new ActivityDescriptor();
            DateTime startDate = DateTime.Now;
            DateTime endDate = DateTime.Now;
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
                    absoluteRating1 = this.ReadRating(currentChild);
                }
            }
            Participation currentParticipation = new Participation(startDate, endDate, activityDescriptor);

            // Chek whether the participation had an embedded rating
            if (absoluteRating1 != null)
            {
                // fill in necessary details
                absoluteRating1.ActivityDescriptor = currentParticipation.ActivityDescriptor;
                AbsoluteRating absoluteRating2 = new AbsoluteRating(absoluteRating1);

                // create a rating for the start of the participation
                absoluteRating1.Date = currentParticipation.StartDate;
                absoluteRating1.Weight = 1;
                this.recommendorToInform.PutRatingInMemory(absoluteRating1);

                // create a rating for the end of the participation
                //absoluteRating2.Date = currentParticipation.EndDate;
                //absoluteRating2.Weight = 0.5;
                //this.recommendorToInform.PutRatingInMemory(absoluteRating2);
            }
            return currentParticipation;
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
        private AbsoluteRating ReadRating(XmlNode nodeRepresentation)
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
        private double ReadDouble(XmlNode nodeRepresentation)
        {
            string text = this.ReadText(nodeRepresentation);
            return Double.Parse(text);
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
        private string ConvertToStringBody(DateTime when)
        {
            string result = when.ToString("yyyy-MM-ddTHH:mm:ss");
            return result;
        }
        // converts the double into a string, and doesn't add the initial <Tag> or  ending </Tag>
        private string ConvertToStringBody(double value)
        {
            return value.ToString();
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

        private string DateTag
        {
            get
            {
                return "Date";
            }
        }
        #endregion

        #region Private Member Variables

        private ActivityRecommendor recommendorToInform;

        #endregion
    }
}
