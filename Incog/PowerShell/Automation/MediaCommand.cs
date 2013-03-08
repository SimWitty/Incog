// <copyright file="MediaCommand.cs" company="SimWitty (http://www.simwitty.org)">
//     Copyright © 2013 and distributed under the BSD license.
// </copyright>

namespace Incog.PowerShell.Automation
{
    using System;
    using System.IO;
    using System.Management.Automation; // System.Management.Automation.dll
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets; // AddressFamily
    using System.Security.Principal;
    using Incog.Tools; // ChannelTools, ConsoleTools
    using Microsoft.PowerShell.Commands; // System.Management.Automation.dll
    using SimWitty.Library.Core.Encrypting; // Cryptkeeper
    using SimWitty.Library.Core.Tools; // StringTools

    /// <summary>
    /// Media command is the base type that subsequent Incog steganography media cmdlet classes inherit from.
    /// </summary>
    public abstract partial class MediaCommand : Incog.PowerShell.Automation.BaseCommand
    {
        /// <summary>
        /// Gets or sets a value indicating the target file path.
        /// The 'Path' name was selected for consistency with Get-Content and Set-Content.
        /// </summary>
        [Parameter(Position = 0, Mandatory = true)]
        public FileInfo Path { get; set; }

        /// <summary>
        /// Gets a value indicating whether the current path is located in the file system. 
        /// True if the path is in the file system, false if not.
        /// </summary>
        public bool IsInFileSystem
        {
            get
            {
                return this.SessionState.Path.CurrentLocation.Path == this.SessionState.Path.CurrentFileSystemLocation.Path;
            }
        }

        /// <summary>
        /// Initialize parameters and base Incog cmdlet components.
        /// </summary>
        protected new void InitializeComponent()
        {
            base.InitializeComponent();

            // Do all the preflight checks here.
            ExpandAndVerifyPath();
        }

        /// <summary>
        /// Expand the file path if relative (".\filename" or "filename") and verify the file exists.
        /// </summary>
        private void ExpandAndVerifyPath()
        {
            // If we are not in the file system, game over.
            if (!this.IsInFileSystem)
            {
                string error = string.Format("The {0} cmdlet applies steganography to local files. Please run it again from the local file system.", this.CmdletName);
                throw new ApplicationException(error);
            }

            string filename = this.Path.ToString();
            string currentPath = this.SessionState.Path.CurrentFileSystemLocation.Path;
            string networkPath = "Microsoft.PowerShell.Core\\FileSystem::\\\\";

            // If the string starts with .\ or is not immediately found, expand to the local file path.
            if (StringTools.StartsWith(filename, ".") || !File.Exists(filename))
            {
                string[] values = this.Path.ToString().Split('\\');
                string name = values[values.Length - 1].Trim();
                filename = string.Concat(currentPath, "\\", name);

                // If the path is on the network, remove the Core and pre-pend the UNC.
                if (SimWitty.Library.Core.Tools.StringTools.StartsWith(filename, networkPath, true))
                {
                    filename = filename.Substring(networkPath.Length);
                    filename = string.Concat("\\\\", filename);
                }

                this.Path = new FileInfo(filename);
            }

            // Double-check that the file can now be found
            if (!File.Exists(this.Path.ToString()))
            {
                string error = string.Format("The {0} cmdlet cannot find the file '{1}'. Please check the file path and try again.", this.CmdletName, this.Path.ToString());
                throw new ApplicationException(error);
            }
        }
    }
}
