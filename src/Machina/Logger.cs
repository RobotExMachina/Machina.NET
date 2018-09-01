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
        public static void SetLevel(int level)
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


        internal static void Error(string msg)
        {
            OnCustomLogging(new LoggerArgs(null, LogLevel.ERROR, msg));
        }

        internal static void Warning(string msg)
        {
            OnCustomLogging(new LoggerArgs(null, LogLevel.WARNING, msg));
        }

        internal static void Info(string msg)
        {
            OnCustomLogging(new LoggerArgs(null, LogLevel.INFO, msg));
        }

        internal static void Verbose(string msg)
        {
            OnCustomLogging(new LoggerArgs(null, LogLevel.VERBOSE, msg));
        }

        internal static void Debug(string msg)
        {
            OnCustomLogging(new LoggerArgs(null, LogLevel.DEBUG, msg));
        }
        



        private static void OnCustomLogging(LoggerArgs e)
        {
            CustomLogging?.Invoke(e);

            // Try sending it to the simpler WriteLine event
            OnWriteLine(e);
        }

        private static void OnWriteLine(LoggerArgs e)
        {
            if (WriteLine != null && e.Level <= _logLevel)
            { 
                WriteLine.Invoke(StringifyArgs(e));
            }
        }

        private static string StringifyArgs(LoggerArgs args)
        {
            string sender = "Machina";

            if (args.Sender is Robot)
            {
                Robot b = args.Sender as Robot;
                sender = b.Name;
            }

            return string.Format("{0} {1}: {2}",
                sender,
                args.Level,
                args.Message);
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
    }
}
