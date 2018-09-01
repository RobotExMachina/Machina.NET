using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina
{
    //   ██████╗██╗   ██╗██████╗ ███████╗ ██████╗ ██████╗ 
    //  ██╔════╝██║   ██║██╔══██╗██╔════╝██╔═══██╗██╔══██╗
    //  ██║     ██║   ██║██████╔╝███████╗██║   ██║██████╔╝
    //  ██║     ██║   ██║██╔══██╗╚════██║██║   ██║██╔══██╗
    //  ╚██████╗╚██████╔╝██║  ██║███████║╚██████╔╝██║  ██║
    //   ╚═════╝ ╚═════╝ ╚═╝  ╚═╝╚══════╝ ╚═════╝ ╚═╝  ╚═╝
    //                                                    
    /// <summary>
    /// Represents an abstraction of the state of a robotic device. 
    /// Keeps track of things such as position, orientation, joint configuration,
    /// current speed, zone, etc.
    /// Useful as virtual representation of a simulated or controlled robot actuator. 
    /// </summary>
    internal class RobotCursor
    {
        // Public props
        public string name;
        public Vector position, prevPosition;
        public Rotation rotation, prevRotation;
        public Joints joints, prevJoints;
        public double acceleration;
        public double speed;
        public double rotationSpeed;
        public double jointAcceleration;
        public double jointSpeed;
        public double precision;
        public MotionType motionType;
        public ReferenceCS referenceCS;
        public Tool tool;
        public ExternalAxes externalAxes;

        // Some robots use ints as pin identifiers (UR, KUKA), while others use strings (ABB). 
        // All pin ids are stored as strings, and are parsed to ints internally if possible. 
        Dictionary<string, bool> digitalOutputs = new Dictionary<string, bool>();
        Dictionary<string, double> analogOutputs = new Dictionary<string, double>();


        // 3D printing
        public bool isExtruding;
        public double extrusionRate;
        public Dictionary<RobotPartType, double> partTemperature = new Dictionary<RobotPartType, double>();
        public double extrudedLength, prevExtrudedLength;  // the length of filament that has been extruded, i.e. the "E" parameter

        /// <summary>
        /// Last Action that was applied to this cursor
        /// </summary>
        public Action lastAction = null;
        protected bool initialized = false;
        private bool applyImmediately = false;  // when an action is issued to this cursor, apply it immediately?


        /// <summary>
        /// Who manages this Cursor?
        /// </summary>
        public Control parentControl;

        /// <summary>
        /// A reference to this parent's Robot Logger.
        /// </summary>
        internal RobotLogger logger;

        /// <summary>
        /// Specified RobotCursor instance will be issued all Actions 
        /// released from this one. 
        /// </summary>
        public RobotCursor child;

        /// <summary>
        /// Robot program compilers now belong to the RobotCursor. 
        /// It makes it easier to attach the right device-specific type, 
        /// and to use the cursor's information to generate the program. 
        /// </summary>
        public Compiler compiler;

        /// <summary>
        /// A buffer that stores Push and PopSettings() states.
        /// </summary>
        internal SettingsBuffer settingsBuffer;

        /// <summary>
        /// Manages pending and released Actions, plus blocks. 
        /// </summary>
        public ActionBuffer actionBuffer;

        /// <summary>
        /// A lock for buffer manipulation operations. 
        /// </summary>
        public object actionBufferLock = new object();





        //  ╔╗ ╔═╗╔═╗╔═╗
        //  ╠╩╗╠═╣╚═╗║╣ 
        //  ╚═╝╩ ╩╚═╝╚═╝
        /// <summary>
        /// Main constructor.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="applyImmediately"></param>
        public RobotCursor(Control parentControl, string name,  bool applyImmediately, RobotCursor childCursor)
        {
            this.parentControl = parentControl;
            this.logger = parentControl.Logger;
            this.name = name;
            this.applyImmediately = applyImmediately;
            this.child = childCursor;

            // @TODO: make this programmatic
            if (this.parentControl.parentRobot.Brand == RobotType.HUMAN)
            {
                compiler = new CompilerHuman();
            }
            else if (this.parentControl.parentRobot.Brand == RobotType.MACHINA)
            {
                compiler = new CompilerMACHINA();
            }
            else if (this.parentControl.parentRobot.Brand == RobotType.ABB)
            {
                compiler = new CompilerABB();
            }
            else if (this.parentControl.parentRobot.Brand == RobotType.UR)
            {
                compiler = new CompilerUR();
            }
            else if (this.parentControl.parentRobot.Brand == RobotType.KUKA)
            {
                compiler = new CompilerKUKA();
            } 
            else if (this.parentControl.parentRobot.Brand == RobotType.ZMORPH)
            {
                compiler = new CompilerZMORPH();
            }

            // Initialize buffers
            actionBuffer = new ActionBuffer(this);
            settingsBuffer = new SettingsBuffer();
        }

        /// <summary>
        /// Minimum information necessary to initialize a robot object.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="joints"></param>
        /// <returns></returns>
        public bool Initialize(Vector position, Rotation rotation, Joints joints,
            double speed, double acceleration, double rotationSpeed, double jointSpeed, double jointAcceleration,
            double precision, MotionType mType, ReferenceCS refCS)
        {
            if (position != null)
            {
                this.position = new Vector(position);
                this.prevPosition = new Vector(position);
            }
            if (rotation != null)
            {
                this.rotation = new Rotation(rotation);
                this.prevRotation = new Rotation(rotation);
            }
            if (joints != null)
            {
                this.joints = new Joints(joints);
                this.prevJoints = new Joints(joints);
            }
            this.acceleration = acceleration;
            this.speed = speed;
            this.rotationSpeed = rotationSpeed;
            this.jointSpeed = jointSpeed;
            this.jointAcceleration = jointAcceleration;
            this.precision = precision;
            this.motionType = mType;
            this.referenceCS = refCS;

            // Initialize temps to zero
            foreach (RobotPartType part in Enum.GetValues(typeof(RobotPartType)))
            {
                partTemperature[part] = 0;
            }
            isExtruding = false;
            extrusionRate = 0;
            extrudedLength = 0;

            // Keep this null until initialized
            //this.externalAxes = new ExternalAxes();  // @TODO: should this be passed as an argument?

            this.initialized = true;
            return this.initialized;
        }
        
        
        /// <summary>
        /// Add an action to this cursor's buffer, to be released whenever assigned priority.
        /// </summary>
        /// <param name="action"></param>
        public bool Issue(Action action)
        {
            lock(actionBufferLock)
            {
                actionBuffer.Add(action);
                if (applyImmediately)
                {
                    return ApplyNextAction();
                }
                return true;
            }
            
        }

        /// <summary>
        /// Applies next single action pending in the buffer.
        /// </summary>
        /// <returns></returns>
        public bool ApplyNextAction()
        {
            lock(actionBufferLock)
            {
                lastAction = actionBuffer.GetNext();
                if (lastAction == null) return false;
                bool success = Apply(lastAction);
                if (success && child != null)
                {
                    child.Issue(lastAction);
                }
                return success;
            }
        }

        /// <summary>
        /// Applies next single action pending in the buffer and outs that action.
        /// </summary>
        /// <param name="lastAction"></param>
        /// <returns></returns>
        public bool ApplyNextAction(out Action lastAction)
        {
            lock (actionBufferLock)
            {
                lastAction = actionBuffer.GetNext();
                if (lastAction == null) return false;
                bool success = Apply(lastAction);
                if (success && child != null)
                {
                    child.Issue(lastAction);
                }
                return success;
            }
        }

        /// <summary>
        /// Ascends the pending actions buffer searching for the given id, and applies
        /// them all, inclusive of the one searched. 
        /// This assumes ids are correlative and ascending, will stop if it finds an
        /// id larger thatn the given one. 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool ApplyActionsUntilId(int id)
        {
            lock (actionBufferLock)
            {
                var actions = actionBuffer.GetAllUpToId(id);
                bool success = true;

                foreach (Action a in actions)
                {
                    success &= Apply(a);
                    if (success && child != null)
                    {
                        child.Issue(a);
                        lastAction = a;
                    }
                }

                return success;
            }
        }

        /// <summary>
        /// Returns the last Action that was released by the buffer.
        /// </summary>
        /// <returns></returns>
        public Action GetLastAction()
        {
            lock(actionBufferLock)
            {
                return actionBuffer.GetLast();
            }
        }

        /// <summary>
        /// Requests all un-blocked pending Actions in the buffer to be flagged
        /// as a block. 
        /// </summary>
        public void QueueActions()
        {
            actionBuffer.SetBlock();
        }

        /// <summary>
        /// Are there Actions pending in the buffer?
        /// </summary>
        /// <returns></returns>
        public bool AreActionsPending()
        {
            return actionBuffer.AreActionsPending();
        }

        /// <summary>
        /// Returns the number of actions pending in this cursor's buffer.
        /// </summary>
        /// <returns></returns>
        public int ActionsPendingCount()
        {
            return actionBuffer.ActionsPendingCount();
        }

        /// <summary>
        /// Return a device-specific program with all the Actions pending in the buffer.
        /// </summary>
        /// <param name="inlineTargets">Write inline targets on action statements, or declare them as independent variables?</param>
        /// <param name="humanComments">If true, a human-readable description will be added to each line of code</param>
        /// <returns></returns>
        public List<string> ProgramFromBuffer(bool inlineTargets, bool humanComments)
        {
            return compiler.UNSAFEProgramFromBuffer(Util.SafeProgramName(parentControl.parentRobot.Name) + "_Program", this, false, inlineTargets, humanComments);
        }

        /// <summary>
        /// Return a device-specific program with the next block of Actions pending in the buffer.
        /// </summary>
        /// <param name="inlineTargets">Write inline targets on action statements, or declare them as independent variables?</param>
        /// <param name="humanComments">If true, a human-readable description will be added to each line of code</param>
        /// <returns></returns>
        public List<string> ProgramFromBlock(bool inlineTargets, bool humanComments)
        {
            return compiler.UNSAFEProgramFromBuffer(Util.SafeProgramName(parentControl.parentRobot.Name) + "_Program", this, true, inlineTargets, humanComments);
        }

        public void LogBufferedActions()
        {
            lock(actionBufferLock)
            {
                actionBuffer.DebugBufferedActions();
            }
        }

        /// <summary>
        /// Returns a Settings object representing the current state of this RobotCursor.
        /// </summary>
        /// <returns></returns>
        public Settings GetSettings()
        {
            return new Settings(this.speed, this.acceleration, this.rotationSpeed, this.jointSpeed, this.jointAcceleration, 
                this.precision, this.motionType, this.referenceCS, this.extrusionRate);
        }







        //   █████╗ ██████╗ ██████╗ ██╗  ██╗   ██╗ █████╗  ██████╗████████╗██╗ ██████╗ ███╗   ██╗
        //  ██╔══██╗██╔══██╗██╔══██╗██║  ╚██╗ ██╔╝██╔══██╗██╔════╝╚══██╔══╝██║██╔═══██╗████╗  ██║
        //  ███████║██████╔╝██████╔╝██║   ╚████╔╝ ███████║██║        ██║   ██║██║   ██║██╔██╗ ██║
        //  ██╔══██║██╔═══╝ ██╔═══╝ ██║    ╚██╔╝  ██╔══██║██║        ██║   ██║██║   ██║██║╚██╗██║
        //  ██║  ██║██║     ██║     ███████╗██║   ██║  ██║╚██████╗   ██║   ██║╚██████╔╝██║ ╚████║
        //  ╚═╝  ╚═╝╚═╝     ╚═╝     ╚══════╝╚═╝   ╚═╝  ╚═╝ ╚═════╝   ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝
        //                                                                                       
        /// <summary>
        /// A dict that maps Action types to the cursor's applicable method.
        /// https://chodounsky.net/2014/01/29/dynamic-dispatch-in-c-number/
        /// </summary>
        Dictionary<Type, Func<Machina.Action, RobotCursor, bool>> ActionsMap = new Dictionary<Type, Func<Action, RobotCursor, bool>>()
        {
            { typeof (ActionSpeed),                     (act, robCur) => robCur.ApplyAction((ActionSpeed) act) },
            { typeof (ActionAcceleration),              (act, robCur) => robCur.ApplyAction((ActionAcceleration) act) },
            { typeof (ActionRotationSpeed),             (act, robCur) => robCur.ApplyAction((ActionRotationSpeed) act) },
            { typeof (ActionJointSpeed),                (act, robCur) => robCur.ApplyAction((ActionJointSpeed) act) },
            { typeof (ActionJointAcceleration),         (act, robCur) => robCur.ApplyAction((ActionJointAcceleration) act) },
            { typeof (ActionPrecision),                 (act, robCur) => robCur.ApplyAction((ActionPrecision) act) },
            { typeof (ActionMotion),                    (act, robCur) => robCur.ApplyAction((ActionMotion) act) },
            { typeof (ActionCoordinates),               (act, robCur) => robCur.ApplyAction((ActionCoordinates) act) },
            { typeof (ActionPushPop),                   (act, robCur) => robCur.ApplyAction((ActionPushPop) act) },
            { typeof (ActionTranslation),               (act, robCur) => robCur.ApplyAction((ActionTranslation) act) },
            { typeof (ActionRotation),                  (act, robCur) => robCur.ApplyAction((ActionRotation) act) },
            { typeof (ActionTransformation),            (act, robCur) => robCur.ApplyAction((ActionTransformation) act) },
            { typeof (ActionAxes),                      (act, robCur) => robCur.ApplyAction((ActionAxes) act) },
            { typeof (ActionMessage),                   (act, robCur) => robCur.ApplyAction((ActionMessage) act) },
            { typeof (ActionWait),                      (act, robCur) => robCur.ApplyAction((ActionWait) act) },
            { typeof (ActionComment),                   (act, robCur) => robCur.ApplyAction((ActionComment) act) },
            { typeof (ActionAttach),                    (act, robCur) => robCur.ApplyAction((ActionAttach) act) },
            { typeof (ActionDetach),                    (act, robCur) => robCur.ApplyAction((ActionDetach) act) },
            { typeof (ActionIODigital),                 (act, robCur) => robCur.ApplyAction((ActionIODigital) act) },
            { typeof (ActionIOAnalog),                  (act, robCur) => robCur.ApplyAction((ActionIOAnalog) act) },
            { typeof (ActionTemperature),               (act, robCur) => robCur.ApplyAction((ActionTemperature) act) },
            { typeof (ActionExtrusion),                 (act, robCur) => robCur.ApplyAction((ActionExtrusion) act) },
            { typeof (ActionExtrusionRate),             (act, robCur) => robCur.ApplyAction((ActionExtrusionRate) act) },
            { typeof (ActionInitialization),            (act, robCur) => robCur.ApplyAction((ActionInitialization) act) },
            { typeof (ActionExternalAxis),              (act, robCur) => robCur.ApplyAction((ActionExternalAxis) act) },
            { typeof (ActionCustomCode),                (act, robCur) => robCur.ApplyAction((ActionCustomCode) act) }
        };

        /// <summary>
        /// Applies the directives of an Action to this cursor. 
        /// </summary>
        /// <remarks>
        /// While this Dictionary dispatch pattern is a bit convoluted, it is faster than dynamic casting, 
        /// more stable and allows for compiler-time checks and non-error fallback.
        /// https://chodounsky.net/2014/01/29/dynamic-dispatch-in-c-number/
        /// </remarks>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool Apply(Action action)
        {
            Type t = action.GetType();
            if (ActionsMap.ContainsKey(t))
            {
                return ActionsMap[t](action, this);
            }

            logger.Verbose($"Found no suitable method for Action \"{action}\"");
            return false;
        }


        //  ╔═╗╔═╗╔╦╗╔╦╗╦╔╗╔╔═╗╔═╗  ╔═╗╔═╗╔╦╗╦╔═╗╔╗╔╔═╗
        //  ╚═╗║╣  ║  ║ ║║║║║ ╦╚═╗  ╠═╣║   ║ ║║ ║║║║╚═╗
        //  ╚═╝╚═╝ ╩  ╩ ╩╝╚╝╚═╝╚═╝  ╩ ╩╚═╝ ╩ ╩╚═╝╝╚╝╚═╝
        /// <summary>
        /// Apply Acceleration Action.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool ApplyAction(ActionAcceleration action)
        {
            if (action.relative)
                this.acceleration += action.acceleration;
            else
                this.acceleration = action.acceleration;

            if (this.acceleration < 0) this.acceleration = 0;

            return true;
        }

        /// <summary>
        /// Apply Speed Action.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool ApplyAction(ActionSpeed action)
        {
            if (action.relative)
                this.speed += action.speed;
            else
                this.speed = action.speed;

            if (this.speed < 0) this.speed = 0;

            return true;
        }

        /// <summary>
        /// Apply RotationSpeed Action.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool ApplyAction(ActionRotationSpeed action)
        {
            if (action.relative)
                this.rotationSpeed += action.rotationSpeed;
            else
                this.rotationSpeed = action.rotationSpeed;

            if (this.rotationSpeed < 0) this.rotationSpeed = 0;

            return true;
        }

        /// <summary>
        /// Apply JointSpeed Action.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool ApplyAction(ActionJointSpeed action)
        {
            if (action.relative)
                this.jointSpeed += action.jointSpeed;
            else
                this.jointSpeed = action.jointSpeed;

            if (this.jointSpeed < 0) this.jointSpeed = 0;

            return true;
        }

        /// <summary>
        /// Apply JointAcceleration Action.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool ApplyAction(ActionJointAcceleration action)
        {
            if (action.relative)
                this.jointAcceleration += action.jointAcceleration;
            else
                this.jointAcceleration = action.jointAcceleration;

            if (this.jointAcceleration < 0) this.jointAcceleration = 0;

            return true;
        }

        /// <summary>
        /// Apply Zone Action.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool ApplyAction(ActionPrecision action)
        {
            if (action.relative)
                this.precision += action.precision;
            else
                this.precision = action.precision;

            if (this.precision < 0) precision = 0;

            return true;
        }

        /// <summary>
        /// Apply Motion Action.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool ApplyAction(ActionMotion action)
        {
            this.motionType = action.motionType;

            return true;
        }

        /// <summary>
        /// Apply ReferenceCS Action.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool ApplyAction(ActionCoordinates action)
        {
            this.referenceCS = action.referenceCS;

            return true;
        }

        /// <summary>
        /// Apply a Push or Pop Action.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool ApplyAction(ActionPushPop action)
        {
            if (action.push)
            {
                return this.settingsBuffer.Push(this);
            }
            else
            {
                Settings s = settingsBuffer.Pop(this);
                if (s != null)
                {
                    this.acceleration = s.Acceleration;
                    this.speed = s.Speed;
                    this.rotationSpeed = s.RotationSpeed;
                    this.jointSpeed = s.JointSpeed;
                    this.jointAcceleration = s.JointAcceleration;
                    this.precision = s.Precision;
                    this.motionType = s.MotionType;
                    this.referenceCS = s.RefCS;
                    this.extrusionRate = s.ExtrusionRate;
                    return true;
                }
            }
            return false;
        }



        //  ╔╦╗╔═╗╔╦╗╦╔═╗╔╗╔  ╔═╗╔═╗╔╦╗╦╔═╗╔╗╔╔═╗
        //  ║║║║ ║ ║ ║║ ║║║║  ╠═╣║   ║ ║║ ║║║║╚═╗
        //  ╩ ╩╚═╝ ╩ ╩╚═╝╝╚╝  ╩ ╩╚═╝ ╩ ╩╚═╝╝╚╝╚═╝
        /// <summary>
        /// Apply Translation Action.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool ApplyAction(ActionTranslation action)
        {
            Vector newPosition = new Vector();

            if (action.relative)
            {
                // If user issued a relative action, make sure there are absolute values to work with. (This limitation is due to current lack of FK/IK solvers)
                if (position == null || rotation == null)
                {
                    logger.Info($"Cannot apply \"{action}\", must provide absolute position values first before applying relative ones... ");
                    return false;
                }

                if (referenceCS == ReferenceCS.World)
                {
                    newPosition = position + action.translation;
                }
                else
                {
                    //Vector worldVector = Vector.Rotation(action.translation, Rotation.Conjugate(this.rotation));
                    Vector worldVector = Vector.Rotation(action.translation, this.rotation);
                    newPosition = position + worldVector;
                }
            }
            else
            {
                // Fail if issued abs movement without prior rotation info. (This limitation is due to current lack of FK/IK solvers)
                if (rotation == null)
                {
                    logger.Info($"Cannot apply \"{action}\", currently missing TCP orientation to work with... ");
                    return false;
                }

                newPosition.Set(action.translation);
            }

            // @TODO: this must be more programmatically implemented 
            //if (Control.SAFETY_CHECK_TABLE_COLLISION)
            //{
            //    if (Control.IsBelowTable(newPosition.Z))
            //    {
            //        if (Control.SAFETY_STOP_ON_TABLE_COLLISION)
            //        {
            //            Console.WriteLine("Cannot perform action: too close to base XY plane --> TCP.z = {0}", newPosition.Z);
            //            return false;
            //        }
            //        else
            //        {
            //            Console.WriteLine("WARNING: too close to base XY plane, USE CAUTION! --> TCP.z = {0}", newPosition.Z);
            //        }
            //    }
            //}

            prevPosition = position;
            position = newPosition;

            prevJoints = joints;
            joints = null;      // flag joints as null to avoid Joint instructions using obsolete data

            if (isExtruding) this.ComputeExtrudedLength();

            return true;
        }


        /// <summary>
        /// Apply Rotation Action.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool ApplyAction(ActionRotation action)
        {
            Rotation newRot;

            // @TODO: implement some kind of security check here...
            if (action.relative)
            {
                // If user issued a relative action, make sure there are absolute values to work with (this limitation is due to current lack of FK/IK solvers).
                if (position == null || rotation == null)
                {
                    logger.Info($"Cannot apply \"{action}\", must provide absolute rotation values first before applying relative ones... ");
                    return false;
                }

                prevRotation = rotation;
                if (referenceCS == ReferenceCS.World)
                {
                    //rotation.RotateGlobal(action.rotation);
                    newRot = Rotation.Global(rotation, action.rotation);  // @TODO: TEST THIS
                }
                else
                {
                    //rotation.RotateLocal(action.rotation);
                    newRot = Rotation.Local(rotation, action.rotation);  // @TODO: TEST THIS
                }
            }
            else
            {
                // Fail if issued abs rotation without prior position info (this limitation is due to current lack of FK/IK solvers).
                if (position == null)
                {
                    logger.Info($"Cannot apply \"{action}\", currently missing TCP position to work with... ");
                    return false;
                }

                newRot = new Rotation(action.rotation);
            }

            prevRotation = rotation;
            rotation = newRot;

            prevJoints = joints;
            joints = null;      // flag joints as null to avoid Joint instructions using obsolete data

            if (isExtruding) this.ComputeExtrudedLength();

            return true;
        }


        /// <summary>
        /// Apply Transformation Action.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool ApplyAction(ActionTransformation action)
        {
            Vector newPos;
            Rotation newRot;

            // Relative transform
            if (action.relative)
            {
                // If user issued a relative action, make sure there are absolute values to work with. (This limitation is due to current lack of FK/IK solvers)
                if (position == null || rotation == null)
                {
                    logger.Info($"Cannot apply \"{action}\", must provide absolute transform values first before applying relative ones...");
                    return false;
                }

                // This is Translate + Rotate
                if (action.translationFirst)
                {
                    if (referenceCS == ReferenceCS.World)
                    {
                        newPos = position + action.translation;
                        newRot = Rotation.Combine(action.rotation, rotation);  // premultiplication
                    }
                    else
                    {
                        //Vector worldVector = Vector.Rotation(action.translation, Rotation.Conjugate(this.rotation));
                        Vector worldVector = Vector.Rotation(action.translation, this.rotation);
                        newPos = position + worldVector;
                        newRot = Rotation.Combine(rotation, action.rotation);  // postmultiplication
                    }
                }

                // or Rotate + Translate
                else
                {
                    if (referenceCS == ReferenceCS.World)
                    {
                        newPos = position + action.translation;
                        newRot = Rotation.Combine(action.rotation, rotation);  // premultiplication
                    }

                    else
                    {
                        // @TOCHECK: is this correct?
                        newRot = Rotation.Combine(rotation, action.rotation);  // postmultiplication
                        //Vector worldVector = Vector.Rotation(action.translation, Rotation.Conjugate(newRot));
                        Vector worldVector = Vector.Rotation(action.translation, newRot);
                        newPos = position + worldVector;
                    }

                }

            }

            // Absolute transform
            else
            {
                newPos = new Vector(action.translation);
                newRot = new Rotation(action.rotation);
            }

            //// @TODO: this must be more programmatically implemented 
            //if (Control.SAFETY_CHECK_TABLE_COLLISION)
            //{
            //    if (Control.IsBelowTable(newPos.Z))
            //    {
            //        if (Control.SAFETY_STOP_ON_TABLE_COLLISION)
            //        {
            //            Console.WriteLine("Cannot perform action: too close to base XY plane --> TCP.z = {0}", newPos.Z);
            //            return false;
            //        }
            //        else
            //        {
            //            Console.WriteLine("WARNING: too close to base XY plane, USE CAUTION! --> TCP.z = {0}", newPos.Z);
            //        }
            //    }
            //}

            prevPosition = position;
            position = newPos;
            prevRotation = rotation;
            rotation = newRot;

            prevJoints = joints;
            joints = null;  // flag joints as null to avoid Joint instructions using obsolete data

            if (isExtruding) this.ComputeExtrudedLength();

            return true;
        }

        /// <summary>
        /// Apply Joints Action.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool ApplyAction(ActionAxes action)
        {
            Joints newJnt;

            // @TODO: implement joint limits checks and general safety...

            // Modify current Joints
            if (action.relative)
            {
                // If user issued a relative action, make sure there are absolute values to work with. 
                // (This limitation is due to current lack of FK/IK solvers)
                if (joints == null)  // could also check for motionType == MotionType.Joints ?
                {
                    logger.Info($"Cannot apply \"{action}\", must provide absolute Joints values first before applying relative ones...");
                    return false;
                }

                newJnt = Joints.Add(joints, action.joints);
            }
            else
            {
                newJnt = new Joints(action.joints);
            }

            prevJoints = joints;
            joints = newJnt;

            // Flag the lack of other geometric data
            prevPosition = position;
            position = null;
            prevRotation = rotation;
            rotation = null;

            if (isExtruding) this.ComputeExtrudedLength();

            return true;
        }



        //  ╔╦╗╦╔═╗╔═╗  ╔═╗╔═╗╔╦╗╦╔═╗╔╗╔╔═╗
        //  ║║║║╚═╗║    ╠═╣║   ║ ║║ ║║║║╚═╗
        //  ╩ ╩╩╚═╝╚═╝  ╩ ╩╚═╝ ╩ ╩╚═╝╝╚╝╚═╝
        /// <summary>
        /// Apply Message Action.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool ApplyAction(ActionMessage action)
        {
            // There is basically nothing to do here! Leave the state of the robot as-is.
            // Maybe do some Console output?
            return true;
        }

        /// <summary>
        /// Apply Wait Action.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool ApplyAction(ActionWait action)
        {
            // There is basically nothing to do here! Leave the state of the robot as-is.
            return true;
        }

        /// <summary>
        /// Apply Comment Action.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool ApplyAction(ActionComment action)
        {
            // There is basically nothing to do here! Leave the state of the robot as-is.
            return true;
        }

        /// <summary>
        /// Apply Attach Tool Action.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool ApplyAction(ActionAttach action)
        {
            // The cursor has now a tool attached to it
            this.tool = action.tool;

            // Relative transform
            // If user issued a relative action, make sure there are absolute values to work with. (This limitation is due to current lack of FK/IK solvers)
            if (this.position == null || this.rotation == null)
            {
                logger.Info($"Cannot apply \"{action}\", must provide absolute transform values before attaching a tool... ");
            }
            else
            {
                // Now transform the cursor position to the tool's transformation params:
                Vector worldVector = Vector.Rotation(action.tool.TCPPosition, this.rotation);
                Vector newPos = this.position + worldVector;
                Rotation newRot = Rotation.Combine(this.rotation, action.tool.TCPOrientation);  // postmultiplication

                this.prevPosition = this.position;
                this.position = newPos;
                this.prevRotation = this.rotation;
                this.rotation = newRot;
                this.prevJoints = this.joints;
                
                // this.joints = null;  // flag joints as null to avoid Joint instructions using obsolete data --> no need to do this, joints remain the same anyway?
            }
            
            return true;
        }

        /// <summary>
        /// Apply Detach Tool action
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool ApplyAction(ActionDetach action)
        {
            if (this.tool == null)
            {
                logger.Verbose("Robot had no tool attached");
                return false;
            }

            // Relative transform
            // If user issued a relative action, make sure there are absolute values to work with. (This limitation is due to current lack of FK/IK solvers)
            if (this.position == null || this.rotation == null)
            {
                logger.Info($"Cannot apply \"{action}\", must provide absolute transform values before detaching a tool... " + this);
                return false;
            }

            // Now undo the tool's transforms
            // TODO: at some point in the future, check for translationFirst here
            Rotation newRot = Rotation.Combine(this.rotation, Rotation.Inverse(this.tool.TCPOrientation));  // postmultiplication by the inverse rotation
            Vector worldVector = Vector.Rotation(this.tool.TCPPosition, this.rotation);
            Vector newPos = this.position - worldVector;

            this.prevPosition = this.position;
            this.position = newPos;
            this.prevRotation = this.rotation;
            this.rotation = newRot;
            this.prevJoints = this.joints;
            this.joints = null;

            // Detach the tool
            this.tool = null;

            return true;
        }

        /// <summary>
        /// Apply ActionIODigital write action to this cursor.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool ApplyAction(ActionIODigital action)
        {
            //if (action.pinName < 0 || action.pinName >= digitalOutputs.Length)
            //{
            //    Console.WriteLine("Cannot write to digital IO: robot has no pin #" + action.pinName);
            //    return false;
            //}

            //this.digitalOutputs[action.pinName] = action.on;
            //return true;

            if (digitalOutputs.ContainsKey(action.pinName))
            {
                digitalOutputs[action.pinName] = action.on;
            }
            else
            {
                digitalOutputs.Add(action.pinName, action.on);
            }

            return true;
        }

        /// <summary>
        /// Apply ActionIOAnalog write action to this cursor. 
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool ApplyAction(ActionIOAnalog action)
        {
            //if (action.pinName < 0 || action.pinName >= analogOutputs.Length)
            //{
            //    Console.WriteLine("Cannot write to analog IO: robot has no analog pin #" + action.pinName);
            //    return false;
            //}

            //this.analogOutputs[action.pinName] = action.value;
            //return true;

            if (analogOutputs.ContainsKey(action.pinName))
            {
                analogOutputs[action.pinName] = action.value;
            }
            else
            {
                analogOutputs.Add(action.pinName, action.value);
            }

            return true;
        }


        //  ╔╦╗╔═╗╔╦╗╔═╗   ╔═╗═╗ ╦╔╦╗╦═╗╦ ╦╔╦╗╔═╗  ┌─┐┌─┐┌┬┐┬┌─┐┌┐┌┌─┐
        //   ║ ║╣ ║║║╠═╝───║╣ ╔╩╦╝ ║ ╠╦╝║ ║ ║║║╣   ├─┤│   │ ││ ││││└─┐
        //   ╩ ╚═╝╩ ╩╩     ╚═╝╩ ╚═ ╩ ╩╚═╚═╝═╩╝╚═╝  ┴ ┴└─┘ ┴ ┴└─┘┘└┘└─┘
        /// <summary>
        /// Apply ActionTemperature write action to this cursor.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool ApplyAction(ActionTemperature action)
        {
            if (action.relative)
                this.partTemperature[action.robotPart] += action.temperature;
            else
                this.partTemperature[action.robotPart] = action.temperature;

            return true;
        }

        /// <summary>
        /// Apply ActionExtrusion write action to this cursor.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool ApplyAction(ActionExtrusion action)
        {
            this.isExtruding = action.extrude;

            return true;
        }

        /// <summary>
        /// Apply ActionExtrusionRate write action to this cursor. 
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool ApplyAction(ActionExtrusionRate action)
        {
            if (action.relative)
                this.extrusionRate += action.rate;
            else
                this.extrusionRate = action.rate;

            return true;
        }

        /// <summary>
        /// This is just to write start/end boilerplates for 3D printers. 
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool ApplyAction(ActionInitialization action)
        {
            // nothing to do here really... 
            return true;
        }



        //  ╔═╗═╗ ╦╔╦╗╔═╗╦═╗╔╗╔╔═╗╦      ╔═╗═╗ ╦╦╔═╗
        //  ║╣ ╔╩╦╝ ║ ║╣ ╠╦╝║║║╠═╣║      ╠═╣╔╩╦╝║╚═╗
        //  ╚═╝╩ ╚═ ╩ ╚═╝╩╚═╝╚╝╩ ╩╩═╝────╩ ╩╩ ╚═╩╚═╝
        public bool ApplyAction(ActionExternalAxis action)
        {
            if (this.externalAxes == null)
            {
                this.externalAxes = new ExternalAxes();
            }

            if (action.relative)
            {
                if (this.externalAxes[action.axisNumber - 1] == null)
                {
                    logger.Info($"Cannot apply \"{action}\", must initialize absolute axis value first for axis {action.axisNumber} before applying relative ones...");
                    return false;
                }

                this.externalAxes[action.axisNumber - 1] += action.value;
            }
            else
            {
                this.externalAxes[action.axisNumber - 1] = action.value;
            }

            return true;
        }



        //  ╔═╗╦ ╦╔═╗╔╦╗╔═╗╔╦╗    ╔═╗╔═╗╔╦╗╔═╗
        //  ║  ║ ║╚═╗ ║ ║ ║║║║    ║  ║ ║ ║║║╣ 
        //  ╚═╝╚═╝╚═╝ ╩ ╚═╝╩ ╩────╚═╝╚═╝═╩╝╚═╝
        /// <summary>
        /// This action doesn't change the state of the cursor... 
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool ApplyAction(ActionCustomCode action) => true;






        //  ██╗   ██╗████████╗██╗██╗     ███████╗
        //  ██║   ██║╚══██╔══╝██║██║     ██╔════╝
        //  ██║   ██║   ██║   ██║██║     ███████╗
        //  ██║   ██║   ██║   ██║██║     ╚════██║
        //  ╚██████╔╝   ██║   ██║███████╗███████║
        //   ╚═════╝    ╚═╝   ╚═╝╚══════╝╚══════╝
        //                                       
        /// <summary>
        /// Update the current extrudedLength.
        /// </summary>
        public void ComputeExtrudedLength()
        {
            if (this.prevPosition == null || this.position == null)
            {
                logger.Info($"Cannot compute new extrusion length: cursor position missing");
                return;
            }

            this.prevExtrudedLength = this.extrudedLength;
            this.extrudedLength += this.extrusionRate * this.prevPosition.DistanceTo(this.position);
        }

        public override string ToString() => $"{name}: {motionType} p{position} r{rotation} j{joints} a{acceleration} v{speed} rv{rotationSpeed} jv{jointSpeed} ja{jointAcceleration} p{precision} {(this.tool == null ? "" : "t" + this.tool)}";


    }
}
