// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System.Diagnostics;
using System.IO;
using FX2.Game.UserInterface;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Visualisation;
using osu.Framework.Input;
using OpenTK.Input;

namespace FX2.Game
{
    public class FXGame : BaseGame
    {
        private DrawVisualiser drawVisualiser;
        protected override string MainResourceFile => @"FX2.Resources.dll";

        public FXGame(string[] args)
        {
        }

        [BackgroundDependencyLoader]
        private void Load(BaseGame game)
        {
            Add(new TestBrowser());
            // Create custom cursor
            Add(new FXCursorContainer());

            drawVisualiser = new DrawVisualiser();
            Add(drawVisualiser);
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if(args.Key == Key.F8)
            {
                if(drawVisualiser.IsVisible)
                {
                    drawVisualiser.Hide();
                }
                else
                {
                    drawVisualiser.Show();
                }
            }
            return base.OnKeyDown(state, args);
        }

        protected override void Update()
        {
            base.Update();
        }
    }
}