// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System;
using FX2.Game.Beatmap;
using FX2.Game.UserInterface;
using osu.Framework.Audio.Track;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Framework.Threading;
using OpenTK;
using OpenTK.Graphics;
using Button = osu.Framework.Graphics.UserInterface.Button;

namespace FX2.Game.Tests
{
    /// <summary>
    /// Overlay used to control the playback of a beatmap and various other settings using sliders
    /// </summary>
    public class ControllerWindow : OverlayContainer
    {
        private FlowContainer container;
        private Box titleBar;
        private AudioTrack audioTrack;
        private BeatmapPlayback beatmapPlayback;
        
        private ScheduledDelegate task;

        public ControllerWindow(AudioTrack track, BeatmapPlayback playback)
        {
            beatmapPlayback = playback;
            audioTrack = track;

            Size = new Vector2(300,400);
            Children = new Drawable[]
            {
                new Box
                {
                    Colour = new Color4(30, 30, 30, 240),
                    RelativeSizeAxes = Axes.Both,
                    Depth = 0
                },
                new FlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FlowDirection.VerticalOnly,
                    Children = new Drawable[]
                    {
                        titleBar = new Box //title decoration
                        {
                            Colour = Color4.DarkBlue,
                            RelativeSizeAxes = Axes.X,
                            Size = new Vector2(1, 20),
                        },
                        new Container //toolbar
                        {
                            RelativeSizeAxes = Axes.X,
                            Size = new Vector2(1, 40),
                            Children = new Drawable[]
                            {
                                new Box {
                                    Colour = new Color4(20, 20, 20, 255),
                                    RelativeSizeAxes = Axes.Both,
                                },
                                new FlowContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Spacing = new Vector2(1),
                                    Children = new Drawable[]
                                    {
                                        new Button
                                        {
                                            Colour = Color4.DarkGray,
                                            Size = new Vector2(100, 1),
                                            RelativeSizeAxes = Axes.Y,
                                            Text = @"Pause",
                                            Action = TogglePause
                                        },
                                        new Button
                                        {
                                            Colour = Color4.DarkGray,
                                            Size = new Vector2(100, 1),
                                            RelativeSizeAxes = Axes.Y,
                                            Text = @"Restart",
                                            Action = Restart
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = 65 },
                    Children = new[]
                    {
                        container = new FlowContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Direction = FlowDirection.VerticalOnly,
                        }
                    }
                },
            };

            // Add slider options
            container.Add(CreateSliderBinding("Position", 0, track.Length, 
                x => audioTrack.Seek(x),
                () => audioTrack.CurrentTime));
            container.Add(CreateSliderBinding("View Duration", 0.1, 4.0, 
                x => beatmapPlayback.ViewDuration = x, 
                () => beatmapPlayback.ViewDuration));
        }

        protected override bool OnDragStart(InputState state) => titleBar.Contains(state.Mouse.NativeState.Position);

        protected override bool OnDrag(InputState state)
        {
            Position += state.Mouse.Delta;
            return base.OnDrag(state);
        }

        protected override void PopIn()
        {
            task = Scheduler.AddDelayed(UpdateTask, 10.0, true);
            FadeIn(100);
        }

        protected override void PopOut()
        {
            task?.Cancel();
            FadeOut(100);
        }

        private void UpdateTask()
        {
        }

        private void Restart()
        {
            audioTrack.Seek(0); // Reset
            audioTrack.Start();
        }

        private void TogglePause()
        {
            if(audioTrack.IsRunning)
            {
                audioTrack.Stop();
            }
            else
            {
                if(audioTrack.CurrentTime >= audioTrack.Length)
                    audioTrack.Seek(0); // Reset
                audioTrack.Start();
            }
        }

        private Drawable CreateSliderBinding(string name, double min, double max, Action<double> set, Func<double> get)
        {
            var text = new SpriteText();
            var slider = new Slider
            {
                Size = new Vector2(200.0f, 20.0f),
                Margin = new MarginPadding { Left = 10.0f },
                Min = min,
                Max = max,
                Get = get,
                Set = set,
            };
            slider.ValueChanged += (a, b) =>
            {
                text.Text = $"{name} ({slider.State:#0.00}):";
            };
            return new FlowContainer
            {
                Direction = FlowDirection.VerticalOnly,
                AutoSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    text,
                    slider
                }
            };
        }

    }
}