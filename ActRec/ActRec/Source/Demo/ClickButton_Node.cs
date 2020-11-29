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
            ButtonClicker.Instance.ClickButton(this.viewManager.FindButton(this.buttonName));
        }

        public override string ToString()
        {
            return "Click button '" + this.buttonName + "'";
        }

        private string buttonName;
        private ViewManager viewManager;
    }
}
