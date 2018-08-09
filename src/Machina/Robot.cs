using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Json;


//  ███╗   ███╗ █████╗  ██████╗██╗  ██╗██╗███╗   ██╗ █████╗ 
//  ████╗ ████║██╔══██╗██╔════╝██║  ██║██║████╗  ██║██╔══██╗
//  ██╔████╔██║███████║██║     ███████║██║██╔██╗ ██║███████║
//  ██║╚██╔╝██║██╔══██║██║     ██╔══██║██║██║╚██╗██║██╔══██║
//  ██║ ╚═╝ ██║██║  ██║╚██████╗██║  ██║██║██║ ╚████║██║  ██║
//  ╚═╝     ╚═╝╚═╝  ╚═╝ ╚═════╝╚═╝  ╚═╝╚═╝╚═╝  ╚═══╝╚═╝  ╚═╝
//                                                          

namespace Machina
{

    //public static class Log
    //{
    //    static bool _consoleDump = true;
    //    static int _debugLevel = 3;

    //    internal static void Debug(string msg)
    //    {

    //        if (_consoleDump && -_debugLevel >= 4)
    //        {
    //            Console.WriteLine("MACHINA DEBUG: " + msg);
    //        }
    //    }


    //}


    //  ██████╗  ██████╗ ██████╗  ██████╗ ████████╗
    //  ██╔══██╗██╔═══██╗██╔══██╗██╔═══██╗╚══██╔══╝
    //  ██████╔╝██║   ██║██████╔╝██║   ██║   ██║   
    //  ██╔══██╗██║   ██║██╔══██╗██║   ██║   ██║   
    //  ██║  ██║╚██████╔╝██████╔╝╚██████╔╝   ██║   
    //  ╚═╝  ╚═╝ ╚═════╝ ╚═════╝  ╚═════╝    ╚═╝   
    //                                             
    /// <summary>
    /// The core Class in Machina. Represents a state and action-based virtual robot, 
    /// and exposes the public API for robot manipulation and control.
    /// </summary>
    public class Robot
    {
        /// <summary>
        /// Build number.
        /// </summary>
        public static readonly int Build = 1410;

        /// <summary>
        /// Version number.
        /// </summary>
        public static readonly string Version = "0.7.0." + Build;

        /// <summary>
        /// A nickname for this Robot.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// What brand of robot is this?
        /// </summary>
        public RobotType Brand { get; internal set; }

        /// <summary>
        /// The main Control object, acts as an interface to all classes that
        /// manage robot control.
        /// </summary>
        private Control c;  // the main control object











        //  ██████╗ ██╗   ██╗██████╗ ██╗     ██╗ ██████╗     █████╗ ██████╗ ██╗
        //  ██╔══██╗██║   ██║██╔══██╗██║     ██║██╔════╝    ██╔══██╗██╔══██╗██║
        //  ██████╔╝██║   ██║██████╔╝██║     ██║██║         ███████║██████╔╝██║
        //  ██╔═══╝ ██║   ██║██╔══██╗██║     ██║██║         ██╔══██║██╔═══╝ ██║
        //  ██║     ╚██████╔╝██████╔╝███████╗██║╚██████╗    ██║  ██║██║     ██║
        //  ╚═╝      ╚═════╝ ╚═════╝ ╚══════╝╚═╝ ╚═════╝    ╚═╝  ╚═╝╚═╝     ╚═╝
        ///// <summary>
        ///// Base constructor.
        ///// </summary>                                                       
        [System.Obsolete("Deprecated constructor, defaults to a human-readable interpretation of the Actions. Please use Robot.Create(name, make) instead. Example: `Robot arm = Robot.Create(\"Machina\", \"ABB\");`")]
        public Robot() : this("Machina", "HUMAN") { }

        [System.Obsolete("Deprecated constructor, use Robot.Create(name, make) instead")]
        public Robot(string make) : this("Machina", make) { }

        /// <summary>
        /// Base constructor.
        /// </summary>
        /// <param name="name">A name for this Robot</param>
        /// <param name="make">The robot make. This will determine which drivers/compilers are used to manage it.</param>
        [System.Obsolete("Deprecated constructor, use Robot.Create(name, make) instead")]
        public Robot(string name, string make)
        {
            this.Name = name;
            RobotType rt;

            try
            {
                rt = (RobotType)Enum.Parse(typeof(RobotType), make, true);
                if (Enum.IsDefined(typeof(RobotType), rt))
                {
                    //Console.WriteLine("Converted '{0}' to {1}", make, rt.ToString());
                    this.Brand = rt;
                    c = new Control(this);
                }
                else
                {
                    Console.WriteLine("{0} is not a RobotType, please specify one of the following: ", make);
                    foreach (string str in Enum.GetNames(typeof(RobotType)))
                    {
                        Console.WriteLine(str.ToString());
                    }
                    this.Brand = RobotType.HUMAN;
                    c = new Control(this);
                }
            }
            catch
            {
                Console.WriteLine("{0} is not a RobotType, please specify one of the following: ", make);
                foreach (string str in Enum.GetNames(typeof(RobotType)))
                {
                    Console.WriteLine(str.ToString());
                }
                this.Brand = RobotType.HUMAN;
                c = new Control(this);
            }
        }

        internal Robot(string name, RobotType make)
        {
            this.Name = name;
            this.Brand = make;
            c = new Control(this);
        }

        /// <summary>
        /// Create a new instance of a Robot.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="make"></param>
        /// <returns></returns>
        static public Robot Create(string name, RobotType make)
        {
            return new Robot(name, make);
        }

        /// <summary>
        /// Create a new instance of a Robot.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="make"></param>
        /// <returns></returns>
        static public Robot Create(string name, string make)
        {
            RobotType rt;

            //try
            //{
            rt = (RobotType)Enum.Parse(typeof(RobotType), make, true);
            if (Enum.IsDefined(typeof(RobotType), rt))
            {
                return new Robot(name, rt);
            }
            else
            {
                Console.WriteLine("{0} is not a RobotType, please specify one of the following: ", make);
                foreach (string str in Enum.GetNames(typeof(RobotType)))
                {
                    Console.WriteLine(str.ToString());
                }
            }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex);
            //    Console.WriteLine("{0} is not a RobotType, please specify one of the following: ", make);
            //    foreach (string str in Enum.GetNames(typeof(RobotType)))
            //    {
            //        Console.WriteLine(str.ToString());
            //    }
            //}
            return null;
        }


        /// What was this even for? Exports checks?
        public bool IsBrand(string brandName)
        {
            return this.Brand.ToString().Equals(brandName, StringComparison.OrdinalIgnoreCase);
        }

        public bool IsBrand(RobotType brand)
        {
            return brand == this.Brand;
        }

        /// <summary>
        /// Configure how Actions are streamed to the controller.
        /// </summary>
        /// <param name="minActionOnController">When Machina detects that the controller has these many Actions or less buffered, it will start streaming new Actions.</param>
        /// <param name="maxActionsOnController">When Maxhina detects that the controller has these many Actions or more buffered, it will stop streaming and wait for them to reach minActionOnController to stream more.</param>
        /// <returns></returns>
        public bool StreamConfiguration(int minActionOnController, int maxActionsOnController)
        {
            return c.ConfigureBuffer(minActionOnController, maxActionsOnController);
        }

