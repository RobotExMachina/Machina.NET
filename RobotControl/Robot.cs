using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;





namespace RobotControl
{
    /// <summary>
    /// Is this Robot connected to the application? 
    /// Offline mode is used to generate robotic programs offline,
    /// Online mode is meant to be used to send real-time instructions 
    /// to a robot connected to the mahcine running the application.
    /// </summary>
    public enum ConnectionMode : int { Offline = 1, Online = 2 };

    /// <summary>
    /// Different operating modes for Online control. 
    /// Instruct loads and executes entire modules to the controller (slower), 
    /// Stream overrides targets on the fly (faster)
    /// </summary>
    public enum OnlineMode : int { Instruct = 1, Stream = 2 };

    /// <summary>
    /// Defines the different modes a program can be ran.
    /// </summary>
    public enum RunMode : int { None = 0, Once = 1, Loop = 2 };


    //██████╗  ██████╗ ██████╗  ██████╗ ████████╗
    //██╔══██╗██╔═══██╗██╔══██╗██╔═══██╗╚══██╔══╝
    //██████╔╝██║   ██║██████╔╝██║   ██║   ██║   
    //██╔══██╗██║   ██║██╔══██╗██║   ██║   ██║   
    //██║  ██║╚██████╔╝██████╔╝╚██████╔╝   ██║   
    //╚═╝  ╚═╝ ╚═════╝ ╚═════╝  ╚═════╝    ╚═╝   
    /// <summary>
    /// The core Class in RobotControl. Represents a state & action-based virtual robot, 
    /// and exposes the public API for robot manipulation and control.
    /// </summary>
    public class Robot
    {
        /// <summary>
        /// Build number
        /// </summary>
        public static readonly int Build = 1100;

        // Private properties
        private Control c;  // the main control object

 
        
        //██████╗ ██╗   ██╗██████╗ ██╗     ██╗ ██████╗     █████╗ ██████╗ ██╗
        //██╔══██╗██║   ██║██╔══██╗██║     ██║██╔════╝    ██╔══██╗██╔══██╗██║
        //██████╔╝██║   ██║██████╔╝██║     ██║██║         ███████║██████╔╝██║
        //██╔═══╝ ██║   ██║██╔══██╗██║     ██║██║         ██╔══██║██╔═══╝ ██║
        //██║     ╚██████╔╝██████╔╝███████╗██║╚██████╗    ██║  ██║██║     ██║
        //╚═╝      ╚═════╝ ╚═════╝ ╚══════╝╚═╝ ╚═════╝    ╚═╝  ╚═╝╚═╝     ╚═╝
        /// <summary>
        /// Base constructor
        /// </summary>                                                       
        public Robot()
        {
            c = new Control();
            c.Reset();
        }

        /// <summary>
        /// Sets ConnectionMode for this robot.
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public bool ConnectionMode(ConnectionMode mode)
        {
            //connectionMode = mode;
            c.SetConnectionMode(mode);
            return true;
        }

        /// <summary>
        /// Sets ConnectionMode for this robot.
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public bool ConnectionMode(string mode)
        {
            mode = mode.ToLower();
            bool success = true;
            if (mode.Equals("offline"))
            {
                //connectionMode = RobotControl.ConnectionMode.Offline;
                return ConnectionMode(RobotControl.ConnectionMode.Offline);
            }
            else if (mode.Equals("online"))
            {
                //connectionMode = RobotControl.ConnectionMode.Online;
                return ConnectionMode(RobotControl.ConnectionMode.Online);
            }
            else
            {
                Console.WriteLine("ConnectionMode '" + mode + "' is not available.");
                success = false;
            }
            return success;
        }

        /// <summary>
        /// Sets OnlineMode type for this robot.
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public bool OnlineMode(OnlineMode mode)
        {
            //onlineMode = mode;
            return c.SetOnlineMode(mode);
        }

        /// <summary>
        /// Sets OnlineMode type for this robot.
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public bool OnlineMode(string mode)
        {
            mode = mode.ToLower();
            bool success = true;
            if (mode.Equals("instruct"))
            {
                //onlineMode = RobotControl.OnlineMode.Instruct;
                return OnlineMode(RobotControl.OnlineMode.Instruct);
            }
            else if (mode.Equals("stream"))
            {
                //onlineMode = RobotControl.OnlineMode.Stream;
                return OnlineMode(RobotControl.OnlineMode.Stream);
            }
            else
            {
                Console.WriteLine("OnlineMode '" + mode + "' is not available.");
                success = false;
            }
            return success;
        }

