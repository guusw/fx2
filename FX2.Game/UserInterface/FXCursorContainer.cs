// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using FX2.Shared;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using OpenTK;
using OpenTK.Graphics;

namespace FX2.Game.UserInterface
{
    public class FXCursorContainer : CursorContainer
    {
        protected override Drawable CreateCursor()
        {
            return new CircularContainer
            {
                AutoSizeAxes = Axes.Both,
                EdgeEffect = new EdgeEffect
                {
                    Type = EdgeEffectType.Glow,
                    Colour = Color4.White.WithAlpha(0.2f),
                    Radius = 10
                },

                Children = new[]
                {
                    new Box
                    {
                        Size = new Vector2(8, 8),
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        Colour = Color4.HotPink,
                    }
                }
            };
        }
    }
}