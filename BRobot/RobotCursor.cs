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
    /// Useful as virtual representation of a simulated or controlled robot actuator. 
    /// </summary>
    internal class RobotCursor
    {
        // Public props
        public string name;
        public Vector position;
        public Rotation rotation;
        public Joints joints;
        public int speed;
        public int zone;
        public MotionType motionType;
        public ReferenceCS referenceCS;
        protected bool initialized = false;
        private bool applyImmediately = false;  // when an action is issued to this cursor, apply it immediately?


        /// <summary>
        /// Who manages this Cursor?
        /// </summary>
        public Control parentControl;

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





        //  ╔╗ ╔═╗╔═╗╔═╗
        //  ╠╩╗╠═╣╚═╗║╣ 
        //  ╚═╝╩ ╩╚═╝╚═╝
        /// <summary>
        /// Main constructor.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="applyImmediately"></param>
        public RobotCursor(Control parentControl, string name,  bool applyImmediately)
        {
            this.parentControl = parentControl;
            this.name = name;
            this.applyImmediately = applyImmediately;

            // 
            if (this.parentControl.robotBrand == RobotType.Undefined)
            {
                compiler = new CompilerHuman();
            }
            else if (this.parentControl.robotBrand == RobotType.ABB)
            {
                compiler = new CompilerABB();
            }
            else if (this.parentControl.robotBrand == RobotType.UR)
            {
                compiler = new CompilerUR();
            }
            else if (this.parentControl.robotBrand == RobotType.KUKA)
            {
                compiler = new CompilerKUKA();
            }

            actionBuffer = new ActionBuffer();
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
            int speed, int zone, MotionType mType, ReferenceCS refCS)
        {
            if (position != null) this.position = new Vector(position);
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




        //  ╔═╗╔═╗╔╦╗╦╔═╗╔╗╔╔═╗
        //  ╠═╣║   ║ ║║ ║║║║╚═╗
        //  ╩ ╩╚═╝ ╩ ╩╚═╝╝╚╝╚═╝
        /// <summary>
        /// A dict that maps Action types to the cursor's applicable method.
        /// https://chodounsky.net/2014/01/29/dynamic-dispatch-in-c-number/
        /// </summary>
        Dictionary<Type, Func<BRobot.Action, RobotCursor, bool>> ActionsMap = new Dictionary<Type, Func<Action, RobotCursor, bool>>()
        {
            { typeof (ActionSpeed),                     (act, robCur) => robCur.ApplyAction((ActionSpeed) act) },
            { typeof (ActionZone),                      (act, robCur) => robCur.ApplyAction((ActionZone) act) },
            { typeof (ActionMotion),                    (act, robCur) => robCur.ApplyAction((ActionMotion) act) },
            { typeof (ActionCoordinates),               (act, robCur) => robCur.ApplyAction((ActionCoordinates) act) },
            { typeof (ActionPushPop),                   (act, robCur) => robCur.ApplyAction((ActionPushPop) act) },
            { typeof (ActionTranslation),               (act, robCur) => robCur.ApplyAction((ActionTranslation) act) },
            { typeof (ActionRotation),                  (act, robCur) => robCur.ApplyAction((ActionRotation) act) },
            { typeof (ActionTransformation),            (act, robCur) => robCur.ApplyAction((ActionTransformation) act) },
            { typeof (ActionJoints),                    (act, robCur) => robCur.ApplyAction((ActionJoints) act) },
            { typeof (ActionMessage),                   (act, robCur) => robCur.ApplyAction((ActionMessage) act) },
            { typeof (ActionWait),                      (act, robCur) => robCur.ApplyAction((ActionWait) act) },
            { typeof (ActionComment),                   (act, robCur) => robCur.ApplyAction((ActionComment) act) },


        };

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
        /// <param name="inlineTargets">Write inline targets on action statements, or declare them as independent variables?</param>
        /// <returns></returns>
        public List<string> ProgramFromBuffer(bool inlineTargets)
        {
            return compiler.UNSAFEProgramFromBuffer("BRobotProgram", this, false, inlineTargets);
        }

        /// <summary>
        /// Return a device-specific program with the next block of Actions pending in the buffer.
        /// </summary>
        /// <param name="inlineTargets">Write inline targets on action statements, or declare them as independent variables?</param>
        /// <returns></returns>
        public List<string> ProgramFromBlock(bool inlineTargets)
        {
            return compiler.UNSAFEProgramFromBuffer("BRobotProgram", this, true, inlineTargets);
        }

        public void LogBufferedActions()
        {
            lock(actionBufferLock)
            {
                actionBuffer.LogBufferedActions();
            }
        }





        //  ╔═╗╔═╗╔╦╗╔╦╗╦╔╗╔╔═╗╔═╗  ╔═╗╔═╗╔╦╗╦╔═╗╔╗╔╔═╗
        //  ╚═╗║╣  ║  ║ ║║║║║ ╦╚═╗  ╠═╣║   ║ ║║ ║║║║╚═╗
        //  ╚═╝╚═╝ ╩  ╩ ╩╝╚╝╚═╝╚═╝  ╩ ╩╚═╝ ╩ ╩╚═╝╝╚╝╚═╝
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

            if (this.speed < 0) speed = 0;

            return true;
        }

        /// <summary>
        /// Apply Zone Action.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool ApplyAction(ActionZone action)
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
                    Console.WriteLine("Sorry, must provide absolute position values first before applying relative ones... " + this);
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
                    Console.WriteLine("Sorry, currently missing TCP orientation to work with... " + this);
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
        public bool ApplyAction(ActionRotation action)
        {
            // @TODO: implement some kind of security check here...
            if (action.relative)
            {
                // If user issued a relative action, make sure there are absolute values to work with. (This limitation is due to current lack of FK/IK solvers)
                if (position == null || rotation == null)
                {
                    Console.WriteLine("Sorry, must provide absolute rotation values first before applying relative ones... " + this);
                    return false;
                }

                if (referenceCS == ReferenceCS.World)
                {
                    rotation.RotateGlobal(action.rotation);
                }
                else
                {
                    rotation.RotateLocal(action.rotation);
                }
            }
            else
            {
                // Fail if issued abs rotation without prior position info. (This limitation is due to current lack of FK/IK solvers)
                if (position == null)
                {
                    Console.WriteLine("Sorry, currently missing TCP position to work with... " + this);
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
                    Console.WriteLine("Sorry, must provide absolute transform values first before applying relative ones..." + this);
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
        public bool ApplyAction(ActionJoints action)
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








        public override string ToString()
        {
            return string.Format("{0}: {1} p{2} r{3} j{4} v{5} z{6}", name, motionType, position, rotation, joints, speed, zone);
        }

    }
}
