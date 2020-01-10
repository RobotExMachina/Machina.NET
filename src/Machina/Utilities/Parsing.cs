using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

//  ██╗   ██╗████████╗██╗██╗     ██╗████████╗██╗███████╗███████╗
//  ██║   ██║╚══██╔══╝██║██║     ██║╚══██╔══╝██║██╔════╝██╔════╝
//  ██║   ██║   ██║   ██║██║     ██║   ██║   ██║█████╗  ███████╗
//  ██║   ██║   ██║   ██║██║     ██║   ██║   ██║██╔══╝  ╚════██║
//  ╚██████╔╝   ██║   ██║███████╗██║   ██║   ██║███████╗███████║
//   ╚═════╝    ╚═╝   ╚═╝╚══════╝╚═╝   ╚═╝   ╚═╝╚══════╝╚══════╝
//                                                              
//  ███████╗████████╗██████╗ ██╗███╗   ██╗ ██████╗ ███████╗     
//  ██╔════╝╚══██╔══╝██╔══██╗██║████╗  ██║██╔════╝ ██╔════╝     
//  ███████╗   ██║   ██████╔╝██║██╔██╗ ██║██║  ███╗███████╗     
//  ╚════██║   ██║   ██╔══██╗██║██║╚██╗██║██║   ██║╚════██║     
//  ███████║   ██║   ██║  ██║██║██║ ╚████║╚██████╔╝███████║     
//  ╚══════╝   ╚═╝   ╚═╝  ╚═╝╚═╝╚═╝  ╚═══╝ ╚═════╝ ╚══════╝     
//                                                              

namespace Machina.Utilities
{
    /// <summary>
    /// Utility functions for code parsing operations.
    /// </summary>
    public static class Parsing
    {
        /// <summary>
        /// Given a statement in the form of "Command(arg1, arg2, ...);", returns an array of clean args,
        /// with the first element being the instruction, and the rest the ordered list of args in string form
        /// without the double quotes. 
        /// </summary>
        /// <param name="statement"></param>
        /// <returns></returns>
        public static string[] ParseStatement(string statement)
        {
            try
            {
                // MEGA quick and dirty
                // assuming a msg int he form of "MoveTo(300, 400, 500);" with optional spaces here and there...  
                string[] split1 = statement.Split(new char[] { '(' });
                string[] split2 = split1[1].Split(new char[] { ')' });
                //string[] args = split2[0].Split(new char[] { ',' });  // @TODO: must especify that commas should not be inside double quotes...
                string[] args = Strings.RemoveEmptyLines(Regex.Split(split2[0], ',' + "(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)")).ToArray();

                // @TODO: here we should account for escaped double quotes and badly formatted arguments (like two integers separated by whitespaces...)
                // This should be waaaay leaner and programmatic...
                // `Convert.ChangeType()` has potential, does all the cleaning up... 

                for (int i = 0; i < args.Length; i++)
                {
                    args[i] = Strings.RemoveSideChars(args[i], ' ');
                    args[i] = Strings.RemoveSideChar(args[i], '"');
                }
                string inst = Strings.RemoveSideChars(split1[0], ' ');

                string[] ret = new string[args.Length + 1];
                ret[0] = inst;
                for (int i = 0; i < args.Length; i++)
                {
                    ret[i + 1] = args[i];
                }

                return ret;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Given a bunch of code, splits it into clean individual statements.
        /// Removes new line chars, in-line "//" comments and splits by statementSeparator.
        /// Will respect characters inside double quotes.
        /// </summary>
        /// <param name="program"></param>
        /// <param name="statementSeparator"></param>
        /// <returns></returns>
        public static string[] SplitStatements(string program, char statementSeparator, string inlineCommentSymbol)
        {
            // Clean new line chars
            string inline = Strings.RemoveString(program, Environment.NewLine);

            // Split by statement
            //string[] statements = inline.Split(new char[] {statementSeparator}, StringSplitOptions.RemoveEmptyEntries);

            // Split by statement maintaining chars wrapped in doublequotes https://stackoverflow.com/a/1757107/1934487
            string[] statements = Machina.Utilities.Strings.RemoveEmptyLines(Regex.Split(inline, statementSeparator + "(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)")).ToArray();

            // Remove inline comments
            for (int i = 0; i < statements.Length; i++)
            {
                statements[i] = RemoveInLineComments(statements[i], inlineCommentSymbol);
            }

            // Clean preceding-trailing whitespaces
            for (int i = 0; i < statements.Length; i++)
            {
                statements[i] = Strings.RemoveSideChars(statements[i], ' ');
            }

            return statements;
        }

        /// <summary>
        /// Given a line of code, returns a new one with all inline comments removed from it.
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="commentSymbol"></param>
        /// <returns></returns>
        public static string RemoveInLineComments(string instruction, string commentSymbol)
        {
            bool commented = Regex.IsMatch(instruction, commentSymbol);
            if (!commented) return instruction;
            return instruction.Split(new string[] { commentSymbol }, StringSplitOptions.None)[0];
        }
    }    
}
