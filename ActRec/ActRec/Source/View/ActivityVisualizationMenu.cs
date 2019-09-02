using ActivityRecommendation.View;
using System;
using VisiPlacement;
using Xamarin.Forms;

// The user is able to view statistics of their data
// This menu is used to allow the user to open that view
namespace ActivityRecommendation
{
    class ActivityVisualizationMenu : TitledControl
    {
        public ActivityVisualizationMenu(ActivityDatabase activityDatabase, LayoutStack layoutStack)
        {
            this.SetTitle("View Statistics");
            Vertical_GridLayout_Builder gridBuilder = new Vertical_GridLayout_Builder().Uniform();

            this.yAxisNameBox = new ActivityNameEntryBox("Activity", activityDatabase, layoutStack);
            gridBuilder.AddLayout(this.yAxisNameBox);

            this.okButton = new Button();

            gridBuilder.AddLayout(new LayoutCache(new ButtonLayout(this.okButton, "Visualize")));

            //this.xAxisNameBox = new ActivityNameEntryBox("X-Axis Activity (optional)");
            //this.xAxisProgressionSelector = new ProgressionSelectionView("X-Axis");

            this.SetContent(new LayoutCache(gridBuilder.Build()));
        }

        public void AddOkClickHandler(EventHandler e)
        {
            this.okButton.Clicked += e;
        }
        /*
        public string XAxisActivityName
        {
            get
            {
                return this.xAxisNameBox.NameText;
            }
            set
            {
                this.xAxisNameBox.NameText = value;
            }
        }
        */
        public IProgression XAxisProgression
        {
            get
            {
                return TimeProgression.AbsoluteTime; //TODO: fix this
                //return this.xAxisProgressionSelector.Progression;
            }
        }
        public ActivityDescriptor YAxisActivityDescriptor
        {
            get
            {
                return this.yAxisNameBox.ActivityDescriptor;
            }
        }
        Button okButton;
        ActivityNameEntryBox yAxisNameBox;
        //ActivityNameEntryBox xAxisNameBox;
        //ProgressionSelectionView xAxisProgressionSelector; // TODO: make this work
    }
}
