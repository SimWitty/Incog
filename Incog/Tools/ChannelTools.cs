// <copyright file="ChannelTools.cs" company="SimWitty (http://www.simwitty.org)">
//     Copyright © 2013 and distributed under the BSD license.
// </copyright>

namespace Incog.Tools
{
    using System;
    using System.Text;

    /// <summary>
    /// A collection of tools for covert channels.
    /// </summary>
    public static class ChannelTools
    {
        /// <summary>
        /// The selected communications mode { Alice (transmit), Bob (receive), Quit (exit the program), Unspecified }
        /// </summary>
        public enum CommunicationMode
        {
            /// <summary>
            /// The Alice mode is for writing or transmitting messages (A -> B).
            /// </summary>
            Alice,

            /// <summary>
            /// The Bob mode is for reading or receiving messages (A -> B).
            /// </summary>
            Bob,

            /// <summary>
            /// Quit indicates that the user has selected to terminate the program.
            /// </summary>
            Quit,

            /// <summary>
            /// The user has not specified the mode.
            /// </summary>
            Unspecified
        }

        /// <summary>
        /// The selected mathematical set to use for distributing the covert channel within the normal communication channel.
        /// </summary>
        public enum MathematicalSet
        {
            /// <summary>
            /// The set of linear incrementing numbers.
            /// </summary>
            Linear,

            /// <summary>
            /// The set of non-repeating random numbers.
            /// </summary>
            Random,

            /// <summary>
            /// The set of prime numbers, calculated using the Sieve of Eratosthenes.
            /// </summary>
            PrimeNumbers,

            /// <summary>
            /// The user has not specified the set.
            /// </summary>
            Unspecified
        }

        /// <summary>
        /// Encode a string for inclusion in a covert channel by adding a leading and trailing control characters.
        /// </summary>
        /// <param name="message">The message string that will be sent.</param>
        /// <returns>The message string with control characters.</returns>
        public static byte[] EncodeString(string message)
        {
            char c = Convert.ToChar(ushort.MaxValue);
            string s = c + message.Trim() + c;
            return Encoding.Unicode.GetBytes(s);
        }

        /// <summary>
        /// Decode a string from a covert channel by removing the leading and trailing control characters.
        /// </summary>
        /// <param name="message">The message string that will was sent.</param>
        /// <returns>The message string without control characters.</returns>
        public static string DecodeString(byte[] message)
        {
            char c = Convert.ToChar(ushort.MaxValue);
            string s = new string(Encoding.Unicode.GetChars(message));
            int start = s.IndexOf(c) + 1;
            int length = s.IndexOf(c, start) - start;

            if ((start == 0) || (length == -1))
            {
                return string.Empty;
            }
            else
            {
                return s.Substring(start, length);
            }
        }

        /// <summary>
        /// Get a mathematical set of linear values starting a minimum value.
        /// For example, if the minimum is 5 and the length is 5, the resulting set would be: { 5, 6, 7, 8, 9 }
        /// </summary>
        /// <param name="minimum">The first number in the set.</param>
        /// <param name="length">The total array length of the resulting set.</param>
        /// <returns>Returns a set of position values.</returns>
        public static uint[] MathSetLinear(uint minimum, uint length)
        {
            uint[] mathset = new uint[length];

            for (uint u = 0; u < mathset.Length; u++)
            {
                mathset[u] = minimum + u;
            }

            return mathset;
        }

        /// <summary>
        /// Get a mathematical set of non-repeating random values between a minimum and maximum.
        /// For example, if the minimum is 1, the maximum is 10, and the length is 5, the resulting set would be: { 1, 5, 9, 4, 2 }
        /// </summary>
        /// <param name="minimum">The minimum value in the set. No values returned will be lower than this value.</param>
        /// <param name="maximum">The maximum value in the set. No values returned will be higher than this value.</param>
        /// <param name="length">The total array length of the resulting set.</param>
        /// <returns>Returns a set of position values.</returns>
        public static uint[] MathSetRandom(uint minimum, uint maximum, uint length)
        {
            //// Define and zero out the math set

            uint[] mathset = new uint[length];
            for (uint u = 0; u < mathset.Length; u++)
            {
                mathset[u] = uint.MinValue;
            }

            // Ensure the minimum and maximum values do not exceed int 
            int minValue = SafeConvertToInt(minimum);
            int maxValue = SafeConvertToInt(maximum);

            //// Randomly fill the math set without repeating values

            int seed = maxValue - minValue;
            Random rand = new Random(seed);
            uint value = uint.MinValue;

            for (int i = 0; i < mathset.Length; i++)
            {
                while (Array.IndexOf(mathset, value) != -1)
                {
                    value = (uint)rand.Next(minValue, maxValue);
                }

                mathset[i] = value;
            }

            return mathset;
        }

        /// <summary>
        /// Get a mathematical set of prime numbers using the Sieve of Eratosthenes algorithm.
        /// </summary>
        /// <param name="minimum">The minimum value in the set. No values returned will be lower than this value.</param>
        /// <param name="maximum">The maximum value in the set. No values returned will be higher than this value.</param>
        /// <param name="length">The total array length of the resulting set.</param>
        /// <returns>Returns a set of position values.</returns>
        public static uint[] MathSetSieveOfEratosthenes(uint minimum, uint maximum, uint length)
        {
            //// Define and zero out the math set

            uint[] mathset = new uint[length];
            for (uint u = 0; u < mathset.Length; u++)
            {
                mathset[u] = uint.MinValue;
            }

            //// Execute the Sieve of Eratosthenes to find prime factor numbers

            double biggestSquareRoot = Math.Sqrt(maximum);
            bool[] eliminated = new bool[maximum];
            int index = 0;

            for (uint i = 3; i < maximum; i += 2)
            {
                if (!eliminated[i])
                {
                    if (i < biggestSquareRoot)
                    {
                        for (uint j = i * i; j < maximum; j += 2 * i)
                            eliminated[j] = true;
                    }

                    if (i >= minimum && i <= maximum)
                    {
                        mathset[index] = i;
                        index++;
                    }

                    if (index >= mathset.Length) break;
                }
            }

            if (mathset[mathset.Length - 1] == uint.MinValue)
            {
                string error = string.Format("The math set could not be completed. There are insufficent prime numbers between {0} and {1} to fill {2} positions.", minimum.ToString(), maximum.ToString(), length.ToString());
                throw new ApplicationException(error);
            }

            return mathset;
        }

        /// <summary>
        /// Convert a unsigned integer to an integer without an System.OverflowException.
        /// If the unsigned integer is greater than integer's maximum value, then the resulting integer is maximum value.
        /// </summary>
        /// <param name="value">The unsigned integer to convert.</param>
        /// <returns>The resulting integer after conversion.</returns>
        private static int SafeConvertToInt(uint value)
        {
            if (value > int.MaxValue) return int.MaxValue;
            else return Convert.ToInt32(value);
        }
    }
}