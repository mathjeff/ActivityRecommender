using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;

namespace ActivityRecommendation.Demo
{
    class ClickButton_Node : WorkflowNode
    {
        public ClickButton_Node(string name, ViewManager viewManager)
        {
            this.buttonName = name;
            this.viewManager = viewManager;
        }

        public void Process()
        {
            System.Diagnostics.Debug.WriteLine("Trying to click " + this.buttonName);
            Button button = this.viewManager.FindButton(this.buttonName);
            ButtonClicker.Instance.ClickButton(button);
            System.Diagnostics.Debug.WriteLine("Clicked button named " + this.buttonName + ": " + button);
        }

        public override string ToString()
        {
            return "Click button '" + this.buttonName + "'";
        }

        private string buttonName;
        private ViewManager viewManager;
    }
}
