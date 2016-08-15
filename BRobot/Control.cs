using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace BRobot
{
    /// <summary>
    /// The core class that centralizes all private control.
    /// </summary>
    class Control
    {
        // Some 'environment variables' to define check states and behavior
        public const bool SAFETY_STOP_IMMEDIATE_ON_DISCONNECT = true;         // when disconnecting from a controller, issue an immediate Stop request?
        public const bool SAFETY_CHECK_TABLE_COLLISION = true;                // when issuing actions, check if it is about to hit the table?
        public const bool SAFETY_STOP_ON_TABLE_COLLISION = true;              // prevent from actually hitting the table?
        public const double SAFETY_TABLE_Z_LIMIT = 100;                       // table security checks will trigger below this z height (mm)

        public const int DEFAULT_SPEED = 20;                                  // default speed for new actions
        public const int DEFAULT_ZONE = 5;                                    // default zone for new actions
        public const MotionType DEFAULT_MOTION_TYPE = MotionType.Linear;      // default motion type for new actions
        public const ReferenceCS DEFAULT_REFCS = ReferenceCS.World;          // default reference coordinate system for relative transform actions
        public const ControlMode DEFAULT_CONTROLMODE = ControlMode.Offline;
        public const RunMode DEFAULT_RUNMODE = RunMode.Once;                  
        




        /// <summary>
        /// Operation modes by default
        /// </summary>
        private ControlMode controlMode = DEFAULT_CONTROLMODE;
        private RunMode runMode = DEFAULT_RUNMODE;


        internal Robot parent;

        /// <summary>
        /// Instances of the main robot Controller and Task
        /// </summary>
        private Communication comm;

        ///// <summary>
        ///// Represents the current values for speed, zone and MotionType.
        ///// </summary>
        //private Settings currentSettings;

        ///// <summary>
        ///// A buffer that stores Push and PopSettings() states.
        ///// </summary>
        //private SettingsBuffer settingsBuffer;

        /// <summary>
        /// A virtual representation of the state of the device after application of issued actions.
        /// </summary>
        private RobotCursor virtualCursor;

        /// <summary>
        /// A virtual representation of the state of the device after releasing pending actions to the controller.
        /// Keeps track of the state of a virtual robot immediately following all the actions released from the 
        /// actionsbuffer to target device defined by controlMode, like an offline program, a full intruction execution 
        /// or a streamed target.
        /// </summary>
        private RobotCursor writeCursor;

        /// <summary>
        /// A virtual representation of the current motion state of the device.
        /// </summary>
        private RobotCursor motionCursor;

        /// <summary>
        /// A shared instance of a Thread to manage sending and executing actions
        /// in the controller, which typically takes a lot of resources
        /// and halts program execution
        /// </summary>
        private Thread actionsExecuter;

        /// <summary>
        /// Are cursors ready to start working?
        /// </summary>
        private bool areCursorsInitialized = false;

        //// @TODO: this will need to get reallocated when fixing stream mode...
        //public StreamQueue streamQueue;










        //██████╗ ██╗   ██╗██████╗ ██╗     ██╗ ██████╗
        //██╔══██╗██║   ██║██╔══██╗██║     ██║██╔════╝
        //██████╔╝██║   ██║██████╔╝██║     ██║██║     
        //██╔═══╝ ██║   ██║██╔══██╗██║     ██║██║     
        //██║     ╚██████╔╝██████╔╝███████╗██║╚██████╗
        //╚═╝      ╚═════╝ ╚═════╝ ╚══════╝╚═╝ ╚═════╝

        /// <summary>
        /// Main constructor.
        /// </summary>
        public Control(Robot parentBot)
        {
            parent = parentBot;
            Reset();  // @TODO necessary?
        }

        /// <summary>
        /// Resets all internal state properties to default values. To be invoked upon
        /// an internal robot reset.
        /// @TODO rethink this
        /// </summary>
        public void Reset()
        {
            virtualCursor = new RobotCursorABB("virtualCursor", true);
            writeCursor = new RobotCursorABB("writeCursor", false);
            virtualCursor.SetChild(writeCursor);
            motionCursor = new RobotCursorABB("motionCursor", false);
            writeCursor.SetChild(motionCursor);
            areCursorsInitialized = false;

            SetControlMode(DEFAULT_CONTROLMODE);

            //currentSettings = new Settings(DEFAULT_SPEED, DEFAULT_ZONE, DEFAULT_MOTION_TYPE, DEFAULT_REFCS);
            //settingsBuffer = new SettingsBuffer();
        }

        /// <summary>
        /// Sets current Control Mode and establishes communication if applicable.
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public bool SetControlMode(ControlMode mode)
        {
            controlMode = mode;

            //// @TODO: Make changes in ControlMode at runtime possible, i.e. resetting controllers and communication, flushing queues, etc.
            //if (mode == ControlMode.Offline)
            //{
            //    DropCommunication();
            //}
            //else
            //{
            //    InitializeCommunication();  // online modes
            //}

            if (mode == ControlMode.Offline)
            {
                DropCommunication();

                // In offline modes, initialize the robot to a bogus standard transform
                InitializeRobotCursors(new Point(), Rotation.FlippedAroundY);  // @TODO: defaults should depend on robot make/model
            }
            else
            {
                InitializeCommunication();
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
            else
            {
                SetRunMode(runMode);

                // If successful, initialize robot cursors to mirror the state of the device
                Point currPos = comm.GetCurrentPosition();
                Rotation currRot = comm.GetCurrentOrientation();
                Joints currJnts = comm.GetCurrentJoints();
                InitializeRobotCursors(currPos, currRot, currJnts);
            }

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

        ///// <summary>
        ///// Returns a Frame object representing the current robot's TCP position and orientation. 
        ///// NOTE: the Frame object's speed and zone still do not represent the acutal state of the robot.
        ///// </summary>
        ///// <returns></returns>
        //public Frame GetCurrentFrame()
        //{
        //    // @TODO: at some point when virtual robots are implemented, this will return either the real robot's TCP
        //    // or the virtual one's (like in offline mode).

        //    return comm.GetCurrentFrame();
        //}

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
        /// For Offline modes, it flushes all pending actions and returns a devide-specific program 
        /// as a stringList representation.
        /// </summary>
        /// <returns></returns>
        public List<string> Export()
        {
            if (controlMode != ControlMode.Offline)
            {
                Console.WriteLine("Export() only works in Offline mode");
                return null;
            }

            //List<Action> actions = actionBuffer.GetAllPending();
            //return programGenerator.UNSAFEProgramFromActions("BRobotProgram", writeCursor, actions);

            return writeCursor.ProgramFromBuffer();
        }

        /// <summary>
        /// For Offline modes, it flushes all pending actions and exports them to a robot-specific program as a text file.
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public bool Export(string filepath)
        {
            // @TODO: add some filepath sanity here

            List<string> programCode = Export();
            if (programCode == null) return false;
            return SaveStringListToFile(programCode, filepath);
        }

        /// <summary>
        /// In 'execute' mode, flushes all pending actions, creates a program, 
        /// uploads it to the controller and runs it.
        /// </summary>
        /// <returns></returns>
        public void Execute()
        {
            if (controlMode != ControlMode.Execute)
            {
                Console.WriteLine("Execute() only works in Execute mode");
                return;
            }

            writeCursor.QueueActions();
            TickWriteCursor();
        }



        //  ███████╗███████╗████████╗████████╗██╗███╗   ██╗ ██████╗ ███████╗
        //  ██╔════╝██╔════╝╚══██╔══╝╚══██╔══╝██║████╗  ██║██╔════╝ ██╔════╝
        //  ███████╗█████╗     ██║      ██║   ██║██╔██╗ ██║██║  ███╗███████╗
        //  ╚════██║██╔══╝     ██║      ██║   ██║██║╚██╗██║██║   ██║╚════██║
        //  ███████║███████╗   ██║      ██║   ██║██║ ╚████║╚██████╔╝███████║
        //  ╚══════╝╚══════╝   ╚═╝      ╚═╝   ╚═╝╚═╝  ╚═══╝ ╚═════╝ ╚══════╝
        //   

        /// <summary>
        /// Gets current speed setting.
        /// </summary>
        /// <returns></returns>
        public int GetCurrentSpeedSetting()
        {
            // @TODO: will need to decide if this returns the current virtual, write or motion speed
            return virtualCursor.speed;
        }

        /// <summary>
        /// Gets current zone setting.
        /// </summary>
        /// <returns></returns>
        public int GetCurrentZoneSetting()
        {
            return virtualCursor.zone;
        }

        /// <summary>
        /// Gets current Motion setting.
        /// </summary>
        /// <returns></returns>
        public MotionType GetCurrentMotionTypeSetting()
        {
            return virtualCursor.motionType;
        }

        /// <summary>
        /// Gets the reference coordinate system used for relative transform actions.
        /// </summary>
        /// <returns></returns>
        public ReferenceCS GetCurrentReferenceCS()
        {
            return virtualCursor.referenceCS;
        }
        
        ///// <summary>
        ///// Buffers current state settings (speed, zone, motion type...), and opens up for 
        ///// temporary settings changes to be reverted by PopSettings().
        ///// </summary>
        //public void PushCurrentSettings()
        //{
        //    //Console.WriteLine("Pushing {0}", currentSettings);
        //    settingsBuffer.Push(currentSettings);
        //    currentSettings = currentSettings.Clone();  // sets currentS to a new object
        //}

        ///// <summary>
        ///// Reverts the state settings (speed, zone, motion type...) to the previously buffered
        ///// state by PushSettings().
        ///// </summary>
        //public void PopCurrentSettings()
        //{
        //    currentSettings = settingsBuffer.Pop();
        //    //Console.WriteLine("Reverted to {0}", currentSettings);
        //}








        // █████╗  ██████╗████████╗██╗ ██████╗ ███╗   ██╗███████╗
        //██╔══██╗██╔════╝╚══██╔══╝██║██╔═══██╗████╗  ██║██╔════╝
        //███████║██║        ██║   ██║██║   ██║██╔██╗ ██║███████╗
        //██╔══██║██║        ██║   ██║██║   ██║██║╚██╗██║╚════██║
        //██║  ██║╚██████╗   ██║   ██║╚██████╔╝██║ ╚████║███████║
        //╚═╝  ╚═╝ ╚═════╝   ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝╚══════╝

        /// <summary>
        /// Issue an Action of whatever kind...
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool IssueApplyActionRequest(Action action)
        {
            if (!areCursorsInitialized)
            {
                Console.WriteLine("ERROR: cursors not initialized. Did you .Connect()?");
                return false;
            }

            bool success = virtualCursor.Issue(action);
            if (controlMode == ControlMode.Stream) comm.TickStreamQueue(true);
            return success;
        }

        /// <summary>
        /// Sets the speed parameter for future issued actions.
        /// </summary>
        /// <param name="speed">In mm/s</param>
        public bool IssueSpeedRequest(int speed, bool relative)
        {
            if (!areCursorsInitialized)
            {
                Console.WriteLine("ERROR: cursors not initialized. Did you .Connect()?");
                return false;
            }
            
            ActionSpeed act = new ActionSpeed(speed, relative);

            bool success = virtualCursor.Issue(act);
            if (controlMode == ControlMode.Stream) comm.TickStreamQueue(true);
            return success;
        }


        public bool IssueZoneRequest(int zone, bool relative)
        {
            if (!areCursorsInitialized)
            {
                Console.WriteLine("ERROR: cursors not initialized. Did you .Connect()?");
                return false;
            }

            ActionZone act = new ActionZone(zone, relative);

            bool success = virtualCursor.Issue(act);
            if (controlMode == ControlMode.Stream) comm.TickStreamQueue(true);
            return success;
        }

        public bool IssueMotionRequest(MotionType motionType)
        {
            if (!areCursorsInitialized)
            {
                Console.WriteLine("ERROR: cursors not initialized. Did you .Connect()?");
                return false;
            }

            ActionMotion act = new ActionMotion(motionType);

            bool success = virtualCursor.Issue(act);
            if (controlMode == ControlMode.Stream) comm.TickStreamQueue(true);
            return success;
        }

        public bool IssueCoordinatesRequest(ReferenceCS referenceCS)
        {
            if (!areCursorsInitialized)
            {
                Console.WriteLine("ERROR: cursors not initialized. Did you .Connect()?");
                return false;
            }

            ActionCoordinates act = new ActionCoordinates(referenceCS);

            bool success = virtualCursor.Issue(act);
            if (controlMode == ControlMode.Stream) comm.TickStreamQueue(true);
            return success;
        }













        /// <summary>
        /// Issue a Translation action request that falls back on the state of current settings.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="trans"></param>
        /// <param name="relative"></param>
        /// <param name="speed"></param>
        /// <param name="zone"></param>
        /// <param name="mType"></param>
        /// <returns></returns>
        public bool IssueTranslationRequest(Point trans, bool relative)
        {
            if (!areCursorsInitialized)
            {
                //if (controlMode == ControlMode.Offline)
                //{  
                //    if ( !InitializeRobotCursors(trans, Frame.DefaultOrientation) )  // @TODO: defaults should depend on robot make/model
                //    {
                //        Console.WriteLine("Could not initialize cursors...");
                //        return false;
                //    }
                //}
                //else
                //{
                    Console.WriteLine("ERROR: cursors not initialized. Did you .Connect()?");
                    return false;
                //}
            }

            ActionTranslation act = new ActionTranslation(trans, relative);

            bool success = virtualCursor.Issue(act);
            if (controlMode == ControlMode.Stream) comm.TickStreamQueue(true);
            return success;
        }

        ///// <summary>
        ///// Issue a Translation action request that falls back on the state of current settings.
        ///// </summary>
        ///// <param name="world"></param>
        ///// <param name="trans"></param>
        ///// <param name="relative"></param>
        ///// <returns></returns>
        //public bool IssueTranslationRequest(Point trans, bool relative)
        //{
        //    return IssueTranslationRequest(currentSettings.RefCS == ReferenceCS.World, trans, relative, currentSettings.Speed, currentSettings.Zone, currentSettings.MotionType);
        //}
        ///// <summary>
        ///// Issue a Translation action request that falls back on the state of current settings.
        ///// </summary>
        ///// <param name="world"></param>
        ///// <param name="trans"></param>
        ///// <param name="relative"></param>
        ///// <returns></returns>
        //public bool IssueTranslationRequest(bool world, Point trans, bool relative)
        //{
        //    return IssueTranslationRequest(world, trans, relative, currentSettings.Speed, currentSettings.Zone, currentSettings.MotionType);
        //}
        ///// <summary>
        ///// Issue a Translation action request that falls back on the state of current settings.
        ///// </summary>
        ///// <param name="world"></param>
        ///// <param name="trans"></param>
        ///// <param name="relative"></param>
        ///// <param name="speed"></param>
        ///// <param name="zone"></param>
        ///// <returns></returns>
        //public bool IssueTranslationRequest(bool world, Point trans, bool relative, int speed, int zone)
        //{
        //    return IssueTranslationRequest(world, trans, relative, speed, zone, currentSettings.MotionType);
        //}
        ///// <summary>
        ///// Issue a Translation action request that falls back on the state of current settings.
        ///// </summary>
        ///// <param name="world"></param>
        ///// <param name="trans"></param>
        ///// <param name="relative"></param>
        ///// <param name="mType"></param>
        ///// <returns></returns>
        //public bool IssueTranslationRequest(bool world, Point trans, bool relative, MotionType mType)
        //{
        //    return IssueTranslationRequest(world, trans, relative, currentSettings.Speed, currentSettings.Zone, mType);
        //}




        /// <summary>
        /// Issue a Rotation action request with fully customized parameters.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="rot"></param>
        /// <param name="relative"></param>
        /// <param name="speed"></param>
        /// <param name="zone"></param>
        /// <param name="mType"></param>
        /// <returns></returns>
        public bool IssueRotationRequest(Rotation rot, bool relative)
        {
            if (!areCursorsInitialized)
            {
                //if (controlMode == ControlMode.Offline)
                //{
                //    if (!InitializeRobotCursors(new Point(), rot))  // @TODO: defaults should depend on robot make/model
                //    {
                //        Console.WriteLine("Could not initialize cursors...");
                //        return false;
                //    }
                //}
                //else
                //{
                    Console.WriteLine("ERROR: cursors not initialized. Did you .Connect()?");
                    return false;
                //}
            }

            ActionRotation act = new ActionRotation(rot, relative);

            bool success = virtualCursor.Issue(act);
            if (controlMode == ControlMode.Stream) comm.TickStreamQueue(true);
            return success;

        }

        ///// <summary>
        ///// Issue a Rotation action request that falls back on the state of current settings.
        ///// </summary>
        ///// <param name="world"></param>
        ///// <param name="rot"></param>
        ///// <param name="relative"></param>
        ///// <returns></returns>
        //public bool IssueRotationRequest(Rotation rot, bool relative)
        //{
        //    return IssueRotationRequest(currentSettings.RefCS == ReferenceCS.World, rot, relative, currentSettings.Speed, currentSettings.Zone, currentSettings.MotionType);
        //}
        ///// <summary>
        ///// Issue a Rotation action request that falls back on the state of current settings.
        ///// </summary>
        ///// <param name="world"></param>
        ///// <param name="rot"></param>
        ///// <param name="relative"></param>
        ///// <returns></returns>
        //public bool IssueRotationRequest(bool world, Rotation rot, bool relative)
        //{
        //    return IssueRotationRequest(world, rot, relative, currentSettings.Speed, currentSettings.Zone, currentSettings.MotionType);
        //}
        ///// <summary>
        ///// Issue a Rotation action request that falls back on the state of current settings.
        ///// </summary>
        ///// <param name="world"></param>
        ///// <param name="rot"></param>
        ///// <param name="relative"></param>
        ///// <param name="speed"></param>
        ///// <param name="zone"></param>
        ///// <returns></returns>
        //public bool IssueRotationRequest(bool world, Rotation rot, bool relative, int speed, int zone)
        //{
        //    return IssueRotationRequest(world, rot, relative, speed, zone, currentSettings.MotionType);
        //}
        ///// <summary>
        ///// Issue a Rotation action request that falls back on the state of current settings.
        ///// </summary>
        ///// <param name="world"></param>
        ///// <param name="rot"></param>
        ///// <param name="relative"></param>
        ///// <param name="mType"></param>
        ///// <returns></returns>
        //public bool IssueRotationRequest(bool world, Rotation rot, bool relative, MotionType mType)
        //{
        //    return IssueRotationRequest(world, rot, relative, currentSettings.Speed, currentSettings.Zone, mType);
        //}



        /// <summary>
        /// Issue a Translation + Rotation action request with fully customized parameters.
        /// </summary>
        /// <param name="worldTrans"></param>
        /// <param name="trans"></param>
        /// <param name="relTrans"></param>
        /// <param name="worldRot"></param>
        /// <param name="rot"></param>
        /// <param name="relRot"></param>
        /// <param name="speed"></param>
        /// <param name="zone"></param>
        /// <param name="mType"></param>
        /// <returns></returns>
        public bool IssueTransformationRequest(Point trans, Rotation rot, bool rel, bool translationFirst)
        {
            if (!areCursorsInitialized)
            {
                //if (controlMode == ControlMode.Offline)
                //{
                //    if (!InitializeRobotCursors(trans, rot))  // @TODO: defaults should depend on robot make/model
                //    {
                //        Console.WriteLine("Could not initialize cursors...");
                //        return false;
                //    }
                //}
                //else
                //{
                    Console.WriteLine("ERROR: cursors not initialized. Did you .Connect()?");
                    return false;
                //}
            }

            //ActionTranslationAndRotation act = new ActionTranslationAndRotation(worldTrans, trans, relTrans, worldRot, rot, relRot, speed, zone, mType);
            //return virtualCursor.Issue(act);

            ActionTransformation act = new ActionTransformation(trans, rot, rel, translationFirst);

            bool success = virtualCursor.Issue(act);
            if (controlMode == ControlMode.Stream) comm.TickStreamQueue(true);
            return success;

        }

        ///// <summary>
        ///// Issue a Translation + Rotation action request that falls back on the state of current settings.
        ///// </summary>
        ///// <param name="worldTrans"></param>
        ///// <param name="trans"></param>
        ///// <param name="relTrans"></param>
        ///// <param name="worldRot"></param>
        ///// <param name="rot"></param>
        ///// <param name="relRot"></param>
        ///// <returns></returns>
        //public bool IssueTranslationAndRotationRequest(
        //    Point trans, bool relTrans,
        //    Rotation rot, bool relRot)
        //{
        //    return IssueTranslationAndRotationRequest(
        //        currentSettings.RefCS == ReferenceCS.World, trans, relTrans,
        //        currentSettings.RefCS == ReferenceCS.World, rot, relRot,
        //        currentSettings.Speed, currentSettings.Zone, currentSettings.MotionType);
        //}
        ///// <summary>
        ///// Issue a Translation + Rotation action request that falls back on the state of current settings.
        ///// </summary>
        ///// <param name="worldTrans"></param>
        ///// <param name="trans"></param>
        ///// <param name="relTrans"></param>
        ///// <param name="worldRot"></param>
        ///// <param name="rot"></param>
        ///// <param name="relRot"></param>
        ///// <returns></returns>
        //public bool IssueTranslationAndRotationRequest(
        //    bool worldTrans, Point trans, bool relTrans,
        //    bool worldRot, Rotation rot, bool relRot)
        //{
        //    return IssueTranslationAndRotationRequest(
        //        worldTrans, trans, relTrans, 
        //        worldRot, rot, relRot, 
        //        currentSettings.Speed, currentSettings.Zone, currentSettings.MotionType);
        //}
        ///// <summary>
        ///// Issue a Translation + Rotation action request that falls back on the state of current settings.
        ///// </summary>
        ///// <param name="worldTrans"></param>
        ///// <param name="trans"></param>
        ///// <param name="relTrans"></param>
        ///// <param name="worldRot"></param>
        ///// <param name="rot"></param>
        ///// <param name="relRot"></param>
        ///// <param name="speed"></param>
        ///// <param name="zone"></param>
        ///// <returns></returns>
        //public bool IssueTranslationAndRotationRequest(
        //    bool worldTrans, Point trans, bool relTrans,
        //    bool worldRot, Rotation rot, bool relRot, 
        //    int speed, int zone)
        //{
        //    return IssueTranslationAndRotationRequest(
        //        worldTrans, trans, relTrans,
        //        worldRot, rot, relRot,
        //        speed, zone, currentSettings.MotionType);
        //}
        ///// <summary>
        ///// Issue a Translation + Rotation action request that falls back on the state of current settings.
        ///// </summary>
        ///// <param name="worldTrans"></param>
        ///// <param name="trans"></param>
        ///// <param name="relTrans"></param>
        ///// <param name="worldRot"></param>
        ///// <param name="rot"></param>
        ///// <param name="relRot"></param>
        ///// <param name="mType"></param>
        ///// <returns></returns>
        //public bool IssueTranslationAndRotationRequest(
        //    bool worldTrans, Point trans, bool relTrans,
        //    bool worldRot, Rotation rot, bool relRot, 
        //    MotionType mType)
        //{
        //    return IssueTranslationAndRotationRequest(
        //        worldTrans, trans, relTrans,
        //        worldRot, rot, relRot,
        //        currentSettings.Speed, currentSettings.Zone, mType);
        //}



        ///// <summary>
        ///// Issue a Rotation + Translation action request with fully customized parameters.
        ///// </summary>
        ///// <param name="worldRot"></param>
        ///// <param name="rot"></param>
        ///// <param name="relRot"></param>
        ///// <param name="worldTrans"></param>
        ///// <param name="trans"></param>
        ///// <param name="relTrans"></param>
        ///// <param name="speed"></param>
        ///// <param name="zone"></param>
        ///// <param name="mType"></param>
        ///// <returns></returns>
        //public bool IssueRotationAndTranslationRequest(
        //    bool worldRot, Rotation rot, bool relRot,
        //    bool worldTrans, Point trans, bool relTrans,
        //    int speed, int zone, MotionType mType)
        //{
        //    if (!areCursorsInitialized)
        //    {
        //        if (controlMode == ControlMode.Offline)
        //        {
        //            if (!InitializeRobotCursors(trans, rot))  // @TODO: defaults should depend on robot make/model
        //            {
        //                Console.WriteLine("Could not initialize cursors...");
        //                return false;
        //            }
        //        }
        //        else
        //        {
        //            Console.WriteLine("ERROR: cursors not initialized. Did you .Connect()?");
        //            return false;
        //        }
        //    }

        //    ActionRotationAndTranslation act = new ActionRotationAndTranslation(worldRot, rot, relRot, worldTrans, trans, relTrans, speed, zone, mType);
        //    //return virtualCursor.Issue(act);
        //    bool success = virtualCursor.Issue(act);
        //    if (controlMode == ControlMode.Stream) comm.TickStreamQueue(true);
        //    return success;

        //}

        ///// <summary>
        ///// Issue a Rotation + Translation action request that falls back on the state of current settings.
        ///// </summary>
        ///// <param name="worldRot"></param>
        ///// <param name="rot"></param>
        ///// <param name="relRot"></param>
        ///// <param name="worldTrans"></param>
        ///// <param name="trans"></param>
        ///// <param name="relTrans"></param>
        ///// <returns></returns>
        //public bool IssueRotationAndTranslationRequest(
        //    Rotation rot, bool relRot,
        //    Point trans, bool relTrans)
        //{
        //    return IssueRotationAndTranslationRequest(
        //        currentSettings.RefCS == ReferenceCS.World, rot, relRot,
        //        currentSettings.RefCS == ReferenceCS.World, trans, relTrans,
        //        currentSettings.Speed, currentSettings.Zone, currentSettings.MotionType);
        //}
        ///// <summary>
        ///// Issue a Rotation + Translation action request that falls back on the state of current settings.
        ///// </summary>
        ///// <param name="worldRot"></param>
        ///// <param name="rot"></param>
        ///// <param name="relRot"></param>
        ///// <param name="worldTrans"></param>
        ///// <param name="trans"></param>
        ///// <param name="relTrans"></param>
        ///// <returns></returns>
        //public bool IssueRotationAndTranslationRequest(
        //    bool worldRot, Rotation rot, bool relRot,
        //    bool worldTrans, Point trans, bool relTrans)
        //{
        //    return IssueRotationAndTranslationRequest(
        //        worldRot, rot, relRot,
        //        worldTrans, trans, relTrans,
        //        currentSettings.Speed, currentSettings.Zone, currentSettings.MotionType);
        //}
        ///// <summary>
        ///// Issue a Rotation + Translation action request that falls back on the state of current settings.
        ///// </summary>
        ///// <param name="worldRot"></param>
        ///// <param name="rot"></param>
        ///// <param name="relRot"></param>
        ///// <param name="worldTrans"></param>
        ///// <param name="trans"></param>
        ///// <param name="relTrans"></param>
        ///// <param name="speed"></param>
        ///// <param name="zone"></param>
        ///// <returns></returns>
        //public bool IssueRotationAndTranslationRequest(
        //    bool worldRot, Rotation rot, bool relRot,
        //    bool worldTrans, Point trans, bool relTrans,
        //    int speed, int zone)
        //{
        //    return IssueRotationAndTranslationRequest(
        //        worldRot, rot, relRot,
        //        worldTrans, trans, relTrans,
        //        speed, zone, currentSettings.MotionType);
        //}
        ///// <summary>
        ///// Issue a Rotation + Translation action request that falls back on the state of current settings.
        ///// </summary>
        ///// <param name="worldRot"></param>
        ///// <param name="rot"></param>
        ///// <param name="relRot"></param>
        ///// <param name="worldTrans"></param>
        ///// <param name="trans"></param>
        ///// <param name="relTrans"></param>
        ///// <param name="mType"></param>
        ///// <returns></returns>
        //public bool IssueRotationAndTranslationRequest(
        //    bool worldRot, Rotation rot, bool relRot,
        //    bool worldTrans, Point trans, bool relTrans,
        //    MotionType mType)
        //{
        //    return IssueRotationAndTranslationRequest(
        //        worldRot, rot, relRot,
        //        worldTrans, trans, relTrans,
        //        currentSettings.Speed, currentSettings.Zone, mType);
        //}



        /// <summary>
        /// Issue a request to set the values of joint angles in configuration space. 
        /// </summary>
        /// <param name="joints"></param>
        /// <param name="relJnts"></param>
        /// <param name="speed"></param>
        /// <param name="zone"></param>
        /// <returns></returns>
        public bool IssueJointsRequest(Joints joints, bool relJnts)
        {
            if (!areCursorsInitialized)
            {
                //if (controlMode == ControlMode.Offline)
                //{
                //    if (relJnts)
                //    {
                //        Console.WriteLine("Sorry, first Joints action upon offline robot initialization must be in absolute values");
                //        return false;
                //    }

                //    if (!InitializeRobotCursors(new Point(), Frame.DefaultOrientation))  // @TODO: defaults should depend on robot make/model
                //    {
                //        Console.WriteLine("Could not initialize cursors...");
                //        return false;
                //    }
                //}
                //else
                //{
                    Console.WriteLine("ERROR: cursors not initialized. Did you .Connect()?");
                    return false;
                //}
            }

            ActionJoints act = new ActionJoints(joints, relJnts);
            
            bool success = virtualCursor.Issue(act);
            if (controlMode == ControlMode.Stream) comm.TickStreamQueue(true);
            return success;

        }
        ///// <summary>
        ///// Issue a request to set the values of joint angles in configuration space. 
        ///// </summary>
        ///// <param name="joints"></param>
        ///// <param name="relJnts"></param>
        ///// <returns></returns>
        //public bool IssueJointsRequest(Joints joints, bool relJnts)
        //{
        //    return IssueJointsRequest(joints, relJnts, currentSettings.Speed, currentSettings.Zone);
        //}




        /// <summary>
        /// Issue a request to display a string message on the device.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool IssueMessageRequest(string message)
        {
            if (!areCursorsInitialized)
            {
                //if (controlMode == ControlMode.Offline)
                //{
                //    if (!InitializeRobotCursors(new Point(), Frame.DefaultOrientation))  // @TODO: defaults should depend on robot make/model
                //    {
                //        Console.WriteLine("Could not initialize cursors...");
                //        return false;
                //    }
                //}
                //else
                //{
                    Console.WriteLine("ERROR: cursors not initialized. Did you .Connect()?");
                    return false;
                //}
            }

            ActionMessage act = new ActionMessage(message);

            bool success = virtualCursor.Issue(act);
            if (controlMode == ControlMode.Stream) comm.TickStreamQueue(true);
            return success;

        }



        /// <summary>
        /// Issue a request for the device to stay idle for a certain amount of time.
        /// </summary>
        /// <param name="millis"></param>
        /// <returns></returns>
        public bool IssueWaitRequest(long millis)
        {
            if (!areCursorsInitialized)
            {
                //if (controlMode == ControlMode.Offline)
                //{
                //    if (!InitializeRobotCursors(new Point(), Frame.DefaultOrientation))  // @TODO: defaults should depend on robot make/model
                //    {
                //        Console.WriteLine("Could not initialize cursors...");
                //        return false;
                //    }
                //}
                //else
                //{
                    Console.WriteLine("ERROR: cursors not initialized. Did you .Connect()?");
                    return false;
                //}
            }

            ActionWait act = new ActionWait(millis);
            
            bool success = virtualCursor.Issue(act);
            if (controlMode == ControlMode.Stream) comm.TickStreamQueue(true);
            return success;

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
            //comm.LinkStreamQueue(streamQueue);
            if (controlMode == ControlMode.Stream)
            {
                comm.LinkWriteCursor(ref writeCursor);
            }

            return true;
        }

        /// <summary>
        /// Disconnects and resets the Communication object.
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
        /// Initializes all instances of robotCursors with base information
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="joints"></param>
        /// <returns></returns>
        internal bool InitializeRobotCursors(Point position = null, Rotation rotation = null, Joints joints = null,
            int speed = Control.DEFAULT_SPEED, int zone = Control.DEFAULT_ZONE,
            MotionType mType = Control.DEFAULT_MOTION_TYPE, ReferenceCS refCS = Control.DEFAULT_REFCS)

        {
            bool success = true;
            success = success && virtualCursor.Initialize(position, rotation, joints, speed, zone, mType, refCS);
            success = success && writeCursor.Initialize(position, rotation, joints, speed, zone, mType, refCS);
            success = success && motionCursor.Initialize(position, rotation, joints, speed, zone, mType, refCS);

            areCursorsInitialized = success;

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

        /// <summary>
        /// Triggers a thread to send instructions to the connected device if applicable. 
        /// </summary>
        public void TickWriteCursor()
        {
            if (controlMode == ControlMode.Execute)
            {
                if (!comm.IsRunning() && areCursorsInitialized && writeCursor.AreActionsPending() && (actionsExecuter == null || !actionsExecuter.IsAlive))
                {
                    actionsExecuter = new Thread(() => RunActionsBlockInController());  // http://stackoverflow.com/a/3360582
                    actionsExecuter.Start();
                }
            }
            //else if (controlMode == ControlMode.Stream)
            //{
            //    comm.TickStreamQueue(true);
            //}
            else
            {
                Console.WriteLine("Nothing to tick here");
            }
        }

        /// <summary>
        /// Cretes a program with the first block of Actions in the cursor, uploads it to the controller
        /// and runs it. 
        /// </summary>
        private void RunActionsBlockInController()
        {
            List<string> program = writeCursor.ProgramFromBlock();
            comm.LoadProgramToController(program);
            comm.StartProgramExecution();
        }

























        ////██╗    ██╗██╗██████╗ 
        ////██║    ██║██║██╔══██╗
        ////██║ █╗ ██║██║██████╔╝
        ////██║███╗██║██║██╔═══╝ 
        ////╚███╔███╔╝██║██║     
        //// ╚══╝╚══╝ ╚═╝╚═╝     

        ///// <summary>
        ///// Adds a path to the queue manager and tick it for execution.
        ///// </summary>
        ///// <param name="path"></param>
        //public void AddPathToQueue(Path path)
        //{
        //    queue.Add(path);
        //    TriggerQueue();
        //}

        ///// <summary>
        ///// Checks the state of the execution of the robot, and if stopped and if elements 
        ///// remaining on the queue, starts executing them.
        ///// </summary>
        //public void TriggerQueue()
        //{
        //    if (!comm.IsRunning() && queue.ArePathsPending() && (pathExecuter == null || !pathExecuter.IsAlive))
        //    {
        //        Path path = queue.GetNext();
        //        // RunPath(path);

        //        // https://msdn.microsoft.com/en-us/library/aa645740(v=vs.71).aspx
        //        // Thread oThread = new Thread(new ThreadStart(oAlpha.Beta));
        //        // http://stackoverflow.com/a/3360582
        //        // Thread thread = new Thread(() => download(filename));

        //        // This needs to be much better handled, and the trigger queue should not trigger if a thread is running... 
        //        //Thread runPathThread = new Thread(() => RunPath(path));  // not working for some reason...
        //        //runPathThread.Start();

        //        pathExecuter = new Thread(() => RunPath(path));  // http://stackoverflow.com/a/3360582
        //        pathExecuter.Start();
        //    }
        //}

        ///// <summary>
        ///// Generates a module from a path, loads it to the controller and runs it.
        ///// It assumes the robot is stopped (does this even matter anyway...?)
        ///// </summary>
        ///// <param name="path"></param>
        //public void RunPath(Path path)
        //{
        //    Console.WriteLine("RUNNING NEW PATH: " + path.Count);
        //    List<string> module = Compiler.UNSAFEModuleFromPath(path, currentSettings.Speed, currentSettings.Zone);

        //    comm.LoadProgramToController(module);
        //    comm.StartProgramExecution();
        //}

        ///// <summary>
        ///// Remove all pending elements from the queue.
        ///// </summary>
        //public void ClearQueue()
        //{
        //    queue.EmptyQueue();
        //}

        ///// <summary>
        ///// Adds a Frame to the streaming queue
        ///// </summary>
        ///// <param name="frame"></param>
        //public void AddFrameToStreamQueue(Frame frame)
        //{
        //    streamQueue.Add(frame);
        //}

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
            Console.WriteLine("VIRTUAL BUFFER:");
            virtualCursor.LogBufferedActions();

            Console.WriteLine("WRITE BUFFER:");
            writeCursor.LogBufferedActions();
        }

        public void DebugRobotCursors()
        {
            if (virtualCursor == null)
                Console.WriteLine("Virtual cursor not initialized");
            else
                Console.WriteLine(virtualCursor);

            if (writeCursor == null)
                Console.WriteLine("Write cursor not initialized");
            else
                Console.WriteLine(writeCursor);

            if (motionCursor == null)
                Console.WriteLine("Motion cursor not initialized");
            else
                Console.WriteLine(writeCursor);
        }

        //public void DebugSettingsBuffer()
        //{
        //    settingsBuffer.LogBuffer();
        //    Console.WriteLine("Current settings: " + currentSettings);
        //}

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
