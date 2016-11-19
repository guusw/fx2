// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

namespace FX2.Game.Beatmap
{
    public class HoldEnd : Object
    {
        /// <summary>
        /// Points to the start of the hold note
        /// </summary>
        public ObjectReference StartingPoint;
    }

    /// <summary>
    /// A hold note
    /// </summary>
    public class Hold : Object
    {
        /// <summary>
        /// An object that indicates the ending point of the hold note
        /// </summary>
        public ObjectReference EndingPoint;

        /// <summary>
        /// Type of effect on the hold note
        /// </summary>
        public EffectType EffectType = 0;

        /// <summary>
        /// The first effect parameter
        /// </summary>
        public short EffectParameter0;

        /// <summary>
        /// Second effect parameter, only used on echo
        /// </summary>
        public short EffectParameter1;

        private byte index;
        
        /// <summary>
        /// Index of the button
        /// </summary>
        public int Index
        {
            get { return index; }
            set { index = (byte)value; }
        }

        /// <summary>
        /// Returns the absolute duration of this hold note
        /// </summary>
        /// <param name="measure">The measure the object is in</param>
        /// <returns></returns>
        public double GetAbsoluteDuration(Measure measure)
        {
            return EndingPoint.AbsolutePosition - GetAbsolutePosition(measure);
        }

        public override string ToString()
        {
            return $"Hold {Index}, {nameof(EffectType)}: {EffectType}";
        }
    }
}