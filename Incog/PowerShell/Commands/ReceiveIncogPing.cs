// <copyright file="ReceiveIncogPing.cs" company="SimWitty (http://www.simwitty.org)">
//     Copyright © 2013 and distributed under the BSD license.
// </copyright>

namespace Incog.PowerShell.Commands
{
    using System;
    using System.Management.Automation; // System.Management.Automation.dll
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using Incog.Tools; // ChannelTools, ConsoleTools
    using Microsoft.PowerShell.Commands; // System.Management.Automation.dll
    using SimWitty.Library.Core.Encrypting; // Cryptkeeper
    using SimWitty.Library.Protocols; // IPv4Packet

    /// <summary>
    /// Send incognito messages over an ICMP covert channel.
    /// </summary>
    [System.Management.Automation.Cmdlet(
        System.Management.Automation.VerbsCommunications.Receive,
        Incog.PowerShell.Nouns.IncogPing)]
    public class ReceiveIncogPing : ChannelCommand
    {
        /// <summary>
        /// Network socket for receiving packets.
        /// </summary>
        private Socket dirtysock;

        /// <summary>
        /// True if currently capturing packets.
        /// </summary>
        private bool packetCapturing = false;

        /// <summary>
        /// Provides a one-time, preprocessing functionality for the cmdlet.
        /// </summary>
        protected override void BeginProcessing()
        {
            this.RequireAdministrator = true;

            // Initialize parameters and base Incog cmdlet components
            this.InitializeComponent();
                       
            // Invoke Interative Mode if selected
            if (this.Interactive) this.InteractiveMode();
            
            // Receiving messages endpoint
            IPEndPoint bobEndpoint = new IPEndPoint(this.LocalAddress, 0);

            // Sending messages endpoing
            EndPoint aliceEndpoing = new IPEndPoint(this.RemoteAddress, 0) as EndPoint;
            
            // Open up the socket for packet capture
            byte[] inValue = new byte[4] { 1, 0, 0, 0 };
            byte[] outValue = new byte[4] { 1, 0, 0, 0 };

            this.dirtysock = new Socket(bobEndpoint.Address.AddressFamily, SocketType.Raw, ProtocolType.Icmp);
            this.dirtysock.Bind(bobEndpoint);
            this.dirtysock.IOControl(IOControlCode.ReceiveAll, inValue, outValue);
                    
            byte[] packet = new byte[4096];
            this.packetCapturing = true;

            do
            {
                this.dirtysock.ReceiveFrom(packet, ref aliceEndpoing);
                this.ReceiveCovertMessage(packet);
            }
            while (this.packetCapturing);

            this.dirtysock.Close();
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

        /// <summary>
        /// Receive incognito message via ping and, if successful, put the value on the pipeline.
        /// </summary>
        /// <param name="rawPacket">The raw bytes that were captured.</param>
        private void ReceiveCovertMessage(byte[] rawPacket)
        {
            try
            {
                int length = rawPacket.Length;
                IPv4Packet packet = new IPv4Packet(rawPacket, length);

                if (packet.SourceAddress.Equals(this.RemoteAddress) && packet.ProtocolType == Protocol.ICMP)
                {
                    ushort len = BitConverter.ToUInt16(new byte[] { packet.Data[8], packet.Data[9] }, 0);
                    byte[] messageBytes = new byte[len];
                    Array.Copy(packet.Data, 10, messageBytes, 0, len);
                    this.WriteVerbose(SimWitty.Library.Core.Encoding.Base16.ToBase16String(messageBytes));

                    Cryptkeeper mycrypt = new Cryptkeeper(this.Passphrase);
                    byte[] checkedBytes = mycrypt.GetBytes(messageBytes, Cryptkeeper.Action.Decrypt);
                    string message = ChannelTools.DecodeString(checkedBytes);

                    if (message.Trim().ToLower() == "exit")
                    {
                        this.packetCapturing = false;
                        return;
                    }

                    if (this.Interactive)
                    {
                        Console.WriteLine("{0} > {1}", packet.SourceAddress.ToString(), message);
                    }
                    else
                    {
                        this.WriteObject(message);
                    }
                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                this.WriteWarning(ex.ToString());
            }   
        }
    }
}