using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

// the ProgressionSelectionView allows the user to choose a variable (which changes over time) to plot
namespace ActivityRecommendation
{
    class ProgressionSelectionView : TitledControl
    {
        public ProgressionSelectionView(string startingTitle)
        {
            this.SetTitle(startingTitle);


            List<object> items = new List<object>();
            items.Add("Time");
            items.Add("Time of day");
            items.Add("Time of week");
            this.nameEntryBox = new ActivityNameEntryBox("Activity named:");
            //items.Add(this.nameEntryBox);

            this.selectorView = new SelectorView(items);

            this.SetContent(this.selectorView);
        }
        public ActivityDatabase ActivityDatabase
        {
            set
            {
                this.nameEntryBox.Database = value;
            }
        }
        // the current progression that the user has selected
        public TimeProgression Progression
        {
            get
            {
                object selectedItem = this.selectorView.SelectedItem;
                if (selectedItem as string == "Time")
                {
                    //return TimeProgression.AbsoluteTime;
                    return null;    // default to plotting the absolute time
                }
                if (selectedItem as string == "Time of day")
                {
                    return TimeProgression.DayCycle;
                }
                if (selectedItem as string == "Time of week")
                {
                    return TimeProgression.WeekCycle;
                }
                if (selectedItem == this.nameEntryBox)
                {
                    throw new NotImplementedException("participation progression won't work yet");
                    //return new CumulativeParticipationProgression(this.nameEntryBox.Activity.ParticipationProgression);
                }
                throw new ArgumentOutOfRangeException("Unknown progression type being requested");
            }
        }
        private ActivityNameEntryBox nameEntryBox;
        private SelectorView selectorView;
    }
}
