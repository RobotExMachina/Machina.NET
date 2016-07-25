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
        RotationAndTranslation = 4
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
        public int velocity;
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
    }
    



    //  ████████╗██████╗  █████╗ ███╗   ██╗███████╗██╗      █████╗ ████████╗██╗ ██████╗ ███╗   ██╗
    //  ╚══██╔══╝██╔══██╗██╔══██╗████╗  ██║██╔════╝██║     ██╔══██╗╚══██╔══╝██║██╔═══██╗████╗  ██║
    //     ██║   ██████╔╝███████║██╔██╗ ██║███████╗██║     ███████║   ██║   ██║██║   ██║██╔██╗ ██║
    //     ██║   ██╔══██╗██╔══██║██║╚██╗██║╚════██║██║     ██╔══██║   ██║   ██║██║   ██║██║╚██╗██║
    //     ██║   ██║  ██║██║  ██║██║ ╚████║███████║███████╗██║  ██║   ██║   ██║╚██████╔╝██║ ╚████║
    //     ╚═╝   ╚═╝  ╚═╝╚═╝  ╚═╝╚═╝  ╚═══╝╚══════╝╚══════╝╚═╝  ╚═╝   ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝
    //                                                                                            
    /// <summary>
    /// An action representing a translation movement in cartesiam world coordinates.
    /// </summary>
    internal class ActionTranslation : Action
    {
        /// <summary>
        /// Full constructor.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="trans"></param>
        /// <param name="relTrans"></param>
        /// <param name="vel"></param>
        /// <param name="zon"></param>
        /// <param name="mType"></param>
        public ActionTranslation(bool world, Point trans, bool relTrans, int vel, int zon, MotionType mType)
        {
            type = ActionType.Translation;

            worldTranslation = world;
            translation = trans;
            relativeTranslation = relTrans;

            velocity = vel;
            zone = zon;
            motionType = mType;
        }
        
        //// Overloads with default invalid flags
        //public ActionTranslation(bool world, Point trans, bool relTrans, int vel, int zon) :
        //    this(world, trans, relTrans, vel, zon, MotionType.Undefined)
        //{ }

        //public ActionTranslation(bool world, Point trans, bool relTrans, MotionType mType) :
        //    this(world, trans, relTrans, -1, -1, mType)
        //{ }

        //public ActionTranslation(bool world, Point trans, bool relTrans) :
        //    this(world, trans, relTrans, -1, -1, MotionType.Undefined)
        //{ }
        
        public override string ToString()
        {
            return string.Format("TRS: {0}, {5} {1} {2}, v{3} z{4}",
                motionType == MotionType.Linear ? "lin" :
                    motionType == MotionType.Joint ? "jnt" :
                        motionType == MotionType.Joints ? "jjj" : "und",
                relativeTranslation ? "rel" : "abs", 
                translation, 
                velocity, 
                zone, 
                worldTranslation ? "globl" : "local");
        }
    }


    //  ██████╗  ██████╗ ████████╗ █████╗ ████████╗██╗ ██████╗ ███╗   ██╗
    //  ██╔══██╗██╔═══██╗╚══██╔══╝██╔══██╗╚══██╔══╝██║██╔═══██╗████╗  ██║
    //  ██████╔╝██║   ██║   ██║   ███████║   ██║   ██║██║   ██║██╔██╗ ██║
    //  ██╔══██╗██║   ██║   ██║   ██╔══██║   ██║   ██║██║   ██║██║╚██╗██║
    //  ██║  ██║╚██████╔╝   ██║   ██║  ██║   ██║   ██║╚██████╔╝██║ ╚████║
    //  ╚═╝  ╚═╝ ╚═════╝    ╚═╝   ╚═╝  ╚═╝   ╚═╝   ╚═╝ ╚═════╝ ╚═╝  ╚═══╝
    //                                                                   
    internal class ActionRotation : Action
    {
        public ActionRotation(bool world, Rotation rot, bool relRot, int vel, int zon, MotionType mType)
        {
            type = ActionType.Rotation;

            worldRotation = world;
            rotation = rot;
            relativeRotation = relRot;

            velocity = vel;
            zone = zon;
            motionType = mType;
        }

        //// Overloads with default invalid flags
        //public ActionRotation(bool world, Rotation rot, bool relRot, int vel, int zon) :
        //    this(world, rot, relRot, vel, zon, MotionType.Undefined)
        //{ }

        //public ActionRotation(bool world, Rotation rot, bool relRot, MotionType mType) :
        //    this(world, rot, relRot, -1, -1, mType)
        //{ }

        //public ActionRotation(bool world, Rotation rot, bool relRot) :
        //    this(world, rot, relRot, -1, -1, MotionType.Undefined)
        //{ }


        public override string ToString()
        {
            return string.Format("ROT: {0}, {5} {1} {2}, v{3} z{4}",
                motionType == MotionType.Linear ? "lin" :
                    motionType == MotionType.Joint ? "jnt" :
                        motionType == MotionType.Joints ? "jjj" : "und",
                relativeRotation ? "rel" : "abs",
                rotation,
                velocity,
                zone,
                worldRotation ? "globl" : "local");
        }

    }


    //  ██████╗ ████████╗████████╗██████╗ 
    //  ██╔══██╗╚══██╔══╝╚══██╔══╝██╔══██╗
    //  ██████╔╝   ██║█████╗██║   ██████╔╝
    //  ██╔══██╗   ██║╚════╝██║   ██╔══██╗
    //  ██║  ██║   ██║      ██║   ██║  ██║
    //  ╚═╝  ╚═╝   ╚═╝      ╚═╝   ╚═╝  ╚═╝
    //                                    
    internal class ActionTranslationAndRotation : Action
    {
        public ActionTranslationAndRotation(
            bool worldTrans, Point trans, bool relTrans,
            bool worldRot, Rotation rot, bool relRot,
            int vel, int zon, MotionType mType)
        {
            type = ActionType.TranslationAndRotation;

            worldTranslation = worldTrans;
            translation = trans;
            relativeTranslation = relTrans;

            worldRotation = worldRot;
            rotation = rot;
            relativeRotation = relRot;

            velocity = vel;
            zone = zon;
            motionType = mType;
        }

        public override string ToString()
        {
            return string.Format("T+R: {0}, {1} {2} {3}, {4} {5} {6}, v{3} z{4}",
                motionType == MotionType.Linear ? "lin" :
                    motionType == MotionType.Joint ? "jnt" :
                        motionType == MotionType.Joints ? "jjj" : "und",
                worldTranslation ? "globl" : "local",
                relativeTranslation ? "rel" : "abs",
                translation,
                worldRotation ? "globl" : "local",
                relativeRotation ? "rel" : "abs",
                rotation,
                velocity,
                zone);
        }
    }

    internal class ActionRotationAndTranslation : Action
    {
        public ActionRotationAndTranslation(
            bool worldRot, Rotation rot, bool relRot,
            bool worldTrans, Point trans, bool relTrans,
            int vel, int zon, MotionType mType)
        {
            type = ActionType.RotationAndTranslation;

            worldRotation = worldRot;
            rotation = rot;
            relativeRotation = relRot;

            worldTranslation = worldTrans;
            translation = trans;
            relativeTranslation = relTrans;

            velocity = vel;
            zone = zon;
            motionType = mType;
        }

        public override string ToString()
        {
            return string.Format("R+T: {0}, {1} {2} {3}, {4} {5} {6}, v{3} z{4}",
                motionType == MotionType.Linear ? "lin" :
                    motionType == MotionType.Joint ? "jnt" :
                        motionType == MotionType.Joints ? "jjj" : "und",
                worldRotation ? "globl" : "local",
                relativeRotation ? "rel" : "abs",
                rotation,
                worldTranslation ? "globl" : "local",
                relativeTranslation ? "rel" : "abs",
                translation,
                velocity,
                zone);
        }
    }























    // NOT USEFUL ANY MORE?


    //internal class Action
    //{
    //    bool relativeTranslation = false;
    //    Point translation = null;

    //    bool relativeRotation = false;
    //    Rotation rotation = null;

    //    bool relativeJoints = false;
    //    Joints joints = null;

    //    MotionType motionType = MotionType.None;

    //    // For the time being, constrain motion settings to integers. 
    //    /// -1 as 'unset' flag.
    //    int velocity = -1;   
    //    int zone = -1; 

    //    /// <summary>
    //    /// Base constructor for Work/Cartesian space movements.
    //    /// </summary>
    //    /// <param name="trans"></param>
    //    /// <param name="relTrans"></param>
    //    /// <param name="rot"></param>
    //    /// <param name="relRot"></param>
    //    /// <param name="mType"></param>
    //    internal Action(Point trans, bool relTrans, Rotation rot, bool relRot, int vel, int zon, MotionType mType)
    //    {
    //        // @TODO: check if mType != MotionType.Joints?

    //        translation = trans;
    //        relativeTranslation = relTrans;

    //        rotation = rot;
    //        relativeRotation = relRot;

    //        velocity = vel;
    //        zone = zon;

    //        motionType = mType;
    //    }

    //    /// <summary>
    //    /// Itemized overload.
    //    /// </summary>
    //    /// <param name="x"></param>
    //    /// <param name="y"></param>
    //    /// <param name="z"></param>
    //    /// <param name="relTrans"></param>
    //    /// <param name="q1"></param>
    //    /// <param name="q2"></param>
    //    /// <param name="q3"></param>
    //    /// <param name="q4"></param>
    //    /// <param name="relRot"></param>
    //    /// <param name="mType"></param>
    //    Action(double x, double y, double z, bool relTrans, double q1, double q2, double q3, double q4, bool relRot, int vel, int zon, MotionType mType) :
    //        this(new Point(x, y, z), relTrans, new Rotation(q1, q2, q3, q4), relRot, vel, zon, mType) {  }

    //    /// <summary>
    //    /// Base constructor for Joints/Configuration space movements.
    //    /// </summary>
    //    /// <param name="jointConfiguration"></param>
    //    /// <param name="relJoints"></param>
    //    /// <param name="mType"></param>
    //    Action(Joints jointConfiguration, bool relJoints, int vel, int zon, MotionType mType)
    //    {
    //        // @TODO: check if mType == MotionType.Joints?

    //        joints = jointConfiguration;
    //        relativeJoints = relJoints;

    //        velocity = vel;
    //        zone = zon;

    //        motionType = mType;
    //    }

    //    /// <summary>
    //    /// Itemized overload.
    //    /// </summary>
    //    /// <param name="j1"></param>
    //    /// <param name="j2"></param>
    //    /// <param name="j3"></param>
    //    /// <param name="j4"></param>
    //    /// <param name="j5"></param>
    //    /// <param name="j6"></param>
    //    /// <param name="relJoints"></param>
    //    /// <param name="mType"></param>
    //    Action(double j1, double j2, double j3, double j4, double j5, double j6, bool relJoints, int vel, int zon, MotionType mType) :
    //        this(new Joints(j1, j2, j3, j4, j5, j6), relJoints, vel, zon, mType) {  }


    //    public override string ToString()
    //    {
    //        if (motionType == MotionType.Joints)
    //            return string.Format("ACTION: {0} {1}-{2} {3}\\{4}", motionType, relativeJoints, joints, velocity, zone);

    //        return string.Format("ACTION: {0} {1}-{2} {3}-{4} {5}\\6}", motionType, relativeTranslation, translation, relativeRotation, rotation, velocity, zone);
    //    }
    //}


}
