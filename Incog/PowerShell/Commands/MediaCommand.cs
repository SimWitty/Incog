// <copyright file="MediaCommand.cs" company="SimWitty (http://www.simwitty.org)">
//     Copyright © 2013 and distributed under the BSD license.
// </copyright>

namespace Incog.PowerShell.Commands
{
    using System;
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
    public abstract partial class MediaCommand : System.Management.Automation.PSCmdlet
    {
        // TODO: Add members / parameters for steganography files (Get-IncogFile, Set-IncogFile)
    }
}
