using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    class PersonaNameCustomizationView : ContainerLayout, OnBack_Listener
    {
        public PersonaNameCustomizationView(Persona persona)
        {
            this.persona = persona;

            this.personaName_box = new Editor();
            this.personaName_box.Text = persona.Name;

            TitledTextbox personaName_holder = new TitledTextbox("Hello! I'm your ActivityRecommender. My name is:", this.personaName_box);
            this.SubLayout = personaName_holder;
        }

        public void OnBack(LayoutChoice_Set previousLayout)
        {
            this.UpdatePersona();
        }

        public List<AppFeature> GetFeatures()
        {
            return new List<AppFeature>() { new PersonaNameCustomizationFeature(this.persona) };
        }

        private void UpdatePersona()
        {
            this.persona.Name = this.personaName_box.Text;
        }

        private Persona persona;
        private Editor personaName_box;
    }

    class PersonaNameCustomizationFeature : AppFeature
    {
        public PersonaNameCustomizationFeature(Persona persona)
        {
            this.persona = persona;
        }
        public string GetDescription()
        {
            return "Change my name";
        }
        public bool GetHasBeenUsed()
        {
            return this.persona.Name != "ActivityRecommender";
        }

        Persona persona;
    }

    class ChangeTheme_Feature : AppFeature
    {
        public ChangeTheme_Feature(Persona persona)
        {
            this.persona = persona;
        }
        public string GetDescription()
        {
            return "Change colors";
        }
        public bool GetHasBeenUsed()
        {
            return this.persona.LayoutDefaults_Name != null;
        }

        Persona persona;
    }


}
