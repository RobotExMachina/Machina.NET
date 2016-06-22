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

namespace TUIO
{

    /**
     * <summary>
     * The TuioCursor class encapsulates /tuio/2Dcur TUIO cursors.</summary>
     *
     * @author Martin Kaltenbrunner
     * @version 1.1.5
     */
    public class TuioCursor : TuioContainer
    {

        /**
         * <summary>
         * The individual cursor ID number that is assigned to each TuioCursor.</summary>
         */
        protected int cursor_id;

        #region Constructors

        /**
         * <summary>
         * This constructor takes a TuioTime argument and assigns it along with the provided
         * Session ID, Cursor ID, X and Y coordinate to the newly created TuioCursor.</summary>
         *
         * <param name="ttime">the TuioTime to assign</param>
         * <param name="si">the Session ID to assign</param>
         * <param name="ci">the Cursor ID to assign</param>
         * <param name="xp">the X coordinate to assign</param>
         * <param name="yp">the Y coordinate to assign</param>
         */
        public TuioCursor(TuioTime ttime, long si, int ci, float xp, float yp)
            : base(ttime, si, xp, yp)
        {
            cursor_id = ci;
        }

        /**
         * <summary>
         * This constructor takes the provided Session ID, Cursor ID, X and Y coordinate
         * and assigs these values to the newly created TuioCursor.</summary>
         *
         * <param name="si">the Session ID to assign</param>
         * <param name="ci">the Cursor ID to assign</param>
         * <param name="xp">the X coordinate to assign</param>
         * <param name="yp">the Y coordinate to assign</param>
         */
        public TuioCursor(long si, int ci, float xp, float yp)
            : base(si, xp, yp)
        {
            cursor_id = ci;
        }

        /**
         * <summary>
         * This constructor takes the atttibutes of the provided TuioCursor
         * and assigs these values to the newly created TuioCursor.</summary>
         *
         * <param name="tcur">the TuioCursor to assign</param>
         */
        public TuioCursor(TuioCursor tcur)
            : base(tcur)
        {
            cursor_id = tcur.CursorID;
        }
        #endregion

        #region Properties & Getter/Setter Methods

        /**
         * <summary>
         * Returns the Cursor ID of this TuioCursor.</summary>
         * <returns>the Cursor ID of this TuioCursor</returns>
         */
        public int CursorID
        {
            get { return cursor_id; }
        }

        [Obsolete("This method has been depracated and is provided only for compatability with legacy code. The CursorID property should be used instead.")]
        public int getCursorID()
        {            
            return cursor_id;
        }
        #endregion

    }
}
