// <copyright file="HttpServer.cs" company="SimWitty (http://www.simwitty.org)">
//     Copyright © 2013 and distributed under the BSD license.
// </copyright>

namespace Incog.Servers
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using Incog.Steganography;
    using Incog.Tools;

    /// <summary>
    /// Simple Hypertext Transfer Protocol (HTTP) Web Server.
    /// </summary>
    public class HttpServer
    {
        /// <summary>
        /// Private TCP/IP listener.
        /// </summary>
        private TcpListener listener;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpServer" /> class.
        /// </summary>
        public HttpServer()
        {
            this.Address = IPAddress.Any;
            this.Port = 80;
            this.Active = false;
            this.MessageQueue = new Queue();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpServer" /> class.
        /// </summary>
        /// <param name="address">Sets the IP address that the server listens on.</param>
        /// <param name="port">Sets the TCP/IP Port that the HTTP server listens on.</param>
        public HttpServer(IPAddress address, int port)
        {
            this.Address = address;
            this.Port = port;
            this.Active = false;
            this.MessageQueue = new Queue();
        }

        /// <summary>
        /// Gets a value indicating whether the server is actively listening or not.
        /// </summary>
        public bool Active { get; private set; }

        /// <summary>
        /// Gets the value of the IP Address hosting the server.
        /// </summary>
        public IPAddress Address { get; private set; }

        /// <summary>
        /// Gets the value of the TCP/IP Port that the HTTP server is listening on.
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// Gets or sets the message queue. Queued messages will be dequeued and returned in web pages.
        /// </summary>
        public Queue MessageQueue { get; set; }

        /// <summary>
        /// Begin listening.
        /// </summary>
        public void Start()
        {
            this.listener = new TcpListener(this.Address, this.Port);
            this.listener.Start();
            this.Active = true;

            while (this.Active)
            {
                TcpClient s = this.listener.AcceptTcpClient();
                HttpProcessor processor = new HttpProcessor(s, this);
                Thread thread = new Thread(new ThreadStart(processor.Process));
                thread.Start();
                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Stop listening
        /// </summary>
        public void Stop()
        {
            this.Active = false;
        }

        /// <summary>
        /// The handler for HTTP GET requests.
        /// </summary>
        /// <param name="processor">The HTTP processor.</param>
        public void HttpGetHandler(HttpProcessor processor)
        {
            processor.WriteSuccess();

            System.Security.SecureString passphrase = new System.Security.SecureString();

            string password = "Passw0rd!";
            char[] characters = password.ToCharArray();
            for (int i = 0; i < characters.Length; i++)
            {
                passphrase.AppendChar(characters[i]);
            }

            SimWitty.Library.Core.Encrypting.Cryptkeeper myCrypt = new SimWitty.Library.Core.Encrypting.Cryptkeeper(passphrase);
            SimWitty.Library.Core.Encrypting.DerivedValue derived = new SimWitty.Library.Core.Encrypting.DerivedValue(passphrase);
            string result = derived.GetString(3, 10);

            // No result? No handshake. Done.
            if (processor.RequestURL.IndexOf(result) == -1) return; 

            // Parse out the host name and fetch the next page.
            string hostname = string.Empty;
            string absolutePath = processor.RequestURL;

            string[] values = absolutePath.Split(new string[] { result }, StringSplitOptions.RemoveEmptyEntries);
            absolutePath = values[0].Trim();
            string hostBase64 = values[1].Trim();
            byte[] hostBytes = System.Convert.FromBase64String(hostBase64);
            hostname = Encoding.ASCII.GetString(hostBytes);

            Uri link = new Uri(string.Concat("http://", hostname, absolutePath));

            try
            {
                WebClient web = new WebClient();
                byte[] page = web.DownloadData(link);
                web.Dispose();

                if (this.MessageQueue.Count > 0)
                {
                    string message = (string)this.MessageQueue.Dequeue();
                    WebPageSteganography stegopage = new WebPageSteganography(page, passphrase);
                    stegopage.WriteValue(message);
                    page = stegopage.GetBytes();

                    Console.WriteLine(stegopage.ReadValue());
                }

                processor.StreamOutput.Write(page);
                processor.StreamOutput.Flush();
                processor.WriteSuccess();

                Console.WriteLine(link);
                Console.WriteLine(page.Length.ToString());
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                processor.StreamOutput.Write("<html><p>Ping! Something odd happened while retrieving ... " + link.ToString() + "</p><p>" + ex.ToString() + "</p></html>");
            }
        }
        
        /// <summary>
        /// The handler for HTTP POST requests.
        /// TODO: This code has not been tested. It should work, right? I mean, what could possibly go wrong?
        /// </summary>
        /// <param name="processor">The HTTP processor.</param>
        public void HttpPostHandler(HttpProcessor processor)
        {
            Console.WriteLine("POST request: {0}", processor.RequestURL);
            string data = string.Empty;

            do
            {
                int next = processor.StreamInput.ReadByte();
                if (next == -1) break;

                data += Convert.ToChar(next);
            }
            while (true);
            
            processor.WriteLine("<html>A post? Thank you. I needed that.</html>");
        }
    }
}
