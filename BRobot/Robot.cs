using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace BRobot
{
    /// <summary>
    /// Represents the type of control that will be performed over the real/virtual robot.
    /// </summary>
    public enum ControlMode : int
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
    /// Defines the different modes a program can be ran.
    /// </summary>
    public enum RunMode : int
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
    /// Defines which type of motion to use for translation actions.;
    /// </summary>
    public enum MotionType : int
    {
        Undefined = 0,
        Linear = 1,
        Joint = 2
    }

    public enum RobotType : int
    {
        Undefined = 0,
        ABB = 1,
        UR = 2,
        KUKA = 3
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
    /// The core Class in BRobot. Represents a state and action-based virtual robot, 
    /// and exposes the public API for robot manipulation and control.
    /// </summary>
    public class Robot
    {
        /// <summary>
        /// Build number.
        /// </summary>
        public static readonly int Build = 1207;

        /// <summary>
        /// Version number.
        /// </summary>
        public static readonly string Version = "0.2.3";
       



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
        public Robot() : this("HUMAN") { }

        /// <summary>
        /// Base constructor.
        /// </summary>
        /// <param name="brand"></param>
        public Robot(string brand)
        {
            string b = brand.ToUpper();
            
            if (b.Equals("ABB"))
            {
                c = new Control(this, RobotType.ABB);
            }
            else if (b.Equals("UR"))
            {
                c = new Control(this, RobotType.UR);
            }
            else if (b.Equals("KUKA"))
            {
                c = new Control(this, RobotType.KUKA);
            } 
            else if (b.Equals("HUMAN"))
            {
                c = new Control(this, RobotType.Undefined);
            }
            else
            {
                Console.WriteLine(brand + " is not a valis Robot type. Please specify one of the following: ");
                for (var i = 1; i < 4; i++)
                {
                    Console.WriteLine(((RobotType)i).ToString());
                }
                c = new Control(this, RobotType.Undefined);
            }
        }


        public bool IsBrand(string brandName)
        {
            brandName = brandName.ToUpper();

            switch (c.robotBrand)
            {
                case RobotType.Undefined:
                    return brandName.Equals("HUMAN");

                case RobotType.ABB:
                    return brandName.Equals("ABB");

                case RobotType.UR:
                    return brandName.Equals("UR");

                case RobotType.KUKA:
                    return brandName.Equals("KUKA");
            }

            return false;
        }




        /// <summary>
        /// Sets the control mode the robot will operate under.
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public bool Mode(ControlMode mode)
        {
            return c.SetControlMode(mode);
        }

        /// <summary>
        /// Sets the control mode the robot will operate under.
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public bool Mode(string mode)
        {
            mode = mode.ToLower();
            bool success = true;
            if (mode.Equals("offline"))
            {
                return Mode(BRobot.ControlMode.Offline);
            }
            else if (mode.Equals("execute"))
            {
                return Mode(BRobot.ControlMode.Execute);
            }
            else if (mode.Equals("stream"))
            {
                return Mode(BRobot.ControlMode.Stream);
            }
            else
            {
                Console.WriteLine("ConnectionMode '" + mode + "' is not available.");
                success = false;
            }
            return success;
        }
        
        /// <summary>
        /// Sets the cycle the robot will run program in (Once or Loop).
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public bool RunMode(RunMode mode)
        {
            return c.SetRunMode(mode);
        }

        /// <summary>
        /// Sets the cycle the robot will run program in (Once or Loop).
        /// </summary>
        /// <param name="mode"></param>
        public bool RunMode(string mode)
        {
            mode = mode.ToLower();

            if (mode.Equals("once"))
            {
                return RunMode(global::BRobot.RunMode.Once);
            }
            else if (mode.Equals("loop"))
            {
                return RunMode(global::BRobot.RunMode.Once);
            }
            else
            {
                Console.WriteLine("RunMode '" + mode + "' is not available.");
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
        /// Returns a Rottaion representation of the Robot's TCP orientation in quaternions.
        /// </summary>
        /// <returns></returns>
        public Rotation GetCurrentOrientation()
        {
            return c.GetCurrentOrientation();
        }

        ///// <summary>
        ///// Returns a Frame object representing the Robot's current TCP position and orientation.
        ///// NOTE: the Frame's velocity and zone are still not representative of the current state.
        ///// </summary>
        ///// <returns></returns>
        //public Frame GetCurrentFrame()
        //{
        //    return c.GetCurrentFrame();
        //}

        /// <summary>
        /// Returns a Joint object representing the current angular rotations of the robot's 6 axes.
        /// </summary>
        /// <returns></returns>
        public Joints GetCurrentJoints()
        {
            return c.GetCurrentJoints();
        }

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
            return c.Export(true);
        }

        /// <summary>
        /// Create a program with all the buffered actions and return it as a string List.
        /// Note all buffered actions will be removed from the queue.
        /// </summary>
        /// <param name="inlineTargets">Write inline targets on action statements, or declare them as independent variables?</param>
        /// <returns></returns>
        public List<string> Export(bool inlineTargets)
        {
            return c.Export(inlineTargets);
        }

        /// <summary>
        /// Create a program with all the buffered actions and save it to a file. 
        /// Note all buffered actions will be removed from the queue.
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public bool Export(string filepath)
        {
            return c.Export(filepath, true);
        }

        /// <summary>
        /// Create a program with all the buffered actions and save it to a file. 
        /// Note all buffered actions will be removed from the queue.
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="inlineTargets">Write inline targets on action statements, or declare them as independent variables?</param>
        /// <returns></returns>
        public bool Export(string filepath, bool inlineTargets)
        {
            return c.Export(filepath, inlineTargets);
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
        public int Zone()
        {
            return c.GetCurrentZoneSetting();
        }

        /// <summary>
        /// Increase the default zone value new actions will be given.
        /// </summary>
        /// <param name="zoneInc"></param>
        public void Zone(int zoneInc)
        {
            c.IssueZoneRequest(zoneInc, true);
        }

        /// <summary>
        /// Sets the default zone value new actions will be given.
        /// </summary>
        /// <param name="zone"></param>
        public void ZoneTo(int zone)
        {
            c.IssueZoneRequest(zone, false);
        }

        /// <summary>
        /// Gets the current MotionType setting.
        /// </summary>
        /// <returns></returns>
        public MotionType Motion()
        {
            return c.GetCurrentMotionTypeSetting();
        }

        /// <summary>
        /// Sets the motion type (linear, joint...) for future issued actions.
        /// </summary>
        /// <param name="type"></param>
        public void Motion(MotionType type)
        {
            c.IssueMotionRequest(type);
        }

        /// <summary>
        /// Sets the motion type (linear, joint...) for future issued actions.
        /// </summary>
        /// <param name="type">"linear", "joint" or "joints"</param>
        public void Motion(string type)
        {
            MotionType t = MotionType.Undefined;
            type = type.ToLower();
            if (type.Equals("linear"))
            {
                t = MotionType.Linear;
            }
            else if (type.Equals("joint"))
            {
                t = MotionType.Joint;
            }

            if (t == MotionType.Undefined)
            {
                Console.WriteLine("Invalid motion type");
                return;
            }

            Motion(t);
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
        /// Sets the reference system used for relative transformations ('local', 'global'...)
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
        /// Buffers current state settings (speed, zone, motion type...), and opens up for 
        /// temporary settings changes to be reverted by PopSettings().
        /// </summary>
        public void PushSettings()
        {
            c.IssuePushPopRequest(true);
        }

        /// <summary>
        /// Reverts the state settings (speed, zone, motion type...) to the previously buffered
        /// state by PushSettings().
        /// </summary>
        public void PopSettings()
        {
            c.IssuePushPopRequest(false);
        }



        //   █████╗  ██████╗████████╗██╗ ██████╗ ███╗   ██╗███████╗
        //  ██╔══██╗██╔════╝╚══██╔══╝██║██╔═══██╗████╗  ██║██╔════╝
        //  ███████║██║        ██║   ██║██║   ██║██╔██╗ ██║███████╗
        //  ██╔══██║██║        ██║   ██║██║   ██║██║╚██╗██║╚════██║
        //  ██║  ██║╚██████╗   ██║   ██║╚██████╔╝██║ ╚████║███████║
        //  ╚═╝  ╚═╝ ╚═════╝   ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝╚══════╝
        //                                                         

        /// <summary>
        /// Applies an action to this robot. 
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
        public bool Move(Point direction)
        {
            return c.IssueTranslationRequest(direction, true);
        }

        ///// <summary>
        ///// Issue a relative movement action request on current coordinate system.
        ///// </summary>
        ///// <param name="incX"></param>
        ///// <param name="incY"></param>
        ///// <returns></returns>
        //public bool Move(double incX, double incY)
        //{
        //    return Move(new Point(incX, incY, 0));
        //}

        /// <summary>
        /// Issue a relative movement action request on current coordinate system.
        /// </summary>
        /// <param name="incX"></param>
        /// <param name="incY"></param>
        /// <param name="incZ"></param>
        /// <returns></returns>
        public bool Move(double incX, double incY, double incZ = 0)
        {
            return Move(new Point(incX, incY, incZ));
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
            return MoveTo(new Point(x, y, z));
        }
        
        /// <summary>
        /// Issue a relative rotation action request around local/global axes.
        /// </summary>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public bool Rotate(Rotation rotation)
        {
            return c.IssueRotationRequest(rotation, true);
        }

        /// <summary>
        /// Issue a relative rotation action request around local/global axes.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="angDegs"></param>
        /// <returns></returns>
        public bool Rotate(Point vector, double angDegs)
        {
            return Rotate(new Rotation(vector, angDegs));
        }

        /// <summary>
        /// Issue a relative rotation action request around local/global axes.
        /// </summary>
        /// <param name="rotVecX"></param>
        /// <param name="rotVecY"></param>
        /// <param name="rotVecZ"></param>
        /// <param name="angDegs"></param>
        /// <returns></returns>
        public bool Rotate(double rotVecX, double rotVecY, double rotVecZ, double angDegs)
        {
            return Rotate(new Rotation(new Point(rotVecX, rotVecY, rotVecZ), angDegs));
        }
                
        /// <summary>
        /// Issue an absolute global reorientation request.
        /// </summary>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public bool RotateTo(Rotation rotation)
        {
            return c.IssueRotationRequest(rotation, false);
        }

        /// <summary>
        /// Issue an absolute global reorientation request.
        /// </summary>
        /// <param name="cs"></param>
        /// <returns></returns>
        public bool RotateTo(CoordinateSystem cs)
        {
            return RotateTo(cs.GetQuaternion());
        }

        /// <summary>
        /// Issue an absolute global reorientation request.
        /// </summary>
        /// <param name="vecX"></param>
        /// <param name="vecY"></param>
        /// <returns></returns>
        public bool RotateTo(Point vecX, Point vecY)
        {
            return RotateTo(new Rotation(vecX, vecY));
        }

        /// <summary>
        /// Issue an absolute global reorientation request.
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
            return RotateTo(new Rotation(x0, x1, x2, y0, y1, y2));
        }

        /// <summary>
        /// Issue a compound relative local Translation + Rotation request. 
        /// Note that, if using local coordinates, order of actions will matter.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public bool Transform(Point position, Rotation rotation)
        {
            // Note the T+R action order
            //return c.IssueTranslationAndRotationRequest(position, true, rotation, true);

            return c.IssueTransformationRequest(position, rotation, true, true);
        }

        /// <summary>
        /// Issue a compound relative local Rotation + Translation request.
        /// Note that, if using local coordinates, order of actions will matter.
        /// </summary>
        /// <param name="rotation"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool Transform(Rotation rotation, Point position)
        {
            // Note the R+T action order
            //return c.IssueRotationAndTranslationRequest(rotation, true, position, true);

            return c.IssueTransformationRequest(position, rotation, true, false);
        }
        
        /// <summary>
        /// Issue a compound absolute global Translation + Rotation request.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public bool TransformTo(Point position, Rotation rotation)
        {
            // Action order is irrelevant in absolute mode (since translations are applied based on immutable world XYZ)
            //return c.IssueTranslationAndRotationRequest(true, position, false, true, rotation, false);

            return c.IssueTransformationRequest(position, rotation, false, true);
        }

        /// <summary>
        /// Issue a compound absolute global Translation + Rotation request.
        /// </summary>
        /// <param name="rotation"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool TransformTo(Rotation rotation, Point position)
        {
            // Action order is irrelevant in absolute mode (since translations are applied based on immutable world XYZ)
            //return c.IssueTranslationAndRotationRequest(true, position, false, true, rotation, false);

            return c.IssueTransformationRequest(position, rotation, false, false);
        }


        /// <summary>
        /// Issue a request to increment the angular values of the robot joint rotations.
        /// Values expressed in degrees.
        /// </summary>
        /// <param name="incJoints"></param>
        /// <returns></returns>
        public bool Joints(Joints incJoints)
        {
            return c.IssueJointsRequest(incJoints, true);
        }

        /// <summary>
        /// Issue a request to increment the angular values of the robot joint rotations.
        /// Values expressed in degrees.
        /// </summary>
        /// <param name="incJ1"></param>
        /// <param name="incJ2"></param>
        /// <param name="incJ3"></param>
        /// <param name="incJ4"></param>
        /// <param name="incJ5"></param>
        /// <param name="incJ6"></param>
        /// <returns></returns>
        public bool Joints(double incJ1, double incJ2, double incJ3, double incJ4, double incJ5, double incJ6)
        {
            return c.IssueJointsRequest(new Joints(incJ1, incJ2, incJ3, incJ4, incJ5, incJ6), true);
        }

        /// <summary>
        /// Issue a request to set the angular values of the robot joint rotations.
        /// Values expressed in degrees.
        /// </summary>
        /// <param name="joints"></param>
        /// <returns></returns>
        public bool JointsTo(Joints joints)
        {
            return c.IssueJointsRequest(joints, false);
        }

        /// <summary>
        /// Issue a request to set the angular values of the robot joint rotations.
        /// Values expressed in degrees.
        /// </summary>
        /// <param name="j1"></param>
        /// <param name="j2"></param>
        /// <param name="j3"></param>
        /// <param name="j4"></param>
        /// <param name="j5"></param>
        /// <param name="j6"></param>
        /// <returns></returns>
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
        
    }
}
