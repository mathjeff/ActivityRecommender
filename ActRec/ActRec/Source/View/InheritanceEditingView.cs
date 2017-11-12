using System;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation
{
    class InheritanceEditingView : TitledControl
    {
        public event InheritanceHandler Submit;
        public delegate void InheritanceHandler(object sender, Inheritance inheritance);


        public InheritanceEditingView(ActivityDatabase activityDatabase, LayoutStack layoutStack, bool makeNewChild)
        {
            this.layoutStack = layoutStack;
            this.makeNewChild = makeNewChild;

            if (makeNewChild)
                this.SetTitle("Add New Activity Choose From");
            else
                this.SetTitle("Add Existing Activity as Child of Another Activity");

            GridLayout content = GridLayout.New(BoundProperty_List.Uniform(2), BoundProperty_List.Uniform(2), LayoutScore.Zero);

            this.childNameBox = new ActivityNameEntryBox("Activity Name", makeNewChild);
            this.childNameBox.Database = activityDatabase;
            content.AddLayout(this.childNameBox);

            this.parentNameBox = new ActivityNameEntryBox("Parent Name");
            this.parentNameBox.Database = activityDatabase;
            content.AddLayout(this.parentNameBox);

            this.okButton = new Button();
            this.okButton.Clicked += OkButton_Clicked;
            content.AddLayout(new LayoutCache(new ButtonLayout(this.okButton, "OK")));


            LayoutChoice_Set helpWindow = (new HelpWindowBuilder()).AddMessage("This page is for you to enter activities, to use as future suggestions")
                .AddMessage("The text box on the left is where you type the activity name.")
                .AddMessage("The text box on the right is where you type another activity that you want to make a supercategory of the activity.")
                .AddMessage("For example, you might specify that Exercise is a subcategory of Useful")
                .AddMessage("While typing you can press Enter to fill in the autocomplete suggestion.")
                .Build();

            HelpButtonLayout helpLayout = new HelpButtonLayout(helpWindow, layoutStack);

            content.AddLayout(helpLayout);

            this.SetContent(new LayoutCache(content));
        }

        private void OkButton_Clicked(object sender, EventArgs e)
        {
            if (this.Submit != null)
            {
                ActivityDescriptor childDescriptor = this.childNameBox.ActivityDescriptor;
                if (!this.makeNewChild && this.childNameBox.Activity == null)
                    return; // invalid
                ActivityDescriptor parentDescriptor = this.parentNameBox.ActivityDescriptor;
                if (this.parentNameBox.Activity == null)
                    return; // invalid
                Inheritance inheritance = new Inheritance(parentDescriptor, childDescriptor);
                this.Submit.Invoke(this, inheritance);
                this.childNameBox.NameText = "";
                this.parentNameBox.NameText = "";
            }
        }

        public string ChildName
        {
            get
            {
                return this.childNameBox.NameText;
            }
            set
            {
                this.childNameBox.NameText = value;
            }
        }
        public string ParentName
        {
            get
            {
                return this.parentNameBox.NameText;
            }
            set
            {
                this.parentNameBox.NameText = value;
            }
        }
        private ActivityNameEntryBox childNameBox;
        private ActivityNameEntryBox parentNameBox;
        private Button okButton;
        private LayoutStack layoutStack;
        private bool makeNewChild;
    }
}
