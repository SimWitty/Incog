// <copyright file="SetIncogMutexCommand.cs" company="SimWitty (http://www.simwitty.org)">
//     Copyright © 2013 and distributed under the BSD license.
// </copyright>

namespace Incog.PowerShell.Commands
{
    using System;
    using System.IO;
    using System.IO.MemoryMappedFiles;
    using System.Management.Automation;
    using System.Runtime.Remoting.Lifetime; // ILease
    using System.Security.AccessControl; // MutexAccessRule
    using System.Security.Principal; // SecurityIdentifier
    using System.Threading; // Mutex
    using Incog.Messaging; // IncogStream
    using Incog.Tools; // ChannelTools
    using SimWitty.Library.Core.Encrypting; // Cryptkeeper
    using SimWitty.Library.Core.Tools; // ShannonEntropy

    /// <summary>
    /// Set an incognito message in to Windows' mutex collection using the Memory Mapped File object.
    /// </summary>
    [System.Management.Automation.Cmdlet(
        System.Management.Automation.VerbsCommon.Set,
        Incog.PowerShell.Nouns.IncogMutex)]
    public class SetIncogMutexCommand : Incog.PowerShell.Automation.BaseCommand
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
        public string Mutex { get; set; }

        /// <summary>
        /// Gets or sets the target Shannon Entropy used for entropy spoofing
        /// </summary>
        [Parameter(Mandatory = false)]
        public double TargetEntropy { get; set; }

        /// <summary>
        /// Provides a one-time, preprocessing functionality for the cmdlet.
        /// </summary>
        protected override void BeginProcessing()
        {
            // Creating global mutexes requires administrator-level access
            this.RequireAdministrator = true;

            // Initialize parameters and base Incog cmdlet components
            this.InitializeComponent();

            // Preflight checks
            if (this.Mutex == null) this.Mutex = this.CmdletGuid;
            if (this.Mutex == string.Empty) this.Mutex = this.CmdletGuid;
            if (this.Mutex.Length < 1) this.Mutex = this.CmdletGuid;
        }

        /// <summary>
        /// Provides a record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            //// Encode and encrypt the message with spoofed entropy

            Cryptkeeper mycrypt = new Cryptkeeper(this.Passphrase);
            byte[] cipherbytes = mycrypt.GetBytes(this.Message, Cryptkeeper.Action.Encrypt);

            int length = cipherbytes.Length;
            if (this.TargetEntropy > 0) length = length * 3;

            byte[] bytes = ShannonEntropy.Spoof(cipherbytes, this.TargetEntropy, length);
            
            //// Create a Memory Mapped File without an actual file

            string mappedName = string.Concat("Global\\", this.Mutex);
            string mutexName = string.Concat("Global\\{0}", this.Mutex, "-mutex");

            MemoryMappedFile map;
            try
            {
                map = MemoryMappedFile.CreateOrOpen(mappedName, bytes.Length, MemoryMappedFileAccess.ReadWrite);
            }
            catch (Exception ex)
            {
                this.WriteWarning(ex.Message);
                return;
            }
            
            // Define an access rules such that all processes on the OS can access it
            var allowEveryoneRule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
            var securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule(allowEveryoneRule);

            // Grab the mutex and set the access rules
            Mutex mutex = new Mutex(false, mutexName);
            mutex.SetAccessControl(securitySettings);
            mutex.WaitOne();

            // Write the bytes
            BinaryWriter writer = new BinaryWriter(map.CreateViewStream(0, bytes.Length, MemoryMappedFileAccess.ReadWrite));
            writer.Write(bytes);
            writer.Close();
            mutex.ReleaseMutex();

            Console.WriteLine("Please check for the message now. Once you continue, the message will be scheduled for garbage collection.");
            Console.Write("Press any key to continue . . . ");
            Console.ReadKey(true);
            Console.WriteLine();

            mutex.Dispose();
        }

        /// <summary>
        /// Provides a one-time, post-processing functionality for the cmdlet.
        /// </summary>
        protected override void EndProcessing()
        {
        }
    }
}