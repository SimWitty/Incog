// <copyright file="ReceiveIncogNamedPipeCommand.cs" company="SimWitty (http://www.simwitty.org)">
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
    /// Receive incognito messages over an covert channel via DCE/RPC named pipes.
    /// </summary>
    [System.Management.Automation.Cmdlet(
        System.Management.Automation.VerbsCommunications.Receive,
        Incog.PowerShell.Nouns.IncogNamedPipe)]
    public class ReceiveIncogNamedPipeCommand : Incog.PowerShell.Automation.ChannelCommand
    {
        /// <summary>
        /// Provides a one-time, preprocessing functionality for the cmdlet.
        /// </summary>
        protected override void BeginProcessing()
        {
            // Initialize parameters and base Incog cmdlet components
            this.InitializeComponent();

            // Invoke Interative Mode if selected
            if (this.Interactive) this.InteractiveMode();

            // Start the connection            
            this.WriteVerbose("Connecting to server . . .");

            NamedPipeClientStream pipeClient = new NamedPipeClientStream(
                    this.RemoteAddress.ToString(),
                    this.CmdletGuid,
                    PipeDirection.InOut,
                    PipeOptions.None,
                    System.Security.Principal.TokenImpersonationLevel.Impersonation);

            DerivedValue derived = new DerivedValue(this.Passphrase);
            string handshake = derived.GetString(3, 12);
            this.WriteVerbose(string.Format("Handshaking with {0}.", handshake));

            pipeClient.Connect();
            IncogStream stream = new IncogStream(pipeClient, this.Passphrase);

            // Validate the server's signature string
            if (stream.ReadString() == handshake)
            {
                // The client security token is sent with the first write.
                // Print the file to the screen.
                this.WriteVerbose("Connected. Incoming chat messages.");
                
                do
                {
                    string message = stream.ReadString();

                    if (message == string.Empty)
                    {
                        Thread.Sleep(250);
                        continue;
                    }
                    
                    if (message.Trim().ToLower() == "exit")
                    {
                        break;
                    }

                    if (this.Interactive)
                    {
                        Console.WriteLine("{0} > {1}", this.RemoteAddress.ToString(), message);
                    }
                    else
                    {
                        this.WriteObject(message);
                    }
                }
                while (true);
            }
            else
            {
                this.WriteVerbose("The connected failed because of an invalid server handshake.");
            }

            pipeClient.Close();
            
            // Give the client process some time to display results before exiting.
            Thread.Sleep(2000);
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
            this.PrintInteractiveMode(ChannelTools.CommunicationMode.Bob);          
        }
    }
}