        public bool RunMode(RunMode mode)
        {
            return c.SetRunMode(mode);
        }


        /// <summary>
        /// Allows to toggle between "once" and "loop" modes.
        /// </summary>
        /// <param name="mode"></param>
        public bool RunMode(string mode)
        {
            //if (connectionMode == RobotControl.ConnectionMode.Offline)
            //{
            //    Console.WriteLine("Cannot set RunMode in Offline mode");
            //    return;
            //}

            //if (isConnected)
            //{
            //    using (Mastership.Request(controller.Rapid))
            //    {
            //        controller.Rapid.Cycle = mode.ToLower().Equals("loop") ? ExecutionCycle.Forever : ExecutionCycle.Once;
            //        //if (DEBUG) Console.WriteLine("RunMode set to " + controller.Rapid.Cycle);
            //    }
            //} 
            //else
            //{
            //    Console.WriteLine("Not connected to controller");
            //}

            mode = mode.ToLower();
            bool success = true;
            if (mode.Equals("once"))
            {
                //onlineMode = RobotControl.OnlineMode.Instruct;
                return RunMode(RobotControl.RunMode.Once);
            }
            else if (mode.Equals("loop"))
            {
                //onlineMode = RobotControl.OnlineMode.Stream;
                return RunMode(RobotControl.RunMode.Once);
            }
            else
            {
                Console.WriteLine("RunMode '" + mode + "' is not available.");
                success = false;
            }
            return success;

        }


        /// <summary>
        /// In 'online' modes, performs all necessary instructions to connect to the robot controller. 
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public bool Connect(int robotId)
        {
            //if (DEBUG) Console.WriteLine("Initializing connection to controller...");
            //if (connectionMode == RobotControl.ConnectionMode.Offline)
            //{
            //    Console.WriteLine("ConnectionMode is currently set to offline");
            //    return false;
            //}
            //return ConnectToController(onlineMode);
            return c.ConnectToController(robotId);
        }

        public bool Connect()
        {
            return c.ConnectToController(0);
        }

        /// <summary>
        /// In 'online' modes, performs all necessary instructions to disconnect from the controller.
        /// </summary>
        public void Disconnect()
        {
            //if (DEBUG) Console.WriteLine("Disconnecting from controller on " + IP);
            //if (SafetyStopImmediateOnDisconnect) StopProgram(true);
            c.DisconnectFromController();
        }

        public string GetIP()
        {
            return c.GetIP();
        }

        ///// <summary>
        ///// Loads a module to the controller from a local file. 
        ///// TODO: By default, it will wipe out any other modules in the task, add the possibility to choose.
        ///// </summary>
        ///// <param name="filename"></param>
        ///// <param name="filepath"></param>
        //public void LoadModule(string filename, string filepath)
        //{
        //    if (connectionMode == RobotControl.ConnectionMode.Offline)
        //    {
        //        Console.WriteLine("Cannot load modules in Offline mode");
        //        return;
        //    }
        //    LoadModuleFromFilename(filename, filepath);
        //}

        /// <summary>
        /// Loads a module to the controller from a local file.
        /// </summary>
        /// <param name="filepath">Full absolute filepath, including root, folders and filename with extension</param>
        /// <returns></returns>
        public bool LoadModule(string filepath)
        {
            return c.LoadModuleFromFilepath(filepath);
        }

        

        /// <summary>
        /// Starts execution of the current module/s in the controller.
        /// @TODO: The behavior of this method will change depending based on Off/Online mode
        /// </summary>
        public void Start()
        {
            c.ResetProgramPointer();
            c.StartProgram();
        }

        /// <summary>
        /// Stops execution of the current module/s in the controller immediately. 
        /// Use StopAfterProgram() to schedule robot atop after completion of current cycle.
        /// </summary>
        public void Stop()
        {
            c.StopProgram(true);
        }

        ///// <summary>
        ///// Returns a string representation of the end frame's TCP position in mm.
        ///// </summary>
        ///// <returns></returns>
        //public string GetPosition()
        //{
        //    RobTarget rt = c.GetTCPRobTarget();
        //    return rt.Trans.ToString();
        //}

        /// <summary>
        /// Returns a Point representation of the Robot's TCP position in mm and World coordinates.
        /// </summary>
        /// <returns></returns>
        public Point GetCurrentPosition()
        {
            return c.GetTCPPosition();
        }

