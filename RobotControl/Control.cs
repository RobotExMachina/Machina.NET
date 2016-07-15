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
        //private ConnectionMode connectionMode = RobotControl.ConnectionMode.Online;  // Try online by default (@TODO: at some point 'offline' should be the default)
        //private OnlineMode onlineMode = RobotControl.OnlineMode.Instruct;
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
        public object rapidDataLock = new object();


        





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
        public bool ConnectToController(int robotId)
        {
            // Sanity
            if (controlMode == ControlMode.Offline)
            {
                if (DEBUG) Console.WriteLine("No robot to connect to in 'offline' mode ;)");
                return false;
            }

            return comm.ConnectToDevice(robotId);

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

        public bool DisconnectFromController()
        {
            // Sanity
            if (controlMode == ControlMode.Offline)
            {
                Console.WriteLine("No robot to disconnect from in 'offline' mode ;)");
                return false;
            }

            return comm.DisconnectFromDevice();
        }

        public bool IsConnectedToController()
        {
            return comm.IsConnected();
        }

        public string GetControllerIP()
        {
            return comm.GetIP();
        }



        //public bool IsConnected()
        //{
        //    return isConnected;
        //}

            //public string GetIP()
            //{
            //    if (!isConnected)
            //    {
            //        Console.WriteLine("Not connected to any controller");
            //        return "";
            //    }

            //    return IP;
            //}
























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
            comm = new CommunicationABB();
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


        /// <summary>
        /// Upon connection, subscribe to relevant events and handle them.
        /// </summary>
        public void SubscribeToEvents()
        {
            controller.Rapid.ExecutionStatusChanged += OnExecutionStatusChanged;
        }
        

        /// <summary>
        /// Deletes all existing modules from main task in the controller. 
        /// </summary>
        /// <returns></returns>
        public int ClearAllModules()
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
        public void LoadModuleToRobot(List<string> mod)
        {
            SaveModuleToFile(mod, localBufferPathname + localBufferFilename);
            LoadModuleFromFilename(localBufferFilename, localBufferPathname);
        }

        /// <summary>
        /// Saves a string representation of a module to a local file. 
        /// </summary>
        /// <param name="module"></param>
        /// <param name="filepath"></param>
        public void SaveModuleToFile(List<string> module, string filepath)
        {
            System.IO.File.WriteAllLines(filepath, module, System.Text.Encoding.ASCII);
        }

        public bool LoadModuleFromFilepath(string filepath)
        {

            // @TODO: all this sanity and checks should probably go into LoadModuleFromFilename() (or basically consolidate everything into one method
            if (controlMode == ControlMode.Offline)
            {
                Console.WriteLine("Cannot load modules in Offline mode");
                return false;
            }

            // Sanity
            string fullPath = "";

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
                Console.WriteLine(e);
                return false;
            }
                        
            string filename, dirname;
            string[] parts = fullPath.Split('\\');
            int len = parts.Length;
            if (len < 2)
            {
                Console.WriteLine("Weird filepath");
                return false;
            }
            filename = parts[len - 1];
            dirname = string.Join("\\", parts, 0, len - 1);

            Console.WriteLine("  filename: " + filename);
            Console.WriteLine("  dirname: " + dirname);
            

            return LoadModuleFromFilename(filename, dirname);
        }

        /// <summary>
        /// Loads a module into de controller from a local file. 
        /// @TODO: This is an expensive operation, should probably become threaded. 
        /// @TODO: By default, wipes out all previous modules --> parameterize.
        /// </summary>
        /// <param name="dirname"></param>
        /// <returns></returns>
        public bool LoadModuleFromFilename(string filename, string dirname)
        {
            // When connecting to a real controller, the reference filesystem 
            // for Task.LoadModuleFromFile() becomes the controller's, so it is necessary
            // to copy the file to the system first, and then load it. 
            string fullPath = dirname + "\\" + filename;

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
                if (DEBUG) Console.WriteLine("Sucessfully loaded {0}{1}", dirname, filename);
            }

            return success;
        }



        /// <summary>
        /// Resets the program pointer in the controller to the main entry point. Needs to be called
        /// before starting execution of a program, otherwise the controller will throw an error. 
        /// </summary>
        public void ResetProgramPointer()
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
        public void StartProgram()
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
        public void StopProgram(bool immediate)
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

        ///// <summary>
        ///// Returns a RobTarget object representing the current robot's TCP.
        ///// @TODO: Should not expose the user publicly to platform-specific objects, like RobTargets
        ///// </summary>
        ///// <returns></returns>
        //public RobTarget GetTCPRobTarget()
        //{
        //    return controller.MotionSystem.ActiveMechanicalUnit.GetPosition(ABB.Robotics.Controllers.MotionDomain.CoordinateSystemType.World);
        //}

        /// <summary>
        /// Returns a Point object representing the current robot's TCP position.
        /// </summary>
        /// <returns></returns>
        public Point GetTCPPosition()
        {
            RobTarget rt = controller.MotionSystem.ActiveMechanicalUnit.GetPosition(ABB.Robotics.Controllers.MotionDomain.CoordinateSystemType.World);
            return new Point(rt.Trans);
        }

        /// <summary>
        /// Returns a Rotation object representing the current robot's TCP orientation.
        /// </summary>
        /// <returns></returns>
        public Rotation GetTCPRotation()
        {
            return new Rotation(controller.MotionSystem.ActiveMechanicalUnit.GetPosition(ABB.Robotics.Controllers.MotionDomain.CoordinateSystemType.World).Rot);
        }

        /// <summary>
        /// Returns a Frame object representing the current robot's TCP position and orientation. 
        /// NOTE: the Frame object's velocity and zone still do not represent the acutal state of the robot.
        /// </summary>
        /// <returns></returns>
        public Frame GetTCPFrame()
        {
            return new Frame(controller.MotionSystem.ActiveMechanicalUnit.GetPosition(ABB.Robotics.Controllers.MotionDomain.CoordinateSystemType.World));
        }

        ///// <summary>
        ///// Returns a RobJoint object representing the current values for joint rotations. 
        ///// </summary>
        ///// <returns></returns>
        //public RobJoint GetRobotJoints()
        //{
        //    return controller.MotionSystem.ActiveMechanicalUnit.GetPosition().RobAx;
        //}

        /// <summary>
        /// Returns a Joints object representing the rotations of the 6 axes of this robot.
        /// </summary>
        /// <returns></returns>
        public Joints GetRobotJoints()
        {
            return new Joints(controller.MotionSystem.ActiveMechanicalUnit.GetPosition().RobAx);
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
                TickStreamQueue(false);
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
                TickStreamQueue(false);
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

        /// <summary>
        /// Checks the state of the execution of the robot, and if stopped and if elements 
        /// remaining on the queue, starts executing them.
        /// </summary>
        public void TriggerQueue()
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
        public void TriggerQueue(bool robotIsStopped)
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
        public void RunPath(Path path)
        {
            if (DEBUG) Console.WriteLine("RUNNING NEW PATH: " + path.Count);
            List<string> module = ProgramGenerator.UNSAFEModuleFromPath(path, (int)currentVelocity, (int)currentZone);
            //SaveModuleToFile(module, localBufferPathname + localBufferFilename);
            //LoadModuleFromFilename(localBufferFilename, localBufferPathname);
            LoadModuleToRobot(module);
            ResetProgramPointer();
            StartProgram();
        }


        /// <summary>
        /// Remove all pending elements from the queue.
        /// </summary>
        public void ClearQueue()
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
        public void LoadStreamingModule()
        {
            LoadModuleToRobot(StaticData.StreamModule.ToList());
        }

        /// <summary>
        /// Loads all relevant Rapid variables
        /// </summary>
        public void HookUpStreamingVariables()
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
        public RapidData LoadRapidDataVariable(string varName)
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
        public void AddFrameToStreamQueue(Frame frame)
        {
            streamQueue.Add(frame);
        }

        /// <summary>
        /// This function will look at the state of the program pointer, the streamQueue, 
        /// and if necessary will add a new target to the stream. This is meant to be called
        /// to initiate the stream update chain, like when adding a new target, or pnum event handling.
        /// </summary>
        public void TickStreamQueue(bool hasPriority)
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
        public void SetNextVirtualTarget(bool hasPriority)
        {
            if (DEBUG) Console.WriteLine("Setting frame #{0}", virtualStepCounter);

            lock (rapidDataLock)
            {
                Frame target = streamQueue.GetNext();
                if (target != null)
                {
                    int fid = virtualStepCounter % virtualRDCount;
                    //// When masterhip is held, only priority calls make it through (which are the ones holding mastership)
                    //while (!hasPriority && mHeld)
                    //{
                    //    Console.WriteLine("TRAPPED IN MASTERHIP CONFLICT 1");
                    //}  // safety mechanism to not hit held mastership by eventhandlers
                    SetRapidDataVarString(RD_p[fid], target.GetUNSAFERobTargetDeclaration());
                    //while (!hasPriority && mHeld)
                    //{
                    //    Console.WriteLine("TRAPPED IN MASTERHIP CONFLICT 2");
                    //}
                    SetRapidDataVarString(RD_vel[fid], target.GetSpeedDeclaration());
                    //while (!hasPriority && mHeld)
                    //{
                    //    Console.WriteLine("TRAPPED IN MASTERHIP CONFLICT 3");
                    //}
                    SetRapidDataVarString(RD_zone[fid], target.GetZoneDeclaration());
                    //while (!hasPriority && mHeld)
                    //{
                    //    Console.WriteLine("TRAPPED IN MASTERHIP CONFLICT 4");
                    //}
                    //SetRapidDataVarBool(RD_pset[virtualStepCounter % virtualRDCount], true);  // --> Looks like this wasn't working well??
                    SetRapidDataVarString(RD_pset[fid], "TRUE");
                }
            }

        }

        /// <summary>
        /// Sets the value of a Rapid variable from a string representation
        /// </summary>
        /// <param name="rd"></param>
        /// <param name="declaration"></param>
        public void SetRapidDataVarString(RapidData rd, string declaration)
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
        public void DebugBanner()
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
        public void DebugControllerDump()
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

        public void DebugTaskDump()
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
        public static bool IsBelowTable(double z)
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
        public void OnExecutionStatusChanged(object sender, ExecutionStatusChangedEventArgs e)
        {
            if (DEBUG) Console.WriteLine("EXECUTION STATUS CHANGED: " + e.Status);

            if (e.Status == ExecutionStatus.Stopped)
            {
                // Only trigger Instruct queue
                if (controlMode == ControlMode.Execute)
                {
                    // Tick queue to move forward
                    TriggerQueue(true);
                }
            }
        }

        public void OnRD_pnum_ValueChanged(object sender, DataValueChangedEventArgs e)
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
