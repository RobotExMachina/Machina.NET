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

using RobotControl;

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

        Robot arm;
        double relSpeed = 25;  // 
         
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

            arm = new Robot();
            arm.ConnectionMode("online");
            arm.OnlineMode("stream");
            arm.Connect();

            arm.Start();
            arm.SetVelocity(100);
            arm.SetZone(5);
            arm.RotateTo(RobotControl.Rotation.FlippedAroundY);
            arm.MoveTo(200, 200, 200);

        }

        void TimerTick(object sender, EventArgs e)
        {
            //DisplayControllerInformation();

            var state = gameController.GetState();
            //Console.WriteLine("LEFT THUMB X:{0}, Y:{1}", state.Gamepad.LeftThumbX, state.Gamepad.LeftThumbY);
            //Console.WriteLine("RIGHT THUMB X:{0}, Y:{1}", state.Gamepad.RightThumbX, state.Gamepad.RightThumbY);
            //Console.WriteLine("BUTTONS: " + state.Gamepad.Buttons);

            RobotControl.Point dir = new RobotControl.Point(
                RemapThumb(state.Gamepad.LeftThumbX, 10000, 32767), 
                RemapThumb(state.Gamepad.LeftThumbY, 10000, 32767), 
                RemapThumb(state.Gamepad.RightThumbY, 10000, 32767)
            );
            dir.Scale(relSpeed);
            
            var speed = dir.Length();
            if (speed > 0)
            {
                Console.WriteLine("--> Moving {0}", dir);
                arm.SetVelocity(speed);
                arm.Move(dir);
            }
            else
            {
                Console.WriteLine("idle");
            }
            

            //dir.Normalize();
            //Console.WriteLine("--> Norm mov {0}", dir);
        }

        private void MainWindowClosing(object sender, CancelEventArgs e)
        {
            arm.Disconnect();

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


        private double RemapThumb(short value, double min, double max)
        {
            bool neg = value < 0;
            int intVal = (int) value;
            double val = (int) Math.Abs(intVal);
            if (val <= min) return 0;
            double norm = (val - min) / (max - min);
            return neg ? -norm : norm;
        }


    }
}
