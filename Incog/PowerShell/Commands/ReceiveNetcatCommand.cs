// <copyright file="ReceiveNetcatCommand.cs" company="SimWitty (http://www.simwitty.org)">
//     Copyright © 2013 and distributed under the BSD license.
// </copyright>

namespace Incog.PowerShell.Commands
{
    using System;
    using System.Management.Automation; // System.Management.Automation.dll
    using System.Net;
    using System.Net.Sockets;
    using Microsoft.PowerShell.Commands; // Microsoft.PowerShell.Commands.Utility
    
    /// <summary>
    /// Cmdlet for reading from network connections using TCP or UDP, inspired by Unix network utility (NC).
    /// </summary>
    [System.Management.Automation.Cmdlet(
        System.Management.Automation.VerbsCommunications.Receive,
        Incog.PowerShell.Nouns.Netcat)]
    public class ReceiveNetcatCommand : System.Management.Automation.PSCmdlet
    {
        #region Properties

        /// <summary>
        /// Cmdlet's TCP client.
        /// </summary>
        private TcpClient client;

        /// <summary>
        /// Cmdlet's TCP listener.
        /// </summary>
        private TcpListener listener;

        /// <summary>
        /// Cmdlet's TCP stream.
        /// </summary>
        private NetworkStream stream;
        
        /// <summary>
        /// Gets or sets the IP address to send bytes to.
        /// </summary>
        [Parameter(Position = 0, Mandatory = false)]
        public System.Net.IPAddress IPAddress { private get; set; }
        
        /// <summary>
        /// Gets or sets the TCP port to send bytes to.
        /// </summary>
        [Parameter(Position = 1, Mandatory = false)]
        public ushort TCP { private get; set; }

        /// <summary>
        /// Gets or sets the UDP port to send bytes to. 
        /// </summary>
        [Parameter(Position = 1, Mandatory = false)]
        public ushort UDP { private get; set; }

        /// <summary>
        /// Gets or sets a value indicating the output encoding: String, Unicode, Byte, et cetera.
        /// </summary>
        [Parameter(Mandatory = false)]
        public FileSystemCmdletProviderEncoding Encoding { private get; set; }

        #endregion 

        #region Methods

        /// <summary>
        /// Provides a one-time, preprocessing functionality for the cmdlet.
        /// </summary>
        protected override void BeginProcessing()
        {
            // Validate that we have a valid IP address.
            try
            {
                string address = this.IPAddress.ToString();
            }
            catch (Exception ex)
            {
                ErrorRecord record = new ErrorRecord(ex, string.Empty, ErrorCategory.InvalidArgument, this.IPAddress);
                this.WriteError(record);
                return;
            }

            // Validate that we have either a valid TCP or UDP port.
            if (this.TCP == 0 && this.UDP == 0)
            {
                string warning = string.Format("A target UDP port or TCP port must be specified. The valid port numbers are 1 to {0}.{1}{2}", ushort.MaxValue.ToString("N0"), Environment.NewLine, Environment.NewLine);
                this.WriteWarning(warning);
                return;
            }

            // Validate that the user entered either TCP or UDP, but not both.
            if (this.TCP != 0 && this.UDP != 0)
            {
                string warning = string.Format("Either a target UDP port or TCP port must be specified, but not both. The valid port numbers are 1 to {0}.{1}{2}", ushort.MaxValue.ToString("N0"), Environment.NewLine, Environment.NewLine);
                this.WriteWarning(warning);
                return;
            }

            // Default the encoding to ASCII.
            if (this.Encoding == FileSystemCmdletProviderEncoding.Unknown)
            {
                this.Encoding = FileSystemCmdletProviderEncoding.Ascii;
            }

            if (this.TCP != 0)
            {
                IPEndPoint target = new IPEndPoint(this.IPAddress, (int)this.TCP);
                this.listener = new TcpListener(target);
                this.listener.Start();
                this.client = this.listener.AcceptTcpClient();
                this.stream = this.client.GetStream();
            }

            base.BeginProcessing();
        }

        /// <summary>
        /// Provides a record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            if (this.TCP != 0)
            {
                byte[] data = new byte[4096];
                int length;

                while (true)
                {
                    length = 0;

                    try
                    {
                        length = this.stream.Read(data, 0, 4096);
                    }
                    catch
                    {
                        break;
                    }

                    // If the length is 0, then the client disconnected from the server
                    if (length == 0) break;

                    // Extract the receipt bytes from the TCP payload
                    this.OutputObject(data, length);
                }
            }

            if (this.UDP != 0)
            {
                byte[] data = new byte[4096];
                IPEndPoint target = new IPEndPoint(this.IPAddress, (int)this.UDP);
                UdpClient socket = new UdpClient(target);
                IPEndPoint sender = new IPEndPoint(System.Net.IPAddress.Any, 0);

                while (true)
                {
                    data = socket.Receive(ref sender);
                    this.OutputObject(data, data.Length);
                }
            }
        }

        /// <summary>
        /// Provides a one-time, post-processing functionality for the cmdlet.
        /// </summary>
        protected override void EndProcessing()
        {
            if (this.TCP != 0)
            {
                this.stream.Close();
                this.client.Close();
                this.listener.Stop();
            }

            base.EndProcessing();
        }
        
        /// <summary>
        /// Output the object from the pipeline using the selected formatting.
        /// </summary>
        /// <param name="value">A byte array from the pipeline or network stream.</param>
        /// <param name="length">The length of the values in the byte array.</param>
        private void OutputObject(byte[] value, int length)
        {
            switch (this.Encoding)
            {
                case FileSystemCmdletProviderEncoding.Ascii:
                    this.WriteObject(System.Text.Encoding.ASCII.GetString(value, 0, length));
                    break;

                case FileSystemCmdletProviderEncoding.Byte:
                    for (int i = 0; i < length; i++)
                    {
                        this.WriteObject(value[i]);
                    }

                    break;

                case FileSystemCmdletProviderEncoding.String:
                case FileSystemCmdletProviderEncoding.Unicode:
                case FileSystemCmdletProviderEncoding.BigEndianUnicode:
                case FileSystemCmdletProviderEncoding.Default:
                case FileSystemCmdletProviderEncoding.Oem:
                case FileSystemCmdletProviderEncoding.Unknown:
                case FileSystemCmdletProviderEncoding.UTF32:
                case FileSystemCmdletProviderEncoding.UTF7:
                case FileSystemCmdletProviderEncoding.UTF8:
                default:
                    this.WriteWarning("The encoding selected has not been implemented.");
                    break;
            }
        }

        #endregion
    }
}
