// <copyright file="IncogStream.cs" company="SimWitty (http://www.simwitty.org)">
//     Copyright © 2013 and distributed under the BSD license.
// </copyright>

namespace Incog.Messaging
{
    using System;
    using System.IO;
    using System.Security; // SecureString
    using System.Text; // Encoding
    using SimWitty.Library.Core.Encrypting; // Cryptkeeper
    using SimWitty.Library.Core.Tools; // ShannonEntropy

    /// <summary>
    /// Incog Pipe Stream defines the data protocol for reading and writing strings on our stream.
    /// </summary>
    public class IncogStream
    {
        /// <summary>
        /// The underlying stream.
        /// </summary>
        private Stream innerStream;

        /// <summary>
        /// The encryption and decryption handler.
        /// </summary>
        private Cryptkeeper mycrypt;

        /// <summary>
        /// The target Shannon Entropy to use when writing bytes.
        /// </summary>
        private double targetEntropy = 0;

        /// <summary>
        ///  Initializes a new instance of the <see cref="IncogStream" /> class.
        /// </summary>
        /// <param name="stream">The stream to write to and read from.</param>
        /// <param name="passphrase">The secure string used to encrypt messages written to the stream and decrypt messages read from the stream.</param>
        public IncogStream(Stream stream, SecureString passphrase)
        {
            this.innerStream = stream;
            this.mycrypt = new Cryptkeeper(passphrase);
            this.targetEntropy = 0;
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="IncogStream" /> class.
        /// </summary>
        /// <param name="stream">The stream to write to and read from.</param>
        /// <param name="passphrase">The secure string used to encrypt messages written to the stream and decrypt messages read from the stream.</param>
        /// <param name="entropy">The approximate Shannon Entropy value (between 1.00 and 7.80) to spoof.</param>
        public IncogStream(Stream stream, SecureString passphrase, double entropy)
        {
            this.innerStream = stream;
            this.mycrypt = new Cryptkeeper(passphrase);
            if (entropy > 0) this.targetEntropy = entropy;
        }

        /// <summary>
        /// Read a string from the underlying stream.
        /// </summary>
        /// <returns>Returns text from the stream.</returns>
        public string ReadString()
        {
            byte[] buffer = this.ReadBytes();
            if (buffer == null) return string.Empty;
            else return Encoding.Unicode.GetString(buffer);
        }

        /// <summary>
        /// Read a byte array from the underlying stream.
        /// </summary>
        /// <returns>Returns bytes from the stream.</returns>
        public byte[] ReadBytes()
        {
            // Fetch the unencrypted length from the beginning of the stream
            ushort length = this.ReadUInt16();
            if (length == 0) return null;

            // Create a buffer the size of the length and populate the buffer
            byte[] buffer = new byte[length];
            this.innerStream.Read(buffer, 0, length);

            // Decrypt the buffer and return the results
            try
            {
                byte[] cleartext = this.mycrypt.GetBytes(buffer, Cryptkeeper.Action.Decrypt);
                return cleartext;
            }
            catch (System.Security.Cryptography.CryptographicException)
            {
                // The decryption process failed
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Write a string to the underlying buffer.
        /// </summary>
        /// <param name="text">The text string to write to the buffer. The string will be converted to Unicode.</param>
        /// <returns>Returns the number of bytes written.</returns>
        public int WriteString(string text)
        {
            // Ensure the string parameter is populated
            if (text == null) return -1;

            // Trim and boundary check. Don't send blank strings.
            text = text.Trim();
            if (text.Length == 0) return -1;

            // Send as bytes
            byte[] buffer = Encoding.Unicode.GetBytes(text);
            return this.WriteBytes(buffer);
        }

        /// <summary>
        /// Write a byte array to the underlying buffer.
        /// </summary>
        /// <param name="bytes">The byte array to write to the buffer.</param>
        /// <returns>Returns the number of bytes written.</returns>
        public int WriteBytes(byte[] bytes)
        {
            // Ensure the bytes parameter is populated
            if (bytes == null) return -1;

            // Encrypt the bytes
            byte[] cipherbytes = this.mycrypt.GetBytes(bytes, Cryptkeeper.Action.Encrypt);

            // Boundary check the length
            if (cipherbytes.Length > ushort.MaxValue)
            {
                string error = string.Format(
                    "The parameter exceeded the length. The actual length is {0} and the maximum length of a byte array is {1}.", 
                    cipherbytes.Length.ToString(),
                    ushort.MaxValue.ToString());
                throw new ApplicationException(error);
            }

            // Set the length
            this.WriteUInt16((ushort)cipherbytes.Length);

            // Send the bytes along their merry way
            this.innerStream.Write(cipherbytes, 0, cipherbytes.Length);

            // Add entropy if needed
            if (this.targetEntropy > 0)
            {
                byte[] noise = ShannonEntropy.GetNoise(cipherbytes, this.targetEntropy, 1024);
                this.innerStream.Write(noise, 0, noise.Length);
            }

            this.innerStream.Flush();
            return 2 + cipherbytes.Length;
        }

        /// <summary>
        /// Read a 2-byte unsigned integer, unencrypted, from the stream.
        /// If either the first or second byte is missing (-1), then 0 is returned.
        /// </summary>
        /// <returns>A 2-byte unsigned integer read from this stream.</returns>
        private ushort ReadUInt16()
        {
            // Get the first byte, which contains the length, and boundary check
            int firstByte = this.innerStream.ReadByte();
            if (firstByte == -1) return 0;

            // Get the second byte, which contains the length, and boundary check
            int secondByte = this.innerStream.ReadByte();
            if (secondByte == -1) return 0;

            // Convert the bytes to the 16-bit unsigned integer
            ushort result = (ushort)firstByte;
            result += (ushort)(secondByte * 256);
            return result;
        }

        /// <summary>
        /// Write a 2-byte unsigned integer, unencrypted, to the stream.
        /// </summary>
        /// <param name="value">A 2-byte unsigned integer to write.</param>
        private void WriteUInt16(ushort value)
        {
            byte[] bytes = BitConverter.GetBytes(value);

            for (int i = 0; i < bytes.Length; i++)
            {
                this.innerStream.WriteByte(bytes[i]);
            }
        }
    }
}
