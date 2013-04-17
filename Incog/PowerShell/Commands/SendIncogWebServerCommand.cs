// <copyright file="SendIncogWebServerCommand.cs" company="SimWitty (http://www.simwitty.org)">
//     Copyright © 2013 and distributed under the BSD license.
// </copyright>

// Source Boston 2013 Demo

namespace Incog.PowerShell.Commands
{
    using System;
    using System.Management.Automation;
    using System.Threading;
    using Incog.Servers; // HttpServer, HttpProcessor
    using Incog.Tools; // ChannelTools, ConsoleTools

    /// <summary>
    /// Send incognito messages over an covert channel.
    /// Starts a web server and returns pages with messages embedded within the pages.
    /// </summary>
    [System.Management.Automation.Cmdlet(
        System.Management.Automation.VerbsCommunications.Send,
        Incog.PowerShell.Nouns.IncogWebServer)]
    public class SendIncogWebServerCommand : Incog.PowerShell.Automation.ChannelCommand
    {
        /// <summary>
        /// Incog Web Server.
        /// </summary>
        private HttpServer server;

        /// <summary>
        /// Thread executing the Incog Web Server.
        /// </summary>
        private Thread thread;

        /// <summary>
        /// Gets or sets the TCP port to start the server on.
        /// </summary>
        [Parameter(Mandatory = false)]
        public ushort TCP { get; set; }

        /// <summary>
        /// Provides a one-time, preprocessing functionality for the cmdlet.
        /// </summary>
        protected override void BeginProcessing()
        {
            // Work in progress
            if (!this.Interactive)
            {
                this.WriteWarning("Currently, the cmdlet only supports -Interactive mode. Please run again with this switch.");
                return;
            }

            // Initialize parameters and base Incog cmdlet components
            this.InitializeComponent();

            // Default the port if not specified
            if (this.TCP == 0) this.TCP = 80;

            // Start the web server
            this.server = new HttpServer(this.Passphrase, this.LocalAddress, this.TCP);
            this.thread = new Thread(new ThreadStart(this.server.Start));
            this.thread.IsBackground = true;
            this.thread.Start();

            // Invoke Interative Mode if selected
            if (this.Interactive) this.InteractiveMode();
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
            this.server.Stop();
            this.thread.Join();
        }

        /// <summary>
        /// Interactive Mode allows the user to key in message after message, and have the message sent over the covert channel.
        /// </summary>
        private void InteractiveMode()
        {
            // Update the screen with the parameters of the chat session
            this.PrintInteractiveMode(ChannelTools.CommunicationMode.Alice);

            do
            {
                Console.Write("{0}> ", this.CmdletName);
                string line = Console.ReadLine();
                this.server.MessageQueue.Enqueue(line);
                if (line.ToLower() == "exit") break;
            }
            while (true);
        }
    }
}