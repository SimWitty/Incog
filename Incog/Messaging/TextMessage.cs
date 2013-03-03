// <copyright file="TextMessage.cs" company="SimWitty (http://www.simwitty.org)">
//     Copyright © 2013 and distributed under the BSD license.
// </copyright>

namespace Incog.Messaging
{
    using System;
    using System.Collections;
    using SimWitty.Library.Core.Encrypting;

    /// <summary>
    /// Text messaging layer class for coordinating a message pipeline in channels such as DNS.
    /// </summary>
    public class TextMessage
    {
        /// <summary>
        /// Represents whether the message has been cleared.
        /// </summary>
        private bool cleared = false;

        /// <summary>
        /// The id of the last fragment in the message.
        /// </summary>
        private ushort finalid = 0;

        /// <summary>
        /// An array of Byte arrays, each Byte array is a fragment in a complete binary message.
        /// </summary>
        private byte[][] fragments = new byte[1][];

        /// <summary>
        /// The message identifier.
        /// </summary>
        private ushort id = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextMessage" /> class.
        /// </summary>
        /// <param name="identifier">The unique identifier number of the message.</param>
        public TextMessage(ushort identifier)
        {
            this.id = identifier;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextMessage" /> class.
        /// </summary>
        /// <param name="identifier">The unique identifier number of the message.</param>
        /// <param name="fragmentCount">The number of fragments in the message.</param>
        public TextMessage(ushort identifier, ushort fragmentCount)
        {
            this.id = identifier;
            this.Length = fragmentCount;
        }

        /// <summary>
        /// Gets a value indicating whether the message has been cleared.
        /// </summary>
        public bool Cleared
        {
            get { return this.cleared; }
        }

        /// <summary>
        /// Gets a value indicating whether the message is complete with all fragments populated.
        /// </summary>
        public bool Complete
        {
            get
            {
                // The message is not complete until the final id is set
                if (this.finalid == 0) return false;

                // If any of the fragments are null or empty, return false
                for (int i = 0; i < this.fragments.Length; i++)
                {
                    if (this.fragments[i] == null) return false;
                    if (this.fragments[i].Length == 0) return false;
                }

                // All fragments have values? Return true
                return true;
            }
        }

        /// <summary>
        /// Gets or sets the id of the last fragment in the message.
        /// Please note the value can only be set once. Once set, future calls to this property will be ignored.
        /// </summary>
        public ushort FinalFragmentId
        {
            get { return this.finalid; }
            set { if (this.finalid == 0) this.finalid = value; }
        }

        /// <summary>
        /// Gets or sets an array of Byte arrays. Each Byte array is a fragment in a complete binary message.
        /// Please note the value can only be set once.
        /// </summary>
        public byte[][] Fragments
        {
            get { return this.fragments; }
            set { if (this.finalid == 0) this.fragments = value; }
        }

        /// <summary>
        /// Gets or sets the number of message fragments. Setting the length causes an Array.Resize.
        /// </summary>
        public ushort Length
        {
            get
            {
                return (ushort)this.fragments.Length;
            }

            set
            {
                if (value > this.fragments.Length)
                {
                    Array.Resize(ref this.fragments, (int)value);
                }
            }
        }

        /// <summary>
        /// Gets the unique message identifier.
        /// </summary>
        public ushort MessageId
        {
            get { return this.id; }
        }

        /// <summary>
        /// Clear the message fragments, set Complete to False, and preserve the message identifier.
        /// </summary>
        public void Clear()
        {            
            // Reset the final identifier 
            this.finalid = 0;

            // Null the Byte array fragments
            for (int i = 0; i < this.fragments.Length; i++)
            {
                this.fragments[i] = null;
            }

            // Mark the message as cleared
            this.cleared = true;
        }

        /// <summary>
        /// Gets the value of the Byte array at the specific position in the Fragments array.
        /// </summary>
        /// <param name="index">The zero-based index of the Byte array fragment to get.</param>
        /// <returns>The value of the Byte array at the specified index.</returns>
        public byte[] Get(int index)
        {
            if (index < 0 || index >= this.Length) throw new System.ArgumentOutOfRangeException();
            return this.fragments[index];
        }

        /// <summary>
        /// Returns the completed message as an array of Bytes.
        /// </summary>
        /// <returns>An array of Bytes containing all the message fragments.</returns>
        public byte[] GetBytes()
        {
            if (!this.Complete) return null;

            int length = 0;

            for (int i = 0; i < this.Length; i++)
            {
                length += this.fragments[i].Length;
            }

            byte[] result = new byte[length];

            int destinationIndex = 0;

            for (int i = 0; i < this.Length; i++)
            {
                Array.Copy(this.fragments[i], 0, result, destinationIndex, this.fragments[i].Length);
                destinationIndex += this.fragments[i].Length;
            }

            return result;
        }
        
        /// <summary>
        /// Decrypts the fragments values of this instance into a clear text message string using the passphrase.
        /// </summary>
        /// <param name="passphrase">Decryption key</param>
        /// <returns>Decrypted message string</returns>
        public string GetDecryptedMessage(System.Security.SecureString passphrase)
        {
            if (!this.Complete) return string.Empty;

            Cryptkeeper mycrypt = new Cryptkeeper(passphrase);
            string cleartext = mycrypt.Decrypt(this.GetBytes());
            return cleartext;
        }

        /// <summary>
        /// Sets the Byte array at a specific position in the Fragments array to a specific value.
        /// </summary>
        /// <param name="index">The zero-based index of the byte array fragment to get.</param>
        /// <param name="value">The Byte array value to assign to the fragment.</param>
        public void Set(int index, byte[] value)
        {
            if (index < 0 || index >= this.Length) throw new System.ArgumentOutOfRangeException();
        }
    }
}