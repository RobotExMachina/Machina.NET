using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

using Machina.Drivers;
using Machina.Controllers;

namespace Machina
{
    //   ██████╗ ██████╗ ███╗   ██╗████████╗██████╗  ██████╗ ██╗     
    //  ██╔════╝██╔═══██╗████╗  ██║╚══██╔══╝██╔══██╗██╔═══██╗██║     
    //  ██║     ██║   ██║██╔██╗ ██║   ██║   ██████╔╝██║   ██║██║     
    //  ██║     ██║   ██║██║╚██╗██║   ██║   ██╔══██╗██║   ██║██║     
    //  ╚██████╗╚██████╔╝██║ ╚████║   ██║   ██║  ██║╚██████╔╝███████╗
    //   ╚═════╝ ╚═════╝ ╚═╝  ╚═══╝   ╚═╝   ╚═╝  ╚═╝ ╚═════╝ ╚══════╝
    //                                                               

    /// <summary>
    /// The core class that centralizes all private control.
    /// </summary>
    class Control
    {
        // Some 'environment variables' to define check states and behavior
        public const bool SAFETY_STOP_IMMEDIATE_ON_DISCONNECT = true;         // when disconnecting from a controller, issue an immediate Stop request?
        //public const bool SAFETY_CHECK_TABLE_COLLISION = true;                // when issuing actions, check if it is about to hit the table?
        //public const bool SAFETY_STOP_ON_TABLE_COLLISION = true;              // prevent from actually hitting the table?
        //public const double SAFETY_TABLE_Z_LIMIT = -10000;                    // table security checks will trigger below this z height (mm)

        // @TODO: move to cursors, make it device specific
        public const double DEFAULT_SPEED = 20;                               // default speed for new actions
        public const double DEFAULT_ACCELERATION = 0;                         // default acc for new actions in mm/s^2; zero values let the controller figure out accelerations
        public const double DEFAULT_ROTATION_SPEED = 0;                       // default rotation speed for new actions in deg/s; under zero values let the controller figure out defaults
        public const double DEFAULT_JOINT_SPEED = 0;
        public const double DEFAULT_JOINT_ACCELERATION = 0;
        public const double DEFAULT_PRECISION = 5;                            // default precision for new actions

        public const MotionType DEFAULT_MOTION_TYPE = MotionType.Linear;      // default motion type for new actions
        public const ReferenceCS DEFAULT_REFCS = ReferenceCS.World;           // default reference coordinate system for relative transform actions
        public const ControlType DEFAULT_CONTROLMODE = ControlType.Offline;
        public const CycleType DEFAULT_RUNMODE = CycleType.Once;
        public const ConnectionType DEFAULT_CONNECTIONMODE = ConnectionType.User;



        /// <summary>
        /// Operation modes by default
        /// </summary>
        internal ControlType _controlMode;
        public ControlType ControlMode { get { return _controlMode; } internal set { _controlMode = value; } }
        internal ControlManager _controlManager;


        internal CycleType runMode = DEFAULT_RUNMODE;
        internal ConnectionType connectionMode;


        /// <summary>
        /// A reference to the Robot object this class is driving.
        /// </summary>
        internal Robot parentRobot;

        /// <summary>
        /// Instances of the main robot Controller and Task
        /// </summary>
        private Driver _driver;
        internal Driver Driver { get { return _driver; } set { _driver = value; } }


        /// <summary>
        /// A virtual representation of the state of the device after application of issued actions.
        /// </summary>
        internal RobotCursor virtualCursor;

        /// <summary>
        /// A virtual representation of the state of the device after releasing pending actions to the controller.
        /// Keeps track of the state of a virtual robot immediately following all the actions released from the 
        /// actionsbuffer to target device defined by controlMode, like an offline program, a full intruction execution 
        /// or a streamed target.
        /// </summary>
        internal RobotCursor writeCursor;

        /// <summary>
        /// A virtual representation of the current motion state of the device.
        /// </summary>
        internal RobotCursor motionCursor;

