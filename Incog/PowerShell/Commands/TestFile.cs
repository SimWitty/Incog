// <copyright file="ChannelCommand.cs" company="SimWitty (http://www.simwitty.org)">
//     Copyright © 2013 and distributed under the BSD license.
// </copyright>

namespace Incog.PowerShell.Commands
{
    using System;
    using System.IO;
    using System.Management.Automation; // System.Management.Automation.dll

    [System.Management.Automation.Cmdlet(
        System.Management.Automation.VerbsDiagnostic.Test,
        "FileInfo")]
    public class TestFile : System.Management.Automation.PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = false)]
        public FileInfo Path { get; set; }

        protected override void ProcessRecord()
        {
            bool isFileSystem = (this.SessionState.Path.CurrentLocation.Path == this.SessionState.Path.CurrentFileSystemLocation.Path);

            Console.WriteLine("CurrentProviderLocation(FileSystem): {0}", this.SessionState.Path.CurrentProviderLocation("FileSystem"));
            Console.WriteLine("CurrentProviderLocation(Registry): {0}", this.CurrentProviderLocation("Registry"));
            Console.WriteLine("CurrentFileSystemLocation: {0}", this.SessionState.Path.CurrentFileSystemLocation.Path);
            Console.WriteLine("CurrentLocation: {0}", this.SessionState.Path.CurrentLocation.Path);
            Console.WriteLine("isFileSystem: {0}", isFileSystem.ToString());

            if (File.Exists(this.Path.ToString())) this.WriteObject(this.Path.ToString());
            else this.WriteWarning("File not found.");
        }
    }
}
