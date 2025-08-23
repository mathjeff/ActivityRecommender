using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using SkiaSharp.Views.Maui.Controls.Hosting;
using System.Reflection;
using VisiPlacement;

namespace ActRec.Droid
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            AppParams parameters = new AppParams(version, new LogcatReader());
            App.AppParams = parameters;

            builder.UseMauiApp<App>();
            builder.UseSkiaSharp();
            builder.ConfigureEffects(effects =>
            {
                effects.Add<RoutingEffect, ActRec.Droid.ButtonEffect>();
            });

            EffectFactory.Instance.RegisterEffect("ActRec.ButtonEffect", new ConstructorProvider<ButtonEffect, Effect>());


            return builder.Build();
        }
    }
}
