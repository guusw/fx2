// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using osu.Framework.Configuration;

namespace FX2.Game.Beatmap
{
    public class BeatmapParserException : Exception
    {
        public BeatmapParserException(string message) : base(message)
        {
        }

        public string Line;
        public int LineNumber;
        public int Column;
    }

    public class BeatmapKSH
    {
        public const string Separator = "--";

        public static readonly Dictionary<char, byte> LaserCharacters;

        public static readonly Dictionary<string, EffectType> EffectTypes = new Dictionary<string, EffectType>
        {
            ["fx;bitc"] = EffectType.BitCrusher,
            ["bitc"] = EffectType.BitCrusher,
            ["lpf1"] = EffectType.LowPassFilter,
            ["peak"] = EffectType.PeakingFilter,
            ["hpf1"] = EffectType.HighPassFilter,

            // New style effect types
            ["Retrigger"] = EffectType.Retrigger,
            ["Gate"] = EffectType.Gate,
            ["Flanger"] = EffectType.Flanger,
            ["Wobble"] = EffectType.Wobble,
            ["SideChain"] = EffectType.SideChain,
            ["Phaser"] = EffectType.Phaser,
            ["Echo"] = EffectType.Echo,
            ["BitCrusher"] = EffectType.BitCrusher,
            ["TapeStop"] = EffectType.TapeStop,
        };

        /// <summary>
        /// Global beatmap options and metadata
        /// </summary>
        public readonly Dictionary<string, string> Options = new Dictionary<string, string>();

        /// <summary>
        /// Custom effect types
        /// </summary>
        public readonly List<EffectDefinition> EffectDefinitions = new List<EffectDefinition>();

        /// <summary>
        /// All the measures in the beatmap
        /// </summary>
        public readonly List<Measure> Measures = new List<Measure>(2000);
        
        static BeatmapKSH()
        {
            LaserCharacters = new Dictionary<char, byte>();
            Action<char, char> AddRange = (char start, char end) =>
            {
                for(char c = start; c <= end; c++)
                {
                    LaserCharacters.Add(c, (byte)LaserCharacters.Count);
                }
            };
            AddRange('0', '9');
            AddRange('A', 'Z');
            AddRange('a', 'o');
        }
        
        public static EffectType ParseEffectType(string name)
        {
            return EffectTypes[name];
        }

        /// <summary>
        /// Tries to load a new beatmap from a KSH format beatmap file
        /// </summary>
        /// <exception cref="BeatmapParserException">Thrown when the parsing failed</exception>
        /// <param name="stream"></param>
        /// <param name="skipBody">if true, the beatmap body is not read, only the metadata and initial timing info</param>
        public BeatmapKSH(Stream stream, bool skipBody = false)
        {
            using(Parser parser = new Parser(stream, skipBody ? 128 : 65536))
            {
                ParserHeader(parser);

                if(skipBody)
                    return;

                ParseBody(parser);
            }
        }

        private void ParserHeader(Parser parser)
        {
            while(true)
            {
                if(!parser.Next())
                    parser.Throw("Unexpected end of file while parsing header");
                string line = parser.Line.Trim();
                if(line.Length == 0)
                    continue;

                if(line == Separator)
                    break; // End of options

                var split = line.Split(new[] {'='}, 2);
                if(split.Length != 2)
                    parser.Throw("Failed to find valid setting entry");
                if(split[0].Length == 0)
                    parser.Throw("Empty key not allowed");
                Options.Add(split[0], split[1]);
            }
        }

        private void ParseBody(Parser parser)
        {
            Measure measure = new Measure();
            Tick tick = new Tick();
            Position time = new Position();
            List<Tick> ticks = new List<Tick>(64);
            EffectType customEffectType = EffectType.UserDefined0;

            while(true)
            {
                if(!parser.Next())
                    return; // End of file

                string line = parser.Line;
                if(line.Length == 0)
                    continue; // Skip empty lines

                if(line == Separator) // End this block
                {
                    measure.Ticks = ticks.ToArray();
                    ticks.Clear();
                    Measures.Add(measure);
                    measure = new Measure();
                    time.Measure++;
                    time.Tick = 0;
                }
                else
                {
                    if(line[0] == '#') // Detect custom effect type
                    {
                        string[] split = line.Split(' ');

                        // Load custom effect type
                        if(split[0] == "#define_fx")
                        {
                            if(split.Length != 3) parser.Throw("Invalid custom effect definition");
                            EffectDefinition effectDefinition = new EffectDefinition();
                            effectDefinition.Name = split[1];
                            effectDefinition.Type = customEffectType;
                            effectDefinition.BaseType = EffectType.UserDefined0;

                            string[] options = split[2].Split(';');
                            foreach(var option in options)
                            {
                                string[] kvp = option.Split('=');
                                if(kvp.Length != 2)
                                    parser.Throw($"Invalid effect key/value pair \"{option}\"");

                                if(kvp[0] == "type")
                                    effectDefinition.BaseType = ParseEffectType(kvp[1]);

                                effectDefinition.Options.Add(kvp[0], kvp[1]);   
                            }

                            if(effectDefinition.BaseType == EffectType.UserDefined0)
                                parser.Throw($"Missing base effect type for custom effect definition \"{effectDefinition.Name}\"");

                            EffectDefinitions.Add(effectDefinition);

                            customEffectType = (EffectType)((int)customEffectType + 1);
                        }
                    }
                    else if(line.Contains("=")) // Detect options
                    {
                        string[] split = line.Split('=');
                        if(split.Length != 2) parser.Throw("Invalid options string", line.IndexOf("="));
                        if(tick.Options == null) tick.Options = new Dictionary<string, string>();
                        tick.Options[split[0]] = split[1];
                    }
                    else
                    {
                        string[] split = line.Split('|');
                        if(split.Length != 3) parser.Throw("Invalid tick format");
                        if(split[0].Length != 4) parser.Throw("Invalid button format");
                        if(split[1].Length != 2) parser.Throw("Invalid fx button format", 5);
                        tick.Button[0] = Convert.ToByte(split[0][0]);
                        tick.Button[1] = Convert.ToByte(split[0][1]);
                        tick.Button[2] = Convert.ToByte(split[0][2]);
                        tick.Button[3] = Convert.ToByte(split[0][3]);
                        tick.Button[4] = Convert.ToByte(split[1][0]);
                        tick.Button[5] = Convert.ToByte(split[1][1]);
                        if(split[2].Length < 2) parser.Throw("Invalid laser button format", 5);
                        tick.Lasers[0] = TranslateLaserCharacter(split[2][0]);
                        tick.Lasers[1] = TranslateLaserCharacter(split[2][1]);

                        // TODO: Laser roll information

                        ticks.Add(tick);
                        tick = new Tick();
                        time.Tick++;
                    }
                }
            }
        }
        
