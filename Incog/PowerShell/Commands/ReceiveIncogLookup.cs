// <copyright file="ReceiveIncogLookup.cs" company="SimWitty (http://www.simwitty.org)">
//     Copyright © 2013 and distributed under the BSD license.
// </copyright>

namespace Incog.PowerShell.Commands
{
    using System;
    using System.Management.Automation; // System.Management.Automation.dll
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets; // UdpClient
    using Incog.Messaging; // TextMessage, TextMessageList
    using Incog.Tools; // ChannelTools, ConsoleTools
    using Microsoft.PowerShell.Commands; // System.Management.Automation.dll
    using SimWitty.Library.Core.Encrypting; // Cryptkeeper
    using SimWitty.Library.Protocols; // DnsPacket

    /// <summary>
    /// Receive incognito messages over a DNS host (A) lookup covert channel.
    /// </summary>
    [System.Management.Automation.Cmdlet(
        System.Management.Automation.VerbsCommunications.Receive,
        Incog.PowerShell.Nouns.IncogLookup)]
    public class ReceiveIncogLookup : Incog.PowerShell.Automation.ChannelCommand
    {
        /// <summary>
        /// Running message list for sending and reassembling messages.
        /// </summary>
        private TextMessageList messagelist = new TextMessageList();

        /// <summary>
        /// True if currently capturing packets.
        /// </summary>
        private bool packetCapturing = false;

        /// <summary>
        /// Provides a one-time, preprocessing functionality for the cmdlet.
        /// </summary>
        protected override void BeginProcessing()
        {
            // Initialize parameters and base Incog cmdlet components
            this.InitializeComponent();

            // Invoke Interative Mode if selected
            if (this.Interactive) this.InteractiveMode();

            // Receiving messages endpoint
            IPEndPoint bobEndpoint = new IPEndPoint(this.LocalAddress, 53);

            // Sending messages endpoing
            IPEndPoint aliceEndpoint = new IPEndPoint(this.RemoteAddress, 0);

            // Open up the UDP port for packet capture
            UdpClient socket = new UdpClient(bobEndpoint);
            this.packetCapturing = true;

            do
            {
                byte[] payload = socket.Receive(ref aliceEndpoint);
                string text = System.Text.Encoding.ASCII.GetString(payload);
                ReceiveCovertMessage(text);
            }
            while (this.packetCapturing);

            socket.Close();
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
        /// Stops processing records when the user stops the cmdlet asynchronously.
        /// </summary>
        protected override void StopProcessing()
        {
            this.packetCapturing = false;
            base.StopProcessing();
        }

        /// <summary>
        /// Interactive Mode allows the user to key in message after message, and have the message sent over the covert channel.
        /// </summary>
        private void InteractiveMode()
        {
            // Update the screen with the parameters of the chat session
            this.PrintInteractiveMode(ChannelTools.CommunicationMode.Bob);
        }

        /// <summary>
        /// Receive incognito messages via DNS and, if successful, return the value.
        /// </summary>
        /// <param name="text">The DNS message received over the covert channel.</param>
        private void ReceiveCovertMessage(string text)
        {
            if (this.Stopping) return;

            // Print the packet payload
            this.WriteVerbose(text);

            // Setup the start and stop values, clear the fragment
            char startChar = Convert.ToChar(62); // > = 62
            char stopChar = Convert.ToChar(3); // ♥ = 3
            string fragment = string.Empty;

            // Pull out the fragment, which is a portion of the base32 encoded and AES encrypted message
            int start = text.IndexOf("www") + 4;
            int stop = text.IndexOf(stopChar, start);
            int length = stop - start;
            if (length < 0) return;

            // If the string length is 0, then something's wrong, exit
            text = text.Substring(start, length);
            if (text.Length == 0) return;
            this.WriteVerbose(text);

            // If the text is not a valid Base32 string, something's wrong, exit
            bool result = SimWitty.Library.Core.Encoding.Base32.TryParse(text, out fragment);
            if (!result) return;

            // Add the message fragment
            messagelist.AddToMessages(fragment);

            // Loop thru all messages and display
            foreach (TextMessage message in messagelist)
            {
                if (message.Complete)
                {
                    text = message.GetDecryptedMessage(this.Passphrase);
                    message.Clear();

                    if (text.Trim().ToLower() == "exit")
                    {
                        this.packetCapturing = false;
                        this.WriteVerbose("----------------> exit <---------------------");
                        return;
                    }

                    if (this.Interactive)
                    {
                        if (this.IsVerbose) Console.WriteLine();
                        Console.WriteLine("{0} > {1}", this.RemoteAddress.ToString(), text);
                    }
                    else
                    {
                        this.WriteObject(text);
                    }
                }
            }
        }
    }
}