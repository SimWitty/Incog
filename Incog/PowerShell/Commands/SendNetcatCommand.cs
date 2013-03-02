// <copyright file="SendNetcatCommand.cs" company="SimWitty (http://www.simwitty.org)">
//     Copyright © 2013 and distributed under the BSD license.
// </copyright>

namespace Incog.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation; // System.Management.Automation.dll
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using Microsoft.PowerShell.Commands; // System.Management.Automation.dll

    /// <summary>
    /// Hello World cmdlet used to test ideas and coding patterns.
    /// </summary>
    [System.Management.Automation.Cmdlet(
        System.Management.Automation.VerbsCommunications.Send,
        Incog.PowerShell.Nouns.Netcat)]
    public class SendNetcatCommand : System.Management.Automation.PSCmdlet
    {
        #region Properties

        /// <summary>
        /// The primary output stream.
        /// </summary>
        private System.Net.Sockets.NetworkStream stream;

        /// <summary>
        /// The primary network client.
        /// </summary>
        private System.Net.Sockets.TcpClient client;

        /// <summary>
        /// Gets or sets the incoming byte stream.
        /// </summary>
        [Parameter(Position = 0, Mandatory = false, ValueFromPipeline = true)]
        public IConvertible Input { private get; set; }

        /// <summary>
        /// Sets the Computer Name to send bytes to.
        /// This should be a fully qualified domain name.
        /// </summary>
        [Parameter(Position = 1, Mandatory = false)]
        public string ComputerName { private get; set; }

        /// <summary>
        /// Gets or sets the IP address to send bytes to.
        /// </summary>
        [Parameter(Position = 1, Mandatory = false)]
        public System.Net.IPAddress IPAddress { private get; set; }
        
        /// <summary>
        /// Gets or sets the TCP port to send bytes to.
        /// </summary>
        [Parameter(Position = 2, Mandatory = false)]
        public ushort TCP { private get; set; }

        /// <summary>
        /// Gets or sets the UDP port to send bytes to. 
        /// </summary>
        [Parameter(Position = 2, Mandatory = false)]
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
            // If we have a fully qualified domain name, use DNS to set the IP address.
            if (this.ComputerName != null && this.ComputerName != string.Empty)
            {
                try
                {
                    IPAddress[] ips;
                    ips = Dns.GetHostAddresses(this.ComputerName);
                    this.IPAddress = ips[0];
                }
                catch (System.Net.Sockets.SocketException)
                {
                    ApplicationException error = new ApplicationException(string.Format("The fully qualified domain name '{0}' specified cannot be resolved to an IP address. Please check the computer name and try again.", this.ComputerName));
                    ErrorRecord record = new ErrorRecord(error, string.Empty, ErrorCategory.InvalidArgument, this.ComputerName);
                    this.WriteError(record);
                    return;
                }
            }

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

            // Open the client and stream
            if (this.TCP != 0)
            {
                client = new System.Net.Sockets.TcpClient();
                IPEndPoint target = new IPEndPoint(this.IPAddress, (int)this.TCP);
                client.Connect(target);
                stream = client.GetStream();
                buffer = new byte[client.SendBufferSize];
            }

            if (this.TCP != 0 && this.Input == null)
            {
                string message = string.Empty;
                if (this.ComputerName == string.Empty)
                {
                    message = string.Format("Sending characters to {0} TCP:{1}. ", this.IPAddress.ToString(), this.TCP.ToString());
                }
                else
                {
                    message = string.Format("Sending characters to {0} [{1}] TCP:{2}. ", this.ComputerName, this.IPAddress.ToString(), this.TCP.ToString());
                }
                if (this.Input == null) message += "End with CRLF.CRLF";
                this.WriteObject(message);
            }

            base.BeginProcessing();
        }

        byte[] buffer;
        int index = 0;

        /// <summary>
        /// Provides a record-by-record processing functionality for the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            if (this.Input == null)
            {
                do
                {
                    string line = Console.ReadLine();
                    if (line.Trim() == ".") break;

                    byte[] bytes = System.Text.Encoding.ASCII.GetBytes(line);
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush();

                }
                while (true);
            }
            else
            {
                switch (this.Encoding)
                {
                    case FileSystemCmdletProviderEncoding.Ascii:
                        char[] c = this.Input.ToString().ToCharArray();
                        byte[] incoming = System.Text.Encoding.ASCII.GetBytes(c);

                        for (int i = 0; i < incoming.Length; i += this.buffer.Length)
                        {
                            int length = Math.Min(this.buffer.Length, incoming.Length - i);
                            Array.Copy(incoming, i, buffer, 0, length);
                            stream.Write(this.buffer, 0, length);
                        }

                        break;

                    case FileSystemCmdletProviderEncoding.Byte:
                        this.buffer[this.index] = Convert.ToByte(this.Input);
                        this.index++;
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

                if (index >= buffer.Length)
                {
                    stream.Write(this.buffer, 0, this.buffer.Length);
                    stream.Flush();
                    this.index = 0;
                }
            }

            //if (this.udp != 0)
            //{
            //    System.Net.Sockets.UdpClient client = new System.Net.Sockets.UdpClient();
            //    IPEndPoint target = new IPEndPoint(this.address, (int)this.udp);
            //    client.Connect(target);

            //    string buffer = string.Empty;
            //    System.Management.Automation.Host.KeyInfo key;
            //    const int CtrlC = 3;
            //    const int Enter = 13;

            //    do
            //    {
            //        key = this.Host.UI.RawUI.ReadKey(System.Management.Automation.Host.ReadKeyOptions.IncludeKeyDown | System.Management.Automation.Host.ReadKeyOptions.AllowCtrlC);
            //        buffer += key.Character;

            //        if ((int)key.Character == Enter)
            //        {
            //            this.bytes = Encoding.ASCII.GetBytes(buffer);
            //            buffer = string.Empty;
            //            client.Send(this.bytes, this.bytes.Length);
            //        }

            //    }
            //    while ((int)key.Character != CtrlC);

            //    client.Close();
            //}
        }

        /// <summary>
        /// Provides a one-time, post-processing functionality for the cmdlet.
        /// </summary>
        protected override void EndProcessing()
        {
            if (this.TCP != 0 && this.index > 0)
            {
                stream.Write(buffer, 0, index);
                stream.Flush();
            }

            // Close the client and stream
            if (this.TCP != 0)
            {
                stream.Close();
                client.Close();
            }

            base.EndProcessing();
        }

        #endregion
    }
}
