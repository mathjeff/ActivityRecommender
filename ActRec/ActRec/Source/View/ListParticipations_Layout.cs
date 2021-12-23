using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    class ListParticipations_Layout : ContainerLayout
    {
        public event AddParticipationComment_Handler AddParticipationComment;
        public delegate void AddParticipationComment_Handler(ParticipationComment comment);
        public ListParticipations_Layout(List<Participation> participations, bool showRatings, Engine engine, ScoreSummarizer scoreSummarizer, UserSettings userSettings, LayoutStack layoutStack, Random randomGenerator)
        {
            this.participations = participations;
            this.engine = engine;
            this.scoreSummarizer = scoreSummarizer;
            this.layoutStack = layoutStack;
            this.randomGenerator = randomGenerator;
            this.userSettings = userSettings;
            this.initialize(showRatings);
        }
        public ListParticipations_Layout(List<Participation> participations, Engine engine, ScoreSummarizer scoreSummarizer, LayoutStack layoutStack)
        {
            this.participations = participations;
            this.engine = engine;
            this.scoreSummarizer = scoreSummarizer;
            this.layoutStack = layoutStack;
            this.initialize(true);
        }
        private void initialize(bool showRatings)
        {
            List<Participation> participations = new List<Participation>(this.participations);
            if (!showRatings)
                this.orderRandomly(participations);

            Vertical_GridLayout_Builder gridBuilder = new Vertical_GridLayout_Builder();
            foreach (Participation participation in participations)
            {
                ParticipationView v = new ParticipationView(participation, this.scoreSummarizer, this.layoutStack, this.engine, this.userSettings, showRatings);
                v.AddParticipationComment += Child_AddParticipationComment;
                gridBuilder.AddLayout(v);
            }
            if (!showRatings)
            {
                Button showRatings_button = new Button();
                showRatings_button.Clicked += ShowRatings_button_Clicked;
                showRatings_button.Text = "Show Ratings";
                ButtonLayout showRatings_layout = new ButtonLayout(showRatings_button);
                gridBuilder.AddLayout(showRatings_layout);
            }
            this.SubLayout = ScrollLayout.New(gridBuilder.BuildAnyLayout());
        }

        private void Child_AddParticipationComment(ParticipationComment comment)
        {
            if (AddParticipationComment != null)
                AddParticipationComment.Invoke(comment);
        }

        private void ShowRatings_button_Clicked(object sender, EventArgs e)
        {
            this.initialize(true);
        }

        private void orderRandomly(List<Participation> participations)
        {
            for (int i = 0; i < participations.Count; i++)
            {
                int j = this.randomGenerator.Next(i, participations.Count);
                Participation temp = participations[i];
                participations[i] = participations[j];
                participations[j] = temp;
            }
        }

        List<Participation> participations;
        Random randomGenerator;
        ScoreSummarizer scoreSummarizer;
        LayoutStack layoutStack;
        Engine engine;
        UserSettings userSettings;
    }
}
