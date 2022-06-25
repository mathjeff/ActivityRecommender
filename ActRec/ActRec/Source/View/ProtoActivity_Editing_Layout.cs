using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisiPlacement;
using Xamarin.Forms;

namespace ActivityRecommendation.View
{
    // Allows creating or updating a ProtoActivity
    public class ProtoActivity_Editing_Layout : ContainerLayout, OnBack_Listener
    {
        public ProtoActivity_Editing_Layout(ProtoActivity protoActivity, ProtoActivity_Database database, ActivityDatabase activityDatabase, LayoutStack layoutStack)
        {
            this.protoActivity = protoActivity;
            this.database = database;
            this.activityDatabase = activityDatabase;
            this.activityDatabase.ActivityAdded += ActivityDatabase_ActivityAdded;
            this.layoutStack = layoutStack;
            this.textBox = new Editor();
            this.textBox.Text = protoActivity.Text;

            Vertical_GridLayout_Builder gridBuilder = new Vertical_GridLayout_Builder();
            string titleText;
            if (protoActivity.Id >= 0)
                titleText = "ProtoActivity #" + protoActivity.Id;
            else
                titleText = "New ProtoActivity";
            gridBuilder.AddLayout(new TextblockLayout(titleText));
            gridBuilder.AddLayout(ScrollLayout.New(new TextboxLayout(this.textBox)));

            Button saveButton = new Button();
            saveButton.Clicked += SaveButton_Clicked;

            Button promoteButton = new Button();
            promoteButton.Clicked += PromoteButton_Clicked;

            Button splitButton = new Button();

            HelpButtonLayout helpButtonLayout = new HelpButtonLayout(new HelpWindowBuilder()
                .AddMessage("This screen allows you to edit " + titleText + ".")
                .AddMessage("Enter as much text as you like.")
                .AddMessage("To save your entry, either press Save or simply go back to another screen.")
                .AddMessage("  After you go back, if you later want to return to this same ProtoActivity, you will have to go to one of the screens for browsing existing ProtoActivities and then search for it.")
                .AddMessage("To turn this ProtoActivity into an Activity, press Promote to Activity.")
                .AddMessage("To delete this ProtoActivity, delete all of the text in the box and then either press Save or go back to a previous screen.")
                .AddLayout(new CreditsButtonBuilder(layoutStack)
                    .AddContribution(ActRecContributor.AARON_SMITH, new DateTime(2019, 9, 16), "Pointed out that the promote-a-protoactivity button crashed if the text box was empty")
                    .Build()
                )
                .Build()
                , layoutStack);

            LayoutChoice_Set buttonsLayout = new Horizontal_GridLayout_Builder()
                .AddLayout(new ButtonLayout(saveButton, "Save"))
                .AddLayout(new ButtonLayout(promoteButton, "Promote to Activity"))
                .AddLayout(new ButtonLayout(splitButton, "Split"))
                .AddLayout(helpButtonLayout)
                .BuildAnyLayout();
            gridBuilder.AddLayout(buttonsLayout);

            this.SubLayout = gridBuilder.Build();
        }

        private void SplitButton_Clicked(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            ProtoActivity child1 = new ProtoActivity(this.textBox.Text, now, this.protoActivity.Ratings);
            ProtoActivity child2 = new ProtoActivity(this.textBox.Text, now, this.protoActivity.Ratings);
            this.textBox.Text = null;
            this.skipWhenGoingBack = true;

            ProtoActivity_Splitting_Layout splitter = new ProtoActivity_Splitting_Layout(child1, child2, this.database, this.layoutStack);
            StackEntry entry = new StackEntry(splitter, "Split", splitter);
            entry.Listeners.Add(this);
            this.layoutStack.AddLayout(entry);
        }

        private void SaveButton_Clicked(object sender, EventArgs e)
        {
            // We don't want the protoactivity in the text file to differ from the protoactivity in memory,
            // so we save the protoactivity whenever we go back.
            // Also, once a user saves the protoactivity, they probably want to go back.
            // So, the save button just goes back
            this.layoutStack.GoBack();
        }

        private void PromoteButton_Clicked(object sender, EventArgs e)
        {
            this.promote();
        }

        private void promote()
        {
            string text = this.textBox.Text;
            int maxLength = 120;
            if (text == null || text == "")
                return;
            if (text.Length > maxLength)
            {
                this.layoutStack.AddLayout(new TextblockLayout("The text of this Protoactivity is too long (" + text.Length + " " +
                    "characters which is more than the limit of " + maxLength + ") to automatically promote into an Activity. " +
                    "It's better to have a short Activity name because:\n" +
                    "#1 It will have to fit onscreen and\n" +
                    "#2 It will be saved to your device's storage every time you record having done it."), "Name too long");
            }
            else
            {
                ActivityCreationLayout creationLayout = new ActivityCreationLayout(this.activityDatabase, this.layoutStack);
                creationLayout.SelectedActivityTypeIsToDo = true;
                creationLayout.ActivityName = this.textBox.Text;
                this.layoutStack.AddLayout(new StackEntry(creationLayout, "Proto", this));
            }
        }

        private void ActivityDatabase_ActivityAdded(Activity activity)
        {
            if (this.layoutStack.Contains(this))
            {
                this.delete();
                this.persist();
                this.skipWhenGoingBack = true;
            }
        }

        private void DeleteButton_Clicked(object sender, EventArgs e)
        {
            this.delete();
        }
        private void delete()
        {
            this.textBox.Text = null;
        }

        public void OnBack(LayoutChoice_Set layout)
        {
            if (layout == this)
            {
                this.OnHide();
            }
            else
            {
                this.OnReturnToHere();
            }
        }
        private void OnHide()
        {
            this.persist();
        }
        private void OnReturnToHere()
        {
            if (this.skipWhenGoingBack)
            {
                this.layoutStack.RemoveLayout();
            }
        }
        private void persist()
        {
            if (this.protoActivity.Text != this.textBox.Text)
            {
                this.protoActivity.Text = this.textBox.Text;
                this.protoActivity.LastInteractedWith = DateTime.Now;
                if (this.protoActivity.Text != null && this.protoActivity.Text != "")
                    this.database.Put(this.protoActivity);
                else
                    this.database.Remove(this.protoActivity);
            }
        }
        private ProtoActivity protoActivity;
        private ProtoActivity_Database database;
        private Editor textBox;
        private ActivityDatabase activityDatabase;
        private LayoutStack layoutStack;
        private bool skipWhenGoingBack;
    }
}
