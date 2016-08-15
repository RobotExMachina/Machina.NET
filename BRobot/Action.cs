using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRobot
{

    /// Defines an Action Type, like Translation, Rotation, Wait... 
    /// Useful to flag base Actions into children classes.
    /// </summary>
    internal enum ActionType : int
    {
        Undefined = 0,
        Translation = 1,
        Rotation = 2,
        Transformation = 3,
        Joints = 4,
        Message = 5,
        Wait = 6,
        Speed = 7,
        Zone = 8,
        Motion = 9,
        Coordinates = 10
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
    internal abstract class Action
    {
        public ActionType type = ActionType.Undefined;


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
    internal class ActionTranslation : Action
    {

        public Point translation;
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
        public ActionTranslation(Point trans, bool relTrans)
        {
            type = ActionType.Translation;

            translation = new Point(trans);  // shallow copy
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
    internal class ActionRotation : Action
    {
        public Rotation rotation;
        public bool relative;
            
        public ActionRotation(Rotation rot, bool relRot)
        {
            type = ActionType.Rotation;

            rotation = new Rotation(rot);  // shallow copy
            relative = relRot;

        }

        public override string ToString()
        {
            return relative ?
                string.Format("Rotate {0}° around {1}", rotation.GetRotationAngle(), rotation.GetRotationVector()) :
                string.Format("Rotate to {0}", rotation.GetCoordinateSystem());
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
    internal class ActionTransformation : Action
    {
        public Point translation;
        public Rotation rotation;
        public bool relative;
        public bool translationFirst;  // for relative transforms, translate or rotate first?

        public ActionTransformation(Point translation, Rotation rotation, bool relative, bool translationFirst)
        {
            type = ActionType.Transformation;

            this.translation = new Point(translation);
            this.rotation = new Rotation(rotation);
            this.relative = relative;
            this.translationFirst = translationFirst;

        }

        public override string ToString()
        {
            string str; 
            if (relative)
            {
                if (translationFirst)
                    str = string.Format("Move {0} mm and rotate {1}° around {2}", translation, rotation.GetRotationAngle(), rotation.GetRotationVector());
                else 
                    str = string.Format("Rotate {0}° around {1} and move {2} mm", rotation.GetRotationAngle(), rotation.GetRotationVector(), translation);
            }
            else
            {
                str = string.Format("Move to {0} mm and rotate to {1}", translation, rotation.GetCoordinateSystem());
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
    internal class ActionJoints : Action
    {
        public Joints joints;
        public bool relative;

        public ActionJoints(Joints joints, bool relative)
        {
            type = ActionType.Joints;

            this.joints = new Joints(joints);
            this.relative = relative;
        }

        public override string ToString()
        {
            return relative ?
                string.Format("Increase joint rotations by {0}°", joints) :
                string.Format("Set joint rotations to {0}°", joints);
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
    internal class ActionMessage : Action
    {
        public string message;

        public ActionMessage(string message)
        {
            type = ActionType.Message;

            this.message = message;
        }

        public override string ToString()
        {
            return string.Format("Send message \"{0}\"", message);
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
    internal class ActionWait : Action
    {
        public long millis;

        public ActionWait(long millis)
        {
            type = ActionType.Wait;

            this.millis = millis;
        }

        public override string ToString()
        {
            return string.Format("Wait {0} ms", millis);
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
    internal class ActionSpeed : Action
    {
        public int speed;
        public bool relative;

        public ActionSpeed(int speed, bool relative)
        {
            type = ActionType.Speed;

            this.speed = speed;
            this.relative = relative;
        }
    }


    //@TODO: add zone, motion and coordinates.














































    ///// <summary>
    ///// Defines an Action Type, like Translation, Rotation, Wait... 
    ///// Useful to flag base Actions into children classes.
    ///// </summary>
    //internal enum ActionType : int
    //{
    //    Undefined = 0,
    //    Translation = 1,
    //    Rotation = 2,
    //    TranslationAndRotation = 3,
    //    RotationAndTranslation = 4,
    //    Joints = 5,
    //    Message = 6, 
    //    Wait = 7
    //}

    ///// <summary>
    ///// If an Action implies movement, what type it is.
    ///// </summary>
    //public enum MotionType : int
    //{
    //    Undefined = 0,  // a null default
    //    Linear = 1,     // linear movement
    //    Joint = 2      // joint movement
    //}


    ////   █████╗  ██████╗████████╗██╗ ██████╗ ███╗   ██╗
    ////  ██╔══██╗██╔════╝╚══██╔══╝██║██╔═══██╗████╗  ██║
    ////  ███████║██║        ██║   ██║██║   ██║██╔██╗ ██║
    ////  ██╔══██║██║        ██║   ██║██║   ██║██║╚██╗██║
    ////  ██║  ██║╚██████╗   ██║   ██║╚██████╔╝██║ ╚████║
    ////  ╚═╝  ╚═╝ ╚═════╝   ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝
    ////                                                 
    ///// <summary>
    ///// Actions represent high-level abstract operations such as movements, rotations, 
    ///// transformations or joint manipulations, both in absolute and relative terms. 
    ///// They are independent from the device's properties, and their translation into
    ///// actual robotic instructions depends on the robot's properties and state. 
    ///// </summary>
    //internal class Action
    //{
    //    public ActionType type = ActionType.Undefined;

    //    // @TOTHINK: Wrap this into a Settings object instead?
    //    public int speed;
    //    public int zone;
    //    public MotionType motionType;

    //    // Translation properties
    //    public Point translation;
    //    public bool relativeTranslation;
    //    public bool worldTranslation;

    //    // Rotation properties
    //    public Rotation rotation;
    //    public bool relativeRotation;
    //    public bool worldRotation;

    //    // Joint properties
    //    public Joints joints;
    //    public bool relativeJoints;

    //    // Message properties
    //    public string message;

    //    // Wait properties
    //    public long waitMillis;
    //}




    ////  ████████╗██████╗  █████╗ ███╗   ██╗███████╗██╗      █████╗ ████████╗██╗ ██████╗ ███╗   ██╗
    ////  ╚══██╔══╝██╔══██╗██╔══██╗████╗  ██║██╔════╝██║     ██╔══██╗╚══██╔══╝██║██╔═══██╗████╗  ██║
    ////     ██║   ██████╔╝███████║██╔██╗ ██║███████╗██║     ███████║   ██║   ██║██║   ██║██╔██╗ ██║
    ////     ██║   ██╔══██╗██╔══██║██║╚██╗██║╚════██║██║     ██╔══██║   ██║   ██║██║   ██║██║╚██╗██║
    ////     ██║   ██║  ██║██║  ██║██║ ╚████║███████║███████╗██║  ██║   ██║   ██║╚██████╔╝██║ ╚████║
    ////     ╚═╝   ╚═╝  ╚═╝╚═╝  ╚═╝╚═╝  ╚═══╝╚══════╝╚══════╝╚═╝  ╚═╝   ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝
    ////                                                                                            
    ///// <summary>
    ///// An action representing a Translation transform in along a guiding vector.
    ///// </summary>
    //internal class ActionTranslation : Action
    //{
    //    /// <summary>
    //    /// Full constructor.
    //    /// </summary>
    //    /// <param name="world"></param>
    //    /// <param name="trans"></param>
    //    /// <param name="relTrans"></param>
    //    /// <param name="speed"></param>
    //    /// <param name="zone"></param>
    //    /// <param name="mType"></param>
    //    public ActionTranslation(bool world, Point trans, bool relTrans, int speed, int zone, MotionType mType)
    //    {
    //        type = ActionType.Translation;

    //        worldTranslation = world;
    //        translation = trans;
    //        relativeTranslation = relTrans;

    //        base.speed = speed;
    //        base.zone = zone;
    //        motionType = mType;
    //    }

    //    public override string ToString()
    //    {
    //        return string.Format("TRNS: {0}, {1} {2} {3}, v{4} z{5}",
    //            motionType == MotionType.Linear ? "lin" : motionType == MotionType.Joint ? "jnt" : "und",
    //            worldTranslation ? "globl" : "local",
    //            relativeTranslation ? "rel" : "abs",
    //            translation,
    //            speed,
    //            zone);
    //    }
    //}


    ////  ██████╗  ██████╗ ████████╗ █████╗ ████████╗██╗ ██████╗ ███╗   ██╗
    ////  ██╔══██╗██╔═══██╗╚══██╔══╝██╔══██╗╚══██╔══╝██║██╔═══██╗████╗  ██║
    ////  ██████╔╝██║   ██║   ██║   ███████║   ██║   ██║██║   ██║██╔██╗ ██║
    ////  ██╔══██╗██║   ██║   ██║   ██╔══██║   ██║   ██║██║   ██║██║╚██╗██║
    ////  ██║  ██║╚██████╔╝   ██║   ██║  ██║   ██║   ██║╚██████╔╝██║ ╚████║
    ////  ╚═╝  ╚═╝ ╚═════╝    ╚═╝   ╚═╝  ╚═╝   ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝
    ////                                                                   
    ///// <summary>
    ///// An Action representing a Rotation transformation in Quaternion represnetation.
    ///// </summary>
    //internal class ActionRotation : Action
    //{
    //    public ActionRotation(bool world, Rotation rot, bool relRot, int speed, int zone, MotionType mType)
    //    {
    //        type = ActionType.Rotation;

    //        worldRotation = world;
    //        rotation = rot;
    //        relativeRotation = relRot;

    //        base.speed = speed;
    //        base.zone = zone;
    //        motionType = mType;
    //    }

    //    public override string ToString()
    //    {
    //        return string.Format("ROTA: {0}, {1} {2} {3}, v{4} z{5}",
    //            motionType == MotionType.Linear ? "lin" : motionType == MotionType.Joint ? "jnt" : "und",
    //            worldRotation ? "globl" : "local",
    //            relativeRotation ? "rel" : "abs",
    //            rotation,
    //            speed,
    //            zone);
    //    }

    //}


    ////  ██████╗ ████████╗████████╗██████╗ 
    ////  ██╔══██╗╚══██╔══╝╚══██╔══╝██╔══██╗
    ////  ██████╔╝   ██║█████╗██║   ██████╔╝
    ////  ██╔══██╗   ██║╚════╝██║   ██╔══██╗
    ////  ██║  ██║   ██║      ██║   ██║  ██║
    ////  ╚═╝  ╚═╝   ╚═╝      ╚═╝   ╚═╝  ╚═╝
    ////          
    ///// <summary>
    ///// An Action representing a combined Translation then Rotation transformation.
    ///// </summary>
    //internal class ActionTranslationAndRotation : Action
    //{
    //    public ActionTranslationAndRotation(
    //        bool worldTrans, Point trans, bool relTrans,
    //        bool worldRot, Rotation rot, bool relRot,
    //        int speed, int zone, MotionType mType)
    //    {
    //        type = ActionType.TranslationAndRotation;

    //        worldTranslation = worldTrans;
    //        translation = trans;
    //        relativeTranslation = relTrans;

    //        worldRotation = worldRot;
    //        rotation = rot;
    //        relativeRotation = relRot;

    //        base.speed = speed;
    //        base.zone = zone;
    //        motionType = mType;
    //    }

    //    public override string ToString()
    //    {
    //        return string.Format("T+R_: {0}, {1} {2} {3}, {4} {5} {6}, v{7} z{8}",
    //            motionType == MotionType.Linear ? "lin" : motionType == MotionType.Joint ? "jnt" : "und",
    //            worldTranslation ? "globl" : "local",
    //            relativeTranslation ? "rel" : "abs",
    //            translation,
    //            worldRotation ? "globl" : "local",
    //            relativeRotation ? "rel" : "abs",
    //            rotation,
    //            speed,
    //            zone);
    //    }
    //}

    ///// <summary>
    ///// An Action representing a combined Rotation then Translation transformation.
    ///// </summary>
    //internal class ActionRotationAndTranslation : Action
    //{
    //    public ActionRotationAndTranslation(
    //        bool worldRot, Rotation rot, bool relRot,
    //        bool worldTrans, Point trans, bool relTrans,
    //        int speed, int zone, MotionType mType)
    //    {
    //        type = ActionType.RotationAndTranslation;

    //        worldRotation = worldRot;
    //        rotation = rot;
    //        relativeRotation = relRot;

    //        worldTranslation = worldTrans;
    //        translation = trans;
    //        relativeTranslation = relTrans;

    //        base.speed = speed;
    //        base.zone = zone;
    //        motionType = mType;
    //    }

    //    public override string ToString()
    //    {
    //        return string.Format("R+T_: {0}, {1} {2} {3}, {4} {5} {6}, v{7} z{8}",
    //            motionType == MotionType.Linear ? "lin" : motionType == MotionType.Joint ? "jnt" : "und",
    //            worldRotation ? "globl" : "local",
    //            relativeRotation ? "rel" : "abs",
    //            rotation,
    //            worldTranslation ? "globl" : "local",
    //            relativeTranslation ? "rel" : "abs",
    //            translation,
    //            speed,
    //            zone);
    //    }

    //}




    ////       ██╗ ██████╗ ██╗███╗   ██╗████████╗███████╗
    ////       ██║██╔═══██╗██║████╗  ██║╚══██╔══╝██╔════╝
    ////       ██║██║   ██║██║██╔██╗ ██║   ██║   ███████╗
    ////  ██   ██║██║   ██║██║██║╚██╗██║   ██║   ╚════██║
    ////  ╚█████╔╝╚██████╔╝██║██║ ╚████║   ██║   ███████║
    ////   ╚════╝  ╚═════╝ ╚═╝╚═╝  ╚═══╝   ╚═╝   ╚══════╝
    ////                                                 
    ///// <summary>
    ///// An Action representing the raw angular values of the device's joint rotations.
    ///// </summary>
    //internal class ActionJoints : Action
    //{
    //    public ActionJoints(Joints js, bool relJnts, int speed, int zone)
    //    {
    //        type = ActionType.Joints;

    //        joints = js;
    //        relativeJoints = relJnts;

    //        base.speed = speed;
    //        base.zone = zone;
    //    }

    //    public override string ToString()
    //    {
    //        return string.Format("JNTS: {0}, {1} {2}, v{3} z{4}",
    //            motionType == MotionType.Linear ? "lin" : motionType == MotionType.Joint ? "jnt" : "und",
    //            relativeJoints ? "rel" : "abs",
    //            joints,
    //            speed,
    //            zone);
    //    }
    //}

    ////  ███╗   ███╗███████╗███████╗███████╗ █████╗  ██████╗ ███████╗
    ////  ████╗ ████║██╔════╝██╔════╝██╔════╝██╔══██╗██╔════╝ ██╔════╝
    ////  ██╔████╔██║█████╗  ███████╗███████╗███████║██║  ███╗█████╗  
    ////  ██║╚██╔╝██║██╔══╝  ╚════██║╚════██║██╔══██║██║   ██║██╔══╝  
    ////  ██║ ╚═╝ ██║███████╗███████║███████║██║  ██║╚██████╔╝███████╗
    ////  ╚═╝     ╚═╝╚══════╝╚══════╝╚══════╝╚═╝  ╚═╝ ╚═════╝ ╚══════╝
    ////                                                              
    ///// <summary>
    ///// An Action representing a string message sent to the device to be displayed.
    ///// </summary>
    //internal class ActionMessage : Action
    //{
    //    public ActionMessage(string msg)
    //    {
    //        type = ActionType.Message;

    //        message = msg; 
    //    }

    //    public override string ToString()
    //    {
    //        return string.Format("MSSG: \"{0}\"", message);
    //    }
    //}


    ////  ██╗    ██╗ █████╗ ██╗████████╗
    ////  ██║    ██║██╔══██╗██║╚══██╔══╝
    ////  ██║ █╗ ██║███████║██║   ██║   
    ////  ██║███╗██║██╔══██║██║   ██║   
    ////  ╚███╔███╔╝██║  ██║██║   ██║   
    ////   ╚══╝╚══╝ ╚═╝  ╚═╝╚═╝   ╚═╝   
    ////                                
    ///// <summary>
    ///// An Action represening the device staying idle for a period of time.
    ///// </summary>
    //internal class ActionWait : Action
    //{
    //    public ActionWait(long millis)
    //    {
    //        type = ActionType.Wait;

    //        waitMillis = millis;
    //    }

    //    public override string ToString()
    //    {
    //        return string.Format("WAIT: {0}ms", waitMillis);
    //    }
    //}


}
