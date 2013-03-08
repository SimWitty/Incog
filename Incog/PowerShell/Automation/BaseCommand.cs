// <copyright file="BaseCommand.cs" company="SimWitty (http://www.simwitty.org)">
//     Copyright © 2013 and distributed under the BSD license.
// </copyright>

namespace Incog.PowerShell.Automation
{
    using System;
    using System.Management.Automation; // System.Management.Automation.dll
    using System.Security.Principal;
    using Incog.Tools; // ChannelTools, ConsoleTools

    /// <summary>
    /// The base command is the base type that the Incog Channel Command and Incog Media Command inherit from.
    /// </summary>
    public abstract partial class BaseCommand : System.Management.Automation.PSCmdlet
    {
        /// <summary>
        /// Gets or sets the encryption pass phrase. 
        /// </summary>
        [Parameter(Mandatory = true)]
        public System.Security.SecureString Passphrase { get; set; }

        /// <summary>
        /// Gets the cmdlet name in verb-noun format.
        /// </summary>
        public string CmdletName
        {
            get
            {
                CmdletAttribute attribute = ConsoleTools.GetAttribute<CmdletAttribute>(GetType());
                if (attribute == null) return string.Empty;
                else return string.Concat(attribute.VerbName, "-", attribute.NounName);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the code requires the user to have local administrator privileges.
        /// This is checked when InitializeComponent() is called.
        /// </summary>
        public bool RequireAdministrator { get; set; }

        /// <summary>
        /// Gets a value indicating whether the cmdlet was started with -Verbose.
        /// </summary>
        public bool IsVerbose { get; private set; }

        /// <summary>
        /// Initialize parameters and base Incog cmdlet components.
        /// </summary>
        protected void InitializeComponent()
        {
            // Set the value indicating the -Verbose switch was used
            this.IsVerbose = this.MyInvocation.BoundParameters.ContainsKey("Verbose");

            // Throw an exception if the cmdlet is set to require administrator but the user is not an administrator.
            this.CheckIfAdministrator();
        }

        /// <summary>
        /// Confirm the user is Administrator (if Requires Administrator == true).
        /// </summary>
        private void CheckIfAdministrator()
        {
            if (this.RequireAdministrator)
            {
                WindowsPrincipal user = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                bool admin = user.IsInRole(WindowsBuiltInRole.Administrator);

                if (!admin)
                {
                    string error = string.Format("The {0} cmdlet requires elevated permissions. Please run it again as a member of the local Administrators group.", this.CmdletName);
                    throw new ApplicationException(error);
                }
            }
        }
    }
}
