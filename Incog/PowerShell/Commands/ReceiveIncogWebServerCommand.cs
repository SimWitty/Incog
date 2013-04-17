// <copyright file="ReceiveIncogWebServerCommand.cs" company="SimWitty (http://www.simwitty.org)">
//     Copyright © 2013 and distributed under the BSD license.
// </copyright>

// Source Boston 2013 Demo

namespace Incog.PowerShell.Commands
{
    using System;
    using System.IO;
    using System.Management.Automation;
    using System.Net;
    using System.Net.NetworkInformation;
    using Incog.Steganography; // WebPageSteganography
    using Incog.Tools; // ChannelTools, ConsoleTools
    using Microsoft.PowerShell.Commands;
    using SimWitty.Library.Core.Encrypting; // Cryptkeeper

    /// <summary>
    /// Receive incognito messages over an covert channel.
    /// Starts a web brower and fetches pages with messages embedded within the pages.
    /// </summary>
    [System.Management.Automation.Cmdlet(
        System.Management.Automation.VerbsCommunications.Receive,
        Incog.PowerShell.Nouns.IncogWebServer)]
    public class ReceiveIncogWebServerCommand : Incog.PowerShell.Automation.ChannelCommand
    {
        /// <summary>
        /// URL Website links used to pull pages from the Incog Web Server.
        /// </summary>
        private Uri[] links;

        /// <summary>
        /// Gets or sets the TCP port to poll the server on.
        /// </summary>
        [Parameter(Mandatory = false)]
        public ushort TCP { get; set; }

        /// <summary>
        /// Gets or sets the TCP port to poll the server on.
        /// </summary>
        [Parameter(Mandatory = true)]
        public FileInfo BrowserHistoryFile { get; set; }
        
        /// <summary>
        /// Provides a one-time, preprocessing functionality for the cmdlet.
        /// </summary>
        protected override void BeginProcessing()
        {
            // Initialize parameters and base Incog cmdlet components
            this.InitializeComponent();

            // Verify the browser history file path
            this.BrowserHistoryFile = ConsoleTools.ExpandAndVerifyPath(this, this.BrowserHistoryFile);
            
            StreamReader history = new StreamReader(this.BrowserHistoryFile.FullName);
            string text = history.ReadToEnd();
            string[] lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            this.links = new Uri[lines.Length];

            for (int i = 0; i < this.links.Length; i++)
            {
                this.links[i] = new Uri(lines[i]);
                this.WriteVerbose(string.Format("Loading ... {0}", this.links[i].ToString()));
            }

            // Invoke Interative Mode if selected
            if (this.Interactive) this.InteractiveMode();

            // Set the default TCP port
            if (this.TCP == 0) this.TCP = 80;

            // Get a derived value to indicate where the path and host split
            DerivedValue derived = new DerivedValue(this.Passphrase);
            string split = derived.GetString(3, 10);

            int linkIndex = 0;

            do
            {
                // Get the host and path of the next URL
                string host = this.links[linkIndex].Host;
                string absolutePath = this.links[linkIndex].AbsolutePath;

                // Get the host as a base64 encoded string
                byte[] hostBytes = System.Text.Encoding.ASCII.GetBytes(host);
                string hostBase64 = System.Convert.ToBase64String(hostBytes);

                // Get the resulting link -- absolute path and host in base 64
                Uri resultingLink = new Uri(string.Concat(
                    "http://", 
                    this.RemoteAddress.ToString(),
                    ":",
                    this.TCP.ToString(),
                    absolutePath,
                    split,
                    hostBase64));

                try
                {
                    // Create a request for the URL and set the timeout
                    WebRequest request = WebRequest.Create(resultingLink);
                    request.Timeout = request.Timeout * 2;

                    // Get the response
                    WebResponse response = request.GetResponse();

                    // Display the status
                    this.WriteVerbose(resultingLink.ToString());
                    this.WriteVerbose(((HttpWebResponse)response).StatusDescription);

                    // Get the stream containing content returned by the server
                    Stream dataStream = response.GetResponseStream();

                    // Read in the stream into a buffer
                    int growBufferBy = 1024;
                    byte[] buffer = new byte[10240];
                    byte[] page;
                    int index = 0;

                    do
                    {
                        int i = dataStream.ReadByte();
                        if (i == -1)
                        {
                            page = new byte[index];
                            Array.Copy(buffer, page, index);
                            break;
                        }

                        buffer[index] = Convert.ToByte(i);
                        index++;

                        if (index == buffer.Length)
                        {
                            byte[] temp = new byte[buffer.Length + growBufferBy];
                            Array.Copy(buffer, temp, buffer.Length);

                            buffer = new byte[temp.Length];
                            Array.Copy(temp, buffer, temp.Length);
                        }
                    }
                    while (true);

                    response.Close();

                    // Feed the bytes into a stego page and test
                    WebPageSteganography stegoPage = new WebPageSteganography(page, this.Passphrase);
                    string message = stegoPage.ReadValue();

                    // Break on exit
                    if (message.Trim().ToLower() == "exit") break;

                    if (message != string.Empty)
                    {
                        if (this.Interactive)
                        {
                            Console.WriteLine("{0} > {1}", this.RemoteAddress.ToString(), message);
                        }
                        else
                        {
                            this.WriteObject(message);
                        }
                    }
                }
                catch (System.Net.Sockets.SocketException ex)
                {
                    this.WriteVerbose(ex.InnerException.ToString());
                    break;
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                    {
                        if (ex.InnerException.GetType() == typeof(System.Net.Sockets.SocketException))
                        {
                            this.WriteWarning(ex.InnerException.Message);
                            break;
                        }
                    }

                    this.WriteWarning(ex.ToString());
                }
                
                linkIndex++;
                if (linkIndex == this.links.Length) linkIndex = 0;
                System.Threading.Thread.Sleep(500);
            }
            while (true);
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
    }
}