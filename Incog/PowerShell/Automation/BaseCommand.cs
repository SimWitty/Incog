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
        /// Gets the cmdlet Globally Unique Identifier from executing assembly.
        /// </summary>
        public string CmdletGuid { get; private set; }

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
            // Set the Globally Unique Identifier
            this.CmdletGuid = ((System.Runtime.InteropServices.GuidAttribute)System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(System.Runtime.InteropServices.GuidAttribute), false).GetValue(0)).Value.ToString();
            
            // Set the value indicating the -Verbose switch was used
            this.IsVerbose = this.MyInvocation.BoundParameters.ContainsKey("Verbose");

            // Do all the preflight checks here.
            this.CheckIfAdministrator();
            this.CheckWindowVersion();
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

        /// <summary>
        /// Check the Windows Operating System version.
        /// </summary>
        private void CheckWindowVersion()
        {
            string error = string.Format("The {0} cmdlet has not been tested on this OS. Please use Windows 7, Windows 8, Windows Server 2008 R2, or Windows Server 2012.", this.CmdletName);

            Version windows = Environment.OSVersion.Version;
            if (windows.Major < 6) throw new ApplicationException(error);

            // if (windows.Minor < 1) throw new ApplicationException(error);
            // For further information, please see: http://msdn.microsoft.com/en-us/library/ms724832(v=vs.85).aspx
        }
    }
}
