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
        private int refreshRate = 33;  // in millis

        int relSpeed = 100;
        double incDist = 25;

        Robot arm;
         
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

            // Robot stuff
            arm = new Robot();
            arm.ControlMode("stream");
            arm.Connect();

            arm.Start();
            arm.Speed(relSpeed);
            arm.Zone(2);
            //arm.RotateTo(RobotControl.Rotation.FlippedAroundY);
            //arm.MoveTo(200, 200, 200);

        }

        void TimerTick(object sender, EventArgs e)
        {
            //DisplayControllerInformation();

            var state = gameController.GetState();
            
            //Console.WriteLine("LEFT THUMB X:{0}, Y:{1}", state.Gamepad.LeftThumbX, state.Gamepad.LeftThumbY);
            //Console.WriteLine("RIGHT THUMB X:{0}, Y:{1}", state.Gamepad.RightThumbX, state.Gamepad.RightThumbY);
            //Console.WriteLine("BUTTONS: " + state.Gamepad.Buttons);

            // Can't really test this right now, my controller's thumb is broken... XD
            //RobotControl.Point dir = new RobotControl.Point(
            //    RemapThumb(state.Gamepad.LeftThumbX, 10000, 32767),
            //    RemapThumb(state.Gamepad.LeftThumbY, 10000, 32767),
            //    RemapThumb(state.Gamepad.RightThumbY, 10000, 32767)
            //);
            //dir.Scale(relSpeed);

            //var speed = dir.Length();
            //if (speed > 0)
            //{
            //    Console.WriteLine("--> Moving {0}", dir);
            //    arm.SetVelocity(speed);
            //    arm.Move(dir);
            //}
            //else
            //{
            //    Console.WriteLine("idle");
            //}

            // A really simple and fast implementation to create a direction vector 
            // for the next issued movement based on button pressed states. 
            // This queues a ton of Frames on the robot, would be much better 
            // implemented when the queue raises events demanding new targets.
            // Directions are assuming a human controller facing the robot frontally.

            RobotControl.Point dir = new RobotControl.Point(0, 0, 0);
            GamepadButtonFlags buttons = state.Gamepad.Buttons;

            // Create direction vector based on buttons
            if (buttons.HasFlag(GamepadButtonFlags.DPadRight))
                dir.Y = 1;
            else if (buttons.HasFlag(GamepadButtonFlags.DPadLeft))
                dir.Y = -1;

            if (buttons.HasFlag(GamepadButtonFlags.DPadUp))
                dir.X = -1;
            else if (buttons.HasFlag(GamepadButtonFlags.DPadDown))
                dir.X = 1;

            if (buttons.HasFlag(GamepadButtonFlags.Y))
                dir.Z = 1;
            else if (buttons.HasFlag(GamepadButtonFlags.A))
                dir.Z = -1;

            // Issue a move command to the robot accordingly (or not)
            if (dir.Length() > 0)
            {
                dir.Normalize();
                dir.Scale(incDist);

                Console.WriteLine("--> Moving {0}", dir);
                arm.MoveGlobal(dir);
            }
            else
            {
                Console.WriteLine("Idle...");
            }
           
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


        private double NormalizeThumb(short value, double min, double max)
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
