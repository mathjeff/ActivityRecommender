using StatLists;
using System;
using System.Collections.Generic;
using System.Text;

namespace ActivityRecommendation
{
    class IntAdder : ICombiner<int>
    {
        public IntAdder()
        {

        }

        public int Combine(int a, int b)
        {
            return a + b;
        }

        public int Default()
        {
            return 0;
        }
    }
}
