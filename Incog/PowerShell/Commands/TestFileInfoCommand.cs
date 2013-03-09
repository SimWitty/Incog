// <copyright file="TestFileInfoCommand.cs" company="SimWitty (http://www.simwitty.org)">
//     Copyright © 2013 and distributed under the BSD license.
// </copyright>

namespace Incog.PowerShell.Commands
{
    using System;
    using System.IO;
    using System.Management.Automation; // System.Management.Automation.dll
    using SimWitty.Library.Core.Tools; // StringTools

    /// <summary>
    /// Testing the Media Command -Path parameter.
    /// </summary>
    [System.Management.Automation.Cmdlet(
        System.Management.Automation.VerbsDiagnostic.Test,
        Incog.PowerShell.Nouns.FileInfo)]
    public class TestFileInfoCommand : Incog.PowerShell.Automation.MediaCommand
    {
        /// <summary>
        /// Provides a one-time, preprocessing functionality for the cmdlet.
        /// </summary>
        protected override void BeginProcessing()
        {
            // Initialize parameters and base Incog cmdlet components
            this.InitializeComponent();

            // Invoke Interative Mode if selected
            Console.WriteLine("CurrentProviderLocation(FileSystem): {0}", this.SessionState.Path.CurrentProviderLocation("FileSystem"));
            Console.WriteLine("CurrentProviderLocation(Registry): {0}", this.CurrentProviderLocation("Registry"));
            Console.WriteLine("CurrentFileSystemLocation: {0}", this.SessionState.Path.CurrentFileSystemLocation.Path);
            Console.WriteLine("CurrentLocation: {0}", this.SessionState.Path.CurrentLocation.Path);
            Console.WriteLine("isFileSystem: {0}", this.IsInFileSystem.ToString());
        }

        /// <summary>
        /// Provides a record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            this.WriteObject(this.Path);
        }
    }
}
