using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

// the SelectorView shows a simple list of RadioButtons, each with text, and allows the user to select one
namespace ActivityRecommendation
{
    class SelectorView : DisplayGrid, IResizable
    {
        public SelectorView(List<string> titles)
            :base(titles.Count, 1)
        {
            List<object> items = new List<object>();
            foreach (string title in titles)
            {
                items.Add(title);
            }
            this.Initialize(items);
        }
        public SelectorView(List<object> items)
            :base(items.Count, 1)
        {
            this.Initialize(items);
        }
        private void Initialize(List<object> items)
        {
            // create the buttons
            foreach (object item in items)
            {
                RadioButton newButton = new ResizableRadioButton();
                newButton.Content = item;
                newButton.HorizontalAlignment = HorizontalAlignment.Center;
                newButton.Click += this.ButtonClick;
                if (this.selectedButton == null)
                {
                    this.selectedButton = newButton;
                    this.selectedButton.IsChecked = true;
                }
                else
                {
                    newButton.IsChecked = false;
                }
                this.AddItem(newButton);
            }
            this.eventHandlers = new List<RoutedEventHandler>();
        }
        public override Size PreliminaryMeasure(Size constraint)
        {
            Size result = base.PreliminaryMeasure(constraint);
            return result;
        }
        protected override Size MeasureOverride(Size constraint)
        {
            Size result = base.MeasureOverride(constraint);
            return result;
        }
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            Size result = base.ArrangeOverride(arrangeSize);
            return result;
        }
        public object SelectedItem
        {
            get
            {
                return this.selectedButton.Content;
            }
        }
        public string SelectedItemText
        {
            get
            {
                return (string)this.selectedButton.Content;
            }
        }
        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            this.selectedButton.IsChecked = false;
            RadioButton converted = sender as RadioButton;
            converted.IsChecked = true;
            this.selectedButton = converted;

            foreach (RoutedEventHandler handler in this.eventHandlers)
            {
                handler.Invoke(sender, e);
            }
        }
        public void AddClickHandler(RoutedEventHandler h)
        {
            this.eventHandlers.Add(h);
        }

        private RadioButton selectedButton;
        List<RoutedEventHandler> eventHandlers;


    }
}
