// <copyright file="GetIncogWebPageCommand.cs" company="SimWitty (http://www.simwitty.org)">
//     Copyright © 2013 and distributed under the BSD license.
// </copyright>

namespace Incog.PowerShell.Commands
{
    using System;
    using System.Management.Automation;
    using Incog.Steganography; // WebPageSteganography

    /// <summary>
    /// Get an incognito message in an web page using steganography.
    /// </summary>
    [System.Management.Automation.Cmdlet(
        System.Management.Automation.VerbsCommon.Get,
        Incog.PowerShell.Nouns.IncogWebPage)]
    public class GetIncogWebPageCommand : Incog.PowerShell.Automation.MediaCommand
    {
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
            string text = stegoPage.ReadValue();
            this.WriteObject(text);
            this.WriteVerbose("Message retrieved.");
        }

        /// <summary>
        /// Provides a one-time, post-processing functionality for the cmdlet.
        /// </summary>
        protected override void EndProcessing()
        {
        }
    }
}