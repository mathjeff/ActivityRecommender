using ActivityRecommendation.Effectiveness;
using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;
using Xamarin.Forms;

// a ChooseMetric_View lets the user choose a Metric assigned to a certain Activity.
// The caller can also demand that the Metric must be a specific one, if the Activity has it
namespace ActivityRecommendation.View
{
    public class ChooseMetric_View : ContainerLayout
    {
        public event ChoseNewMetricHandler ChoseNewMetric;
        public delegate void ChoseNewMetricHandler(ChooseMetric_View view);
        public ChooseMetric_View()
        {
            Button button = new Button();
            button.Clicked += Button_Clicked;
            this.buttonLayout = new ButtonLayout(button);
            this.singleMetric_layout = new TextblockLayout();
        }


        public void SetActivity(Activity activity)
        {
            this.activity = activity;
            this.selectedIndex = 0;
            this.updateLayout();
        }

        public void DemandMetric(Metric metric)
        {
            this.demandedMetric = metric;
            this.updateLayout();
        }

        public void Choose(string metricName)
        {
            int index = -1;
            List<Metric> metrics = this.activity.AllMetrics;
            for (int i = 0; i < metrics.Count; i++)
            {
                if (metrics[i].Name == metricName)
                {
                    index = i;
                    break;
                }
            }
            if (index < 0)
                throw new ArgumentException("Metric " + metricName + " not found in " + this.activity);
            this.selectedIndex = index;
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            if (this.activity != null && this.activity.HasAMetric)
            {
                this.selectedIndex++;
                if (this.selectedIndex >= this.activity.AllMetrics.Count)
                    this.selectedIndex = 0;
            }
            if (this.ChoseNewMetric != null)
            {
                this.ChoseNewMetric.Invoke(this);
            }
            this.updateLayout();
        }

        public Metric Metric
        {
            get
            {
                // If there's no activity then there can't be a metric
                if (this.activity == null || !this.activity.HasAMetric)
                    return null;
                List<Metric> metrics = this.activity.AllMetrics;
                // If the caller demanded a certain valid metric, then use that
                if (this.demandedMetric != null)
                {
                    if (metrics.Contains(this.demandedMetric))
                        return this.demandedMetric;
                }
                // Return currently selected metric
                return this.activity.AllMetrics[this.selectedIndex];
            }
        }

        private void updateLayout()
        {
            Metric metric = this.Metric;
            if (metric == null)
            {
                this.SubLayout = null;
                return;
            }
            string text = metric.Name + "?";
            if (this.demandedMetric != null || this.activity.AllMetrics.Count == 1)
            {
                this.singleMetric_layout.setText(text);
                this.SubLayout = this.singleMetric_layout;
            }
            else
            {
                this.buttonLayout.setText(text);
                this.SubLayout = this.buttonLayout;
            }
        }

        private Activity activity;
        private Metric demandedMetric;
        int selectedIndex;
        ButtonLayout buttonLayout;
        TextblockLayout singleMetric_layout;
    }
}
