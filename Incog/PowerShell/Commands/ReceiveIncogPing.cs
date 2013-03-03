// <copyright file="ReceiveIncogPing.cs" company="SimWitty (http://www.simwitty.org)">
//     Copyright © 2013 and distributed under the BSD license.
// </copyright>

namespace Incog.PowerShell.Commands
{
    using System;
    using System.Management.Automation; // System.Management.Automation.dll
    using System.Net;
    using System.Net.Sockets;
    using System.Net.NetworkInformation;
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
    public class ReceiveIncogPing : IncogCommand
    {
        private Socket dirtysock;
        private byte[] packet = new byte[4096];
        private bool packetCapturing = false;
        //private System.Threading.AutoResetEvent receiving = new System.Threading.AutoResetEvent(false);
        private bool receiving = false;

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
            // Blocks until the transmission is complete and receiving is set.
            //receiving.WaitOne();

            //do
            //{
            //    System.Threading.Thread.Sleep(500);
            //    Console.Write(".");
            //}
            //while (receiving);


            // Open up the socket for packet capture
            byte[] inValue = new byte[4] { 1, 0, 0, 0 };
            byte[] outValue = new byte[4] { 1, 0, 0, 0 };

            dirtysock = new Socket(this.LocalAddress.AddressFamily, SocketType.Raw, ProtocolType.IP);
            dirtysock.Bind(new IPEndPoint(this.LocalAddress, 0));
            dirtysock.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);
            dirtysock.IOControl(IOControlCode.ReceiveAll, inValue, outValue);
            dirtysock.BeginReceive(packet, 0, packet.Length, SocketFlags.None, new AsyncCallback(Socket_Receive), null);

            do
            {
                string line = Console.ReadLine();
                if (line.ToLower() == "exit") break;
            }
            while (true);

            // Close the packet capture socket
            packetCapturing = false;
            dirtysock.Close();
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
        /// Send incognito message via ping and, if successful, return the value encoded in Base64.
        /// </summary>
        /// <param name="message">The Unicode character string to send over the covert channel.</param>
        /// <returns>String.Empty if the message fails, otherwise, the encrypted message encoded in Base64.</returns>
        private string SendCovertMessage(string message)
        {
            return string.Empty;
        }

        /// <summary>
        /// Receive incognito message via ping and, if successful, return the value.
        /// </summary>
        /// <param name="message">The Unicode character string to received over the covert channel.</param>
        /// <returns>String.Empty if the message fails, otherwise, the decrypted message decoded.</returns>
        private string ReceiveCovertMessage(string message)
        {
            return string.Empty;
        }


        private void Socket_Receive(IAsyncResult r)
        {
            //System.IO.StreamWriter w = new System.IO.StreamWriter("testing.txt");
            //w.WriteLine(DateTime.Now.ToShortTimeString());
            //w.WriteLine("ping!");
            //w.Close();
            //return;

            Console.WriteLine("Ping!");

            try
            {
                int length = dirtysock.EndReceive(r);
                IPv4Packet ipPacket = new IPv4Packet(packet, length);

                if (ipPacket.SourceAddress.Equals(this.RemoteAddress) && ipPacket.ProtocolType == Protocol.ICMP)
                {
                    ushort len = BitConverter.ToUInt16(new byte[] { ipPacket.Data[8], ipPacket.Data[9] }, 0);
                    byte[] messageBytes = new byte[len];
                    Array.Copy(ipPacket.Data, 10, messageBytes, 0, len);

                    Cryptkeeper mycrypt = new Cryptkeeper(this.Passphrase);
                    byte[] checkedBytes = mycrypt.GetBytes(messageBytes, Cryptkeeper.Action.Decrypt);
                    string message = ChannelTools.DecodeString(checkedBytes);

                    if (message.ToLower() == "exit")
                    {
                        //receiving.Set();
                        receiving = false;
                        return;
                    }

                    if (this.Interactive)
                    {
                        Console.WriteLine("{0} : {1}", ipPacket.SourceAddress.ToString(), message);
                    }
                    else
                    {
                        this.WriteObject(message);
                    }
                }

                if (packetCapturing)
                {
                    packet = new byte[4096];
                    dirtysock.BeginReceive(packet, 0, packet.Length, SocketFlags.None, new AsyncCallback(Socket_Receive), null);
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