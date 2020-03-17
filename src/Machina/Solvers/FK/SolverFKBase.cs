using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Machina.Types.Geometry;
using Machina.Descriptors.Models;

namespace Machina.Solvers.FK
{
    //  ███████╗ ██████╗ ██╗    ██╗   ██╗███████╗██████╗ ███████╗██╗  ██╗
    //  ██╔════╝██╔═══██╗██║    ██║   ██║██╔════╝██╔══██╗██╔════╝██║ ██╔╝
    //  ███████╗██║   ██║██║    ██║   ██║█████╗  ██████╔╝█████╗  █████╔╝ 
    //  ╚════██║██║   ██║██║    ╚██╗ ██╔╝██╔══╝  ██╔══██╗██╔══╝  ██╔═██╗ 
    //  ███████║╚██████╔╝███████╗╚████╔╝ ███████╗██║  ██║██║     ██║  ██╗
    //  ╚══════╝ ╚═════╝ ╚══════╝ ╚═══╝  ╚══════╝╚═╝  ╚═╝╚═╝     ╚═╝  ╚═╝
    //                                                                   
    /// <summary>
    /// A base class for Forward-Kinematics solvers.
    /// </summary>
    internal abstract class SolverFK
    {
        internal RobotModel Model { get; set; }

        internal SolverFK(RobotModel model)
        {
            this.Model = model;
        }

        /// <summary>
        /// Compute the forward kinematics of this device, given a vector of joint values.
        /// </summary>
        /// <param name="jointValues">These are typically the angular rotation of the joints, but could be motion for linear joints.</param>
        /// <param name="units">Units defined for these joint values.</param>
        /// <returns>Transformation matrices for each joint.</returns>
        internal abstract List<Matrix> ForwardKinematics(List<double> jointValues, Units units);

    }
}
