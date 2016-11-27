// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System;
using System.IO;
using System.Linq;
using FX2.Game.Beatmap;
using FX2.Game.Graphics;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics;
using osu.Framework.Input;
using OpenTK;
using OpenTK.Input;

namespace FX2.Game.Tests
{
    public class TestBeatmap : TestCase
    {
        private BaseGame game;
        private AudioManager audio;
        private string beatmapRootPath;
        private Beatmap.Beatmap beatmap;
        private BeatmapPlayback playback = new BeatmapPlayback();
        private AudioTrack audioTrack;
        private GameRenderView gameView;

        private ControllerWindow controllerWindow;
        private FileStream audioTrackStream;

        public override string Name { get; } = "Beatmap (2D)";

        public override string Description { get; } = "Tests various aspects of beatmap playback, including audio/effects";

        public TestBeatmap()
        {
            
        }

        public Beatmap.Beatmap LoadTestBeatmap(string name, out string mapRootPath, string preferedFileName = null)
        {
            mapRootPath = Path.Combine(Environment.CurrentDirectory, "TestMaps", name);
            var mapFiles = Directory.EnumerateFiles(mapRootPath, "*.ksh");

            string mapFile = mapFiles.FirstOrDefault();
            if(preferedFileName != null)
            {
                string prefferedMapFile = mapFiles.FirstOrDefault(x=>x.Contains(preferedFileName));
                if(prefferedMapFile != null)
                    mapFile = prefferedMapFile;
            }

            Stream stream = File.Open(mapFile, FileMode.Open);
            return new Beatmap.Beatmap(new BeatmapKSH(stream));
        }

        protected override bool UpdateSubTree()
        {
            return base.UpdateSubTree();
        }

        public override void Reset()
        {
            base.Reset();

            beatmap = LoadTestBeatmap("C18H27NO3", out beatmapRootPath);
            //beatmap = LoadTestBeatmap("soflan", out beatmapRootPath, "two");
            //beatmap = LoadTestBeatmap("bb", out beatmapRootPath, "grv");
            playback.Beatmap = beatmap;
            playback.ViewDuration = 0.8f;

            // Load beatmap audio
            string audioPath = Path.Combine(beatmapRootPath, beatmap.Metadata.AudioPath);
            audioTrackStream = File.Open(audioPath, FileMode.Open);
            audioTrack = new AudioTrackBass(audioTrackStream, false);
            
            //audioTrack.Start();
            
            Add(gameView = new GameRenderView
            {
                RelativeSizeAxes = Axes.Both,
                Size = Vector2.One
            });

            playback.ViewDurationChanged += (sender, args) =>
            {
                gameView.renderer.ViewDuration = playback.ViewDuration;
            };
            gameView.renderer.ViewDuration = playback.ViewDuration;

            Add(controllerWindow = new ControllerWindow(audioTrack, playback));
            controllerWindow.Show();
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if(args.Key == Key.F7)
            {
                controllerWindow.ToggleVisibility();
            }
            return base.OnKeyDown(state, args);
        }

        protected override void Update()
        {
            base.Update();
            
            if(beatmap == null) return;

            playback.Position = audioTrack.CurrentTime / 1000.0; // ms -> s
            gameView.renderer.Position = playback.Position;

            foreach(var measure in playback.MeasuresInView)
            {
                for(int i = 0; i < measure.TimingPoint.Numerator; i++)
                {
                    var div = new TimeDivision(i, measure.TimingPoint.Numerator);
                    gameView.renderer.UpdateSplit(new TimeDivisionReference(div, measure));
                }
            }
            foreach(var obj in playback.ObjectsInView)
            {
                gameView.renderer.UpdateObject(obj);
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            audioTrack?.Dispose();
            audioTrackStream?.Dispose();
        }

        [BackgroundDependencyLoader]
        private void load(BaseGame game, AudioManager audio)
        {
            this.game = game;
            this.audio = audio;
        }
    }
}