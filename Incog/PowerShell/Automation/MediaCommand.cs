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
                return ConsoleTools.IsInFileSystem(this);
            }
        }

        /// <summary>
        /// Initialize parameters and base Incog cmdlet components.
        /// </summary>
        protected new void InitializeComponent()
        {
            base.InitializeComponent();

            // Do all the preflight checks here.
            this.Path = ConsoleTools.ExpandAndVerifyPath(this, this.Path);
        }        
    }
}
