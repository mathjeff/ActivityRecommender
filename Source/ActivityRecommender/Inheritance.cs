using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActivityRecommendation
{
    public class Inheritance
    {
        public Inheritance()
        {
        }
        public Inheritance(ActivityDescriptor parent, ActivityDescriptor child)
        {
            this.parentDescriptor = parent;
            this.childDescriptor = child;
            this.Weight = 1;
        }
        public ActivityDescriptor ParentDescriptor
        {
            get
            {
                return this.parentDescriptor;
            }
            set
            {
                this.parentDescriptor = value;
            }
        }
        public ActivityDescriptor ChildDescriptor
        {
            get
            {
                return this.childDescriptor;
            }
            set
            {
                this.childDescriptor = value;
            }
        }
        public DateTime DiscoveryDate { get; set; }
        public double Weight { get; set; }
        private ActivityDescriptor parentDescriptor;
        private ActivityDescriptor childDescriptor;
    }
}
