// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information
namespace FX2.Game.Beatmap
{
    public enum EffectType
    {
        None = 0,
        Retrigger,
        Flanger,
        Phaser,
        Gate,
        TapeStop,
        Bitcrush,
        Wobble,
        SideChain,
        Echo,
        Panning,
        PitchShift,
        LowPassFilter,
        HighPassFilter,
        PeakingFilter,
        UserDefined = 0x40, // This ID or higher is user for user defined effects inside map objects
        UserDefined1,       // Keep this ID at least a few ID's away from the normal effect so more native effects can be added later
        UserDefined2,
        UserDefined3,
        UserDefined4,
        UserDefined5,
        UserDefined6,
        UserDefined7,
        UserDefined8,
        UserDefined9 // etc...
    }
}