        /// <summary>
        /// Are cursors ready to start working?
        /// </summary>
        private bool _areCursorsInitialized = false;

        /// <summary>
        /// An mutable alias for the cursor that will be used to return the current state for the robot,
        /// aka which cursor to use for sync GetJoints(), GetPose()-kind of functions...
        /// Mainly the virtualCursor for Offline modes, motionCursor for Stream, etc.
        /// </summary>
        internal RobotCursor stateCursor;

        /// <summary>
        /// A shared instance of a Thread to manage sending and executing actions
        /// in the controller, which typically takes a lot of resources
        /// and halts program execution
        /// </summary>
        private Thread actionsExecuter;


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
            parentRobot = parentBot;

            // Reset();

            motionCursor = new RobotCursor(this, "motionCursor", false, null);
            writeCursor = new RobotCursor(this, "writeCursor", false, motionCursor);
            virtualCursor = new RobotCursor(this, "virtualCursor", true, writeCursor);

            SetControlMode(DEFAULT_CONTROLMODE);
            SetConnectionMode(DEFAULT_CONNECTIONMODE);
        }

        ///// <summary>
        ///// Resets all internal state properties to default values. To be invoked upon
        ///// an internal robot reset.
        ///// @TODO rethink this
        ///// </summary>
        //public void Reset()
        //{
        //    virtualCursor = new RobotCursor(this, "virtualCursor", true);
        //    writeCursor = new RobotCursor(this, "writeCursor", false);
        //    virtualCursor.SetChild(writeCursor);
        //    motionCursor = new RobotCursor(this, "motionCursor", false);
        //    writeCursor.SetChild(motionCursor);
        //    areCursorsInitialized = false;

        //    SetControlMode(DEFAULT_CONTROLMODE);

        //    //currentSettings = new Settings(DEFAULT_SPEED, DEFAULT_ZONE, DEFAULT_MOTION_TYPE, DEFAULT_REFCS);
        //    //settingsBuffer = new SettingsBuffer();
        //}

        /// <summary>
        /// Sets current Control Mode and establishes communication if applicable.
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public bool SetControlMode(ControlType mode)
        {
            if (mode == ControlType.Execute)
            {
                Console.WriteLine("Execute mode temporarily deactivated. Try 'stream' instead, it's cooler ;)");
                Console.WriteLine($"ControlMode reverted to {_controlMode}");
                return false;
            }

            _controlMode = mode;

            return ResetControl();
        }

        /// <summary>
        /// Resets control parameters using the appropriate ControlManager.
        /// </summary>
        /// <returns></returns>
        private bool ResetControl()
        {
            _controlManager = ControlFactory.GetControlManager(this);

            bool success = _controlManager.Initialize();

            if (ControlMode == ControlType.Offline)
            {
                InitializeRobotCursors();
            }

            if (!success)
                throw new Exception("Couldn't SetControlMode()");

            return success;
        }



        //private bool InitializeMode(ControlType mode)
        //{
        //    switch (mode) {
        //        case ControlType.Stream:
        //            return InitializeCommunication();
        //            break;

        //        case ControlType.Execute:
        //            // @TODO
        //            return false;
        //            break;

        //        // Offline
        //        default:
        //            if (comm != null) DropCommunication();
        //            // In offline modes, initialize the robot to a bogus standard transform
        //            return InitializeRobotCursors(new Vector(), Rotation.FlippedAroundY);  // @TODO: defaults should depend on robot make/model
        //            break;
        //    }
        //}


        ///// <summary>
        ///// Returns current Control Mode.
        ///// </summary>
        ///// <returns></returns>
        //public ControlType GetControlMode()
        //{
        //    return _controlMode;
        //}

        ///// <summary>
        ///// Sets current RunMode. 
        ///// </summary>
        ///// <param name="mode"></param>
        ///// <returns></returns>
        //public bool SetRunMode(CycleType mode)
        //{
        //    runMode = mode;

