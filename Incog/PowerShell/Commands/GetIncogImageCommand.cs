// <copyright file="GetIncogImageCommand.cs" company="SimWitty (http://www.simwitty.org)">
//     Copyright © 2013 and distributed under the BSD license.
// </copyright>

namespace Incog.PowerShell.Commands
{
    using System;
    using System.Management.Automation;
    using Incog.Steganography;
    using SimWitty.Library.Core.Encrypting; // Cryptkeeper

    /// <summary>
    /// Get an incognito message in an image using steganography.
    /// </summary>
    [System.Management.Automation.Cmdlet(
        System.Management.Automation.VerbsCommon.Get,
        Incog.PowerShell.Nouns.IncogImage)]
    public class GetIncogImageCommand : Incog.PowerShell.Automation.MediaCommand
    {
        /// <summary>
        /// Gets or sets a value indicating the first pixel to begin the message.
        /// </summary>
        [Parameter(Mandatory = false)]
        public ushort BitmapIndex { get; set; }

        /// <summary>
        /// Provides a one-time, preprocessing functionality for the cmdlet.
        /// </summary>
        protected override void BeginProcessing()
        {
            // Initialize parameters and base Incog cmdlet components
            this.InitializeComponent();

            // Set the bitmap index default if no index was supplied.
            if (this.BitmapIndex == 0) this.BitmapIndex = 128;
        }

        /// <summary>
        /// Provides a record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            Cryptkeeper crypt = new Cryptkeeper(this.Passphrase);
            string text = BitmapSteganography.SteganographyRead(this.Path.FullName, this.BitmapIndex);
            this.WriteObject(text);
        }

        /// <summary>
        /// Provides a one-time, post-processing functionality for the cmdlet.
        /// </summary>
        protected override void EndProcessing()
        {
        }
    }
}