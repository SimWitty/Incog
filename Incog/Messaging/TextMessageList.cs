// <copyright file="TextMessageList.cs" company="SimWitty (http://www.simwitty.org)">
//     Copyright © 2013 and distributed under the BSD license.
// </copyright>

namespace Incog.Messaging
{
    using System;
    using SimWitty.Library.Core.Encoding;
    using SimWitty.Library.Core.Tools;

    /// <summary>
    /// Text messaging list class for coordinating a message pipeline in channels such as DNS.
    /// </summary>
    public class TextMessageList : System.Collections.Generic.List<TextMessage>
    {
        /// <summary>
        /// A message fragment is a Byte array, and this value is the maximum length of a fragment.
        /// Fragments below the maximum 
        /// </summary>
        private int maximumByteLength = 31;

        /// <summary>
        /// Gets or sets a value that specifies the maximum length of a message fragment (byte[].Length).
        /// </summary>
        public int MaximumByteLength
        {
            get { return this.maximumByteLength; }
            set { this.maximumByteLength = value; }
        }

        /// <summary>
        /// Add a new message fragment to the message list
        /// </summary>
        /// <param name="base32encodedAESencryptedFragment">A message fragment that is Base32 encoded and AES encrypted, and contains a message id, fragment id, and length value.</param>
        public void AddToMessages(string base32encodedAESencryptedFragment)
        {
            // Boundary check the encoded and encrypted string
            if (base32encodedAESencryptedFragment == null) return;
            if (base32encodedAESencryptedFragment.Length == 0) return;

            // Unblend to extract the encoded values (message id, fragment id, length) and encrypted message fragment
            byte[] encrypted = new byte[1];
            byte[] encoded = new byte[4];
            BinaryTools.UnblendBits(Base32.GetBits(base32encodedAESencryptedFragment), ref encrypted, ref encoded, 5);

            // Get the message id, fragment id, and byte length for the encrypted message fragment
            ushort messageid = BitConverter.ToUInt16(new byte[] { encoded[0], encoded[1] }, 0);
            ushort fragmentid = BitConverter.ToUInt16(new byte[] { encoded[2], encoded[3] }, 0);
            ushort length = BitConverter.ToUInt16(new byte[] { encoded[4], encoded[5] }, 0);

            // Boundary check on the length
            if (length > this.maximumByteLength) throw new System.ArgumentOutOfRangeException("base32encodedAESencryptedFragment", "The length encoded in the parameter exceeds the maximum byte length allowed by the class.");

            // Pare the message back to the correct length
            byte[] cryptbytes = new byte[length];
            if (encrypted.Length == length) cryptbytes = encrypted;
            else Array.Copy(encrypted, 0, cryptbytes, 0, length);

            // The final message will have less than the maximum characters
            ushort fragmentLength = (ushort)(fragmentid + 1);
            ushort finalid = 0;
            if (cryptbytes.Length < this.maximumByteLength) finalid = fragmentid;

            // Has this message been set?
            bool set = false;

            // If we have seen this message identifier already, add the fragment
            foreach (TextMessage message in this)
            {
                if (message.MessageId == messageid)
                {
                    message.FinalFragmentId = finalid;
                    message.Length = fragmentLength;
                    message.Fragments[fragmentid] = cryptbytes;
                    set = true;
                    break;
                }
            }

            // If we have not seen this message identifier already, add it to the list
            if (!set)
            {
                TextMessage m = new TextMessage(messageid, fragmentLength);
                m.FinalFragmentId = finalid;
                m.Fragments[fragmentid] = cryptbytes;
                this.Add(m);
            }

            return;
        }
    }
}