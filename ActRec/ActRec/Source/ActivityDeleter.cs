using System;
using System.Collections.Generic;
using System.Text;

namespace ActivityRecommendation
{
    class ActivityDeleter : HistoryWriter
    {
        public ActivityDeleter(Activity activity)
        {
            this.activityToDelete = activity.MakeDescriptor();
        }

        public override void PostInheritance(Inheritance newInheritance)
        {
            if (this.include(newInheritance.ChildDescriptor))
                base.PostInheritance(newInheritance);
            else
                System.Diagnostics.Debug.WriteLine("Deleting " + newInheritance);
        }

        public override void PostCategory(Category category)
        {
            if (this.include(category))
                base.PostCategory(category);
            else
                System.Diagnostics.Debug.WriteLine("Deleting " + category);
        }
        public override void PostProblem(Problem problem)
        {
            if (this.include(problem))
                base.PostProblem(problem);
            else
                System.Diagnostics.Debug.WriteLine("Deleting " + problem);
        }
        public override void PostToDo(ToDo todo)
        {
            if (this.include(todo))
                base.PostToDo(todo);
            else
                System.Diagnostics.Debug.WriteLine("Deleting " + todo);
        }

        public bool include(ActivityDescriptor activityDescriptor)
        {
            return !this.activityToDelete.CanMatch(activityDescriptor);
        }
        public bool include(Activity activity)
        {
            return !this.activityToDelete.Matches(activity);
        }

        private ActivityDescriptor activityToDelete;
    }
}
