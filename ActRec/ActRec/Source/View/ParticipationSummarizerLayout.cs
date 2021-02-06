using ActivityRecommendation.TextSummary;
using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    class ParticipationSummarizerLayout : ContainerLayout
    {
        public ParticipationSummarizerLayout(Engine engine, Persona persona, LayoutStack layoutStack)
        {
            this.lifeSummarizer = new LifeSummarizer(engine, persona, new Random());
            this.layoutStack = layoutStack;

            this.sinceView = new DateEntryView("since", layoutStack);
            this.sinceView.SetDay(DateTime.Now.Subtract(TimeSpan.FromDays(7)));

            Button button = new Button();
            button.Clicked += Button_Clicked;
            ButtonLayout buttonLayout = new ButtonLayout(button, "See my Story!");

            LayoutChoice_Set sublayout = new Vertical_GridLayout_Builder()
                .AddLayout(new TextblockLayout("Summarize Recent Participations"))
                .AddLayout(this.sinceView)
                .AddLayout(buttonLayout)
                .BuildAnyLayout();

            this.SubLayout = sublayout;

        }

        private DateTime StartDate
        {
            get
            {
                if (this.sinceView.IsDateValid())
                {
                    return this.sinceView.GetDate();
                }
                return new DateTime(0);
            }
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            string summary = this.lifeSummarizer.Summarize(this.StartDate, DateTime.Now);
            // Show the summary in a text box so the user can copy the text. If the user wants to change the text, that's ok too
            Editor textBox = new Editor();
            textBox.Text = summary;
            this.layoutStack.AddLayout(ScrollLayout.New(new TextboxLayout(textBox), true), "Summary");
        }

        private LifeSummarizer lifeSummarizer;
        private LayoutStack layoutStack;
        private DateEntryView sinceView;
    }
}
