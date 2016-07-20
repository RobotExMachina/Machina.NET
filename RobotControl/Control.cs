using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace RobotControl
{
    /// <summary>
    /// The core class that centralizes all private control.
    /// </summary>
    class Control
    {
        // Some 'environment variables' to define check states and behavior
        public static readonly bool SafetyStopImmediateOnDisconnect = true;
        public static readonly bool SafetyCheckTableCollision = true;
        public static readonly bool SafetyStopOnTableCollision = true;
        public static readonly double SafetyTableZLimit = 100;                     // table security checks will trigger under this z height (mm)
        public static readonly bool DEBUG = true;                                  // dump a bunch of debug logs
        public static readonly int DefaultVelocity = 20;
        public static readonly int DefaultZone = 5;
        public static readonly MotionType DefaultMotionType = MotionType.Linear;
        

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

        /// <summary>
        /// A buffer that stores issued actions pending to be released to controllers, exports, etc.
        /// </summary>
        private ActionBuffer actionBuffer;


        private ProgramGenerator programGenerator = new ProgramGeneratorABB();  // @TODO: this must be more programmatic and shimmed

        // STUFF NEEDED FOR STREAM MODE
        // Most of it represents a virtual current state of the robot, to be able to 
        // issue appropriate relative actions.
        // @TODO: move all of this to a VirtualRobot object and to the Comm + QueueManager objects
        //public Point TCPPosition = null;
        //public Rotation TCPRotation = null;
        //public int currentVelocity = 10;
        //public int currentZone = 5;
        //public MotionType currentMotionType = MotionType.Linear;        // linear motion by default
        public StreamQueue streamQueue;
        public bool mHeld = false;                                      // is Mastership currently held by someone? useful when several threads want to write to the robot...

        private bool arePointersInitialized = false;
        private RobotPointer virtualRobotPointer;
        private RobotPointer writeRobotPointer;

        private Settings currentSettings;





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
            Reset();  // @TODO necessary?
        }

        /// <summary>
        /// Resets all internal state properties to default values. To be invoked upon
        /// an internal robot reset.
        /// @TODO rethink this
        /// </summary>
        public void Reset()
        {
            //isConnected = false;
            //isLogged = false;
            //isMainTaskRetrieved = false;
            //IP = "";
            
            // @TODO: to deprecate
            queue = new Queue();
            streamQueue = new StreamQueue();

            actionBuffer = new ActionBuffer();

            arePointersInitialized = false;
            virtualRobotPointer = null;
            writeRobotPointer = null;

            currentSettings = new Settings(DefaultVelocity, DefaultZone, DefaultMotionType);
        }

        /// <summary>
        /// Sets current Control Mode and establishes communication if applicable.
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

            //// @TODO rework this into Virtual Robots
            //Frame curr = comm.GetCurrentFrame();
            //TCPPosition = curr.Position;
            //TCPRotation = curr.Orientation;

            return true;
        }

        /// <summary>
        /// Requests the Communication object to disconnect from controller and reset.
        /// </summary>
        /// <returns></returns>
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
        /// <param name="programLines">A string list representation of the program's code.</param>
        /// <returns></returns>
        public bool LoadProgramToDevice(List<string> programLines)
        {
            return comm.LoadProgramToController(programLines);
        }

        /// <summary>
        /// Loads a programm to the connected device and executes it. 
        /// </summary>
        /// <param name="filepath">Full filepath including root, directory structure, filename and extension.</param>
        /// <returns></returns>
        public bool LoadProgramToDevice(string filepath)
        {
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
            filename = fileparts.Length > 2 ? string.Join(".", fileparts, 0, fileparts.Length - 1) : fileparts[0];  // account for filenames with multiple dots
            extension = fileparts[fileparts.Length - 1];

            Console.WriteLine("  filename: " + filename);
            Console.WriteLine("  dirname: " + dirname);
            Console.WriteLine("  extension: " + extension);

            return comm.LoadProgramToController(dirname, filename, extension);
        }

        /// <summary>
        /// Triggers program start on device.
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
        /// Stops execution of running program on device.
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
            // @TODO: at some point when virtual robots are implemented, this will return either the real robot's TCP
            // or the virtual one's (like in offline mode).
            return comm.GetCurrentOrientation();
        }

        /// <summary>
        /// Returns a Frame object representing the current robot's TCP position and orientation. 
        /// NOTE: the Frame object's velocity and zone still do not represent the acutal state of the robot.
        /// </summary>
        /// <returns></returns>
        public Frame GetCurrentFrame()
        {
            // @TODO: at some point when virtual robots are implemented, this will return either the real robot's TCP
            // or the virtual one's (like in offline mode).
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

        public bool Export(string filepath)
        {
            List<Action> actions = actionBuffer.GetAllPending();

            List<string> programCode = programGenerator.UNSAFEProgramFromActions("offlineTests", writeRobotPointer, actions);

            // @TODO: add some filepath sanity here

            return SaveStringListToFile(programCode, filepath);
        }



        









        // █████╗  ██████╗████████╗██╗ ██████╗ ███╗   ██╗███████╗
        //██╔══██╗██╔════╝╚══██╔══╝██║██╔═══██╗████╗  ██║██╔════╝
        //███████║██║        ██║   ██║██║   ██║██╔██╗ ██║███████╗
        //██╔══██║██║        ██║   ██║██║   ██║██║╚██╗██║╚════██║
        //██║  ██║╚██████╗   ██║   ██║╚██████╔╝██║ ╚████║███████║
        //╚═╝  ╚═╝ ╚═════╝   ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝╚══════╝

        /// <summary>
        /// Sets the velocity parameter for future issued actions.
        /// </summary>
        /// <param name="vel">In mm/s</param>
        public void SetCurrentVelocity(int vel)
        {
            currentSettings.Velocity = vel;
        }

        /// <summary>
        /// Sets the approximation corner radius for future issued actions.
        /// </summary>
        /// <param name="zone">In mm.</param>
        public void SetCurrentZone(int zone)
        {
            currentSettings.Zone = zone;
        }

        /// <summary>
        /// Sets the motion type (linear, joint...) for future issued actions.
        /// </summary>
        /// <param name="type"></param>
        public void SetCurrentMotionType(MotionType type)
        {
            currentSettings.MotionType = type;
        }



        ///// <summary>
        ///// Requests relative linear movement based on current virtual position.
        ///// The action will execute based on current ControlMode and priority.
        ///// </summary>
        ///// <param name="incX"></param>
        ///// <param name="incY"></param>
        ///// <param name="incZ"></param>
        ///// <returns></returns>
        //public bool IssueRelativeMovementRequest(double incX, double incY, double incZ)
        //{
        //    if (controlMode != ControlMode.Stream)
        //    {
        //        Console.WriteLine("Move() only supported in Stream mode");
        //        return false;
        //    }

        //    if (SafetyCheckTableCollision)
        //    {
        //        if (IsBelowTable(TCPPosition.Z + incZ))
        //        {
        //            Console.WriteLine("WARNING: TCP ABOUT TO HIT THE TABLE");
        //            if (SafetyStopOnTableCollision)
        //            {
        //                return false;
        //            }
        //        }
        //    }

        //    TCPPosition.Add(incX, incY, incZ);
        //    AddFrameToStreamQueue(new Frame(TCPPosition.X, TCPPosition.Y, TCPPosition.Z, currentVelocity, currentZone));

        //    // Only tick queue if there are no targets pending to be streamed
        //    if (streamQueue.FramesPending() == 1)
        //    {
        //        comm.TickStreamQueue(false);
        //    }
        //    else
        //    {
        //        Console.WriteLine("{0} frames pending", streamQueue.FramesPending());
        //    }

        //    return true;
        //}

        ///// <summary>
        ///// Requests absolute linear movement based on current virtual position.
        ///// The action will execute based on current ControlMode and priority.
        ///// </summary>
        ///// <param name="newX"></param>
        ///// <param name="newY"></param>
        ///// <param name="newZ"></param>
        ///// <returns></returns>
        //public bool IssueAbsoluteMovementRequest(double newX, double newY, double newZ)
        //{
        //    if (controlMode != ControlMode.Stream)
        //    {
        //        Console.WriteLine("MoveTo() only supported in Stream mode");
        //        return false;
        //    }

        //    if (SafetyCheckTableCollision)
        //    {
        //        if (IsBelowTable(newZ))
        //        {
        //            Console.WriteLine("WARNING: TCP ABOUT TO HIT THE TABLE");
        //            if (SafetyStopOnTableCollision)
        //            {
        //                return false;
        //            }
        //        }
        //    }

        //    TCPPosition.Set(newX, newY, newZ);
        //    AddFrameToStreamQueue(new Frame(TCPPosition.X, TCPPosition.Y, TCPPosition.Z,
        //       TCPRotation.Q1, TCPRotation.Q2, TCPRotation.Q3, TCPRotation.Q4,
        //       currentVelocity, currentZone));

        //    // Only tick queue if there are no targets pending to be streamed
        //    if (streamQueue.FramesPending() == 1)
        //    {
        //        comm.TickStreamQueue(false);
        //    }
        //    else
        //    {
        //        Console.WriteLine("{0} frames pending", streamQueue.FramesPending());
        //    }

        //    return true;
        //}

        ///// <summary>
        ///// Requests absolute rotation movement based on current virtual orientation.
        ///// The action will execute based on current ControlMode and priority.
        ///// </summary>
        ///// <param name="q1"></param>
        ///// <param name="q2"></param>
        ///// <param name="q3"></param>
        ///// <param name="q4"></param>
        ///// <returns></returns>
        //public bool IssueAbsoluteRotationRequest(double q1, double q2, double q3, double q4)
        //{
        //    if (controlMode != ControlMode.Stream)
        //    {
        //        Console.WriteLine("RotateTo() only supported in Stream mode");
        //        return false;
        //    }

        //    // WARNING: NO TABLE COLLISIONS ARE PERFORMED HERE YET!
        //    TCPRotation.Set(q1, q2, q3, q4);
        //    AddFrameToStreamQueue(new Frame(TCPPosition.X, TCPPosition.Y, TCPPosition.Z,
        //       TCPRotation.Q1, TCPRotation.Q2, TCPRotation.Q3, TCPRotation.Q4,
        //       currentVelocity, currentZone));

        //    // Only tick queue if there are no targets pending to be streamed
        //    if (streamQueue.FramesPending() == 1)
        //    {
        //        comm.TickStreamQueue(false);
        //    }
        //    else
        //    {
        //        Console.WriteLine("{0} frames pending", streamQueue.FramesPending());
        //    }

        //    return true;
        //}


        //public bool IssueMovementRequest()
        //{
        //    throw new NotImplementedException();
        //}

        //public bool IssueRotationRequest()
        //{
        //    throw new NotImplementedException();
        //}


        public bool IssueTranslationRequest(Point trans, bool relative, int vel, int zon, MotionType mType)
        {
            if (!arePointersInitialized)
            {
                InitializeRobotPointers(trans, Frame.DefaultOrientation);

                //InitializeRobotPointer(trans, Frame.DefaultOrientation, out virtualRobotPointer);  // @TODO: defaults should depend on robot make/model
                //InitializeRobotPointer(trans, Frame.DefaultOrientation, out writeRobotPointer);  // @TODO: defaults should depend on robot make/model
            }
            
            ActionTranslation act = new ActionTranslation(trans, relative, vel, zon, mType);
            virtualRobotPointer.ApplyAction(act);
            return actionBuffer.Add(act);
        }
    
        // Overloads falling back on current settings values
        public bool IssueTranslationRequest(Point trans, bool relative)
        {
            return IssueTranslationRequest(trans, relative, currentSettings.Velocity, currentSettings.Zone, currentSettings.MotionType);
        }
        public bool IssueTranslationRequest(Point trans, bool relative, int vel, int zon)
        {
            return IssueTranslationRequest(trans, relative, vel, zon, currentSettings.MotionType);
        }
        public bool IssueTranslationRequest(Point trans, bool relative, MotionType mType)
        {
            return IssueTranslationRequest(trans, relative, currentSettings.Velocity, currentSettings.Zone, mType);
        }




        //public bool IssueTransformationRequest(Point trans, bool relTrans, Rotation rot, bool relRot, int vel, int zon, MotionType mType)
        //{
        //    throw new NotImplementedException();
        //}

        //public bool IssueConfigurationRequest()
        //{
        //    throw new NotImplementedException();
        //}





















        //██████╗ ██████╗ ██╗██╗   ██╗ █████╗ ████████╗███████╗
        //██╔══██╗██╔══██╗██║██║   ██║██╔══██╗╚══██╔══╝██╔════╝
        //██████╔╝██████╔╝██║██║   ██║███████║   ██║   █████╗  
        //██╔═══╝ ██╔══██╗██║╚██╗ ██╔╝██╔══██║   ██║   ██╔══╝  
        //██║     ██║  ██║██║ ╚████╔╝ ██║  ██║   ██║   ███████╗
        //╚═╝     ╚═╝  ╚═╝╚═╝  ╚═══╝  ╚═╝  ╚═╝   ╚═╝   ╚══════╝

        /// <summary>
        /// Initializes the Communication object.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Disconnects and resets the Communcation object.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Initializes 
        /// </summary>
        /// <param name="pointer"></param>
        /// <returns></returns>
        //private bool InitializeRobotPointer(Point position, Rotation rotation, out RobotPointer pointer)
        //{
        //    pointer = new RobotPointerABB(); // @TODO: shim brand/model
        //    return pointer.Initialize(position, rotation);
        //}

        private bool InitializeRobotPointers(Point position, Rotation rotation)
        {
            bool success = true;
            if (controlMode == ControlMode.Offline)
            {
                virtualRobotPointer = new RobotPointerABB();
                success = success && virtualRobotPointer.Initialize(position, rotation);

                writeRobotPointer = new RobotPointerABB();
                success = success && writeRobotPointer.Initialize(position, rotation);
            }

            arePointersInitialized = success;

            return success;
        }

        private bool SaveStringListToFile(List<string> lines, string filepath)
        {
            try
            {
                System.IO.File.WriteAllLines(filepath, lines, System.Text.Encoding.ASCII);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not save file...");
                Console.WriteLine(ex);
            }
            return false;
        }
































        //██╗    ██╗██╗██████╗ 
        //██║    ██║██║██╔══██╗
        //██║ █╗ ██║██║██████╔╝
        //██║███╗██║██║██╔═══╝ 
        //╚███╔███╔╝██║██║     
        // ╚══╝╚══╝ ╚═╝╚═╝     

        /// <summary>
        /// Adds a path to the queue manager and tick it for execution.
        /// </summary>
        /// <param name="path"></param>
        public void AddPathToQueue(Path path)
        {
            queue.Add(path);
            TriggerQueue();
        }

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
            List<string> module = ProgramGenerator.UNSAFEModuleFromPath(path, currentSettings.Velocity, currentSettings.Zone);

            comm.LoadProgramToController(module);
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

        // This should be moved somewhere else
        private static bool IsBelowTable(double z)
        {
            return z <= SafetyTableZLimit;
        }









        //  ██████╗ ███████╗██████╗ ██╗   ██╗ ██████╗ 
        //  ██╔══██╗██╔════╝██╔══██╗██║   ██║██╔════╝ 
        //  ██║  ██║█████╗  ██████╔╝██║   ██║██║  ███╗
        //  ██║  ██║██╔══╝  ██╔══██╗██║   ██║██║   ██║
        //  ██████╔╝███████╗██████╔╝╚██████╔╝╚██████╔╝
        //  ╚═════╝ ╚══════╝╚═════╝  ╚═════╝  ╚═════╝ 
        //                                            
        public void DebugDump()
        {
            DebugBanner();
            comm.DebugDump();
        }

        public void DebugBuffer()
        {
            actionBuffer.LogBufferedActions();
        }

        public void DebugVirtualPointer()
        {
            if (virtualRobotPointer == null)
                Console.WriteLine("Virtual pointer not initialized");
            else
                Console.WriteLine(virtualRobotPointer);
        }

        public void DebugWritePointer()
        {
            if (writeRobotPointer == null)
                Console.WriteLine("Write pointer not initialized");
            else
                Console.WriteLine(writeRobotPointer);
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

        
    }
}
