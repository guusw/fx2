// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System;
using System.Collections.Generic;
using System.Linq;
using FX2.Game.Beatmap.Effects;

namespace FX2.Game.Beatmap
{
    class BeatmapKshConverter
    {
        private static readonly short[] GateRateParameterMap = {4, 8, 16, 32, 12, 24};
        private static readonly short[] RetriggerRateParameterMap = {8, 16, 32, 12, 24};

        private static List<KshEffectFactory> EffectSettingsFactories = new List<KshEffectFactory>
        {
            new BiQuadFilterSettings.FromKsh(),
            new BitCrusherSettings.FromKsh(),
            new EchoSettings.FromKsh(),
            new FlangerSettings.FromKsh(),
            new PhaserSettings.FromKsh(),
            new RetriggerSettings.FromKsh(),
            new SideChainSettings.FromKsh(),
            new TapeStopSettings.FromKsh(),
            new WobbleSettings.FromKsh(),
        };

        private Beatmap beatmap;
        private BeatmapKsh sourceBeatmap;

        private TimingPoint currentTimingPoint;

        private TimeDivision currentPosition;

        /// <summary>
        /// Timing point that is pending to be added in the next measure
        /// </summary>
        private TimingPoint pendingTimingPoint;

        private Measure currentMeasure;
        private BeatmapKsh.Measure sourceMeasure;

        /// <summary>
        /// Absolute measure index
        /// </summary>
        private int measureIndex;

        /// <summary>
        /// Measure index, relative to the current timing point
        /// </summary>
        private int relativeMeasureIndex;

        /// <summary>
        /// Tick index, relative to the current measure
        /// </summary>
        private int tickIndex;

        /// <summary>
        /// Temporary state for generating hold buttons
        /// </summary>
        private TempButtonState[] buttonStates = new TempButtonState[6];

        /// <summary>
        /// Temporary state for generating laser parts
        /// </summary>
        private TempLaserState[] laserStates = new TempLaserState[2];

        /// <summary>
        /// The extended laser state
        /// </summary>
        private bool[] laserExtended = new bool[2];

        /// <summary>
        /// The effect types currently assigned to fx buttons through the use of tick options
        /// </summary>
        private EffectType[] currentButtonEffectTypes = new EffectType[2];

        /// <summary>
        /// Effect parameters, 2 per button. assigned using tick options
        /// </summary>
        private short[][] currentButtonEffectParameters = {new short[2], new short[2]};

        private Dictionary<ControlPointType, ObjectReference> lastControlPoints =
            new Dictionary<ControlPointType, ObjectReference>();

        private bool rollLocked = false;
        private float rollIntensity = 1.0f;

        private static void ParseBeatOption(string option, TimingPoint timingPoint)
        {
            string[] split = option.Split('/');
            if(split.Length != 2) throw new BeatmapParserException("Invalid beat option");

            timingPoint.Numerator = int.Parse(split[0]);
            timingPoint.Denominator = int.Parse(split[1]);
        }

        public BeatmapKshConverter(Beatmap beatmap, BeatmapKsh sourceBeatmap)
        {
            this.beatmap = beatmap;
            this.sourceBeatmap = sourceBeatmap;
        }

