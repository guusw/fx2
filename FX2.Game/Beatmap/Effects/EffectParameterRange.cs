// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information
namespace FX2.Game.Beatmap.Effects
{
    public class EffectParameterRange<TType>
    {
        /// <summary>
        /// Minimum value
        /// </summary>
        public TType Minimum;

        /// <summary>
        /// Maxmimum value
        /// </summary>
        public TType Maximum;

        /// <summary>
        /// Single value
        /// </summary>
        public TType Value
        {
            get { return Minimum; }
            set
            {
                Minimum = value;
                Maximum = value;
            }
        }

        public EffectParameterRange()
        {
        }

        public EffectParameterRange(TType initialValue)
        {
            Value = initialValue;
        }

        public EffectParameterRange(TType min, TType max)
        {
            Minimum = min;
            Maximum = max;
        }

        /// <summary>
        /// Implicitly cast range to a single value
        /// </summary>
        public static implicit operator TType(EffectParameterRange<TType> range)
        {
            return range.Value;
        }
    }
}