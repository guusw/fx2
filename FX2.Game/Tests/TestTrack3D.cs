// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics3D;

namespace FX2.Game.Tests
{
    public class TestTrack3D : TestCase
    {
        private Render3DContainer render3DContainer;

        public override string Name { get; } = "Beatmap (3D)";
        public override string Description { get; } = "Tests rendering 3D beatmaps";

        public override void Reset()
        {
            base.Reset();
        }
    }
}