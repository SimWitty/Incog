// <copyright file="SetIncogFileSystemCommand.cs" company="SimWitty (http://www.simwitty.org)">
//     Copyright © 2013 and distributed under the BSD license.
// </copyright>

namespace Incog.PowerShell.Commands
{
    using System;
    using System.Management.Automation;
    using Incog.Tools; // ChannelTools
    using SimWitty.Library.Core.Encrypting; // Cryptkeeper
    using SimWitty.Library.Interop; // AlternateDataStream

    /// <summary>
    /// Set an incognito message in the file system using NTFS Alternate Data Streams.
    /// </summary>
    [System.Management.Automation.Cmdlet(
        System.Management.Automation.VerbsCommon.Set,
        Incog.PowerShell.Nouns.IncogFileSystem)]
    public class SetIncogFileSystemCommand : Incog.PowerShell.Automation.MediaCommand
    {
        /// <summary>
        /// Gets or sets a value indicating the message to covertly place in the image.
        /// </summary>
        [Parameter(Mandatory = true)]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the message to covertly place in the image.
        /// </summary>
        [Parameter(Mandatory = false)]
        public string Stream { get; set; }

        /// <summary>
        /// Provides a one-time, preprocessing functionality for the cmdlet.
        /// </summary>
        protected override void BeginProcessing()
        {
            // Initialize parameters and base Incog cmdlet components
            this.InitializeComponent();

            // Preflight checks
            if (this.Stream == null) this.Stream = this.CmdletGuid;
            if (this.Stream == string.Empty) this.Stream = this.CmdletGuid;
            if (this.Stream.Length < 1) this.Stream = this.CmdletGuid;
        }

        /// <summary>
        /// Provides a record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            Cryptkeeper mycrypt = new Cryptkeeper(this.Passphrase);
            byte[] bytes = mycrypt.GetBytes(this.Message, Cryptkeeper.Action.Encrypt);
            AlternateDataStream.Write(this.Path.FullName, this.Stream, bytes);
            this.WriteObject("Message saved.");
        }

        /// <summary>
        /// Provides a one-time, post-processing functionality for the cmdlet.
        /// </summary>
        protected override void EndProcessing()
        {
        }
    }
}