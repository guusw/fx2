// Copyright(c) 2016 Guus Waals (guus_waals@live.nl)
// Licensed under the MIT License(MIT)
// See "LICENSE.txt" for more information

using System.Collections.Generic;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.GameModes.Testing;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Graphics3D;
using osu.Framework.Input;
using OpenTK;
using OpenTK.Graphics;

namespace FX2.Game.Tests
{
    public class Test3D : TestCase
    {
        private Render3DContainer render3DContainer;
        public override string Name { get; } = "3D Rendering";
        public override string Description { get; } = "Tests the basic 3D rendering framework";

        private Texture testTexture;

        private Drawable3D cameraBoom;
        private Sprite3D testSprite;
        private Sprite3D testSprite1;

        private Vector3 cameraRotation = Vector3.Zero;

        public override void Reset()
        {
            base.Reset();

            Add(render3DContainer = new Render3DContainer
            {
                RelativeSizeAxes = Axes.Both,
                BackgroundColour = Color4.White.Scale(0.01f),
                Children = new Drawable3D[]
                {
                    testSprite = new Sprite3D
                    {
                        Colour = Color4.Blue, // Z
                        Texture = testTexture,
                        BlendingMode = BlendingMode.Additive,
                        Position = new Vector3(0.0f, 0.0f, 2.0f),
                        Rotation = Quaternion.FromAxisAngle(Vector3.Up, MathHelper.Pi)
                    },
                    testSprite1 = new Sprite3D
                    {
                        Colour = Color4.Green, // Y
                        Texture = testTexture,
                        BlendingMode = BlendingMode.Additive,
                        Position = new Vector3(0.0f, 2.0f, 0.0f),
                        Rotation = Quaternion.FromAxisAngle(Vector3.Right, MathHelper.PiOver2)
                    },
                    new Sprite3D
                    {
                        Colour = Color4.Red, // X
                        Texture = testTexture,
                        BlendingMode = BlendingMode.Additive,
                        Position = new Vector3(2.0f, 0.0f, 0.0f),
                        Rotation = Quaternion.FromAxisAngle(Vector3.Up, -MathHelper.PiOver2)
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

            testSprite1.FadeColour(Color4.LimeGreen, 1000, EasingTypes.InCubic);
            testSprite1.Loop();

            Scheduler.AddDelayed(() =>
            {
                testSprite.Rotation *= Quaternion.FromAxisAngle(Vector3.Forward, MathHelper.Pi * 0.001f);
            }, 10.0, true);
        }

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args)
        {
            return base.OnMouseDown(state, args);
        }

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args)
        {
            return base.OnMouseUp(state, args);
        }

        Quaternion CameraRotation(float pitch, float yaw, float roll)
        {
            return Quaternion.FromAxisAngle(new Vector3(0.0f, 1.0f, 0.0f), MathHelper.DegreesToRadians(yaw)) *
                   Quaternion.FromAxisAngle(new Vector3(1.0f, 0.0f, 0.0f), MathHelper.DegreesToRadians(pitch));
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