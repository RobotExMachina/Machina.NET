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
using Vec = Machina.Point;
using Rot = Machina.Rotation;

using Machina;

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

        int relSpeed = 50;
        double incDist = 5;

        Robot arm;

        bool firstPersonMode = true;  // fp mode does relative movement to x axis, absolute mode jsut from the outside...

        Vec dir = new Vec();
         
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
            arm.Mode("stream");
            arm.Connect();

            // Suscribe to events
            //arm.BufferEmpty += new BufferEmptyHandler(GenerateMovements);
            //arm.BufferEmpty += GenerateMovements;

            arm.Start();
            arm.Speed(relSpeed);
            arm.Zone(2);
            //arm.RotateTo(Machina.Rotation.FlippedAroundY);
            //arm.MoveTo(200, 200, 200);

            // initial quick to the stream queue
            arm.TransformTo(new Vec(300, 0, 500), Rot.FlippedAroundY);
            //arm.Rotate(Vec.ZAxis, -180);

            if (firstPersonMode) arm.Coordinates("local");
            

        }

        void TimerTick(object sender, EventArgs e)
        {
            //DisplayControllerInformation();

            var state = gameController.GetState();

            //Console.WriteLine("LEFT THUMB X:{0}, Y:{1}", state.Gamepad.LeftThumbX, state.Gamepad.LeftThumbY);
            //Console.WriteLine("RIGHT THUMB X:{0}, Y:{1}", state.Gamepad.RightThumbX, state.Gamepad.RightThumbY);
            //Console.WriteLine("TRIGGGERS LEFT:{0}, RIGHT:{1}", state.Gamepad.LeftTrigger, state.Gamepad.RightTrigger);
            //Console.WriteLine("BUTTONS: " + state.Gamepad.Buttons);

            // Can't really test this right now, my controller's thumb is broken... XD
            //Machina.Point dir = new Machina.Point(
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

            Rot r = new Rot();
            GamepadButtonFlags buttons = state.Gamepad.Buttons;

            //// Create direction vector based on buttons
            //if (buttons.HasFlag(GamepadButtonFlags.DPadRight))
            //    dir.Y = 1;
            //else if (buttons.HasFlag(GamepadButtonFlags.DPadLeft))
            //    dir.Y = -1;
            //else
            //    dir.Y = 0;

            //if (buttons.HasFlag(GamepadButtonFlags.DPadUp))
            //    dir.X = 1;
            //else if (buttons.HasFlag(GamepadButtonFlags.DPadDown))
            //    dir.X = -1;
            //else
            //    dir.X = 0;

            //if (buttons.HasFlag(GamepadButtonFlags.Y))
            //    dir.Z = 1;
            //else if (buttons.HasFlag(GamepadButtonFlags.A))
            //    dir.Z = -1;
            //else
            //    dir.Z = 0;

            if (buttons.HasFlag(GamepadButtonFlags.Back))
            {
                firstPersonMode = !firstPersonMode;
                if (firstPersonMode) arm.Coordinates("local");
                else arm.Coordinates("global");
                Console.WriteLine("COordinatesMode: " + arm.Coordinates());
            }

            if (buttons.HasFlag(GamepadButtonFlags.Start))
            {
                DebugStates();
            }

            dir.X = NormalizeThumb(state.Gamepad.LeftThumbY, 10000, 32767);
            dir.Y = NormalizeThumb(state.Gamepad.LeftThumbX, 10000, 32767);
            dir.Z = NormalizeThumb(state.Gamepad.LeftTrigger, 25, 255) - NormalizeThumb(state.Gamepad.RightTrigger, 25, 255);

            double rotZ = NormalizeThumb(state.Gamepad.RightThumbX, 10000, 32767);
            r = new Rot(Vec.ZAxis, 0.5 * incDist * rotZ);
            double rotY = NormalizeThumb(state.Gamepad.RightThumbY, 10000, 32767);
            r.Multiply(new Rot(Vec.YAxis, -0.5 * incDist * rotY));
            // Issue a move command to the robot accordingly (or not)
            if (dir.Length() > 0)
            {
                //dir.Normalize();
                dir.Scale(incDist);

                Console.WriteLine("  --> Moving and rotating {0} {1}", dir, r);
                //arm.Transform(r, dir);
                arm.Move(dir);
            }
            else if (r.W < 1)
            {
                Console.WriteLine("  --> Rotating {0}", r);
                arm.Rotate(r);
            }
            else 
            {
                //Console.WriteLine("Idle...");
            }
           

        }

        public void DebugStates()
        {
            arm.DebugRobotCursors();
            Console.WriteLine("Pos: " + arm.GetCurrentPosition());
            Console.WriteLine("Ori: " + arm.GetCurrentOrientation());
            Console.WriteLine("Jnt: " + arm.GetCurrentJoints());
        }


        public void GenerateMovements(object sender, EventArgs args)
        {
            if (firstPersonMode)
            {

            }
            else
            {
                Console.WriteLine("  --> Moving {0}", dir);
                //arm.Move(dir);
            }

        }



        private void MainWindowClosing(object sender, CancelEventArgs e)
        {
            // Robot
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
