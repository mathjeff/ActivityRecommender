using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActivityRecommendation
{
    interface IPredictionLink
    {
        Prediction Guess(DateTime when);
    }
}
