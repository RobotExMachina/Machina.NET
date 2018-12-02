using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina
{
    //  ██╗      ██████╗  ██████╗  ██████╗ ███████╗██████╗ 
    //  ██║     ██╔═══██╗██╔════╝ ██╔════╝ ██╔════╝██╔══██╗
    //  ██║     ██║   ██║██║  ███╗██║  ███╗█████╗  ██████╔╝
    //  ██║     ██║   ██║██║   ██║██║   ██║██╔══╝  ██╔══██╗
    //  ███████╗╚██████╔╝╚██████╔╝╚██████╔╝███████╗██║  ██║
    //  ╚══════╝ ╚═════╝  ╚═════╝  ╚═════╝ ╚══════╝╚═╝  ╚═╝
    //                                                     

    /// <summary>
    /// A class to bind Machina logging information to custom outputs via events.
    /// </summary>
    public static class Logger
    {

        /// <summary>
        /// Subscribe to this event to receive formatted logging messages. 
        /// Designed to be linked to stdouts, like: "Machina.Logger.WriteLine += Console.WriteLine;"
        /// </summary>
        public static event WriteLineHandler WriteLine;
        public delegate void WriteLineHandler(string msg);

        /// <summary>
        /// Subscribe to this event to receive logging messages, including source and levels. 
        /// All messages will be broadcasted to this logger, regardless of level.
        /// </summary>
        public static event CustomLoggingHandler CustomLogging;
        public delegate void CustomLoggingHandler(LoggerArgs e);

        /// <summary>
        /// Define the level of logging desired for the WriteLine logger: 0 None, 1 Error, 2 Warning, 3 Info (default), 4 Verbose or 5 Debug.
        /// </summary>
        /// <param name="level"></param>
        public static void SetLogLevel(int level)
        {
            _logLevel = (LogLevel)level;
        }

        /// <summary>
        /// Define the level of logging desired for the WriteLine logger: None, Error, Warning, Info (default), Verbose or Debug.
        /// </summary>
        /// <param name="level"></param>
        public static void SetLogLevel(LogLevel level)
        {
            _logLevel = level;
        }

        /// <summary>
        /// Level of logging for WriteLine. CustomLogging will catch them all.
        /// </summary>
        private static LogLevel _logLevel = LogLevel.INFO;


        public static void Error(string msg)
        {
            OnCustomLogging(new LoggerArgs(null, LogLevel.ERROR, msg));
        }

        public static void Warning(string msg)
        {
            OnCustomLogging(new LoggerArgs(null, LogLevel.WARNING, msg));
        }

        public static void Info(string msg)
        {
            OnCustomLogging(new LoggerArgs(null, LogLevel.INFO, msg));
        }

        public static void Verbose(string msg)
        {
            OnCustomLogging(new LoggerArgs(null, LogLevel.VERBOSE, msg));
        }

        public static void Debug(string msg)
        {
            OnCustomLogging(new LoggerArgs(null, LogLevel.DEBUG, msg));
        }
        



        internal static void OnCustomLogging(LoggerArgs e)
        {
            CustomLogging?.Invoke(e);

            // Try sending it to the simpler WriteLine event
            OnWriteLine(e);
        }

        internal static void OnWriteLine(LoggerArgs e)
        {
            if (WriteLine != null && e.Level <= _logLevel)
            { 
                WriteLine.Invoke(e.ToString());
            }
        }
    }


    /// <summary>
    /// A "Console" class that can be attached to objects to track their log messages.
    /// </summary>
    public class RobotLogger
    {
        internal object _sender;

        internal RobotLogger(object sender)
        {
            _sender = sender;
        }

        // STRINGS
        public void Error(string msg)
        {
            Logger.OnCustomLogging(new LoggerArgs(_sender, LogLevel.ERROR, msg));
        }

        public void Warning(string msg)
        {
            Logger.OnCustomLogging(new LoggerArgs(_sender, LogLevel.WARNING, msg));
        }

        public void Info(string msg)
        {
            Logger.OnCustomLogging(new LoggerArgs(_sender, LogLevel.INFO, msg));
        }

        public void Verbose(string msg)
        {
            Logger.OnCustomLogging(new LoggerArgs(_sender, LogLevel.VERBOSE, msg));
        }

        public void Debug(string msg)
        {
            Logger.OnCustomLogging(new LoggerArgs(_sender, LogLevel.DEBUG, msg));
        }


        // OBJECTS
        public void Error(object obj)
        {
            Logger.OnCustomLogging(new LoggerArgs(_sender, LogLevel.ERROR, obj.ToString()));
        }

        public void Warning(object obj)
        {
            Logger.OnCustomLogging(new LoggerArgs(_sender, LogLevel.WARNING, obj.ToString()));
        }

        public void Info(object obj)
        {
            Logger.OnCustomLogging(new LoggerArgs(_sender, LogLevel.INFO, obj.ToString()));
        }

        public void Verbose(object obj)
        {
            Logger.OnCustomLogging(new LoggerArgs(_sender, LogLevel.VERBOSE, obj.ToString()));
        }

        public void Debug(object obj)
        {
            Logger.OnCustomLogging(new LoggerArgs(_sender, LogLevel.DEBUG, obj.ToString()));
        }

    }




    /// <summary>
    /// Custom logging arguments
    /// </summary>
    public class LoggerArgs
    {

        public object Sender { get; }
        public LogLevel Level { get; }
        public string Message { get; }


        public LoggerArgs(object sender, int level, string msg)
        {
            Sender = sender;
            Level = (LogLevel)level;
            Message = msg;
        }

        public LoggerArgs(object sender, LogLevel level, string msg)
        {
            Sender = sender;
            Level = level;
            Message = msg;
        }

        /// <summary>
        /// Formatted representation of this object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string sender = "Machina";

            if (Sender is Robot)
            {
                Robot b = Sender as Robot;
                sender = b.Name;
            }

            return string.Format("{0} {1}: {2}",
                sender,
                Level,
                Message);
        }
    }
}
