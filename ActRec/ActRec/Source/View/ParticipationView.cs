using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisiPlacement;

namespace ActivityRecommendation.View
{
    class ParticipationView : ContainerLayout
    {
        public ParticipationView(Participation participation, bool showRating = true)
        {
            Vertical_GridLayout_Builder mainGrid_builder = new Vertical_GridLayout_Builder();
            mainGrid_builder.AddLayout(new TextblockLayout(participation.ActivityDescriptor.ActivityName, 30));
            mainGrid_builder.AddLayout(new TextblockLayout(participation.StartDate.ToString() + " - " + participation.EndDate.ToString(), 16));
            if (showRating && participation.GetAbsoluteRating() != null)
                mainGrid_builder.AddLayout(new TextblockLayout("Score: " + participation.GetAbsoluteRating().Score, 16));
            if (participation.Comment != null)
                mainGrid_builder.AddLayout(new TextblockLayout("Comment: " + participation.Comment, 16));
            this.SubLayout = mainGrid_builder.Build();
        }
    }
}
