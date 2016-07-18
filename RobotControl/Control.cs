using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

//// the Control class should not need any ABB controllers...
//using ABB.Robotics;
//using ABB.Robotics.Controllers;
//using ABB.Robotics.Controllers.Discovery;
//using ABB.Robotics.Controllers.RapidDomain;  // This is for the Task Class
//using ABB.Robotics.Controllers.EventLogDomain;
//using ABB.Robotics.Controllers.FileSystemDomain;


namespace RobotControl
{
    /// <summary>
    /// The core class that centralizes all private control .
    /// </summary>
    class Control
    {
        // Some 'environment variables' to define check states and behavior
        public static readonly bool SafetyStopImmediateOnDisconnect = true;
        public static readonly bool SafetyCheckTableCollision = true;
        public static readonly bool SafetyStopOnTableCollision = true;
        public static readonly double SafetyTableZLimit = 100;                     // table security checks will trigger under this z height (mm)
        public static readonly bool DEBUG = true;                                  // dump a bunch of debug logs

        




        /// <summary>
        /// Operation modes by default
        /// </summary>
        private ControlMode controlMode = ControlMode.Offline;
        private RunMode runMode = RunMode.Once;
        
        /// <summary>
        /// A shared instance of a Thread to manage uploading modules
        /// to the controller, which typically takes a lot of resources
        /// and halts program execution
        /// </summary>
        private Thread pathExecuter;

        /// <summary>
        /// Instances of the main robot Controller and Task
        /// </summary>
        private Communication comm;

        /// <summary>
        /// The queue that manages what instructions get sent to the robot
        /// </summary>
        private Queue queue;

        // STUFF NEEDED FOR STREAM MODE
        // Most of it represents a virtual current state of the robot, to be able to 
        // issue appropriate relative actions.
        // @TODO: move all of this to a VirtualRobot object and to the Comm + QueueManager objects
        public Point TCPPosition = null;
        public Rotation TCPRotation = null;
        public double currentVelocity = 10;
        public double currentZone = 5;
        public StreamQueue streamQueue;
        public bool mHeld = false;                                                 // is Mastership currently held by someone? useful when several threads want to write to the robot...
        

        





        //██████╗ ██╗   ██╗██████╗ ██╗     ██╗ ██████╗
        //██╔══██╗██║   ██║██╔══██╗██║     ██║██╔════╝
        //██████╔╝██║   ██║██████╔╝██║     ██║██║     
        //██╔═══╝ ██║   ██║██╔══██╗██║     ██║██║     
        //██║     ╚██████╔╝██████╔╝███████╗██║╚██████╗
        //╚═╝      ╚═════╝ ╚═════╝ ╚══════╝╚═╝ ╚═════╝
                                            
        /// <summary>
        /// Main constructor.
        /// </summary>
        public Control()
        {
            Reset();
        }

        /// <summary>
        /// Resets all internal state properties to default values. To be invoked upon
        /// an internal robot reset.
        /// </summary>
        public void Reset()
        {
            //isConnected = false;
            //isLogged = false;
            //isMainTaskRetrieved = false;
            //IP = "";

            queue = new Queue();
            streamQueue = new StreamQueue();
        }

        /// <summary>
        /// Sets current Control Mode. 
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public bool SetControlMode(ControlMode mode)
        {
            controlMode = mode;

            // @TODO: Make changes in ControlMode at runtime possible, i.e. resetting controllers and communication, flushing queues, etc.
            if (mode == ControlMode.Offline)
            {
                DropCommunication();
            }
            else
            {
                InitializeCommunication();  // online modes
            }
            
            return true;
        }

        /// <summary>
        /// Returns current Control Mode.
        /// </summary>
        /// <returns></returns>
        public ControlMode GetControlMode()
        {
            return controlMode;
        }

        /// <summary>
        /// Sets current RunMode. 
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public bool SetRunMode(RunMode mode)
        {
            runMode = mode;

            if (controlMode == ControlMode.Offline)
            {
                Console.WriteLine("Remember RunMode.{0} will have no effect in Offline mode", mode);
            }
            else
            {
                return comm.SetRunMode(mode);
            }

            return false;
        }
        
        /// <summary>
        /// Returns current RunMode.
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public RunMode GetRunMode(RunMode mode)
        {
            return runMode;
        }


