using System;
using System.Collections.Generic;
using System.Text;
using VisiPlacement;

namespace ActivityRecommendation.Demo
{
    public class PressButton_Node : WorkflowNode
    {
        public PressButton_Node(string name, ViewManager viewManager)
        {
            this.buttonName = name;
            this.viewManager = viewManager;
        }

        public void Process()
        {
            ButtonClicker.Instance.MakeButtonAppearPressed(this.viewManager.FindButton(this.buttonName));
        }


        public override string ToString()
        {
            return "Press button '" + this.buttonName + "'";
        }

        private string buttonName;
        private ViewManager viewManager;
    }
}
