using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    class PersonaNameCustomizationView : ContainerLayout, OnBack_Listener
    {
        public PersonaNameCustomizationView(UserSettings persona)
        {
            this.persona = persona;

            this.personaName_box = new Editor();
            this.personaName_box.Text = persona.PersonaName;

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
            this.persona.PersonaName = this.personaName_box.Text;
        }

        private UserSettings persona;
        private Editor personaName_box;
    }

    class PersonaNameCustomizationFeature : AppFeature
    {
        public PersonaNameCustomizationFeature(UserSettings persona)
        {
            this.persona = persona;
        }
        public string GetDescription()
        {
            return "Change my name";
        }
        public bool GetHasBeenUsed()
        {
            return this.persona.PersonaName != "ActivityRecommender";
        }
        public bool GetIsUsable()
        {
            return true;
        }

        UserSettings persona;
    }

    class ChangeTheme_Feature : AppFeature
    {
        public ChangeTheme_Feature(UserSettings persona)
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

        public bool GetIsUsable()
        {
            return true;
        }

        UserSettings persona;
    }


}
