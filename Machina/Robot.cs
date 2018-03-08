using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


//  ███╗   ███╗ █████╗  ██████╗██╗  ██╗██╗███╗   ██╗ █████╗ 
//  ████╗ ████║██╔══██╗██╔════╝██║  ██║██║████╗  ██║██╔══██╗
//  ██╔████╔██║███████║██║     ███████║██║██╔██╗ ██║███████║
//  ██║╚██╔╝██║██╔══██║██║     ██╔══██║██║██║╚██╗██║██╔══██║
//  ██║ ╚═╝ ██║██║  ██║╚██████╗██║  ██║██║██║ ╚████║██║  ██║
//  ╚═╝     ╚═╝╚═╝  ╚═╝ ╚═════╝╚═╝  ╚═╝╚═╝╚═╝  ╚═══╝╚═╝  ╚═╝
//                                                          

namespace Machina
{
    /// <summary>
    /// Represents the type of control that will be performed over the real/virtual robot.
    /// </summary>
    public enum ControlType : int
    {
        /// <summary>
        /// Not connected to any controller. Useful for robot code generation and export.
        /// </summary>
        Offline = 0,

        /// <summary>
        /// Online connection to a controller, the library will upload complete programs 
        /// and run them. Provides robust and fluid movement, useful on real-time 
        /// interactivity where response time is not a priority. 
        /// </summary>
        Execute = 1,

        /// <summary>
        /// Online connection to a controller, the library will stream individual targets
        /// at run time as they get priority. Provides the closest approximation to real-time
        /// interaction, useful on situations where low latency is required.
        /// </summary>
        Stream = 2
    }

    /// <summary>
    /// Defines the different cycle type modes a program can be ran.
    /// </summary>
    public enum CycleType : int
    {
        None = 0,
        Once = 1,
        Loop = 2
    }

    /// <summary>
    /// Defines which reference coordinate system to use for transform actions.
    /// </summary>
    public enum ReferenceCS : int
    {
        World = 0,
        Local = 1
    }

    /// <summary>
    /// Defines which type of motion to use for translation actions.
    /// </summary>
    public enum MotionType
    {
        Linear,
        Joint
    }

    /// <summary>
    /// Defines the type of robotic device.
    /// </summary>
    public enum RobotType
    {
        Undefined,
        HUMAN,
        ABB,
        UR,
        KUKA,
        ZMORPH,
    }

    ///// <summary>
    ///// Defines if the parameters for new Actions will be considered in absolute values 
    ///// or relative increments.
    ///// </summary>
    //public enum ActionModes
    //{
    //    Absolute, 
    //    Relative
    //}

    /// <summary>
    /// An enum with different robotic parts, to be used as targets for execution operations, 
    /// e.g. 3D printing, I/O, etc.
    /// @TODO: temp, this should probably go somewhere else... 
    /// </summary>
    public enum RobotPartType
    {
        Extruder,
        Bed, 
        Chamber
    }

    public delegate void BufferEmptyHandler(object sender, EventArgs e);



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
        public static readonly int Build = 1308;

        /// <summary>
        /// Version number.
        /// </summary>
        public static readonly string Version = "0.5.0." + Build;
       

        /// <summary>
        /// A nickname for this Robot.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// The main Control object, acts as an interface to all classes that
        /// manage robot control.
        /// </summary>
        private Control c;  // the main control object

        public event BufferEmptyHandler BufferEmpty;

        internal virtual void OnBufferEmpty(EventArgs e)
        {
            Console.WriteLine("Event raised, about to call handlers");
            if (BufferEmpty != null)
                BufferEmpty(this, e);
        }




