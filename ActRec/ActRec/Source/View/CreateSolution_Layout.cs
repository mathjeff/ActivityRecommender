using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;

namespace ActivityRecommendation.View
{
    class CreateSolution_Layout : ActivityCreationLayout
    {
        public CreateSolution_Layout(ActivityDatabase activityDatabase, LayoutStack layoutStack)
            : base(activityDatabase, layoutStack)
        {
        }

        public override List<AppFeature> GetFeatures()
        {
            return new List<AppFeature>() {
                new CreateSolution_Feature(this.activityDatabase)
            };
        }

        protected override string doCreate(Inheritance inheritance)
        {
            return this.activityDatabase.CreateCategory(inheritance);
        }
        protected override string getShortExplanation()
        {
            return "A Solution is another name for a Category. If you assign a Category as a child of a Problem, the Category will be considered a solution to that Problem.";
        }
    }

    class CreateSolution_Feature : AppFeature
    {
        public CreateSolution_Feature(ActivityDatabase activityDatabase)
        {
            this.activityDatabase = activityDatabase;
        }
        public string GetDescription()
        {
            return "Declare a Solution";
        }
        public bool GetHasBeenUsed()
        {
            return this.activityDatabase.HasSolution;
        }

        public bool GetIsUsable()
        {
            return this.activityDatabase.HasProblem;
        }
        ActivityDatabase activityDatabase;
    }

}
