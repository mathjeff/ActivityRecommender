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

                Vertical_GridLayout_Builder builder = new Vertical_GridLayout_Builder();
                // Split text into multiple text blocks so that if roudning error occurs in text measurement, the errors are spaced evenly.
                // There could be 50 pages of text, and we don't want 2% rounding error to add a completely blank page at the end
                List<string> lines = new List<string>(this.text.Split(new char[] { '\n' }));
                int numLinesPerBlock = 10;
                for (int i = 0; i < lines.Count; i += numLinesPerBlock)
                {
                    int maxIndex = Math.Min(i + numLinesPerBlock, lines.Count);
                    List<string> currentLines = lines.GetRange(i, maxIndex - i);
                    string currentText = string.Join("\n", currentLines);
                    builder.AddLayout(new TextblockLayout(currentText, 16, false, true));
                }

                this.SubLayout = ScrollLayout.New(builder.BuildAnyLayout());
            }
            return base.GetBestLayout(query);
        }

        private ValueProvider<StreamReader> textProvider;
        private string text = null;
    }
}