        public void Process()
        {
            beatmap.Metadata = new BeatmapMetadata();

            // Process metadata
            foreach(var option in sourceBeatmap.Options)
            {
                if(option.Key == "artist")
                    beatmap.Metadata.Artist = option.Value;
                else if(option.Key == "title")
                    beatmap.Metadata.Title = option.Value;
                else if(option.Key == "effect")
                    beatmap.Metadata.Creator = option.Value;
                else if(option.Key == "illustrator")
                    beatmap.Metadata.Illustrator = option.Value;
                else if(option.Key == "jacket")
                    beatmap.Metadata.JacketPath = option.Value;
                else if(option.Key == "m")
                {
                    if(option.Value.Length == 0)
                        throw new BeatmapParserException("Invalid song path in beatmap options detected");

                    string[] paths = option.Value.Split(';');
                    if(paths.Length == 0)
                        beatmap.Metadata.AudioPath = option.Value;
                    else
                    {
                        beatmap.Metadata.AudioPath = paths[0];
                        if(paths.Length > 1)
                            beatmap.Metadata.EffectedAudioPath = paths[1];
                    }
                }
                else if(option.Key == "po")
                    beatmap.Metadata.PreviewOffset = double.Parse(option.Value) / 1000.0;
                else if(option.Key == "plength")
                    beatmap.Metadata.PreviewDuration = double.Parse(option.Value) / 1000.0;
            }

            var firstTimingPoint = new TimingPoint();
            beatmap.TimingPoints.Add(firstTimingPoint);
            firstTimingPoint.Beatmap = beatmap;
            currentTimingPoint = beatmap.TimingPoints[0];

            // Must contain BPM and offset at least
            if(!sourceBeatmap.Options.ContainsKey("t") || !sourceBeatmap.Options.ContainsKey("o"))
                throw new BeatmapParserException("Map does not contain valid timing information");

            // Setup the initial timing point
            double startingBPM = 0.0;
            if(double.TryParse(sourceBeatmap.Options["t"], out startingBPM))
                currentTimingPoint.BPM = startingBPM;

            currentTimingPoint.Offset = double.Parse(sourceBeatmap.Options["o"]) / 1000.0;
            if(sourceBeatmap.Options.ContainsKey("beat"))
            {
                ParseBeatOption(sourceBeatmap.Options["beat"], currentTimingPoint);
            }
            else
            {
                currentTimingPoint.Numerator = 4;
                currentTimingPoint.Denominator = 4;
            }

            // Process custom effect types
            ProcessEffectDefinitions();

            foreach(var sourceMeasure in sourceBeatmap.Measures)
            {
                this.sourceMeasure = sourceMeasure;

                // Process pending timing point
                if(pendingTimingPoint != null)
                {
                    // TODO: Add test for this
                    beatmap.TimingPoints.Add(pendingTimingPoint);
                    pendingTimingPoint.Beatmap = beatmap;
                    currentTimingPoint = pendingTimingPoint;
                    pendingTimingPoint = null;
                }

                // Process ticks
                currentMeasure = currentTimingPoint.AddMeasure();
                foreach(var tick in sourceMeasure.Ticks)
                {
                    currentPosition = new TimeDivision(tickIndex, sourceMeasure.Ticks.Length);

                    if(tick.Options != null)
                        ProcessTickOptions(tick);

                    // Process button states
                    for(int i = 0; i < 6; i++)
                    {
                        if(tick.Button[i] == '0')
                        {
                            // Terminate current button
                            if(buttonStates[i] != null)
                            {
                                EndButtonState(i);
                            }
                        }
                        else if(buttonStates[i] == null)
                        {
                            BeginButtonState(i, tick.Button[i]);
                        }
                        else
                        {
                            var state = buttonStates[i];

                            // For buttons not using the 1/32 grid
                            if(!state.Snap32)
                            {
                                EndButtonState(i);

                                // Create new button state
                                BeginButtonState(i, tick.Button[i]);
                            }
                            else
                            {
                                // Sort hold state
                                state.NumTicks++;
                            }
                        }
                    }

                    // Process laser states
                    for(int i = 0; i < 2; i++)
                    {
                        if(tick.Lasers[i] == BeatmapKsh.Tick.LaserNone)
                        {
                            // End laser
                            laserStates[i] = null;
                            laserExtended[i] = false; // Reset extended range
                        }
                        else if(tick.Lasers[i] == BeatmapKsh.Tick.LaserLerp)
                        {
                            laserStates[i].NumTicks++;
                        }
                        else
                        {
                            UpdateLaserState(i, tick);
                        }
                    }

                    tickIndex++;
                }

                tickIndex = 0;
                measureIndex++;
                relativeMeasureIndex++;
            }
        }

        private void ProcessEffectDefinitions()
        {
            foreach(var def in sourceBeatmap.EffectDefinitions)
            {
                var factory = EffectSettingsFactories.First(
                        effectFactory => effectFactory.SupportedEffectTypes.Contains(def.BaseType));
                beatmap.RegisterEffect(def.Type, factory.GenerateEffectSettings(def));
            }
        }

        /// <summary>
        /// Friendly position string
        /// </summary>
        private string GetPositionString()
        {
            return $"m{measureIndex}-{tickIndex}/{sourceMeasure.Ticks.Length}";
        }

