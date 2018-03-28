using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Machina.Users
{
    /// <summary>
    /// A class representing an user profile for signup and logging purposes.
    /// </summary>
    internal class User
    {
        private string _name;
        private string _password;

        /// <summary>
        /// This user's name.
        /// </summary>
        internal string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// This user's log in password.
        /// </summary>
        internal string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        /// <summary>
        /// Create a default blank User profile.
        /// </summary>
        internal User()
        {
            Name = "";
            Password = "";
        }

        /// <summary>
        /// Create a new User profile from username and password.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        internal User(string username, string password)
        {
            Name = username;
            Password = password;
        }
    }
}
