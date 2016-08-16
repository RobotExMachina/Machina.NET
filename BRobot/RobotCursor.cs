using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRobot
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
    /// Useful as virtual representation of a simulated or controller robot actuator. 
    /// </summary>
    abstract class RobotCursor
    {
        // Public props
        public string name;
        public Point position;
        public Rotation rotation;
        public Joints joints;
        public int speed;
        public int zone;
        public MotionType motionType;
        public ReferenceCS referenceCS;
        protected bool initialized = false;
        private bool applyImmediately = false;  // when an action is issued to this cursor, apply it immediately?

        

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
        protected SettingsBuffer settingsBuffer;

        /// <summary>
        /// Manages pending and released Actions, plus blocks. 
        /// </summary>
        public ActionBuffer actionBuffer;

        /// <summary>
        /// A lock for buffer manipulation operations. 
        /// </summary>
        public object actionBufferLock = new object();

        /// <summary>
        /// Main constructor.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="applyImmediately"></param>
        public RobotCursor(string name, bool applyImmediately)
        {
            this.name = name;
            this.applyImmediately = applyImmediately;

            actionBuffer = new ActionBuffer();
            settingsBuffer = new SettingsBuffer();
        }

        // Abstract methods
        public abstract bool ApplyAction(ActionSpeed action);
        public abstract bool ApplyAction(ActionZone action);
        public abstract bool ApplyAction(ActionMotion action);
        public abstract bool ApplyAction(ActionCoordinates action);
        public abstract bool ApplyAction(ActionPushPop action);
        public abstract bool ApplyAction(ActionTranslation action);
        public abstract bool ApplyAction(ActionRotation action);
        public abstract bool ApplyAction(ActionTransformation action);
        public abstract bool ApplyAction(ActionJoints action);
        public abstract bool ApplyAction(ActionMessage action);
        public abstract bool ApplyAction(ActionWait action);

        

        /// <summary>
        /// A dict that maps Action types to the cursor's applicable method.
        /// https://chodounsky.net/2014/01/29/dynamic-dispatch-in-c-number/
        /// </summary>
        Dictionary<Type, Func<BRobot.Action, RobotCursor, bool>> ActionsMap = new Dictionary<Type, Func<Action, RobotCursor, bool>>()
        {
            { typeof (ActionSpeed),                     (i, rc) => rc.ApplyAction((ActionSpeed) i) },
            { typeof (ActionZone),                      (i, rc) => rc.ApplyAction((ActionZone) i) },
            { typeof (ActionMotion),                    (i, rc) => rc.ApplyAction((ActionMotion) i) },
            { typeof (ActionCoordinates),               (i, rc) => rc.ApplyAction((ActionCoordinates) i) },
            { typeof (ActionPushPop),                   (i, rc) => rc.ApplyAction((ActionPushPop) i) },
            { typeof (ActionTranslation),               (i, rc) => rc.ApplyAction((ActionTranslation) i) },
            { typeof (ActionRotation),                  (i, rc) => rc.ApplyAction((ActionRotation) i) },
            { typeof (ActionTransformation),            (i, rc) => rc.ApplyAction((ActionTransformation) i) },
            { typeof (ActionJoints),                    (i, rc) => rc.ApplyAction((ActionJoints) i) },
            { typeof (ActionMessage),                   (i, rc) => rc.ApplyAction((ActionMessage) i) },
            { typeof (ActionWait),                      (i, rc) => rc.ApplyAction((ActionWait) i) }
            
        };

        /// <summary>
        /// Minimum information necessary to initialize a robot object.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="joints"></param>
        /// <returns></returns>
        public bool Initialize(Point position, Rotation rotation, Joints joints, 
            int speed, int zone, MotionType mType, ReferenceCS refCS)
        {
            if (position != null) this.position = new Point(position);
            if (rotation != null) this.rotation = new Rotation(rotation);
            if (joints != null) this.joints = new Joints(joints);
            this.speed = speed;
            this.zone = zone;
            this.motionType = mType;
            this.referenceCS = refCS; 

            initialized = true;
            return initialized;
        }
        
        /// <summary>
        /// Set specified RobotCursor as child to this one.
        /// </summary>
        /// <param name="childCursor"></param>
        public void SetChild(RobotCursor childCursor)
        {
            child = childCursor;
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
                Action next = actionBuffer.GetNext();
                if (next == null) return false;
                bool success = Apply(next);
                if (success)
                {
                    child.Issue(next);
                }
                return success;
            }
        }

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

        public int ActionsPending()
        {
            return actionBuffer.ActionsPending();
        }
        
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
            
            Console.WriteLine("Found no suitable method for this Action " + action);
            return false;
        }

        /// <summary>
        /// Return a device-specific program with all the Actions pending in the buffer.
        /// </summary>
        /// <returns></returns>
        public List<string> ProgramFromBuffer()
        {
            return compiler.UNSAFEProgramFromBuffer("BRobotProgram", this, false);
        }

        /// <summary>
        /// Return a device-specific program with the next block of Actions pending in the buffer.
        /// </summary>
        /// <returns></returns>
        public List<string> ProgramFromBlock()
        {
            return compiler.UNSAFEProgramFromBuffer("BRobotProgram", this, true);
        }

        public void LogBufferedActions()
        {
            lock(actionBufferLock)
            {
                actionBuffer.LogBufferedActions();
            }
        }

    }




    //   █████╗ ██████╗ ██████╗ 
    //  ██╔══██╗██╔══██╗██╔══██╗
    //  ███████║██████╔╝██████╔╝
    //  ██╔══██║██╔══██╗██╔══██╗
    //  ██║  ██║██████╔╝██████╔╝
    //  ╚═╝  ╚═╝╚═════╝ ╚═════╝ 
    //                          
    internal class RobotCursorABB : RobotCursor
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="applyImmediately"></param>
        public RobotCursorABB(string name, bool applyImmediately) : base(name, applyImmediately)
        {
            compiler = new CompilerABB();
        }










        /// <summary>
        /// Apply Speed Action.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public override bool ApplyAction(ActionSpeed action)
        {
            if (action.relative)
                this.speed += action.speed;
            else
                this.speed = action.speed;

            if (this.speed < 0) speed = 0;

            return true;
        }

        /// <summary>
        /// Apply Zone Action.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public override bool ApplyAction(ActionZone action)
        {
            if (action.relative)
                this.zone += action.zone;
            else
                this.zone = action.zone;

            if (this.zone < 0) zone = 0;

            return true;
        }

        /// <summary>
        /// Apply Motion Action.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public override bool ApplyAction(ActionMotion action)
        {
            this.motionType = action.motionType;

            return true;
        }

        /// <summary>
        /// Apply ReferenceCS Action.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public override bool ApplyAction(ActionCoordinates action)
        {
            this.referenceCS = action.referenceCS;

            return true;
        }

        /// <summary>
        /// Apply a Push or Pop Action.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public override bool ApplyAction(ActionPushPop action)
        {
            if (action.push)
            {
                Settings s = new Settings(this.speed, this.zone, this.motionType, this.referenceCS);
                return this.settingsBuffer.Push(s);
            }
            else
            {
                Settings s = settingsBuffer.Pop();
                if (s != null)
                {
                    this.speed = s.Speed;
                    this.zone = s.Zone;
                    this.motionType = s.MotionType;
                    this.referenceCS = s.RefCS;
                    return true;
                }
            }
            return false;
        }











        /// <summary>
        /// Apply Translation Action.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public override bool ApplyAction(ActionTranslation action)
        {
            Point newPosition = new Point();

            if (action.relative)
            {
                // If user issued a relative action, make sure there are absolute values to work with. (This limitation is due to current lack of FK/IK solvers)
                if (position == null || rotation == null)
                {
                    Console.WriteLine("Sorry, must provide absolute position values first before applying relative ones..." + this);
                    return false;
                }

                if (referenceCS == ReferenceCS.World)
                {
                    newPosition = position + action.translation;
                }
                else
                {
                    //Point worldVector = Point.Rotation(action.translation, Rotation.Conjugate(this.rotation));
                    Point worldVector = Point.Rotation(action.translation, this.rotation);
                    newPosition = position + worldVector;
                }
            }
            else
            {
                // Fail if issued abs movement without prior rotation info. (This limitation is due to current lack of FK/IK solvers)
                if (rotation == null)
                {
                    Console.WriteLine("Sorry, currently missing TCP orientation to work with..." + this);
                    return false;
                }

                newPosition.Set(action.translation);
            }

            // @TODO: this must be more programmatically implemented 
            if (Control.SAFETY_CHECK_TABLE_COLLISION)
            {
                if (Control.IsBelowTable(newPosition.Z))
                {
                    if (Control.SAFETY_STOP_ON_TABLE_COLLISION)
                    {
                        Console.WriteLine("Cannot perform action: too close to base XY plane --> TCP.z = {0}", newPosition.Z);
                        return false;
                    }
                    else
                    {
                        Console.WriteLine("WARNING: too close to base XY plane, USE CAUTION! --> TCP.z = {0}", newPosition.Z);
                    }
                }
            }

            position = newPosition;
            joints = null;      // flag joints as null to avoid Joint instructions using obsolete data

            return true;
        }


        /// <summary>
        /// Apply Rotation Action.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public override bool ApplyAction(ActionRotation action)
        {
            // @TODO: implement some kind of security check here...
            if (action.relative)
            {
                // If user issued a relative action, make sure there are absolute values to work with. (This limitation is due to current lack of FK/IK solvers)
                if (position == null || rotation == null)
                {
                    Console.WriteLine("Sorry, must provide absolute rotation values first before applying relative ones..." + this);
                    return false;
                }

                if (referenceCS == ReferenceCS.World)
                {
                    rotation.PreMultiply(action.rotation);
                }
                else
                {
                    rotation.Multiply(action.rotation);
                }
            }
            else
            {
                // Fail if issued abs rotation without prior position info. (This limitation is due to current lack of FK/IK solvers)
                if (position == null)
                {
                    Console.WriteLine("Sorry, currently missing TCP position to work with..." + this);
                    return false;
                }

                rotation = new Rotation(action.rotation);
            }

            joints = null;      // flag joints as null to avoid Joint instructions using obsolete data

            return true;
        }


        /// <summary>
        /// Apply Transformation Action.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public override bool ApplyAction(ActionTransformation action)
        {
            Point newPos;
            Rotation newRot;

            // Relative transform
            if (action.relative)
            {
                // If user issued a relative action, make sure there are absolute values to work with. (This limitation is due to current lack of FK/IK solvers)
                if (position == null || rotation == null)
                {
                    Console.WriteLine("Sorry, must provide absolute transform values first before applying relative ones..." + this);
                    return false;
                }

                // This is Translate + Rotate
                if (action.translationFirst)
                {
                    if (referenceCS == ReferenceCS.World)
                    {
                        newPos = position + action.translation;
                        newRot = Rotation.Multiply(action.rotation, rotation);  // premultiplication
                    }
                    else
                    {
                        //Point worldVector = Point.Rotation(action.translation, Rotation.Conjugate(this.rotation));
                        Point worldVector = Point.Rotation(action.translation, this.rotation);
                        newPos = position + worldVector;
                        newRot = Rotation.Multiply(rotation, action.rotation);  // postmultiplication
                    }
                }

                // or Rotate + Translate
                else
                {
                    if (referenceCS == ReferenceCS.World)
                    {
                        newPos = position + action.translation;
                        newRot = Rotation.Multiply(action.rotation, rotation);  // premultiplication
                    }

                    else
                    {
                        // @TOCHECK: is this correct?
                        newRot = Rotation.Multiply(rotation, action.rotation);  // postmultiplication
                        //Point worldVector = Point.Rotation(action.translation, Rotation.Conjugate(newRot));
                        Point worldVector = Point.Rotation(action.translation, newRot);
                        newPos = position + worldVector;
                    }

                }

            }

            // Absolute transform
            else
            {
                newPos = new Point(action.translation);
                newRot = new Rotation(action.rotation);
            }

            // @TODO: this must be more programmatically implemented 
            if (Control.SAFETY_CHECK_TABLE_COLLISION)
            {
                if (Control.IsBelowTable(newPos.Z))
                {
                    if (Control.SAFETY_STOP_ON_TABLE_COLLISION)
                    {
                        Console.WriteLine("Cannot perform action: too close to base XY plane --> TCP.z = {0}", newPos.Z);
                        return false;
                    }
                    else
                    {
                        Console.WriteLine("WARNING: too close to base XY plane, USE CAUTION! --> TCP.z = {0}", newPos.Z);
                    }
                }
            }
            
            position = newPos;
            rotation = newRot;
            joints = null;  // flag joints as null to avoid Joint instructions using obsolete data

            return true;
        }

        /// <summary>
        /// Apply Joints Action.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public override bool ApplyAction(ActionJoints action)
        {
            
            // @TODO: implement joint limits checks and general safety...

            // Modify current Joints
            if (action.relative)
            {
                // If user issued a relative action, make sure there are absolute values to work with. 
                // (This limitation is due to current lack of FK/IK solvers)
                if (joints == null)  // could also check for motionType == MotionType.Joints ?
                {
                    Console.WriteLine("Sorry, must provide absolute Joints values first before applying relative ones..." + this);
                    return false;
                }
                joints.Add(action.joints);
            }
            else
            {
                joints = new Joints(action.joints);  // create a new object since it was probably null
            }

            // Flag the lack of other geometric data
            position = null;
            rotation = null;

            return true;
        }

        /// <summary>
        /// Apply Message Action.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public override bool ApplyAction(ActionMessage action)
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
        public override bool ApplyAction(ActionWait action)
        {
            // There is basically nothing to do here! Leave the state of the robot as-is.
            return true;
        }

        







        /// <summary>
        /// Returns an ABB robtarget representation of the current state of the cursor.
        /// WARNING: this method is EXTREMELY UNSAFE; it performs no IK calculations, assigns default [0,0,0,0] 
        /// robot configuration and assumes the robot controller will figure out the correct one.
        /// </summary>
        /// <returns></returns>
        public string GetUNSAFERobTargetValue()
        {
            return string.Format("[{0},{1},[0,0,0,0],[0,9E9,9E9,9E9,9E9,9E9]]", position, rotation);
        }

        /// <summary>
        /// Returns an ABB jointtarget representation of the current stat of the cursor.
        /// </summary>
        /// <returns></returns>
        public string GetJointTargetValue()
        {
            return string.Format("[{0},[0,9E9,9E9,9E9,9E9,9E9]]", joints);
        }

        public string GetSpeedDeclaration(int speed)
        {
            // Default speed declarations in ABB always use 500 deg/s as rot speed, but it feels too fast (and scary). 
            // Using the same value as lin motion here.
            return string.Format("[{0},{1},{2},{3}]", speed, speed, 5000, 1000);
        }

        public string GetSpeedValue()
        {
            return GetSpeedDeclaration(speed);
        }
        
        public string GetZoneDeclaration(int zone)
        {
            // Following conventions for default RAPID zones.
            double high = 1.5 * zone;
            double low = 0.15 * zone;
            return string.Format("[FALSE,{0},{1},{2},{3},{4},{5}]", zone, high, high, low, high, low);
        }

        public string GetZoneValue()
        {
            return GetZoneDeclaration(zone);
        }




        public override string ToString()
        {
            return string.Format("{5}: {0} p{1} r{2} j{6} v{3} z{4}", motionType, position, rotation, speed, zone, name, joints);
        }

    }
}
