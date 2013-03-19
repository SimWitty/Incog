// <copyright file="ConsoleTools.cs" company="SimWitty (http://www.simwitty.org)">
//     Copyright © 2013 and distributed under the BSD license.
// </copyright>

namespace Incog.Tools
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Security;
    using SimWitty.Library.Core.Tools; // StringTools

    /// <summary>
    /// A collection of tools for automating common console inputs and outputs.
    /// </summary>
    public static class ConsoleTools
    {
        /// <summary>
        /// Expand the file path if relative (".\filename" or "filename") and verify the file exists.
        /// </summary>
        /// <param name="cmdlet">A reference to the PSCmdlet.</param>
        /// <param name="path">The path passed in as an argument.</param>
        /// <returns>Returns the expanded and verified path (File Info).</returns>
        public static FileInfo ExpandAndVerifyPath(System.Management.Automation.PSCmdlet cmdlet, FileInfo path)
        {
            // If we are not in the file system, game over.
            if (!ConsoleTools.IsInFileSystem(cmdlet))
            {
                string error = "The cmdlet applies steganography to local files. Please run it again from the local file system.";
                throw new ApplicationException(error);
            }

            string filename = path.ToString();
            string currentPath = cmdlet.SessionState.Path.CurrentFileSystemLocation.Path;
            string networkPath = "Microsoft.PowerShell.Core\\FileSystem::\\\\";

            // If the string starts with .\ or is not immediately found, expand to the local file path.
            if (StringTools.StartsWith(filename, ".") || !File.Exists(filename))
            {
                string[] values = path.ToString().Split('\\');
                filename = currentPath;

                // Concat every name in the path
                for (int i = 1; i < values.Length; i++)
                {
                    string name = values[i].Trim();
                    filename = string.Concat(filename, "\\", name);
                }

                // If the path is on the network, remove the Core and pre-pend the UNC.
                if (SimWitty.Library.Core.Tools.StringTools.StartsWith(filename, networkPath, true))
                {
                    filename = filename.Substring(networkPath.Length);
                    filename = string.Concat("\\\\", filename);
                }

                path = new FileInfo(filename);
            }

            // Double-check that the file can now be found
            if (!File.Exists(path.FullName))
            {
                string error = string.Format("The cmdlet cannot find the file '{0}'. Please check the file path and try again.", path.ToString());
                throw new ApplicationException(error);
            }

            return path;
        }

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

        /// <summary>
        /// Gets a value indicating whether the current path is located in the file system. 
        /// </summary>
        /// <param name="cmdlet">A reference to the PSCmdlet.</param>
        /// <returns>True if the path is in the file system, false if not.</returns>
        public static bool IsInFileSystem(System.Management.Automation.PSCmdlet cmdlet)
        {
            return cmdlet.SessionState.Path.CurrentLocation.Path == cmdlet.SessionState.Path.CurrentFileSystemLocation.Path;
        }
    }
}
