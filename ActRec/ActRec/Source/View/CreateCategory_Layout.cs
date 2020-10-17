using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;

namespace ActivityRecommendation.View
{
    class CreateCategory_Layout : ActivityCreationLayout
    {
        public CreateCategory_Layout(ActivityDatabase activityDatabase, LayoutStack layoutStack)
            : base(activityDatabase, layoutStack)
        {

        }

        public override List<AppFeature> GetFeatures()
        {
            return new List<AppFeature>() {
                new CreateActivity_Feature(this.activityDatabase)
            };
        }

        protected override string getShortExplanation()
        {
            return "A Category is a class of things you may do multiple times. A Category may have other activities as children.";
        }


        override protected string doCreate(Inheritance inheritance)
        {
            return this.activityDatabase.CreateCategory(inheritance);
        }
    }

    class CreateActivity_Feature : AppFeature
    {
        public CreateActivity_Feature(ActivityDatabase activityDatabase)
        {
            this.activityDatabase = activityDatabase;
        }
        public string GetDescription()
        {
            return "Create an activity";
        }
        public bool GetHasBeenUsed()
        {
            return this.activityDatabase.ContainsCustomActivity();
        }
        ActivityDatabase activityDatabase;
    }

}
