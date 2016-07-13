using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ABB.Robotics;
using ABB.Robotics.Controllers;
using ABB.Robotics.Controllers.Discovery;
using ABB.Robotics.Controllers.RapidDomain;  // This is for the Task Class
using ABB.Robotics.Controllers.EventLogDomain;
using ABB.Robotics.Controllers.FileSystemDomain;




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
        // Some 'environment variables' to define check states and behavior
        private static readonly bool SafetyStopImmediateOnDisconnect = true;
        private static readonly bool SafetyCheckTableCollision = true;
        private static readonly bool SafetyStopOnTableCollision = true;
        private static readonly double SafetyTableZLimit = 100;                     // table security checks will trigger under this z height (mm)
        private static readonly bool DEBUG = true;                                  // dump a bunch of debug logs

        /// <summary>
        /// Come route names to be used for file handling
        /// </summary>
        private static string localBufferPathname = @"C:\";
        private static string localBufferFilename = "buffer.mod";
        private static string remoteBufferDirectory = "RobotControl";

        /// <summary>
        /// A shared instance of a Thread to manage uploading modules
        /// to the controller, which typically takes a lot of resources
        /// and halts program execution
        /// </summary>
        private Thread pathExecuter;

        /// <summary>
        /// Instances of the main robot Controller and Task
        /// </summary>
        public Controller controller;                                               // @TODO: make this private (made it public for quick debugging)
        public ABB.Robotics.Controllers.RapidDomain.Task mainTask;                  // @TODO: make this private (made it public for quick debugging)

        /// <summary>
        /// Connection modes by default
        /// </summary>
        private ConnectionMode connectionMode = RobotControl.ConnectionMode.Online;  // Try online by default (@TODO: at some point 'offline' should be the default)
        private OnlineMode onlineMode = RobotControl.OnlineMode.Instruct;            // Instruct mode by default

        /// <summary>
        /// The queue that manages what instructions get sent to the robot
        /// </summary>
        private Queue queue;


        // STUFF NEEDED FOR STREAM MODE
        // Most of it represents a virtual current state of the robot, to be able to 
        // issue appropriate relative actions.
        private Point TCPPosition = null;
        private Rotation TCPRotation = null;
        private double currentVelocity = 10;
        private double currentZone = 5;

        private StreamQueue streamQueue;
        private bool mHeld = false;                                                 // is Mastership currently held by someone? useful when several threads want to write to the robot...
        private object rapidDataLock = new object();

        
        // Public properties
        public bool isConnected { get; protected set; }
        public bool isLogged { get; protected set; }
        public bool isMainTaskRetrieved { get; private set; }
        public string IP { get; protected set; }

        



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
            Reset();
        }

        /// <summary>
        /// Sets ConnectionMode for this robot.
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public bool ConnectionMode(ConnectionMode mode)
        {
            connectionMode = mode;
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
                connectionMode = RobotControl.ConnectionMode.Offline;
            }
            else if (mode.Equals("online"))
            {
                connectionMode = RobotControl.ConnectionMode.Online;
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
            onlineMode = mode;
            return true;
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
                onlineMode = RobotControl.OnlineMode.Instruct;
            }
            else if (mode.Equals("stream"))
            {
                onlineMode = RobotControl.OnlineMode.Stream;
            }
            else
            {
                Console.WriteLine("OnlineMode '" + mode + "' is not available.");
                success = false;
            }
            return success;
        }


        /// <summary>
        /// In 'online' modes, performs all necessary instructions to connect to the robot controller. 
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public bool Connect()
        {
            if (DEBUG) Console.WriteLine("Initializing connection to controller...");
            if (connectionMode == RobotControl.ConnectionMode.Offline)
            {
                Console.WriteLine("ConnectionMode is currently set to offline");
                return false;
            }
            return ConnectToController(onlineMode);
        }

        /// <summary>
        /// In 'online' modes, performs all necessary instructions to disconnect from the controller.
        /// </summary>
        public void Disconnect()
        {
            if (DEBUG) Console.WriteLine("Disconnecting from controller on " + IP);
            if (SafetyStopImmediateOnDisconnect) StopProgram(true);
            DisconnectFromController();
        }

        /// <summary>
        /// Loads a module to the controller from a local file. 
        /// TODO: By default, it will wipe out any other modules in the task, add the possibility to choose.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="filepath"></param>
        public void LoadModule(string filename, string filepath)
        {
            if (connectionMode == RobotControl.ConnectionMode.Offline)
            {
                Console.WriteLine("Cannot load modules in Offline mode");
                return;
            }
            LoadModuleFromFilename(filename, filepath);
        }

        /// <summary>
        /// Allows to toggle between "once" and "loop" modes.
        /// </summary>
        /// <param name="mode"></param>
        public void RunMode(string mode)
        {
            if (connectionMode == RobotControl.ConnectionMode.Offline)
            {
                Console.WriteLine("Cannot set RunMode in Offline mode");
                return;
            }

            if (isConnected)
            {
                using (Mastership.Request(controller.Rapid))
                {
                    controller.Rapid.Cycle = mode.ToLower().Equals("loop") ? ExecutionCycle.Forever : ExecutionCycle.Once;
                    //if (DEBUG) Console.WriteLine("RunMode set to " + controller.Rapid.Cycle);
                }
            } 
            else
            {
                Console.WriteLine("Not connected to controller");
            }
        }

        /// <summary>
        /// Starts execution of the current module/s in the controller.
        /// @TODO: The behavior of this method will change depending based on Off/Online mode
        /// </summary>
        public void Start()
        {
            ResetProgramPointer();
            StartProgram();
        }

        /// <summary>
        /// Stops execution of the current module/s in the controller immediately. 
        /// Use StopAfterProgram() to schedule robot atop after completion of current cycle.
        /// </summary>
        public void Stop()
        {
            StopProgram(true);
        }

        /// <summary>
        /// Returns a string representation of the end frame's TCP position in mm.
        /// </summary>
        /// <returns></returns>
        public string GetPosition()
        {
            RobTarget rt = GetTCPRobTarget();
            return rt.Trans.ToString();
        }

        /// <summary>
        /// Returns a string representation of the end frame's TCP orientation in quaterions.
        /// </summary>
        /// <returns></returns>
        public string GetOrientation()
        {
            RobTarget rt = GetTCPRobTarget();
            return rt.Rot.ToString();
        }

        /// <summary>
        /// Returns a string representation of the robot's joint rotations in degrees.
        /// </summary>
        /// <returns></returns>
        public string GetJoints()
        {
            RobJoint rj = GetRobotJoints();
            return rj.ToString();
        }

        /// <summary>
        /// Loads the path to the queue manager and triggers execution of the program if applicable
        /// </summary>
        /// <param name="path"></param>
        public void LoadPath(Path path)
        {
            AddPathToQueue(path);
            TriggerQueue();
        }

        /// <summary>
        /// Stops the robot after execution of current program. This will also clear the queue.
        /// </summary>
        public void StopAfterProgram()
        {
            ClearQueue();
            StopProgram(false);
        }

        /// <summary>
        /// Dumps a ton of information to the console about the controller, the main task, etc.
        /// </summary>
        public void DebugDump()
        {
            DebugBanner();
            TestMastership();
            DebugControllerDump();
            DebugTaskDump();
        }
        
        /// <summary>
        /// Sets the default velocity new actions will be run at.
        /// </summary>
        /// <param name="vel"></param>
        public void SetVelocity(double vel)
        {
            currentVelocity = vel;
        }

        /// <summary>
        /// Sets the default zone value new actions will be given.
        /// </summary>
        /// <param name="zone"></param>
        public void SetZone(double zone)
        {
            currentZone = zone;
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
            if (onlineMode != RobotControl.OnlineMode.Stream)
            {
                Console.WriteLine("Move() only supported in Stream mode");
                return false;
            }

            if (SafetyCheckTableCollision)
            {
                if (IsBelowTable(TCPPosition.Z + incZ))
                {
                    Console.WriteLine("WARNING: TCP ABOUT TO HIT THE TABLE");
                    if (SafetyStopOnTableCollision)
                    {
                        return false;
                    }
                }
            }

            TCPPosition.Add(incX, incY, incZ);
            AddFrameToStreamQueue(new Frame(TCPPosition.X, TCPPosition.Y, TCPPosition.Z, currentVelocity, currentZone));

            // Only tick queue if there are no targets pending to be streamed
            if (streamQueue.FramesPending() == 1 )
            {
                TickStreamQueue(false);
            } else
            {
                Console.WriteLine("{0} frames pending", streamQueue.FramesPending());
            }

            return true;
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
            if (onlineMode != RobotControl.OnlineMode.Stream)
            {
                Console.WriteLine("MoveTo() only supported in Stream mode");
                return false;
            }

            if (SafetyCheckTableCollision)
            {
                if (IsBelowTable(newZ))
                {
                    Console.WriteLine("WARNING: TCP ABOUT TO HIT THE TABLE");
                    if (SafetyStopOnTableCollision)
                    {
                        return false;
                    }
                }
            }

            TCPPosition.Set(newX, newY, newZ);
            //AddFrameToStreamQueue(new Frame(newX, newY, newZ, currentVelocity, currentZone));
            AddFrameToStreamQueue(new Frame(TCPPosition.X, TCPPosition.Y, TCPPosition.Z,
               TCPRotation.Q1, TCPRotation.Q2, TCPRotation.Q3, TCPRotation.Q4,
               currentVelocity, currentZone));

            // Only tick queue if there are no targets pending to be streamed
            if (streamQueue.FramesPending() == 1)
            {
                TickStreamQueue(false);
            }
            else
            {
                Console.WriteLine("{0} frames pending", streamQueue.FramesPending());
            }

            return true;
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
            if (onlineMode != RobotControl.OnlineMode.Stream)
            {
                Console.WriteLine("RotateTo() only supported in Stream mode");
                return false;
            }

            // WARNING: NO TABLE COLLISIONS ARE PERFORMED HERE YET!
            TCPRotation.Set(q1, q2, q3, q4);
            AddFrameToStreamQueue(new Frame(TCPPosition.X, TCPPosition.Y, TCPPosition.Z,
               TCPRotation.Q1, TCPRotation.Q2, TCPRotation.Q3, TCPRotation.Q4,
               currentVelocity, currentZone));

            // Only tick queue if there are no targets pending to be streamed
            if (streamQueue.FramesPending() == 1)
            {
                TickStreamQueue(false);
            }
            else
            {
                Console.WriteLine("{0} frames pending", streamQueue.FramesPending());
            }

            return true;
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
            return false;
        }











        //██████╗ ██████╗ ██╗██╗   ██╗ █████╗ ████████╗███████╗    ███╗   ███╗███████╗████████╗██╗  ██╗ ██████╗ ██████╗ ███████╗
        //██╔══██╗██╔══██╗██║██║   ██║██╔══██╗╚══██╔══╝██╔════╝    ████╗ ████║██╔════╝╚══██╔══╝██║  ██║██╔═══██╗██╔══██╗██╔════╝
        //██████╔╝██████╔╝██║██║   ██║███████║   ██║   █████╗      ██╔████╔██║█████╗     ██║   ███████║██║   ██║██║  ██║███████╗
        //██╔═══╝ ██╔══██╗██║╚██╗ ██╔╝██╔══██║   ██║   ██╔══╝      ██║╚██╔╝██║██╔══╝     ██║   ██╔══██║██║   ██║██║  ██║╚════██║
        //██║     ██║  ██║██║ ╚████╔╝ ██║  ██║   ██║   ███████╗    ██║ ╚═╝ ██║███████╗   ██║   ██║  ██║╚██████╔╝██████╔╝███████║
        //╚═╝     ╚═╝  ╚═╝╚═╝  ╚═══╝  ╚═╝  ╚═╝   ╚═╝   ╚══════╝    ╚═╝     ╚═╝╚══════╝   ╚═╝   ╚═╝  ╚═╝ ╚═════╝ ╚═════╝ ╚══════╝

        /// <summary>
        /// Resets all internal state properties to default values. To be invoked upon
        /// an internal robot reset.
        /// </summary>
        private void Reset()
        {
            isConnected = false;
            isLogged = false;
            isMainTaskRetrieved = false;
            IP = "";

            queue = new Queue();
            streamQueue = new StreamQueue();
        }

        /// <summary>
        /// Searches the network for a robot controller and establishes a connection with the first one available. 
        /// Necessary for "online" modes.
        /// </summary>
        /// <returns></returns>
        private bool ConnectToController(OnlineMode oMode)
        {
            NetworkScanner scanner = new NetworkScanner();

            //ControllerInfo[] controllers = scanner.GetControllers(NetworkScannerSearchCriterias.Virtual);
            ControllerInfo[] controllers = scanner.GetControllers();
            if (controllers.Length > 0)
            {
                controller = ControllerFactory.CreateFrom(controllers[0]);
                isConnected = true;
                IP = controller.IPAddress.ToString();
                if (DEBUG) Console.WriteLine("Found controller on " + IP);

                LogOn();
                RetrieveMainTask();
                if (TestMastership()) RunMode("once");
                SubscribeToEvents();
            }
            else
            {
                if (DEBUG) Console.WriteLine("No controllers found on the network");
                isConnected = false;
            }

            // Pick up the state of the robot if doing Stream mode
            if (oMode == RobotControl.OnlineMode.Stream)
            {
                LoadStreamingModule();
                HookUpStreamingVariables();
                TCPPosition = new Point(GetTCPRobTarget().Trans);
                TCPRotation = new Rotation(GetTCPRobTarget().Rot);
                if (DEBUG) Console.WriteLine("Current TCP Position: {0}", TCPPosition);
            }

            return true;
        }

        /// <summary>
        /// Checks if the controller resource is available for Mastership, or held by someone else. 
        /// </summary>
        /// <returns></returns>
        private bool TestMastership()
        {
            bool available = false;
            if (isConnected)
            {
                try
                {
                    using (Mastership.Request(controller.Rapid))
                    {
                        // Gets the current execution cycle from the RAPID module and sets it back to the same value
                        ExecutionCycle mode = controller.Rapid.Cycle;
                        controller.Rapid.Cycle = mode;
                        available = true;
                    }
                }
                catch (GenericControllerException ex)
                {
                    Console.WriteLine("MASTERSHIP ERROR: The controller is held by someone else");
                    if (DEBUG) Console.WriteLine(ex);
                }
            }
            else
            {
                Console.WriteLine("TestMastership(): not connected to controller");
            }

            if (available && DEBUG) Console.WriteLine("Controller Mastership available");

            return available;
        }

        /// <summary>
        /// Forces disconnection from current controller and manages associated logoffs, disposals, etc.
        /// </summary>
        private void DisconnectFromController()
        {
            StopProgram(true);  // for security... TODO: implement a flag with a global variable
            DisposeMainTask();
            DisposeController();
            LogOff();
            Reset();
        }

        /// <summary>
        /// Disposes the controller object. This has to be done manually, since COM objects are not
        /// automatically garbage collected. 
        /// </summary>
        /// <returns></returns>
        private bool DisposeController()
        {
            if (controller != null)
            {
                controller.Dispose();
                controller = null;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Upon connection, subscribe to relevant events and handle them.
        /// </summary>
        private void SubscribeToEvents()
        {
            controller.Rapid.ExecutionStatusChanged += OnExecutionStatusChanged;
        }

        /// <summary>
        /// Logs on to the controller with default credentials.
        /// </summary>
        private bool LogOn()
        {
            if (isLogged)
            {
                LogOff();
            }

            controller.Logon(UserInfo.DefaultUser);

            try
            {
                controller.Logon(UserInfo.DefaultUser);
                isLogged = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("USER LOGON ERROR: " + ex);
                isLogged = false;
            }
            
            return isLogged;
        }

        /// <summary>
        /// Logs off from current controller.
        /// </summary>
        private void LogOff()
        {
            if (controller != null)
            {
                controller.Logoff();
                isLogged = false;
            }
        }

        /// <summary>
        /// Retrieves main task from the controller. E.g. for ABB robots this would typically be "T_ROB1".
        /// </summary>
        /// <returns></returns>
        private bool RetrieveMainTask()
        {
            ABB.Robotics.Controllers.RapidDomain.Task[] tasks = controller.Rapid.GetTasks();
            if (tasks.Length > 0)
            {
                mainTask = tasks[0];
                isMainTaskRetrieved = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Disposes the task objects. This has to be done manually, since COM objects are not
        /// automatically garbage collected. 
        /// </summary>
        /// <returns></returns>
        private bool DisposeMainTask()
        {
            if (mainTask != null)
            {
                mainTask.Dispose();
                mainTask = null;
                isMainTaskRetrieved = false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Deletes all existing modules from main task in the controller. 
        /// </summary>
        /// <returns></returns>
        private int ClearAllModules()
        {
            if (!isConnected)
            {
                if (DEBUG) Console.WriteLine("Can't ClearAllModules(), not connected to controller");
                return -1;
            }

            int count = -1;

            Module[] modules = mainTask.GetModules();
            count = modules.Length;

            try
            {
                using (Mastership.Request(controller.Rapid))
                {
                    foreach (Module m in modules)
                    {
                        if (DEBUG) Console.WriteLine("Deleting module: {0}", m.Name);
                        m.Delete();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("CLEAR MODULES ERROR: {0}", ex);
            }

            return count;
        }

        /// <summary>
        /// Loads a module to the robot controller.
        /// </summary>
        /// <param name="mod"></param>
        private void LoadModuleToRobot(List<string> mod)
        {
            SaveModuleToFile(mod, localBufferPathname + localBufferFilename);
            LoadModuleFromFilename(localBufferFilename, localBufferPathname);
        }

        /// <summary>
        /// Saves a string representation of a module to a local file. 
        /// </summary>
        /// <param name="module"></param>
        /// <param name="filepath"></param>
        private void SaveModuleToFile(List<string> module, string filepath)
        {
            System.IO.File.WriteAllLines(filepath, module, System.Text.Encoding.ASCII);
        }

        /// <summary>
        /// Loads a module into de controller from a local file. 
        /// @TODO: This is an expensive operation, should probably become threaded. 
        /// @TODO: By default, wipes out all previous modules --> parameterize.
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        private bool LoadModuleFromFilename(string filename, string filepath)
        {
            // When connecting to a real controller, the reference filesystem 
            // for Task.LoadModuleFromFile() becomes the controller's, so it is necessary
            // to copy the file to the system first, and then load it. 
            string fullPath = filepath + filename;

            if (!isConnected)
            {
                Console.WriteLine("Could not load module '{0}', not connected to controller", fullPath);
                return false;
            }

            // For the time being, we will always wipe out previous modules on load
            if (ClearAllModules() < 0)
            {
                Console.WriteLine("Error clearing modules");
                return false;
            }

            // Load the module
            bool success = false;
            try
            {
                using (Mastership.Request(controller.Rapid))
                {
                    // Create the remoteBufferDirectory if applicable
                    FileSystem fs = controller.FileSystem;
                    string remotePath = fs.RemoteDirectory + "/" + remoteBufferDirectory;
                    bool dirExists = fs.DirectoryExists(remoteBufferDirectory);
                    //Console.WriteLine("Exists? " + dirExists);
                    if (!dirExists)
                    {
                        if (DEBUG) Console.WriteLine("Creating {0} on remote controller", remotePath);
                        fs.CreateDirectory(remoteBufferDirectory);
                    }
                    //@TODO: Should implement some kind of file cleanup at somepoint

                    // Copy the file to the remote controller
                    controller.FileSystem.PutFile(fullPath, remoteBufferDirectory + "/" + filename, true);
                    if (DEBUG) Console.WriteLine("Copied {0} to {1}", filename, remoteBufferDirectory);

                    // Loads a Rapid module to the task in the robot controller
                    success = mainTask.LoadModuleFromFile(remotePath + "/" + filename, RapidLoadMode.Replace);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: Could not load module: {0}", ex);
            }

            // True if loading succeeds without any errors, otherwise false.  
            if (!success)
            {
                //// Gets the available categories of the EventLog. 
                //foreach (EventLogCategory category in controller.EventLog.GetCategories())
                //{
                //    if (category.Name == "Common")
                //    {
                //        if (category.Messages.Count > 0)
                //        {
                //            foreach (EventLogMessage message in category.Messages)
                //            {
                //                Console.WriteLine("Program [{1}:{2}({0})] {3} {4}",
                //                    message.Name, message.SequenceNumber,
                //                    message.Timestamp, message.Title, message.Body);
                //            }
                //        }
                //    }
                //}
            }
            else
            {
                if (DEBUG) Console.WriteLine("Sucessfully loaded {0}{1}", filepath, filename);
            }

            return success;
        }

        /// <summary>
        /// Resets the program pointer in the controller to the main entry point. Needs to be called
        /// before starting execution of a program, otherwise the controller will throw an error. 
        /// </summary>
        private void ResetProgramPointer()
        {
            if (isMainTaskRetrieved)
            {
                try
                {
                    using (Mastership.Request(controller.Rapid))
                    {
                        mainTask.ResetProgramPointer();
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("POINTER RESET ERROR: " + ex);
                }
                
            }

        }

        /// <summary>
        /// Requests start executing the program in the main task. Remember to call ResetProgramPointer() before. 
        /// </summary>
        private void StartProgram()
        {
            if (isMainTaskRetrieved)
            {

                try
                {
                    using (Mastership.Request(controller.Rapid))
                    {
                        controller.Rapid.Start(true);
                    }
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine("PROGRAM START ERROR: " + ex);
                }
                
            }
        }

        /// <summary>
        /// Requests stop executing the program in the main task.
        /// </summary> 
        private void StopProgram(bool immediate)
        {
            if (isMainTaskRetrieved)
            {
                using (Mastership.Request(controller.Rapid))
                {
                    controller.Rapid.Stop(immediate ? StopMode.Immediate : StopMode.Cycle);
                    //controller.Rapid.Stop(StopMode.Immediate);
                }
            }
        }

        /// <summary>
        /// Returns a RobTarget object representing the current robot's TCP.
        /// </summary>
        /// <returns></returns>
        private RobTarget GetTCPRobTarget()
        {
            return controller.MotionSystem.ActiveMechanicalUnit.GetPosition(ABB.Robotics.Controllers.MotionDomain.CoordinateSystemType.World);
        }

        /// <summary>
        /// Returns a RobJoint object representing the current values for joint rotations. 
        /// </summary>
        /// <returns></returns>
        private RobJoint GetRobotJoints()
        {
            return controller.MotionSystem.ActiveMechanicalUnit.GetPosition().RobAx;
        }












        //██╗    ██╗██╗██████╗ 
        //██║    ██║██║██╔══██╗
        //██║ █╗ ██║██║██████╔╝
        //██║███╗██║██║██╔═══╝ 
        //╚███╔███╔╝██║██║     
        // ╚══╝╚══╝ ╚═╝╚═╝     
                     
        /// <summary>
        /// Adds a path to the queue manager.
        /// </summary>
        /// <param name="path"></param>
        private void AddPathToQueue(Path path)
        {
            queue.Add(path);
        }

        /// <summary>
        /// Checks the state of the execution of the robot, and if stopped and if elements 
        /// remaining on the queue, starts executing them.
        /// </summary>
        private void TriggerQueue()
        {
            if (controller.Rapid.ExecutionStatus == ExecutionStatus.Stopped) 
            {
                TriggerQueue(true);
            }
        }

        /// <summary>
        /// An overload to bypass ExecutionStatus check.
        /// </summary>
        /// <param name="robotIsStopped"></param>
        private void TriggerQueue(bool robotIsStopped)
        {
            if (queue.ArePathsPending() && (pathExecuter == null || !pathExecuter.IsAlive))
            {
                Path path = queue.GetNext();
                // RunPath(path);

                // https://msdn.microsoft.com/en-us/library/aa645740(v=vs.71).aspx
                // Thread oThread = new Thread(new ThreadStart(oAlpha.Beta));
                // http://stackoverflow.com/a/3360582
                // Thread thread = new Thread(() => download(filename));

                // This needs to be much better handled, and the trigger queue should not trigger if a thread is running... 
                //Thread runPathThread = new Thread(() => RunPath(path));  // not working for some reason...
                //runPathThread.Start();

                pathExecuter = new Thread(() => RunPath(path));  // http://stackoverflow.com/a/3360582
                pathExecuter.Start();
            }
        }
        
        /// <summary>
        /// Generates a module from a path, loads it to the controller and runs it.
        /// It assumes the robot is stopped (does this even matter anyway...?)
        /// </summary>
        /// <param name="path"></param>
        private void RunPath(Path path)
        {
            if (DEBUG) Console.WriteLine("RUNNING NEW PATH: " + path.Count);
            List<string> module = RAPID.UNSAFEModuleFromPath(path, (int) currentVelocity, (int) currentZone);
            //SaveModuleToFile(module, localBufferPathname + localBufferFilename);
            //LoadModuleFromFilename(localBufferFilename, localBufferPathname);
            LoadModuleToRobot(module);
            ResetProgramPointer();
            StartProgram();
        }


        /// <summary>
        /// Remove all pending elements from the queue.
        /// </summary>
        private void ClearQueue()
        {
            queue.EmptyQueue();
        }






        //███████╗████████╗██████╗ ███████╗ █████╗ ███╗   ███╗██╗███╗   ██╗ ██████╗ 
        //██╔════╝╚══██╔══╝██╔══██╗██╔════╝██╔══██╗████╗ ████║██║████╗  ██║██╔════╝ 
        //███████╗   ██║   ██████╔╝█████╗  ███████║██╔████╔██║██║██╔██╗ ██║██║  ███╗
        //╚════██║   ██║   ██╔══██╗██╔══╝  ██╔══██║██║╚██╔╝██║██║██║╚██╗██║██║   ██║
        //███████║   ██║   ██║  ██║███████╗██║  ██║██║ ╚═╝ ██║██║██║ ╚████║╚██████╔╝
        //╚══════╝   ╚═╝   ╚═╝  ╚═╝╚══════╝╚═╝  ╚═╝╚═╝     ╚═╝╚═╝╚═╝  ╚═══╝ ╚═════╝ 
                                                                          
        private static int virtualRDCount = 4;
        private RapidData
            RD_aborted,
            RD_pnum;
        private RapidData[]
            RD_vel = new RapidData[virtualRDCount],
            RD_zone = new RapidData[virtualRDCount],
            RD_p = new RapidData[virtualRDCount],
            RD_pset = new RapidData[virtualRDCount];

        private int virtualStepCounter = 0;  // this keeps track of what is the next target index that needs to be assigned. Its %4 should asynchronously be ~4 units ahead of the 'pnum' rapid counter

        /// <summary>
        /// Loads the default StreamModule designed for streaming.
        /// </summary>
        private void LoadStreamingModule()
        {
            LoadModuleToRobot(StaticData.StreamModule.ToList());
        }

        /// <summary>
        /// Loads all relevant Rapid variables
        /// </summary>
        private void HookUpStreamingVariables()
        {
            // Load RapidData control variables
            RD_aborted = LoadRapidDataVariable("aborted");
            RD_pnum = LoadRapidDataVariable("pnum");
            RD_pnum.ValueChanged += OnRD_pnum_ValueChanged;  // add an eventhandler to 'pnum' to track when it changes

            if (DEBUG)
            {
                Console.WriteLine(RD_aborted.StringValue);
                Console.WriteLine(RD_pnum.StringValue);
            }

            // Load and set the first four targets
            for (int i = 0; i < virtualRDCount; i++)
            {
                RD_vel[i] = LoadRapidDataVariable("vel" + i);
                RD_zone[i] = LoadRapidDataVariable("zone" + i);
                RD_p[i] = LoadRapidDataVariable("p" + i);
                RD_pset[i] = LoadRapidDataVariable("pset" + i);
                //AddVirtualTarget();

                if (DEBUG)
                {
                    Console.WriteLine("{0}, {1}, {2}, {3}", 
                        RD_vel[i].StringValue, 
                        RD_zone[i].StringValue, 
                        RD_p[i].StringValue, 
                        RD_pset[i].StringValue);
                }
            }

        }

        /// <summary>
        /// Retrieves a Rapid variable in current module and returns it
        /// </summary>
        /// <param name="varName"></param>
        /// <returns></returns>
        private RapidData LoadRapidDataVariable(string varName)
        {
            RapidData rd = null;
            try
            {
                rd = mainTask.GetModule("StreamModule").GetRapidData(varName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return rd;
        }

        /// <summary>
        /// Adds a Frame to the streaming queue
        /// </summary>
        /// <param name="frame"></param>
        private void AddFrameToStreamQueue(Frame frame)
        {
            streamQueue.Add(frame);
        }

        /// <summary>
        /// This function will look at the state of the program pointer, the streamQueue, 
        /// and if necessary will add a new target to the stream. This is meant to be called
        /// to initiate the stream update chain, like when adding a new target, or pnum event handling.
        /// </summary>
        private void TickStreamQueue(bool hasPriority)
        {
            if (DEBUG) Console.WriteLine("TICKING StreamQueue: {0} targets pending", streamQueue.FramesPending());
            if (streamQueue.AreFramesPending() && RD_pset[virtualStepCounter % virtualRDCount].StringValue.Equals("FALSE"))
            {
                Console.WriteLine("About to set targets");
                SetNextVirtualTarget(hasPriority);
                virtualStepCounter++;
                TickStreamQueue(hasPriority);
            }
            else
            {
                Console.WriteLine("Not setting targets, streamQueue.AreFramesPending() {0} RD_pset[virtualStepCounter % virtualRDCount].StringValue.Equals(\"FALSE\") {1}",
                     streamQueue.AreFramesPending(),
                    RD_pset[virtualStepCounter % virtualRDCount].StringValue.Equals("FALSE"));
            }
        }

        /// <summary>
        /// Figures out the appropriate virtual target in the streaming module and 
        /// sets new values according to the streaming queue.
        /// </summary>
        /// <param name="hasPriority"></param>
        private void SetNextVirtualTarget(bool hasPriority)
        {
            if (DEBUG) Console.WriteLine("Setting frame #{0}", virtualStepCounter);

            lock (rapidDataLock)
            {
                Frame target = streamQueue.GetNext();
                if (target != null)
                {
                    //// When masterhip is held, only priority calls make it through (which are the ones holding mastership)
                    //while (!hasPriority && mHeld)
                    //{
                    //    Console.WriteLine("TRAPPED IN MASTERHIP CONFLICT 1");
                    //}  // safety mechanism to not hit held mastership by eventhandlers
                    SetRapidDataVarString(RD_p[virtualStepCounter % virtualRDCount], target.GetUNSAFERobTargetDeclaration());                   
                    //while (!hasPriority && mHeld)
                    //{
                    //    Console.WriteLine("TRAPPED IN MASTERHIP CONFLICT 2");
                    //}
                    SetRapidDataVarString(RD_vel[virtualStepCounter % virtualRDCount], target.GetSpeedDeclaration());
                    //while (!hasPriority && mHeld)
                    //{
                    //    Console.WriteLine("TRAPPED IN MASTERHIP CONFLICT 3");
                    //}
                    SetRapidDataVarString(RD_zone[virtualStepCounter % virtualRDCount], target.GetZoneDeclaration());
                    //while (!hasPriority && mHeld)
                    //{
                    //    Console.WriteLine("TRAPPED IN MASTERHIP CONFLICT 4");
                    //}
                    //SetRapidDataVarBool(RD_pset[virtualStepCounter % virtualRDCount], true);  // --> Looks like this wasn't working well??
                    SetRapidDataVarString(RD_pset[virtualStepCounter % virtualRDCount], "TRUE");
                }
            }

        }

        // DEPRECATED: use SetRapidDataVarString() instead
        //private void SetRapidDataVarBool(RapidData rd, bool value)
        //{
        //    try
        //    {
        //        using (Mastership.Request(controller.Rapid))
        //        {
        //            if (DEBUG) Console.WriteLine("    current value for '{0}': {1}", rd.Name, rd.StringValue);
        //            rd.Value = new Bool(value);
        //            if (DEBUG) Console.WriteLine("        NEW value for '{0}': {1}", rd.Name, rd.StringValue);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("    ERROR SetRapidDataVarBool: {0}", ex);
        //    }
        //}

        /// <summary>
        /// Sets the value of a Rapid variable from a string representation
        /// </summary>
        /// <param name="rd"></param>
        /// <param name="declaration"></param>
        private void SetRapidDataVarString(RapidData rd, string declaration)
        {
            try
            {
                using (Mastership.Request(controller.Rapid))
                {
                    if (DEBUG) Console.WriteLine("    current value for '{0}': {1}", rd.Name, rd.StringValue);
                    rd.StringValue = declaration;
                    if (DEBUG) Console.WriteLine("        NEW value for '{0}': {1}", rd.Name, rd.StringValue);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("    ERROR SetRapidDataVarString: {0}", ex);
            }
        }








        /// <summary>
        /// Printlines a "DEBUG" ASCII banner... ;)
        /// </summary>
        private void DebugBanner()
        {
            Console.WriteLine("");
            Console.WriteLine("██████╗ ███████╗██████╗ ██╗   ██╗ ██████╗ ");
            Console.WriteLine("██╔══██╗██╔════╝██╔══██╗██║   ██║██╔════╝ ");
            Console.WriteLine("██║  ██║█████╗  ██████╔╝██║   ██║██║  ███╗");
            Console.WriteLine("██║  ██║██╔══╝  ██╔══██╗██║   ██║██║   ██║");
            Console.WriteLine("██████╔╝███████╗██████╔╝╚██████╔╝╚██████╔╝");
            Console.WriteLine("╚═════╝ ╚══════╝╚═════╝  ╚═════╝  ╚═════╝ ");
            Console.WriteLine("");
        }

        /// <summary>
        /// Dumps a bunch of controller info to the console.
        /// </summary>
        private void DebugControllerDump()
        {
            if (isConnected)
            {
                Console.WriteLine("");
                Console.WriteLine("DEBUG CONTROLLER DUMP:");
                Console.WriteLine("     AuthenticationSystem: " + controller.AuthenticationSystem.Name);
                Console.WriteLine("     BackupInProgress: " + controller.BackupInProgress);
                Console.WriteLine("     Configuration: " + controller.Configuration);
                Console.WriteLine("     Connected: " + controller.Connected);
                Console.WriteLine("     CurrentUser: " + controller.CurrentUser);
                Console.WriteLine("     DateTime: " + controller.DateTime);
                Console.WriteLine("     EventLog: " + controller.EventLog);
                Console.WriteLine("     FileSystem: " + controller.FileSystem);
                Console.WriteLine("     IOSystem: " + controller.IOSystem);
                Console.WriteLine("     IPAddress: " + controller.IPAddress);
                Console.WriteLine("     Ipc: " + controller.Ipc);
                Console.WriteLine("     IsMaster: " + controller.IsMaster);
                Console.WriteLine("     IsVirtual: " + controller.IsVirtual);
                Console.WriteLine("     MacAddress: " + controller.MacAddress);
                //Console.WriteLine("     MainComputerServiceInfo: ");
                //Console.WriteLine("         BoardType: " + controller.MainComputerServiceInfo.BoardType);
                //Console.WriteLine("         CpuInfo: " + controller.MainComputerServiceInfo.CpuInfo);
                //Console.WriteLine("         RamSize: " + controller.MainComputerServiceInfo.RamSize);
                //Console.WriteLine("         Temperature: " + controller.MainComputerServiceInfo.Temperature);
                Console.WriteLine("     MastershipPolicy: " + controller.MastershipPolicy);
                Console.WriteLine("     MotionSystem: " + controller.MotionSystem);
                Console.WriteLine("     Name: " + controller.Name);
                //Console.WriteLine("     NetworkSettings: " + controller.NetworkSettings);
                Console.WriteLine("     OperatingMode: " + controller.OperatingMode);
                Console.WriteLine("     Rapid: " + controller.Rapid);
                Console.WriteLine("     RobotWare: " + controller.RobotWare);
                Console.WriteLine("     RobotWareVersion: " + controller.RobotWareVersion);
                Console.WriteLine("     RunLevel: " + controller.RunLevel);
                Console.WriteLine("     State: " + controller.State);
                Console.WriteLine("     SystemId: " + controller.SystemId);
                Console.WriteLine("     SystemName: " + controller.SystemName);
                //Console.WriteLine("     TimeServer: " + controller.TimeServer);
                //Console.WriteLine("     TimeZone: " + controller.TimeZone);
                //Console.WriteLine("     UICulture: " + controller.UICulture);
                Console.WriteLine("");
            }
        }

        private void DebugTaskDump()
        {
            if (isMainTaskRetrieved)
            {
                Console.WriteLine("");
                Console.WriteLine("DEBUG TASK DUMP:");
                Console.WriteLine("    Cycle: " + mainTask.Cycle);
                Console.WriteLine("    Enabled: " + mainTask.Enabled);
                Console.WriteLine("    ExecutionStatus: " + mainTask.ExecutionStatus);
                Console.WriteLine("    ExecutionType: " + mainTask.ExecutionType);
                Console.WriteLine("    Motion: " + mainTask.Motion);
                Console.WriteLine("    MotionPointer: " + mainTask.MotionPointer.Module);
                Console.WriteLine("    Name: " + mainTask.Name);
                Console.WriteLine("    ProgramPointer: " + mainTask.ProgramPointer.Module);
                Console.WriteLine("    RemainingCycles: " + mainTask.RemainingCycles);
                Console.WriteLine("    TaskType: " + mainTask.TaskType);
                Console.WriteLine("    Type: " + mainTask.Type);
                Console.WriteLine("");
            }
        }


        // This should be moved somewhere else
        private static bool IsBelowTable(double z)
        {
            return z <= SafetyTableZLimit;
        }







        //███████╗██╗   ██╗███████╗███╗   ██╗████████╗    ██╗  ██╗ █████╗ ███╗   ██╗██████╗ ██╗     ██╗███╗   ██╗ ██████╗ 
        //██╔════╝██║   ██║██╔════╝████╗  ██║╚══██╔══╝    ██║  ██║██╔══██╗████╗  ██║██╔══██╗██║     ██║████╗  ██║██╔════╝ 
        //█████╗  ██║   ██║█████╗  ██╔██╗ ██║   ██║       ███████║███████║██╔██╗ ██║██║  ██║██║     ██║██╔██╗ ██║██║  ███╗
        //██╔══╝  ╚██╗ ██╔╝██╔══╝  ██║╚██╗██║   ██║       ██╔══██║██╔══██║██║╚██╗██║██║  ██║██║     ██║██║╚██╗██║██║   ██║
        //███████╗ ╚████╔╝ ███████╗██║ ╚████║   ██║       ██║  ██║██║  ██║██║ ╚████║██████╔╝███████╗██║██║ ╚████║╚██████╔╝
        //╚══════╝  ╚═══╝  ╚══════╝╚═╝  ╚═══╝   ╚═╝       ╚═╝  ╚═╝╚═╝  ╚═╝╚═╝  ╚═══╝╚═════╝ ╚══════╝╚═╝╚═╝  ╚═══╝ ╚═════╝ 

        /// <summary>
        /// What to do when the robot starts running or stops.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnExecutionStatusChanged(object sender, ExecutionStatusChangedEventArgs e)
        {
            if (DEBUG) Console.WriteLine("EXECUTION STATUS CHANGED: " + e.Status);

            if (e.Status == ExecutionStatus.Stopped)
            {
                // Only trigger Instruct queue
                if (onlineMode == RobotControl.OnlineMode.Instruct)
                {
                    // Tick queue to move forward
                    TriggerQueue(true);
                }
            }
        }

        private void OnRD_pnum_ValueChanged(object sender, DataValueChangedEventArgs e)
        {
            RapidData rd = (RapidData)sender;
            if (DEBUG) Console.WriteLine("   variable '{0}' changed: {1}", rd.Name, rd.StringValue);

            // Do not add target if pnum is being reset to initial value (like on program load)
            if (!rd.StringValue.Equals("-1"))
            {
                if (DEBUG) Console.WriteLine("Ticking from pnum event handler");
                mHeld = true;
                TickStreamQueue(true);
                mHeld = false;
            }

            if (rd != null)
            {
                //Console.WriteLine("Disposing rd");
                //rd.Dispose();
                //rd = null;
            }

        }

        

    }
}
