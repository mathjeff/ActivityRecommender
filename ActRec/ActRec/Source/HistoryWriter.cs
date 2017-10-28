using System;
using System.IO;

// The HistoryWriter will resave the user's rating data
// Note that because it makes use of the latest TextConverter for reading, it might save different data than was read
// The purpose of this is to clean up the data and make it accurate
namespace ActivityRecommendation
{
    class HistoryWriter : RatingReplayer
    {
        public HistoryWriter(TextConverter textConverter)
        {
            this.textConverter = textConverter;
        }
        public override RelativeRating ProcessRating(RelativeRating newRating)
        {
            return newRating;
        }

        public override void PostParticipation(Participation newParticipation)
        {
            string text = this.textConverter.ConvertToString(newParticipation) + Environment.NewLine;
            this.fileContent += text;
        }

        public override void PreviewSuggestion(ActivitySuggestion suggestion)
        {
            string text = this.textConverter.ConvertToString(suggestion) + Environment.NewLine;
            this.fileContent += text;
        }

        public override void PreviewSkip(ActivitySkip newSkip)
        {
            string text = this.textConverter.ConvertToString(newSkip) + Environment.NewLine;
            this.fileContent += text;
        }

        public override void PreviewRequest(ActivityRequest newRequest)
        {
            string text = this.textConverter.ConvertToString(newRequest) + Environment.NewLine;
            this.fileContent += text;
        }

        public override void Finish()
        {
            StreamWriter writer = this.textConverter.EraseFileAndOpenForWriting("reformatted.txt");
            writer.Write(this.fileContent);
            //writer.Close();
        }

        private TextConverter textConverter;
        private string fileContent = "";
    }
}