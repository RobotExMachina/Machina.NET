/*
 TUIO C# Library - part of the reacTIVision project
 Copyright (c) 2005-2014 Martin Kaltenbrunner <martin@tuio.org>

 This library is free software; you can redistribute it and/or
 modify it under the terms of the GNU Lesser General Public
 License as published by the Free Software Foundation; either
 version 3.0 of the License, or (at your option) any later version.
 
 This library is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
 Lesser General Public License for more details.
 
 You should have received a copy of the GNU Lesser General Public
 License along with this library.
*/

using System;
using System.Collections.Generic;

namespace TUIO
{

    /**
     * <remarks>
     * The TuioObject class encapsulates /tuio/2Dobj TUIO objects.
     * </remarks>
     *
     * @author Martin Kaltenbrunner
     * @version 1.1.5
     */
    public class TuioObject : TuioContainer
    {
        /**
         * <summary>
         * The individual symbol ID number that is assigned to each TuioObject.</summary>
         */
        protected int symbol_id;

        /**
         * <summary>
         * The rotation angle value.</summary>
         */
        protected float angle;

        /**
         * <summary>
         * The rotation speed value.</summary>
         */
        protected float rotation_speed;

        /**
         * <summary>
         * The rotation acceleration value.</summary>
         */
        protected float rotation_accel;

        #region State Enumeration Values

        /**
         * <summary>
         * Defines the ROTATING state.</summary>
         */
        public static readonly int TUIO_ROTATING = 5;
        #endregion

        #region Constructors

        /**
         * <summary>
         * This constructor takes a TuioTime argument and assigns it along with the provided
         * Session ID, Symbol ID, X and Y coordinate and angle to the newly created TuioObject.</summary>
         *
         * <param name="ttime">the TuioTime to assign</param>
         * <param name="si">the Session ID to assign</param>
         * <param name="sym">the Symbol ID to assign</param>
         * <param name="xp">the X coordinate to assign</param>
         * <param name="yp">the Y coordinate to assign</param>
         * <param name="a">the angle to assign</param>
         */
        public TuioObject(TuioTime ttime, long si, int sym, float xp, float yp, float a)
            : base(ttime, si, xp, yp)
        {
            symbol_id = sym;
            angle = a;
            rotation_speed = 0.0f;
            rotation_accel = 0.0f;
        }

        /**
         * <summary>
         * This constructor takes the provided Session ID, Symbol ID, X and Y coordinate
         * and angle, and assigs these values to the newly created TuioObject.</summary>
         *
         * <param name="si">the Session ID to assign</param>
         * <param name="sym">the Symbol ID to assign</param>
         * <param name="xp">the X coordinate to assign</param>
         * <param name="yp">the Y coordinate to assign</param>
         * <param name="a">the angle to assign</param>
         */
        public TuioObject(long si, int sym, float xp, float yp, float a)
            : base(si, xp, yp)
        {
            symbol_id = sym;
            angle = a;
            rotation_speed = 0.0f;
            rotation_accel = 0.0f;
        }

        /**
         * <summary>
         * This constructor takes the atttibutes of the provided TuioObject
         * and assigs these values to the newly created TuioObject.</summary>
         *
         * <param name="tobj">the TuioObject to assign</param>
         */
        public TuioObject(TuioObject tobj)
            : base(tobj)
        {
            symbol_id = tobj.SymbolID;
            angle = tobj.Angle;
            rotation_speed = 0.0f;
            rotation_accel = 0.0f;
        }
        #endregion

        #region Update Methods

        /**
         * <summary>
         * Takes a TuioTime argument and assigns it along with the provided
         * X and Y coordinate, angle, X and Y velocity, motion acceleration,
         * rotation speed and rotation acceleration to the private TuioObject attributes.</summary>
         *
         * <param name="ttime">the TuioTime to assign</param>
         * <param name="xp">the X coordinate to assign</param>
         * <param name="yp">the Y coordinate to assign</param>
         * <param name="a">the angle coordinate to assign</param>
         * <param name="xs">the X velocity to assign</param>
         * <param name="ys">the Y velocity to assign</param>
         * <param name="rs">the rotation velocity to assign</param>
         * <param name="ma">the motion acceleration to assign</param>
         * <param name="ra">the rotation acceleration to assign</param>
         */
        public void update(TuioTime ttime, float xp, float yp, float a, float xs, float ys, float rs, float ma, float ra)
        {
            base.update(ttime, xp, yp, xs, ys, ma);
            angle = a;
            rotation_speed = rs;
            rotation_accel = ra;
            if ((rotation_accel != 0) && (state != TUIO_STOPPED)) state = TUIO_ROTATING;
        }