        private void UpdateLaserState(int index, BeatmapKsh.Tick tick)
        {
            Laser laser;
            TempLaserState state;
            if(laserStates[index] == null)
            {
                state = laserStates[index] = new TempLaserState();

                // Create laser root
                var root = new LaserRoot();
                root.IsExtended = laserExtended[index];
                root.Index = index;
                laser = root;

                state.Root = root.Root = new ObjectReference(root, currentMeasure);
                state.LastObject = state.Root;
            }
            else
            {
                state = laserStates[index];

                // Create laser point
                laser = new Laser();
                laser.Previous = state.LastObject;
                laser.Root = state.Root;

                var myRef = new ObjectReference(laser, currentMeasure);

                // Calculate distance to last
                var lastObject = (state.LastObject.Object as Laser);

                lastObject.Next = myRef;
                state.LastObject = myRef;
            }

            laser.Position = currentPosition;
            laser.HorizontalPosition = tick.LaserAsFloat(index);
            if(laser.IsExtended)
            {
                laser.HorizontalPosition *= 2.0f;
                laser.HorizontalPosition -= 0.5f;
            }

            // Decide to create slam instead?
            var previousLaser = laser.Previous?.Object as Laser;
            bool createInstantSegment = false;
            if(previousLaser != null)
            {
                double laserSlamThreshold = currentTimingPoint.BeatDuration / 7.0;
                double duration = previousLaser.Next.AbsolutePosition - laser.Previous.AbsolutePosition;
                createInstantSegment = duration <= laserSlamThreshold &&
                                       previousLaser.HorizontalPosition != laser.HorizontalPosition;
            }
            if(createInstantSegment)
            {
                laser.Previous.Measure.Objects.Add(laser);
                laser.Position = laser.Previous.Object.Position;
            }
            else
            {
                // Create normal control point
                currentMeasure.Objects.Add(laser);
            }

            // Reset tick and last measure on laser state
            state.NumTicks = 0;
            state.LastSourceMeasure = sourceMeasure;
        }

        private void BeginButtonState(int index, byte buttonCharacter)
        {
            var state = buttonStates[index] = new TempButtonState();
            state.StartTime = new TimeDivisionReference
            {
                Measure = currentMeasure,
                Position = currentPosition
            };

            // TODO: Connect hold objects

            if(index < 4)
            {
                // Normal '1' notes are always individual
                state.Snap32 = buttonCharacter != '1';
            }
            else
            {
                // Hold are always on a high enough snap to make sure they are seperate when needed
                state.Snap32 = true;

                // Remap to 0/1
                index -= 4;

                // Set effect
                var c = buttonCharacter;
                if(c == 'B')
                {
                    state.EffectType = EffectType.BitCrusher;
                    state.EffectParameter0 = currentButtonEffectParameters[index][0];
                }
                else if(c >= 'G' && c <= 'L') // Gate 4/8/16/32/12/24
                {
                    state.EffectType = EffectType.Gate;
                    state.EffectParameter0 = GateRateParameterMap[c - 'G'];
                }
                else if(c >= 'S' && c <= 'W') // Retrigger 8/16/32/12/24
                {
                    state.EffectType = EffectType.Retrigger;
                    state.EffectParameter0 = RetriggerRateParameterMap[c - 'S'];
                }
                else if(c == 'Q')
                {
                    state.EffectType = EffectType.Phaser;
                }
                else if(c == 'F')
                {
                    state.EffectType = EffectType.Flanger;
                }
                else if(c == 'X')
                {
                    state.EffectType = EffectType.Wobble;
                    state.EffectParameter0 = 12;
                }
                else if(c == 'D')
                {
                    state.EffectType = EffectType.SideChain;
                }
                else if(c == 'A')
                {
                    state.EffectType = EffectType.TapeStop;
                    state.EffectParameter0 = currentButtonEffectParameters[index][0];
                }
                else
                {
                    state.EffectType = currentButtonEffectTypes[index];
                    state.EffectParameter0 = currentButtonEffectParameters[index][0];
                    state.EffectParameter1 = currentButtonEffectParameters[index][1];
                }
            }
        }

        private void EndButtonState(int index)
        {
            var state = buttonStates[index];
            var targetMeasure = state.StartTime.Measure;
            if(state.IsHoldState)
            {
                Hold start = new Hold();
                start.Position = state.StartTime.Position;
                start.Index = (byte)index;

                // Copy effect settings
                start.EffectType = state.EffectType;
                start.EffectParameter0 = state.EffectParameter0;
                start.EffectParameter1 = state.EffectParameter1;

                // Add and resort
                targetMeasure.Objects.Add(start);
                targetMeasure.Sort();

                HoldEnd end = new HoldEnd();
                end.Position = currentPosition;
                end.StartingPoint = new ObjectReference(start, currentMeasure);
                start.EndingPoint = new ObjectReference(end, currentMeasure);
                currentMeasure.Objects.Add(end);
            }
            else
            {
                Button button = new Button();
                button.Position = state.StartTime.Position;
                button.Index = (byte)index;
                targetMeasure.Objects.Add(button);
            }

            buttonStates[index] = null;
        }