        //  ██████╗ ██╗   ██╗██████╗ ██╗     ██╗ ██████╗     █████╗ ██████╗ ██╗
        //  ██╔══██╗██║   ██║██╔══██╗██║     ██║██╔════╝    ██╔══██╗██╔══██╗██║
        //  ██████╔╝██║   ██║██████╔╝██║     ██║██║         ███████║██████╔╝██║
        //  ██╔═══╝ ██║   ██║██╔══██╗██║     ██║██║         ██╔══██║██╔═══╝ ██║
        //  ██║     ╚██████╔╝██████╔╝███████╗██║╚██████╗    ██║  ██║██║     ██║
        //  ╚═╝      ╚═════╝ ╚═════╝ ╚══════╝╚═╝ ╚═════╝    ╚═╝  ╚═╝╚═╝     ╚═╝
        ///// <summary>
        ///// Base constructor.
        ///// </summary>                                                       
        [System.Obsolete("Deprecated constructor, defaults to a human-readable interpretation of the actions. Please use Robot(\"BrandName\") instead. Example: `Robot arm = new Robot(\"ABB\");`")]
        public Robot() : this("Machina", "HUMAN") { }

        [System.Obsolete("Deprecated constructor, use Robot(name, make) instead")]
        public Robot(string make) : this("Machina", make) { }

