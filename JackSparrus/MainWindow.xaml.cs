using JackSparrus.State;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JackSparrus
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public TreasureHub TreasureHub
        {
            get;
            private set;
        }

        public WebManager WebManager
        {
            get;
            private set;
        }

        public MainWindow()
        {
            WindowManager.InitWindowManager();
            Closing += new CancelEventHandler(MainWindow_Closing);

            InitializeComponent();

            this.TreasureHub = new TreasureHub();
            this.WebManager = new WebManager();
        }

        void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            this.WebManager.CloseDriver();
        }

        public void UpdateHubArray()
        {
            this.cTreasureHub.Dispatcher.Invoke(new Action(() => this.cTreasureHub.ItemsSource = new List<TreasureRow>(this.TreasureHub.Rows)));
        }

        public void ResetGoButton()
        {
            this.cTreasureHub.Dispatcher.Invoke(new Action(() => this.cGo_button.IsEnabled = true));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TreasureHuntState treasureHuntState = new TreasureHuntState();
            treasureHuntState.StartState = "getHint";

            this.cGo_button.IsEnabled = false;

            Task.Factory.StartNew(() => treasureHuntState.RunState(this));
        }
    }
}
