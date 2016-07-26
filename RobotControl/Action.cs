using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotControl
{
    /// <summary>
    /// Defines an Action Type, like Translation, Rotation, Wait... 
    /// Useful to flag base Actions into children classes.
    /// </summary>
    internal enum ActionType : int
    {
        Undefined = 0,
        Translation = 1,
        Rotation = 2,
        TranslationAndRotation = 3,
        RotationAndTranslation = 4,
        Joints = 5,
        Message = 6, 
        Wait = 7
    }

    /// <summary>
    /// If an Action implies movement, what type it is.
    /// </summary>
    public enum MotionType : int
    {
        Undefined = 0,  // a null default
        Linear = 1,     // linear movement
        Joint = 2,      // joint movement
        Joints = 3      // direct joints manipulation
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
    internal class Action
    {
        public ActionType type = ActionType.Undefined;

        // @TOTHINK: Wrap this into a Settings object instead?
        public int speed;
        public int zone;
        public MotionType motionType;

        // Translation properties
        public Point translation;
        public bool relativeTranslation;
        public bool worldTranslation;

        // Rotation properties
        public Rotation rotation;
        public bool relativeRotation;
        public bool worldRotation;

        // Joint properties
        public Joints joints;
        public bool relativeJoints;

        // Message properties
        public string message;

        // Wait properties
        public long waitMillis;

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
        /// <summary>
        /// Full constructor.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="trans"></param>
        /// <param name="relTrans"></param>
        /// <param name="speed"></param>
        /// <param name="zone"></param>
        /// <param name="mType"></param>
        public ActionTranslation(bool world, Point trans, bool relTrans, int speed, int zone, MotionType mType)
        {
            type = ActionType.Translation;

            worldTranslation = world;
            translation = trans;
            relativeTranslation = relTrans;

            base.speed = speed;
            base.zone = zone;
            motionType = mType;
        }

        public override string ToString()
        {
            return string.Format("TRNS: {0}, {1} {2} {3}, v{4} z{5}",
                motionType == MotionType.Linear ? "lin" :
                    motionType == MotionType.Joint ? "jnt" :
                        motionType == MotionType.Joints ? "jjj" : "und",
                worldTranslation ? "globl" : "local",
                relativeTranslation ? "rel" : "abs",
                translation,
                speed,
                zone);
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
        public ActionRotation(bool world, Rotation rot, bool relRot, int speed, int zone, MotionType mType)
        {
            type = ActionType.Rotation;

            worldRotation = world;
            rotation = rot;
            relativeRotation = relRot;

            base.speed = speed;
            base.zone = zone;
            motionType = mType;
        }

        public override string ToString()
        {
            return string.Format("ROTA: {0}, {1} {2} {3}, v{4} z{5}",
                motionType == MotionType.Linear ? "lin" :
                    motionType == MotionType.Joint ? "jnt" :
                        motionType == MotionType.Joints ? "jjj" : "und",
                worldRotation ? "globl" : "local",
                relativeRotation ? "rel" : "abs",
                rotation,
                speed,
                zone);
        }

    }


    //  ██████╗ ████████╗████████╗██████╗ 
    //  ██╔══██╗╚══██╔══╝╚══██╔══╝██╔══██╗
    //  ██████╔╝   ██║█████╗██║   ██████╔╝
    //  ██╔══██╗   ██║╚════╝██║   ██╔══██╗
    //  ██║  ██║   ██║      ██║   ██║  ██║
    //  ╚═╝  ╚═╝   ╚═╝      ╚═╝   ╚═╝  ╚═╝
    //          
    /// <summary>
    /// An Action representing a combined Translation then Rotation transformation.
    /// </summary>
    internal class ActionTranslationAndRotation : Action
    {
        public ActionTranslationAndRotation(
            bool worldTrans, Point trans, bool relTrans,
            bool worldRot, Rotation rot, bool relRot,
            int speed, int zone, MotionType mType)
        {
            type = ActionType.TranslationAndRotation;

            worldTranslation = worldTrans;
            translation = trans;
            relativeTranslation = relTrans;

            worldRotation = worldRot;
            rotation = rot;
            relativeRotation = relRot;

            base.speed = speed;
            base.zone = zone;
            motionType = mType;
        }

        public override string ToString()
        {
            return string.Format("T+R_: {0}, {1} {2} {3}, {4} {5} {6}, v{7} z{8}",
                motionType == MotionType.Linear ? "lin" :
                    motionType == MotionType.Joint ? "jnt" :
                        motionType == MotionType.Joints ? "jjj" : "und",
                worldTranslation ? "globl" : "local",
                relativeTranslation ? "rel" : "abs",
                translation,
                worldRotation ? "globl" : "local",
                relativeRotation ? "rel" : "abs",
                rotation,
                speed,
                zone);
        }
    }

    /// <summary>
    /// An Action representing a combined Rotation then Translation transformation.
    /// </summary>
    internal class ActionRotationAndTranslation : Action
    {
        public ActionRotationAndTranslation(
            bool worldRot, Rotation rot, bool relRot,
            bool worldTrans, Point trans, bool relTrans,
            int speed, int zone, MotionType mType)
        {
            type = ActionType.RotationAndTranslation;

            worldRotation = worldRot;
            rotation = rot;
            relativeRotation = relRot;

            worldTranslation = worldTrans;
            translation = trans;
            relativeTranslation = relTrans;

            base.speed = speed;
            base.zone = zone;
            motionType = mType;
        }

        public override string ToString()
        {
            return string.Format("R+T_: {0}, {1} {2} {3}, {4} {5} {6}, v{7} z{8}",
                motionType == MotionType.Linear ? "lin" :
                    motionType == MotionType.Joint ? "jnt" :
                        motionType == MotionType.Joints ? "jjj" : "und",
                worldRotation ? "globl" : "local",
                relativeRotation ? "rel" : "abs",
                rotation,
                worldTranslation ? "globl" : "local",
                relativeTranslation ? "rel" : "abs",
                translation,
                speed,
                zone);
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
        public ActionJoints(Joints js, bool relJnts, int speed, int zone, MotionType mType)
        {
            type = ActionType.Joints;

            joints = js;
            relativeJoints = relJnts;

            base.speed = speed;
            base.zone = zone;
            motionType = mType;
        }

        public override string ToString()
        {
            return string.Format("JNTS: {0}, {1} {2}, v{3} z{4}",
                motionType == MotionType.Linear ? "lin" :
                    motionType == MotionType.Joint ? "jnt" :
                        motionType == MotionType.Joints ? "jjj" : "und",
                relativeJoints ? "rel" : "abs",
                joints,
                speed,
                zone);
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
        public ActionMessage(string msg)
        {
            type = ActionType.Message;

            message = msg; 
        }

        public override string ToString()
        {
            return string.Format("MSSG: \"{0}\"", message);
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
        public ActionWait(long millis)
        {
            type = ActionType.Wait;

            waitMillis = millis;
        }

        public override string ToString()
        {
            return string.Format("WAIT: {0}ms", waitMillis);
        }
    }


}
