using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina
{

    /// <summary>
    /// Defines an Action Type, like Translation, Rotation, Wait... 
    /// Useful to flag base Actions into children classes.
    /// </summary>
    public enum ActionType
    {
        Undefined = 0,
        Translation = 1,
        Rotation = 2,
        Transformation = 3,
        Axes = 4,
        Message = 5,
        Wait = 6,
        Speed = 7,
        Precision = 8,
        Motion = 9,
        Coordinates = 10,
        PushPop = 11, 
        Comment = 12,
        Attach = 13,
        Detach = 14,
        IODigital = 15,
        IOAnalog = 16, 
        Temperature = 17,
        Extrusion = 18,
        ExtrusionRate = 19,
        Initialization = 20
    }

    


    //   █████╗  ██████╗████████╗██╗ ██████╗ ███╗   ██╗
    //  ██╔══██╗██╔════╝╚══██╔══╝██║██╔═══██╗████╗  ██║
    //  ███████║██║        ██║   ██║██║   ██║██╔██╗ ██║
    //  ██╔══██║██║        ██║   ██║██║   ██║██║╚██╗██║
    //  ██║  ██║╚██████╗   ██║   ██║╚██████╔╝██║ ╚████║
    //  ╚═╝  ╚═╝ ╚═════╝   ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝
    //                                                 
    /// <summary>
    /// Actions represent high-level abstract operations such as movements, rotations, 
    /// transformations or joint manipulations, both in absolute and relative terms. 
    /// They are independent from the device's properties, and their translation into
    /// actual robotic instructions depends on the robot's properties and state. 
    /// </summary>
    public class Action
    {

        //  ╔═╗╔╦╗╔═╗╔╦╗╦╔═╗  ╔═╗╔╦╗╦ ╦╔═╗╔═╗
        //  ╚═╗ ║ ╠═╣ ║ ║║    ╚═╗ ║ ║ ║╠╣ ╠╣ 
        //  ╚═╝ ╩ ╩ ╩ ╩ ╩╚═╝  ╚═╝ ╩ ╚═╝╚  ╚  
        internal static int currentId = 1;  // a rolling id counter

        // Static constructors for APIs creating abstract actions (Dynamo ehem!)
        public static ActionSpeed Speed(int speedInc)
        {
            return new ActionSpeed(speedInc, true);
        }

        public static ActionSpeed SpeedTo(int speed)
        {
            return new ActionSpeed(speed, false);
        }

        public static ActionPrecision Zone(int zoneInc)
        {
            return new ActionPrecision(zoneInc, true);
        }

        public static ActionPrecision ZoneTo(int zone)
        {
            return new ActionPrecision(zone, false);
        }

        public static ActionMotion Motion(MotionType motionType)
        {
            return new ActionMotion(motionType);
        }

        public static ActionCoordinates Coordinates(ReferenceCS referenceCS)
        {
            return new ActionCoordinates(referenceCS);
        }

        public static ActionTranslation Move(Vector pos)
        {
            return new ActionTranslation(pos, true); 
        }

        public static ActionTranslation MoveTo(Vector pos)
        {
            return new ActionTranslation(pos, false);
        }

        public static ActionRotation Rotate(Rotation rot)
        {
            return new ActionRotation(rot, true);
        }

        public static ActionRotation RotateTo(Rotation rot)
        {
            return new ActionRotation(rot, false);
        }

        public static ActionTransformation Transform(Vector pos, Rotation rot, bool translationFirst)
        {
            return new ActionTransformation(pos, rot, true, translationFirst);
        }

        public static ActionTransformation TransformTo(Vector pos, Rotation rot)
        {
            return new ActionTransformation(pos, rot, false, true);
        }

        public static ActionAxes Joints(Joints jointsInc)
        {
            return new ActionAxes(jointsInc, true);
        }

        public static ActionAxes JointsTo(Joints joints)
        {
            return new ActionAxes(joints, false);
        }
        
        public static ActionWait Wait(long millis)
        {
            return new ActionWait(millis);
        }

        public static ActionMessage Message(string msg)
        {
            return new ActionMessage(msg);
        }

        // Why were these not here...?
        public static ActionPushPop PushSettings()
        {
            return new ActionPushPop(true);
        }

        public static ActionPushPop PopSettings()
        {
            return new ActionPushPop(false);
        }

        public static ActionComment Comment(string comment)
        {
            return new ActionComment(comment);
        }

        public static ActionAttach Attach(Tool tool)
        {
            return new ActionAttach(tool);
        }

        public static ActionIODigital WriteDigital(int pinNum, bool isOn)
        {
            return new ActionIODigital(pinNum, isOn);
        }

        public static ActionIOAnalog WriteAnalog(int pinNum, double value)
        {
            return new ActionIOAnalog(pinNum, value);
        }

        public static ActionTemperature Temperature(double temp, RobotPart devicePart, bool wait, bool relative)
        {
            return new ActionTemperature(temp, devicePart, wait, relative);
        }

        public static ActionExtrusion Extrude(bool extrude)
        {
            return new ActionExtrusion(extrude);
        }

        public static ActionExtrusionRate FeedRate(double rate, bool relative)
        {
            return new ActionExtrusionRate(rate, relative);
        }

        public static ActionInitialization Initialize(bool init)
        {
            return new ActionInitialization(init);
        }



        //  ╦╔╗╔╔═╗╔╦╗╔═╗╔╗╔╔═╗╔═╗  ╔═╗╔╦╗╦ ╦╔═╗╔═╗
        //  ║║║║╚═╗ ║ ╠═╣║║║║  ║╣   ╚═╗ ║ ║ ║╠╣ ╠╣ 
        //  ╩╝╚╝╚═╝ ╩ ╩ ╩╝╚╝╚═╝╚═╝  ╚═╝ ╩ ╚═╝╚  ╚  
        public ActionType type = ActionType.Undefined;
        public int id;

        /// <summary>
        /// A base constructor to take care of common setup for all actionss
        /// </summary>
        public Action()
        {
            this.id = currentId++;
        }
    }




    //  ███████╗██████╗ ███████╗███████╗██████╗ 
    //  ██╔════╝██╔══██╗██╔════╝██╔════╝██╔══██╗
    //  ███████╗██████╔╝█████╗  █████╗  ██║  ██║
    //  ╚════██║██╔═══╝ ██╔══╝  ██╔══╝  ██║  ██║
    //  ███████║██║     ███████╗███████╗██████╔╝
    //  ╚══════╝╚═╝     ╚══════╝╚══════╝╚═════╝ 
    //                                          
    /// <summary>
    /// An Action to change the current speed setting.
    /// </summary>
    public class ActionSpeed : Action
    {
        public int speed;
        public bool relative;

        public ActionSpeed(int speed, bool relative) : base()
        {
            type = ActionType.Speed;

            this.speed = speed;
            this.relative = relative;
        }

        public override string ToString()
        {
            return relative ?
                string.Format("{0} speed by {1} mm/s", this.speed < 0 ? "Decrease" : "Increase", speed) :
                string.Format("Set speed to {0} mm/s", speed);
        }
    }

    //  ███████╗ ██████╗ ███╗   ██╗███████╗
    //  ╚══███╔╝██╔═══██╗████╗  ██║██╔════╝
    //    ███╔╝ ██║   ██║██╔██╗ ██║█████╗  
    //   ███╔╝  ██║   ██║██║╚██╗██║██╔══╝  
    //  ███████╗╚██████╔╝██║ ╚████║███████╗
    //  ╚══════╝ ╚═════╝ ╚═╝  ╚═══╝╚══════╝
    //                                     
    /// <summary>
    /// An Action to change current zone setting.
    /// </summary>
    public class ActionPrecision : Action
    {
        public int zone;
        public bool relative;

        public ActionPrecision(int zone, bool relative) : base()
        {
            type = ActionType.Precision;

            this.zone = zone;
            this.relative = relative;
        }

        public override string ToString()
        {
            return relative ?
                string.Format("{0} precision by {1} mm", this.zone < 0 ? "Decrease" : "Increase", this.zone) :
                string.Format("Set precision to {0} mm", this.zone);
        }
    }

    //  ███╗   ███╗ ██████╗ ████████╗██╗ ██████╗ ███╗   ██╗
    //  ████╗ ████║██╔═══██╗╚══██╔══╝██║██╔═══██╗████╗  ██║
    //  ██╔████╔██║██║   ██║   ██║   ██║██║   ██║██╔██╗ ██║
    //  ██║╚██╔╝██║██║   ██║   ██║   ██║██║   ██║██║╚██╗██║
    //  ██║ ╚═╝ ██║╚██████╔╝   ██║   ██║╚██████╔╝██║ ╚████║
    //  ╚═╝     ╚═╝ ╚═════╝    ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝
    //                                                     
    /// <summary>
    /// An Action to change current MotionType.
    /// </summary>
    public class ActionMotion : Action
    {
        public MotionType motionType;
        
        public ActionMotion(MotionType motionType) : base()
        {
            type = ActionType.Motion;

            this.motionType = motionType;
        }

        public override string ToString()
        {
            return string.Format("Set motion type to '{0}'", motionType);
        }
    }

    //   ██████╗ ██████╗  ██████╗ ██████╗ ██████╗ ██╗███╗   ██╗ █████╗ ████████╗███████╗███████╗
    //  ██╔════╝██╔═══██╗██╔═══██╗██╔══██╗██╔══██╗██║████╗  ██║██╔══██╗╚══██╔══╝██╔════╝██╔════╝
    //  ██║     ██║   ██║██║   ██║██████╔╝██║  ██║██║██╔██╗ ██║███████║   ██║   █████╗  ███████╗
    //  ██║     ██║   ██║██║   ██║██╔══██╗██║  ██║██║██║╚██╗██║██╔══██║   ██║   ██╔══╝  ╚════██║
    //  ╚██████╗╚██████╔╝╚██████╔╝██║  ██║██████╔╝██║██║ ╚████║██║  ██║   ██║   ███████╗███████║
    //   ╚═════╝ ╚═════╝  ╚═════╝ ╚═╝  ╚═╝╚═════╝ ╚═╝╚═╝  ╚═══╝╚═╝  ╚═╝   ╚═╝   ╚══════╝╚══════╝
    //                                                                                          
    /// <summary>
    /// An Action to change current Reference Coordinate System.
    /// </summary>
    public class ActionCoordinates : Action
    {
        public ReferenceCS referenceCS;

        public ActionCoordinates(ReferenceCS referenceCS) : base()
        {
            type = ActionType.Coordinates;

            this.referenceCS = referenceCS;
        }

        public override string ToString()
        {
            return string.Format("Set reference coordinate system to '{0}'", referenceCS);
        }
    }

    //  ██████╗ ██╗   ██╗███████╗██╗  ██╗      ██████╗  ██████╗ ██████╗ 
    //  ██╔══██╗██║   ██║██╔════╝██║  ██║      ██╔══██╗██╔═══██╗██╔══██╗
    //  ██████╔╝██║   ██║███████╗███████║█████╗██████╔╝██║   ██║██████╔╝
    //  ██╔═══╝ ██║   ██║╚════██║██╔══██║╚════╝██╔═══╝ ██║   ██║██╔═══╝ 
    //  ██║     ╚██████╔╝███████║██║  ██║      ██║     ╚██████╔╝██║     
    //  ╚═╝      ╚═════╝ ╚══════╝╚═╝  ╚═╝      ╚═╝      ╚═════╝ ╚═╝     
    //                                                                  
    /// <summary>
    /// An Action to Push or Pop current device settings (such as speed, zone, etc.)
    /// </summary>
    public class ActionPushPop: Action
    {
        public bool push;  // is this push or pop?

        public ActionPushPop(bool push) : base()
        {
            type = ActionType.PushPop;

            this.push = push;
        }

        public override string ToString()
        {
            return push ?
                "Push settings to buffer" :
                "Pop settings";
        }
    }











    //  ████████╗██████╗  █████╗ ███╗   ██╗███████╗██╗      █████╗ ████████╗██╗ ██████╗ ███╗   ██╗
    //  ╚══██╔══╝██╔══██╗██╔══██╗████╗  ██║██╔════╝██║     ██╔══██╗╚══██╔══╝██║██╔═══██╗████╗  ██║
    //     ██║   ██████╔╝███████║██╔██╗ ██║███████╗██║     ███████║   ██║   ██║██║   ██║██╔██╗ ██║
    //     ██║   ██╔══██╗██╔══██║██║╚██╗██║╚════██║██║     ██╔══██║   ██║   ██║██║   ██║██║╚██╗██║
    //     ██║   ██║  ██║██║  ██║██║ ╚████║███████║███████╗██║  ██║   ██║   ██║╚██████╔╝██║ ╚████║
    //     ╚═╝   ╚═╝  ╚═╝╚═╝  ╚═╝╚═╝  ╚═══╝╚══════╝╚══════╝╚═╝  ╚═╝   ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝
    //                                                                                            
    /// <summary>
    /// An action representing a Translation transform in along a guiding vector.
    /// </summary>
    public class ActionTranslation : Action
    {

        public Vector translation;
        public bool relative;

        /// <summary>
        /// Full constructor.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="trans"></param>
        /// <param name="relTrans"></param>
        /// <param name="speed"></param>
        /// <param name="zone"></param>
        /// <param name="mType"></param>
        public ActionTranslation(Vector trans, bool relTrans) : base()
        {
            type = ActionType.Translation;

            translation = new Vector(trans);  // shallow copy
            relative = relTrans;
        }

        public override string ToString()
        {
            return relative ?
                string.Format("Move {0} mm", translation) :
                string.Format("Move to {0} mm", translation);
        }
    }


    //  ██████╗  ██████╗ ████████╗ █████╗ ████████╗██╗ ██████╗ ███╗   ██╗
    //  ██╔══██╗██╔═══██╗╚══██╔══╝██╔══██╗╚══██╔══╝██║██╔═══██╗████╗  ██║
    //  ██████╔╝██║   ██║   ██║   ███████║   ██║   ██║██║   ██║██╔██╗ ██║
    //  ██╔══██╗██║   ██║   ██║   ██╔══██║   ██║   ██║██║   ██║██║╚██╗██║
    //  ██║  ██║╚██████╔╝   ██║   ██║  ██║   ██║   ██║╚██████╔╝██║ ╚████║
    //  ╚═╝  ╚═╝ ╚═════╝    ╚═╝   ╚═╝  ╚═╝   ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝
    //                                                                   
    /// <summary>
    /// An Action representing a Rotation transformation in Quaternion represnetation.
    /// </summary>
    public class ActionRotation : Action
    {
        public Rotation rotation;
        public bool relative;
            
        public ActionRotation(Rotation rot, bool relRot) : base()
        {
            type = ActionType.Rotation;

            rotation = new Rotation(rot);  // shallow copy
            relative = relRot;

        }

        public override string ToString()
        {
            return relative ?
                string.Format("Rotate {0} deg around {1}", rotation.Angle, rotation.Axis) :
                string.Format("Rotate to {0}", new Orientation(rotation));
        }

    }


    //  ████████╗██████╗  █████╗ ███╗   ██╗███████╗███████╗ ██████╗ ██████╗ ███╗   ███╗ █████╗ ████████╗██╗ ██████╗ ███╗   ██╗
    //  ╚══██╔══╝██╔══██╗██╔══██╗████╗  ██║██╔════╝██╔════╝██╔═══██╗██╔══██╗████╗ ████║██╔══██╗╚══██╔══╝██║██╔═══██╗████╗  ██║
    //     ██║   ██████╔╝███████║██╔██╗ ██║███████╗█████╗  ██║   ██║██████╔╝██╔████╔██║███████║   ██║   ██║██║   ██║██╔██╗ ██║
    //     ██║   ██╔══██╗██╔══██║██║╚██╗██║╚════██║██╔══╝  ██║   ██║██╔══██╗██║╚██╔╝██║██╔══██║   ██║   ██║██║   ██║██║╚██╗██║
    //     ██║   ██║  ██║██║  ██║██║ ╚████║███████║██║     ╚██████╔╝██║  ██║██║ ╚═╝ ██║██║  ██║   ██║   ██║╚██████╔╝██║ ╚████║
    //     ╚═╝   ╚═╝  ╚═╝╚═╝  ╚═╝╚═╝  ╚═══╝╚══════╝╚═╝      ╚═════╝ ╚═╝  ╚═╝╚═╝     ╚═╝╚═╝  ╚═╝   ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝
    //                                                                                                                         
    /// <summary>
    /// An Action representing a combined Translation and Rotation Transformation.
    /// </summary>
    public class ActionTransformation : Action
    {
        public Vector translation;
        public Rotation rotation;
        public bool relative;
        public bool translationFirst;  // for relative transforms, translate or rotate first?

        public ActionTransformation(Vector translation, Rotation rotation, bool relative, bool translationFirst) : base()
        {
            type = ActionType.Transformation;

            this.translation = new Vector(translation);  // shallow copy
            this.rotation = new Rotation(rotation);  // shallow copy
            this.relative = relative;
            this.translationFirst = translationFirst;

        }

        public override string ToString()
        {
            string str; 
            if (relative)
            {
                if (translationFirst)
                    str = string.Format("Transform: move {0} mm and rotate {1} deg around {2}", translation, rotation.Angle, rotation.Axis);
                else 
                    str = string.Format("Transform: rotate {0} deg around {1} and move {2} mm", rotation.Angle, rotation.Axis, translation);
            }
            else
            {
                str = string.Format("Transform: move to {0} mm and rotate to {1}", translation, new Orientation(rotation));
            }
            return str;
        }
    }





    //       ██╗ ██████╗ ██╗███╗   ██╗████████╗███████╗
    //       ██║██╔═══██╗██║████╗  ██║╚══██╔══╝██╔════╝
    //       ██║██║   ██║██║██╔██╗ ██║   ██║   ███████╗
    //  ██   ██║██║   ██║██║██║╚██╗██║   ██║   ╚════██║
    //  ╚█████╔╝╚██████╔╝██║██║ ╚████║   ██║   ███████║
    //   ╚════╝  ╚═════╝ ╚═╝╚═╝  ╚═══╝   ╚═╝   ╚══════╝
    //                                                 
    /// <summary>
    /// An Action representing the raw angular values of the device's joint rotations.
    /// </summary>
    public class ActionAxes : Action
    {
        public Joints joints;
        public bool relative;

        //public ActionJoints(double j1, double j2, double j3, double j4, double j5, double j6, bool relative) : base()
        //{
        //    type = ActionType.Joints;

        //    this.joints = new Joints(j1, j2, j3, j4, j5, j6);
        //    this.relative = relative;
        //}

        //public ActionJoints(Joints joints, bool relative)
        //    : this(joints.J1, joints.J2, joints.J3, joints.J4, joints.J5, joints.J6, relative) { }  // shallow copy

        public ActionAxes(Joints joints, bool relative)
        {
            type = ActionType.Axes;

            this.joints = new Joints(joints);  // shallow copy
            this.relative = relative;
        }

        public override string ToString() 
        {
            return relative ?
                string.Format("Increase joint rotations by {0} deg", joints) :
                string.Format("Set joint rotations to {0} deg", joints);
        }
    }

    //  ███╗   ███╗███████╗███████╗███████╗ █████╗  ██████╗ ███████╗
    //  ████╗ ████║██╔════╝██╔════╝██╔════╝██╔══██╗██╔════╝ ██╔════╝
    //  ██╔████╔██║█████╗  ███████╗███████╗███████║██║  ███╗█████╗  
    //  ██║╚██╔╝██║██╔══╝  ╚════██║╚════██║██╔══██║██║   ██║██╔══╝  
    //  ██║ ╚═╝ ██║███████╗███████║███████║██║  ██║╚██████╔╝███████╗
    //  ╚═╝     ╚═╝╚══════╝╚══════╝╚══════╝╚═╝  ╚═╝ ╚═════╝ ╚══════╝
    //                                                              
    /// <summary>
    /// An Action representing a string message sent to the device to be displayed.
    /// </summary>
    public class ActionMessage : Action
    {
        public string message;

        public ActionMessage(string message) : base()
        {
            this.type = ActionType.Message;

            this.message = message;
        }

        public override string ToString()
        {
            return string.Format("Display message \"{0}\"", message);
        }
    }


    //  ██╗    ██╗ █████╗ ██╗████████╗
    //  ██║    ██║██╔══██╗██║╚══██╔══╝
    //  ██║ █╗ ██║███████║██║   ██║   
    //  ██║███╗██║██╔══██║██║   ██║   
    //  ╚███╔███╔╝██║  ██║██║   ██║   
    //   ╚══╝╚══╝ ╚═╝  ╚═╝╚═╝   ╚═╝   
    //                                
    /// <summary>
    /// An Action represening the device staying idle for a period of time.
    /// </summary>
    public class ActionWait : Action
    {
        public long millis;

        public ActionWait(long millis) : base()
        {
            type = ActionType.Wait;

            this.millis = millis;
        }

        public override string ToString()
        {
            return string.Format("Wait {0} ms", millis);
        }
    }



    //   ██████╗ ██████╗ ███╗   ███╗███╗   ███╗███████╗███╗   ██╗████████╗
    //  ██╔════╝██╔═══██╗████╗ ████║████╗ ████║██╔════╝████╗  ██║╚══██╔══╝
    //  ██║     ██║   ██║██╔████╔██║██╔████╔██║█████╗  ██╔██╗ ██║   ██║   
    //  ██║     ██║   ██║██║╚██╔╝██║██║╚██╔╝██║██╔══╝  ██║╚██╗██║   ██║   
    //  ╚██████╗╚██████╔╝██║ ╚═╝ ██║██║ ╚═╝ ██║███████╗██║ ╚████║   ██║   
    //   ╚═════╝ ╚═════╝ ╚═╝     ╚═╝╚═╝     ╚═╝╚══════╝╚═╝  ╚═══╝   ╚═╝   
    //
    /// <summary>
    /// Adds a line comment to the compiled code
    /// </summary>
    public class ActionComment : Action
    {
        public string comment;

        public ActionComment(string comment) : base()
        {
            type = ActionType.Comment;

            this.comment = comment;
        }

        public override string ToString()
        {
            return string.Format("Comment: \"{0}\"", comment);
        }
    }




    //  ██████╗ ███████╗    ██╗ █████╗ ████████╗████████╗ █████╗  ██████╗██╗  ██╗
    //  ██╔══██╗██╔════╝   ██╔╝██╔══██╗╚══██╔══╝╚══██╔══╝██╔══██╗██╔════╝██║  ██║
    //  ██║  ██║█████╗    ██╔╝ ███████║   ██║      ██║   ███████║██║     ███████║
    //  ██║  ██║██╔══╝   ██╔╝  ██╔══██║   ██║      ██║   ██╔══██║██║     ██╔══██║
    //  ██████╔╝███████╗██╔╝   ██║  ██║   ██║      ██║   ██║  ██║╚██████╗██║  ██║
    //  ╚═════╝ ╚══════╝╚═╝    ╚═╝  ╚═╝   ╚═╝      ╚═╝   ╚═╝  ╚═╝ ╚═════╝╚═╝  ╚═╝
    //                                                                           
    /// <summary>
    /// Attaches a Tool to the robot flange. 
    /// If the robot already had a tool, this will be substituted.
    /// </summary>
    public class ActionAttach : Action
    {
        public Tool tool;
        internal bool translationFirst;

        public ActionAttach(Tool tool) : base()
        {
            type = ActionType.Attach;

            this.tool = tool;
            this.translationFirst = tool.translationFirst;
        }

        public override string ToString()
        {
            return string.Format("Attach tool \"{0}\"", this.tool.name);
        }
    }

    /// <summary>
    /// Detaches any tool currently attached to the robot.
    /// </summary>
    public class ActionDetach : Action
    {
        public ActionDetach() : base()
        {
            type = ActionType.Detach;
        }

        public override string ToString()
        {
            return "Detach all tools";
        }
    }





    //  ██╗    ██╗ ██████╗ 
    //  ██║   ██╔╝██╔═══██╗
    //  ██║  ██╔╝ ██║   ██║
    //  ██║ ██╔╝  ██║   ██║
    //  ██║██╔╝   ╚██████╔╝
    //  ╚═╝╚═╝     ╚═════╝ 
    //                     
    /// <summary>
    /// Turns digital pin # on or off.
    /// </summary>
    public class ActionIODigital : Action
    {
        public int pin;
        public bool on;

        public ActionIODigital(int pinNum, bool isOn) : base()
        {
            this.type = ActionType.IODigital;

            this.pin = pinNum;
            this.on = isOn; 
        }

        public override string ToString()
        {
            return string.Format("Turn digital IO {0} {1}",
                this.pin,
                this.on ? "ON" : "OFF");
        }
    }

    /// <summary>
    /// Writes a value to analog pin #.
    /// </summary>
    public class ActionIOAnalog : Action
    {
        public int pin;
        public double value;

        public ActionIOAnalog(int pinNum, double value) : base()
        {
            this.type = ActionType.IOAnalog;

            this.pin = pinNum;
            this.value = value;
        }

        public override string ToString()
        {
            return string.Format("Set analog IO {0} to {1}",
                this.pin,
                this.value);
        }
    }



    //  ████████╗███████╗███╗   ███╗██████╗ ███████╗██████╗  █████╗ ████████╗██╗   ██╗██████╗ ███████╗
    //  ╚══██╔══╝██╔════╝████╗ ████║██╔══██╗██╔════╝██╔══██╗██╔══██╗╚══██╔══╝██║   ██║██╔══██╗██╔════╝
    //     ██║   █████╗  ██╔████╔██║██████╔╝█████╗  ██████╔╝███████║   ██║   ██║   ██║██████╔╝█████╗  
    //     ██║   ██╔══╝  ██║╚██╔╝██║██╔═══╝ ██╔══╝  ██╔══██╗██╔══██║   ██║   ██║   ██║██╔══██╗██╔══╝  
    //     ██║   ███████╗██║ ╚═╝ ██║██║     ███████╗██║  ██║██║  ██║   ██║   ╚██████╔╝██║  ██║███████╗
    //     ╚═╝   ╚══════╝╚═╝     ╚═╝╚═╝     ╚══════╝╚═╝  ╚═╝╚═╝  ╚═╝   ╚═╝    ╚═════╝ ╚═╝  ╚═╝╚══════╝
    //                                                                                                
    /// <summary>
    /// Sets the temperature of the 3D printer part, and optionally waits for the part to reach the temp to resume eexecution.
    /// </summary>
    public class ActionTemperature : Action
    {
        public double temperature;
        public RobotPart robotPart;
        public bool wait;
        public bool relative;

        public ActionTemperature(double temperature, RobotPart robotPart, bool wait, bool relative) : base()
        {
            this.type = ActionType.Temperature;

            this.temperature = temperature;
            this.robotPart = robotPart;
            this.wait = wait; 
            this.relative = relative;
        }

        public override string ToString()
        {
            if (relative)
            {
                return string.Format("{0} {1} temperature by {2} °C{3}",
                    this.temperature < 0 ? "Decrease" : "Increase",
                    Enum.GetName(typeof(RobotPart), this.robotPart),
                    this.temperature,
                    this.wait ? " and wait" : "");
            }
            else
            {
                return string.Format("Set {0} temperature to {1} °C{2}",
                    Enum.GetName(typeof(RobotPart), this.robotPart),
                    this.temperature,
                    this.wait ? " and wait" : "");
            }
        }
    }

    

    //  ███████╗██╗  ██╗████████╗██████╗ ██╗   ██╗███████╗██╗ ██████╗ ███╗   ██╗
    //  ██╔════╝╚██╗██╔╝╚══██╔══╝██╔══██╗██║   ██║██╔════╝██║██╔═══██╗████╗  ██║
    //  █████╗   ╚███╔╝    ██║   ██████╔╝██║   ██║███████╗██║██║   ██║██╔██╗ ██║
    //  ██╔══╝   ██╔██╗    ██║   ██╔══██╗██║   ██║╚════██║██║██║   ██║██║╚██╗██║
    //  ███████╗██╔╝ ██╗   ██║   ██║  ██║╚██████╔╝███████║██║╚██████╔╝██║ ╚████║
    //  ╚══════╝╚═╝  ╚═╝   ╚═╝   ╚═╝  ╚═╝ ╚═════╝ ╚══════╝╚═╝ ╚═════╝ ╚═╝  ╚═══╝
    //
    /// <summary>
    /// Turns extrusion on/off in 3D printers.
    /// </summary>
    public class ActionExtrusion : Action
    {
        public bool extrude;

        public ActionExtrusion(bool extrude)
        {
            this.type = ActionType.Extrusion;

            this.extrude = extrude; 
        }

        public override string ToString()
        {
            return $"Turn extrusion {(this.extrude ? "on" : "off")}";
        }
    }

    /// <summary>
    /// Sets the extrusion rate in 3D printers in mm of filament per mm of lineal travel.
    /// </summary>
    public class ActionExtrusionRate : Action
    {
        public double rate;
        public bool relative;

        public ActionExtrusionRate(double rate, bool relative) : base()
        {
            this.type = ActionType.ExtrusionRate;

            this.rate = rate;
            this.relative = relative;
        }

        public override string ToString()
        {
            return this.relative ? 
                $"{(this.rate < 0 ? "Decrease" : "Increase")} feed rate by {this.rate} mm/s" :
                $"Set feed rate to {this.rate} mm/s";
        }
    }


    //  ██╗███╗   ██╗██╗████████╗██╗ █████╗ ██╗     ██╗███████╗ █████╗ ████████╗██╗ ██████╗ ███╗   ██╗
    //  ██║████╗  ██║██║╚══██╔══╝██║██╔══██╗██║     ██║╚══███╔╝██╔══██╗╚══██╔══╝██║██╔═══██╗████╗  ██║
    //  ██║██╔██╗ ██║██║   ██║   ██║███████║██║     ██║  ███╔╝ ███████║   ██║   ██║██║   ██║██╔██╗ ██║
    //  ██║██║╚██╗██║██║   ██║   ██║██╔══██║██║     ██║ ███╔╝  ██╔══██║   ██║   ██║██║   ██║██║╚██╗██║
    //  ██║██║ ╚████║██║   ██║   ██║██║  ██║███████╗██║███████╗██║  ██║   ██║   ██║╚██████╔╝██║ ╚████║
    //  ╚═╝╚═╝  ╚═══╝╚═╝   ╚═╝   ╚═╝╚═╝  ╚═╝╚══════╝╚═╝╚══════╝╚═╝  ╚═╝   ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝
    //                                                                                                

    public class ActionInitialization : Action
    {
        public bool initialize;

        public ActionInitialization(bool initialize) : base()
        {
            this.type = ActionType.Initialization;

            this.initialize = initialize;
        }

        public override string ToString()
        {
            return $"{(this.initialize ? "Initialize" : "Terminate")} this device.";
        }
    }


}