        //    if (controlMode == ControlType.Offline)
        //    {
        //        Console.WriteLine($"Remember RunMode.{mode} will have no effect in Offline mode");
        //    }
        //    else
        //    {
        //        return comm.SetRunMode(mode);
        //    }

        //    return false;
        //}

        ///// <summary>
        ///// Returns current RunMode.
        ///// </summary>
        ///// <param name="mode"></param>
        ///// <returns></returns>
        //public CycleType GetRunMode(CycleType mode)
        //{
        //    return runMode;
        //}


        internal bool ConfigureBuffer(int minActions, int maxActions)
        {
            return this._driver.ConfigureBuffer(minActions, maxActions);
        }


        /// <summary>
        /// Sets the current ConnectionManagerType.
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public bool SetConnectionMode(ConnectionType mode)
        {
            if (_driver == null)
            {
                throw new Exception("Missing Driver object");
            }

            if (!_driver.AvailableConnectionTypes[mode])
            {
                Console.WriteLine($"WARNING: this device's driver does not accept ConnectionType {mode}");
                Console.WriteLine($"ConnectionMode remains {this.connectionMode}");
                return false;
            }

            this.connectionMode = mode;

            return ResetControl();
        }



        /// <summary>
        /// Searches the network for a robot controller and establishes a connection with the specified one by position. 
        /// Necessary for "online" modes.
        /// </summary>
        /// <returns></returns>
        public bool ConnectToDevice(int robotId)
        {
            if (connectionMode == ConnectionType.User)
            {
                throw new Exception("Cannot search for robots automatically, please use ConnectToDevice(ip, port) instead");
            }

            // Sanity
            if (!_driver.ConnectToDevice(robotId))
            {
                throw new Exception("Cannot connect to device");
            }
            else
            {
                //SetRunMode(runMode);

                //// If successful, initialize robot cursors to mirror the state of the device
                //Vector currPos = _comm.GetCurrentPosition();
                //Rotation currRot = _comm.GetCurrentOrientation();
                //Joints currJnts = _comm.GetCurrentJoints();
                //InitializeRobotCursors(currPos, currRot, currJnts);

                // If successful, initialize robot cursors to mirror the state of the device.
                // The function will initialize them based on the _comm object.
                InitializeRobotCursors();
            }

            return true;
        }

        public bool ConnectToDevice(string ip, int port)
        {
            if (connectionMode == ConnectionType.Machina)
            {
                throw new Exception("Try ConnectToDevice() instead");
            }

            // Sanity
            if (!_driver.ConnectToDevice(ip, port))
            {
                throw new Exception("Cannot connect to device");
            }
            else
            {
                InitializeRobotCursors();
            }
            return true;
        }



        /// <summary>
        /// Requests the Communication object to disconnect from controller and reset.
        /// </summary>
        /// <returns></returns>
        public bool DisconnectFromDevice()
        {
            return _driver.DisconnectFromDevice();
        }

        /// <summary>
        /// Is this robot connected to a real/virtual device?
        /// </summary>
        /// <returns></returns>
        public bool IsConnectedToDevice()
        {
            return _driver.Connected;
        }

        /// <summary>
        /// Sets the creddentials for logging into the controller.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool SetUserCredentials(string name, string password) =>
            Driver == null ? false : Driver.SetUser(name, password);

        /// <summary>
        /// If connected to a device, return the IP address
        /// </summary>
        /// <returns></returns>
        public string GetControllerIP() => _driver.IP;

        ///// <summary>
        ///// Loads a programm to the connected device and executes it. 
        ///// </summary>
        ///// <param name="programLines">A string list representation of the program's code.</param>
        ///// <param name="programName">Program name</param>
        ///// <returns></returns>
        //public bool LoadProgramToDevice(List<string> programLines, string programName = "Program")
        //{
        //    return comm.LoadProgramToController(programLines, programName);
        //}