        /// <summary>
        /// Sets the control mode the robot will operate under.
        /// </summary>
        /// <param name="controlType"></param>
        /// <returns></returns>
        public bool ControlMode(ControlType controlType)
        {
            return c.SetControlMode(controlType);
        }

        /// <summary>
        /// Sets the control type the robot will operate under, like "offline", "execute" or "stream".
        /// </summary>
        /// <param name="controlType"></param>
        /// <returns></returns>
        public bool ControlMode(string controlType)
        {
            ControlType ct;
            try
            {
                ct = (ControlType)Enum.Parse(typeof(ControlType), controlType, true);
                if (Enum.IsDefined(typeof(ControlType), ct))
                {
                    return c.SetControlMode(ct);
                }
            }
            catch
            {
                Console.WriteLine($"{controlType} is not a valid ControlMode type, please specify one of the following:");
                foreach (string str in Enum.GetNames(typeof(ControlType)))
                {
                    Console.WriteLine(str);
                }
            }
            return false;
        }

        /// <summary>
        /// Sets the cycle the robot will run program in (Once or Loop).
        /// </summary>
        /// <param name="cycleType"></param>
        /// <returns></returns>
        public bool CycleMode(CycleType cycleType)
        {
            //return c.SetRunMode(cycleType);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the cycle the robot will run program in (Once or Loop).
        /// </summary>
        /// <param name="cycleType"></param>
        public bool CycleMode(string cycleType)
        {
            //CycleType ct;
            //try
            //{
            //    ct = (CycleType)Enum.Parse(typeof(CycleType), cycleType, true);
            //    if (Enum.IsDefined(typeof(CycleType), ct))
            //        return c.SetRunMode(ct);
            //}
            //catch
            //{
            //    Console.WriteLine($"{cycleType} is not a valid CycleMode type, please specify one of the following:");
            //    foreach (string str in Enum.GetNames(typeof(CycleType)))
            //        Console.WriteLine(str);
            //}
            //return false;

            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets who will be in charge of managing the connection to the device,
        /// i.e. having "Machina" try to load a server/firmata modules to the controller or 
        /// leave that task to the "User" (default).
        /// </summary>
        /// <param name="connectionManager">"User" or "Machina"</param>
        /// <returns></returns>
        public bool ConnectionManager(string connectionManager)
        {
            ConnectionType cm;
            try
            {
                cm = (ConnectionType)Enum.Parse(typeof(ConnectionType), connectionManager, true);
                if (Enum.IsDefined(typeof(ConnectionType), cm))
                    return c.SetConnectionMode(cm);
            }
            catch
            {
                Console.WriteLine($"{connectionManager} is not a valid ConnectionManagerType type, please specify one of the following:");
                foreach (string str in Enum.GetNames(typeof(ConnectionType)))
                    Console.WriteLine(str);
            }
            return false;
        }

        /// <summary>
        /// Sets who will be in charge of managing the connection to the device,
        /// i.e. having "Machina" try to load a server/firmata modules to the controller or 
        /// leave that task to the "User" (default).
        /// </summary>
        /// <param name="connectionManager">"User" or "Machina"</param>
        /// <returns></returns>
        public bool ConnectionManager(ConnectionType connectionManager)
        {
            return c.SetConnectionMode(connectionManager);
        }

        /// <summary>
        /// If the controller needs special user logging, set the credentials here. 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool SetUser(string name, string password)
        {
            return c.SetUserCredentials(name, password);
        }

        /// <summary>
        /// Scans the network for robotic devices, real or virtual, and performs all necessary 
        /// operations to connect to it. This is necessary for 'online' modes such as 'execute' and 'stream.'
        /// </summary>
        /// <param name="robotId">If multiple devices are connected, choose this id from the list.</param>
        /// <returns></returns>
        public bool Connect(int robotId = 0)
        {
            return c.ConnectToDevice(robotId);
        }

        /// <summary>
        /// Tries to establish connection to a remote device for 'online' modes.
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool Connect(string ip, int port)
        {
            return c.ConnectToDevice(ip, port);
        }

        /// <summary>
        /// Performs all necessary instructions to disconnect from and dispose a robot device, real or virtual. 
        /// This is necessary before leaving current execution thread.
        /// </summary>
        public bool Disconnect()
        {
            return c.DisconnectFromDevice();
        }

        /// <summary>
        /// Returns a string representation of the IP of the currently connected robot device.
        /// </summary>
        /// <returns></returns>
        public string GetIP()
        {
            return c.GetControllerIP();
        }

        /// <summary>
        /// Loads a program to the robot from a local file.
        /// </summary>
        /// <param name="filepath">Full absolute filepath including root, directory structure, filename and extension.</param>
        /// <returns></returns>
        public bool LoadProgram(string filepath)
        {
            //return c.LoadProgramToDevice(filepath, true);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Loads a program to the robot from a string list of code.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool LoadProgram(List<string> code)
        {
            //return c.LoadProgramToDevice(code);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Starts execution of the current module/s in the controller.
        /// @TODO: The behavior of this method will change depending based on Off/Online mode
        /// </summary>
        public bool Start()
        {
            //return c.StartProgramOnDevice();
            throw new NotImplementedException();
        }

        /// <summary>
        /// Immediately stops execution of the current program/s in the connected robot. 
        /// </summary>
        public bool Stop()
        {
            //return c.StopProgramOnDevice(true);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create a program with all the buffered Actions and return it as a string List.
        /// Note all buffered Actions will be removed from the queue.
        /// </summary>
        /// <param name="inlineTargets">Write inline targets on action statements, or declare them as independent variables?</param>
        /// <param name="humanComments">If true, a human-readable description will be added to each line of code</param>
        /// <returns></returns>
        public List<string> Export(bool inlineTargets = true, bool humanComments = true)
        {
            return c.Export(inlineTargets, humanComments);
        }

        /// <summary>
        /// Create a program with all the buffered Actions and save it to a file. 
        /// Note all buffered Actions will be removed from the queue.
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="inlineTargets">Write inline targets on action statements, or declare them as independent variables?</param>
        /// <param name="humanComments">If true, a human-readable description will be added to each line of code</param>
        /// <returns></returns>
        public bool Export(string filepath, bool inlineTargets = true, bool humanComments = true)
        {
            return c.Export(filepath, inlineTargets, humanComments);
        }


        /// <summary>
        /// In 'execute' mode, flushes all pending Actions, creates a program, 
        /// uploads it to the controller and runs it.
        /// </summary>
        /// <returns></returns>
        public void Execute()
        {
            c.Execute();
        }

        ///// <summary>
        ///// ABB IOs must have a name corresponding to their definition in the controller. This function is useful to give them
        ///// a custom name that matches the controller's, and is used in code generation.
        ///// </summary>
        ///// <param name="ioName"></param>
        ///// <param name="pinNumber"></param>
        ///// <param name="isDigital"></param>
        //[System.Obsolete("Deprecated method, use string inputs on Robot.WriteDigital() instead")]
        //public bool SetIOName(string ioName, int pinNumber, bool isDigital)
        //{
        //    return c.SetIOName(ioName, pinNumber, isDigital);
        //}







        //  ███████╗███████╗████████╗████████╗██╗███╗   ██╗ ██████╗ ███████╗
        //  ██╔════╝██╔════╝╚══██╔══╝╚══██╔══╝██║████╗  ██║██╔════╝ ██╔════╝
        //  ███████╗█████╗     ██║      ██║   ██║██╔██╗ ██║██║  ███╗███████╗
        //  ╚════██║██╔══╝     ██║      ██║   ██║██║╚██╗██║██║   ██║╚════██║
        //  ███████║███████╗   ██║      ██║   ██║██║ ╚████║╚██████╔╝███████║
        //  ╚══════╝╚══════╝   ╚═╝      ╚═╝   ╚═╝╚═╝  ╚═══╝ ╚═════╝ ╚══════╝
        /// <summary>
        /// Gets the current speed setting.
        /// </summary>
        /// <returns></returns>
        [System.Obsolete("Deprecated method, use GetSpeed() instead")]
        public double Speed()
        {
            return c.GetCurrentSpeedSetting();
        }

        /// <summary>
        /// Increase the TCP velocity value new Actions will be ran at.
        /// </summary>
        /// <param name="speedInc">TCP speed increment in mm/s.</param>
        public bool Speed(double speedInc)
        {
            return c.IssueSpeedRequest(speedInc, true);
        }

        /// <summary>
        /// Set the TCP velocity value new Actions will be ran at.
        /// </summary>
        /// <param name="speed">TCP speed value in mm/s</param>
        public bool SpeedTo(double speed)
        {
            return c.IssueSpeedRequest(speed, false);
        }

        /// <summary>
        /// Increase the TCP acceleration value new Actions will be ran at.
        /// </summary>
        /// <param name="accInc">TCP acceleration increment in mm/s^2. Decreasing the total to zero or less will reset it back the robot's default.</param>
        /// <returns></returns>
        public bool Acceleration(double accInc)
        {
            return c.IssueAccelerationRequest(accInc, true);
        }

        /// <summary>
        /// Set the TCP acceleration value new Actions will be ran at. 
        /// </summary>
        /// <param name="acceleration">TCP acceleration value in mm/s^2. Setting this value to zero or less will reset acceleration to the robot's default.</param>
        /// <returns></returns>
        public bool AccelerationTo(double acceleration)
        {
            return c.IssueAccelerationRequest(acceleration, false);
        }

        /// <summary>
        /// Increase the TCP angular rotation speed value new Actions will be ran at.
        /// </summary>
        /// <param name="rotationSpeedInc">TCP angular rotation speed increment in deg/s. Decreasing the total to zero or less will reset it back to the robot's default.</param>
        /// <returns></returns>
        public bool RotationSpeed(double rotationSpeedInc)
        {
            return c.IssueRotationSpeedRequest(rotationSpeedInc, true);
        }

        /// <summary>
        /// Set the TCP angular rotation speed value new Actions will be ran at.
        /// </summary>
        /// <param name="rotationSpeed">TCP angular rotation speed value in deg/s. Setting this value to zero or less will reset it back to the robot's default.</param>
        /// <returns></returns>
        public bool RotationSpeedTo(double rotationSpeed)
        {
            return c.IssueRotationSpeedRequest(rotationSpeed, true);
        }

        /// <summary>
        /// Increase the maximum joint angular rotation speed value. Movement will be constrained so that the fastest joint rotates below this threshold. 
        /// </summary>
        /// <param name="jointSpeedInc">Maximum joint angular rotation speed increment in deg/s. Decreasing the total to zero or less will reset it back to the robot's default.</param>
        /// <returns></returns>
        public bool JointSpeed(double jointSpeedInc) => c.IssueJointSpeedRequest(jointSpeedInc, true);

        /// <summary>
        /// Set the maximum joint angular rotation speed value. Movement will be constrained so that the fastest joint rotates below this threshold. 
        /// </summary>
        /// <param name="jointSpeed">Maximum joint angular rotation speed value in deg/s. Setting this value to zero or less will reset it back to the robot's default.</param>
        /// <returns></returns>
        public bool JointSpeedTo(double jointSpeed) => c.IssueJointSpeedRequest(jointSpeed, false);

        /// <summary>
        /// Increase the maximum joint angular rotation acceleration value. Movement will be constrained so that the fastest joint accelerates below this threshold. 
        /// </summary>
        /// <param name="jointAccelerationInc">Maximum joint angular rotation acceleration increment in deg/s^2. Decreasing the total to zero or less will reset it back to the robot's default.</param>
        /// <returns></returns>
        public bool JointAcceleration(double jointAccelerationInc) => c.IssueJointAccelerationRequest(jointAccelerationInc, true);

        /// <summary>
        /// Set the maximum joint angular rotation acceleration value. Movement will be constrained so that the fastest joint accelerates below this threshold. 
        /// </summary>
        /// <param name="jointAcceleration">Maximum joint angular rotation acceleration value in deg/s^2. Setting this value to zero or less will reset it back to the robot's default.</param>
        /// <returns></returns>
        public bool JointAccelerationTo(double jointAcceleration) => c.IssueJointAccelerationRequest(jointAcceleration, false);

        /// <summary>
        /// Gets the current zone setting.
        /// </summary>
        /// <returns></returns>
        [System.Obsolete("Deprecated method, use GetPrecision() instead")]
        public double Zone()
        {
            return c.GetCurrentPrecisionSettings();
        }

        /// <summary>
        /// Increase the default zone value new Actions will be given.
        /// </summary>
        /// <param name="zoneInc"></param>
        [System.Obsolete("Deprecated method, use Precision(radiusInc) instead")]
        public void Zone(double zoneInc)
        {
            c.IssuePrecisionRequest(zoneInc, true);
        }

        /// <summary>
        /// Sets the default zone value new Actions will be given.
        /// </summary>
        /// <param name="zone"></param>
        [System.Obsolete("Deprecated method, use PrecisionTo(radius) instead")]
        public void ZoneTo(double zone)
        {
            c.IssuePrecisionRequest(zone, false);
        }

        /// <summary>
        /// Increase the default precision value new Actions will be given. 
        /// Precision is measured as the radius of the smooth interpolation
        /// between motion targets. This is refered to as "Zone", "Approximate
        /// Positioning" or "Blending Radius" in different platforms. 
        /// </summary>
        /// <param name="radiusInc">Smoothing radius increment in mm.</param>
        public bool Precision(double radiusInc)
        {
            return c.IssuePrecisionRequest(radiusInc, true);
        }

        /// <summary>
        /// Set the default precision value new Actions will be given. 
        /// Precision is measured as the radius of the smooth interpolation
        /// between motion targets. This is refered to as "Zone", "Approximate
        /// Positioning" or "Blending Radius" in different platforms. 
        /// </summary>
        /// <param name="radius">Smoothing radius in mm.</param>
        public bool PrecisionTo(double radius)
        {
            return c.IssuePrecisionRequest(radius, false);
        }

        ///// <summary>
        ///// Gets the current MotionType setting.
        ///// </summary>
        ///// <returns></returns>
        //public MotionType MotionMode()
        //{
        //    return c.GetCurrentMotionTypeSetting();
        //}

        /// <summary>
        /// Sets the motion type (linear, joint...) for future issued Actions.
        /// </summary>
        /// <param name="motionType"></param>
        public bool MotionMode(MotionType motionType)
        {
            return c.IssueMotionRequest(motionType);
        }

        /// <summary>
        /// Sets the motion type (linear, joint...) for future issued Actions.
        /// </summary>
        /// <param name="motionType">"linear", "joint", etc.</param>
        public bool MotionMode(string motionType)
        {
            MotionType mt;
            try
            {
                mt = (MotionType)Enum.Parse(typeof(MotionType), motionType, true);
                if (Enum.IsDefined(typeof(MotionType), mt))
                {
                    return c.IssueMotionRequest(mt);
                }
            }
            catch
            {
                Console.WriteLine($"{motionType} is not a valid target part for motion type changes, please specify one of the following: ");
                foreach (string str in Enum.GetNames(typeof(MotionType)))
                {
                    Console.WriteLine(str);
                }
            }
            return false;
        }

        /// <summary>
        /// Gets current ReferenceCS setting.
        /// </summary>
        /// <returns></returns>
        public ReferenceCS Coordinates()
        {
            return c.GetCurrentReferenceCS();
        }

        /// <summary>
        /// Sets the reference system used for relative transformations.
        /// </summary>
        /// <param name="refcs"></param>
        public void Coordinates(ReferenceCS refcs)
        {
            c.IssueCoordinatesRequest(refcs);
        }

        /// <summary>
        /// Sets the reference system used for relative transformations ("local", "global", etc.)
        /// </summary>
        /// <param name="type"></param>
        public void Coordinates(string type)
        {
            ReferenceCS refcs;
            type = type.ToLower();
            if (type.Equals("global") || type.Equals("world"))
            {
                refcs = ReferenceCS.World;
            }
            else if (type.Equals("local"))
            {
                refcs = ReferenceCS.Local;
            }
            else
            {
                Console.WriteLine("Invalid reference coordinate system");
                return;
            }

            Coordinates(refcs);
        }

        /// <summary>
        /// Buffers current state settings (speed, precision, motion type...), and opens up for 
        /// temporary settings changes to be reverted by PopSettings().
        /// </summary>
        public void PushSettings()
        {
            c.IssuePushPopRequest(true);
        }

        /// <summary>
        /// Reverts the state settings (speed, precision, motion type...) to the previously buffered
        /// state by PushSettings().
        /// </summary>
        public void PopSettings()
        {
            c.IssuePushPopRequest(false);
        }

        /// <summary>
        /// Increments the working temperature of one of the device's parts. Useful for 3D printing operations. 
        /// </summary>
        /// <param name="temp">Temperature increment in °C.</param>
        /// <param name="devicePart">Device's part that will change temperature, e.g. "extruder", "bed", etc.</param>
        /// <param name="waitToReachTemp">If true, execution will wait for the part to heat up and resume when reached the target.</param>
        /// <returns></returns>
        public bool Temperature(double temp, string devicePart, bool waitToReachTemp = true)
        {
            RobotPartType tt;
            try
            {
                tt = (RobotPartType)Enum.Parse(typeof(RobotPartType), devicePart, true);
                if (Enum.IsDefined(typeof(RobotPartType), tt))
                {
                    return c.IssueTemperatureRequest(temp, tt, waitToReachTemp, true);
                }
            }
            catch
            {
                Console.WriteLine("{0} is not a valid target part for temperature changes, please specify one of the following: ", devicePart);
                foreach (string str in Enum.GetNames(typeof(RobotPartType)))
                {
                    Console.WriteLine(str);
                }
            }
            return false;
        }

        /// <summary>
        /// Sets the working temperature of one of the device's parts. Useful for 3D printing operations. 
        /// </summary>
        /// <param name="temp">Temperature increment in °C.</param>
        /// <param name="devicePart">Device's part that will change temperature, e.g. "extruder", "bed", etc.</param>
        /// <param name="waitToReachTemp">If true, execution will wait for the part to heat up and resume when reached the target.</param>
        /// <returns></returns>
        public bool TemperatureTo(double temp, string devicePart, bool waitToReachTemp = true)
        {
            RobotPartType tt;
            try
            {
                tt = (RobotPartType)Enum.Parse(typeof(RobotPartType), devicePart, true);
                if (Enum.IsDefined(typeof(RobotPartType), tt))
                {
                    return c.IssueTemperatureRequest(temp, tt, waitToReachTemp, false);
                }
            }
            catch
            {
                Console.WriteLine("{0} is not a valid target part for temperature changes, please specify one of the following: ", devicePart);
                foreach (string str in Enum.GetNames(typeof(RobotPartType)))
                {
                    Console.WriteLine(str);
                }
            }
            return false;
        }



        /// <summary>
        /// Increases the extrusion rate of filament for 3D printers.
        /// </summary>
        /// <param name="rateInc">Increment of mm of filament per mm of movement.</param>
        /// <returns></returns>
        public bool ExtrusionRate(double rateInc)
        {
            return c.IssueExtrusionRateRequest(rateInc, true);
        }

        /// <summary>
        /// Sets the extrusion rate of filament for 3D printers.
        /// </summary>
        /// <param name="rate">mm of filament per mm of movement.</param>
        /// <returns></returns>
        public bool ExtrusionRateTo(double rate)
        {
            return c.IssueExtrusionRateRequest(rate, false);
        }




        //   █████╗  ██████╗████████╗██╗ ██████╗ ███╗   ██╗███████╗
        //  ██╔══██╗██╔════╝╚══██╔══╝██║██╔═══██╗████╗  ██║██╔════╝
        //  ███████║██║        ██║   ██║██║   ██║██╔██╗ ██║███████╗
        //  ██╔══██║██║        ██║   ██║██║   ██║██║╚██╗██║╚════██║
        //  ██║  ██║╚██████╗   ██║   ██║╚██████╔╝██║ ╚████║███████║
        //  ╚═╝  ╚═╝ ╚═════╝   ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝╚══════╝
        //                                                         

        /// <summary>
        /// Applies an Action object to this robot. 
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool Do(Action action)
        {
            return c.IssueApplyActionRequest(action);
        }

        /// <summary>
        /// Issue a relative movement action request on current coordinate system.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public bool Move(Vector direction)
        {
            return c.IssueTranslationRequest(direction, true);
        }

        /// <summary>
        /// Issue a relative movement action request on current coordinate system.
        /// </summary>
        /// <param name="incX"></param>
        /// <param name="incY"></param>
        /// <param name="incZ"></param>
        /// <returns></returns>
        public bool Move(double incX, double incY, double incZ = 0)
        {
            return Move(new Vector(incX, incY, incZ));
        }

        /// <summary>
        /// Issue an absolute movement action request.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool MoveTo(Point position)
        {
            return c.IssueTranslationRequest(position, false);
        }

        /// <summary>
        /// Issue an absolute movement action request.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public bool MoveTo(double x, double y, double z)
        {
            return MoveTo(new Vector(x, y, z));
        }

        /// <summary>
        /// Issue a RELATIVE rotation action request according to the current reference system.
        /// </summary>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public bool Rotate(Rotation rotation)
        {
            return c.IssueRotationRequest(rotation, true);
        }

        /// <summary>
        /// Issue a RELATIVE rotation action request according to the current reference system.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="angDegs"></param>
        /// <returns></returns>
        public bool Rotate(Vector vector, double angDegs)
        {
            return Rotate(new Rotation(vector.X, vector.Y, vector.Z, angDegs, true));
        }

        /// <summary>
        /// Issue a RELATIVE rotation action request according to the current reference system.
        /// </summary>
        /// <param name="rotVecX"></param>
        /// <param name="rotVecY"></param>
        /// <param name="rotVecZ"></param>
        /// <param name="angDegs"></param>
        /// <returns></returns>
        public bool Rotate(double rotVecX, double rotVecY, double rotVecZ, double angDegs)
        {
            return Rotate(new Rotation(rotVecX, rotVecY, rotVecZ, angDegs, true));
        }

        /// <summary>
        /// Issue an ABSOLUTE reorientation request according to the current reference system.
        /// </summary>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public bool RotateTo(Rotation rotation)
        {
            return c.IssueRotationRequest(rotation, false);
        }

        /// <summary>
        /// Issue an ABSOLUTE reorientation request according to the current reference system.
        /// </summary>
        /// <param name="cs"></param>
        /// <returns></returns>
        public bool RotateTo(Orientation cs)
        {
            return RotateTo((Rotation)cs);
        }

        /// <summary>
        /// Issue an ABSOLUTE reorientation request according to the current reference system.
        /// </summary>
        /// <param name="vecX"></param>
        /// <param name="vecY"></param>
        /// <returns></returns>
        public bool RotateTo(Vector vecX, Vector vecY)
        {
            return RotateTo((Rotation)new Orientation(vecX, vecY));
        }

        /// <summary>
        /// Issue an ABSOLUTE reorientation request according to the current reference system.
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="y0"></param>
        /// <param name="y1"></param>
        /// <param name="y2"></param>
        /// <returns></returns>
        public bool RotateTo(double x0, double x1, double x2, double y0, double y1, double y2)
        {
            return RotateTo((Rotation)new Orientation(x0, x1, x2, y0, y1, y2));
        }

        /// <summary>
        /// Issue a compound RELATIVE local Translation + Rotation request
        /// according to the current reference system.
        /// Note that, if using local coordinates, order of Actions will matter.  // TODO: wouldn't they matter too if the are in global coordinates?
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public bool Transform(Vector direction, Rotation rotation)
        {
            // Note the T+R action order
            //return c.IssueTranslationAndRotationRequest(position, true, rotation, true);

            return c.IssueTransformationRequest(direction, rotation, true, true);
        }

        /// <summary>
        /// Issue a compound RELATIVE local Rotation + Translation request
        /// according to the current reference system.
        /// Note that, if using local coordinates, order of Actions will matter. // TODO: wouldn't they matter too if the are in global coordinates?
        /// </summary>
        /// <param name="rotation"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public bool Transform(Rotation rotation, Vector direction)
        {
            // Note the R+T action order
            //return c.IssueRotationAndTranslationRequest(rotation, true, position, true);

            return c.IssueTransformationRequest(direction, rotation, true, false);
        }

        /// <summary>
        /// Issue a compound ABSOLUTE global Translation + Rotation request
        /// according to the current reference system.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public bool TransformTo(Point position, Orientation orientation)
        {
            // Action order is irrelevant in absolute mode (since translations are applied based on immutable world XYZ)
            //return c.IssueTranslationAndRotationRequest(true, position, false, true, rotation, false);

            return c.IssueTransformationRequest(position, orientation, false, true);
        }

        /// <summary>
        /// Issue a compound ABSOLUTE global Translation + Rotation request
        /// according to the current reference system.
        /// </summary>
        /// <param name="rotation"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool TransformTo(Orientation orientation, Point position)
        {
            // Action order is irrelevant in absolute mode (since translations are applied based on immutable world XYZ)
            //return c.IssueTranslationAndRotationRequest(true, position, false, true, rotation, false);

            return c.IssueTransformationRequest(position, orientation, false, false);
        }

        /// <summary>
        /// Issue a compound ABSOLUTE global Translation + Rotation request
        /// according to the current reference system.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="vX0"></param>
        /// <param name="vX1"></param>
        /// <param name="vX2"></param>
        /// <param name="vY0"></param>
        /// <param name="vY1"></param>
        /// <param name="vY2"></param>
        /// <returns></returns>
        public bool TransformTo(double x, double y, double z, double vX0, double vX1, double vX2, double vY0, double vY1, double vY2) =>
            c.IssueTransformationRequest(new Vector(x, y, z), new Orientation(vX0, vX1, vX2, vY0, vY1, vY2), false, true);


        /// <summary>
        /// Issue a request to increment the angular values of the robot joint axes rotations.
        /// Values expressed in degrees.
        /// </summary>
        /// <param name="incJoints"></param>
        /// <returns></returns>
        public bool Axes(Joints incJoints)
        {
            return c.IssueJointsRequest(incJoints, true);
        }
        [System.Obsolete("Deprecated, use Axes() instead")]
        public bool Joints(Joints incJoints)
        {
            return c.IssueJointsRequest(incJoints, true);
        }

        /// <summary>
        /// Issue a request to increment the angular values of the robot joint axes rotations.
        /// Values expressed in degrees.
        /// </summary>
        /// <param name="incJ1"></param>
        /// <param name="incJ2"></param>
        /// <param name="incJ3"></param>
        /// <param name="incJ4"></param>
        /// <param name="incJ5"></param>
        /// <param name="incJ6"></param>
        /// <returns></returns>
        public bool Axes(double incJ1, double incJ2, double incJ3, double incJ4, double incJ5, double incJ6)
        {
            return c.IssueJointsRequest(new Joints(incJ1, incJ2, incJ3, incJ4, incJ5, incJ6), true);
        }
        [System.Obsolete("Deprecated, use Axes() instead")]
        public bool Joints(double incJ1, double incJ2, double incJ3, double incJ4, double incJ5, double incJ6)
        {
            return c.IssueJointsRequest(new Joints(incJ1, incJ2, incJ3, incJ4, incJ5, incJ6), true);
        }

        /// <summary>
        /// Issue a request to set the angular values of the robot joint axes rotations.
        /// Values expressed in degrees.
        /// </summary>
        /// <param name="joints"></param>
        /// <returns></returns>
        /// 
        public bool AxesTo(Joints joints)
        {
            return c.IssueJointsRequest(joints, false);
        }
        [System.Obsolete("Deprecated, use AxesTo() instead")]
        public bool JointsTo(Joints joints)
        {
            return c.IssueJointsRequest(joints, false);
        }

        /// <summary>
        /// Issue a request to set the angular values of the robot joint axes rotations.
        /// Values expressed in degrees.
        /// </summary>
        /// <param name="j1"></param>
        /// <param name="j2"></param>
        /// <param name="j3"></param>
        /// <param name="j4"></param>
        /// <param name="j5"></param>
        /// <param name="j6"></param>
        /// <returns></returns>
        public bool AxesTo(double j1, double j2, double j3, double j4, double j5, double j6)
        {
            return c.IssueJointsRequest(new Joints(j1, j2, j3, j4, j5, j6), false);
        }
        [System.Obsolete("Deprecated, use AxesTo() instead")]
        public bool JointsTo(double j1, double j2, double j3, double j4, double j5, double j6)
        {
            return c.IssueJointsRequest(new Joints(j1, j2, j3, j4, j5, j6), false);
        }

        /// <summary>
        /// Issue a request to wait idle before moving to next action. 
        /// </summary>
        /// <param name="timeMillis">Time expressed in milliseconds</param>
        /// <returns></returns>
        public bool Wait(long timeMillis)
        {
            return c.IssueWaitRequest(timeMillis);
        }

        /// <summary>
        /// Send a string message to the device, to be displayed based on device's capacities.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool Message(string message)
        {
            return c.IssueMessageRequest(message);
        }

        /// <summary>
        /// Display an internal comment in the compilation code. 
        /// Useful for internal annotations, reminders, etc. 
        /// </summary>
        /// <param name="comment"></param>
        /// <returns></returns>
        public bool Comment(string comment)
        {
            return c.IssueCommentRequest(comment);
        }

        /// <summary>
        /// Attach a Tool to the flange of this Robot.
        /// From this moment, all Actions like Move or Rotate will refer
        /// to the Tool Center Point (TCP).
        /// </summary>
        /// <param name="tool"></param>
        /// <returns></returns>
        public bool Attach(Tool tool)
        {
            return c.IssueAttachRequest(tool);
        }

        /// <summary>
        /// Detach all Tools from the flange of this Robot.
        /// From this moment, all Actions like Move or Rotate will refer
        /// to the Flange Center Point (FCP).
        /// </summary>
        /// <returns></returns>
        public bool Detach()
        {
            return c.IssueDetachRequest();
        }


        /// <summary>
        /// Writes to the digital IO pin.
        /// </summary>
        /// <param name="pinNumber"></param>
        /// <param name="isOn"></param>
        /// <param name="toolPin">Is this pin on the tool?</param>
        public bool WriteDigital(int pinNumber, bool isOn, bool toolPin = false)
        {
            return c.IssueWriteToDigitalIORequest(pinNumber.ToString(), isOn, toolPin);
        }

        /// <summary>
        /// Writes to the digital IO pin.
        /// </summary>
        /// <param name="pinId">Pin name.</param>
        /// <param name="isOn"></param>
        /// <param name="toolPin">Is this pin on the tool?</param>
        public bool WriteDigital(string pinId, bool isOn, bool toolPin = false)
        {
            return c.IssueWriteToDigitalIORequest(pinId, isOn, toolPin);
        }

        /// <summary>
        /// Writes to the analog IO pin.
        /// </summary>
        /// <param name="pinNumber"></param>
        /// <param name="value"></param>
        /// <param name="toolPin">Is this pin on the tool?</param>
        public bool WriteAnalog(int pinNumber, double value, bool toolPin = false)
        {
            return c.IssueWriteToAnalogIORequest(pinNumber.ToString(), value, toolPin);
        }

        /// <summary>
        /// Writes to the analog IO pin.
        /// </summary>
        /// <param name="pinId">Pin name.</param>
        /// <param name="value"></param>
        /// <param name="toolPin">Is this pin on the tool?</param>
        public bool WriteAnalog(string pinId, double value, bool toolPin = false)
        {
            return c.IssueWriteToAnalogIORequest(pinId.ToString(), value, toolPin);
        }

        /// <summary>
        /// Reads from the digital IO pin.
        /// </summary>
        /// <param name="pinNumber"></param>
        /// <returns></returns>
        private bool ReadDigital(int pinNumber)
        {
            Console.WriteLine("ReadDigital not implemented yet!");
            return false;
        }

        /// <summary>
        /// Reads from the analog IO pin.
        /// </summary>
        /// <param name="pinNumber"></param>
        /// <returns></returns>
        private double ReadAnalog(int pinNumber)
        {
            Console.WriteLine("ReadAnalog not implemented yet!");
            return 0.0;
        }

        ///// <summary>
        ///// Turn digital IO on. Is alias for `WriteDigital(pinNumber, true)`
        ///// </summary>
        ///// <param name="pinNumber"></param>
        //public bool TurnOn(int pinNumber)
        //{
        //    return this.WriteDigital(pinNumber, true);
        //}

        ///// <summary>
        ///// Turn digital IO off. Is alias for `WriteDigital(pinNumber, false)`
        ///// </summary>
        ///// <param name="pinNumber"></param>
        //public bool TurnOff(int pinNumber)
        //{
        //    return this.WriteDigital(pinNumber, false);
        //}

        /// <summary>
        /// Turns extrusion in 3D printers on/off.
        /// </summary>
        /// <param name="extrude">True/false for on/off.</param>
        /// <returns></returns>
        public bool Extrude(bool extrude = true)
        {
            return c.IssueExtrudeRequest(extrude);
        }

        /// <summary>
        /// Initialize this device for action. Initialization uses device-specific
        /// common initialization routines, like homing and calibration, to set the 
        /// device ready for typical procedures like 3D printing. 
        /// </summary>
        /// <returns></returns>
        public bool Initialize()
        {
            return c.IssueInitializationRequest(true);
        }

        /// <summary>
        /// Terminate this device. Termination uses device-specific
        /// common termination routines, like cooling or turning fans off, to prepare
        /// the device for idleness.
        /// </summary>
        /// <returns></returns>
        public bool Terminate()
        {
            return c.IssueInitializationRequest(false);
        }

        /// <summary>
        /// Increase the value of one of the robot's external axis. 
        /// Values expressed in degrees or milimeters, depending on the nature of the external axis.
        /// Note that the effect of this change of external axis will go in effect on the next motion Action.
        /// </summary>
        /// <param name="axisNumber">Axis number from 1 to 6.</param>
        /// <param name="increment">Increment value in mm or degrees.</param>
        public bool ExternalAxis(int axisNumber, double increment)
        {
            if (axisNumber == 0)
            {
                Console.WriteLine("Please enter an axis number between 1-6");
                return false;
            }
            return c.IssueExternalAxisRequest(axisNumber, increment, true);
        }

        /// <summary>
        /// Set the value of one of the robot's external axis. 
        /// Values expressed in degrees or milimeters, depending on the nature of the external axis.
        /// Note that the effect of this change of external axis will go in effect on the next motion Action.
        /// </summary>
        /// <param name="axisNumber">Axis number from 1 to 6.</param>
        /// <param name="value">Axis value in mm or degrees.</param>
        public bool ExternalAxisTo(int axisNumber, double value)
        {
            if (axisNumber == 0)
            {
                Console.WriteLine("Please enter an axis number between 1-6");
                return false;
            }
            return c.IssueExternalAxisRequest(axisNumber, value, false);
        }


        /// <summary>
        /// Insert a line of custom code directly into a compiled program. 
        /// This is useful for obscure instructions that are not covered by Machina's API. 
        /// Note that this Action cannot be checked for validity by Machina, and you are responsible for correct syntax.
        /// This Action is non-streamable. 
        /// </summary>
        /// <param name="statement">Code in the machine's native language.</param>
        /// <param name="isDeclaration">Is this a declaration, like a variable or a workobject? If so, this statement will be placed at the beginning of the program.</param>
        /// <returns></returns>
        public bool CustomCode(string statement, bool isDeclaration = false) =>
                c.IssueCustomCodeRequest(statement, isDeclaration);



        //   ██████╗ ███████╗████████╗████████╗███████╗██████╗ ███████╗
        //  ██╔════╝ ██╔════╝╚══██╔══╝╚══██╔══╝██╔════╝██╔══██╗██╔════╝
        //  ██║  ███╗█████╗     ██║      ██║   █████╗  ██████╔╝███████╗
        //  ██║   ██║██╔══╝     ██║      ██║   ██╔══╝  ██╔══██╗╚════██║
        //  ╚██████╔╝███████╗   ██║      ██║   ███████╗██║  ██║███████║
        //   ╚═════╝ ╚══════╝   ╚═╝      ╚═╝   ╚══════╝╚═╝  ╚═╝╚══════╝
        //               

        /// <summary>
        /// Returns a Point representation of the Robot's TCP position in mm and World coordinates.
        /// </summary>
        /// <returns></returns>
        public Point GetCurrentPosition() => c.GetCurrentPosition();

        /// <summary>
        /// Returns a Rotation representation of the Robot's TCP orientation in quaternions.
        /// </summary>
        /// <returns></returns>
        public Rotation GetCurrentRotation() => c.GetCurrentRotation();

        /// <summary>
        /// Returns a Joint object representing the rotations in the robot axes.
        /// </summary>
        /// <returns></returns>
        public Joints GetCurrentAxes() => c.GetCurrentAxes();

        /// <summary>
        /// Retuns an ExternalAxes object representing the values of the external axes. If a value is null, that axis is not valid.
        /// </summary>
        /// <returns></returns>
        public ExternalAxes GetCurrentExternalAxes() => c.GetCurrentExternalAxes();

        /// <summary>
        /// Returns the Tool object currently attached to this Robot, null if none.
        /// </summary>
        /// <returns>The Tool object currently attached to this Robot, null if none.</returns>
        public Tool GetCurrentTool() => c.GetCurrentTool();

        /// <summary>
        /// Returns a Point represnting the current location of the Tool Center Point
        /// (if there is a Tool attached) or the Flange Center Point (if there isn't).
        /// </summary>
        /// <returns></returns>
        public Point GetVirtualPosition() => c.GetVirtualPosition();

        /// <summary>
        /// Return a Orientation object representing the current orientation of the Tool Center Point
        /// (if there is a Tool attached) or the Flange Center Point (if there isn't).
        /// </summary>
        /// <returns></returns>
        public Orientation GetVirtualRotation() => c.GetVirtualRotation();

        /// <summary>
        /// Returns a Joint object representing the rotations in the robot axes.
        /// </summary>
        /// <returns></returns>
        public Joints GetVirtualAxes() => c.GetVirtualAxes();

        /// <summary>
        /// Returns the Tool object currently attached to this Robot, null if none.
        /// </summary>
        /// <returns>The Tool object currently attached to this Robot, null if none.</returns>
        public Tool GetVirtualTool() => c.GetVirtualTool();


        public override string ToString() => $"Robot[\"{this.Name}\", {this.Brand}]";





        //  ██████╗ ███████╗██████╗ ██╗   ██╗ ██████╗ 
        //  ██╔══██╗██╔════╝██╔══██╗██║   ██║██╔════╝ 
        //  ██║  ██║█████╗  ██████╔╝██║   ██║██║  ███╗
        //  ██║  ██║██╔══╝  ██╔══██╗██║   ██║██║   ██║
        //  ██████╔╝███████╗██████╔╝╚██████╔╝╚██████╔╝
        //  ╚═════╝ ╚══════╝╚═════╝  ╚═════╝  ╚═════╝ 
        //                                            
        /// <summary>
        /// Dumps a bunch of information to the console about the controller, the main task, etc.
        /// </summary>
        public void DebugDump()
        {
            c.DebugDump();
        }

        /// <summary>
        /// Dumps a list of the remaining buffered Actions.
        /// </summary>
        public void DebugBuffers()
        {
            c.DebugBuffers();
        }

        /// <summary>
        /// Dumps the state of the internal RobotPointers
        /// </summary>
        public void DebugRobotCursors()
        {
            c.DebugRobotCursors();
        }

        /// <summary>
        /// Dumps current Settings values
        /// </summary>
        public void DebugSettingsBuffer()
        {
            //c.DebugSettingsBuffer();
        }





        //  ███████╗██╗   ██╗███████╗███╗   ██╗████████╗███████╗
        //  ██╔════╝██║   ██║██╔════╝████╗  ██║╚══██╔══╝██╔════╝
        //  █████╗  ██║   ██║█████╗  ██╔██╗ ██║   ██║   ███████╗
        //  ██╔══╝  ╚██╗ ██╔╝██╔══╝  ██║╚██╗██║   ██║   ╚════██║
        //  ███████╗ ╚████╔╝ ███████╗██║ ╚████║   ██║   ███████║
        //  ╚══════╝  ╚═══╝  ╚══════╝╚═╝  ╚═══╝   ╚═╝   ╚══════╝
        //                                                      
        /// <summary>
        /// Will be raised when Machina has finished streaming all pending Actions to the controller.
        /// Note that the controller still needs to receive them and execute them. This gives Machina 
        /// time to prepare the next batch.
        /// </summary>
        public event BufferEmptyHandler BufferEmpty;
        public delegate void BufferEmptyHandler(object sender, BufferEmptyArgs e);
        internal virtual void OnBufferEmpty(BufferEmptyArgs e) => BufferEmpty?.Invoke(this, e);

        /// <summary>
        /// Will be raised when Machina received an update from the controller as has new motion
        /// information available. Useful to keep track of the state of the controller.
        /// </summary>
        public event MotionCursorUpdatedHandler MotionCursorUpdated;
        public delegate void MotionCursorUpdatedHandler(object sender, MotionCursorUpdatedArgs e);
        internal virtual void OnMotionCursorUpdated(MotionCursorUpdatedArgs e) => MotionCursorUpdated?.Invoke(this, e);

        /// <summary>
        /// Raised whenever an action has been completed by the device. 
        /// </summary>
        public event ActionCompletedHandler ActionCompleted;
        public delegate void ActionCompletedHandler(object sender, ActionCompletedArgs e);
        internal virtual void OnActionCompleted(ActionCompletedArgs e) => ActionCompleted?.Invoke(this, e);

        public event ToolCreatedHandler ToolCreated;
        public delegate void ToolCreatedHandler(object sender, ToolCreatedArgs e);
        internal virtual void OnToolCreated(ToolCreatedArgs e) => ToolCreated?.Invoke(this, e);

        ///// <summary>
        ///// Raised when Machina wants to log something. Suscribe to this event to receive string logs with prioroty level.
        ///// </summary>
        //public event LogHandler Log;
        //public delegate void LogHandler(object sender, LogArgs e);
        //internal virtual void OnLog(LogArgs e) => Log?.Invoke(this, e);

    }








    //  ███████╗██╗   ██╗███████╗███╗   ██╗████████╗ █████╗ ██████╗  ██████╗ ███████╗
    //  ██╔════╝██║   ██║██╔════╝████╗  ██║╚══██╔══╝██╔══██╗██╔══██╗██╔════╝ ██╔════╝
    //  █████╗  ██║   ██║█████╗  ██╔██╗ ██║   ██║   ███████║██████╔╝██║  ███╗███████╗
    //  ██╔══╝  ╚██╗ ██╔╝██╔══╝  ██║╚██╗██║   ██║   ██╔══██║██╔══██╗██║   ██║╚════██║
    //  ███████╗ ╚████╔╝ ███████╗██║ ╚████║   ██║   ██║  ██║██║  ██║╚██████╔╝███████║
    //  ╚══════╝  ╚═══╝  ╚══════╝╚═╝  ╚═══╝   ╚═╝   ╚═╝  ╚═╝╚═╝  ╚═╝ ╚═════╝ ╚══════╝
    //                                                                              
    public abstract class MachinaEventArgs : EventArgs
    {
        /// <summary>
        /// The arguments on this event must be serializable to a JSON object
        /// </summary>
        /// <returns></returns>
        public abstract string ToJSONString();
        

        //public string SerializeToJSON()
        //{
        //    MemoryStream ms = new MemoryStream();
        //    DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(MachinaEventArgs));
        //    ser.WriteObject(ms, this);
        //    byte[] json = ms.ToArray();
        //    ms.Close();
        //    return Encoding.UTF8.GetString(json, 0, json.Length);
        //}

        //public MachinaEventArgs DeserializeFromJSON(string json)
        //{
        //    MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
        //    DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(MachinaEventArgs));
        //    MachinaEventArgs e = ser.ReadObject(ms) as MachinaEventArgs;
        //    ms.Close();
        //    return e;
        //}
        
    }


    public class BufferEmptyArgs : MachinaEventArgs
    {
        // There is nothing really worthy on this event...
        public string eventType = "buffer-empty";

        public override string ToJSONString() => $"{{\"event\":\"buffer-empty\"}}";
    }


    public class ActionCompletedArgs : MachinaEventArgs
    {
        public Action LastAction { get; set; }
        public int RemainingActions { get; set; }
        public int RemainingInBuffer { get; set; }

        /// <summary>
        /// Arguments for onActionCompleted
        /// </summary>
        /// <param name="last">The last Action recently executed by the Robot.</param>
        /// <param name="pendingWrite">How many Actions are left to be executed by the Robot?</param>
        /// <param name="pendingBuffer">How many Actions are currently loaded in the Robot's buffer to be executed?</param>
        public ActionCompletedArgs(Action last, int pendingWrite, int pendingBuffer)
        {
            LastAction = last;
            RemainingActions = pendingWrite;
            RemainingInBuffer = pendingBuffer;
        }

        public override string ToJSONString()
        {
            return string.Format("{{\"event\":\"action-completed\",\"rem\":{0},\"robBuf\":{1},\"last\":\"{2}\"}}",
                this.RemainingActions,
                this.RemainingInBuffer,
                this.LastAction.ToInstruction());
        }
    }

    public class MotionCursorUpdatedArgs : MachinaEventArgs
    {
        public Vector Position;
        public Rotation Rotation;
        public Joints Axes;
        public ExternalAxes ExternalAxes;

        public MotionCursorUpdatedArgs(Vector pos, Rotation rot, Joints axes, ExternalAxes extAxes)
        {
            this.Position = pos;
            this.Rotation = rot;
            this.Axes = axes;
            this.ExternalAxes = extAxes;
        }

        public override string ToJSONString()
        {
            return string.Format("{{\"event\":\"execution-update\",\"pos\":{0},\"ori\":{1},\"quat\":{2},\"axes\":{3},\"extax\":{4},\"conf\":{5}}}",
                this.Position?.ToArrayString() ?? "null",
                this.Rotation?.ToOrientation()?.ToArrayString() ?? "null",
                this.Rotation?.Q.ToArrayString() ?? "null",
                this.Axes?.ToArrayString() ?? "null",
                this.ExternalAxes?.ToArrayString() ?? "null",
                "null");  // placeholder for whenever IK are introduced...
        }
    }

    /// <summary>
    /// A quick tool-created event because the interns need it for their awesome project! :)
    /// </summary>
    public class ToolCreatedArgs : MachinaEventArgs
    {
        public Tool tool;

        public ToolCreatedArgs(Tool tool)
        {
            this.tool = tool;
        }

        public override string ToJSONString()
        {
            return string.Format("{{\"event\":\"tool-created\",\"tool\":\"{0}\"}}",
                Util.EscapeDoubleQuotes(tool.ToInstruction())
            );
        }
    }

    //public class LogArgs : EventArgs
    //{
    //    public string Message { get; set; }
    //    public int Level { get; set; }

    //    public LogArgs(string message, int level)
    //    {
    //        Message = message;
    //        Level = level;
    //    }
    //}

    //[DataContract]
    //public class FooBar
    //{
    //    [DataMember(Name = "myName")]
    //    public string name;

    //    [DataMember]
    //    public int age;

    //    [DataMember]
    //    public Vector v;

    //    public FooBar(string name, int age, Vector v)
    //    {
    //        this.name = name;
    //        this.age = age;
    //        this.v = v;
    //    }

    //    public string SelfSerializeToJSON()
    //    {
    //        //Create a stream to serialize the object to.  
    //        MemoryStream ms = new MemoryStream();

    //        // Serializer the User object to the stream.  
    //        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(FooBar));
    //        ser.WriteObject(ms, this);
    //        byte[] json = ms.ToArray();
    //        ms.Close();
    //        return Encoding.UTF8.GetString(json, 0, json.Length);
    //    }
        
    //}



}
