// <copyright file="ChannelTools.cs" company="SimWitty (http://www.simwitty.org)">
//     Copyright © 2013 and distributed under the BSD license.
// </copyright>

namespace Incog.Tools
{
    using System;
    using System.Text;

    /// <summary>
    /// A collection of tools for covert channels.
    /// </summary>
    public static class ChannelTools
    {
        /// <summary>
        /// The selected communications mode { Alice (transmit), Bob (receive), Quit (exit the program), Unspecified }
        /// </summary>
        public enum CommunicationMode
        {
            /// <summary>
            /// The Alice mode is for writing or transmitting messages (A -> B).
            /// </summary>
            Alice,

            /// <summary>
            /// The Bob mode is for reading or receiving messages (A -> B).
            /// </summary>
            Bob,

            /// <summary>
            /// Quit indicates that the user has selected to terminate the program.
            /// </summary>
            Quit,

            /// <summary>
            /// The user has not specified the mode.
            /// </summary>
            Unspecified
        }

        /// <summary>
        /// Encode a string for inclusion in a covert channel by adding a leading and trailing control characters.
        /// </summary>
        /// <param name="message">The message string that will be sent.</param>
        /// <returns>The message string with control characters.</returns>
        public static byte[] EncodeString(string message)
        {
            char c = Convert.ToChar(ushort.MaxValue);
            string s = c + message.Trim() + c;
            return Encoding.Unicode.GetBytes(s);
        }

        /// <summary>
        /// Decode a string from a covert channel by removing the leading and trailing control characters.
        /// </summary>
        /// <param name="message">The message string that will was sent.</param>
        /// <returns>The message string without control characters.</returns>
        public static string DecodeString(byte[] message)
        {
            char c = Convert.ToChar(ushort.MaxValue);
            string s = new string(Encoding.Unicode.GetChars(message));
            int start = s.IndexOf(c) + 1;
            int length = s.IndexOf(c, start) - start;

            if ((start == 0) || (length == -1))
            {
                return string.Empty;
            }
            else
            {
                return s.Substring(start, length);
            }
        }
    }
}
