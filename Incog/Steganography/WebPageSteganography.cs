// <copyright file="WebPageSteganography.cs" company="SimWitty (http://www.simwitty.org)">
//     Copyright © 2013 and distributed under the BSD license.
// </copyright>

namespace Incog.Steganography
{
    using System;
    using System.IO;
    using System.Security;
    using System.Text;
    using Incog.Tools;
    using SimWitty.Library.Core.Encrypting;

    /// <summary>
    /// The steganography web page class allows reading and writing short messages into HTML.
    /// </summary>
    public class WebPageSteganography
    {
       /// <summary>
        /// The encryption passphrase.
        /// </summary>
        private SecureString passphrase;

        /// <summary>
        /// The HTML page as a byte array with the steganographic message.
        /// </summary>
        private byte[] steganographyBytes = new byte[0];

        /// <summary>
        /// Is the object wrapping a File Info object?
        /// </summary>
        private bool isFileInfo = false;

        /// <summary>
        /// The internal File Info for reading and writing to the file.
        /// </summary>
        private FileInfo webPageFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebPageSteganography" /> class.
        /// </summary>
        /// <param name="file">A file containing HTML.</param>
        /// <param name="passphrase">The passphrase to use for encryption and decryption.</param>
        public WebPageSteganography(FileInfo file, SecureString passphrase)
        {
            // Update the file info
            this.isFileInfo = true;
            this.webPageFile = new FileInfo(file.FullName);

            // Load the bytes from the file and then reset last access time.
            DateTime created = this.webPageFile.CreationTime;
            DateTime modified = this.webPageFile.LastWriteTime;
            DateTime accessed = this.webPageFile.LastAccessTime;

            BinaryReader readFile = new BinaryReader(File.Open(file.FullName, FileMode.Open));
            this.OriginalBytes = new byte[readFile.BaseStream.Length];
            this.OriginalBytes = readFile.ReadBytes(this.OriginalBytes.Length);
            readFile.Close();

            this.webPageFile.CreationTime = created;
            this.webPageFile.LastWriteTime = modified;
            this.webPageFile.LastAccessTime = accessed;

            // Setup the original steganography bytes
            this.CopyOriginalToStego();

            // Setup the index positions for each steganographic message byte
            uint length = Math.Min(4096, this.Maximum - this.Minimum);
            this.Positions = ChannelTools.MathSetRandom(this.Minimum, this.Maximum, length);

            // Save the password
            this.passphrase = passphrase.Copy();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebPageSteganography" /> class.
        /// </summary>
        /// <param name="html">The HTML from the stream byte array.</param>
        /// <param name="passphrase">The passphrase to use for encryption and decryption.</param>
        public WebPageSteganography(byte[] html, SecureString passphrase)
        {
            // Setup the original HTML bytes
            this.OriginalBytes = new byte[html.Length];
            Array.Copy(html, this.OriginalBytes, this.OriginalBytes.Length);

            // Setup the original steganography bytes
            this.CopyOriginalToStego();

            // Setup the index positions for each steganographic message byte
            uint length = Math.Min(4096, this.Maximum - this.Minimum);
            this.Positions = ChannelTools.MathSetRandom(this.Minimum, this.Maximum, length);
            
            // Save the password
            this.passphrase = passphrase.Copy();
        }

        /// <summary>
        /// Gets a value indicating where the HTML ends. 
        /// This is the last closing-tag / less-than character. (<html></html>)
        /// </summary>
        public uint Maximum
        {
            get
            {
                int index = this.OriginalText.LastIndexOf('<');
                if (index == -1) return (uint)this.OriginalText.Length - 1;
                else return (uint)index;
            }
        }

        /// <summary>
        /// Gets a value indicating where the HTML begins. 
        /// This is the first opening-tag / greater-than character. (<html></html>)
        /// </summary>
        public uint Minimum
        {
            get
            {
                int index = this.OriginalText.IndexOf('>');
                if (index == -1) return 1;
                else return (uint)index;
            }
        }

        /// <summary>
        /// Gets the original HTML in a byte array.
        /// </summary>
        public byte[] OriginalBytes { get; private set; }

        /// <summary>
        /// Gets the original HTML converted from byte array to ASCII text.
        /// Note: The ASCII conversion will erase all bytes with values greater than 127.
        /// </summary>
        public string OriginalText { get; private set; }

        /// <summary>
        /// Gets the index numbers were the steganography is positioned within the byte array.
        /// </summary>
        public uint[] Positions { get; private set; }

        /// <summary>
        /// Get the HTML bytes with the steganographic message.
        /// </summary>
        /// <returns>Returns a byte array.</returns>
        public byte[] GetBytes()
        {
            if (this.steganographyBytes.Length != this.OriginalBytes.Length)
            {
                // Setup the zeroed message
                this.EmbedBytesInPage(BitConverter.GetBytes((uint)0));
            }

            return this.steganographyBytes;
        }

        /// <summary>
        /// Read -- decrypt, decode, and display the steganographic message.
        /// </summary>
        /// <returns>Returns the clear text message.</returns>
        public string ReadValue()
        {
            // Always use GetBytes because GetBytes does error handling
            byte[] buffer = this.GetBytes();

            // Extract the first four bytes as an unsigned integer specifying length
            byte[] lengthBytes = new byte[4];
            lengthBytes[0] = buffer[this.Positions[0]];
            lengthBytes[1] = buffer[this.Positions[1]];
            lengthBytes[2] = buffer[this.Positions[2]];
            lengthBytes[3] = buffer[this.Positions[3]];
            uint length = BitConverter.ToUInt32(lengthBytes, 0);

            // If the length is 0, there is no message, and we return an empty string
            if (length == 0) return string.Empty;

            // Extract the encrypted bytes
            byte[] cipherBytes = new byte[length];

            for (int i = 0; i < cipherBytes.Length; i++)
            {
                // It should not happen, but just in case, prevent an index out of range exception
                if (i >= buffer.Length) break;

                // We start at length bytes and increment the position index from that point
                uint position = this.Positions[lengthBytes.Length + i];
                byte stegoByte = buffer[position];
                cipherBytes[i] = stegoByte;
            }

            // Decrypt and return
            Cryptkeeper myCrypt = new Cryptkeeper(this.passphrase);
            byte[] cleartext = myCrypt.GetBytes(cipherBytes, Cryptkeeper.Action.Decrypt);
            string value = Encoding.Unicode.GetString(cleartext);
            return value;
        }

        /// <summary>
        /// Write -- encode, encrypt, and hide the steganographic message.
        /// </summary>
        /// <param name="value">The text value to write into the page.</param>
        public void WriteValue(string value)
        {
            Cryptkeeper myCrypt = new Cryptkeeper(this.passphrase);

            byte[] cipherBytes = myCrypt.GetBytes(value, Cryptkeeper.Action.Encrypt);
            byte[] cleartext = myCrypt.GetBytes(cipherBytes, Cryptkeeper.Action.Decrypt);

            byte[] lengthBytes = BitConverter.GetBytes((uint)cipherBytes.Length);
            byte[] buffer = new byte[lengthBytes.Length + cipherBytes.Length];

            Array.Copy(lengthBytes, 0, buffer, 0, lengthBytes.Length);
            Array.Copy(cipherBytes, 0, buffer, lengthBytes.Length, cipherBytes.Length);

            this.EmbedBytesInPage(buffer);

            // If this is a file info, write the bytes down to the file while preserving the file dates
            if (this.isFileInfo)
            {
                // Write the bytes to the file and then reset last access time.
                DateTime created = this.webPageFile.CreationTime;
                DateTime modified = this.webPageFile.LastWriteTime;
                DateTime accessed = this.webPageFile.LastAccessTime;

                BinaryWriter writeFile = new BinaryWriter(File.Open(this.webPageFile.FullName, FileMode.Create));
                writeFile.Write(this.GetBytes());
                writeFile.Close();

                this.webPageFile.CreationTime = created;
                this.webPageFile.LastWriteTime = modified;
                this.webPageFile.LastAccessTime = accessed;
            }
        }

        /// <summary>
        /// Copy the original bytes to the steganography bytes.
        /// </summary>
        private void CopyOriginalToStego()
        {
            this.OriginalText = Encoding.ASCII.GetString(this.OriginalBytes);
            this.steganographyBytes = new byte[this.OriginalBytes.Length];
            Array.Copy(this.OriginalBytes, this.steganographyBytes, this.steganographyBytes.Length);
        }

        /// <summary>
        /// Write the bytes in the buffer to the positions within the byte array.
        /// </summary>
        /// <param name="buffer">An array of bytes to hide within the HTML.</param>
        private void EmbedBytesInPage(byte[] buffer)
        {
            // Sanity check -- are there enough bytes?
            uint maximumLength = this.Maximum - this.Minimum;
            if (buffer.Length > maximumLength) throw new ApplicationException("The web page does not have sufficient bytes to write a message of this size.");

            // Sanity check -- are there enough positions?
            if (buffer.Length > this.Positions.Length) throw new ApplicationException("The web page does not have sufficient bytes to write a message of this size.");

            // Embed the message into the html byte array
            for (int i = 0; i < buffer.Length; i++)
            {
                uint position = this.Positions[i];
                byte stegoByte = buffer[i];
                this.steganographyBytes[position] = stegoByte;
            }
        }
    }
}