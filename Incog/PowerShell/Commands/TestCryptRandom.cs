// <copyright file="TestCryptRandom.cs" company="SimWitty (http://www.simwitty.org)">
//     Copyright © 2013 and distributed under the BSD license.
// </copyright>

namespace Incog.PowerShell.Commands
{
    using System;
    using System.Management.Automation;
    using SimWitty.Library.Core.Encrypting;
    
    /// <summary>
    /// Exposing the SimWitty Crypt Random generator for testing in PowerShell.
    /// </summary>
    [System.Management.Automation.Cmdlet(
        System.Management.Automation.VerbsDiagnostic.Test,
        Incog.PowerShell.Nouns.CryptRandom)]
    public class TestCryptRandom : System.Management.Automation.PSCmdlet
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
