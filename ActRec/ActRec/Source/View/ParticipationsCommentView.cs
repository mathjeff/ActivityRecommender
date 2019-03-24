using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisiPlacement;
using Xamarin.Forms;

// a ParticipationsCommentView lets the user see the comments for their favorite participations
namespace ActivityRecommendation.View
{
    class ParticipationsCommentView : ContainerLayout
    {
        public ParticipationsCommentView(ActivityDatabase activityDatabase, LayoutStack layoutStack)
        {
            this.activityDatabase = activityDatabase;
            this.layoutStack = layoutStack;

            TextblockLayout helpLayout = new TextblockLayout("Browse the highest-scored participations among those having comments");

            ActivityNameEntryBox categoryBox = new ActivityNameEntryBox("From category (Optional)");
            categoryBox.Database = activityDatabase;
            this.categoryBox = categoryBox;

            Button okButton = new Button();
            ButtonLayout okButtonLayout = new ButtonLayout(okButton, "Browse");
            okButton.Clicked += OkButton_Clicked;

            this.SubLayout = new Vertical_GridLayout_Builder().Uniform().AddLayout(helpLayout).AddLayout(categoryBox).AddLayout(okButtonLayout).Build();
        }

        private void OkButton_Clicked(object sender, EventArgs e)
        {
            Activity activity = this.categoryBox.Activity;
            if (activity == null)
                activity = this.activityDatabase.RootActivity;

            this.layoutStack.AddLayout(this.summarizeActivity(activity));
        }

        private LayoutChoice_Set summarizeActivity(Activity activity)
        {
            List<Participation> participations = activity.CommentedParticipationsSortedByDecreasingScore;
            int maxCount = 10;
            if (participations.Count < 1)
                return new TextblockLayout("No commented participations exist for " + activity.Name + "! First record a participation and enter a comment.");

            if (participations.Count > maxCount)
                participations = participations.GetRange(0, maxCount);

            TitledControl mainView = new TitledControl("Top " + participations.Count + " commented participations in " + activity.Name);
            Vertical_GridLayout_Builder gridBuilder = new Vertical_GridLayout_Builder().Uniform();
            foreach (Participation participation in participations)
            {
                gridBuilder.AddLayout(new ParticipationView(participation));
            }
            mainView.SetContent(ScrollLayout.New(gridBuilder.Build()));
            return mainView;
        }

        private ActivityDatabase activityDatabase;
        private LayoutStack layoutStack;
        private ActivityNameEntryBox categoryBox;
    }
}
