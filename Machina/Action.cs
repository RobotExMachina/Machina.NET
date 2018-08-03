using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Machina.Types;

namespace Machina
{

    /// <summary>
    /// Defines an Action Type, like Translation, Rotation, Wait... 
    /// Useful to flag base Actions into children classes.
    /// </summary>
    public enum ActionType
    {
        Undefined,
        Translation,
        Rotation,
        Transformation,
        Axes,
        Message,
        Wait,
        Speed,
        Acceleration,
        RotationSpeed,
        JointSpeed, 
        JointAcceleration,
        Precision,
        Motion,
        Coordinates,
        PushPop, 
        Comment,
        Attach,
        Detach,
        IODigital,
        IOAnalog, 
        Temperature,
        Extrusion,
        ExtrusionRate,
        Initialization, 
        ExternalAxes
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
        public static ActionAcceleration Acceleration(double accInc)
        {
            return new ActionAcceleration(accInc, true);
        }

        public static ActionAcceleration AccelerationTo(double acc)
        {
            return new ActionAcceleration(acc, false);
        }

        public static ActionSpeed Speed(double speedInc)
        {
            return new ActionSpeed(speedInc, true);
        }

        public static ActionSpeed SpeedTo(double speed)
        {
            return new ActionSpeed(speed, false);
        }

        public static ActionRotationSpeed RotationSpeed(double rotSpeedInc) 
        {
            return new ActionRotationSpeed(rotSpeedInc, true);
        }

        public static ActionRotationSpeed RotationSpeedTo(double rotSpeed)
        {
            return new ActionRotationSpeed(rotSpeed, false);
        }

        public static ActionJointSpeed JointSpeed(double jointSpeedInc)
        {
            return new ActionJointSpeed(jointSpeedInc, true);
        }

        public static ActionJointSpeed JointSpeedTo(double jointSpeed)
        {
            return new ActionJointSpeed(jointSpeed, false);
        }

        public static ActionJointAcceleration JointAcceleration(double jointAccelerationInc)
        {
            return new ActionJointAcceleration(jointAccelerationInc, true);
        }

        public static ActionJointAcceleration JointAccelerationTo(double jointAcceleration)
        {
            return new ActionJointAcceleration(jointAcceleration, false);
        }

        public static ActionPrecision Precision(double precisionInc)
        {
            return new ActionPrecision(precisionInc, true);
        }

