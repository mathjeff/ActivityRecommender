using System;
using System.Collections.Generic;
using System.Text;

namespace ActivityRecommendation
{
    // a Persona describes the way that ActivityRecommender talks to the user
    public class Persona
    {
        public delegate void PersonaNameChanged(string newName);
        public event PersonaNameChanged NameChanged;

        // The name that ActivityRecommender uses to refer to itself
        public String Name
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
        private string name = "ActivityRecommender";
    }
}
