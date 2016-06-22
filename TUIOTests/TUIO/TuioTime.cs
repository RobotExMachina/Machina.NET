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
     * <remarks>
     * The TuioTime class is a simple structure that is used to reprent the time that has elapsed since the session start.
     * The time is internally represented as seconds and fractions of microseconds which should be more than sufficient for gesture related timing requirements.
     * Therefore at the beginning of a typical TUIO session the static method initSession() will set the reference time for the session.
     * Another important static method getSessionTime will return a TuioTime object representing the time elapsed since the session start.
     * The class also provides various addtional convience method, which allow some simple time arithmetics.
     * </remarks>
     *
     * @author Martin Kaltenbrunner
     * @version 1.1.5
     */
    public class TuioTime
    {

        /**
         * <summary>
         * the time since session start in seconds</summary>
         */
        private long seconds = 0;

        /**
         * <summary>
         * time fraction in microseconds</summary>
         */
        private long micro_seconds = 0;

        /**
         * <summary>
         * the session start time in seconds</summary>
         */
        private static long start_seconds = 0;

        /**
         * <summary>
         * start time fraction in microseconds</summary>
         */
        private static long start_micro_seconds = 0;

        #region Constructors

        /**
         * <summary>
         * The default constructor takes no arguments and sets
         * the Seconds and Microseconds attributes of the newly created TuioTime both to zero.</summary>
         */
        public TuioTime()
        {
            this.seconds = 0;
            this.micro_seconds = 0;
        }

        /**
         * <summary>
         * This constructor takes the provided time represented in total Milliseconds
         * and assigs this value to the newly created TuioTime.</summary>
         *
         * <param name="msec">the total time in Millseconds</param>
         */
        public TuioTime(long msec)
        {
            this.seconds = msec / 1000;
            this.micro_seconds = 1000 * (msec % 1000);
        }

        /**
         * <summary>
         * This constructor takes the provided time represented in Seconds and Microseconds
         * and assigs these value to the newly created TuioTime.</summary>
         *
         * <param name="sec">the total time in seconds</param>
         * <param name="usec">the microseconds time component</param>
         */
        public TuioTime(long sec, long usec)
        {
            this.seconds = sec;
            this.micro_seconds = usec;
        }

        /**
         * <summary>
         * This constructor takes the provided TuioTime
         * and assigs its Seconds and Microseconds values to the newly created TuioTime.</summary>
         *
         * <param name="ttime">the TuioTime used to copy</param>
         */
        public TuioTime(TuioTime ttime)
        {
            this.seconds = ttime.Seconds;
            this.micro_seconds = ttime.Microseconds;
        }

        #endregion

        #region Operator Overloads

        /**
         * <summary>
         * Sums the provided time value represented in total Microseconds to the base TuioTime.</summary>
         *
         * <param name="btime">the base TuioTime</param>
         * <param name="us">the total time to add in Microseconds</param>
         * <returns>the sum of this TuioTime with the provided argument in microseconds</returns>
        */
        public static TuioTime operator +(TuioTime atime, long us)
        {
            long sec = atime.Seconds + us / 1000000;
            long usec = atime.Microseconds + us % 1000000;
            return new TuioTime(sec, usec);
        }

        /**
         * <summary>
         * Sums the provided TuioTime to the base TuioTime.</summary>
         *
         * <param name="btime">the base TuioTime</param>
         * <param name="ttime">the TuioTime to add</param>
         * <returns>the sum of this TuioTime with the provided TuioTime argument</returns>
         */
        public static TuioTime operator +(TuioTime btime, TuioTime ttime)
        {
            long sec = btime.Seconds + ttime.Seconds;
            long usec = btime.Microseconds + ttime.Microseconds;
            sec += usec / 1000000;
            usec = usec % 1000000;
            return new TuioTime(sec, usec);
        }

        /**
         * <summary>
         * Subtracts the provided time represented in Microseconds from the base TuioTime.</summary>
         *
         * <param name="btime">the base TuioTime</param>
         * <param name="us">the total time to subtract in Microseconds</param>
         * <returns>the subtraction result of this TuioTime minus the provided time in Microseconds</returns>
         */
        public static TuioTime operator -(TuioTime btime, long us)
        {
            long sec = btime.Seconds - us / 1000000;
            long usec = btime.Microseconds - us % 1000000;

            if (usec < 0)
            {
                usec += 1000000;
                sec--;
            }

            return new TuioTime(sec, usec);
        }

        /**
         * <summary>
         * Subtracts the provided TuioTime from the private Seconds and Microseconds attributes.</summary>
         *
         * <param name="btime">the base TuioTime</param>
         * <param name="ttime">the TuioTime to subtract</param>
         * <returns>the subtraction result of this TuioTime minus the provided TuioTime</returns>
         */
        public static TuioTime operator -(TuioTime btime, TuioTime ttime)
        {
            long sec = btime.Seconds - ttime.Seconds;
            long usec = btime.Microseconds - ttime.Microseconds;

            if (usec < 0)
            {
                usec += 1000000;
                sec--;
            }

            return new TuioTime(sec, usec);
        }

        /**
         * <summary>
         * Takes a TuioTime argument and compares the provided TuioTime to the private Seconds and Microseconds attributes.</summary>
         *
         * <param name="ttime">the TuioTime to compare</param>
         * <returns>true if the two TuioTime have equal Seconds and Microseconds attributes</returns>
         */
        public bool Equals(TuioTime ttime)
        {
            if ((seconds == ttime.Seconds) && (micro_seconds == ttime.Microseconds)) return true;
            else return false;
        }

        #endregion

        /**
         * <summary>
         * Resets the seconds and micro_seconds attributes to zero.</summary>
         */
        public void reset()
        {
            seconds = 0;
            micro_seconds = 0;
        }

        /**
         * <summary>
         * Returns the TuioTime Seconds component.</summary>
         * <returns>the TuioTime Seconds component</returns>
         */
        public long Seconds
        {
            get { return seconds; }
        }

        /**
         * <summary>
         * Returns the TuioTime Microseconds component.</summary>
         * <returns>the TuioTime Microseconds component</returns>
         */
        public long Microseconds
        {
            get { return micro_seconds; }
        }

        /**
         * <summary>
         * Returns the total TuioTime in Milliseconds.</summary>
         * <returns>the total TuioTime in Milliseconds</returns>
         */
        public long TotalMilliseconds
        {
            get { return seconds * 1000 + micro_seconds / 1000; }
        }

        /**
         * <summary>
         * This static method globally resets the TUIO session time.</summary>
         */
        public static void initSession()
        {
            TuioTime startTime = SystemTime;
            start_seconds = startTime.Seconds;
            start_micro_seconds = startTime.Microseconds;
        }

        /**
         * <summary>
         * Returns the present TuioTime representing the time since session start.</summary>
         * <returns>the present TuioTime representing the time since session start</returns>
         */
        public static TuioTime SessionTime
        {
            get { return SystemTime - StartTime; }
        }

        [Obsolete("This method is provided only for compatability with legacy code. Use of the property SessionTime instead is recommended.")]
        public static TuioTime getSessionTime()
        {
            return SessionTime;
        }


        /**
         * <summary>
         * Returns the absolut TuioTime representing the session start.</summary>
         * <returns>the absolut TuioTime representing the session start</returns>
         */
        public static TuioTime StartTime
        {
            get { return new TuioTime(start_seconds, start_micro_seconds); }
        }

        [Obsolete("This method is provided only for compatability with legacy code. Use of the property StartTime instead is recommended.")]
        public static TuioTime getStartTime()
        {
            return StartTime;
        }

        /**
         * <summary>
         * Returns the absolut TuioTime representing the current system time.</summary>
         * <returns>the absolut TuioTime representing the current system time</returns>
         */
        public static TuioTime SystemTime
        {
            get
            {
                long usec = DateTime.Now.Ticks / 10;
                return new TuioTime(usec / 1000000, usec % 1000000);
            }
        }

        [Obsolete("This method is provided only for compatability with legacy code. Use of the property SystemTime instead is recommended.")]
        public static TuioTime getSystemTime()
        {
            return SystemTime;
        }
    }
}