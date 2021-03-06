﻿// <copyright file="ChannelCommand.cs" company="SimWitty (http://www.simwitty.org)">
//     Copyright © 2013 and distributed under the BSD license.
// </copyright>

namespace Incog.PowerShell.Automation
{
    using System;
    using System.Management.Automation; // System.Management.Automation.dll
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets; // AddressFamily
    using System.Security.Principal;
    using Incog.Tools; // ChannelTools, ConsoleTools
    using Microsoft.PowerShell.Commands; // System.Management.Automation.dll
    using SimWitty.Library.Core.Encrypting; // Cryptkeeper

    /// <summary>
    /// Channel command is the base type that subsequent Incog covert channel cmdlet classes inherit from.
    /// </summary>
    public abstract partial class ChannelCommand : Incog.PowerShell.Automation.BaseCommand
    {
        /// <summary>
        /// Gets or sets the local IP address (receive) for the covert channel.
        /// Use [IPAddress]::Any for any addressing.
        /// </summary>
        [Parameter(Position = 0, Mandatory = false)]
        public IPAddress LocalAddress { get; set; }

        /// <summary>
        /// Gets or sets the remote IP address (transmit) for the covert channel.
        /// </summary>
        [Parameter(Position = 1, Mandatory = true)]
        public IPAddress RemoteAddress { get; set; }

        /// <summary>
        /// Gets or sets the Unicode text to transmit over the covert channel.
        /// </summary>
        [Parameter(Position = 2, Mandatory = false)]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the bytes to transmit over the covert channel.
        /// </summary>
        [Parameter(Position = 2, Mandatory = false)]
        public byte[] Bytes { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether to run in Interactive mode.
        /// </summary>
        [Parameter(Mandatory = false)]
        public System.Management.Automation.SwitchParameter Interactive { get; set; }
        
        /// <summary>
        /// Update the screen with the parameters of the chat session.
        /// </summary>
        /// <param name="mode">Communications mode (Alice/sending or Bob/receiving).</param>
        protected void PrintInteractiveMode(Incog.Tools.ChannelTools.CommunicationMode mode)
        {
            string line0 = string.Concat(this.CmdletName, ": Interactive mode");
            string line1 = string.Empty;
            string line2 = string.Empty;
            string line3 = "Type Exit to end the session.";

            switch (mode)
            {
                case Incog.Tools.ChannelTools.CommunicationMode.Alice:
                    line1 = "Sending messages: A in A->B";
                    line2 = string.Format("{0} -> {1}", this.LocalAddress.ToString(), this.RemoteAddress.ToString());
                    break;
                case Incog.Tools.ChannelTools.CommunicationMode.Bob:
                    line1 = "Receiving messages: B in A->B.";
                    line2 = string.Format("{0} -> {1}", this.RemoteAddress.ToString(), this.LocalAddress.ToString());
                    break;
                default:
                    break;
            }

            string hr = ConsoleTools.HorizontalLine(new int[] { line0.Length, line1.Length, line2.Length, line3.Length });

            Console.Clear();
            Console.WriteLine(line0);
            Console.WriteLine(line1);
            Console.WriteLine(line2);
            Console.WriteLine(line3);
            Console.WriteLine(hr);
        }

        /// <summary>
        /// Initialize parameters and base Incog cmdlet components.
        /// </summary>
        protected new void InitializeComponent()
        {
            base.InitializeComponent();

            // Do all the preflight checks here.
            this.DefaultLocalAddressToAny();
            this.CheckLocalAddressIsBound();
            this.CheckLocalAddressRemoteAddressFamily();
            this.CheckWindowsFirewall();
        }

        /// <summary>
        /// Default the Local IP Address to .Any if it is blank.
        /// </summary>
        private void DefaultLocalAddressToAny()
        {
            if (this.LocalAddress == null)
            {
                switch (this.RemoteAddress.AddressFamily)
                {
                    case AddressFamily.InterNetwork:
                        this.LocalAddress = IPAddress.Any;
                        break;
                    case AddressFamily.InterNetworkV6:
                        this.LocalAddress = IPAddress.IPv6Any;
                        break;
                    default:
                        string error = string.Format("The address type '{0}' is not supported.", this.RemoteAddress.AddressFamily.ToString());
                        throw new ApplicationException(error);
                }
            }
        }

        /// <summary>
        /// Check that the Local IP Address is bound to the current adapters.
        /// </summary>
        private void CheckLocalAddressIsBound()
        {
            if (this.LocalAddress == IPAddress.Any || this.LocalAddress == IPAddress.IPv6Any) return;
            IPAddress[] addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            bool foundIPAddress = false;

            for (int i = 0; i < addressList.Length; i++)
            {
                if (addressList[i].AddressFamily == AddressFamily.InterNetwork || addressList[i].AddressFamily == AddressFamily.InterNetworkV6)
                {
                    if (this.LocalAddress.Equals(addressList[i]))
                    {
                        foundIPAddress = true;
                        break;
                    }
                }
            }

            if (foundIPAddress) return;
            string error = string.Format("The Local IP Address '{0}' was not found on the local computer.", this.LocalAddress.ToString());
            throw new ApplicationException(error);
        }

        /// <summary>
        /// Confirm both the Local IP Address and Remote IP Address are in the same family (IPV4, IPV6).
        /// </summary>
        private void CheckLocalAddressRemoteAddressFamily()
        {
            if (this.LocalAddress.AddressFamily == this.RemoteAddress.AddressFamily) return;
            string error = string.Format("Both the local IP address and the remote IP address must be in the same family. The local is '{0}' and the remote is '{1}'.", this.LocalAddress.AddressFamily.ToString(), this.RemoteAddress.AddressFamily.ToString());
            throw new ApplicationException(error);
        }

        /// <summary>
        /// Check the Windows Firewall and warn the user if it is enabled.
        /// </summary>
        private void CheckWindowsFirewall()
        {
            bool firewalled = SimWitty.Library.Interop.WindowsFirewall.IsEnabled();
            if (firewalled)
            {
                string warning = string.Format("The {0} cmdlet has not been tested with the Windows firewall enabled. You may see inconsistent results.", this.CmdletName);
                this.WriteWarning(warning);
            }
        }
    }
}