// <copyright file="GetCryptRandom.cs" company="SimWitty (http://www.simwitty.org)">
//     Copyright © 2013 and distributed under the BSD license.
// </copyright>

namespace Incog.PowerShell.Commands
{
    using System;
    using System.Management.Automation; // System.Management.Automation.dll
    using System.Net;
    using System.Net.Sockets;
    using Microsoft.PowerShell.Commands; // Microsoft.PowerShell.Commands.Utility
    using SimWitty.Library.Core.Encrypting;
    
    /// <summary>
    /// Hello World cmdlet used to test ideas and coding patterns.
    /// </summary>
    [System.Management.Automation.Cmdlet(
        System.Management.Automation.VerbsCommon.Get,
        Incog.PowerShell.Nouns.CryptRandom)]
    public class GetCryptRandom : System.Management.Automation.PSCmdlet
    {
        /// <summary>
        /// Gets or sets the maximum value for the random integer.
        /// </summary>
        [Parameter(Position = 0, Mandatory = false)]
        public int Maximum { get; set; }

        /// <summary>
        /// Gets or sets the minimum value for the random integer.
        /// </summary>
        [Parameter(Position = 1, Mandatory = false)]
        public int Minimum { get; set; }

        /// <summary>
        /// Provides a one-time, preprocessing functionality for the cmdlet.
        /// </summary>
        protected override void BeginProcessing()
        {
        }
                
        /// <summary>
        /// Provides a record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            if (this.Minimum == 0) this.Minimum = int.MinValue;
            if (this.Maximum == 0) this.Maximum = int.MaxValue;
            
            CryptRandom randomize = new CryptRandom(true);
            int value = randomize.Next(this.Minimum, this.Maximum);
            this.WriteObject(value);
        }

        /// <summary>
        /// Provides a one-time, post-processing functionality for the cmdlet.
        /// </summary>
        protected override void EndProcessing()
        {
        }
    }
}