        private void AddNewTimingPoint(TimingPoint newTimingPoint)
        {
            // Propagate BPM change to pending timing point
            if(pendingTimingPoint != null)
                pendingTimingPoint.BPM = newTimingPoint.BPM;

            double inMeasureOffset = 0.0;
            if(tickIndex == 0)
            {
                // Replace current measure
                currentMeasure.TimingPoint.RemoveMeasure(currentMeasure);
                currentMeasure = newTimingPoint.AddMeasure();
                relativeMeasureIndex--;
            }
            else
            {
                // Handle timings where a new BPM is chosen in the middle of a measure
                inMeasureOffset = (double)tickIndex / sourceMeasure.Ticks.Length;
                newTimingPoint.FirstMeasureOffsetPercentage = inMeasureOffset;

                // Create a new measure
                currentMeasure = newTimingPoint.AddMeasure();
            }

            // Calculate offset for new timing point
            var originalEnding = currentTimingPoint.GetMeasureOffset(relativeMeasureIndex + 1) -
                                 currentTimingPoint.MeasureDuration * inMeasureOffset;

            newTimingPoint.Offset = originalEnding;

            // Add new timing point to map
            beatmap.TimingPoints.Add(newTimingPoint);
            newTimingPoint.Beatmap = beatmap;
            currentTimingPoint = newTimingPoint;

            relativeMeasureIndex = 0;
        }

        private void SetEffectTypeAndParameters(ObjectFilter objectFilter, string options)
        {
            string[] values = options.Split(';');
            if(values.Length == 1)
                SetEffectType(objectFilter, options);
            else
            {
                // Filter out parameters
                SetEffectType(objectFilter, values[0]);
                short parameter = short.Parse(values[1]);
                if(objectFilter == ObjectFilter.Fx0)
                    currentButtonEffectParameters[0][0] = parameter;
                else
                    currentButtonEffectParameters[1][0] = parameter;
            }
        }

        private void SetEffectType(ObjectFilter objectFilter, string options)
        {
            EffectType effectType = sourceBeatmap.ParseEffectType(options);
            if((objectFilter & ObjectFilter.LaserMask) != 0)
            {
                // Only use events for laser effect type
                LaserEffectTypeEvent evt = new LaserEffectTypeEvent
                {
                    EffectType = effectType,
                    Position = currentPosition
                };

                currentMeasure.Objects.Add(evt);
            }
            else
            {
                // Store the current effect being used for this type so it can be embeded into the hold object
                int button = objectFilter == ObjectFilter.Fx0 ? 0 : 1;
                currentButtonEffectTypes[button] = effectType;
            }
        }

