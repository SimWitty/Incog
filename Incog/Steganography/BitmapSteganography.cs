// <copyright file="BitmapSteganography.cs" company="SimWitty (http://www.simwitty.org)">
//     Copyright © 2013 and distributed under the BSD license.
// </copyright>

namespace Incog.Steganography
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Text;
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
        /// <returns>Returns the message in Unicode characters.</returns>
        public static string SteganographyRead(string path, int startingPosition)
        {
            // Read the value in as bytes and convert to Unicode string
            byte[] messageBytes = SteganographyReadBytes(path, startingPosition);
            string finaltext = new string(Encoding.Unicode.GetChars(messageBytes));

            // Find the stop character: 0xFFFF or 65,535
            int endof = finaltext.IndexOf(Convert.ToChar(ushort.MaxValue));

            // Return the string from the start to the stop character
            return finaltext.Substring(0, endof).TrimStart();
        }

        /// <summary>
        /// Read a message using the Least Significant Bit Steganographic method.
        /// </summary>
        /// <param name="path">The bitmap file to write to.</param>
        /// <param name="startingPosition">The starting byte address.</param>
        /// <returns>Returns a byte array containing the message.</returns>
        public static byte[] SteganographyReadBytes(string path, int startingPosition)
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

            // Open the binary image
            BinaryReader image = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));
            long imageLength = image.BaseStream.Length;
            long maxMessageLength = imageLength - startingPosition;
            BitArray messageBits = new BitArray(Convert.ToInt32(maxMessageLength));

            // Preflight check - does the file have enough bytes?
            if (imageLength < startingPosition)
            {
                image.Close();
                throw new System.ArgumentException("The image file is too small. Cannot continue.");
            }

            // Jump to the starting position 
            image.BaseStream.Position = startingPosition;

            // Loop thru the image bytes and extract the least significant bits
            for (long l = startingPosition; l < imageLength; l++)
            {
                // Get the byte and the least significant bit
                byte currentByte = image.ReadByte();

                // Only process bytes with addresses in the integer range
                if (l < int.MaxValue)
                {
                    int position = Convert.ToInt32(l - startingPosition);
                    bool stegoBit = BinaryTools.LeastSignificantBit(currentByte);
                    messageBits.Set(position, stegoBit);
                }
                else
                {
                    break;
                }
            }

            image.Close();

            // Revert the dates created, modified, and accessed
            file = new FileInfo(path);
            file.CreationTime = created;
            file.LastWriteTime = modified;
            file.LastAccessTime = accessed;

            // Convert the bits to bytes, and the bytes to unicode characters
            byte[] messageBytes = new byte[maxMessageLength];
            messageBits.CopyTo(messageBytes, 0);
            return messageBytes;
        }

        /// <summary>
        /// Write a message using the Least Significant Bit Steganographic method.
        /// </summary>
        /// <param name="path">The bitmap file to write to.</param>
        /// <param name="startingPosition">The starting byte address.</param>
        /// <param name="message">The message represented in a byte array.</param>
        public static void SteganographyWrite(string path, int startingPosition, byte[] message)
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

            // Prepare the message
            BitArray messageBits = new BitArray(message);
            BinaryReader image = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None));
            long imageLength = image.BaseStream.Length;
            long maxMessageLength = imageLength - startingPosition;
            byte[] buffer = new byte[imageLength];

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

            // The beginning of the new file is the same as the source file
            for (long l = 0; l < startingPosition; l++)
            {
                buffer[l] = image.ReadByte();
            }

            // Populate the rest of the buffer
            for (long l = startingPosition; l < imageLength; l++)
            {
                // Get the byte and the least significant bit
                byte currentByte = image.ReadByte();
                bool imageBit = BinaryTools.LeastSignificantBit(currentByte);
                bool stegoBit = imageBit;

                // Only process bytes with addresses in the integer range
                if (l < int.MaxValue)
                {
                    int position = Convert.ToInt32(l - startingPosition);
                    if (position < messageBits.Length)
                    {
                        stegoBit = messageBits.Get(position);
                    }
                }

                // If the imageBit is the same as the stegoBit
                // That's because either a) we are outside the range, or 
                // b) we are inside the range but the value is already set.

                // If the imageBit and stegoBit do not match, update the least significant bit
                if (imageBit != stegoBit)
                {
                    currentByte = BinaryTools.SetLeastSignificantBit(currentByte, stegoBit);
                }

                buffer[l] = currentByte;
            }

            image.Close();

            // Re-open the image and update it
            BinaryWriter newImage = new BinaryWriter(new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write));
            newImage.Write(buffer);
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
        public static void SteganographyWrite(string path, int startingPosition, string message)
        {
            // Append the stop character: 0xFFFF or 65,535
            message += Convert.ToChar(ushort.MaxValue);
            
            // Write the value as Unicode bytes
            SteganographyWrite(path, startingPosition, Encoding.Unicode.GetBytes(message));
        }
    }
}