// <copyright file="HttpProcessor.cs" company="SimWitty (http://www.simwitty.org)">
//     Copyright © 2013 and distributed under the BSD license.
// </copyright>

namespace Incog.Servers
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Net.Sockets;
    using System.Threading;
    
    /// <summary>
    /// Simple Hypertext Transfer Protocol (HTTP) request processor.
    /// </summary>
    public class HttpProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpProcessor" /> class.
        /// </summary>
        /// <param name="socket">Pass in the client socket.</param>
        /// <param name="server">Pass in the HTTP server.</param>
        public HttpProcessor(TcpClient socket, HttpServer server)
        {
            this.Socket = socket;
            this.Server = server;
            this.Headers = new Hashtable();
        }

        /// <summary>
        /// Gets the "Content-Length" from the HTTP header. 
        /// Used in HTTP Post requests, Content-Length specifies the bytes in the request's data.
        /// </summary>
        public int ContentLength { get; private set; }

        /// <summary>
        /// Gets the HTTP headers in the request.
        /// </summary>
        public Hashtable Headers { get; private set; }

        /// <summary>
        /// Gets the HTTP method of the request. Typically, this is POST or GET.
        /// </summary>
        public string Method { get; private set; }

        /// <summary>
        /// Gets the HTTP version of the request.
        /// </summary>
        public string RequestProtocol { get; private set; }

        /// <summary>
        /// Gets the URL of the request.
        /// </summary>
        public string RequestURL { get; private set; }

        /// <summary>
        /// Gets the HTTP server servicing the request.
        /// </summary>
        public HttpServer Server { get; private set; }

        /// <summary>
        /// Gets the underlying HTTP socket.
        /// </summary>
        public TcpClient Socket { get; private set; }

        /// <summary>
        /// Gets the input stream.
        /// </summary>
        public Stream StreamInput { get; private set; }

        /// <summary>
        /// Gets the output stream.
        /// </summary>
        public BinaryWriter StreamOutput { get; private set; }

        /// <summary>
        /// Process incoming requests.
        /// </summary>
        public void Process()
        {
            // Use buffered stream to obtain the raw data (after the headers)
            this.StreamInput = new BufferedStream(this.Socket.GetStream());
            this.StreamOutput = new BinaryWriter(new BufferedStream(this.Socket.GetStream()));

            try
            {
                this.ParseRequest();
                this.ParseHeaders();
            }
            catch
            {
                this.WriteFailure();
                this.StreamOutput.Flush();
                this.Socket.Close();
                return;
            }

            switch (this.Method)
            {
                case "GET":
                    this.Server.HttpGetHandler(this);
                    break;

                case "POST":
                    this.Server.HttpPostHandler(this);
                    break;

                default:
                    // Not implemented
                    this.WriteFailure();
                    break;
            }
     
            this.Socket.Close();
        }

        /// <summary>
        /// Write an HTTP success (200).
        /// </summary>
        public void WriteSuccess()
        {
            this.WriteLine("HTTP/1.0 200 OK");
            this.WriteLine("Content-Type: text/html");
            this.WriteLine("Connection: close");
            this.WriteLine(string.Empty);
        }

        /// <summary>
        /// Write a HTTP failure (500).
        /// </summary>
        public void WriteFailure()
        {
            this.WriteLine("HTTP/1.0 500 Internal Server Error");
            this.WriteLine("Connection: close");
            this.WriteLine(string.Empty);
        }

        /// <summary>
        /// Write a single line of ASCII text followed by the new line character.
        /// </summary>
        /// <param name="value">The string in ASCII format.</param>
        public void WriteLine(string value)
        {
            value += Environment.NewLine;
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(value);
            this.StreamOutput.Write(bytes);
        }

        /// <summary>
        /// Parse the request to load the method, URL, and protocol.
        /// </summary>
        private void ParseRequest()
        {
            string request = this.ReadLine();
            string[] tokens = request.Split(' ');
            if (tokens.Length != 3) throw new ApplicationException("The HTTP processor received an invalid HTTP request.");

            this.Method = tokens[0].ToUpper();
            this.RequestURL = tokens[1];
            this.RequestProtocol = tokens[2];
        }

        /// <summary>
        /// Parse the headers.
        /// </summary>
        private void ParseHeaders()
        {
            string line = string.Empty;

            while ((line = this.ReadLine()) != null)
            {
                // The headers have been loaded when we hit the empty line
                if (line == string.Empty) return;

                // Split on : because headers come in name:value pairs
                int separator = line.IndexOf(':');
                if (separator == -1) throw new ApplicationException("The HTTP processor received an invalid HTTP request: could not parse the header."); 

                string name = line.Substring(0, separator).Trim();
                int index = separator + 1;
                string value = line.Substring(index, line.Length - index).Trim();

                this.Headers[name] = value;
            }

            if (this.Headers.ContainsKey("Content-Length"))
            {
                this.ContentLength = Convert.ToInt32(this.Headers["Content-Length"]);
            }
            else
            {
                this.ContentLength = -1;
            }
        }

        /// <summary>
        /// Read the next line from the HTTP request.
        /// </summary>
        /// <returns>Returns the next line from the underlying input stream.</returns>
        private string ReadLine()
        {
            string result = string.Empty;

            do
            {
                int next = this.StreamInput.ReadByte();

                // If the stream returns -1, wait, and then retry the stream
                if (next == -1)
                {
                    Thread.Sleep(10);
                    continue;
                }

                // New line is \r (carriage return) and \n (new line). Continue on \r and break on \n
                if (next == '\r') continue;
                if (next == '\n') break;

                // Append the character
                result += Convert.ToChar(next);
            }
            while (true);

            return result;
        }
    }
}
