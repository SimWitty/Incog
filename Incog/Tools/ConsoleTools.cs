// <copyright file="ConsoleTools.cs" company="SimWitty (http://www.simwitty.org)">
//     Copyright © 2013 and distributed under the BSD license.
// </copyright>

namespace Incog.Tools
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Security;

    /// <summary>
    /// A collection of tools for automating common console inputs and outputs.
    /// </summary>
    public static class ConsoleTools
    {
        /// <summary>
        /// Get cmdlet attribute, based on type, using reflection.
        /// </summary>
        /// <typeparam name="T">Attribute type.</typeparam>
        /// <param name="provider">Attribute provider.</param>
        /// <returns>Return the attribute specified.</returns>
        public static T GetAttribute<T>(ICustomAttributeProvider provider) where T : Attribute
        {
            return GetAttribute<T>(provider, false);
        }

        /// <summary>
        /// Get cmdlet attribute, based on type, using reflection.
        /// </summary>
        /// <typeparam name="T">Attribute type.</typeparam>
        /// <param name="provider">Attribute provider.</param>
        /// <param name="inherit">If true, include inheritance classes.</param>
        /// <returns>Return the attribute specified.</returns>
        public static T GetAttribute<T>(ICustomAttributeProvider provider, bool inherit) where T : Attribute
        {
            T[] a = GetAttributes<T>(provider, inherit);
            if (a == null || a.Length == 0) return null;
            return a[0];
        }

        /// <summary>
        /// Get cmdlet attributes as an array, based on type, using reflection.
        /// </summary>
        /// <typeparam name="T">Attribute type.</typeparam>
        /// <param name="provider">Attribute provider.</param>
        /// <param name="inherit">If true, include inheritance classes.</param>
        /// <returns>Return the attributes specified in an array.</returns>
        public static T[] GetAttributes<T>(ICustomAttributeProvider provider, bool inherit)
        {
            object[] a = provider.GetCustomAttributes(typeof(T), inherit);
            if (a.Length == 0) return null;
            return a as T[];
        }

        /// <summary>
        /// Create a horizontal line for use in Console applications. If the length exceeds the width of the console, the line will be the width of the console.
        /// </summary>
        /// <param name="length">The number of characters in the resulting line.</param>
        /// <returns>Returns a horizontal line.</returns>
        public static string HorizontalLine(int length)
        {
            char line = '-';
            return HorizontalLine(length, line);
        }

        /// <summary>
        /// Create a horizontal line for use in Console applications. If the length exceeds the width of the console, the line will be the width of the console.
        /// </summary>
        /// <param name="length">The number of characters in the resulting line.</param>
        /// <param name="line">The character to use for the resulting line (the default is '-').</param>
        /// <returns>Returns a horizontal line.</returns>
        public static string HorizontalLine(int length, char line)
        {
            length = Math.Min(length, Console.WindowWidth - 1);
            return new string(line, length);
        }

        /// <summary>
        /// Determine which length is the longest in an array, and return a horizontal line that is that length. If the maximum length exceeds the width of the console, the line will be the width of the console.
        /// </summary>
        /// <param name="lengths">An array of possible lengths.</param>
        /// <returns>Returns a horizontal line.</returns>
        public static string HorizontalLine(int[] lengths)
        {
            Array.Sort(lengths);
            int length = lengths[lengths.Length - 1];
            return HorizontalLine(length);
        }

        /// <summary>
        /// Determine which length is the longest in an array, and return a horizontal line that is that length. If the maximum length exceeds the width of the console, the line will be the width of the console.
        /// </summary>
        /// <param name="lengths">An array of possible lengths.</param>
        /// <param name="line">The character to use for the resulting line (the default is '-').</param>
        /// <returns>Returns a horizontal line.</returns>
        public static string HorizontalLine(int[] lengths, char line)
        {
            Array.Sort(lengths);
            int length = lengths[lengths.Length - 1];
            return HorizontalLine(length, line);
        }
    }
}