        /**
         * <summary>
         * Assigns the provided X and Y coordinate, angle, X and Y velocity, motion acceleration
         * rotation velocity and rotation acceleration to the private TuioContainer attributes.
         * The TuioTime time stamp remains unchanged.</summary>
         *
         * <param name="xp">the X coordinate to assign</param>
         * <param name="yp">the Y coordinate to assign</param>
         * <param name="a">the angle coordinate to assign</param>
         * <param name="xs">the X velocity to assign</param>
         * <param name="ys">the Y velocity to assign</param>
         * <param name="rs">the rotation velocity to assign</param>
         * <param name="ma">the motion acceleration to assign</param>
         * <param name="ra">the rotation acceleration to assign</param>
         */
        public void update(float xp, float yp, float a, float xs, float ys, float rs, float ma, float ra)
        {
            base.update(xp, yp, xs, ys, ma);
            angle = a;
            rotation_speed = rs;
            rotation_accel = ra;
            if ((rotation_accel != 0) && (state != TUIO_STOPPED)) state = TUIO_ROTATING;
        }

        /**
         * <summary>
         * Takes a TuioTime argument and assigns it along with the provided
         * X and Y coordinate and angle to the private TuioObject attributes.
         * The speed and accleration values are calculated accordingly.</summary>
         *
         * <param name="ttime">the TuioTime to assign</param>
         * <param name="xp">the X coordinate to assign</param>
         * <param name="yp">the Y coordinate to assign</param>
         * <param name="a">the angle coordinate to assign</param>
         */
        public void update(TuioTime ttime, float xp, float yp, float a)
        {
            TuioPoint lastPoint = path[path.Count - 1];
            base.update(ttime, xp, yp);

            TuioTime diffTime = currentTime - lastPoint.TuioTime;
            float dt = diffTime.TotalMilliseconds / 1000.0f;
            float last_angle = angle;
            float last_rotation_speed = rotation_speed;
            angle = a;

            float da = (angle - last_angle) / (2.0f * (float)Math.PI);
            if (da > 0.75f) da -= 1.0f;
            else if (da < -0.75f) da += 1.0f;

            rotation_speed = da / dt;
            rotation_accel = (rotation_speed - last_rotation_speed) / dt;
            if ((rotation_accel != 0) && (state != TUIO_STOPPED)) state = TUIO_ROTATING;
        }

        /**
         * <summary>
         * Takes the atttibutes of the provided TuioObject
         * and assigs these values to this TuioObject.
         * The TuioTime time stamp of this TuioContainer remains unchanged.</summary>
         *
         * <param name="tobj">the TuioContainer to assign</param>
         */
        public void update(TuioObject tobj)
        {
            base.update(tobj);
            angle = tobj.Angle;
            rotation_speed = tobj.RotationSpeed;
            rotation_accel = tobj.RotationAccel;
            if ((rotation_accel != 0) && (state != TUIO_STOPPED)) state = TUIO_ROTATING;
        }

        /**
         * <summary>
         * This method is used to calculate the speed and acceleration values of a
         * TuioObject with unchanged position and angle.</summary>
         */
        public new void stop(TuioTime ttime)
        {
            update(ttime, this.xpos, this.ypos, this.angle);
        }
        #endregion

        #region Properties & Getter/Setter Methods

        /**
         * <summary>
         * Returns the symbol ID of this TuioObject.</summary>
         * <returns>the symbol ID of this TuioObject</returns>
         */
        public int SymbolID
        {
            get { return symbol_id; }
        }
		
		[Obsolete("This method is provided only for compatability with legacy code. Use of the property instead is recommended.")]
        public int getSymbolID()
        {
            return SymbolID;
        }
		
        /**
         * <summary>
         * Returns the rotation angle of this TuioObject.</summary>
         * <returns>the rotation angle of this TuioObject</returns>
         */
        public float Angle
        {
            get { return angle; }
        }

		[Obsolete("This method is provided only for compatability with legacy code. Use of the property instead is recommended.")]
        public float getAngle()
        {
            return Angle;
        }
		
        /**
         * <summary>
         * Returns the rotation angle in degrees of this TuioObject.</summary>
         * <returns>the rotation angle in degrees of this TuioObject</returns>
         */
        public float AngleDegrees
        {
            get { return angle / (float)Math.PI * 180.0f; }
        }

		[Obsolete("This method is provided only for compatability with legacy code. Use of the property instead is recommended.")]
        public float getAngleDegrees()
        {
            return AngleDegrees;
        }
		
        /**
         * <summary>
         * Returns the rotation speed of this TuioObject.</summary>
         * <returns>the rotation speed of this TuioObject</returns>
         */
        public float RotationSpeed
        {
            get { return rotation_speed; }
        }

		[Obsolete("This method is provided only for compatability with legacy code. Use of the property instead is recommended.")]
        public float getRotationSpeed()
        {
            return RotationSpeed;
        }
		
        /**
         * <summary>
         * Returns the rotation acceleration of this TuioObject.</summary>
         * <returns>the rotation acceleration of this TuioObject</returns>
         */
        public float RotationAccel
        {
            get { return rotation_accel; }
        }

		[Obsolete("This method is provided only for compatability with legacy code. Use of the property instead is recommended.")]
        public float getRotationAccel()
        {
            return RotationAccel;
        }
		
        /**
         * <summary>
         * Returns true of this TuioObject is moving.</summary>
         * <returns>true of this TuioObject is moving</returns>
         */
        public override bool isMoving
        {
            get
            {
                if ((state == TUIO_ACCELERATING) || (state == TUIO_DECELERATING) || (state == TUIO_ROTATING)) return true;
                else return false;
            }
        }
        #endregion
    }

}