        public static ActionPrecision PrecisionTo(double precision)
        {
            return new ActionPrecision(precision, false);
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

        public static ActionAxes Axes(Joints jointsInc)
        {
            return new ActionAxes(jointsInc, true);
        }

        public static ActionAxes AxesTo(Joints joints)
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

        public static ActionIODigital WriteDigital(string pinName, bool isOn, bool toolPin)
        {
            return new ActionIODigital(pinName, isOn, toolPin);
        }

        public static ActionIOAnalog WriteAnalog(string pinName, double value, bool toolPin)
        {
            return new ActionIOAnalog(pinName, value, toolPin);
        }

        public static ActionTemperature Temperature(double temp, RobotPartType devicePart, bool wait, bool relative)
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

        public static ActionExternalAxes ExternalAxes(double? a1, double? a2, double? a3, double? a4, double? a5, double? a6, bool relative)
        {
            return new ActionExternalAxes(a1, a2, a3, a4, a5, a6, relative);
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

        /// <summary>
        /// Generates a string representing a "serialized" instruction representing the 
        /// Machina-API command that would have generated this action. 
        /// Useful for generating actions to send to the Bridge.
        /// </summary>
        /// <returns></returns>
        public virtual string ToInstruction() => null;
    }




    //   █████╗  ██████╗ ██████╗███████╗██╗     ███████╗██████╗  █████╗ ████████╗██╗ ██████╗ ███╗   ██╗
    //  ██╔══██╗██╔════╝██╔════╝██╔════╝██║     ██╔════╝██╔══██╗██╔══██╗╚══██╔══╝██║██╔═══██╗████╗  ██║
    //  ███████║██║     ██║     █████╗  ██║     █████╗  ██████╔╝███████║   ██║   ██║██║   ██║██╔██╗ ██║
    //  ██╔══██║██║     ██║     ██╔══╝  ██║     ██╔══╝  ██╔══██╗██╔══██║   ██║   ██║██║   ██║██║╚██╗██║
    //  ██║  ██║╚██████╗╚██████╗███████╗███████╗███████╗██║  ██║██║  ██║   ██║   ██║╚██████╔╝██║ ╚████║
    //  ╚═╝  ╚═╝ ╚═════╝ ╚═════╝╚══════╝╚══════╝╚══════╝╚═╝  ╚═╝╚═╝  ╚═╝   ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝
    //                                                                                                 
    public class ActionAcceleration : Action
    {
        public double acceleration;
        public bool relative;

        public ActionAcceleration(double acc, bool relative) : base()
        {
            this.type = ActionType.Acceleration;

            this.acceleration = acc;
            this.relative = relative;
        }

        public override string ToString()
        {
            return relative ?
                string.Format("{0} TCP acceleration by {1} mm/s^2", this.acceleration < 0 ? "Decrease" : "Increase", this.acceleration) :
                string.Format("Set TCP acceleration to {0} mm/s^2", this.acceleration);
        }

        public override string ToInstruction()
        {
            return relative ?
                $"Acceleration({this.acceleration});" :
                $"AccelerationTo({this.acceleration});";
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
        public double speed;
        public bool relative;

        public ActionSpeed(double speed, bool relative) : base()
        {
            this.type = ActionType.Speed;

            this.speed = speed;
            this.relative = relative;
        }

        public override string ToString()
        {
            return relative ?
                string.Format("{0} TCP speed by {1} mm/s", this.speed < 0 ? "Decrease" : "Increase", speed) :
                string.Format("Set TCP speed to {0} mm/s", speed);
        }

        public override string ToInstruction()
        {
            return relative ?
                $"Speed({this.speed});" :
                $"SpeedTo({this.speed});";
        }
    }





    //  ██████╗  ██████╗ ████████╗ █████╗ ████████╗██╗ ██████╗ ███╗   ██╗    ███████╗██████╗ ███████╗███████╗██████╗ 
    //  ██╔══██╗██╔═══██╗╚══██╔══╝██╔══██╗╚══██╔══╝██║██╔═══██╗████╗  ██║    ██╔════╝██╔══██╗██╔════╝██╔════╝██╔══██╗
    //  ██████╔╝██║   ██║   ██║   ███████║   ██║   ██║██║   ██║██╔██╗ ██║    ███████╗██████╔╝█████╗  █████╗  ██║  ██║
    //  ██╔══██╗██║   ██║   ██║   ██╔══██║   ██║   ██║██║   ██║██║╚██╗██║    ╚════██║██╔═══╝ ██╔══╝  ██╔══╝  ██║  ██║
    //  ██║  ██║╚██████╔╝   ██║   ██║  ██║   ██║   ██║╚██████╔╝██║ ╚████║    ███████║██║     ███████╗███████╗██████╔╝
    //  ╚═╝  ╚═╝ ╚═════╝    ╚═╝   ╚═╝  ╚═╝   ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝    ╚══════╝╚═╝     ╚══════╝╚══════╝╚═════╝ 
    //                                                                                                               
    public class ActionRotationSpeed : Action
    {
        public double rotationSpeed;
        public bool relative;

        public ActionRotationSpeed(double rotSpeed, bool rel) : base()
        {
            this.type = ActionType.RotationSpeed;

            this.rotationSpeed = rotSpeed;
            this.relative = rel;
        }

        public override string ToString()
        {
            return relative ?
                string.Format("{0} TCP rotation speed by {1} mm/s", this.rotationSpeed < 0 ? "Decrease" : "Increase", this.rotationSpeed) :
                string.Format("Set TCP rotation speed to {0} mm/s", this.rotationSpeed);
        }

        public override string ToInstruction()
        {
            return relative ?
                $"RotationSpeed({this.rotationSpeed});" :
                $"RotationSpeedTo({this.rotationSpeed});";
        }
    }



    //       ██╗ ██████╗ ██╗███╗   ██╗████████╗    ███████╗██████╗   ██╗ █████╗  ██████╗ ██████╗
    //       ██║██╔═══██╗██║████╗  ██║╚══██╔══╝    ██╔════╝██╔══██╗ ██╔╝██╔══██╗██╔════╝██╔════╝
    //       ██║██║   ██║██║██╔██╗ ██║   ██║       ███████╗██████╔╝██╔╝ ███████║██║     ██║     
    //  ██   ██║██║   ██║██║██║╚██╗██║   ██║       ╚════██║██╔═══╝██╔╝  ██╔══██║██║     ██║     
    //  ╚█████╔╝╚██████╔╝██║██║ ╚████║   ██║       ███████║██║   ██╔╝   ██║  ██║╚██████╗╚██████╗
    //   ╚════╝  ╚═════╝ ╚═╝╚═╝  ╚═══╝   ╚═╝       ╚══════╝╚═╝   ╚═╝    ╚═╝  ╚═╝ ╚═════╝ ╚═════╝
    //                                                                                          
    public class ActionJointSpeed : Action
    {
        public double jointSpeed;
        public bool relative;

        public ActionJointSpeed(double jointSpeed, bool rel) : base()
        {
            this.type = ActionType.JointSpeed;

            this.jointSpeed = jointSpeed;
            this.relative = rel;
        }

        public override string ToString()
        {
            return relative ?
                string.Format("{0} joint speed by {1} deg/s", this.jointSpeed < 0 ? "Decrease" : "Increase", this.jointSpeed) :
                string.Format("Set joint speed to {0} deg/s", this.jointSpeed);
        }

        public override string ToInstruction()
        {
            return relative ?
                $"JointSpeed({this.jointSpeed});" :
                $"JointSpeedTo({this.jointSpeed});";
        }
    }

    public class ActionJointAcceleration : Action
    {
        public double jointAcceleration;
        public bool relative;

        public ActionJointAcceleration(double jointAcceleration, bool rel) : base()
        {
            this.type = ActionType.JointAcceleration;

            this.jointAcceleration = jointAcceleration;
            this.relative = rel;
        }

        public override string ToString()
        {
            return relative ?
                string.Format("{0} joint acceleration by {1} deg/s^2", this.jointAcceleration < 0 ? "Decrease" : "Increase", this.jointAcceleration) :
                string.Format("Set joint acceleration to {0} deg/s^2", this.jointAcceleration);
        }

        public override string ToInstruction()
        {
            return relative ?
                $"JointAcceleration({this.jointAcceleration});" :
                $"JointAccelerationTo({this.jointAcceleration});";
        }
    }



    //  ██████╗ ██████╗ ███████╗ ██████╗██╗███████╗██╗ ██████╗ ███╗   ██╗
    //  ██╔══██╗██╔══██╗██╔════╝██╔════╝██║██╔════╝██║██╔═══██╗████╗  ██║
    //  ██████╔╝██████╔╝█████╗  ██║     ██║███████╗██║██║   ██║██╔██╗ ██║
    //  ██╔═══╝ ██╔══██╗██╔══╝  ██║     ██║╚════██║██║██║   ██║██║╚██╗██║
    //  ██║     ██║  ██║███████╗╚██████╗██║███████║██║╚██████╔╝██║ ╚████║
    //  ╚═╝     ╚═╝  ╚═╝╚══════╝ ╚═════╝╚═╝╚══════╝╚═╝ ╚═════╝ ╚═╝  ╚═══╝
    //                                                                   
    /// <summary>
    /// An Action to change current precision settings.
    /// </summary>
    public class ActionPrecision : Action
    {
        public double precision;
        public bool relative;

        public ActionPrecision(double value, bool relative) : base()
        {
            this.type = ActionType.Precision;

            this.precision = value;
            this.relative = relative;
        }

        public override string ToString()
        {
            return relative ?
                string.Format("{0} precision radius by {1} mm", this.precision < 0 ? "Decrease" : "Increase", this.precision) :
                string.Format("Set precision radius to {0} mm", this.precision);
        }

        public override string ToInstruction()
        {
            return relative ?
                $"Precision({this.precision});" :
                $"PrecisionTo({this.precision});";
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
            this.type = ActionType.Motion;

            this.motionType = motionType;
        }

        public override string ToString()
        {
            return string.Format("Set motion type to '{0}'", motionType);
        }

        public override string ToInstruction()
        {
            return $"MotionMode(\"{(Enum.GetName(typeof(MotionType), this.motionType))}\");";
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
            this.type = ActionType.Coordinates;

            this.referenceCS = referenceCS;
        }

        public override string ToString()
        {
            return string.Format("Set reference coordinate system to '{0}'", referenceCS);
        }

        public override string ToInstruction() => null;
    }

    //  ██████╗ ██╗   ██╗███████╗██╗  ██╗      ██████╗  ██████╗ ██████╗ 
    //  ██╔══██╗██║   ██║██╔════╝██║  ██║      ██╔══██╗██╔═══██╗██╔══██╗
    //  ██████╔╝██║   ██║███████╗███████║█████╗██████╔╝██║   ██║██████╔╝
    //  ██╔═══╝ ██║   ██║╚════██║██╔══██║╚════╝██╔═══╝ ██║   ██║██╔═══╝ 
    //  ██║     ╚██████╔╝███████║██║  ██║      ██║     ╚██████╔╝██║     
    //  ╚═╝      ╚═════╝ ╚══════╝╚═╝  ╚═╝      ╚═╝      ╚═════╝ ╚═╝     
    //                                                                  
    /// <summary>
    /// An Action to Push or Pop current device settings (such as speed, precision, etc.)
    /// </summary>
    public class ActionPushPop: Action
    {
        public bool push;  // is this push or pop?

        public ActionPushPop(bool push) : base()
        {
            this.type = ActionType.PushPop;

            this.push = push;
        }

        public override string ToString()
        {
            return push ?
                "Push settings to buffer" :
                "Pop settings";
        }

        public override string ToInstruction()
        {
            return this.push ?
                "PushSettings()" :
                "PopSettings()";
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

        public ActionTranslation(Vector trans, bool relTrans) : base()
        {
            this.type = ActionType.Translation;

            this.translation = new Vector(trans);  // shallow copy
            this.relative = relTrans;
        }

        public override string ToString()
        {
            return relative ?
                string.Format("Move {0} mm", translation) :
                string.Format("Move to {0} mm", translation);
        }

        public override string ToInstruction()
        {
            return string.Format("{0}({1},{2},{3});",
                (this.relative ? "Move" : "MoveTo"),
                Math.Round(this.translation.X, Geometry.STRING_ROUND_DECIMALS_MM),
                Math.Round(this.translation.Y, Geometry.STRING_ROUND_DECIMALS_MM),
                Math.Round(this.translation.Z, Geometry.STRING_ROUND_DECIMALS_MM)
            );
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
            this.type = ActionType.Rotation;

            this.rotation = new Rotation(rot);  // shallow copy
            this.relative = relRot;
        }

        public override string ToString()
        {
            return relative ?
                string.Format("Rotate {0} deg around {1}", rotation.Angle, rotation.Axis) :
                string.Format("Rotate to {0}", new Orientation(rotation));
        }

        public override string ToInstruction()
        {
            if (this.relative)
            {
                return string.Format("Rotate({0},{1},{2},{3});",
                    Math.Round(this.rotation.AA.X, Geometry.STRING_ROUND_DECIMALS_VECTOR),
                    Math.Round(this.rotation.AA.Y, Geometry.STRING_ROUND_DECIMALS_VECTOR),
                    Math.Round(this.rotation.AA.Z, Geometry.STRING_ROUND_DECIMALS_VECTOR),
                    Math.Round(this.rotation.AA.Angle, Geometry.STRING_ROUND_DECIMALS_DEGS)
                );
            }

            // @TODO: improve this! 
            Orientation ori = new Orientation(this.rotation);
            return string.Format("RotateTo({0},{1},{2},{3},{4},{5});",
                Math.Round(ori.XAxis.X, Geometry.STRING_ROUND_DECIMALS_VECTOR),
                Math.Round(ori.XAxis.Y, Geometry.STRING_ROUND_DECIMALS_VECTOR),
                Math.Round(ori.XAxis.Z, Geometry.STRING_ROUND_DECIMALS_VECTOR),
                Math.Round(ori.YAxis.X, Geometry.STRING_ROUND_DECIMALS_VECTOR),
                Math.Round(ori.YAxis.Y, Geometry.STRING_ROUND_DECIMALS_VECTOR),
                Math.Round(ori.YAxis.Z, Geometry.STRING_ROUND_DECIMALS_VECTOR)
            );
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
            this.type = ActionType.Transformation;

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

        public override string ToInstruction()
        {
            if (this.relative)
            {
                return null;  // @TODO
            }

            Orientation ori = new Orientation(this.rotation);

            return string.Format("TransformTo({0},{1},{2},{3},{4},{5},{6},{7},{8});",
                    Math.Round(this.translation.X, Geometry.STRING_ROUND_DECIMALS_MM),
                    Math.Round(this.translation.Y, Geometry.STRING_ROUND_DECIMALS_MM),
                    Math.Round(this.translation.Z, Geometry.STRING_ROUND_DECIMALS_MM),
                    Math.Round(ori.XAxis.X, Geometry.STRING_ROUND_DECIMALS_MM),
                    Math.Round(ori.XAxis.Y, Geometry.STRING_ROUND_DECIMALS_MM),
                    Math.Round(ori.XAxis.Z, Geometry.STRING_ROUND_DECIMALS_MM),
                    Math.Round(ori.YAxis.X, Geometry.STRING_ROUND_DECIMALS_MM),
                    Math.Round(ori.YAxis.Y, Geometry.STRING_ROUND_DECIMALS_MM),
                    Math.Round(ori.YAxis.Z, Geometry.STRING_ROUND_DECIMALS_MM)
                );
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
        //    this.type = ActionType.Joints;

        //    this.joints = new Joints(j1, j2, j3, j4, j5, j6);
        //    this.relative = relative;
        //}

        //public ActionJoints(Joints joints, bool relative)
        //    : this(joints.J1, joints.J2, joints.J3, joints.J4, joints.J5, joints.J6, relative) { }  // shallow copy

        public ActionAxes(Joints joints, bool relative)
        {
            this.type = ActionType.Axes;

            this.joints = new Joints(joints);  // shallow copy
            this.relative = relative;
        }

        public override string ToString() 
        {
            return relative ?
                string.Format("Increase joint rotations by {0} deg", joints) :
                string.Format("Set joint rotations to {0} deg", joints);
        }

        public override string ToInstruction()
        {
            return string.Format("{0}({1},{2},{3},{4},{5},{6});",
                (this.relative ? "Axes" : "AxesTo"),
                Math.Round(this.joints.J1, Geometry.STRING_ROUND_DECIMALS_DEGS),
                Math.Round(this.joints.J2, Geometry.STRING_ROUND_DECIMALS_DEGS),
                Math.Round(this.joints.J3, Geometry.STRING_ROUND_DECIMALS_DEGS),
                Math.Round(this.joints.J4, Geometry.STRING_ROUND_DECIMALS_DEGS),
                Math.Round(this.joints.J5, Geometry.STRING_ROUND_DECIMALS_DEGS),
                Math.Round(this.joints.J6, Geometry.STRING_ROUND_DECIMALS_DEGS)
            );
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

        public override string ToInstruction()
        {
            return $"Message(\"{this.message}\");";
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
            this.type = ActionType.Wait;

            this.millis = millis;
        }

        public override string ToString()
        {
            return string.Format("Wait {0} ms", millis);
        }

        public override string ToInstruction()
        {
            return $"Wait({this.millis});";
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
            this.type = ActionType.Comment;

            this.comment = comment;
        }

        public override string ToString()
        {
            return string.Format("Comment: \"{0}\"", comment);
        }

        public override string ToInstruction()
        {
            return null;
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
            this.type = ActionType.Attach;

            this.tool = tool;
            this.translationFirst = tool.translationFirst;
        }

        public override string ToString()
        {
            return string.Format("Attach tool \"{0}\"", this.tool.name);
        }

        public override string ToInstruction()
        {
            return $"Attach(\"{this.tool.name}\");";
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

        public override string ToInstruction()
        {
            return $"Detach();";
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
        // See RobotCursor for string/int32 naming
        public string pinName;
        public bool on;
        public bool isDigit = false;
        public int pinNum = 0;
        public bool isToolPin = false;

        public ActionIODigital(string pin, bool isOn, bool toolPin) : base()
        {
            this.type = ActionType.IODigital;
            this.pinName = pin;
            this.on = isOn;
            this.isDigit = Int32.TryParse(this.pinName, out this.pinNum);
            this.isToolPin = toolPin;
        }

        public override string ToString()
        {
            return $"Turn {(this.isToolPin ? "tool " : "")}digital IO {(this.isDigit ? this.pinNum.ToString() : "\"" + this.pinName + "\"")} {(this.on ? "ON" : "OFF")}";
        }

        public override string ToInstruction()
        {
            return $"WriteDigital({(this.isDigit ? this.pinNum.ToString() : "\"" + this.pinName + "\"")},{this.on},{this.isToolPin});";
        }
    }

    /// <summary>
    /// Writes a value to analog pin #.
    /// </summary>
    public class ActionIOAnalog : Action
    {
        public string pinName;
        public double value;
        public bool isDigit = false;
        public int pinNum = 0;
        public bool isToolPin = false;

        public ActionIOAnalog(string pin, double value, bool toolPin) : base()
        {
            this.type = ActionType.IOAnalog;
            this.pinName = pin;
            this.value = value;
            this.isDigit = Int32.TryParse(this.pinName, out this.pinNum);
            this.isToolPin = toolPin;
        }

        public override string ToString()
        {
            //return string.Format("Set analog IO {0} to {1}",
            //    this.pinName,
            //    this.value);
            return $"Set {(this.isToolPin ? "tool " : "")}analog IO {(this.isDigit ? this.pinNum.ToString() : "\"" + this.pinName + "\"")} to {this.value}";
        }

        public override string ToInstruction()
        {
            return $"WriteAnalog({(this.isDigit ? this.pinNum.ToString() : "\"" + this.pinName + "\"")},{this.value},{this.isToolPin});";
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
        public RobotPartType robotPart;
        public bool wait;
        public bool relative;

