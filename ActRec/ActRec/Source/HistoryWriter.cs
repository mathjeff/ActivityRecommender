using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

// The HistoryWriter will resave the user's rating data
// Note that because it makes use of the latest TextConverter for reading, it might save different data than was read
// The purpose of this is to clean up the data and make it accurate
namespace ActivityRecommendation
{
    class HistoryWriter : HistoryReplayer
    {
        public HistoryWriter(TextConverter textConverter)
        {
            this.textConverter = textConverter;
        }
        public override void PostInheritance(Inheritance newInheritance)
        {
            string text = this.textConverter.ConvertToString(newInheritance) + Environment.NewLine;
            this.addText(text);
        }
        public override RelativeRating ProcessRating(RelativeRating newRating)
        {
            return newRating;
        }

        public override void PostParticipation(Participation newParticipation)
        {
            string text = this.textConverter.ConvertToString(newParticipation) + Environment.NewLine;
            this.addText(text);
        }

        public override void PreviewSuggestion(ActivitySuggestion suggestion)
        {
            string text = this.textConverter.ConvertToString(suggestion) + Environment.NewLine;
            this.addText(text);
        }

        public override void PreviewSkip(ActivitySkip newSkip)
        {
            string text = this.textConverter.ConvertToString(newSkip) + Environment.NewLine;
            this.addText(text);
        }

        public override void PreviewRequest(ActivityRequest newRequest)
        {
            string text = this.textConverter.ConvertToString(newRequest) + Environment.NewLine;
            this.addText(text);
        }
        private void addText(string text)
        {
            this.fileContentComponents.Add(text);
        }

        public override Engine Finish()
        {
            // TODO: figure out how to delete a file
            string nowText = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            string fileName = "ActivityData-reformatted-" + nowText + ".txt";

            this.publicFileIo.ExportFile(fileName, String.Join("", this.fileContentComponents));

            return null;
        }

        private TextConverter textConverter;
        private List<string> fileContentComponents = new List<string>();
        private PublicFileIo publicFileIo = new PublicFileIo();
    }
}