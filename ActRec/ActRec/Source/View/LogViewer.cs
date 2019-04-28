using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisiPlacement;

namespace ActivityRecommendation.View
{
    class LogViewer : ContainerLayout
    {
        public LogViewer(ValueProvider<StreamReader> textProvider)
        {
            this.textProvider = textProvider;
        }

        public override SpecificLayout GetBestLayout(LayoutQuery query)
        {
            if (this.text == null)
            {
                if (this.textProvider == null)
                    this.text = "Reading logs on this platform is not supported. Sorry";
                else
                    this.text = textProvider.Get().ReadToEnd();
                TextblockLayout textLayout = new TextblockLayout(this.text, 16, false, true);
                this.SubLayout = ScrollLayout.New(textLayout);
            }
            return base.GetBestLayout(query);
        }

        private ValueProvider<StreamReader> textProvider;
        private string text = null;
    }
}