        ///// <summary>
        ///// Returns a string representation of the end frame's TCP orientation in quaterions.
        ///// </summary>
        ///// <returns></returns>
        //public string GetOrientation()
        //{
        //    RobTarget rt = GetTCPRobTarget();
        //    return rt.Rot.ToString();
        //}

        /// <summary>
        /// Returns a Rottaion representation of the Robot's TCP orientation in quaternions.
        /// </summary>
        /// <returns></returns>
        public Rotation GetCurrentOrientation()
        {
            return c.GetTCPRotation();
        }

        /// <summary>
        /// Returns a Frame object representing the robot's current TCP position and orientation.
        /// NOTE: the Frame's velocity and zone are still not representative of the current state.
        /// </summary>
        /// <returns></returns>
        public Frame GetCurrentFrame()
        {
            return c.GetTCPFrame();
        }

        ///// <summary>
        ///// Returns a string representation of the robot's joint rotations in degrees.
        ///// </summary>
        ///// <returns></returns>
        //public string GetJoints()
        //{
        //    RobJoint rj = c.GetRobotJoints();
        //    return rj.ToString();
        //}

        /// <summary>
        /// Returns a Joint object representing the current angular rotations of the robot's 6 axes
        /// </summary>
        /// <returns></returns>
        public Joints GetCurrentJoints()
        {
            return c.GetRobotJoints();
        }

        /// <summary>
        /// Loads the path to the queue manager and triggers execution of the program if applicable
        /// </summary>
        /// <param name="path"></param>
        public void LoadPath(Path path)
        {
            c.AddPathToQueue(path);
            c.TriggerQueue();
        }

        /// <summary>
        /// Stops the robot after execution of current program. This will also clear the queue.
        /// </summary>
        public void StopAfterProgram()
        {
            c.ClearQueue();
            c.StopProgram(false);
        }

        /// <summary>
        /// Dumps a ton of information to the console about the controller, the main task, etc.
        /// </summary>
        public void DebugDump()
        {
            c.DebugBanner();
            c.TestMastership();
            c.DebugControllerDump();
            c.DebugTaskDump();
        }
        
        /// <summary>
        /// Sets the default velocity new actions will be run at.
        /// </summary>
        /// <param name="vel"></param>
        public void SetVelocity(double vel)
        {
            //c.currentVelocity = vel;
            c.SetCurrentVelocity(vel);
        }

        /// <summary>
        /// Sets the default zone value new actions will be given.
        /// </summary>
        /// <param name="zone"></param>
        public void SetZone(double zone)
        {
            //c.currentZone = zone;
            c.SetCurrentZone(zone);
        }

        /// <summary>
        /// Issue a relative movement action request.
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public bool Move(Point dir)
        {
            return Move(dir.X, dir.Y, dir.Z);
        }
        
        /// <summary>
        /// Issue a relative movement action request.
        /// </summary>
        /// <param name="incX"></param>
        /// <param name="incY"></param>
        /// <param name="incZ"></param>
        /// <returns></returns>
        public bool Move(double incX, double incY, double incZ)
        {
            //if (onlineMode != RobotControl.OnlineMode.Stream)
            //{
            //    Console.WriteLine("Move() only supported in Stream mode");
            //    return false;
            //}

            //if (SafetyCheckTableCollision)
            //{
            //    if (IsBelowTable(TCPPosition.Z + incZ))
            //    {
            //        Console.WriteLine("WARNING: TCP ABOUT TO HIT THE TABLE");
            //        if (SafetyStopOnTableCollision)
            //        {
            //            return false;
            //        }
            //    }
            //}

            //TCPPosition.Add(incX, incY, incZ);
            //AddFrameToStreamQueue(new Frame(TCPPosition.X, TCPPosition.Y, TCPPosition.Z, currentVelocity, currentZone));

            //// Only tick queue if there are no targets pending to be streamed
            //if (streamQueue.FramesPending() == 1 )
            //{
            //    TickStreamQueue(false);
            //} else
            //{
            //    Console.WriteLine("{0} frames pending", streamQueue.FramesPending());
            //}

            //return true;

            return c.RequestMove(incX, incY, incZ);
        }

