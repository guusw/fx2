// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information
namespace FX2.Game.Beatmap
{
    /// <summary>
    /// The type of value to control for a control point
    /// </summary>
    public enum ControlPointType
    {
        ZoomBottom,
        ZoomTop,
        FilterMix,
        SampleMix,
        RollIntensity
    }

    /// <summary>
    /// Unique identifier for event groups which can be filtered when selecting the current active event in a beatmap
    /// </summary>
    public enum EventGroup
    {
        ZoomBottom,
        ZoomTop,
        FilterMix,
        SampleMix,
        RollIntensity,
        RollModifier,
        LaserEffectType,
    }

    /// <summary>
    /// Event object
    /// </summary>
    public abstract class Event : Object
    {
        public override string ToString()
        {
            return $"Event";
        }

        public abstract EventGroup EventGroup { get; }
    }
    
    /// <summary>
    /// Modifies track roll
    /// </summary>
    public class RollModifier : Event
    {
        /// <summary>
        /// Should the roll at this point be locked?
        /// </summary>
        public bool Lock;

        public override string ToString()
        {
            return $"Roll Modifier {nameof(Lock)}: {Lock}";
        }

        public override EventGroup EventGroup => EventGroup.RollModifier;
    }

    /// <summary>
    /// Event that changes an effect type on lasers
    /// </summary>
    public class LaserEffectTypeEvent : Event
    {
        /// <summary>
        /// The effect type to change to
        /// </summary>
        public EffectType EffectType;

        public override string ToString()
        {
            return $"Effect Type {nameof(EffectType)}: {EffectType}";
        }

        public override EventGroup EventGroup => EventGroup.LaserEffectType;
    }

    /// <summary>
    /// A float value that can animate(lerp) or hard-set a specific value
    /// </summary>
    public class ControlPoint : Event
    {
        /// <summary>
        /// The type of value to control
        /// </summary>
        public ControlPointType Type;

        /// <summary>
        /// The new value
        /// </summary>
        public float Value;

        /// <summary>
        /// Previous zoom event
        /// </summary>
        public ObjectReference Previous;

        /// <summary>
        /// Next zoom event to blend to
        /// </summary>
        public ObjectReference Next;

        public override string ToString()
        {
            return $"Control Point {nameof(Type)}: {Type}, {nameof(Value)}: {Value}";
        }

        public override EventGroup EventGroup => (EventGroup)Type;
    }
}