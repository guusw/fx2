// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System;

namespace FX2.Game.Beatmap
{
    /// <summary>
    /// Identifies a gameplay object or a combination of those
    /// </summary>
    [Flags]
    public enum ObjectFilter
    {
        Button0 = 1,
        Button1 = 1 << 1,
        Button2 = 1 << 2,
        Button3 = 1 << 3,
        ButtonMask = 0xF,
        Fx0 = 1 << 4,
        Fx1 = 1 << 5,
        FxMask = 0x3 << 4,
        Laser0 = 1 << 6,
        Laser1 = 1 << 7,
        LaserMask = 0x3 << 6,
    }
}