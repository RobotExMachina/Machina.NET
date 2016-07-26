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
        public static readonly bool SAFETY_STOP_IMMEDIATE_ON_DISCONNECT = true;         // when disconnecting from a controller, issue an immediate Stop request?
        public static readonly bool SAFETY_CHECK_TABLE_COLLISION = true;                // when issuing actions, check if it is about to hit the table?
        public static readonly bool SAFETY_STOP_ON_TABLE_COLLISION = true;              // prevent from actually hitting the table?
        public static readonly double SAFETY_TABLE_Z_LIMIT = 100;                       // table security checks will trigger below this z height (mm)

        public static readonly int DEFAULT_VELOCITY = 20;                               // default velocity for new actions
        public static readonly int DEFAULT_ZONE = 5;                                    // default zone for new actions
        public static readonly MotionType DEFAULT_MOTION_TYPE = MotionType.Linear;      // default motion type for new actions
        

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

        /// <summary>
        /// An 'interface' to create robot programs based on platform-specific languages and descriptions.
        /// </summary>
        private ProgramGenerator programGenerator = new ProgramGeneratorABB();  // @TODO: this must be more programmatic and shimmed

        /// <summary>
        /// Represents the current values for velocity, zone and MotionType.
        /// </summary>
        private Settings currentSettings;

        /// <summary>
        /// A buffer that stores Push and PopSettings() states.
        /// </summary>
        private SettingsBuffer settingsBuffer;

        /// <summary>
        /// Keeps track of the state of a virtual robot immediately following all the actions issued to Control.
        /// </summary>
        private RobotCursor virtualCursor;

        /// <summary>
        /// Keeps track of the state of a virtual robot immediately following all the actions released from the 
        /// actionsbuffer to target device defined by controlMode, like an offline program, a full intruction execution 
        /// or a streamed target.
        /// </summary>
        private RobotCursor writeCursor;

        /// <summary>
        /// Are cursors ready to start working?
        /// </summary>
        private bool areCursorsInitialized = false;

        // @TODO: this will need to get reallocated when fixing stream mode...
        public StreamQueue streamQueue;


        

        





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
            // @TODO: to deprecate
            queue = new Queue();
            streamQueue = new StreamQueue();

            actionBuffer = new ActionBuffer();

            areCursorsInitialized = false;
            virtualCursor = null;
            writeCursor = null;

            currentSettings = new Settings(DEFAULT_VELOCITY, DEFAULT_ZONE, DEFAULT_MOTION_TYPE);
            settingsBuffer = new SettingsBuffer();
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
            // @TODO: same here as above

            return comm.GetCurrentJoints();
        }

        /// <summary>
        /// For Offline modes, it flushes all pending actions and exports them to a robot-specific program as a text file.
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public bool Export(string filepath)
        {
            if (controlMode != ControlMode.Offline)
            {
                Console.WriteLine("Export() only works in Offline mode");
                return false;
            }

            // @TODO: add some filepath sanity here

            List<Action> actions = actionBuffer.GetAllPending();
            List<string> programCode = programGenerator.UNSAFEProgramFromActions("offlineTests", writeCursor, actions);

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


        public void PushCurrentSettings()
        {
            Console.WriteLine("Pushing {0}", currentSettings);
            settingsBuffer.Push(currentSettings);
            //Settings newSettings = currentSettings.Clone();  // create a new object
            //currentSettings = newSettings;
            currentSettings = currentSettings.Clone();
        }


        public void PopCurrentSettings()
        {
            currentSettings = settingsBuffer.Pop();
            Console.WriteLine("Reverted to {0}", currentSettings);
        }
















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

        /// <summary>
        /// Issue a customized simple Translation action request.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="trans"></param>
        /// <param name="relative"></param>
        /// <param name="vel"></param>
        /// <param name="zon"></param>
        /// <param name="mType"></param>
        /// <returns></returns>
        public bool IssueTranslationRequest(bool world, Point trans, bool relative, int vel, int zon, MotionType mType)
        {
            if (!areCursorsInitialized)
            {
                if (controlMode == ControlMode.Offline)
                {  
                    if ( !InitializeRobotPointers(trans, Frame.DefaultOrientation) )  // @TODO: defaults should depend on robot make/model
                    {
                        Console.WriteLine("Could not initialize cursors...");
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine("Still only working in Offline mode");
                    return false;
                }
            }
            
            ActionTranslation act = new ActionTranslation(world, trans, relative, vel, zon, mType);
            bool success = virtualCursor.ApplyAction(act);
            // Only add this action to the queue if it was successfuly applied to the virtualCursor
            if (success) actionBuffer.Add(act);
            return success;
        }

        // Overloads falling back on current settings values
        public bool IssueTranslationRequest(bool world, Point trans, bool relative)
        {
            return IssueTranslationRequest(world, trans, relative, currentSettings.Velocity, currentSettings.Zone, currentSettings.MotionType);
        }
        public bool IssueTranslationRequest(bool world, Point trans, bool relative, int vel, int zon)
        {
            return IssueTranslationRequest(world, trans, relative, vel, zon, currentSettings.MotionType);
        }
        public bool IssueTranslationRequest(bool world, Point trans, bool relative, MotionType mType)
        {
            return IssueTranslationRequest(world, trans, relative, currentSettings.Velocity, currentSettings.Zone, mType);
        }




        public bool IssueRotationRequest(bool world, Rotation rot, bool relative, int vel, int zon, MotionType mType)
        {
            if (!areCursorsInitialized)
            {
                if (controlMode == ControlMode.Offline)
                {
                    if (!InitializeRobotPointers(new Point(), rot))  // @TODO: defaults should depend on robot make/model
                    {
                        Console.WriteLine("Could not initialize cursors...");
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine("Still only working in Offline mode");
                    return false;
                }
            }

            ActionRotation act = new ActionRotation(world, rot, relative, vel, zon, mType);
            bool success = virtualCursor.ApplyAction(act);
            // Only add this action to the queue if it was successfuly applied to the virtualCursor
            if (success) actionBuffer.Add(act);
            return success;
        }

        // Overloads falling back on current settings values
        public bool IssueRotationRequest(bool world, Rotation rot, bool relative)
        {
            return IssueRotationRequest(world, rot, relative, currentSettings.Velocity, currentSettings.Zone, currentSettings.MotionType);
        }
        public bool IssueRotationRequest(bool world, Rotation rot, bool relative, int vel, int zon)
        {
            return IssueRotationRequest(world, rot, relative, vel, zon, currentSettings.MotionType);
        }
        public bool IssueRotationRequest(bool world, Rotation rot, bool relative, MotionType mType)
        {
            return IssueRotationRequest(world, rot, relative, currentSettings.Velocity, currentSettings.Zone, mType);
        }




        public bool IssueTranslationAndRotationRequest(
            bool worldTrans, Point trans, bool relTrans, 
            bool worldRot, Rotation rot, bool relRot, 
            int vel, int zon, MotionType mType)
        {
            if (!areCursorsInitialized)
            {
                if (controlMode == ControlMode.Offline)
                {
                    if (!InitializeRobotPointers(trans, rot))  // @TODO: defaults should depend on robot make/model
                    {
                        Console.WriteLine("Could not initialize cursors...");
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine("Still only working in Offline mode");
                    return false;
                }
            }

            ActionTranslationAndRotation act = new ActionTranslationAndRotation(worldTrans, trans, relTrans, worldRot, rot, relRot, vel, zon, mType);
            // Only add this action to the queue if it was successfuly applied to the virtualCursor
            if (virtualCursor.ApplyAction(act))
            {
                actionBuffer.Add(act);
                return true;
            }
            return false;
        }

        // Overloads falling back on current settings values
        public bool IssueTranslationAndRotationRequest(
            bool worldTrans, Point trans, bool relTrans,
            bool worldRot, Rotation rot, bool relRot)
        {
            return IssueTranslationAndRotationRequest(
                worldTrans, trans, relTrans, 
                worldRot, rot, relRot, 
                currentSettings.Velocity, currentSettings.Zone, currentSettings.MotionType);
        }
        public bool IssueTranslationAndRotationRequest(
            bool worldTrans, Point trans, bool relTrans,
            bool worldRot, Rotation rot, bool relRot, 
            int vel, int zon)
        {
            return IssueTranslationAndRotationRequest(
                worldTrans, trans, relTrans,
                worldRot, rot, relRot,
                vel, zon, currentSettings.MotionType);
        }
        public bool IssueTranslationAndRotationRequest(
            bool worldTrans, Point trans, bool relTrans,
            bool worldRot, Rotation rot, bool relRot, 
            MotionType mType)
        {
            return IssueTranslationAndRotationRequest(
                worldTrans, trans, relTrans,
                worldRot, rot, relRot,
                currentSettings.Velocity, currentSettings.Zone, mType);
        }




        public bool IssueRotationAndTranslationRequest(
            bool worldRot, Rotation rot, bool relRot,
            bool worldTrans, Point trans, bool relTrans,
            int vel, int zon, MotionType mType)
        {
            if (!areCursorsInitialized)
            {
                if (controlMode == ControlMode.Offline)
                {
                    if (!InitializeRobotPointers(trans, rot))  // @TODO: defaults should depend on robot make/model
                    {
                        Console.WriteLine("Could not initialize cursors...");
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine("Still only working in Offline mode");
                    return false;
                }
            }

            ActionRotationAndTranslation act = new ActionRotationAndTranslation(worldRot, rot, relRot, worldTrans, trans, relTrans, vel, zon, mType);
            // Only add this action to the queue if it was successfuly applied to the virtualCursor
            if (virtualCursor.ApplyAction(act))
            {
                actionBuffer.Add(act);
                return true;
            }
            return false;
        }

        // Overloads falling back on current settings values
        public bool IssueRotationAndTranslationRequest(
            bool worldRot, Rotation rot, bool relRot,
            bool worldTrans, Point trans, bool relTrans)
        {
            return IssueRotationAndTranslationRequest(
                worldRot, rot, relRot,
                worldTrans, trans, relTrans,
                currentSettings.Velocity, currentSettings.Zone, currentSettings.MotionType);
        }
        public bool IssueRotationAndTranslationRequest(
            bool worldRot, Rotation rot, bool relRot,
            bool worldTrans, Point trans, bool relTrans,
            int vel, int zon)
        {
            return IssueRotationAndTranslationRequest(
                worldRot, rot, relRot,
                worldTrans, trans, relTrans,
                vel, zon, currentSettings.MotionType);
        }
        public bool IssueRotationAndTranslationRequest(
            bool worldRot, Rotation rot, bool relRot,
            bool worldTrans, Point trans, bool relTrans,
            MotionType mType)
        {
            return IssueRotationAndTranslationRequest(
                worldRot, rot, relRot,
                worldTrans, trans, relTrans,
                currentSettings.Velocity, currentSettings.Zone, mType);
        }




        public bool IssueJointsRequest(Joints joints, bool relJnts, int vel, int zon)
        {
            if (!areCursorsInitialized)
            {
                if (controlMode == ControlMode.Offline)
                {
                    if (relJnts)
                    {
                        Console.WriteLine("Sorry, first Joints action upon offline robot initialization must be in absolute values");
                        return false;
                    }

                    if (!InitializeRobotPointers(new Point(), Frame.DefaultOrientation))  // @TODO: defaults should depend on robot make/model
                    {
                        Console.WriteLine("Could not initialize cursors...");
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine("Still only working in Offline mode");
                    return false;
                }
            }

            ActionJoints act = new ActionJoints(joints, relJnts, vel, zon, MotionType.Joints);
            // Only add this action to the queue if it was successfuly applied to the virtualCursor
            if (virtualCursor.ApplyAction(act))
            {
                actionBuffer.Add(act);
                return true;
            }
            return false;
        }

        // Overloads falling back on current settings values
        public bool IssueJointsRequest(Joints joints, bool relJnts)
        {
            return IssueJointsRequest(joints, relJnts, currentSettings.Velocity, currentSettings.Zone);
        }





        public bool IssueMessageRequest(string message)
        {
            if (!areCursorsInitialized)
            {
                if (controlMode == ControlMode.Offline)
                {
                    if (!InitializeRobotPointers(new Point(), Frame.DefaultOrientation))  // @TODO: defaults should depend on robot make/model
                    {
                        Console.WriteLine("Could not initialize cursors...");
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine("Still only working in Offline mode");
                    return false;
                }
            }

            ActionMessage act = new ActionMessage(message);
            if (virtualCursor.ApplyAction(act))
            {
                actionBuffer.Add(act);
                return true;
            }
            return false;
        }

        public bool IssueWaitRequest(long millis)
        {
            if (!areCursorsInitialized)
            {
                if (controlMode == ControlMode.Offline)
                {
                    if (!InitializeRobotPointers(new Point(), Frame.DefaultOrientation))  // @TODO: defaults should depend on robot make/model
                    {
                        Console.WriteLine("Could not initialize cursors...");
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine("Still only working in Offline mode");
                    return false;
                }
            }

            ActionWait act = new ActionWait(millis);
            if (virtualCursor.ApplyAction(act))
            {
                actionBuffer.Add(act);
                return true;
            }
            return false;
        }











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
        private bool InitializeRobotPointers(Point position, Rotation rotation)
        {
            bool success = true;
            if (controlMode == ControlMode.Offline)
            {
                virtualCursor = new RobotCursorABB("virtualCursor");
                success = success && virtualCursor.Initialize(position, rotation);

                writeCursor = new RobotCursorABB("writeCursor");
                success = success && writeCursor.Initialize(position, rotation);

                areCursorsInitialized = success;
            }    

            return success;
        }

        /// <summary>
        /// Saves a string List to a file.
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="filepath"></param>
        /// <returns></returns>
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
        public static bool IsBelowTable(double z)
        {
            return z < SAFETY_TABLE_Z_LIMIT;
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
            Console.WriteLine("BUFFERED ACTIONS:");
            actionBuffer.LogBufferedActions();
        }

        public void DebugRobotCursors()
        {
            if (virtualCursor == null)
                Console.WriteLine("Virtual pointer not initialized");
            else
                Console.WriteLine(virtualCursor);

            if (writeCursor == null)
                Console.WriteLine("Write pointer not initialized");
            else
                Console.WriteLine(writeCursor);
        }

        public void DebugSettingsBuffer()
        {
            settingsBuffer.LogBuffer();
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