        /// <summary>
        /// Base constructor.
        /// </summary>
        /// <param name="name">A name for this Robot</param>
        /// <param name="make">The robot make. This will determine which drivers/compilers are used to manage it.</param>
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
                    c = new Control(this, rt);
                }
                else
                {
                    Console.WriteLine("{0} is not a RobotType, please specify one of the following: ", make);
                    foreach (string str in Enum.GetNames(typeof(RobotType)))
                    {
                        Console.WriteLine(str.ToString());
                    }
                    c = new Control(this, RobotType.Undefined);
                }
            }
            catch
            {
                Console.WriteLine("{0} is not a RobotType, please specify one of the following: ", make);
                foreach (string str in Enum.GetNames(typeof(RobotType)))
                {
                    Console.WriteLine(str.ToString());
                }
                c = new Control(this, RobotType.Undefined);
            }
        }


        /// What was this even for? Exposrt checks?
        public bool IsBrand(string brandName)
        {
            return c.robotBrand.ToString().ToUpper().Equals(brandName.ToUpper());
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
            return c.SetRunMode(cycleType);
        }

        /// <summary>
        /// Sets the cycle the robot will run program in (Once or Loop).
        /// </summary>
        /// <param name="cycleType"></param>
        public bool CycleMode(string cycleType)
        {
            CycleType ct;
            try
            {
                ct = (CycleType)Enum.Parse(typeof(CycleType), cycleType, true);
                if (Enum.IsDefined(typeof(CycleType), ct))
                    return c.SetRunMode(ct);
            }
            catch
            {
                Console.WriteLine($"{cycleType} is not a valid CycleMode type, please specify one of the following:");
                foreach (string str in Enum.GetNames(typeof(CycleType)))
                    Console.WriteLine(str);
            }
            return false;
        }

        /// <summary>
        /// Scans the network for robotic devices, real or virtual, and performs all necessary 
        /// operations to connect to it. This is necessary for 'online' modes such as 'execute' and 'stream.'
        /// </summary>
        /// <param name="mode">If multiple devices are connected, choose this id from the list.</param>
        /// <returns></returns>
        public bool Connect(int robotId)
        {
            return c.ConnectToDevice(robotId);
        }

        /// <summary>
        /// Performs all necessary operations to connect to the first robot device found on the network, real or virtual.
        /// This is necessary for 'online' modes.
        /// </summary>
        /// <returns></returns>
        public bool Connect()
        {
            return c.ConnectToDevice(0);
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
            return c.LoadProgramToDevice(filepath);
        }

        /// <summary>
        /// Loads a program to the robot from a string list of code.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool LoadProgram(List<string> code)
        {
            return c.LoadProgramToDevice(code);
        }

        /// <summary>
        /// Starts execution of the current module/s in the controller.
        /// @TODO: The behavior of this method will change depending based on Off/Online mode
        /// </summary>
        public bool Start()
        {
            return c.StartProgramOnDevice();
        }

        /// <summary>
        /// Immediately stops execution of the current program/s in the connected robot. 
        /// </summary>
        public bool Stop()
        {
            return c.StopProgramOnDevice(true);
        }

        /// <summary>
        /// Returns a Point representation of the Robot's TCP position in mm and World coordinates.
        /// </summary>
        /// <returns></returns>
        public Point GetCurrentPosition()
        {
            return c.GetCurrentPosition();
        }

        /// <summary>
        /// Returns a Rotation representation of the Robot's TCP orientation in quaternions.
        /// </summary>
        /// <returns></returns>
        public Orientation GetCurrentOrientation()
        {
            return c.GetCurrentOrientation();
        }

        ///// <summary>
        ///// Returns a Frame object representing the Robot's current TCP position and orientation.
        ///// NOTE: the Frame's velocity and precision are still not representative of the current state.
        ///// </summary>
        ///// <returns></returns>
        //public Frame GetCurrentFrame()
        //{
        //    return c.GetCurrentFrame();
        //}

        ///// <summary>
        ///// Returns a Joint object representing the current angular rotations of the robot's 6 axes.
        ///// </summary>
        ///// <returns></returns>
        //public Joints GetCurrentAxes()
        //{
        //    return c.getCurrentAxes();
        //}

        ///// <summary>
        ///// Loads the path to the queue manager and triggers execution of the program if applicable.
        ///// </summary>
        ///// <param name="path"></param>
        //public void LoadPath(Path path)
        //{
        //    c.AddPathToQueue(path);
        //}

        ///// <summary>
        ///// Stops the robot after execution of current program. This will also clear the queue.
        ///// </summary>
        //public void StopAfterProgram()
        //{
        //    c.ClearQueue();
        //    c.StopProgramOnDevice(false);
        //}

        /// <summary>
        /// Create a program with all the buffered actions and return it as a string List.
        /// Note all buffered actions will be removed from the queue.
        /// </summary>
        /// <returns></returns>
        public List<string> Export()
        {
            return c.Export(true, true);
        }

        /// <summary>
        /// Create a program with all the buffered actions and return it as a string List.
        /// Note all buffered actions will be removed from the queue.
        /// </summary>
        /// <param name="inlineTargets">Write inline targets on action statements, or declare them as independent variables?</param>
        /// <returns></returns>
        public List<string> Export(bool inlineTargets, bool humanComments)
        {
            return c.Export(inlineTargets, humanComments);
        }

        /// <summary>
        /// Create a program with all the buffered actions and save it to a file. 
        /// Note all buffered actions will be removed from the queue.
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public bool Export(string filepath)
        {
            return c.Export(filepath, true, true);
        }

        /// <summary>
        /// Create a program with all the buffered actions and save it to a file. 
        /// Note all buffered actions will be removed from the queue.
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="inlineTargets">Write inline targets on action statements, or declare them as independent variables?</param>
        /// <param name="humanComments">If true, a human-readable description will be added to each line of code</param>
        /// <returns></returns>
        public bool Export(string filepath, bool inlineTargets, bool humanComments)
        {
            return c.Export(filepath, inlineTargets, humanComments);
        }


        /// <summary>
        /// In 'execute' mode, flushes all pending actions, creates a program, 
        /// uploads it to the controller and runs it.
        /// </summary>
        /// <returns></returns>
        public void Execute()
        {
            c.Execute();
        }

        /// <summary>
        /// ABB IOs must have a name corresponding to their definition in the controller. This function is useful to give them
        /// a custom name that matches the controller's, and is used in code generation.
        /// </summary>
        /// <param name="ioName"></param>
        /// <param name="pinNumber"></param>
        /// <param name="isDigital"></param>
        public bool SetIOName(string ioName, int pinNumber, bool isDigital)
        {
            return c.SetIOName(ioName, pinNumber, isDigital);
        }
        





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
        public int Speed()
        {
            return c.GetCurrentSpeedSetting();
        }
                                                            
        /// <summary>
        /// Increase the default velocity new actions will be run at.
        /// </summary>
        /// <param name="speedInc"></param>
        public void Speed(int speedInc)
        {
            c.IssueSpeedRequest(speedInc, true);
        }

        /// <summary>
        /// Sets the default velocity new actions will be run at.
        /// </summary>
        /// <param name="speed"></param>
        public void SpeedTo(int speed)
        {
            c.IssueSpeedRequest(speed, false);
        }

        /// <summary>
        /// Gets the current zone setting.
        /// </summary>
        /// <returns></returns>
        [System.Obsolete("Deprecated method, use Precision() instead")]
        public int Zone()
        {
            return c.GetCurrentPrecisionSettings();
        }

        /// <summary>
        /// Increase the default zone value new actions will be given.
        /// </summary>
        /// <param name="zoneInc"></param>
        [System.Obsolete("Deprecated method, use Precision(radiusInc) instead")]
        public void Zone(int zoneInc)
        {
            c.IssuePrecisionRequest(zoneInc, true);
        }

        /// <summary>
        /// Sets the default zone value new actions will be given.
        /// </summary>
        /// <param name="zone"></param>
        [System.Obsolete("Deprecated method, use PrecisionTo(radius) instead")]
        public void ZoneTo(int zone)
        {
            c.IssuePrecisionRequest(zone, false);
        }

        /// <summary>
        /// Increase the default precision value new actions will be given. 
        /// Precision is measured as the radius of the smooth interpolation
        /// between motion targets. This is refered to as "Zone", "Approximate
        /// Positioning" or "Blending Radius" in different platforms. 
        /// </summary>
        /// <param name="radiusInc">Smoothing radius increment in mm</param>
        public void Precision(int radiusInc)
        {
            c.IssuePrecisionRequest(radiusInc, true);
        }

        /// <summary>
        /// Set the default precision value new actions will be given. 
        /// Precision is measured as the radius of the smooth interpolation
        /// between motion targets. This is refered to as "Zone", "Approximate
        /// Positioning" or "Blending Radius" in different platforms. 
        /// </summary>
        /// <param name="radius">Smoothing radius in mm</param>
        public void PrecisionTo(int radius)
        {
            c.IssuePrecisionRequest(radius, false);
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
        /// Sets the motion type (linear, joint...) for future issued actions.
        /// </summary>
        /// <param name="motionType"></param>
        public bool MotionMode(MotionType motionType)
        {
            return c.IssueMotionRequest(motionType);
        }

        /// <summary>
        /// Sets the motion type (linear, joint...) for future issued actions.
        /// </summary>
        /// <param name="motionType">"linear", "joint", etc."</param>
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
                foreach(string str in Enum.GetNames(typeof(MotionType)))
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





        //        /// What was this even for?

        //        public bool IsBrand(string brandName)
        //{
        //    return c.robotBrand.ToString().ToUpper().Equals(brandName.ToUpper());
        //}


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
            return RotateTo((Rotation) cs);
        }

        /// <summary>
        /// Issue an ABSOLUTE reorientation request according to the current reference system.
        /// </summary>
        /// <param name="vecX"></param>
        /// <param name="vecY"></param>
        /// <returns></returns>
        public bool RotateTo(Vector vecX, Vector vecY)
        {
            return RotateTo((Rotation) new Orientation(vecX, vecY));
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
            return RotateTo((Rotation) new Orientation(x0, x1, x2, y0, y1, y2));
        }

        /// <summary>
        /// Issue a compound RELATIVE local Translation + Rotation request
        /// according to the current reference system.
        /// Note that, if using local coordinates, order of actions will matter.  // TODO: wouldn't they matter too if the are in global coordinates?
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
        /// Note that, if using local coordinates, order of actions will matter. // TODO: wouldn't they matter too if the are in global coordinates?
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
        public bool WriteDigital(int pinNumber, bool isOn)
        {
            return c.IssueWriteToDigitalIORequest(pinNumber, isOn);
        }

        /// <summary>
        /// Writes to the analog IO pin.
        /// </summary>
        /// <param name="pinNumber"></param>
        /// <param name="value"></param>
        public bool WriteAnalog(int pinNumber, double value)
        {
            return c.IssueWriteToAnalogIORequest(pinNumber, value);
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

        /// <summary>
        /// Turn digital IO on. Is alias for `WriteDigital(pinNumber, true)`
        /// </summary>
        /// <param name="pinNumber"></param>
        public bool TurnOn(int pinNumber)
        {
            return this.WriteDigital(pinNumber, true);
        }

        /// <summary>
        /// Turn digital IO off. Is alias for `WriteDigital(pinNumber, false)`
        /// </summary>
        /// <param name="pinNumber"></param>
        public bool TurnOff(int pinNumber)
        {
            return this.WriteDigital(pinNumber, false);
        }

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





        //   ██████╗ ███████╗████████╗████████╗███████╗██████╗ ███████╗
        //  ██╔════╝ ██╔════╝╚══██╔══╝╚══██╔══╝██╔════╝██╔══██╗██╔════╝
        //  ██║  ███╗█████╗     ██║      ██║   █████╗  ██████╔╝███████╗
        //  ██║   ██║██╔══╝     ██║      ██║   ██╔══╝  ██╔══██╗╚════██║
        //  ╚██████╔╝███████╗   ██║      ██║   ███████╗██║  ██║███████║
        //   ╚═════╝ ╚══════╝   ╚═╝      ╚═╝   ╚══════╝╚═╝  ╚═╝╚══════╝
        //                                                             
        /// <summary>
        /// Returns a Point represnting the current location of the Tool Center Point
        /// (if there is a Tool attached) or the Flange Center Point (if there isn't).
        /// </summary>
        /// <returns></returns>
        public Point GetPosition()
        {
            return c.GetCurrentPosition();
        }

        /// <summary>
        /// Return a Rotation object representing the current rotation of the Tool Center Point
        /// (if there is a Tool attached) or the Flange Center Point (if there isn't).
        /// </summary>
        /// <returns></returns>
        public Rotation GetRotation()
        {
            return c.GetCurrentOrientation();
        }

        /// <summary>
        /// Return a Orientation object representing the current orientation of the Tool Center Point
        /// (if there is a Tool attached) or the Flange Center Point (if there isn't).
        /// </summary>
        /// <returns></returns>
        public Orientation GetOrientation()
        {
            return (Orientation) c.GetCurrentOrientation();
        }

        /// <summary>
        /// Returns a Joint object representing the rotations in the robot axes.
        /// </summary>
        /// <returns></returns>
        public Joints GetAxes()
        {
            return c.getCurrentAxes();
        }

        /// <summary>
        /// Returns the Tool object currently attached to this Robot, null if none.
        /// </summary>
        /// <returns>The Tool object currently attached to this Robot, null if none.</returns>
        public Tool GetTool()
        {
            return c.GetCurrentTool();
        }










        //// @TODO: feels like Path is too much of a spatial geometry description. 
        //// Should it become something more action-related? Like Instructions, Program, Commands?
        //// Could implement Procedure, as an ordered collection of abstract Actions...
        //public bool Follow(Path path)
        //{
        //    throw new NotImplementedException();
        //}













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
        /// Dumps a list of the remaining buffered actions.
        /// </summary>
        public void DebugBuffer()
        {
            c.DebugBuffer();
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


        public override string ToString()
        {
            return string.Format("Robot[\"{0}\", {1}]",
                this.Name,
                this.c.robotBrand);
        }

    }
}
