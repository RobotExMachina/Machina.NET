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
     * The TuioBlob class encapsulates /tuio/2Dblb TUIO objects.
     * </remarks>
     *
     * @author Martin Kaltenbrunner
     * @version 1.1.5
     */
    public class TuioBlob : TuioContainer
    {
        /**
         * <summary>
         * The individual symbol ID number that is assigned to each TuioBlob.</summary>
         */
        protected int blob_id;

        /**
         * <summary>
         * The rotation angle value.</summary>
         */
        protected float angle;

		/**
         * <summary>
         * The blob width value.</summary>
         */
		protected float width;

		/**
         * <summary>
         * The blob height value.</summary>
         */
		protected float height;

		/**
         * <summary>
         * The blob area value.</summary>
         */
		protected float area;


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
         * Session ID, Symbol ID, X and Y coordinate and angle to the newly created TuioBlob.</summary>
         *
         * <param name="ttime">the TuioTime to assign</param>
         * <param name="si">the Session ID to assign</param>
         * <param name="bi">the Blob ID to assign</param>
         * <param name="xp">the X coordinate to assign</param>
         * <param name="yp">the Y coordinate to assign</param>
         * <param name="a">the angle to assign</param>
         * <param name="w">the width to assign</param>
         * <param name="h">the height to assign</param>
         * <param name="f">the area to assign</param>
         */
		public TuioBlob(TuioTime ttime, long si, int bi, float xp, float yp, float a, float w, float h, float f)
            : base(ttime, si, xp, yp)
        {
            blob_id = bi;
            angle = a;
			width = w;
			height = h;
			area = f;
            rotation_speed = 0.0f;
            rotation_accel = 0.0f;
        }

        /**
         * <summary>
         * This constructor takes the provided Session ID, Symbol ID, X and Y coordinate
         * and angle, and assigs these values to the newly created TuioBlob.</summary>
         *
         * <param name="si">the Session ID to assign</param>
         * <param name="sym">the Symbol ID to assign</param>
         * <param name="xp">the X coordinate to assign</param>
         * <param name="yp">the Y coordinate to assign</param>
         * <param name="a">the angle to assign</param>
         * <param name="w">the width to assign</param>
         * <param name="h">the height to assign</param>
         * <param name="f">the area to assign</param>
         */
		public TuioBlob(long si, int bi, float xp, float yp, float a, float w, float h, float f)
            : base(si, xp, yp)
        {
            blob_id = bi;
            angle = a;
			width = w;
			height = h;
			area = f;
            rotation_speed = 0.0f;
            rotation_accel = 0.0f;
        }

        /**
         * <summary>
         * This constructor takes the atttibutes of the provided TuioBlob
         * and assigs these values to the newly created TuioBlob.</summary>
         *
         * <param name="tblb">the TuioBlob to assign</param>
         */
        public TuioBlob(TuioBlob tblb)
            : base(tblb)
        {
            blob_id = tblb.BlobID;
            angle = tblb.Angle;
			width = tblb.Width;
			height = tblb.Height;
			area = tblb.Area;
            rotation_speed = 0.0f;
            rotation_accel = 0.0f;
        }
        #endregion

        #region Update Methods

        /**
         * <summary>
         * Takes a TuioTime argument and assigns it along with the provided
         * X and Y coordinate, angle, X and Y velocity, motion acceleration,
         * rotation speed and rotation acceleration to the private TuioBlob attributes.</summary>
         *
         * <param name="ttime">the TuioTime to assign</param>
         * <param name="xp">the X coordinate to assign</param>
         * <param name="yp">the Y coordinate to assign</param>
         * <param name="a">the angle coordinate to assign</param>
         * <param name="w">the width to assign</param>
         * <param name="h">the height to assign</param>
         * <param name="f">the area to assign</param>
         * <param name="xs">the X velocity to assign</param>
         * <param name="ys">the Y velocity to assign</param>
         * <param name="rs">the rotation velocity to assign</param>
         * <param name="ma">the motion acceleration to assign</param>
         * <param name="ra">the rotation acceleration to assign</param>
         */
		public void update(TuioTime ttime, float xp, float yp, float a, float w, float h, float f, float xs, float ys, float rs, float ma, float ra)
        {
            base.update(ttime, xp, yp, xs, ys, ma);
            angle = a;
			width = w;
			height = h;
			area = f;
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
         * <param name="w">the width to assign</param>
         * <param name="h">the height to assign</param>
         * <param name="f">the area to assign</param>
         * <param name="xs">the X velocity to assign</param>
         * <param name="ys">the Y velocity to assign</param>
         * <param name="rs">the rotation velocity to assign</param>
         * <param name="ma">the motion acceleration to assign</param>
         * <param name="ra">the rotation acceleration to assign</param>
         */
		public void update(float xp, float yp, float a, float w, float h, float f, float xs, float ys, float rs, float ma, float ra)
        {
            base.update(xp, yp, xs, ys, ma);
            angle = a;
			width = w;
			height = h;
			area = f;
            rotation_speed = rs;
            rotation_accel = ra;
            if ((rotation_accel != 0) && (state != TUIO_STOPPED)) state = TUIO_ROTATING;
        }

        /**
         * <summary>
         * Takes a TuioTime argument and assigns it along with the provided
         * X and Y coordinate and angle to the private TuioBlob attributes.
         * The speed and accleration values are calculated accordingly.</summary>
         *
         * <param name="ttime">the TuioTime to assign</param>
         * <param name="xp">the X coordinate to assign</param>
         * <param name="yp">the Y coordinate to assign</param>
         * <param name="a">the angle coordinate to assign</param>
         * <param name="w">the width to assign</param>
         * <param name="h">the height to assign</param>
         * <param name="f">the area to assign</param>
         */
		public void update(TuioTime ttime, float xp, float yp, float a,float w, float h, float f)
        {
            TuioPoint lastPoint = path[path.Count - 1];
            base.update(ttime, xp, yp);

			width = w;
			height = h;
			area = f;

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
         * Takes the atttibutes of the provided TuioBlob
         * and assigs these values to this TuioBlob.
         * The TuioTime time stamp of this TuioContainer remains unchanged.</summary>
         *
         * <param name="tblb">the TuioContainer to assign</param>
         */
        public void update(TuioBlob tblb)
        {
            base.update(tblb);
            angle = tblb.Angle;
			width = tblb.Width;
			height = tblb.Height;
			area = tblb.Area;
            rotation_speed = tblb.RotationSpeed;
            rotation_accel = tblb.RotationAccel;
            if ((rotation_accel != 0) && (state != TUIO_STOPPED)) state = TUIO_ROTATING;
        }

        /**
         * <summary>
         * This method is used to calculate the speed and acceleration values of a
         * TuioBlob with unchanged position and angle.</summary>
         */
        public new void stop(TuioTime ttime)
        {
			update(ttime, this.xpos, this.ypos, this.angle, this.width, this.height, this.area);
        }
        #endregion

        #region Properties & Getter/Setter Methods

        /**
         * <summary>
         * Returns the symbol ID of this TuioBlob.</summary>
         * <returns>the symbol ID of this TuioBlob</returns>
         */
        public int BlobID
        {
            get { return blob_id; }
        }
		
		[Obsolete("This method is provided only for compatability with legacy code. Use of the property instead is recommended.")]
        public int getBlobID()
        {
            return BlobID;
        }
	
		/**
         * <summary>
         * Returns the width of this TuioBlob.</summary>
         * <returns>the width of this TuioBlob</returns>
         */
		public float Width
		{
			get { return width; }
		}

		[Obsolete("This method is provided only for compatability with legacy code. Use of the property instead is recommended.")]
		public float getWidth()
		{
			return Width;
		}

		/**
         * <summary>
         * Returns the height of this TuioBlob.</summary>
         * <returns>the heigth of this TuioBlob</returns>
         */
		public float Height
		{
			get { return height; }
		}

		[Obsolete("This method is provided only for compatability with legacy code. Use of the property instead is recommended.")]
		public float getHeight()
		{
			return Height;
		}

		/**
         * <summary>
         * Returns the area of this TuioBlob.</summary>
         * <returns>the area of this TuioBlob</returns>
         */
		public float Area
		{
			get { return area; }
		}

		[Obsolete("This method is provided only for compatability with legacy code. Use of the property instead is recommended.")]
		public float getArea()
		{
			return Area;
		}

        /**
         * <summary>
         * Returns the rotation angle of this TuioBlob.</summary>
         * <returns>the rotation angle of this TuioBlob</returns>
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
         * Returns the rotation angle in degrees of this TuioBlob.</summary>
         * <returns>the rotation angle in degrees of this TuioBlob</returns>
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
         * Returns the rotation speed of this TuioBlob.</summary>
         * <returns>the rotation speed of this TuioBlob</returns>
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
         * Returns the rotation acceleration of this TuioBlob.</summary>
         * <returns>the rotation acceleration of this TuioBlob</returns>
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
         * Returns true of this TuioBlob is moving.</summary>
         * <returns>true of this TuioBlob is moving</returns>
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
