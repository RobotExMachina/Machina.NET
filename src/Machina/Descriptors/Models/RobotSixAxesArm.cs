using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Machina.Types.Geometry;
using Machina.Descriptors.Components;

using Machina.Solvers.FK;

namespace Machina.Descriptors.Models
{
    public class RobotSixAxesArm : RobotModel
    {
        public RobotJoint Base;
        public RobotJoint Joint1;
        public RobotJoint Joint2;
        public RobotJoint Joint3;
        public RobotJoint Joint4;
        public RobotJoint Joint5;
        public RobotJoint Joint6;


        public RobotSixAxesArm() : base()
        {

        }

        

    }
}
