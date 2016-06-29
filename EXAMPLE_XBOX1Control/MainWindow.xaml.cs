// https://elbruno.com/2014/06/28/coding4fun-xboxone-game-controller-c-fun-time-2/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

/// <summary>
/// PM> Install-Package SharpDX.XInput -Version 2.6.2"
/// </summary>
using SharpDX.XInput;


namespace EXAMPLE_XBOX1Control
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer = new DispatcherTimer();
        private Controller gameController;
        private int refreshRate = 100;  // in millis
         
        public MainWindow()
        {
            // For the WPF context
            DataContext = this;

            // Handle window events
            Loaded += MainWindowLoaded;
            Closing += MainWindowClosing;

            // Go for it!
            InitializeComponent();

            // Periodic tick
            timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(refreshRate) };
            timer.Tick += TimerTick;
            timer.Start();

        }

        void TimerTick(object sender, EventArgs e)
        {
            //DisplayControllerInformation();

            var state = gameController.GetState();
            Console.WriteLine("LEFT THUMB X:{0}, Y:{1}", state.Gamepad.LeftThumbX, state.Gamepad.LeftThumbY);
            Console.WriteLine("RIGHT THUMB X:{0}, Y:{1}", state.Gamepad.RightThumbX, state.Gamepad.RightThumbY);
            Console.WriteLine("BUTTONS: " + state.Gamepad.Buttons);
        }

        private void MainWindowClosing(object sender, CancelEventArgs e)
        {
            gameController = null;
        }

        private void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            gameController = new Controller(UserIndex.One);
            Console.WriteLine("CONNECTED: " + gameController.IsConnected);
            if (gameController.IsConnected) return;
            MessageBox.Show("Gameroller is not connected ... you know ;)");
            App.Current.Shutdown();
        }



    }
}
