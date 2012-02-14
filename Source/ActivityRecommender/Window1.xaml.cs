using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ActivityRecommendation
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(Window1_Loaded);
            this.Loaded += new RoutedEventHandler(Window1_Loaded2);
            this.Closed += new EventHandler(Window1_Closed);
        }

        void Window1_Closed(object sender, EventArgs e)
        {
            this.recommender.ShutDown();
        }

        // gets called when the window loads
        void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            this.recommender = new ActivityRecommender(this);
            this.WindowState = WindowState.Maximized;
        }
        // gets called when the window loads but after Window1_Loaded
        void Window1_Loaded2(object sender, EventArgs e)
        {
            this.recommender.PrepareEngine();
        }

        private ActivityRecommender recommender;
    }
}
