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

        public Engine GetEngine()
        {
            base.Finish();
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
            foreach (ActivityDescriptor descriptor in newSkip.ActivityDescriptors)
            {
                ActivitySuggestion suggestion = this.SuggestionDatabase.GetSuggestion(descriptor, suggestionCreationDate);
                if (suggestion != null)
                    suggestion.Skip = newSkip;
            }
        }
        public override void PreviewSuggestion(ActivitiesSuggestion suggestion)
        {
            foreach (ActivitySuggestion child in suggestion.Children)
            {
                this.SuggestionDatabase.AddSuggestion(child);
            }
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
        public override void SetPersona(UserSettings persona)
        {
            this.Persona = persona;
        }

        public SuggestionDatabase SuggestionDatabase = new SuggestionDatabase();
        public ProtoActivity_Database ProtoActivity_Database = new ProtoActivity_Database();
        public Participation LatestParticipation;
        public RecentUserData RecentUserData = new RecentUserData();
        public DateTime LatestDate;
        public UserSettings Persona = new UserSettings();
    }
}
