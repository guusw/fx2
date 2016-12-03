// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

namespace FX2.Game.Beatmap.Effects
{
    /// <summary>
    /// A parameter that provides a time duration in various ways
    /// </summary>
    public abstract class EffectTimeParameter
    {
        public abstract double GetActualDuration(TimingPoint timingPoint);
    }
    
    /// <summary>
    /// Uses an absolute timing value
    /// </summary>
    public class AbsoluteTimeParameter : EffectTimeParameter
    {
        public double Duration;

        public AbsoluteTimeParameter(double duration = 0)
        {
            Duration = duration;
        }

        public override double GetActualDuration(TimingPoint timingPoint)
        {
            return Duration;
        }
    }

    /// <summary>
    /// Uses a relative value multiplied by the length of a measure
    /// </summary>
    public class RelativeTimeParameter : EffectTimeParameter
    {
        public double Percentage;

        public RelativeTimeParameter(double percentage = 0)
        {
            Percentage = percentage;
        }

        public override double GetActualDuration(TimingPoint timingPoint)
        {
            return timingPoint.MeasureDuration * Percentage;
        }
    }

    /// <summary>
    /// Using a division as a time parameter
    /// </summary>
    public class DivisionTimeParameter : EffectTimeParameter
    {
        public TimeDivision Division;

        public DivisionTimeParameter()
        {
            Division = TimeDivision.Start;
        }

        public DivisionTimeParameter(int numerator, int denominator)
        {
            Division = new TimeDivision(numerator, denominator);
        }

        public DivisionTimeParameter(TimeDivision division)
        {
            Division = division;
        }

        public override double GetActualDuration(TimingPoint timingPoint)
        {
            return timingPoint.GetDivisionDuration(Division);
        }
    }
}