// <copyright file="SetIncogWebPageCommand.cs" company="SimWitty (http://www.simwitty.org)">
//     Copyright © 2013 and distributed under the BSD license.
// </copyright>

namespace Incog.PowerShell.Commands
{
    using System;
    using System.Management.Automation;
    using Incog.Steganography; // WebPageSteganography

    /// <summary>
    /// Set an incognito message in an web page using steganography.
    /// </summary>
    [System.Management.Automation.Cmdlet(
        System.Management.Automation.VerbsCommon.Set,
        Incog.PowerShell.Nouns.IncogWebPage)]
    public class SetIncogWebPageCommand : Incog.PowerShell.Automation.MediaCommand
    {
        /// <summary>
        /// Gets or sets a value indicating the message to covertly place in the image.
        /// </summary>
        [Parameter(Mandatory = true)]
        public string Message { get; set; }
        
        /// <summary>
        /// Provides a one-time, preprocessing functionality for the cmdlet.
        /// </summary>
        protected override void BeginProcessing()
        {
            // Initialize parameters and base Incog cmdlet components
            this.InitializeComponent();
        }

        /// <summary>
        /// Provides a record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            WebPageSteganography stegoPage = new WebPageSteganography(this.Path, this.Passphrase);
            stegoPage.WriteValue(this.Message);
            string check = stegoPage.ReadValue();
            this.WriteVerbose(check);
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