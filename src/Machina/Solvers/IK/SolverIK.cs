using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Machina.Types.Geometry;
using Machina.Descriptors.Models;

namespace Machina.Solvers.IK
{
    internal abstract class SolverIK
    {
        internal RobotModel Model { get; set; }

        internal SolverIK(RobotModel model)
        {
            this.Model = model;
        }

        /// <summary>
        /// Compute the Inverse Kinematics for this device, given a Matrix representing the TCP.
        /// </summary>
        internal abstract List<double> InverseKinematics(Matrix targetTCP, Matrix prevTCP, Tool tool = null);
    }
}