        private byte TranslateLaserCharacter(char source)
        {
            if(source == ':')
                return Tick.LaserLerp;
            else if(source == '-')
                return Tick.LaserNone;

            return LaserCharacters[source];
        }

        /// <summary>
        /// Custom effect definition inside of a ksh map
        /// </summary>
        public class EffectDefinition
        {
            /// <summary>
            /// The built-in type of the effect
            /// </summary>
            public EffectType Type;

            /// <summary>
            /// Base type for the effect
            /// </summary>
            public EffectType BaseType;

            /// <summary>
            /// Name of this custom effect
            /// </summary>
            public string Name;

            /// <summary>
            /// Options for this effect
            /// </summary>
            public Dictionary<string, string> Options = new Dictionary<string, string>();
        }

        /// <summary>
        /// A single measure (the parts separated by --- in a ksh map)
        /// </summary>
        public class Measure
        {
            /// <summary>
            /// The ticks inside this measure
            /// </summary>
            public Tick[] Ticks;
        }

        /// <summary>
        /// Single element inside of a <see cref="Measure"/>
        /// </summary>
        public class Tick
        {
            public static byte LaserRange => (byte)LaserCharacters.Count;
            public const byte LaserNone = 0xFF;
            public const byte LaserLerp = 0xFE;

            /// <summary>
            /// Options set at the start of this tick
            /// </summary>
            public Dictionary<string, string> Options;

            /// <summary>
            /// 0 1 2 3 FXL FXR Buttons (in this order)
            /// </summary>
            public byte[] Button = new byte[6];

            /// <summary>
            /// Laser control point characters
            /// </summary>
            public byte[] Lasers = {LaserNone, LaserNone};

            public float LaserAsFloat(int index)
            {
                return (float)Lasers[index] / (LaserRange - 1);
            }

            public override string ToString()
            {
                return
                    $"{(char)Button[0]}{(char)Button[1]}{(char)Button[2]}{(char)Button[3]}|{(char)Button[4]}{(char)Button[5]}|{(char)Lasers[0]}{(char)Lasers[1]}";
            }
        }

        public struct Position
        {
            /// <summary>
            /// Measure index, relative to the whole map
            /// </summary>
            public int Measure;

            /// <summary>
            /// Tick index inside the measure at <see cref="Measure"/>
            /// </summary>
            public int Tick;

            public override string ToString()
            {
                return $"Measure {Measure} - Tick {Tick}";
            }
        }

        public class Parser : IDisposable
        {
            public readonly StreamReader Reader;

            /// <summary>
            /// Current line number
            /// </summary>
            public int LineNumber { get; private set; } = 0;

            /// <summary>
            /// Last read line
            /// </summary>
            public string Line { get; private set; } = "";

            public Parser(Stream stream, int bufferSize)
            {
                Reader = new StreamReader(stream, Encoding.Default, true, bufferSize);
            }

            /// <summary>
            /// Reads the next line, output is stored in <see cref="Line"/>
            /// </summary>
            /// <returns>false if there are no more lines to read</returns>
            public bool Next()
            {
                string newLine = Reader.ReadLine();
                if(newLine == null)
                    return false;

                LineNumber++;
                Line = newLine;
                return true;
            }

            public void Dispose()
            {
                Reader.Dispose();
            }

            /// <summary>
            /// Throws a new parser error at the current line
            /// </summary>
            /// <param name="column"></param>
            public void Throw(string message, int column = 0)
            {
                throw new BeatmapParserException(message) {LineNumber = LineNumber, Column = column, Line = Line};
            }
        }
    }
}