using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina.Types
{
    class Plane : Geometry
    {
        public static Plane WorldXY => new Plane(0, 0, 0, 1, 0, 0, 0, 1, 0);
        
        public Point Origin { get; internal set; }
        public Orientation Orientation { get; internal set; }

        public Vector XAxis => this.Orientation.XAxis;
        public Vector YAxis => this.Orientation.YAxis;
        public Vector ZAxis => this.Orientation.ZAxis;



        /// <summary>
        /// Creates a World centered Plane. 
        /// </summary>
        public Plane()
        {
            this.Identity();
        }

        /// <summary>
        /// Creates a Plane from origin Point and main axis vectors.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="xAxis"></param>
        /// <param name="yAxis"></param>
        public Plane(Point origin, Vector xAxis, Vector yAxis) :
            this(origin.X, origin.Y, origin.Z,
                 xAxis.X, xAxis.Y, xAxis.Z,
                 yAxis.X, yAxis.Y, yAxis.Z)
        { }

        /// <summary>
        /// Creates a Plane from origin and main axis vector coordinates.
        /// </summary>
        /// <param name="originX"></param>
        /// <param name="originY"></param>
        /// <param name="originZ"></param>
        /// <param name="xVecX"></param>
        /// <param name="xVecY"></param>
        /// <param name="xVecZ"></param>
        /// <param name="yVecX"></param>
        /// <param name="yVecY"></param>
        /// <param name="yVecZ"></param>
        public Plane(double originX, double originY, double originZ,
                     double xVecX, double xVecY, double xVecZ,
                     double yVecX, double yVecY, double yVecZ)
        {
            this.Orientation = new Orientation(xVecX, xVecY, xVecZ, yVecX, yVecY, yVecZ);
            this.Origin = new Point(originX, originY, originZ);
        }

        /// <summary>
        /// Turns this Plane into an identity Plane
        /// </summary>
        public void Identity()
        {
            this.Origin = new Point(0, 0, 0);
            this.Orientation = new Orientation(1, 0, 0, 0, 1, 0);
        }

        // @TODO: create a function that returns the plane that is targetPlane in basePlane coordinates
        //public static Plane Remap(Plane basePlane, Plane targetPlane)
        //{
        //    // must apply to targetPlane the inverse transformation represented by baseplane
        //}


    }
}
