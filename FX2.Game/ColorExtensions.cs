// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using OpenTK.Graphics;

namespace FX2.Shared
{
    public static class ColorExtensions
    {
        public static Color4 WithAlpha(this Color4 color, float alpha)
        {
            return new Color4(color.R, color.G, color.B, alpha);
        }
        public static Color4 Scale(this Color4 color, float mult)
        {
            return new Color4(color.R * mult, color.G * mult, color.B * mult, color.A);
        }
    }
}