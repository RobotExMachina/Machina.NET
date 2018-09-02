using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Machina.Types;

// Not adding a Machina.Actions namespace here just for ease of writing Actions everywhere and avoid conflicts with System.Action
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
        Precision,
        MotionMode,
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
        ExternalAxis,
        CustomCode
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
    public abstract class Action
    {
        //  ╔═╗╔╦╗╔═╗╔╦╗╦╔═╗  ╔═╗╔╦╗╦ ╦╔═╗╔═╗
        //  ╚═╗ ║ ╠═╣ ║ ║║    ╚═╗ ║ ║ ║╠╣ ╠╣ 
        //  ╚═╝ ╩ ╩ ╩ ╩ ╩╚═╝  ╚═╝ ╩ ╚═╝╚  ╚  
        internal static int currentId = 1;  // a rolling id counter

        /// <summary>
        /// Unique id for this Action.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// The type of Action this object is representing.
        /// </summary>
        public abstract ActionType Type { get; }


        /// <summary>
        /// A base constructor to take care of common setup for all actionss
        /// </summary>
        public Action()
        {
            this.Id = currentId++;
        }

        /// <summary>
        /// Generates a string representing a "serialized" instruction representing the 
        /// Machina-API command that would have generated this action. 
        /// Useful for generating actions to send to the Bridge.
        /// </summary>
        /// <returns></returns>
        public abstract string ToInstruction();

    }

}