        /// <summary>
        /// Searches the network for a robot controller and establishes a connection with the specified one by position. 
        /// Necessary for "online" modes.
        /// </summary>
        /// <returns></returns>
        public bool ConnectToDevice(int robotId)
        {
            // Sanity
            if (controlMode == ControlMode.Offline)
            {
                Console.WriteLine("No robot to connect to in 'offline' mode ;)");
                return false;
            }

            if (!comm.ConnectToDevice(robotId))
            {
                Console.WriteLine("Cannot connect to device");
                return false;
            }

            Frame curr = comm.GetCurrentFrame();
            TCPPosition = curr.Position;
            TCPRotation = curr.Orientation;

            return true;

            //// Scan the network and hookup to the specified controller
            //bool success = false;
            
            //// This is specific to ABB, should become abstracted at some point...
            //NetworkScanner scanner = new NetworkScanner();
            //ControllerInfo[] controllers = scanner.GetControllers();
            //if (controllers.Length > 0)
            //{
            //    int id = robotId > controllers.Length ? controllers.Length - 1 :
            //        robotId < 0 ? 0 : robotId;
            //    controller = ControllerFactory.CreateFrom(controllers[id]);
            //    if (controller != null)
            //    {
            //        isConnected = true;
            //        IP = controller.IPAddress.ToString();
            //        if (DEBUG) Console.WriteLine("Found controller on " + IP);

            //        LogOn();
            //        RetrieveMainTask();
            //        if (TestMastership()) SetRunMode(RunMode.Once);  // why was this here? 
            //        SubscribeToEvents();
            //        success = true;
            //    }
            //    else
            //    {
            //        Console.WriteLine("Could not connect to controller");
            //        isConnected = false;
            //    }
               
            //}
            //else
            //{
            //    if (DEBUG) Console.WriteLine("No controllers found on the network");
            //    isConnected = false;
            //}

            //// Pick up the state of the robot if doing Stream mode
            //if (controlMode == ControlMode.Stream)
            //{
            //    LoadStreamingModule();
            //    HookUpStreamingVariables();
            //    //TCPPosition = new Point(GetTCPRobTarget().Trans);
            //    TCPPosition = GetTCPPosition();
            //    //TCPRotation = new Rotation(GetTCPRobTarget().Rot);
            //    TCPRotation = GetTCPRotation();
            //    if (DEBUG) Console.WriteLine("Current TCP Position: {0}", TCPPosition);
            //}
        }

        public bool DisconnectFromDevice()
        {
            // Sanity
            if (controlMode == ControlMode.Offline)
            {
                Console.WriteLine("No robot to disconnect from in 'offline' mode ;)");
                return false;
            }

            return comm.DisconnectFromDevice();
        }

        /// <summary>
        /// Is this robot connected to a real/virtual device?
        /// </summary>
        /// <returns></returns>
        public bool IsConnectedToDevice()
        {
            return comm.IsConnected();
        }

        /// <summary>
        /// If connected to a device, return the IP address
        /// </summary>
        /// <returns></returns>
        public string GetControllerIP()
        {
            return comm.GetIP();
        }


        /// <summary>
        /// Loads a programm to the connected device and executes it. 
        /// </summary>
        /// <param name="programLines"></param>
        /// <returns></returns>
        public bool LoadProgramToDevice(List<string> programLines)
        {
            return comm.LoadProgramFromStringList(programLines);
        }