        public ActionTemperature(double temperature, RobotPartType robotPart, bool wait, bool relative) : base()
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
                return string.Format("{0} {1} temperature by {2} C{3}",
                    this.temperature < 0 ? "Decrease" : "Increase",
                    Enum.GetName(typeof(RobotPartType), this.robotPart),
                    Math.Round(this.temperature, Geometry.STRING_ROUND_DECIMALS_TEMPERATURE),
                    this.wait ? " and wait" : "");
            }
            else
            {
                return string.Format("Set {0} temperature to {1} C{2}",
                    Enum.GetName(typeof(RobotPartType), this.robotPart),
                    Math.Round(this.temperature, Geometry.STRING_ROUND_DECIMALS_TEMPERATURE),
                    this.wait ? " and wait" : "");
            }
        }

        public override string ToInstruction() => null;
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

        public override string ToInstruction() => null;
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

        public override string ToInstruction() => null;
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

        public override string ToInstruction() => null;
    }



    //  ███████╗██╗  ██╗████████╗███████╗██████╗ ███╗   ██╗ █████╗ ██╗      █████╗ ██╗  ██╗███████╗███████╗
    //  ██╔════╝╚██╗██╔╝╚══██╔══╝██╔════╝██╔══██╗████╗  ██║██╔══██╗██║     ██╔══██╗╚██╗██╔╝██╔════╝██╔════╝
    //  █████╗   ╚███╔╝    ██║   █████╗  ██████╔╝██╔██╗ ██║███████║██║     ███████║ ╚███╔╝ █████╗  ███████╗
    //  ██╔══╝   ██╔██╗    ██║   ██╔══╝  ██╔══██╗██║╚██╗██║██╔══██║██║     ██╔══██║ ██╔██╗ ██╔══╝  ╚════██║
    //  ███████╗██╔╝ ██╗   ██║   ███████╗██║  ██║██║ ╚████║██║  ██║███████╗██║  ██║██╔╝ ██╗███████╗███████║
    //  ╚══════╝╚═╝  ╚═╝   ╚═╝   ╚══════╝╚═╝  ╚═╝╚═╝  ╚═══╝╚═╝  ╚═╝╚══════╝╚═╝  ╚═╝╚═╝  ╚═╝╚══════╝╚══════╝
    //                                                                                                     

    public class ActionExternalAxes : Action
    {
        public ExternalAxes externalAxes;
        public bool relative;

        public ActionExternalAxes(double? a1, double? a2, double? a3, double? a4, double? a5, double? a6, bool relative) : base()
        {
            this.type = ActionType.ExternalAxes;

            this.externalAxes = new ExternalAxes(a1, a2, a3, a4, a5, a6);

            this.relative = relative;
        }

        public override string ToString()
        {
            return relative ?
                $"Increase external axes by {this.externalAxes.ToArrayString()}" :
                $"Set external axes to {this.externalAxes.ToArrayString()}"; 
        }

        public override string ToInstruction()
        {
            string arr = this.externalAxes.ToArrayString();
            arr = arr.Substring(1, arr.Length - 2);
            return relative ?
                $"ExternalAxes({arr});" :
                $"ExternalAxesTo({arr});";
        }


            
    }
}
