using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;

namespace ActivityRecommendation.View
{
    class CustomizeLayout : ContainerLayout
    {
        public CustomizeLayout(PersonaNameCustomizationView nameLayout, Choose_LayoutDefaults_Layout themeLayout, Persona persona, LayoutStack layoutStack)
        {
            this.nameLayout = new PersonaNameCustomizationView(persona);
            this.nameLayout = nameLayout;
            this.themeLayout = themeLayout;
            this.themeFeatures = new List<AppFeature>() { new ChangeTheme_Feature(persona) };
            this.SubLayout = new MenuLayoutBuilder(layoutStack)
                .AddLayout(
                    new AppFeatureCount_ButtonName_Provider("Name Me", this.nameLayout.GetFeatures()),
                    new StackEntry(this.nameLayout, "Name Me", this.nameLayout)
                )
                .AddLayout(
                    new AppFeatureCount_ButtonName_Provider("Change Colors", themeFeatures),
                    new StackEntry(this.themeLayout, "Change Colors", null)
                )
                .Build();
        }
        public List<AppFeature> GetFeatures()
        {
            List<AppFeature> features = new List<AppFeature>(this.themeFeatures);
            features.AddRange(this.nameLayout.GetFeatures());
            return features;
        }
        PersonaNameCustomizationView nameLayout;
        Choose_LayoutDefaults_Layout themeLayout;
        List<AppFeature> themeFeatures;
    }
}
