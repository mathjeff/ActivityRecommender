using ActivityRecommendation.Effectiveness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActivityRecommendation
{
    abstract class HistoryReplayer
    {
        public HistoryReplayer()
        {
            this.engine = new Engine();
            this.activityDatabase = this.engine.ActivityDatabase;
            this.textConverter = new TextConverter(this, this.engine.ActivityDatabase);
        }

        public void LoadFile(string filePath)
        {
            this.textConverter.ReadFile(filePath);
        }

        public void AddInheritance(Inheritance newInheritance)
        {
            this.engine.PutInheritanceInMemory(newInheritance);
            this.updatedAfterInheritances = false;
            this.PostInheritance(newInheritance);
        }
        public virtual void PostInheritance(Inheritance newInheritance) { }

        public void AddRequest(ActivityRequest newRequest)
        {
            this.PreviewRequest(newRequest);
            this.engine.PutActivityRequestInMemory(newRequest);
        }
        public virtual void PreviewRequest(ActivityRequest request) { }

        public void AddSkip(ActivitySkip newSkip)
        {
            this.PreviewSkip(newSkip);
            this.engine.PutSkipInMemory(newSkip);
        }
        public virtual void PreviewSkip(ActivitySkip newSkip) { }

        public void AddParticipation(Participation newParticipation)
        {
            if (!this.updatedAfterInheritances)
            {
                this.engine.FullUpdate();
                this.updatedAfterInheritances = true;
            }
            this.PreviewParticipation(newParticipation);
            RelativeRating newRelativeRating = newParticipation.GetCompleteRating() as RelativeRating;
            if (newRelativeRating != null)
            {
               RelativeRating newRating = this.AddRating(newRelativeRating);
               newParticipation.PutAndCompressRating(newRating);
            }

            this.engine.PutParticipationInMemory(newParticipation);
            this.PostParticipation(newParticipation);

        }
        public virtual void PreviewParticipation(Participation newParticipation) { }
        public virtual void PostParticipation(Participation newParticipation) { }

        public Rating AddRating(Rating newRating)
        {
            if (newRating is RelativeRating)
                return this.ProcessRating((RelativeRating)newRating);
            if (newRating is AbsoluteRating)
                return this.ProcessRating((AbsoluteRating)newRating);
            return null;
        }
        public virtual AbsoluteRating ProcessRating(AbsoluteRating newRating) { return newRating; }
        public virtual RelativeRating ProcessRating(RelativeRating newRating) 
        {
            this.ProcessRating(newRating.FirstRating);
            this.ProcessRating(newRating.SecondRating);
            return newRating;
        }
        public RelativeRating AddRating(RelativeRating newRating)
        {
            this.ProcessRating(newRating.FirstRating);
            this.ProcessRating(newRating.SecondRating);
            return this.ProcessRating(newRating);
        }
        public void AddSuggestion(ActivitySuggestion suggestion)
        {
            this.PreviewSuggestion(suggestion);
            this.engine.PutSuggestionInMemory(suggestion);
        }
        public void AddExperiment(PlannedExperiment experiment)
        {
            this.PreviewExperiment(experiment);
            this.engine.PutExperimentInMemory(experiment);
        }
        public virtual void PreviewSuggestion(ActivitySuggestion suggestion) { }
        public virtual void PreviewExperiment(PlannedExperiment experiment) { }

        public virtual void PreviewActivityDescriptor(ActivityDescriptor activityDescriptor) { }

        public virtual void SetRecentUserData(RecentUserData recentUserData) { }
        public virtual void SetLatestDate(DateTime when) { }

        // Does cleanup
        // Also can return a new Engine for ActivityRecommender to use
        public virtual Engine Finish() { return null; }


        protected Engine engine;
        protected ActivityDatabase activityDatabase;

        private bool updatedAfterInheritances;
        private TextConverter textConverter;

    }
}