        /// <summary>
        /// Issue an absolute movement action request.
        /// </summary>
        /// <param name="newX"></param>
        /// <param name="newY"></param>
        /// <param name="newZ"></param>
        /// <returns></returns>
        public bool MoveTo(double newX, double newY, double newZ)
        {
            //if (onlineMode != RobotControl.OnlineMode.Stream)
            //{
            //    Console.WriteLine("MoveTo() only supported in Stream mode");
            //    return false;
            //}

            //if (SafetyCheckTableCollision)
            //{
            //    if (IsBelowTable(newZ))
            //    {
            //        Console.WriteLine("WARNING: TCP ABOUT TO HIT THE TABLE");
            //        if (SafetyStopOnTableCollision)
            //        {
            //            return false;
            //        }
            //    }
            //}

            //TCPPosition.Set(newX, newY, newZ);
            ////AddFrameToStreamQueue(new Frame(newX, newY, newZ, currentVelocity, currentZone));
            //AddFrameToStreamQueue(new Frame(TCPPosition.X, TCPPosition.Y, TCPPosition.Z,
            //   TCPRotation.Q1, TCPRotation.Q2, TCPRotation.Q3, TCPRotation.Q4,
            //   currentVelocity, currentZone));

            //// Only tick queue if there are no targets pending to be streamed
            //if (streamQueue.FramesPending() == 1)
            //{
            //    TickStreamQueue(false);
            //}
            //else
            //{
            //    Console.WriteLine("{0} frames pending", streamQueue.FramesPending());
            //}

            //return true;

            return c.RequestMoveTo(newX, newY, newZ);
        }

        /// <summary>
        /// Issue an absolute movement action request.
        /// </summary>
        /// <param name="bookmarkTarget"></param>
        /// <returns></returns>
        public bool MoveTo(string bookmarkTarget)
        {
            string str = bookmarkTarget.ToLower();

            if (str.Equals("home"))
            {
                return MoveTo(300, 0, 550);  // @TODO: this should issue a MoveAbsJ(0,0,0,0,0,0) or similar
            }
            else
            {
                Console.WriteLine("Named position '{0}' not found", bookmarkTarget);
            }

            return false;
        }

        /// <summary>
        /// Issue an absolute rotation of the TCP action request.
        /// </summary>
        /// <param name="q1"></param>
        /// <param name="q2"></param>
        /// <param name="q3"></param>
        /// <param name="q4"></param>
        /// <returns></returns>
        public bool RotateTo(double q1, double q2, double q3, double q4)
        {
            //if (onlineMode != RobotControl.OnlineMode.Stream)
            //{
            //    Console.WriteLine("RotateTo() only supported in Stream mode");
            //    return false;
            //}

            //// WARNING: NO TABLE COLLISIONS ARE PERFORMED HERE YET!
            //TCPRotation.Set(q1, q2, q3, q4);
            //AddFrameToStreamQueue(new Frame(TCPPosition.X, TCPPosition.Y, TCPPosition.Z,
            //   TCPRotation.Q1, TCPRotation.Q2, TCPRotation.Q3, TCPRotation.Q4,
            //   currentVelocity, currentZone));

            //// Only tick queue if there are no targets pending to be streamed
            //if (streamQueue.FramesPending() == 1)
            //{
            //    TickStreamQueue(false);
            //}
            //else
            //{
            //    Console.WriteLine("{0} frames pending", streamQueue.FramesPending());
            //}

            //return true;

            return c.RequestRotateTo(q1, q2, q3, q4);
        }

        /// <summary>
        /// Issue an absolute rotation of the TCP action request.
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="x3"></param>
        /// <param name="y1"></param>
        /// <param name="y2"></param>
        /// <param name="y3"></param>
        /// <param name="z1"></param>
        /// <param name="z2"></param>
        /// <param name="z3"></param>
        /// <returns></returns>
        public bool RotateTo(double x1, double x2, double x3, double y1, double y2, double y3, double z1, double z2, double z3)
        {

            List<double> q = Rotation.PlaneToQuaternions(x1, x2, x3, y1, y2, y3, z1, z2, z3);
            return RotateTo(q[0], q[1], q[2], q[3]);
        }

        /// <summary>
        /// Issue an absolute rotation of the TCP action request.
        /// </summary>
        /// <param name="rot"></param>
        /// <returns></returns>
        public bool RotateTo(Rotation rot)
        {
            return RotateTo(rot.Q1, rot.Q2, rot.Q3, rot.Q4);
        }

        /// <summary>
        /// Issue an absolute rotation of the TCP action request.
        /// </summary>
        /// <param name="bookmarkRotation"></param>
        /// <returns></returns>
        public bool RotateTo(string bookmarkRotation)
        {
            string str = bookmarkRotation.ToLower();

            if (str.Equals("globalxy"))
            {
                return RotateTo(Rotation.GlobalXY);
            }
            else
            {
                Console.WriteLine("Named rotation '{0}' not found", bookmarkRotation);
            }

            return false;
        }

        

    }
}
