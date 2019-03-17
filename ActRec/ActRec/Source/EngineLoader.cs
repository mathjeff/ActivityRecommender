using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActivityRecommendation
{
    class EngineLoader : HistoryReplayer
    {
        public EngineLoader()
        {
        }

        public override Engine Finish()
        {
            return this.engine;
        }
        public override void PreviewParticipation(Participation newParticipation)
        {
            if (this.LatestParticipation == null || newParticipation.EndDate.CompareTo(this.LatestParticipation.EndDate) > 0)
                this.LatestParticipation = newParticipation;
        }
        public override void PreviewSkip(ActivitySkip newSkip)
        {
            // link the skip to its suggestion
            DateTime suggestionCreationDate = newSkip.SuggestionCreationDate;
            ActivitySuggestion suggestion = this.SuggestionDatabase.GetSuggestion(newSkip.ActivityDescriptor, suggestionCreationDate);
            if (suggestion != null)
                suggestion.Skip = newSkip;
        }
        public override void PreviewSuggestion(ActivitySuggestion suggestion)
        {
            this.SuggestionDatabase.AddSuggestion(suggestion);
        }
        public override void PreviewActivityDescriptor(ActivityDescriptor activityDescriptor)
        {
            this.engine.PutActivityDescriptorInMemory(activityDescriptor);
        }
        public override void Preview_ProtoActivity(ProtoActivity protoActivity)
        {
            this.ProtoActivity_Database.Put(protoActivity);
        }
        public override void SetRecentUserData(RecentUserData recentUserData)
        {
            this.RecentUserData = recentUserData;
        }
        public override void SetLatestDate(DateTime when)
        {
            this.LatestDate = when;
        }

        public SuggestionDatabase SuggestionDatabase = new SuggestionDatabase();
        public ProtoActivity_Database ProtoActivity_Database = new ProtoActivity_Database();
        public Participation LatestParticipation;
        public RecentUserData RecentUserData = new RecentUserData();
        public DateTime LatestDate;
    }
}
