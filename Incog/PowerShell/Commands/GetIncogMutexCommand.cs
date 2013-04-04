// <copyright file="GetIncogMutexCommand.cs" company="SimWitty (http://www.simwitty.org)">
//     Copyright © 2013 and distributed under the BSD license.
// </copyright>

namespace Incog.PowerShell.Commands
{
    using System;
    using System.IO;
    using System.IO.MemoryMappedFiles;
    using System.Management.Automation;
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
        System.Management.Automation.VerbsCommon.Get,
        Incog.PowerShell.Nouns.IncogMutex)]
    public class GetIncogMutexCommand : Incog.PowerShell.Automation.BaseCommand
    {
        /// <summary>
        /// Gets or sets a value indicating the message to covertly place in the image.
        /// </summary>
        [Parameter(Mandatory = false)]
        public string Mutex { get; set; }

        /// <summary>
        /// Provides a one-time, preprocessing functionality for the cmdlet.
        /// </summary>
        protected override void BeginProcessing()
        {
            // Reading global mutexes requires administrator-level access
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
            //// Open a Memory Mapped File without an actual file

            string MemoryMappedName = string.Concat("Global\\", this.Mutex); // mappedName
            string MemoryMutexName = string.Concat("Global\\{0}", this.Mutex, "-mutex"); // mutexName
            
            MemoryMappedFile map;
            try
            {
                map = MemoryMappedFile.OpenExisting(MemoryMappedName);
            }
            catch (Exception ex)
            {
                this.WriteWarning(ex.Message);
                return;
            }

            // Open a existing mutex and prepare to receive
            Mutex mutex = System.Threading.Mutex.OpenExisting(MemoryMutexName);
            mutex.WaitOne();

            // Read the memory map and release the mutex
            BinaryReader reader = new BinaryReader(map.CreateViewStream());
            byte[] receiveBytes = new byte[reader.BaseStream.Length];
            reader.Read(receiveBytes, 0, (int)reader.BaseStream.Length);
            reader.Close();
            mutex.ReleaseMutex();

            // Remove spoofing, decrypt, decode, and display the message
            Cryptkeeper mycrypt = new Cryptkeeper(this.Passphrase);
            byte[] cipherbytes = ShannonEntropy.Despoof(receiveBytes);
            byte[] clearbytes = mycrypt.GetBytes(cipherbytes, Cryptkeeper.Action.Decrypt);
            string message = System.Text.Encoding.Unicode.GetString(clearbytes);
           
            this.WriteObject(message);
        }

        /// <summary>
        /// Provides a one-time, post-processing functionality for the cmdlet.
        /// </summary>
        protected override void EndProcessing()
        {
        }
    }
}