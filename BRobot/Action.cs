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
    public enum ActionType : int
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
    public class Action
    {
        public ActionType type = ActionType.Undefined;

        public static ActionTranslation Move(Point pos)
        {
            return new ActionTranslation(pos, true); 
        }

        public static ActionTranslation MoveTo(Point pos)
        {
            return new ActionTranslation(pos, false);
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
    internal class ActionZone : Action
    {
        public int zone;
        public bool relative;

        public ActionZone(int zone, bool relative)
        {
            type = ActionType.Zone;

            this.zone = zone;
            this.relative = relative;
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
    internal class ActionMotion : Action
    {
        public MotionType motionType;
        
        public ActionMotion(MotionType motionType)
        {
            this.motionType = motionType;
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
    internal class ActionCoordinates : Action
    {
        public ReferenceCS referenceCS;

        public ActionCoordinates(ReferenceCS referenceCS)
        {
            this.referenceCS = referenceCS;
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
    
}