        ///// <summary>
        ///// Loads a programm to the connected device and executes it. 
        ///// </summary>
        ///// <param name="filepath">Full filepath including root, directory structure, filename and extension.</param>
        ///// <param name="wipeout">Delete all previous modules in the device?</param>
        ///// <returns></returns>
        //public bool LoadProgramToDevice(string filepath, bool wipeout)
        //{
        //    if (controlMode == ControlType.Offline)
        //    {
        //        Console.WriteLine("Cannot load modules in Offline mode");
        //        return false;
        //    }

        //    // Sanity
        //    string fullPath = "";

        //    // Is the filepath a valid Windows path?
        //    try
        //    {
        //        fullPath = System.IO.Path.GetFullPath(filepath);
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine("'{0}' is not a valid filepath", filepath);
        //        Console.WriteLine(e);
        //        return false;
        //    }

        //    // Is it an absolute path?
        //    try
        //    {
        //        bool absolute = System.IO.Path.IsPathRooted(fullPath);
        //        if (!absolute)
        //        {
        //            Console.WriteLine("Relative paths are currently not supported");
        //            return false;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine("'{0}' is not a valid absolute filepath", filepath);
        //        Console.WriteLine(e);
        //        return false;
        //    }

        //    //// Split the full path into directory, file and extension names
        //    //string dirname;     // full directory path
        //    //string filename;    // filename without extension
        //    //string extension;   // file extension

        //    //string[] parts = fullPath.Split('\\');
        //    //int len = parts.Length;
        //    //if (len < 2)
        //    //{
        //    //    Console.WriteLine("Weird filepath");
        //    //    return false;
        //    //}
        //    //dirname = string.Join("\\", parts, 0, len - 1);
        //    //string[] fileparts = parts[len - 1].Split('.');
        //    //filename = fileparts.Length > 2 ? string.Join(".", fileparts, 0, fileparts.Length - 1) : fileparts[0];  // account for filenames with multiple dots
        //    //extension = fileparts[fileparts.Length - 1];

        //    //Console.WriteLine("  filename: " + filename);
        //    //Console.WriteLine("  dirname: " + dirname);
        //    //Console.WriteLine("  extension: " + extension);

        //    //return comm.LoadFileToController(dirname, filename, extension, true);
        //    return comm.LoadFileToDevice(fullPath, wipeout);
        //}

        ///// <summary>
        ///// Triggers program start on device.
        ///// </summary>
        ///// <returns></returns>
        //public bool StartProgramOnDevice()
        //{
        //    if (controlMode == ControlType.Offline)
        //    {
        //        Console.WriteLine("No program to start in Offline mode");
        //        return false;
        //    }

        //    return comm.StartProgramExecution();
        //}

        ///// <summary>
        ///// Stops execution of running program on device.
        ///// </summary>
        ///// <param name="immediate"></param>
        ///// <returns></returns>
        //public bool StopProgramOnDevice(bool immediate)
        //{
        //    if (controlMode == ControlType.Offline)
        //    {
        //        Console.WriteLine("No program to stop in Offline mode");
        //        return false;
        //    }

        //    return comm.StopProgramExecution(immediate);
        //}


        public Vector GetVirtualPosition() => virtualCursor.position;
        public Rotation GetVirtualRotation() => virtualCursor.rotation;
        public Joints GetVirtualAxes() => virtualCursor.joints;
        public Tool GetVirtualTool() => virtualCursor.tool;


        /// <summary>
        /// Returns a Vector object representing the current robot's TCP position.
        /// </summary>
        /// <returns></returns>
        public Vector GetCurrentPosition() => stateCursor.position;

        /// <summary>
        /// Returns a Rotation object representing the current robot's TCP orientation.
        /// </summary>
        /// <returns></returns>
        public Rotation GetCurrentRotation() => stateCursor.rotation;

        /// <summary>
        /// Returns a Joints object representing the rotations of the 6 axes of this robot.
        /// </summary>
        /// <returns></returns>
        public Joints GetCurrentAxes() => stateCursor.joints;

        /// <summary>
        /// Returns a Tool object representing the currently attached tool, null if none.
        /// </summary>
        /// <returns></returns>
        public Tool GetCurrentTool() => stateCursor.tool;






