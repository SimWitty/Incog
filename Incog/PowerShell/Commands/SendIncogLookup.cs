// <copyright file="SendIncogLookup.cs" company="SimWitty (http://www.simwitty.org)">
//     Copyright © 2013 and distributed under the BSD license.
// </copyright>

namespace Incog.PowerShell.Commands
{
    using System;
    using System.Collections; // BitArray
    using System.Management.Automation; // System.Management.Automation.dll
    using System.Net;
    using System.Net.NetworkInformation;
    using Incog.Tools; // ChannelTools, ConsoleTools
    using Incog.Messaging; // TextMessage, TextMessageList
    using Microsoft.PowerShell.Commands; // System.Management.Automation.dll
    using SimWitty.Library.Core.Encrypting; // Cryptkeeper
    using SimWitty.Library.Core.Tools; // ArrayTools

    /// <summary>
    /// Send incognito messages over an DNS host (A) lookup covert channel.
    /// </summary>
    [System.Management.Automation.Cmdlet(
        System.Management.Automation.VerbsCommunications.Send,
        Incog.PowerShell.Nouns.IncogLookup)]
    public class SendIncogLookup : ChannelCommand
    {
        private TextMessageList messagelist = new TextMessageList();

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
                if (line == string.Empty) continue;


                string sent = this.SendCovertMessage(line);
                if (sent == string.Empty)
                {
                    this.WriteWarning("The message encoding failed and the message was not sent.");
                    continue;
                }

                if (line.ToLower() == "exit") break;

            } while (true);
        }
        
        /// <summary>
        /// Send incognito message via DNS and, if successful, return the value encoded in Base64.
        /// </summary>
        /// <param name="message">The Unicode character string to send over the covert channel.</param>
        /// <returns>String.Empty if the message fails, otherwise, the encrypted message encoded in Base64.</returns>
        private string SendCovertMessage(string message)
        {
            // Execute the covert channel
            ushort messageId = 1;
            ushort fragmentId = 0;

            // Ensure the line is at least 16 Byte (128 bit) for encryption
            do
            {
                message += " ";
            } while (message.Length < 16);

            Cryptkeeper mycrypt = new Cryptkeeper(this.Passphrase);
            byte[] messageBytes = mycrypt.GetBytes(message, Cryptkeeper.Action.Encrypt);

            // Setup the DNS calls array, count, and index
            int hostIndex = 0;
            int hostCount = (int)Math.Ceiling((decimal)messageBytes.Length / (decimal)messagelist.MaximumByteLength);
            string[] hosts = new string[hostCount];

            for (int i = 0; i < messageBytes.Length; i += messagelist.MaximumByteLength)
            {
                ushort length = (ushort)Math.Min(messagelist.MaximumByteLength, messageBytes.Length - i);

                byte[] msg = BitConverter.GetBytes(messageId);
                byte[] frag = BitConverter.GetBytes(fragmentId);
                byte[] len = BitConverter.GetBytes(length);

                byte[] blendme = new byte[6];
                blendme[0] = msg[0];
                blendme[1] = msg[1];
                blendme[2] = frag[0];
                blendme[3] = frag[1];
                blendme[4] = len[0];
                blendme[5] = len[1];

                byte[] tmp = new byte[length];
                Array.Copy(messageBytes, i, tmp, 0, length);

                BitArray mine = BinaryTools.BlendBits(tmp, blendme, 5);
                string fragment = SimWitty.Library.Core.Encoding.Base32.ToBase32String(mine);

                hosts[hostIndex] = string.Concat(
                    "www.",
                    fragment,
                    ".com.");
                hostIndex++;
                fragmentId++;
            }

            // Randomize the fragmments
            ArrayTools.Scramble(hosts);

            // Send the fragments as DNS hosts using Nslookup
            for (int i = 0; i < hosts.Length; i++)
            {
                ExecuteNslookup(hosts[i], this.RemoteAddress);
            }

            return System.Convert.ToBase64String(messageBytes);
        }

        /// <summary>
        /// Execute DNS lookup using the nslookup tool, and redirect the output.
        /// </summary>
        /// <param name="hostname">The hostname to lookup. In a covert channel, this is the message.</param>
        /// <param name="dnsserver">The DNS server to query. In a covert channel, this is the receipient.</param>
        private void ExecuteNslookup(string hostname, IPAddress dnsserver)
        {
            string command = string.Concat(
                "nslookup.exe",
                " ",
                hostname,
                " ",
                dnsserver.ToString());
            
            this.WriteVerbose(hostname);

            System.Diagnostics.ProcessStartInfo procStartInfo =
                new System.Diagnostics.ProcessStartInfo("cmd", "/c " + command);

            // Redirect the output and do not use another shell or console window
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;
            procStartInfo.CreateNoWindow = true;

            // Start the process to execute nslookup
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo = procStartInfo;
            proc.Start();

            // Reat the output to a string, and display it within the current console window
            string result = proc.StandardOutput.ReadToEnd();
            result = result.Trim(Environment.NewLine.ToCharArray());
            result = string.Concat(result, Environment.NewLine, Environment.NewLine);
            this.WriteVerbose(result);
        }
    }
}