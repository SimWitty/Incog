﻿// <copyright file="Nouns.cs" company="SimWitty (http://www.simwitty.org)">
//     Copyright © 2013 and distributed under the BSD license.
// </copyright>

namespace Incog.PowerShell
{
    using System;

    /// <summary>
    /// Defines the common noun names that can be used to name Incog cmdlets.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Scope = "module", Justification = "Reviewed. The element names are self-explanatory.")]
    internal static class Nouns
    {
        // Channel commands -- Covert Channels and Communications
        public const string IncogLookup = "IncogLookup"; // DnsDemo
        public const string IncogNamedPipe = "IncogNamedPipe"; // NamedPipeDemo
        public const string IncogPing = "IncogPing"; // PingDemo
        public const string IncogWebServer = "IncogWebServer";
        public const string Netcat = "Netcat";

        // Media commands -- Steganography and Files
        public const string IncogFileSystem = "IncogFileSystem"; // AlternateDataStreamDemo
        public const string IncogAudio = "IncogAudio"; // WaveFileSteganographyDemo
        public const string IncogImage = "IncogImage"; // BitmapSteganographyDemo
        public const string IncogMutex = "IncogMutex"; // MemoryMappedFileDemo
        public const string IncogWebPage = "IncogWebPage";
        
        // Miscellaneous
        public const string ShannonEntropy = "ShannonEntropy"; // EntropyCalculatorDemo, EntropySpoofDemo
        public const string Shellcode = "Shellcode";

        // Diagnostics
        public const string CryptRandom = "CryptRandom";
        public const string FileInfo = "FileInfo";
    }
}