        /// <summary>
        /// For Offline modes, it flushes all pending actions and returns a devide-specific program 
        /// as a stringList representation.
        /// </summary>
        /// <param name="inlineTargets">Write inline targets on action statements, or declare them as independent variables?</param>
        /// <param name="humanComments">If true, a human-readable description will be added to each line of code</param>
        /// <returns></returns>
        public List<string> Export(bool inlineTargets, bool humanComments)
        {
            if (_controlMode != ControlType.Offline)
            {
                Console.WriteLine("Export() only works in Offline mode");
                return null;
            }

            //List<Action> actions = actionBuffer.GetAllPending();
            //return programGenerator.UNSAFEProgramFromActions("BRobotProgram", writeCursor, actions);

            return writeCursor.ProgramFromBuffer(inlineTargets, humanComments);
        }

        /// <summary>
        /// For Offline modes, it flushes all pending actions and exports them to a robot-specific program as a text file.
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="inlineTargets">Write inline targets on action statements, or declare them as independent variables?</param>
        /// <param name="humanComments">If true, a human-readable description will be added to each line of code</param>
        /// <returns></returns>
        public bool Export(string filepath, bool inlineTargets, bool humanComments)
        {
            // @TODO: add some filepath sanity here

            List<string> programCode = Export(inlineTargets, humanComments);
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
            //if (_controlMode != ControlType.Execute)
            //{
            //    Console.WriteLine("Execute() only works in Execute mode");
            //    return;
            //}

            //writeCursor.QueueActions();
            //TickWriteCursor();

            throw new NotImplementedException();
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
        public double GetCurrentSpeedSetting()
        {
            // @TODO: will need to decide if this returns the current virtual, write or motion speed
            return virtualCursor.speed;
        }

        /// <summary>
        /// Gets current precision setting.
        /// </summary>
        /// <returns></returns>
        public double GetCurrentPrecisionSettings()
        {
            return virtualCursor.precision;
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



        // TODO: take another look at this, it was quick and dirty...
        public bool SetIOName(string ioName, int pinNumber, bool isDigital)
        {

            if (isDigital)
            {
                if (pinNumber < 0 || pinNumber >= virtualCursor.digitalOutputs.Length)
                {
                    Console.WriteLine("ERROR: pin # out of range");
                    return false;
                }
                else
                {
                    virtualCursor.digitalOutputNames[pinNumber] = ioName;
                    writeCursor.digitalOutputNames[pinNumber] = ioName;
                }
                return true;
            }
            else
            {
                if (pinNumber < 0 || pinNumber >= virtualCursor.analogOutputs.Length)
                {
                    Console.WriteLine("ERROR: pin # out of range");
                    return false;
                }
                else
                {
                    virtualCursor.analogOutputNames[pinNumber] = ioName;
                    writeCursor.analogOutputNames[pinNumber] = ioName;
                }
                return true;
            }
        }










        //   █████╗  ██████╗████████╗██╗ ██████╗ ███╗   ██╗███████╗
        //  ██╔══██╗██╔════╝╚══██╔══╝██║██╔═══██╗████╗  ██║██╔════╝
        //  ███████║██║        ██║   ██║██║   ██║██╔██╗ ██║███████╗
        //  ██╔══██║██║        ██║   ██║██║   ██║██║╚██╗██║╚════██║
        //  ██║  ██║╚██████╗   ██║   ██║╚██████╔╝██║ ╚████║███████║
        //  ╚═╝  ╚═╝ ╚═════╝   ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝╚══════╝
        //                                                         

        /// <summary>
        /// Issue an Action of whatever kind...
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool IssueApplyActionRequest(Action action)
        {
            if (!_areCursorsInitialized)
            {
                Console.WriteLine("ERROR: cursors not initialized. Did you .Connect()?");
                return false;
            }

            bool success = virtualCursor.Issue(action);
            //if (_controlMode == ControlType.Stream) _comm.TickStreamQueue(true);
            return success;
        }





        public bool IssueSpeedRequest(double speed, bool relative) => IssueApplyActionRequest(new ActionSpeed(speed, relative));

        public bool IssueAccelerationRequest(double acc, bool relative) => IssueApplyActionRequest(new ActionAcceleration(acc, relative));

        public bool IssueRotationSpeedRequest(double rotSpeed, bool rel) => IssueApplyActionRequest(new ActionRotationSpeed(rotSpeed, rel));

        public bool IssueJointSpeedRequest(double jointSpeed, bool relative) => IssueApplyActionRequest(new ActionJointSpeed(jointSpeed, relative));

        public bool IssueJointAccelerationRequest(double jointAcceleration, bool relative) => IssueApplyActionRequest(new ActionJointAcceleration(jointAcceleration, relative));

        public bool IssuePrecisionRequest(double precision, bool relative) =>
                IssueApplyActionRequest(new ActionPrecision(precision, relative));


        public bool IssueMotionRequest(MotionType motionType) =>
                IssueApplyActionRequest(new ActionMotion(motionType));


        public bool IssueCoordinatesRequest(ReferenceCS referenceCS) =>
                IssueApplyActionRequest(new ActionCoordinates(referenceCS));


        public bool IssuePushPopRequest(bool push) =>
                IssueApplyActionRequest(new ActionPushPop(push));


        public bool IssueTemperatureRequest(double temp, RobotPartType robotPart, bool waitToReachTemp, bool relative) =>
                IssueApplyActionRequest(new ActionTemperature(temp, robotPart, waitToReachTemp, relative));


        public bool IssueExtrudeRequest(bool extrude) =>
                IssueApplyActionRequest(new ActionExtrusion(extrude));


        public bool IssueExtrusionRateRequest(double rate, bool relative) =>
                IssueApplyActionRequest(new ActionExtrusionRate(rate, relative));

        /// <summary>
        /// Issue a Translation action request that falls back on the state of current settings.
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="relative"></param>
        /// <returns></returns>
        public bool IssueTranslationRequest(Vector trans, bool relative) =>
                IssueApplyActionRequest(new ActionTranslation(trans, relative));


        /// <summary>
        /// Issue a Rotation action request with fully customized parameters.
        /// </summary>
        /// <param name="rot"></param>
        /// <param name="relative"></param>
        /// <returns></returns>
        public bool IssueRotationRequest(Rotation rot, bool relative) =>
                IssueApplyActionRequest(new ActionRotation(rot, relative));


        /// <summary>
        /// Issue a Translation + Rotation action request with fully customized parameters.
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="rot"></param>
        /// <param name="rel"></param>
        /// <param name="translationFirst"></param>
        /// <returns></returns>
        public bool IssueTransformationRequest(Vector trans, Rotation rot, bool rel, bool translationFirst) =>
                IssueApplyActionRequest(new ActionTransformation(trans, rot, rel, translationFirst));


        /// <summary>
        /// Issue a request to set the values of joint angles in configuration space. 
        /// </summary>
        /// <param name="joints"></param>
        /// <param name="relJnts"></param>
        /// <param name="speed"></param>
        /// <param name="zone"></param>
        /// <returns></returns>
        public bool IssueJointsRequest(Joints joints, bool relJnts) =>
                IssueApplyActionRequest(new ActionAxes(joints, relJnts));


        /// <summary>
        /// Issue a request to display a string message on the device.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool IssueMessageRequest(string message) =>
                IssueApplyActionRequest(new ActionMessage(message));


        /// <summary>
        /// Issue a request for the device to stay idle for a certain amount of time.
        /// </summary>
        /// <param name="millis"></param>
        /// <returns></returns>
        public bool IssueWaitRequest(long millis) =>
                IssueApplyActionRequest(new ActionWait(millis));


        /// <summary>
        /// Issue a request to add an internal comment in the compiled code. 
        /// </summary>
        /// <param name="comment"></param>
        /// <returns></returns>
        public bool IssueCommentRequest(string comment) =>
                IssueApplyActionRequest(new ActionComment(comment));


        /// <summary>
        /// Issue a request to attach a Tool to the flange of the robot
        /// </summary>
        /// <param name="tool"></param>
        /// <returns></returns>
        public bool IssueAttachRequest(Tool tool) =>
                IssueApplyActionRequest(new ActionAttach(tool));


        /// <summary>
        /// Issue a request to return the robot to no tools attached. 
        /// </summary>
        /// <returns></returns>
        public bool IssueDetachRequest() =>
                IssueApplyActionRequest(new ActionDetach());


        /// <summary>
        /// Issue a request to turn digital IO on/off.
        /// </summary>
        /// <param name="pinNum"></param>
        /// <param name="isOn"></param>
        /// <returns></returns>
        public bool IssueWriteToDigitalIORequest(int pinNum, bool isOn) =>
                IssueApplyActionRequest(new ActionIODigital(pinNum, isOn));


        /// <summary>
        /// Issue a request to write to analog pin.
        /// </summary>
        /// <param name="pinNum"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool IssueWriteToAnalogIORequest(int pinNum, double value) =>
                IssueApplyActionRequest(new ActionIOAnalog(pinNum, value));


        /// <summary>
        /// Issue a request to add common initialization/termination procedures on the device, 
        /// like homing, calibration, fans, etc.
        /// </summary>
        /// <param name="initiate"></param>
        /// <returns></returns>
        public bool IssueInitializationRequest(bool initiate) =>
                IssueApplyActionRequest(new ActionInitialization(initiate));









        //██████╗ ██████╗ ██╗██╗   ██╗ █████╗ ████████╗███████╗
        //██╔══██╗██╔══██╗██║██║   ██║██╔══██╗╚══██╔══╝██╔════╝
        //██████╔╝██████╔╝██║██║   ██║███████║   ██║   █████╗  
        //██╔═══╝ ██╔══██╗██║╚██╗ ██╔╝██╔══██║   ██║   ██╔══╝  
        //██║     ██║  ██║██║ ╚████╔╝ ██║  ██║   ██║   ███████╗
        //╚═╝     ╚═╝  ╚═╝╚═╝  ╚═══╝  ╚═╝  ╚═╝   ╚═╝   ╚══════╝

        // THIS IS NOW A TASK FOR THE ControlManager.SetCommunicationObject()
        /// <summary>
        /// Initializes the Communication object.
        /// </summary>
        /// <returns></returns>
        //private bool InitializeCommunication()
        //{
        //    Console.WriteLine("InitializeCommunication");

        //    // If there is already some communication going on
        //    if (_driver != null)
        //    {
        //        Console.WriteLine("Communication protocol might be active. Please CloseControllerCommunication() first.");
        //        return false;
        //    }

        //    // @TODO: shim assignment of correct robot model/brand
        //    //_driver = new DriverABB(this);
        //    if (this.parentRobot.Brand == RobotType.ABB)
        //    {
        //        _driver = new DriverABB(this);
        //    }
        //    else if (this.parentRobot.Brand == RobotType.UR)
        //    {
        //        _driver = new DriverUR(this);
        //    }
        //    else
        //    {
        //        throw new NotImplementedException();
        //    }

        //    // Pass the streamQueue object as a shared reference
        //    //comm.LinkStreamQueue(streamQueue);
        //    if (_controlMode == ControlType.Stream)
        //    {
        //        _driver.LinkWriteCursor(ref writeCursor);
        //    }

        //    return true;
        //}

        /// <summary>
        /// Disconnects and resets the Communication object.
        /// </summary>
        /// <returns></returns>
        private bool DropCommunication()
        {
            if (_driver == null)
            {
                Console.WriteLine("Communication protocol not established.");
                return false;
            }
            bool success = _driver.DisconnectFromDevice();
            _driver = null;
            return success;
        }

        ///// <summary>
        ///// If there was a running Communication protocol, drop it and restart it again.
        ///// </summary>
        ///// <returns></returns>
        //private bool ResetCommunication()
        //{
        //    if (_driver == null)
        //    {
        //        Console.WriteLine("Communication protocol not established, please initialize first.");
        //    }
        //    DropCommunication();
        //    return InitializeCommunication();
        //}

        /// <summary>
        /// Initializes all instances of robotCursors with base information
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="joints"></param>
        /// <returns></returns>
        internal bool InitializeRobotCursors(Point position = null, Rotation rotation = null, Joints joints = null,
            double speed = Control.DEFAULT_SPEED, double acc = Control.DEFAULT_ACCELERATION, double rotationSpeed = Control.DEFAULT_ROTATION_SPEED,
            double jointSpeed = Control.DEFAULT_JOINT_SPEED, double jointAcceleration = Control.DEFAULT_JOINT_ACCELERATION,
            double precision = Control.DEFAULT_PRECISION,
            MotionType mType = Control.DEFAULT_MOTION_TYPE, ReferenceCS refCS = Control.DEFAULT_REFCS)

        {
            bool success = true;
            success &= virtualCursor.Initialize(position, rotation, joints, speed, acc, jointSpeed, jointAcceleration, rotationSpeed, precision, mType, refCS);
            success &= writeCursor.Initialize(position, rotation, joints, speed, acc, jointSpeed, jointAcceleration, rotationSpeed, precision, mType, refCS);
            success &= motionCursor.Initialize(position, rotation, joints, speed, acc, jointSpeed, jointAcceleration, rotationSpeed, precision, mType, refCS);

            _areCursorsInitialized = success;

            return success;
        }


        internal bool InitializeRobotCursors()
        {
            if (_driver == null)
            {
                throw new Exception("Cannot initialize Robotcursors without a _comm object");
            }

            // If successful, initialize robot cursors to mirror the state of the device
            Vector currPos = _driver.GetCurrentPosition();
            Rotation currRot = _driver.GetCurrentOrientation();
            Joints currJnts = _driver.GetCurrentJoints();

            return InitializeRobotCursors(currPos, currRot, currJnts);
        }


        /// <summary>
        /// Saves a string List to a file.
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="filepath"></param>
        /// <returns></returns>
        internal bool SaveStringListToFile(List<string> lines, string filepath)
        {
            try
            {
                System.IO.File.WriteAllLines(filepath, lines, this.parentRobot.Brand == RobotType.HUMAN ? Encoding.UTF8 : Encoding.ASCII);  // human compiler works better at UTF8, but this was ASCII for ABB controllers, right??
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not save program to file...");
                Console.WriteLine(ex);
            }
            return false;
        }






        ///// <summary>
        ///// Triggers a thread to send instructions to the connected device if applicable. 
        ///// </summary>
        //public void TickWriteCursor()
        //{
        //    if (_controlMode == ControlType.Execute)
        //    {
        //        if (!_comm.IsRunning() && _areCursorsInitialized && writeCursor.AreActionsPending() && (actionsExecuter == null || !actionsExecuter.IsAlive))
        //        {
        //            actionsExecuter = new Thread(() => RunActionsBlockInController(true, false));  // http://stackoverflow.com/a/3360582
        //            actionsExecuter.Start();
        //        }
        //    }
        //    //else if (controlMode == ControlMode.Stream)
        //    //{
        //    //    comm.TickStreamQueue(true);
        //    //}
        //    else
        //    {
        //        Console.WriteLine("Nothing to tick here");
        //    }
        //}

        ///// <summary>
        ///// Creates a program with the first block of Actions in the cursor, uploads it to the controller
        ///// and runs it. 
        ///// </summary>
        //private void RunActionsBlockInController(bool inlineTargets, bool humanComments)
        //{
        //    List<string> program = writeCursor.ProgramFromBlock(inlineTargets, humanComments);
        //    _comm.LoadProgramToController(program, "Buffer");
        //    _comm.StartProgramExecution();
        //}








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

        //// This should be moved somewhere else
        //public static bool IsBelowTable(double z)
        //{
        //    return z < SAFETY_TABLE_Z_LIMIT;
        //}









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
            _driver.DebugDump();
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
