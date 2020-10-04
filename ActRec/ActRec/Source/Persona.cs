using System;
using System.Collections.Generic;
using System.Text;

namespace ActivityRecommendation
{
    // a Persona describes the way that ActivityRecommender talks to the user and what ActivityRecommender looks like
    // TODO: Should this class be renamed to something like Style?
    public class Persona
    {
        public delegate void PersonaNameChanged(string newName);
        public event PersonaNameChanged NameChanged;

        // The name that ActivityRecommender uses to refer to itself
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                if (value != this.name)
                {
                    this.name = value;
                    if (this.NameChanged != null)
                        this.NameChanged.Invoke(this.name);
                }
            }
        }
        public string LayoutDefaults_Name
        {
            get
            {
                return this.layoutDefaults_name;
            }
            set
            {
                this.layoutDefaults_name = value;
            }
        }
        private string name = "ActivityRecommender";
        private string layoutDefaults_name;
    }
}
