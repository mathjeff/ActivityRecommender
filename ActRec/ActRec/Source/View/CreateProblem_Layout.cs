using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;

namespace ActivityRecommendation.View
{
    class CreateProblem_Layout : ActivityCreationLayout
    {
        public CreateProblem_Layout(ActivityDatabase activityDatabase, LayoutStack layoutStack)
            : base(activityDatabase, layoutStack)
        {
        }

        public override List<AppFeature> GetFeatures()
        {
            return new List<AppFeature>() {
                new CreateProblem_Feature(this.activityDatabase)
            };
        }

        protected override string doCreate(Inheritance inheritance)
        {
            return this.activityDatabase.CreateProblem(inheritance);
        }

        protected override string getShortExplanation()
        {
            return "A Problem is something you may want to fix multiple times. Its children may be other Problems or may be other Categories (Solutions).";
        }
    }

    class CreateProblem_Feature : AppFeature
    {
        public CreateProblem_Feature(ActivityDatabase activityDatabase)
        {
            this.activityDatabase = activityDatabase;
        }
        public string GetDescription()
        {
            return "Declare a Problem";
        }
        public bool GetHasBeenUsed()
        {
            return this.activityDatabase.HasProblem;
        }

        public bool GetIsUsable()
        {
            return true;
        }
        ActivityDatabase activityDatabase;
    }

}