        /// <summary>
        /// Loads a programm to the connected device and executes it. 
        /// </summary>
        /// <param name="filepath">Full filepath including root, directory structure, filename and extension</param>
        /// <returns></returns>
        public bool LoadProgramToDevice(string filepath)
        {
            // @TODO: all this sanity and checks should probably go into LoadModuleFromFilename() (or basically consolidate everything into one method
            if (controlMode == ControlMode.Offline)
            {
                Console.WriteLine("Cannot load modules in Offline mode");
                return false;
            }


            // Sanity
            string fullPath = "";

            // Is the filepath a valid Windows path?
            try
            {
                fullPath = System.IO.Path.GetFullPath(filepath);
            }
            catch (Exception e)
            {
                Console.WriteLine("'{0}' is not a valid filepath", filepath);
                Console.WriteLine(e);
                return false;
            }

            // Is it an absolute path?
            try
            {
                bool absolute = System.IO.Path.IsPathRooted(fullPath);
                if (!absolute)
                {
                    Console.WriteLine("Relative paths are currently not supported");
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("'{0}' is not a valid absolute filepath", filepath);
                Console.WriteLine(e);
                return false;
            }

            // Split the full path into directory, file and extension names
            string dirname;     // full directory path
            string filename;    // filename without extension
            string extension;   // file extension

            string[] parts = fullPath.Split('\\');
            int len = parts.Length;
            if (len < 2)
            {
                Console.WriteLine("Weird filepath");
                return false;
            }
            dirname = string.Join("\\", parts, 0, len - 1);
            string[] fileparts = parts[len - 1].Split('.');
            if (fileparts.Length != 2)
            {
                Console.WriteLine("Weird filename");
                return false;
            }
            filename = fileparts[0];
            extension = fileparts[1];

            Console.WriteLine("  filename: " + filename);
            Console.WriteLine("  dirname: " + dirname);
            Console.WriteLine("  extension: " + extension);

            return comm.LoadProgramFromFilename(dirname, filename, extension);
        }

        /// <summary>
        /// Triggers start of program on device.
        /// </summary>
        /// <returns></returns>
        public bool StartProgramOnDevice()
        {
            if (controlMode == ControlMode.Offline)
            {
                Console.WriteLine("No program to start in Offline mode");
                return false;
            }

            return comm.StartProgramExecution();
        }

        /// <summary>
        /// Stops execution of the running program on device.
        /// </summary>
        /// <param name="immediate"></param>
        /// <returns></returns>
        public bool StopProgramOnDevice(bool immediate)
        {
            if (controlMode == ControlMode.Offline)
            {
                Console.WriteLine("No program to stop in Offline mode");
                return false;
            }

            return comm.StopProgramExecution(immediate);
        }

        /// <summary>
        /// Returns a Point object representing the current robot's TCP position.
        /// </summary>
        /// <returns></returns>
        public Point GetCurrentPosition()
        {
            // @TODO: at some point when virtual robots are implemented, this will return either the real robot's TCP
            // or the virtual one's (like in offline mode).

            return comm.GetCurrentPosition();
        }

        /// <summary>
        /// Returns a Rotation object representing the current robot's TCP orientation.
        /// </summary>
        /// <returns></returns>
        public Rotation GetCurrentOrientation()
        {
            return comm.GetCurrentOrientation();
        }

        /// <summary>
        /// Returns a Frame object representing the current robot's TCP position and orientation. 
        /// NOTE: the Frame object's velocity and zone still do not represent the acutal state of the robot.
        /// </summary>
        /// <returns></returns>
        public Frame GetCurrentFrame()
        {
            return comm.GetCurrentFrame();
        }

        /// <summary>
        /// Returns a Joints object representing the rotations of the 6 axes of this robot.
        /// </summary>
        /// <returns></returns>
        public Joints GetCurrentJoints()
        {
            return comm.GetCurrentJoints();
        }






























        //██████╗ ██████╗ ██╗██╗   ██╗ █████╗ ████████╗███████╗
        //██╔══██╗██╔══██╗██║██║   ██║██╔══██╗╚══██╔══╝██╔════╝
        //██████╔╝██████╔╝██║██║   ██║███████║   ██║   █████╗  
        //██╔═══╝ ██╔══██╗██║╚██╗ ██╔╝██╔══██║   ██║   ██╔══╝  
        //██║     ██║  ██║██║ ╚████╔╝ ██║  ██║   ██║   ███████╗
        //╚═╝     ╚═╝  ╚═╝╚═╝  ╚═══╝  ╚═╝  ╚═╝   ╚═╝   ╚══════╝

        private bool InitializeCommunication()
        {
            // If there is already some communication going on
            if (comm != null)
            {
                Console.WriteLine("Communication protocol might be active. Please CloseControllerCommunication() first.");
                return false;
            }
            
            // @TODO: shim assignment of correct robot model/brand
            comm = new CommunicationABB(this);

            // Pass the streamQueue object as a shared reference
            comm.LinkStreamQueue(streamQueue);

            return true;
        }

        private bool DropCommunication()
        {
            if (comm == null)
            {
                Console.WriteLine("Communication protocol not established.");
                return false;
            }
            bool success = comm.DisconnectFromDevice();
            comm = null;
            return success;
        }
        
        /// <summary>
        /// If there was a running Communication protocol, drop it and restart it again.
        /// </summary>
        /// <returns></returns>
        private bool ResetCommunication()
        {
            if (comm == null)
            {
                Console.WriteLine("Communication protocol not established, please initialize first.");
            }
            DropCommunication();
            return InitializeCommunication();
        }



























































        // █████╗  ██████╗████████╗██╗ ██████╗ ███╗   ██╗███████╗
        //██╔══██╗██╔════╝╚══██╔══╝██║██╔═══██╗████╗  ██║██╔════╝
        //███████║██║        ██║   ██║██║   ██║██╔██╗ ██║███████╗
        //██╔══██║██║        ██║   ██║██║   ██║██║╚██╗██║╚════██║
        //██║  ██║╚██████╗   ██║   ██║╚██████╔╝██║ ╚████║███████║
        //╚═╝  ╚═╝ ╚═════╝   ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝╚══════╝
                               
        public void SetCurrentVelocity(double vel)
        {
            currentVelocity = vel;
        }                        

        public void SetCurrentZone(double zone)
        {
            currentZone = zone;
        }

        public bool RequestMove(double incX, double incY, double incZ)
        {
            if (controlMode != ControlMode.Stream)
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
            if (streamQueue.FramesPending() == 1)
            {
                comm.TickStreamQueue(false);
            }
            else
            {
                Console.WriteLine("{0} frames pending", streamQueue.FramesPending());
            }

            return true;
        }

        public bool RequestMoveTo(double newX, double newY, double newZ)
        {
            if (controlMode != ControlMode.Stream)
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
            AddFrameToStreamQueue(new Frame(TCPPosition.X, TCPPosition.Y, TCPPosition.Z,
               TCPRotation.Q1, TCPRotation.Q2, TCPRotation.Q3, TCPRotation.Q4,
               currentVelocity, currentZone));

            // Only tick queue if there are no targets pending to be streamed
            if (streamQueue.FramesPending() == 1)
            {
                comm.TickStreamQueue(false);
            }
            else
            {
                Console.WriteLine("{0} frames pending", streamQueue.FramesPending());
            }

            return true;
        }

        public bool RequestRotateTo(double q1, double q2, double q3, double q4)
        {
            if (controlMode != ControlMode.Stream)
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
                comm.TickStreamQueue(false);
            }
            else
            {
                Console.WriteLine("{0} frames pending", streamQueue.FramesPending());
            }

            return true;
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
        public void AddPathToQueue(Path path)
        {
            queue.Add(path);
        }

        ///// <summary>
        ///// Checks the state of the execution of the robot, and if stopped and if elements 
        ///// remaining on the queue, starts executing them.
        ///// </summary>
        //public void TriggerQueue()
        //{
        //    if (controller.Rapid.ExecutionStatus == ExecutionStatus.Stopped)
        //    {
        //        TriggerQueue(true);
        //    }
        //}

        /// <summary>
        /// Checks the state of the execution of the robot, and if stopped and if elements 
        /// remaining on the queue, starts executing them.
        /// </summary>
        public void TriggerQueue()
        {
            if (!comm.IsRunning() && queue.ArePathsPending() && (pathExecuter == null || !pathExecuter.IsAlive))
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
        public void RunPath(Path path)
        {
            Console.WriteLine("RUNNING NEW PATH: " + path.Count);
            List<string> module = ProgramGenerator.UNSAFEModuleFromPath(path, (int)currentVelocity, (int)currentZone);
            ////SaveModuleToFile(module, localBufferPathname + localBufferFilename);
            ////LoadModuleFromFilename(localBufferFilename, localBufferPathname);
            //LoadModuleToRobot(module);
            //ResetProgramPointer();
            //StartProgram();

            comm.LoadProgramFromStringList(module);
            comm.StartProgramExecution();
        }


        /// <summary>
        /// Remove all pending elements from the queue.
        /// </summary>
        public void ClearQueue()
        {
            queue.EmptyQueue();
        }

        /// <summary>
        /// Adds a Frame to the streaming queue
        /// </summary>
        /// <param name="frame"></param>
        public void AddFrameToStreamQueue(Frame frame)
        {
            streamQueue.Add(frame);
        }


        public void DebugDump()
        {
            DebugBanner();
            comm.DebugDump();
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




        // This should be moved somewhere else
        private static bool IsBelowTable(double z)
        {
            return z <= SafetyTableZLimit;
        }

        



    }
}
