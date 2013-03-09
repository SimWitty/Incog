// <copyright file="BitmapSteganography.cs" company="SimWitty (http://www.simwitty.org)">
//     Copyright © 2013 and distributed under the BSD license.
// </copyright>

namespace Incog.Steganography
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Security;
    using System.Text;
    using Incog.Tools;
    using SimWitty.Library.Core.Encrypting;
    using SimWitty.Library.Core.Tools;

    /// <summary>
    /// Bitmap Steganography tools
    /// </summary>
    public static class BitmapSteganography
    {
        /// <summary>
        /// Read a message using the Least Significant Bit Steganographic method.
        /// </summary>
        /// <param name="path">The bitmap file to write to.</param>
        /// <param name="startingPosition">The starting byte address.</param>
        /// <param name="mathset">The set to use to determine where to update the least significant bits.</param>
        /// <param name="passphrase">The passphrase for encrypting and decrypting the message text.</param>
        /// <returns>Returns the message in Unicode characters.</returns>
        public static string SteganographyRead(string path, uint startingPosition, ChannelTools.MathematicalSet mathset, SecureString passphrase)
        {
            // Read the value in as bytes and convert to Unicode string
            byte[] cipherBytes = SteganographyReadBytes(path, startingPosition, mathset);
            
            // Decrypt the message
            Cryptkeeper crypt = new Cryptkeeper(passphrase);
            byte[] messageBytes = crypt.GetBytes(cipherBytes, Cryptkeeper.Action.Decrypt);
            string finaltext = new string(Encoding.Unicode.GetChars(messageBytes));

            // Find the stop character: 0xFFFF or 65,535
            int endof = finaltext.IndexOf(Convert.ToChar(ushort.MaxValue));

            // No final character? Return an empty string
            if (endof == -1) return string.Empty;

            // Return the string from the start to the stop character
            return finaltext.Substring(0, endof).TrimStart();
        }

        /// <summary>
        /// Read a message using the Least Significant Bit Steganographic method.
        /// </summary>
        /// <param name="path">The bitmap file to write to.</param>
        /// <param name="startingPosition">The starting byte address.</param>
        /// <param name="mathset">The set to use to determine where to update the least significant bits.</param>
        /// <returns>Returns a byte array containing the message.</returns>
        public static byte[] SteganographyReadBytes(string path, uint startingPosition, ChannelTools.MathematicalSet mathset)
        {
            // Preflight check - does the file exist?
            if (!File.Exists(path))
            {
                throw new System.ArgumentException("File not found.");
            }

            // Save the dates (we will revert later)
            FileInfo file = new FileInfo(path);
            DateTime created = file.CreationTime;
            DateTime modified = file.LastWriteTime;
            DateTime accessed = file.LastAccessTime;

            // Open the binary image and read the image into an array
            BinaryReader image = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None));
            byte[] imageBytes = image.ReadBytes((int)image.BaseStream.Length);
            long imageLength = imageBytes.Length;
            image.Close();

            // Prepare the message
            long maxMessageLength = imageLength - startingPosition;
            BitArray messageBits = new BitArray(Convert.ToInt32(maxMessageLength));

            // Prepare the set
            uint[] positions = new uint[messageBits.Length];
            uint maximum = (uint)Math.Min(uint.MaxValue, imageLength);

            // TODO: Determine a better way to estimate the length. Currently hard-coded for 4096 or approximately 2000 characters.
            uint length = 4096; // (uint)Math.Min(uint.MaxValue, messageBits.Length);

            switch (mathset)
            {
                case ChannelTools.MathematicalSet.Linear:
                    positions = ChannelTools.MathSetLinear(startingPosition, length);
                    break;
                case ChannelTools.MathematicalSet.PrimeNumbers:
                    positions = ChannelTools.MathSetSieveOfEratosthenes(startingPosition, maximum, length);
                    break;
                case ChannelTools.MathematicalSet.Random:
                    positions = ChannelTools.MathSetRandom(startingPosition, maximum, length);
                    break;
                default:
                    string error = "An unexpected mathematical set was passed into the SteganographyWrite function.";
                    throw new ApplicationException(error);
            }

            // Preflight check - does the file have enough bytes?
            if (imageLength < startingPosition)
            {
                image.Close();
                throw new System.ArgumentException("The image file is too small. Cannot continue.");
            }
            
            // Extract the message from the image byte array
            for (int i = 0; i < positions.Length; i++)
            {
                // Get the byte and the least significant bit
                byte currentByte = imageBytes[positions[i]];
                bool stegoBit = BinaryTools.LeastSignificantBit(currentByte);
                messageBits.Set(i, stegoBit);
            }

            // Revert the dates created, modified, and accessed
            file = new FileInfo(path);
            file.CreationTime = created;
            file.LastWriteTime = modified;
            file.LastAccessTime = accessed;

            // Convert the bits to bytes
            byte[] buffer = new byte[maxMessageLength];
            messageBits.CopyTo(buffer, 0);
            
            // Resize the array
            byte[] lengthBytes = new byte[4];
            Array.Copy(buffer, lengthBytes, 4);
            uint messageLength = BitConverter.ToUInt32(lengthBytes, 0);

            byte[] messageBytes = new byte[messageLength];
            Array.Copy(buffer, 4, messageBytes, 0, messageBytes.Length);

            return messageBytes;
        }

        /// <summary>
        /// Write a message using the Least Significant Bit Steganographic method.
        /// </summary>
        /// <param name="path">The bitmap file to write to.</param>
        /// <param name="startingPosition">The starting byte address.</param>
        /// <param name="message">The message represented in a byte array.</param>
        /// <param name="mathset">The set to use to determine where to update the least significant bits.</param>
        public static void SteganographyWrite(string path, uint startingPosition, byte[] message, ChannelTools.MathematicalSet mathset)
        {
            // Preflight check - does the file exist?
            if (!File.Exists(path))
            {
                throw new System.ArgumentException("File not found.");
            }

            // Save the dates (we will revert later)
            FileInfo file = new FileInfo(path);
            DateTime created = file.CreationTime;
            DateTime modified = file.LastWriteTime;
            DateTime accessed = file.LastAccessTime;

            // Pre-pend the message array length
            byte[] buffer = new byte[message.Length + 4];
            byte[] lengthBytes = BitConverter.GetBytes((uint)message.Length);
            Array.Copy(lengthBytes, buffer, 4);
            Array.Copy(message, 0, buffer, 4, message.Length);
            
            // Open the binary image and read the image into an array
            BinaryReader image = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None));
            byte[] imageBytes = image.ReadBytes((int)image.BaseStream.Length);
            long imageLength = imageBytes.Length;
            image.Close();

            // Prepare the message
            BitArray messageBits = new BitArray(buffer);
            long maxMessageLength = imageLength - startingPosition;

            // Prepare the set
            uint[] positions = new uint[messageBits.Length];
            uint maximum = (uint)Math.Min(uint.MaxValue, imageLength);
            uint length = (uint)Math.Min(uint.MaxValue, messageBits.Length);

            switch (mathset)
            {
                case ChannelTools.MathematicalSet.Linear:
                    positions = ChannelTools.MathSetLinear(startingPosition, length);
                    break;
                case ChannelTools.MathematicalSet.PrimeNumbers:
                    positions = ChannelTools.MathSetSieveOfEratosthenes(startingPosition, maximum, length);
                    break;
                case ChannelTools.MathematicalSet.Random:
                    positions = ChannelTools.MathSetRandom(startingPosition, maximum, length);
                    break;
                default:
                    string error = "An unexpected mathematical set was passed into the SteganographyWrite function.";
                    throw new ApplicationException(error);
            }
            
            // Preflight check - does the file have enough bytes?
            if (imageLength < startingPosition)
            {
                image.Close();
                throw new System.ArgumentException("The image file is too small. Cannot continue.");
            }

            // Preflight check - does the file have enough bytes?
            if (maxMessageLength < messageBits.Length)
            {
                image.Close();
                throw new System.ArgumentException("The image file is too small. Cannot continue.");
            }

            // Embed the message into the image byte array
            for (int i = 0; i < positions.Length; i++)
            {
                // Get the byte and the least significant bit
                byte currentByte = imageBytes[positions[i]];
                bool imageBit = BinaryTools.LeastSignificantBit(currentByte);
                bool stegoBit = messageBits.Get(i);
                
                // If the imageBit is the same as the stegoBit, that is because the value is already correct.
                // If the imageBit and stegoBit do not match, update the least significant bit
                if (imageBit != stegoBit)
                {
                    currentByte = BinaryTools.SetLeastSignificantBit(currentByte, stegoBit);
                    imageBytes[positions[i]] = currentByte;
                }
            }

            // Re-open the image and update it
            BinaryWriter newImage = new BinaryWriter(new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write));
            newImage.Write(imageBytes);
            newImage.Close();

            // Revert the dates created, modified, and accessed
            file = new FileInfo(path);
            file.CreationTime = created;
            file.LastWriteTime = modified;
            file.LastAccessTime = accessed;
        }
        
        /// <summary>
        /// Write a message using the Least Significant Bit Steganographic method.
        /// </summary>
        /// <param name="path">The bitmap file to write to.</param>
        /// <param name="startingPosition">The starting byte address.</param>
        /// <param name="message">The message in Unicode characters.</param>
        /// <param name="passphrase">The passphrase for encrypting and decrypting the message text.</param>
        /// <param name="mathset">The set to use to determine where to update the least significant bits.</param>
        public static void SteganographyWrite(string path, uint startingPosition, string message, ChannelTools.MathematicalSet mathset, SecureString passphrase)
        {
            // Append the stop character: 0xFFFF or 65,535
            message += Convert.ToChar(ushort.MaxValue);

            // Encrypt the message
            Cryptkeeper crypt = new Cryptkeeper(passphrase);
            byte[] cipherBytes = crypt.GetBytes(message, Cryptkeeper.Action.Encrypt);

            // Write the encoded and encrypted message
            SteganographyWrite(path, startingPosition, cipherBytes, mathset);
        }
    }
}