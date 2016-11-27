// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System;
using System.Drawing;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Graphics3D;
using osu.Framework.Graphics3D.Particles;
using osu.Framework.Input;
using osu.Framework.Threading;
using OpenTK;
using OpenTK.Graphics;

namespace FX2.Game.Tests
{
    public class TestParticles : TestCase
    {
        private Texture testTexture;
        private Drawable3D cameraBoom;
        private ParticleSystem particleSystem;
        private Render3DContainer render3DContainer;

        private ScheduledDelegate animation;

        private Vector3 cameraRotation = Vector3.Zero;

        public override string Name { get; } = "Particle System";
        public override string Description { get; } = "Tests the functionality of particle systems";

        public override void Reset()
        {
            base.Reset();
            
            Add(render3DContainer = new Render3DContainer
            {
                RelativeSizeAxes = Axes.Both,
                BackgroundColour = Color4.White.Scale(0.01f),
                Children = new Drawable3D[]
                {
                    new Sprite3D
                    {
                        Colour = Color4.HotPink,
                        Texture = testTexture,
                        BlendingMode = BlendingMode.Mixture,
                        Position = new Vector3(0.0f, 0.0f, 0.0f),
                        Rotation = Quaternion.FromAxisAngle(Vector3.Right, MathHelper.PiOver2)
                    },
                    particleSystem = new ParticleSystem()
                    {
                        EmissionRate = 300.0f,
                        MaximumParticles = 1000,
                        Texture = testTexture,
                        BlendingMode = BlendingMode.Mixture,
                        Position = new Vector3(0.0f, 0.0f, 0.0f),
                        Rotation = Quaternion.FromAxisAngle(Vector3.Right, -MathHelper.PiOver2), // Facing up
                        VelocityInitializer = new ConeVelocityInitializer {Angle=MathHelper.Pi*0.4f, MinimumVelocity = 1.0f, MaximumVelocity = 3.0f},
                        RotationInitializer = new RotationInitializer {Minimum = 0.0f, Maximum = MathHelper.Pi},
                        SizeInitializer = new SizeInitializer {MaximumUniform = 0.6f, MinimumUniform = 0.4f},
                        ColourInitializer = new ColourInitializer(Color4.AliceBlue.WithAlpha(0.6f)),
                        ParticleUpdaters = new Updater[]
                        {
                            new ConstantRotationSpeed(),
                            new VelocityDecay(),
                        },
                        ComputedProperties = new ComputedProperty[]
                        {
                            new AlphaFade(),
                            new SizeFade {Easing = EasingTypes.None, End = 1.0f, Start = 0.2f }
                        },
                        TransparencyGroup = 3
                    },
                    cameraBoom = new Node
                    {
                        Children = new Drawable3D[]
                        {
                            new Camera()
                            {
                                Position = new Vector3(0.0f, 0.0f, 5.0f)
                            }
                        }
                    }
                }
            });

            cameraRotation = new Vector3(-20.0f, -45.0f, 0.0f);
            UpdateCamera();

            animation = Scheduler.AddDelayed(() =>
            {
                double phase = Time.Current / 1000.0 * 2.0f;
                particleSystem.Position = new Vector3((float)Math.Cos(phase), 0.0f, (float)Math.Sin(phase)) * 2.0f;
            }, 10.0, true);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            animation?.Cancel();
            animation = null;
        }

        Quaternion CameraRotation(float pitch, float yaw, float roll)
        {
            return Quaternion.FromAxisAngle(new Vector3(0.0f, 1.0f, 0.0f), MathHelper.DegreesToRadians(yaw)) *
                   Quaternion.FromAxisAngle(new Vector3(1.0f, 0.0f, 0.0f), MathHelper.DegreesToRadians(pitch));
        }

        protected void UpdateCamera()
        {
            cameraBoom.Rotation = CameraRotation(cameraRotation.X, cameraRotation.Y, cameraRotation.Z);
        }

        protected override bool OnMouseMove(InputState state)
        {
            if(state.Mouse.HasMainButtonPressed)
            {
                var delta = state.Mouse.Delta;
                cameraRotation.X = MathHelper.Clamp(cameraRotation.X - delta.Y, -80.0f, 80.0f);
                cameraRotation.Y -= delta.X;
                cameraBoom.Rotation = CameraRotation(cameraRotation.X, cameraRotation.Y, cameraRotation.Z);
            }
            return base.OnMouseMove(state);
        }

        [BackgroundDependencyLoader]
        void Load(BaseGame game)
        {
            testTexture = game.Textures.Get("laser.png");
        }
    }
}