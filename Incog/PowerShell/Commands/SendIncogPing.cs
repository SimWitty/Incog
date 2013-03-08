// <copyright file="SendIncogPing.cs" company="SimWitty (http://www.simwitty.org)">
//     Copyright © 2013 and distributed under the BSD license.
// </copyright>

namespace Incog.PowerShell.Commands
{
    using System;
    using System.Management.Automation; // System.Management.Automation.dll
    using System.Net;
    using System.Net.NetworkInformation;
    using Incog.Tools; // ChannelTools, ConsoleTools
    using Microsoft.PowerShell.Commands; // System.Management.Automation.dll
    using SimWitty.Library.Core.Encrypting; // Cryptkeeper
    
    /// <summary>
    /// Send incognito messages over an ICMP covert channel.
    /// </summary>
    [System.Management.Automation.Cmdlet(
        System.Management.Automation.VerbsCommunications.Send,
        Incog.PowerShell.Nouns.IncogPing)]
    public class SendIncogPing : Incog.PowerShell.Automation.ChannelCommand
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
            
            do
            {
                Console.Write("{0}> ", this.CmdletName);
                string line = Console.ReadLine();

                string sent = this.SendCovertMessage(line);
                if (sent == string.Empty) 
                {
                    this.WriteWarning("The message encoding failed and the message was not sent.");
                    continue;
                }

                this.WriteVerbose(sent);
                if (this.IsVerbose) Console.WriteLine();

                if (line.ToLower() == "exit") break;
            } 
            while (true);
        }

        /// <summary>
        /// Send incognito message via ping and, if successful, return the value encoded in Base16.
        /// </summary>
        /// <param name="message">The Unicode character string to send over the covert channel.</param>
        /// <returns>String.Empty if the message fails, otherwise, the message encoded in Base16.</returns>
        private string SendCovertMessage(string message)
        {
            // Encode the message 
            byte[] bytes = ChannelTools.EncodeString(message.Trim());

            // Encrypt the message
            Cryptkeeper mycrypt = new Cryptkeeper(this.Passphrase);
            byte[] messageBytes = mycrypt.GetBytes(bytes, Cryptkeeper.Action.Encrypt);

            // Get two bytes that represent the length of the messageBytes array
            byte[] lengthBytes = BitConverter.GetBytes(Convert.ToUInt16(messageBytes.Length));

            // Create the ping bytes (length + encrypted message)
            byte[] pingBytes = new byte[messageBytes.Length + 2];
            Array.Copy(lengthBytes, 0, pingBytes, 0, 2);
            Array.Copy(messageBytes, 0, pingBytes, 2, messageBytes.Length);

            // Double-check
            ushort length = BitConverter.ToUInt16(new byte[] { pingBytes[0], pingBytes[1] }, 0);
            byte[] checkBytes = mycrypt.GetBytes(messageBytes, Cryptkeeper.Action.Decrypt);
            string checkText = ChannelTools.DecodeString(checkBytes);
            if (message != checkText) return string.Empty;

            // Send the message in a ping
            try
            {
                Ping p = new Ping();
                p.Send(this.RemoteAddress, 1000, pingBytes);
            }
            catch (Exception ex)
            {
                this.WriteWarning(ex.ToString());
                return string.Empty;
            }

            // Return the message encoded in Base64.
            return SimWitty.Library.Core.Encoding.Base16.ToBase16String(checkBytes);
        }
    }
}
