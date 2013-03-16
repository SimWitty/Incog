// <copyright file="SendIncogNamedPipeCommand.cs" company="SimWitty (http://www.simwitty.org)">
//     Copyright © 2013 and distributed under the BSD license.
// </copyright>

namespace Incog.PowerShell.Commands
{
    using System;
    using System.IO.Pipes; // NamedPipeServerStream
    using System.Management.Automation;
    using System.Threading;
    using Incog.Messaging; // IncogStream
    using Incog.Tools; // ChannelTools, ConsoleTools
    using SimWitty.Library.Core.Encrypting; // Cryptkeeper

    /// <summary>
    /// Send incognito messages over an covert channel via DCE/RPC named pipes.
    /// </summary>
    [System.Management.Automation.Cmdlet(
        System.Management.Automation.VerbsCommunications.Send,
        Incog.PowerShell.Nouns.IncogNamedPipe)]
    public class SendIncogNamedPipeCommand : Incog.PowerShell.Automation.ChannelCommand
    {
        /// <summary>
        /// Gets or sets the encryption pass phrase. 
        /// </summary>
        [Parameter(Mandatory = false)]
        public double TargetEntropy { get; set; }

        /// <summary>
        /// Provides a one-time, preprocessing functionality for the cmdlet.
        /// </summary>
        protected override void BeginProcessing()
        {
            // Initialize parameters and base Incog cmdlet components
            this.InitializeComponent();

            // Invoke Interative Mode if selected
            if (this.Interactive) this.InteractiveMode();

            // Work in progress
            if (!this.Interactive) this.WriteWarning("Currently, the cmdlet only supports -Interactive mode. Please run again with this switch.");
        }

        /// <summary>
        /// Provides a record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            // If we are in Interactive mode, do not process records from the pipeline.
            if (this.Interactive) return;
        }

        /// <summary>
        /// Provides a one-time, post-processing functionality for the cmdlet.
        /// </summary>
        protected override void EndProcessing()
        {
        }

        /// <summary>
        /// Interactive Mode allows the user to key in message after message, and have the message sent over the covert channel.
        /// </summary>
        private void InteractiveMode()
        {
            // Update the screen with the parameters of the chat session
            this.PrintInteractiveMode(ChannelTools.CommunicationMode.Alice);

            // Wait for the client connection
            this.WriteVerbose("Waiting for client connect . . .");

            NamedPipeServerStream pipeServer = new NamedPipeServerStream(
                this.CmdletGuid,
                PipeDirection.InOut);
            
            DerivedValue derived = new DerivedValue(this.Passphrase);
            string handshake = derived.GetString(3, 12);
            this.WriteVerbose(string.Format("Handshaking with {0}.", handshake));
            
            pipeServer.WaitForConnection();
            this.WriteVerbose("Connected. Ready to send chat messages.");

            try
            {
                // Read the request from the client
                IncogStream stream = new IncogStream(pipeServer, this.Passphrase, this.TargetEntropy);

                // Verify our identity to the connected client using a handshake string
                stream.WriteString(handshake);

                do
                {
                    Console.Write("{0}> ", this.CmdletName);
                    string line = Console.ReadLine();
                    if (line == string.Empty) continue;
                    stream.WriteString(line);
                    if (line.ToLower() == "exit") break;
                }
                while (true);
            }
            catch (System.IO.IOException e)
            {
                // Catch the IOException that is raised if the pipe is broken or disconnected.
                this.WriteWarning(e.Message);
            }

            pipeServer.Close();
        }

        /// <summary>
        /// Send incognito message via DCE/RPC named pipes and, if successful, return the value encoded in Base64.
        /// </summary>
        /// <param name="message">The Unicode character string to send over the covert channel.</param>
        /// <returns>String.Empty if the message fails, otherwise, the encrypted message encoded in Base64.</returns>
        private string SendCovertMessage(string message)
        {
            return string.Empty;
        }
    }
}