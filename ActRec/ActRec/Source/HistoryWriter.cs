using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ActivityRecommendation.Effectiveness;

// The HistoryWriter will resave the user's rating data
// Note that because it makes use of the latest TextConverter for reading, it might save different data than was read
// The purpose of this is to clean up the data and make it accurate
namespace ActivityRecommendation
{
    class HistoryWriter : HistoryReplayer
    {
        public HistoryWriter()
        {
        }
        public override void PostCategory(Category category)
        {
            string text = this.textConverter.ConvertToString(category);
            this.addText(text);
        }
        public override void PostToDo(ToDo todo)
        {
            string text = this.textConverter.ConvertToString(todo);
            this.addText(text);
        }
        public override void PostProblem(Problem problem)
        {
            string text = this.textConverter.ConvertToString(problem);
            this.addText(text);
        }
        public override void PostInheritance(Inheritance newInheritance)
        {
            string text = this.textConverter.ConvertToString(newInheritance);
            this.addText(text);
        }
        public override RelativeRating ProcessRating(RelativeRating newRating)
        {
            return newRating;
        }
        public override void PreviewMetric(Metric metric)
        {
            string text = this.textConverter.ConvertToString(metric);
            this.addText(text);
        }

        public override void PostParticipation(Participation newParticipation)
        {
            string text = this.textConverter.ConvertToString(newParticipation);
            this.addText(text);
        }

        public override void PreviewSuggestion(ActivitiesSuggestion suggestion)
        {
            string text = this.textConverter.ConvertToString(suggestion);
            this.addText(text);
        }

        public override void PreviewSkip(ActivitySkip newSkip)
        {
            string text = this.textConverter.ConvertToString(newSkip);
            this.addText(text);
        }

        public override void PreviewRequest(ActivityRequest newRequest)
        {
            string text = this.textConverter.ConvertToString(newRequest);
            this.addText(text);
        }

        public override void PreviewExperiment(PlannedExperiment experiment)
        {
            string text = this.textConverter.ConvertToString(experiment);
            this.addText(text);
        }

        public override void SetRecentUserData(RecentUserData recentUserData)
        {
            string text = this.textConverter.ConvertToString(recentUserData);
            this.addText(text);
        }
        public override void Preview_ProtoActivity(ProtoActivity protoActivity)
        {
            string text = this.textConverter.ConvertToString(protoActivity);
            this.addText(text);
        }
        public override void SetPersona(Persona persona)
        {
            string text = this.textConverter.ConvertToString(persona);
            this.addText(text);
        }

        private void addText(string text)
        {
            this.fileContentComponents.Add(text);
        }

        public override string Serialize()
        {
            // TODO: figure out how to delete a file

            // add a new empty line so a newline appears after the last tline
            this.fileContentComponents.Add("");

            return String.Join(Environment.NewLine, this.fileContentComponents);
        }

        private List<string> fileContentComponents = new List<string>();
        private PublicFileIo publicFileIo = new PublicFileIo();
    }
}