        private void ProcessTickOptions(BeatmapKsh.Tick tick)
        {
            TimingPoint newTimingPoint = null;

            // Process tick options
            foreach(var option in tick.Options)
            {
                var key = option.Key;
                if(key == "beat")
                {
                    if(tickIndex == 0)
                    {
                        // Special case where the first tick has a beat option
                        if(measureIndex == 0)
                            ParseBeatOption(option.Value, currentTimingPoint);
                        else
                        {
                            if(newTimingPoint == null)
                            {
                                newTimingPoint = new TimingPoint();
                                newTimingPoint.BPM = currentTimingPoint.BPM;
                            }
                            ParseBeatOption(option.Value, newTimingPoint);
                        }
                    }
                    else
                    {
                        if(pendingTimingPoint == null)
                        {
                            pendingTimingPoint = new TimingPoint();
                        }
                        pendingTimingPoint.BPM = currentTimingPoint.BPM;
                        ParseBeatOption(option.Value, pendingTimingPoint);
                    }
                }
                else if(key == "t")
                {
                    if(tickIndex == 0)
                    {
                        // Special case where the first tick has a timing option
                        if(measureIndex == 0)
                        {
                            currentTimingPoint.BPM = double.Parse(option.Value);
                            continue;
                        }
                    }

                    if(newTimingPoint == null)
                    {
                        newTimingPoint = new TimingPoint();
                        newTimingPoint.Numerator = currentTimingPoint.Numerator;
                        newTimingPoint.Denominator = currentTimingPoint.Denominator;
                    }
                    newTimingPoint.BPM = double.Parse(option.Value);
                }
                else if(key == "laserrange_l")
                {
                    laserExtended[0] = true;
                }
                else if(key == "laserrange_r")
                {
                    laserExtended[1] = true;
                }
                else if(key == "fx-l")
                {
                    SetEffectTypeAndParameters(ObjectFilter.Fx0, option.Value);
                }
                else if(key == "fx-r")
                {
                    SetEffectTypeAndParameters(ObjectFilter.Fx1, option.Value);
                }
                else if(key == "fx-l_param1")
                {
                    currentButtonEffectParameters[0][0] = short.Parse(option.Value);
                }
                else if(key == "fx-r_param1")
                {
                    currentButtonEffectParameters[1][0] = short.Parse(option.Value);
                }
                else if(key == "filtertype")
                {
                    SetEffectType(ObjectFilter.Laser0 | ObjectFilter.Laser1, option.Value);
                }
                else if(key == "pfiltergain")
                {
                    AddControlPoint(float.Parse(option.Value) / 100.0f, ControlPointType.FilterMix, false);
                }
                else if(key == "chokkakuvol")
                {
                    AddControlPoint(float.Parse(option.Value) / 100.0f, ControlPointType.SampleMix, false);
                }
                else if(key == "zoom_bottom")
                {
                    AddControlPoint(float.Parse(option.Value) / 100.0f, ControlPointType.ZoomBottom, true);
                }
                else if(key == "zoom_top")
                {
                    AddControlPoint(float.Parse(option.Value) / 100.0f, ControlPointType.ZoomTop, true);
                }
                else if(key == "tilt")
                {
                    bool lockRoll = false;
                    string value = option.Value;
                    if(value.StartsWith("keep_"))
                    {
                        lockRoll = true;
                        value = value.Substring(5);
                    }

                    if(rollLocked != lockRoll)
                    {
                        RollModifier mod = new RollModifier {Position = currentPosition, Lock = lockRoll};
                        currentMeasure.Objects.Add(mod);
                        rollLocked = lockRoll;
                    }

                    float intensity = 1.0f;
                    if(value == "zero")
                    {
                        intensity = 0.0f;
                    }
                    else if(value == "normal")
                    {
                        intensity = 1.0f;
                    }
                    else if(value == "bigger")
                    {
                        intensity = 2.0f;
                    }
                    else if(value == "biggest")
                    {
                        intensity = 3.0f;
                    }
                    else
                    {
                        throw new BeatmapParserException($"Unkown roll type {value}");
                    }

                    if(intensity != rollIntensity)
                    {
                        AddControlPoint(intensity, ControlPointType.RollIntensity, false);
                        rollIntensity = intensity;
                    }
                }
                else
                {
                    throw new BeatmapParserException($"Unknown option: {key}={option.Value} at {GetPositionString()}");
                }
            }

            if(newTimingPoint != null)
            {
                AddNewTimingPoint(newTimingPoint);
            }
        }

        private void AddControlPoint(float newValue, ControlPointType type, bool lerp)
        {
            ObjectReference last;
            lastControlPoints.TryGetValue(type, out last);

            ControlPoint current = new ControlPoint {Type = type, Value = newValue};
            current.Position = currentPosition;
            ObjectReference currentReference = new ObjectReference(current, currentMeasure);

            if(lerp && last != null)
            {
                ControlPoint lastControlPoint = (ControlPoint)last.Object;
                current.Previous = last;
                lastControlPoint.Next = currentReference;
            }

            currentMeasure.Objects.Add(current);
            lastControlPoints[type] = currentReference;
        }

        /// <summary>
        /// Temporary object to keep track if a button is a hold button
        /// </summary>
        private class TempButtonState
        {
            public TimeDivisionReference StartTime;
            public int NumTicks = 0;
            public EffectType EffectType = 0;
            public short EffectParameter0;
            public short EffectParameter1;

            /// <summary>
            /// Snap to 1/32th grid for buttons
            /// </summary>
            public bool Snap32 = false;

            public bool IsHoldState => NumTicks > 0 && Snap32;
        }

        public class TempLaserState
        {
            public ObjectReference Root;
            public ObjectReference LastObject;

            /// <summary>
            /// The last source measure
            /// </summary>
            public BeatmapKsh.Measure LastSourceMeasure;

            /// <summary>
            /// Number of ticks in source map since last control point
            /// </summary>
            public int NumTicks = 0;
        }
    }

    public partial class Beatmap
    {
        /// <summary>
        /// Tries to construct a beatmap from a KSH format beatmap
        /// </summary>
        /// <param name="beatmap">A valid KSH beatmap</param>
        public Beatmap(BeatmapKsh beatmap)
        {
            RegisterDefaultEffectSettings();
            BeatmapKshConverter parser = new BeatmapKshConverter(this, beatmap);
            parser.Process();
        }
    }
}