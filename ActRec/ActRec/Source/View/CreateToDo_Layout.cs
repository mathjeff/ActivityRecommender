using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;

namespace ActivityRecommendation.View
{
    class CreateToDo_Layout : ActivityCreationLayout
    {
        public CreateToDo_Layout(ActivityDatabase activityDatabase, LayoutStack layoutStack)
            : base(activityDatabase, layoutStack)
        {
        }

        public override List<AppFeature> GetFeatures()
        {
            return new List<AppFeature>() {
                new CreateTodo_Feature(this.activityDatabase)
            };
        }

        protected override string doCreate(Inheritance inheritance)
        {
            return this.activityDatabase.CreateToDo(inheritance);
        }

        protected override string getShortExplanation()
        {
            return "A ToDo is a specific thing that you complete once. A ToDo can't be given children.";
        }
    }

    class CreateTodo_Feature : AppFeature
    {
        public CreateTodo_Feature(ActivityDatabase activityDatabase)
        {
            this.activityDatabase = activityDatabase;
        }
        public string GetDescription()
        {
            return "Create a Todo";
        }
        public bool GetHasBeenUsed()
        {
            return this.activityDatabase.HasTodo;
        }
        ActivityDatabase activityDatabase;
    }

}
