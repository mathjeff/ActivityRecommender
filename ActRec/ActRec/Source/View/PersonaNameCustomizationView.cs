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

            TitledTextbox personaName_holder = new TitledTextbox("My Name", this.personaName_box);
            this.SubLayout = personaName_holder;
        }

        public void OnBack(LayoutChoice_Set previousLayout)
        {
            this.UpdatePersona();
        }

        private void UpdatePersona()
        {
            this.persona.Name = this.personaName_box.Text;
        }

        private Persona persona;
        private Editor personaName_box;
    }
}
